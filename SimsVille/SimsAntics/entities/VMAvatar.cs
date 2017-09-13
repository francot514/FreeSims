/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tso.world.Components;
using TSO.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSO.SimsAntics.Model;
using tso.world.Model;
using TSO.Files.formats.iff.chunks;
using TSO.Common.utils;
using TSO.SimsAntics.Model.Routing;
using TSO.HIT;
using System.IO;
using tso.simantics.Model;
using TSO.vitaboy;
using TSO.Common.rendering.vitaboy;
using TSO.SimAntics.Model;
using tso.simantics.Engine;

namespace TSO.SimsAntics
{
    public class VMAvatar : VMEntity
    {
        public static uint TEMPLATE_PERSON = 0x7FD96B54;
        public static uint NPC_MAID = 0xC61931DC;
        public static uint NPC_GARDENER = 0xD62B91B4;
        public static uint DOG_TEMPLATE = 0x5F0C674C;

        public SimAvatar Avatar;

        /** Animation vars **/

        public List<VMAnimationState> Animations;

        public VMAnimationState CurrentAnimationState {
            get
            {
                return (Animations.Count == 0) ? null : Animations[0];
            }
        }
        public VMAnimationState CarryAnimationState;


        public string Name;
        private string m_Message = "";
        public string Message
        {
            get { return m_Message; }
            set
            {
                m_Message = value;
                SetPersonData(VMPersonDataVariable.ChatBaloonOn, 1);
                MessageTimeout = 150 + value.Length/2;
            }
        }

        public int MessageTimeout;
        public Vector3 Velocity; //used for 60 fps walking animation

        private VMMotiveChange[] MotiveChanges = new VMMotiveChange[16];
        private VMMotiveDecay MotiveDecay;
        private short[] PersonData = new short[100];
        private short[] MotiveData = new short[16];
        private VMEntity HandObject;
        private float _RadianDirection;

        /*
            APPEARANCE DATA
        */
        public VMAvatarDefaultSuits DefaultSuits = new VMAvatarDefaultSuits(false);
        public HashSet<string> BoundAppearances = new HashSet<string>();

        private VMOutfitReference _BodyOutfit;
        public VMOutfitReference BodyOutfit
        {
            set
            {
                _BodyOutfit = value;
                if (UseWorld)
                {
                    Avatar.Body = value.GetContent();
                    if (AvatarType == VMAvatarType.Adult || AvatarType == VMAvatarType.Child) Avatar.Handgroup = Avatar.Body;
                }
            }
            get
            {
                return _BodyOutfit;
            }
        }

        private VMOutfitReference _HeadOutfit;
        public VMOutfitReference HeadOutfit
        {
            set
            {
                _HeadOutfit = value;
                if (UseWorld)
                {
                    Avatar.Head = value.GetContent();
                }
            }
            get
            {
                return _HeadOutfit;
            }
        }


        private AppearanceType _SkinTone;
        public AppearanceType SkinTone
        {
            set
            {
                _SkinTone = value;
                Avatar.Appearance = value;
            }
            get { return _SkinTone; }
        }

        public override Vector3 VisualPosition
        {
            get { return (UseWorld) ? WorldUI.Position : new Vector3(); }
            set { if (UseWorld) WorldUI.Position = value; }
        }
        public override float RadianDirection
        {
            get { return _RadianDirection; }
            set
            {
                _RadianDirection = value;
                if (UseWorld) ((AvatarComponent)WorldUI).RadianDirection = value;
            }
        }

        public override Direction Direction
        {
            get
            {
                int midPointDir = (int)DirectionUtils.PosMod(Math.Round(_RadianDirection / (Math.PI / 4f)), 8);
                return (Direction)(1 << (midPointDir));
            }
            set { RadianDirection = ((int)Math.Round(Math.Log((double)value, 2))) * (float)(Math.PI / 4.0); }
        }

        //inferred properties
        public string[] WalkAnimations = new string[50];
        public string[] SwimAnimations = new string[50];
        private STR BodyStrings;
        private VMAvatarType AvatarType;

