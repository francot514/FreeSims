/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.SimsAntics.Model;
using Microsoft.Xna.Framework;
using tso.world.Model;
using TSO.Files.formats.iff.chunks;
using tso.world.Components;
using TSO.SimsAntics.Utils;
using TSO.Common.utils;
using TSO.SimsAntics.Engine.Routing;
using TSO.SimsAntics.Model.Routing;
using TSO.SimsAntics.Primitives;

namespace TSO.SimsAntics.Engine
{
    /// <summary>
    /// Determines the path to a set destination from an avatar and provides a system to walk them there. First finds the
    /// set of room portals to get to the target's room (calls their portal functions in order), then pathfinds to the relevant 
    /// position once in the same room.
    /// 
    /// If in the same room or a flag is set to "ignore rooms", the room step is ignored and the character is directly routed 
    /// to the specified position. Pathfinders are normally pushed to the stack, but behave completely differently from
    /// stack frames. You can imagine an example case, where the avatar is currently routing to a door, but the final destination
    /// is the piano.
    /// 
    /// STACK:
    /// Private: Interaction - Play (piano)
    /// Private: route to piano and sit (piano)
    /// VMPathFinder: Go on top of, opposing direction. (this is passed as a position and direction.)
    /// Semi-global: Portal function (door)
    /// Semi-global: Goto door and gace through (door)
    /// VMPathFinder: Go on top of, same direction. (passed as position direction, but this time destination is in same room, so we call no portal functions)
    /// 
    /// Once the first portal function returns true, we will pop back to the initial VMPathFinder to route to the second or the object.
    /// This can repeat many times. 
    /// 
    /// However, if a portal function returns false, we will need to re-evaluate the portal route to the
    /// destination, discounting the route from the previous portal to the portal we failed to route to. (nodes are no longer connected) 
    /// The Sims 1 does not do this, but routing problems annoy me as much as everyone else. :)
    /// </summary>
    public class VMRoutingFrame : VMStackFrame
    {

        private static ushort ROUTE_FAIL_TREE = 398;
        private static uint GOTO_GUID = 0x000007C4;
        private static short SHOO_INTERACTION = 3;
        private static ushort SHOO_TREE = 4107;

        //each within-room route gets these allowances separately.
        private static int WAIT_TIMEOUT = 10 * 30; //10 seconds
        private static int MAX_RETRIES = 10;

        private Stack<VMRoomPortal> Rooms = new Stack<VMRoomPortal>();
        private VMRoomPortal CurrentPortal;

        public LinkedList<Point> WalkTo;
        private double WalkDirection;
        private double TargetDirection;
        private bool IgnoreRooms;

        public VMRoutingFrameState State = VMRoutingFrameState.INITIAL;
        public int PortalTurns = 0;
        public int WaitTime = 0;
        private int Timeout = WAIT_TIMEOUT;
        private int Retries = MAX_RETRIES;

        private bool AttemptedChair = false;
        private float TurnTweak = 0;
        private int TurnFrames = 0;

        private int MoveTotalFrames = 0;
        private int MoveFrames = 0;
        private int Velocity = 0;
        private VMRoutingFrame ParentRoute;

        private short WalkStyle
        {
            get
            {
                return (InPool)?(short)0:Caller.GetValue(VMStackObjectVariable.WalkStyle);
            }
        }

        private bool InPool
        {
            get
            {
                return VM.Context.RoomInfo[VM.Context.GetRoomAt(Caller.Position)].Room.IsPool;
            }
        }

        public bool CallFailureTrees = false;

        private HashSet<VMRoomPortal> IgnoredRooms = new HashSet<VMRoomPortal>();

        private LotTilePos PreviousPosition;
        private LotTilePos CurrentWaypoint = LotTilePos.OUT_OF_WORLD;

        private bool RoomRouteInvalid;
        private SLOTItem Slot;
        private VMEntity Target;
        private List<VMFindLocationResult> Choices;
        private VMFindLocationResult CurRoute;

        public VMRoutingFrame() { }
        
        private void Init()
        {
            ParentRoute = GetParentFrame();
           
            
        }

        public bool InitRoutes(SLOTItem slot, VMEntity target)
        {
            Init();

            Slot = slot;
            Target = target;
            var found = AttemptRoute(null);

            if (found == VMRouteFailCode.Success) return true;
            else HardFail(found, null);
            return false;
        }

