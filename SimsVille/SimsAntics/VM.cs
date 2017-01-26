using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Files.formats.iff.chunks;
using TSO.SimsAntics.Engine;
/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using Microsoft.Xna.Framework;
using TSO.Content;
using TSO.SimsAntics.Model;
using System.Collections.Concurrent;
using tso.world.Components;
using TSO.SimsAntics.Entities;

namespace TSO.SimsAntics
{
    /// <summary>
    /// Simantics Virtual Machine.
    /// </summary>
    public class VM
    {
        private static bool _UseWorld = true;
        public static bool UseWorld
        {
            get { return _UseWorld; }
            set
            {
                _UseWorld = value;
                VMContext.UseWorld = value;
                VMEntity.UseWorld = value;
            }
        }
        public long TickInterval = 33 * TimeSpan.TicksPerMillisecond;
        public int Speed = 3;

        public VMContext Context { get; internal set; }

        public List<VMEntity> Entities = new List<VMEntity>();
        public VMEntity ActiveEntity = null;
        public short[] GlobalState;
        public string LotName;

        private object ThreadLock;
        private HashSet<VMThread> ActiveThreads = new HashSet<VMThread>();
        private HashSet<VMThread> IdleThreads = new HashSet<VMThread>();
        private List<VMStateChangeEvent> ThreadEvents = new List<VMStateChangeEvent>();

        private Dictionary<short, VMEntity> ObjectsById = new Dictionary<short, VMEntity>();
        private short ObjectId = 1;

        public VMHeadlineRendererProvider Headline;

        public bool Ready;
        public bool BHAVDirty;

        public event VMDialogHandler OnDialog;
        public event VMRefreshHandler OnFullRefresh;
        public event VMBreakpointHandler OnBreakpoint;

        public delegate void VMDialogHandler(VMDialogInfo info);
        public delegate void VMRefreshHandler();
        public delegate void VMBreakpointHandler(VMEntity entity);

        /// <summary>
        /// Constructs a new Virtual Machine instance.
        /// </summary>
        /// <param name="context">The VMContext instance to use.</param>
        public VM(VMContext context)
        {
            context.VM = this;
            ThreadLock = this;
            this.Context = context;
            OnBHAVChange += VM_OnBHAVChange;

            //Set VM Ready
            Ready = true;
           
        }

        private void VM_OnBHAVChange()
        {
            BHAVDirty = true;
        }

        /// <summary>
        /// Gets an entity from this VM.
        /// </summary>
        /// <param name="id">The entity's ID.</param>
        /// <returns>A VMEntity instance associated with the ID.</returns>
        public VMEntity GetObjectById(short id)
        {
            if (ObjectsById.ContainsKey(id))
            {
                return ObjectsById[id];
            }
            return null;
        }

        /// <summary>
        /// Initializes this Virtual Machine.
        /// </summary>
        public void Init()
        {
            Context.Globals = TSO.Content.Content.Get().WorldObjectGlobals.Get("Global");
            GlobalState = new short[33];
            GlobalState[20] = 255; //Game Edition. Basically, what "expansion packs" are running. Let's just say all of them.
            GlobalState[25] = 4; //as seen in EA-Land edith's simulator globals, this needs to be set for people to do their idle interactions.
            GlobalState[17] = 4; //Runtime Code Version, is this in EA-Land.
        }

        private bool AlternateTick;

        private long LastTick = 0;
        public void Update(GameTime time)
        {

            
            TickInterval = Speed/3 * 33 * TimeSpan.TicksPerMillisecond;
            if (Ready)
            if (LastTick == 0 || (time.TotalGameTime.Ticks - LastTick) >= TickInterval)
            {
                Tick(time);
            }
            
            AlternateTick = !AlternateTick;
        }



        private void Tick(GameTime time)
        {
            Context.Clock.Tick();

            if (Context.Architecture != null)
            Context.Architecture.Tick();

            lock (ThreadLock)
            {
                foreach (var evt in ThreadEvents)
                {
                    switch (evt.NewState)
                    {
                        case VMThreadState.Idle:
                            evt.Thread.State = VMThreadState.Idle;
                            IdleThreads.Add(evt.Thread);
                            ActiveThreads.Remove(evt.Thread);
                            break;
                        case VMThreadState.Active:
                            if (evt.Thread.State != VMThreadState.Active) ActiveThreads.Add(evt.Thread);
                            evt.Thread.State = VMThreadState.Active;
                            IdleThreads.Remove(evt.Thread);
                            break;
                        case VMThreadState.Removed:
                            if (evt.Thread.State == VMThreadState.Active) ActiveThreads.Remove(evt.Thread);
                            else IdleThreads.Remove(evt.Thread);
                            evt.Thread.State = VMThreadState.Removed;
                            break;
                    }
                }

                ThreadEvents.Clear();

                LastTick = time.TotalGameTime.Ticks;
                foreach (var thread in ActiveThreads) thread.Tick();

                var entCpy = new List<VMEntity>(Entities);
                foreach (var obj in entCpy)
                {
                    Context.NextRandom(1);
                    obj.Tick(); //run object specific tick behaviors, like lockout count decrement
                } //run object specific tick behaviors, like lockout count decrement

                

            }
        }