        public void SubmitHITVars(HITThread thread)
        {
            if (thread.ObjectVar == null) return;
            thread.ObjectVar[12] = GetPersonData(VMPersonDataVariable.Gender);
        }

        public bool IsPet
        {
            get
            {
                var gender = GetPersonData(VMPersonDataVariable.Gender);
                return (gender & (8 | 16)) > 0; //flags are dog, cat.
            }
        }

        public VMAvatar(GameObject obj)
            : base(obj)
        {
            Name = "Sim";

            BodyStrings = Object.Resource.Get<STR>(Object.OBJ.BodyStringID);

            SetAvatarType(BodyStrings);
            SkinTone = AppearanceType.Light;

            if (UseWorld)
            {
                WorldUI = new AvatarComponent();
                var avatarc = (AvatarComponent)WorldUI;
                avatarc.Avatar = Avatar;
            }

            MotiveDecay = new VMMotiveDecay();
            for (int i = 0; i < 16; i++)
            {
                MotiveChanges[i] = new VMMotiveChange();
                MotiveChanges[i].Motive = (VMMotive)i;
            }
        }

        public void SetAvatarType(STR data) {
            if (data == null)
            {
                AvatarType = VMAvatarType.Adult;
            }
            else
            {
                var type = data.GetString(0);
                if (type == "adult") AvatarType = VMAvatarType.Adult;
                else if (type == "child") AvatarType = VMAvatarType.Child;
                else if (type == "cat") AvatarType = VMAvatarType.Cat;
                else if (type == "dog") AvatarType = VMAvatarType.Dog;
            }

            switch (AvatarType)
            {
                case VMAvatarType.Adult:
                    Avatar = new SimAvatar(TSO.Content.Content.Get().AvatarSkeletons.Get("adult.skel"));
                    Avatar.Head = _HeadOutfit.GetContent();//default to bob newbie, why not
                    Avatar.Body = _BodyOutfit.GetContent();
                    Avatar.Handgroup = Avatar.Body;
                    break;
                case VMAvatarType.Cat:
                    var skel = TSO.Content.Content.Get().AvatarSkeletons.Get("cat.skel");
                    Avatar = new SimAvatar(skel);
                    Avatar.Body = _BodyOutfit.GetContent();
                    break;
                case VMAvatarType.Dog:
                    Avatar = new SimAvatar(TSO.Content.Content.Get().AvatarSkeletons.Get("dog.skel"));
                    Avatar.Body = _BodyOutfit.GetContent();
                    break;
            }
        }

        public void SetAvatarBodyStrings(STR data, VMContext context) {
            if (data == null) return;

            try
            {
                var body = data.GetString(1);
                var randBody = data.GetString(10);

                 if (randBody != "")
                    {
                        var bodySpl = randBody.Split(';');
                        DefaultSuits.Daywear = VMOutfitReference.Parse(bodySpl[context.NextRandom((ulong)bodySpl.Length - 1)], true);
                    }
                 else if (body != "")
                 {
                     DefaultSuits.Daywear = VMOutfitReference.Parse(body, true);
                 }

                HeadOutfit = new VMOutfitReference(data, true);
                BodyOutfit = DefaultSuits.Daywear; BodyOutfit = DefaultSuits.Daywear;

                var head = data.GetString(2);
                var randHead = data.GetString(9);

                if (randHead != "")
                {
                    var headSpl = randHead.Split(';');
                    HeadOutfit = VMOutfitReference.Parse(headSpl[context.NextRandom((ulong)headSpl.Length - 1)], true);
                }
                else if (head != "")
                {
                    HeadOutfit = VMOutfitReference.Parse(head, true);
                }
            }
            catch
            {
                //head or body invalid, resort to default.
            }

            var gender = data.GetString(12);
            var genVar = (int)VMPersonDataVariable.Gender;

            if (gender.Equals("male", StringComparison.InvariantCultureIgnoreCase)) PersonData[genVar] = 0;
            else if (gender.Equals("female", StringComparison.InvariantCultureIgnoreCase)) PersonData[genVar] = 1;
            else if (gender.Equals("dogmale", StringComparison.InvariantCultureIgnoreCase)) PersonData[genVar] = 8;
            else if (gender.Equals("dogfemale", StringComparison.InvariantCultureIgnoreCase)) PersonData[genVar] = 9;
            else if (gender.Equals("catmale", StringComparison.InvariantCultureIgnoreCase)) PersonData[genVar] = 16;
            else if (gender.Equals("catfemale", StringComparison.InvariantCultureIgnoreCase)) PersonData[genVar] = 17;

            var names = data.GetString(11);
            if (names != "")
            {
                var nameSpl = names.Split(';');
                Name = nameSpl[context.NextRandom((ulong)nameSpl.Length)];
            }

            PersonData[(int)VMPersonDataVariable.PersonsAge] = Convert.ToInt16(data.GetString(13));

            var skinTone = data.GetString(14);
            if (skinTone.Equals("lgt", StringComparison.InvariantCultureIgnoreCase)) SkinTone = AppearanceType.Light;
            else if (skinTone.Equals("med", StringComparison.InvariantCultureIgnoreCase)) SkinTone = AppearanceType.Medium;
            else if (skinTone.Equals("drk", StringComparison.InvariantCultureIgnoreCase)) SkinTone = AppearanceType.Dark;
        }