        public bool InitRoutes(List<VMFindLocationResult> choices) //returns false if we can't find a single route
        {
            Init();

            Choices = choices; //should be ordered by most preferred first, with a little bit of random shuffling to keep things interesting for "wander"
            //style movements. Also includes flags dictating if this route goes through walls etc.
            var found = VMRouteFailCode.NoValidGoals;
            while (found != VMRouteFailCode.Success && Choices.Count > 0) {
                found = AttemptRoute(Choices[0]);
                Choices.RemoveAt(0);
            }

            if (found == VMRouteFailCode.Success) return true;
            else HardFail(found, null);
            return false;
        }

        public void InvalidateRoomRoute()
        {
            RoomRouteInvalid = true;
        }

        public void SoftFail(VMRouteFailCode code, VMEntity blocker)
        {
            var found = VMRouteFailCode.NoValidGoals;
            while (found != VMRouteFailCode.Success && Choices != null && Choices.Count > 0)
            {
                found = AttemptRoute(Choices[0]);
                Choices.RemoveAt(0);
            }

            if (found != VMRouteFailCode.Success) HardFail(code, blocker);
        }

        private void HardFail(VMRouteFailCode code, VMEntity blocker)
        {
            State = VMRoutingFrameState.FAILED;
            if (CallFailureTrees && ParentRoute == null)
            {
                var bhav = Global.Resource.Get<BHAV>(ROUTE_FAIL_TREE);
                Thread.ExecuteSubRoutine(this, bhav, CodeOwner, new VMSubRoutineOperand(new short[] { (short)code, (blocker==null)?(short)0:blocker.ObjectID, 0, 0 }));
            }
        }

        private bool DoRoomRoute(VMFindLocationResult route)
        {
            Rooms = new Stack<VMRoomPortal>();

            LotTilePos dest;
            if (Slot != null)
            {
                //take destination pos from object. Estimate room closeness using distance to object, not destination.
                dest = Target.Position;
            }
            else
            {
                if (route != null) dest = route.Position;
                else return false; //???
            }

            var DestRoom = VM.Context.GetRoomAt(dest);
            var MyRoom = VM.Context.GetRoomAt(Caller.Position);

            IgnoreRooms = (Slot != null && (Slot.Rsflags & SLOTFlags.IgnoreRooms) > 0);

            if (DestRoom == MyRoom || IgnoreRooms) return true; //we don't have to do any room finding for this
            else
            {
                //find shortest room traversal to destination. Simple A* pathfind.
                //Portals are considered nodes to allow multiple portals between rooms to be considered.

                var openSet = new List<VMRoomPortal>(); //we use this like a queue, but we need certain functions for sorted queue that are only provided by list.
                var closedSet = new HashSet<VMRoomPortal>();

                var gScore = new Dictionary<VMRoomPortal, double>();
                var fScore = new Dictionary<VMRoomPortal, double>();
                var parents = new Dictionary<VMRoomPortal, VMRoomPortal>();

                var StartPortal = new VMRoomPortal(Caller.ObjectID, MyRoom); //consider the sim as a portal to this room (as a starting point)
                openSet.Add(StartPortal);
                gScore[StartPortal] = 0;
                fScore[StartPortal] = GetDist(Caller.Position, dest);

                while (openSet.Count != 0)
                {
                    var current = openSet[0];
                    openSet.RemoveAt(0);

                    if (current.TargetRoom == DestRoom)
                    {
                        //this portal gets us to the room.
                        while (current != StartPortal) //push previous portals till we get to our first "portal", the sim in its current room (we have already "traversed" this portal)
                        {
                            Rooms.Push(current);
                            current = parents[current];
                        }
                        return true;
                    }

                    closedSet.Add(current);

                    var portals = VM.Context.RoomInfo[current.TargetRoom].Portals;

                    foreach (var portal in portals)
                    { //evaluate all neighbor portals
                        if (IgnoredRooms.Contains(portal) || closedSet.Contains(portal)) continue; //already evaluated, or couldn't get to the portal.

                        var pos = VM.GetObjectById(portal.ObjectID).Position;
                        var gFromCurrent = gScore[current] + GetDist(VM.GetObjectById(current.ObjectID).Position, pos);
                        var newcomer = !openSet.Contains(portal);

                        if (newcomer || gFromCurrent < gScore[portal])
                        {
                            parents[portal] = current; //best parent for now
                            gScore[portal] = gFromCurrent;
                            fScore[portal] = gFromCurrent + GetDist(pos, dest);
                            if (newcomer)
                            { //add and move to relevant position
                                OpenSetSortedInsert(openSet, fScore, portal);
                            }
                            else
                            { //remove and reinsert to refresh sort
                                openSet.Remove(portal);
                                OpenSetSortedInsert(openSet, fScore, portal);
                            }
                        }
                    }
                }

                return false;
            }
        }

