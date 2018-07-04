using FSO.Files.HIT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSO.HIT
{
    public enum HITVolumeGroup
    {
        FX = 0,
        MUSIC = 1,
        VOX = 2,
        AMBIENCE = 3
    }

    public abstract class HITSound
    {
        

        public bool VolumeSet;
        public float Volume = 1;
        public float Pan;
        public float InstVolume = 1;
        public float PreviousVolume = 1; //This is accessed by HitVM.Unduck()
        public Track ActiveTrack;

        public HITVolumeGroup VolGroup;

        public HITDuckingPriorities DuckPriority
        {
            get
            {
                if (ActiveTrack != null)
                    return ActiveTrack.DuckingPriority;
                else
                    return HITDuckingPriorities.duckpri_normal;
            }
        }

        public HITVM VM;

        public bool EverHadOwners; //if we never had owners, don't kill the thread. (ui sounds)
        public List<int> Owners;

        public bool Dead;

        public HITSound()
        {
            Owners = new List<int>();
        }

        public abstract bool Tick();

        public void SetVolume(float volume, float pan)
        {
            if (VolumeSet)
            {
                if (volume > Volume)
                {
                    Volume = volume;
                    Pan = pan;
                }
            }
            else
            {
                Volume = volume;
                Pan = pan;
            }

            VolumeSet = true;
        }

        public void AddOwner(int id)
        {
            EverHadOwners = true;
            Owners.Add(id);
        }

        public float GetVolFactor()
        {
            return VM?.GetMasterVolume(VolGroup) ?? 1f;
        }

        public void RemoveOwner(int id)
        {
            Owners.Remove(id);
        }

        public bool AlreadyOwns(int id)
        {
            return Owners.Contains(id);
        }

        public abstract void Pause();

        public abstract void Resume();

        public abstract void Dispose();

    }
}
