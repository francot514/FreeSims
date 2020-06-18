/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the SimsLib.

The Initial Developer of the Original Code is
Rhys Simpson. All Rights Reserved.

Contributor(s): Mats 'Afr0' Vederhus
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using FSO.Files.HIT;

namespace TSO.HIT
{
    public class HITThread : HITSound
    {
        public uint PC; //program counter
        public HITFile Src;
        public HITVM VM;
        private Hitlist Hitlist;
        private int[] Registers; //includes args, vars, whatever "h" is up to 0xf
        private int[] LocalVar; //the sims online set, 0x10 "argstyle" up to 0x45 orientz. are half of these even used? no. but even in the test files? no
        public int[] ObjectVar; //IsInsideViewFrustrum 0x271a to Neatness 0x2736. Set by object on thread invocation.
       
        public int LoopPointer = -1;
        public int WaitRemain = -1;
        public bool Paused;
        public int TickN;

        private bool SimpleMode; //certain sounds play with no HIT.
        private bool PlaySimple;


        public bool HasSetLoop;
        public bool LoopDefined;
        public bool ThreadDead;

        public bool Interruptable
        {
            get { return LocalVar != null && (LocalVar[0x21] > 0 || LocalVar[0x27] > 0); }
        }
        public bool Interrupted;
        public HITThread InterruptWaiter;
        public HITThread InterruptBlocker;
        private uint Patch; //sound id

        private List<HITNoteEntry> Notes;
        private Dictionary<SoundEffectInstance, HITNoteEntry> NotesByChannel;
        public int LastNote
        {
            get { return Notes.Count - 1; }
        }



        public bool ZeroFlag; //flags set by instructions
        public bool SignFlag;

        public Stack<int> Stack;

        private FSO.Content.Audio audContent;

        public void Interrupt(HITThread waiter)
        {
            Interrupted = true;
            InterruptWaiter = waiter;
            waiter.InterruptBlocker = this;
            for (int i = 0; i < Notes.Count; i++)
            {
                var note = Notes[i];
                if (note.EndTick == -1)
                {
                    var tickDuration = (note.Duration);
                    note.EndTick = note.StartTick + (int)(Math.Ceiling((TickN - note.StartTick) / tickDuration) * tickDuration);
                    Notes[i] = note;
                }
            }
        }

        public void Unblock()
        {
            InterruptBlocker = null;
        }