        private VMRouteFailCode AttemptRoute(VMFindLocationResult route) { //returns false if there is no room portal route to the destination room.
            //if route is not null, we are on a DIRECT route, where either the SLOT has been resolved or a route has already been passed to us.
            //resets some variables either way, so that the route can start again.

            CurRoute = route;

            WalkTo = null; //reset routing state
            AttemptedChair = false;
            TurnTweak = 0;

            return (DoRoomRoute(route)) ? VMRouteFailCode.Success : VMRouteFailCode.NoRoomRoute;
        }

        /// <summary>
        /// Pathfinds to the destination position from the current. The room pathfind should get us to the same room before we do this.
        /// </summary>
        private bool AttemptWalk() 
        {
            //find shortest path to destination tile. Simple A* pathfind.
            //portals are used to traverse floors, so we do not care about the floor each point is on.
            //when evaluating possible adjacent tiles we use the Caller's current floor.


            LotTilePos startPos = Caller.Position;
            CurrentWaypoint = LotTilePos.OUT_OF_WORLD;
            var myRoom = VM.Context.GetRoomAt(startPos);

            var roomInfo = VM.Context.RoomInfo[myRoom];
            var obstacles = new List<VMObstacle>();

            int bx = (roomInfo.Room.Bounds.X-1) << 4;
            int by = (roomInfo.Room.Bounds.Y-1) << 4;
            int width = (roomInfo.Room.Bounds.Width+2) << 4;
            int height = (roomInfo.Room.Bounds.Height+2) << 4;
            obstacles.Add(new VMObstacle(bx-16, by-16, bx+width+16, by));
            obstacles.Add(new VMObstacle(bx-16, by+height, bx+width+16, by+height+16));

            obstacles.Add(new VMObstacle(bx-16, by-16, bx, by+height+16));
            obstacles.Add(new VMObstacle(bx+width, by-16, bx+width+16, by+height+16));

            foreach (var obj in roomInfo.Entities)
            {
                var ft = obj.Footprint;

                var flags = (VMEntityFlags)obj.GetValue(VMStackObjectVariable.Flags);
                if (obj != Caller && ft != null &&
                    (obj is VMGameObject) &&
                    ((flags & VMEntityFlags.DisallowPersonIntersection) > 0 || (flags & VMEntityFlags.AllowPersonIntersection) == 0)
                    && (!(Caller.ExecuteEntryPoint(5, VM.Context, true, obj, new short[] { obj.ObjectID, 0, 0, 0 })
                        || obj.ExecuteEntryPoint(5, VM.Context, true, Caller, new short[] { Caller.ObjectID, 0, 0, 0 }))))
                    obstacles.Add(new VMObstacle(ft.x1-3, ft.y1-3, ft.x2+3, ft.y2+3));
            }

            obstacles.AddRange(roomInfo.Room.WallObs);
            if (!IgnoreRooms) obstacles.AddRange(roomInfo.Room.RoomObs);

            var startPoint = new Point((int)startPos.x, (int)startPos.y);
            var endPoint = new Point((int)CurRoute.Position.x, (int)CurRoute.Position.y);

            foreach (var rect in obstacles)
            {
                if (rect.HardContains(startPoint)) return false;
            }

            var router = new VMRectRouter(obstacles);

            if (startPoint == endPoint)
            {
                State = VMRoutingFrameState.TURN_ONLY;
                return true;
            }

            WalkTo = router.Route(startPoint, endPoint);
            if (WalkTo != null)
            {
                if (WalkTo.First.Value != endPoint) WalkTo.RemoveFirst();
                AdvanceWaypoint();
            }
            return (WalkTo != null);
        }

        private void OpenSetSortedInsert(List<VMRoomPortal> set, Dictionary<VMRoomPortal, double> fScore, VMRoomPortal portal)
        {
            var myScore = fScore[portal];
            for (int i = 0; i < set.Count; i++)
            {
                if (myScore < fScore[set[i]])
                {
                    set.Insert(i, portal);
                    return;
                }
            }
            set.Add(portal);
        }