        public void SetAvatarData(XmlCharacter charInfo)
        {

            if (charInfo != null)
            {
                PersistID = Convert.ToUInt16(charInfo.Id);
                AppearanceType type;
                Enum.TryParse(charInfo.Appearance, out type);


                Name = charInfo.Name;
                SkinTone = type;
                AvatarType = VMAvatarType.Adult;

                var Gender = charInfo.Gender;
                if (Gender == "Male")
                    SetPersonData(VMPersonDataVariable.Gender, 0);
                else if (Gender == "Female")
                    SetPersonData(VMPersonDataVariable.Gender, 1);

                Avatar.Handgroup = Avatar.Body;
            }
            else
            {

                Avatar.Head = _HeadOutfit.GetContent();
                Avatar.Body = _BodyOutfit.GetContent();
                Avatar.Handgroup = Avatar.Body;

            }


        }

        public void InitBodyData(VMContext context)
        {
            //init walking strings
            var GlobWalk = context.Globals.Resource.Get<STR>(150);
            for (int i = 0; i < GlobWalk.Length; i++)
            {
                WalkAnimations[i] = GlobWalk.GetString(i);
            }

            var GlobSwim = context.Globals.Resource.Get<STR>(158);
            for (int i = 0; i < GlobSwim.Length; i++)
            {
                SwimAnimations[i] = GlobSwim.GetString(i);
            }

            var SpecialWalk = Object.Resource.Get<STR>(150);
            if (SpecialWalk != null)
            {
                for (int i = 0; i < SpecialWalk.Length; i++)
                {
                    var str = SpecialWalk.GetString(i);
                    if (str != "") WalkAnimations[i] = str;
                }
            }
        }



        public override void Init(VMContext context)
        {
            if (UseWorld) WorldUI.ObjectID = ObjectID;
            base.Init(context);

            Animations = new List<VMAnimationState>();

            SetAvatarBodyStrings(Object.Resource.Get<STR>(Object.OBJ.BodyStringID), context);
            InitBodyData(context);

            SetMotiveData(VMMotive.Comfort, 100);
            SetPersonData(VMPersonDataVariable.NeatPersonality, 1000); //for testing wash hands after toilet
            SetPersonData(VMPersonDataVariable.OnlineJobID, 1); //for testing wash hands after toilet
            SetPersonData(VMPersonDataVariable.IsHousemate, 2);

            SetPersonData(VMPersonDataVariable.CreativitySkill, 1000);
            SetPersonData(VMPersonDataVariable.CookingSkill, 1000);
            SetPersonData(VMPersonDataVariable.CharismaSkill, 1000);
            SetPersonData(VMPersonDataVariable.LogicSkill, 1000);
            SetPersonData(VMPersonDataVariable.BodySkill, 1000);
        }