        public void InternalTick()
        {
            Context.Clock.Tick();
            GlobalState[6] = (short)Context.Clock.Seconds;
            GlobalState[5] = (short)Context.Clock.Minutes;
            GlobalState[0] = (short)Context.Clock.Hours;
            GlobalState[4] = (short)Context.Clock.TimeOfDay;

            Context.Architecture.Tick();

            var entCpy = new List<VMEntity>(Entities);
            foreach (var obj in entCpy)
            {
                Context.NextRandom(1);
                obj.Tick(); //run object specific tick behaviors, like lockout count decrement
            }
        }

        /// <summary>
        /// Adds an entity to this Virtual Machine.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void AddEntity(VMEntity entity)
        {
            entity.ObjectID = ObjectId;
            ObjectsById.Add(entity.ObjectID, entity);
            AddToObjList(this.Entities, entity);
            ObjectId = NextObjID();
        }

        public static void AddToObjList(List<VMEntity> list, VMEntity entity)
        {
            if (list.Count == 0) { list.Add(entity); return; }
            int id = entity.ObjectID-1;
            int max = list.Count-1;
            int min = 0;
            while (max-1>min)
            {
                int mid = (max+min) / 2;
                int nid = list[mid].ObjectID;
                if (id < nid) max = mid;
                else min = mid;
            }
            list.Insert((list[min].ObjectID>id)?min:((list[max].ObjectID > id)?max:max+1), entity);
        }

        /// <summary>
        /// Removes an entity from this Virtual Machine.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void RemoveEntity(VMEntity entity)
        {
            if (Entities.Contains(entity))
            {
                this.Entities.Remove(entity);
                ObjectsById.Remove(entity.ObjectID);
                if (entity.ObjectID < ObjectId) ObjectId = entity.ObjectID; //this id is now the smallest free object id.
            }
            entity.Dead = true;
        }

        /// <summary>
        /// Finds the next free object ID and remembers it for use when making another object.
        /// </summary>
        private short NextObjID()
        {
            for (short i = ObjectId; i > 0; i++)
                if (!ObjectsById.ContainsKey(i)) return i;
            return 0;
        }

        /// <summary>
        /// Gets a global value set for this Virtual Machine.
        /// </summary>
        /// <param name="var">The index of the global value to get. WARNING: Throws exception if index is OOB.
        /// Must be in range of 0 - 31.</param>
        /// <returns>A global value if found.</returns>
        public short GetGlobalValue(ushort var)
        {
            // should this be in VMContext?
            if (var > 32) throw new Exception("Global Access out of bounds!");
            return GlobalState[var];
        }

        /// <summary>
        /// Sets a global value for this Virtual Machine.
        /// </summary>
        /// <param name="var">Index for value, must be in range 0 - 31.</param>
        /// <param name="value">Global value.</param>
        /// <returns>True if successful. WARNING: If index was OOB, exception is thrown.</returns>
        public bool SetGlobalValue(ushort var, short value)
        {
            if (var > 32) throw new Exception("Global Access out of bounds!");
            GlobalState[var] = value;
            return true;
        }

        private static Dictionary<BHAV, VMRoutine> _Assembled = new Dictionary<BHAV, VMRoutine>();
        private static event VMBHAVChangeDelegate OnBHAVChange;

        /// <summary>
        /// Assembles a set of instructions.
        /// </summary>
        /// <param name="bhav">The instruction set to assemble.</param>
        /// <returns>A VMRoutine instance.</returns>
        public VMRoutine Assemble(BHAV bhav)
        {
            if (_Assembled.ContainsKey(bhav)) return _Assembled[bhav];
            lock (_Assembled)
            {
                if (_Assembled.ContainsKey(bhav))
                {
                    return _Assembled[bhav];
                }
                var routine = VMTranslator.Assemble(this, bhav);
                _Assembled.Add(bhav, routine);
                return routine;
            }
        }

        public static void BHAVChanged(BHAV bhav)
        {
            lock (_Assembled)
            {
                bhav.RuntimeVer++;
                if (_Assembled.ContainsKey(bhav)) _Assembled.Remove(bhav);
            }
            if (OnBHAVChange != null) OnBHAVChange();
        }

        /// <summary>
        /// Signals a Dialog to all listeners. (usually a UI)
        /// </summary>
        /// <param name="info">The dialog info to pass along.</param>
        public void SignalDialog(VMDialogInfo info)
        {
            if (OnDialog != null) OnDialog(info);
        }


        public VMSandboxRestoreState Sandbox()
        {
            var state = new VMSandboxRestoreState { Entities = Entities, ObjectId = ObjectId, ObjectsById = ObjectsById };

            Entities = new List<VMEntity>();
            ObjectsById = new Dictionary<short, VMEntity>();
            ObjectId = 1;

            return state;
        }

        public void SandboxRestore(VMSandboxRestoreState state)
        {
            Entities = state.Entities;
            ObjectsById = state.ObjectsById;
            ObjectId = state.ObjectId;
        }

        internal void BreakpointHit(VMEntity entity)
        {
            if (OnBreakpoint == null) entity.Thread.ThreadBreak = VMThreadBreakMode.Active; //no handler..
            else OnBreakpoint(entity);
        }

    }

    public delegate void VMBHAVChangeDelegate();

    public class VMStateChangeEvent
    {
        public VMThread Thread;
        public VMThreadState NewState;
    }

    public class VMSandboxRestoreState
    {
        public List<VMEntity> Entities;
        public Dictionary<short, VMEntity> ObjectsById;
        public short ObjectId = 1;
    }
}