        private double GetDist(LotTilePos pos1, LotTilePos pos2)
        {
            return Math.Sqrt(Math.Pow(pos1.x - pos2.x, 2) + Math.Pow(pos1.y - pos2.y, 2))/16.0 + Math.Abs(pos1.Level-pos2.Level)*10;
        }

        private bool PushEntryPoint(int entryPoint, VMEntity ent) {
            if (ent.EntryPoints[entryPoint].ActionFunction != 0)
            {
                bool Execute;
                if (ent.EntryPoints[entryPoint].ConditionFunction != 0) //check if we can definitely execute this...
                {
                    var Behavior = ent.GetBHAVWithOwner(ent.EntryPoints[entryPoint].ConditionFunction, VM.Context);
                    Execute = (VMThread.EvaluateCheck(VM.Context, Caller, new VMQueuedAction()
                    {
                        Callee = ent,
                        CodeOwner = Behavior.owner,
                        StackObject = ent,
                        Routine = VM.Assemble(Behavior.bhav),
                    }) == VMPrimitiveExitCode.RETURN_TRUE);

                }
                else
                {
                    Execute = true;
                }

                if (Execute)
                {
                    //push it onto our stack, except now the object owns our soul! when we are returned to we can evaluate the result and determine if the action failed.
                    var Behavior = ent.GetBHAVWithOwner(ent.EntryPoints[entryPoint].ActionFunction, VM.Context);
                    var routine = VM.Assemble(Behavior.bhav);
                    var childFrame = new VMStackFrame
                    {
                        Routine = routine,
                        Caller = Caller,
                        Callee = ent,
                        CodeOwner = Behavior.owner,
                        StackObject = ent
                    };
                    childFrame.Args = new short[routine.Arguments];
                    Thread.Push(childFrame);
                    return true;
                }
                else
                {
                    return false; //could not execute portal function. todo: re-evaluate room route
                }
            }
            else
            {
                return false;
            }
        }