        private void HandleTimePropsEvent(TimePropertyListItem tp)
        {
            VMAvatar avatar = this;
            var evt = tp.Properties["xevt"];
            if (evt != null)
            {
                var eventValue = short.Parse(evt);
                avatar.CurrentAnimationState.EventCode = eventValue;
                avatar.CurrentAnimationState.EventFired = true;
            }
            var rhevt = tp.Properties["righthand"];
            if (rhevt != null)
            {
                var eventValue = short.Parse(rhevt);
                avatar.Avatar.RightHandGesture = (SimHandGesture)eventValue;
            }
            var lhevt = tp.Properties["lefthand"];
            if (lhevt != null)
            {
                var eventValue = short.Parse(lhevt);
                avatar.Avatar.LeftHandGesture = (SimHandGesture)eventValue;
            }

            var soundevt = tp.Properties["sound"];
            if (UseWorld && soundevt != null)
            {
                var thread = TSO.HIT.HITVM.Get().PlaySoundEvent(soundevt);
                if (thread != null)
                {
                    var owner = this;
                    if (thread is HITThread) SubmitHITVars((HITThread)thread);

                    if (!thread.AlreadyOwns(owner.ObjectID)) thread.AddOwner(owner.ObjectID);

                    var entry = new VMSoundEntry()
                    {
                        Sound = thread,
                        Pan = true,
                        Zoom = true,
                    };
                    owner.SoundThreads.Add(entry);
                    owner.TickSounds();
                }
            }
        }

        public override void Tick()
        {
            Velocity = new Vector3(0, 0, 0);
            base.Tick();

            if (Message != "")
            {
                if (MessageTimeout-- > 0)
                {
                    if (MessageTimeout == 0)
                    {
                        SetPersonData(VMPersonDataVariable.ChatBaloonOn, 0);
                        m_Message = "";
                    }
                }
            }

            if (Thread != null && Thread.ThreadBreak == Engine.VMThreadBreakMode.Pause) return;

            if (PersonData[(int)VMPersonDataVariable.OnlineJobStatusFlags] == 0) PersonData[(int)VMPersonDataVariable.OnlineJobStatusFlags] = 1;
            if (Thread != null)
                {
                MotiveDecay.Tick(this, Thread.Context);
                SetPersonData(VMPersonDataVariable.OnlineJobGrade, Math.Max((short)0, Thread.Context.VM.GetGlobalValue(11))); //force job grade to what we expect
                }

            //animation update for avatars
            VMAvatar avatar = this;
            if (avatar.Position == LotTilePos.OUT_OF_WORLD) avatar.Position = new LotTilePos(8, 8, 1);
            float totalWeight = 0f;
            foreach (var state in Animations)
            {
                totalWeight += state.Weight;
                if (!state.EndReached && state.Weight != 0)
                {
                    if (state.PlayingBackwards) state.CurrentFrame -= state.Speed;
                    else state.CurrentFrame += state.Speed;
                    var currentFrame = state.CurrentFrame;
                    var currentTime = currentFrame * 33.33f;
                    var timeProps = state.TimePropertyLists;
                    if (!state.PlayingBackwards)
                    {
                        for (var i = 0; i < timeProps.Count; i++)
                        {
                            var tp = timeProps[i];
                            if (tp.ID > currentTime)
                            {
                                break;
                            }

                            timeProps.RemoveAt(0);
                            i--;

                            HandleTimePropsEvent(tp);
                        }
                    }
                    else
                    {
                        for (var i = timeProps.Count - 1; i >= 0; i--)
                        {
                            var tp = timeProps[i];
                            if (tp.ID < currentTime)
                            {
                                break;
                            }

                            timeProps.RemoveAt(timeProps.Count - 1);
                            HandleTimePropsEvent(tp);
                        }
                    }

                    var status = Animator.RenderFrame(avatar.Avatar, state.Anim, (int)state.CurrentFrame, state.CurrentFrame%1f, state.Weight/totalWeight);
                    if (status != AnimationStatus.IN_PROGRESS)
                    {
                        if (state.Loop)
                        {
                            if (state.PlayingBackwards) state.CurrentFrame += state.Anim.NumFrames;
                            else state.CurrentFrame -= state.Anim.NumFrames;
                        }
                        else
                            state.EndReached = true;
                    }
                }
            }

            if (avatar.CarryAnimationState != null)
            {
                var status = Animator.RenderFrame(avatar.Avatar, avatar.CarryAnimationState.Anim, (int)avatar.CarryAnimationState.CurrentFrame, 0.0f, 1f); //currently don't advance frames... I don't think any of them are animated anyways.
            }

            for (int i = 0; i < 16; i++)
            {
                MotiveChanges[i].Tick(this); //tick over motive changes
            }

            avatar.Avatar.ReloadSkeleton();

            PersonData[(int)VMPersonDataVariable.TickCounter]++;
        }

