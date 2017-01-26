/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Content;
using TSO.Files.formats.iff.chunks;

namespace TSO.SimsAntics.Engine
{
    public class VMQueuedAction
    {
        public VMQueuedAction() { }

        public VMRoutine Routine;
        public VMEntity Callee;
        public VMEntity StackObject; //set to callee for interactions

        private VMEntity _IconOwner = null; //defaults to callee
        public VMEntity IconOwner {
            get {
                return (_IconOwner == null)?Callee:_IconOwner;
            }
            set {
                _IconOwner = value;
            }
        } 

        public GameObject CodeOwner; //used to access local resources from BHAVs like strings
        public string Name;
        public short[] Args; //WARNING - if you use this, the args array MUST have the same number of elements the routine is expecting!

        public int InteractionNumber = -1; //this interaction's number... This is needed for create object callbacks 
                                           //for This Interaction but entry point functions don't have this...
                                           //suggests init and main don't use action queue.
        public bool Cancelled;

        public short Priority = (short)VMQueuePriority.Idle;
        public VMQueueMode Mode = VMQueueMode.Normal;
        public TTABFlags Flags;
        public TSOFlags Flags2 = (TSOFlags)0x1f;

        public ushort UID; //a wraparound ID that is just here so that a specific interaction can be reliably "cancelled" by a client.

        public VMActionCallback Callback;

    }

    public enum VMQueuePriority : short
    {
        Maximum = 100,
        Autonomous = 2,
        UserDriven = 50,
        ParentIdle = 25,
        ParentExit = 24,
        Idle = 0
    }

    public enum VMQueueMode : byte
    {
        Normal,
        ParentIdle,
        ParentExit, //hidden until active. DO NOT CANCEL OR SKIP!
        Idle
    }
}