        public override  bool Tick() //true if continue, false if kill
        {

            if (Paused) return true;
            TickN++;

            if (EverHadOwners && Owners.Count == 0)
            {
                KillVocals();
                Dead = true;
                return false;
            }

            if (VolumeSet)
            {
                for (int i = 0; i < Notes.Count; i++)
                {
                    var note = Notes[i];
                    var inst = Notes[i].instance;
                    if (note.EndTick != -1 && TickN > note.EndTick) inst.Stop();
                    if (!note.started && inst.State == SoundState.Stopped)
                    {
                        if (!inst.IsDisposed) inst.Dispose();
                        continue;
                    }
                    inst.Pan = Pan;
                    inst.Volume = Volume;
                }
            }

            VolumeSet = false;

            if (SimpleMode)
            {
                if (PlaySimple)
                {
                    NoteOn();
                    PlaySimple = false;
                }
                if (NoteActive(LastNote)) return true;
                else
                {
                    Dead = true;
                    return false;
                }
            }
            else
            {
                try
                {
                    while (true)
                    {
                        var opcode = Src.Data[PC++];
                        if (opcode > HITInterpreter.Instructions.Length) opcode = 0;
                        var result = HITInterpreter.Instructions[opcode](this);
                        if (result == HITResult.HALT) return true;
                        else if (result == HITResult.KILL)
                        {
                            Dead = true;
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Kills all playing sounds.
        /// </summary>
        public void KillVocals()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                if (NoteActive(i))
                {
                    Notes[i].instance.Stop();
                    Notes[i].instance.Dispose();
                }
            }
        }

        public HITThread()
        {


        }

        public HITThread(HITFile Src, HITVM VM)
        {
            this.Src = Src;
            this.VM = VM;
            Registers = new int[16];
            Registers[1] = 12;
            LocalVar = new int[54];
            ObjectVar = new int[29];

            Notes = new List<HITNoteEntry>();
            NotesByChannel = new Dictionary<SoundEffectInstance, HITNoteEntry>();
            Owners = new List<int>();

            Stack = new Stack<int>();
            audContent = FSO.Content.Content.Get().Audio;
        }

        

        public HITThread(uint TrackID)
        {
            Owners = new List<int>();
            Notes = new List<HITNoteEntry>();
            NotesByChannel = new Dictionary<SoundEffectInstance, HITNoteEntry>();

            audContent = FSO.Content.Content.Get().Audio;
            SetTrack(TrackID);

            Patch = ActiveTrack.SoundID;
            SimpleMode = true;
            PlaySimple = true; //play next frame, so we have time to set volumes.
        }

        public void SetVolume(float volume, float pan)
        {
            if (VolumeSet)
            {
                if (volume > Volume)
                {
                    Volume = volume;
                    PreviousVolume = Volume;
                    Pan = pan;
                }
            }
            else
            {
                Volume = volume;
                PreviousVolume = Volume;
                Pan = pan;
            }

            VolumeSet = true;
        }


        public void LoadHitlist(uint id)
        {
            Hitlist = audContent.GetHitlist(id);
        }

        public uint HitlistChoose() //returns a random id from the hitlist
        {
            Random rand = new Random();
            if (Hitlist != null) return Hitlist.IDs[rand.Next(Hitlist.IDs.Count)];
            else return 0;
        }

        public byte ReadByte()
        {
            return Src.Data[PC++];
        }

        public uint ReadUInt32()
        {
            uint result = 0;
            result |= ReadByte();
            result |= ((uint)ReadByte() << 8);
            result |= ((uint)ReadByte() << 16);
            result |= ((uint)ReadByte() << 24);
            return result;
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public void SetTrack(uint value)
        {
            if (audContent.TracksById.ContainsKey(value))
            {
                ActiveTrack = audContent.TracksById[value];
                Patch = ActiveTrack.SoundID;
            }
            else
            {
                Debug.WriteLine("Couldn't find track: " + value);
            }
        }

        public override void Pause()
        {
            Paused = true;
            foreach (var note in Notes)
            {
                if (note.instance.State != SoundState.Stopped) note.instance.Pause();
            }
        }

        public override void Resume()
        {
            Paused = false;
            foreach (var note in Notes)
            {
                if (note.instance.State != SoundState.Stopped) note.instance.Resume();
            }
        }

        /// <summary>
        /// Loads a track from the current HitList.
        /// </summary>
        /// <param name="value">ID of track to load.</param>
        public uint LoadTrack(int value)
        {
            SetTrack(Hitlist.IDs[value]);
            return Hitlist.IDs[value];
        }

        public int NoteOn()
        {
            var sound = audContent.GetSFX(Patch);

            if (sound != null)
            {
                var instance = sound.CreateInstance();
                instance.Volume = Volume;
                instance.Pan = Pan;
                instance.Play();

                var entry = new HITNoteEntry(sound, instance, Patch, TickN);
                Notes.Add(entry);
                NotesByChannel.Add(instance, entry);
                return Notes.Count - 1;
            }
            else
            {
                Debug.WriteLine("HITThread: Couldn't find sound: " + Patch.ToString());
            }

            return -1;
        }

        /// <summary>
        /// Plays a note and loops it.
        /// </summary>
        /// <returns>-1 if unsuccessful, or the number of notes in this thread if successful.</returns>
        public int NoteLoop() //todo, make loop again.
        {
            var sound = audContent.GetSFX(Patch);

            if (sound != null)
            {
                var instance = sound.CreateInstance();
                instance.Volume = Volume;
                instance.Pan = Pan;
                instance.Play();

                var entry = new HITNoteEntry(sound, instance, Patch, TickN);
                Notes.Add(entry);
                NotesByChannel.Add(instance, entry);
                return Notes.Count - 1;
            }
            else
            {
                Debug.WriteLine("HITThread: Couldn't find sound: " + Patch.ToString());
            }
            return -1;
        }

        /// <summary>
        /// Is a note active?
        /// </summary>
        /// <param name="note">The note to check.</param>
        /// <returns>True if active, false if not.</returns>
        public bool NoteActive(int note)
        {
            if (note == -1 || note >= Notes.Count) return false;
            return (Notes[note].instance.State != SoundState.Stopped);
        }

        /// <summary>
        /// Signals the VM to duck all threads with a higher ducking priority than this one.
        /// </summary>
        public void Duck()
        {
            VM.Duck(this.DuckPriority);
        }

        /// <summary>
        /// Signals to the VM to unduck all threads that are currently ducked.
        /// </summary>
        public void Unduck()
        {
            VM.Unduck();
        }

        private void LocalVarSet(int location, int value)
        {
            switch (location)
            {
                case 0x12: //patch, switch active track
                    Patch = (uint)value;
                    break;
            }
        }

        public void SetFlags(int value)
        {
            ZeroFlag = (value == 0);
            SignFlag = (value < 0);
        }

        public void WriteVar(int location, int value)
        {
            if (location < 0x10)
            {
                Registers[location] = value;
            }
            else if (location < 0x46)
            {
                LocalVarSet(location, value); //invoke any special behaviours, like track switch for setting patch
                LocalVar[location - 0x10] = value;
            }
            else if (location < 0x64)
            {
                return; //not mapped
            }
            else if (location < 0x88)
            {
                VM.WriteGlobal(location - 0x64, value);
            }
            else if (location < 0x271a)
            {
                return; //not mapped
            }
            else if (location < 0x2737)
            {
                ObjectVar[location - 0x271a] = value; //this probably should not be valid... but if it is used it may require some reworking to get this to sync across object threads.
            }
        }

        public int ReadVar(int location)
        {
            if (location < 0x10)
            {
                return Registers[location];
            } 
            else if (location < 0x46) 
            {
                return LocalVar[location - 0x10];
            }
            else if (location < 0x64)
            {
                return 0; //not mapped
            }
            else if (location < 0x88)
            {
                return VM.ReadGlobal(location - 0x64);
            }
            else if (location < 0x271a)
            {
                return 0; //not mapped
            }
            else if (location < 0x2737)
            {
                return ObjectVar[location - 0x271a];
            }
            return 0;
        }

        public void JumpToEntryPoint(int TrackID)
        {
            PC = (uint)Src.EntryPointByTrackID[(uint)TrackID];
        }



        public override void Dispose()
        {
            if (InterruptWaiter != null)
                InterruptWaiter.Unblock();

            foreach (var note in Notes) note.instance.Dispose();
        }

    }

    public struct HITNoteEntry 
    {
        public SoundEffectInstance instance;
        public uint SoundID; //This is for killing specific sounds, see HITInterpreter.SeqGroupKill.
        public bool ended;
        public bool started;
        public int StartTick;
        public int EndTick;
        public float Duration;

        public HITNoteEntry(SoundEffect sfx, SoundEffectInstance instance, uint SoundID, int startTick)
        {
            this.instance = instance;
            this.SoundID = SoundID;
            this.ended = false;
            this.started = false;
            this.EndTick = -1;
            this.Duration = (float)sfx.Duration.TotalSeconds;
            this.StartTick = startTick;

        }
    }
}