        public void FractionalAnim(float fraction)
        {
            var avatar = (VMAvatar)this;
            float totalWeight = 0f;
            foreach (var state in Animations)
            {
                totalWeight += state.Weight;
                if (!state.EndReached)
                {
                    float visualFrame = state.CurrentFrame;
                    if (state.PlayingBackwards) visualFrame -= state.Speed/2;
                    else visualFrame += state.Speed/2;

                    Animator.RenderFrame(avatar.Avatar, state.Anim, (int)visualFrame, visualFrame%1, state.Weight/totalWeight);
                }
            }
            if (avatar.CarryAnimationState != null) Animator.RenderFrame(avatar.Avatar, avatar.CarryAnimationState.Anim, (int)avatar.CarryAnimationState.CurrentFrame, 0.0f, 1f);

            //TODO: if this gets changed to run at variable framerate need to "remember" visual position
            avatar.Avatar.ReloadSkeleton();
            VisualPosition += fraction * Velocity;
        }

        public virtual short GetPersonData(VMPersonDataVariable variable)
        {
            if ((ushort)variable > 100) throw new Exception("Person Data out of bounds!");
            switch (variable)
            {
                case VMPersonDataVariable.Priority:
                    return (Thread.Queue.Count == 0) ? (short)0 : Thread.Queue[0].Priority;
            }
            return PersonData[(ushort)variable];
            
        }

        public virtual void SetMotiveChange(VMMotive motive, short PerHourChange, short MaxValue)
        {
            var temp = MotiveChanges[(int)motive];
            if (temp.Ticked)
                {
                temp.PerHourChange = PerHourChange;
                temp.MaxValue = MaxValue;
                temp.Ticked = false;
                }
        }

        public virtual void ClearMotiveChanges()
        {
            for (int i = 0; i < 16; i++)
            {
                MotiveChanges[i].Clear();
            }
        }

        public virtual bool SetPersonData(VMPersonDataVariable variable, short value)
            {
            if ((ushort)variable > 100) throw new Exception("Person Data out of bounds!");
            switch (variable)
            {
                case VMPersonDataVariable.Priority:
                    if (Thread.Queue.Count != 0) Thread.Queue[0].Priority = value;
                    return true;
            }
            PersonData[(ushort)variable] = value;
            return true;
        }

        public virtual short GetMotiveData(VMMotive variable) //needs special conditions for ones like Mood.
        {
            switch (variable){
                case VMMotive.Mood:
                    return 50; //always happy!! really!! it's not a front :(
            }
            if ((ushort)variable > 15) throw new Exception("Motive Data out of bounds!");
            return MotiveData[(ushort)variable];
        }

        public virtual bool SetMotiveData(VMMotive variable, short value)
        {
            if ((ushort)variable > 15) throw new Exception("Motive Data out of bounds!");
            MotiveData[(ushort)variable] = (short)Math.Max(Math.Min((int)value, 100), -100);
            return true;
        }

        public override string ToString()
        {
            return Name;
        }

        public override VMObstacle GetObstacle(LotTilePos pos, Direction dir)
        {
            return new VMObstacle(
                (pos.x - 3),
                (pos.y - 3),
                (pos.x + 3),
                (pos.y + 3));
        }

        public override void PositionChange(VMContext context, bool noEntryPoint)
        {
            if (GhostImage) return;

            var room = context.GetObjectRoom(this);
            SetRoom(room);

            if (HandObject != null)
            {
                HandObject.Position = Position;
                HandObject.SetRoom(room);
            }

            if (Container != null) return;
            if (Position == LotTilePos.OUT_OF_WORLD) return;

            context.RegisterObjectPos(this);

            base.PositionChange(context, noEntryPoint);
        }