        public VMPrimitiveExitCode Tick()
        {

            if (State != VMRoutingFrameState.FAILED)
            {
                HardFail(VMRouteFailCode.Interrupted, null);
                return VMPrimitiveExitCode.CONTINUE;
            }

            if (WaitTime > 0)
            {
                if (Velocity > 0) Velocity--;

                WaitTime--;
                Timeout--;
                if (Timeout <= 0)
                {
                    //try again. not sure if we should reset timeout for the new route
                    SoftFail(VMRouteFailCode.NoPath, null);
                    if (State != VMRoutingFrameState.FAILED) {
                        Velocity = 0;
                        State = VMRoutingFrameState.WALKING;
                    }
                } else return VMPrimitiveExitCode.CONTINUE_NEXT_TICK;
            }

            if (RoomRouteInvalid && State != VMRoutingFrameState.BEGIN_TURN && State != VMRoutingFrameState.END_TURN)
            {
                RoomRouteInvalid = false;
                IgnoredRooms.Clear();

                WalkTo = null; //reset routing state
                if (!DoRoomRoute(CurRoute))
                {
                    if (CurRoute != null) SoftFail(VMRouteFailCode.NoRoomRoute, null);
                    else HardFail(VMRouteFailCode.NoRoomRoute, null);
                }
                else if (Rooms.Count > 0)
                {
                    State = VMRoutingFrameState.INITIAL;
                }
            }

            switch (State)
            {
                case VMRoutingFrameState.STAND_FUNC:
                    if (Thread.LastStackExitCode == VMPrimitiveExitCode.RETURN_TRUE)
                    {
                        State = VMRoutingFrameState.INITIAL;
                       
                    }
                    else {

                    }
                    return VMPrimitiveExitCode.CONTINUE;
                case VMRoutingFrameState.INITIAL:
                case VMRoutingFrameState.ROOM_PORTAL:
                    //check if the room portal that just finished succeeded.
                    if (State == VMRoutingFrameState.ROOM_PORTAL) { 
                        if (Thread.LastStackExitCode != VMPrimitiveExitCode.RETURN_TRUE)
                        {
                            IgnoredRooms.Add(CurrentPortal);
                            State = VMRoutingFrameState.INITIAL;
                            if (!DoRoomRoute(CurRoute))
                            {
                                SoftFail(VMRouteFailCode.NoRoomRoute, null); //todo: reattempt room route with portal we tried removed.
                                return VMPrimitiveExitCode.CONTINUE;
                            }
                        }
                    }

                    if (Rooms.Count > 0)
                    { //push portal function of next portal
                        CurrentPortal = Rooms.Pop();
                        var ent = VM.GetObjectById(CurrentPortal.ObjectID);
                        State = VMRoutingFrameState.ROOM_PORTAL;
                        if (!PushEntryPoint(15, ent)) //15 is portal function
                            SoftFail(VMRouteFailCode.NoRoomRoute, null); //could not execute portal function
                        return VMPrimitiveExitCode.CONTINUE;
                    }

                    //if we're here, room route is OK. start routing to a destination.
                    if (Choices == null)
                    {
                        //perform slot parse.
                        if (Slot == null)
                        {
                            HardFail(VMRouteFailCode.Unknown, null);
                            return VMPrimitiveExitCode.CONTINUE; //this should never happen. If it does, someone has used the routing system incorrectly.
                        }

                        var parser = new VMSlotParser(Slot);

                        Choices = parser.FindAvaliableLocations(Target, VM.Context, null);
                        if (Choices.Count == 0)
                        {
                            HardFail(parser.FailCode, parser.Blocker);
                            return VMPrimitiveExitCode.CONTINUE;
                        }
                        else
                        {
                            CurRoute = Choices[0];
                            Choices.RemoveAt(0);
                        }
                    }

                    //do we need to sit in a seat? it should take over.
                    if (CurRoute.Chair != null)
                    {
                        if (!AttemptedChair)
                        {
                            AttemptedChair = true;
                            if (PushEntryPoint(26, CurRoute.Chair)) return VMPrimitiveExitCode.CONTINUE;
                            else
                            {
                                SoftFail(VMRouteFailCode.CantSit, null);
                                return VMPrimitiveExitCode.CONTINUE;
                            }
                        }
                        else
                        {
                            if (Thread.LastStackExitCode == VMPrimitiveExitCode.RETURN_TRUE) return VMPrimitiveExitCode.RETURN_TRUE;
                            else
                            {
                                SoftFail(VMRouteFailCode.CantSit, null);
                                return VMPrimitiveExitCode.CONTINUE;
                            }
                        }
                    }


                    //no chair, we just need to walk to the spot. Start the within-room routing.
                    if (WalkTo == null)
                    {
                        if (!AttemptWalk())
                        {
                            SoftFail(VMRouteFailCode.NoPath, null);
                            return VMPrimitiveExitCode.CONTINUE;
                        }
                    }

                    return VMPrimitiveExitCode.CONTINUE;
                case VMRoutingFrameState.FAILED:
              
                    return VMPrimitiveExitCode.CONTINUE;
                case VMRoutingFrameState.BEGIN_TURN:
 
                return VMPrimitiveExitCode.CONTINUE_NEXT_TICK;
            }
            return VMPrimitiveExitCode.GOTO_FALSE; //???
        }



        private VMRoutingFrame GetParentFrame()
        {
            //look for parent frame
            for (int i = Thread.Stack.Count - 2; i >= 0; i--)
            {
                var frame = Thread.Stack[i];
                if (frame is VMRoutingFrame)
                {
                    return (VMRoutingFrame)frame;
                    
                }
            }
            return null;
        }

        private bool CanPortalTurn()
        {
            var rf = ParentRoute;
            if (rf == null) rf = this;
            return (rf.State != VMRoutingFrameState.ROOM_PORTAL || rf.PortalTurns++ == 0);
        }



        private bool AdvanceWaypoint()
        {
            if (WalkTo.Count == 0) return false;

            var point = WalkTo.First.Value;
            WalkTo.RemoveFirst();
            if (WalkTo.Count > 0)
            {
                CurrentWaypoint = new LotTilePos((short)point.X, (short)point.Y, Caller.Position.Level);
            }
            else CurrentWaypoint = CurRoute.Position; //go directly to position at last
            PreviousPosition = Caller.Position;
            MoveFrames = 0;


            WalkDirection = Caller.RadianDirection;
            TargetDirection = Math.Atan2(CurrentWaypoint.x - Caller.Position.x, Caller.Position.y - CurrentWaypoint.y); //y+ as north. x+ is -90 degrees.
            TurnFrames = 10;
            return true;
        }

    }

    public enum VMRoutingFrameState : byte
    {
        INITIAL,
    
        BEGIN_TURN,
        WALKING,
        TURN_ONLY,
        END_TURN,

        SHOOED, //recalculate route once the stack gets back here.
        ROOM_PORTAL,
        STAND_FUNC,
        FAILED
    }
}