        public override void PrePositionChange(VMContext context)
        {
            Footprint = null;
            if (GhostImage && UseWorld)
            {
                if (WorldUI.Container != null)
                {
                    WorldUI.Container = null;
                    WorldUI.ContainerSlot = 0;
                }
                return;
            }
            if (Container != null)
            {
                Container.ClearSlot(ContainerSlot);
                return;
            }
            if (Position == LotTilePos.OUT_OF_WORLD) return;

            context.UnregisterObjectPos(this);
            base.PrePositionChange(context);
        }

        // Begin Container SLOTs interface

        public override int TotalSlots()
        {
            return 1;
        }

        public override void PlaceInSlot(VMEntity obj, int slot, bool cleanOld, VMContext context)
        {
            if (cleanOld) obj.PrePositionChange(context);

            if (!obj.GhostImage)
            {
                HandObject = obj;

                CarryAnimationState = new VMAnimationState(TSO.Content.Content.Get().AvatarAnimations.Get("a2o-rarm-carry-loop.anim"), false); //set default carry animation

                obj.Container = this;
                obj.ContainerSlot = (short)slot;
            }
            if (UseWorld)
            {
                obj.WorldUI.Container = this.WorldUI;
                obj.WorldUI.ContainerSlot = slot;
                if (obj.WorldUI is ObjectComponent)
                {
                    var objC = (ObjectComponent)obj.WorldUI;
                    objC.ForceDynamic = true;
                }
            }
            obj.Position = Position; //TODO: is physical position the same as the slot offset position?
            if (cleanOld) obj.PositionChange(context, false);
        }

        public override int GetSlotHeight(int slot)
        {
            return 5; //in hand
            //TODO: verify
        }

        public override VMEntity GetSlot(int slot)
        {
            return HandObject;
        }

        public override void ClearSlot(int slot)
        {
            HandObject.Container = null;
            HandObject.ContainerSlot = -1;
            CarryAnimationState = null;

            if (UseWorld)
            {
                HandObject.WorldUI.Container = null;
                HandObject.WorldUI.ContainerSlot = 0;

                if (HandObject.WorldUI is ObjectComponent)
                {
                    var objC = (ObjectComponent)HandObject.WorldUI;
                    objC.ForceDynamic = false;
                }
            }

            HandObject = null;
        }

        // End Container SLOTs interface

        public override void SetRoom(ushort room)
        {
            base.SetRoom(room);
            if (VM.UseWorld) WorldUI.Room = (ushort)GetValue(VMStackObjectVariable.Room);
        }

        public override Texture2D GetIcon(GraphicsDevice gd, int store)
        {
            Outfit ThumbOutfit = (Avatar.Head == null) ? Avatar.Body : Avatar.Head;

            return null;
        }

    }

    public enum VMAvatarType : byte {
        Adult,
        Child,
        Cat,
        Dog
    }

    public class VMAvatarDefaultSuits : VMSerializable
    {
        public VMOutfitReference Daywear;
        public VMOutfitReference Swimwear;
        public VMOutfitReference Sleepwear;

        public VMAvatarDefaultSuits(bool female)
        {
            Daywear = new VMOutfitReference(0x24C0000000D);
            Swimwear = new VMOutfitReference((ulong)((female) ? 0x620000000D : 0x5470000000D));
            Sleepwear = new VMOutfitReference((ulong)((female) ? 0x5150000000D : 0x5440000000D));
        }

        public VMAvatarDefaultSuits(BinaryReader reader)
        {
            Deserialize(reader);
        }

        public void SerializeInto(BinaryWriter writer)
        {
            Daywear.SerializeInto(writer);
            Swimwear.SerializeInto(writer);
            Sleepwear.SerializeInto(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            Daywear = new VMOutfitReference(reader);
            Swimwear = new VMOutfitReference(reader);
            Sleepwear = new VMOutfitReference(reader);
        }
    }
}
