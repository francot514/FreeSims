using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.SimAntics;
using FSO.Files.Formats.IFF.Chunks;
using FSO.LotView.Model;
using FSO.Content;
using FSO.SimAntics.Engine;

    namespace TSO.SimsAntics
    {
        public class VMFreeWill
        {
            public VM VM;
            public List<VMEntity> Objects;
            public List<VMEntity> Entities;
            public List<VMAvatar> Visitors;
            public VMEntity Target, Subject;
            private TTAB TreeTableSelected;
            private List<string> interactionList;
            public int SelectedID;


            public VMFreeWill(VM vm)
            {

                VM = vm;
                Entities = vm.Entities;
                Objects = new List<VMEntity>();
                Visitors = new List<VMAvatar>();

                foreach (VMEntity ent in Entities)
                    if (((VMAvatar)(ent)).Visitor)
                        Visitors.Add((VMAvatar)ent);

            }

            public void CheckForUsableObjects(VM vm)
            {

                Visitors.Clear();
                Entities = vm.Entities;


                if (Entities.Count > 0)
                    foreach (var ent in Entities)
                    {
                        if (ent.Position != LotTilePos.OUT_OF_WORLD && ent.GetBadNames() == false
                            && ent.Object.GUID != VMAvatar.TEMPLATE_PERSON && ent.Object.GUID != VMAvatar.TEMPLATE_NPC)
                            Objects.Add(ent);

                        
                    }


                foreach (VMEntity ent in Entities)
                    if (ent is VMAvatar)
                        Visitors.Add((VMAvatar)ent);


            }



            private void SetSelected(VMEntity entity, bool isPet)
            {
                interactionList = new List<string>();
                interactionList.Clear();
                if (entity.TreeTable != null && entity.TreeTableStrings != null)
                {
                    TreeTableSelected = entity.TreeTable;

                    if (isPet)
                    foreach (var interaction in entity.TreeTable.Interactions)
                    {

                       if (interaction.Flags == TTABFlags.TS1AllowDogs || interaction.Flags == TTABFlags.TS1AllowCats)
                         interactionList.Add(entity.TreeTableStrings.GetString((int)interaction.TTAIndex));
                    }

                    else
                    foreach (var interaction in entity.TreeTable.Interactions)
                    {

                        if (!interaction.Debug || !interaction.WhenDead || !interaction.Leapfrog 
                            || !interaction.AllowGhosts || !interaction.AllowCSRs || !interaction.AllowConsecutive
                            || !interaction.AllowDogs || !interaction.AllowCats)
                            interactionList.Add(entity.TreeTableStrings.GetString((int)interaction.TTAIndex));
                    }
                }

            }

            private void ExecuteAction(VMEntity avatar, VMEntity target)
            {
                int id = 0;

                Random rand = new Random();

                if (interactionList.Count > 0)
                {
                    id = rand.Next(0, interactionList.Count - 1);

                    var interaction = TreeTableSelected.Interactions[id];
                    var ActionID = interaction.ActionFunction;
                    BHAV bhav;
                    GameIffResource CodeOwner;

                    if (ActionID < 4096)
                    { //global
                        bhav = null;
                        //unimp as it has to access the context to get this.
                    }
                    else if (ActionID < 8192)
                    { //local
                        bhav = target.Object.Resource.Get<BHAV>(ActionID);
                        CodeOwner = target.Object.Resource;
                    }
                    else
                    { //semi-global
                        bhav = target.SemiGlobal.Get<BHAV>(ActionID);
                        CodeOwner = target.SemiGlobal;
                    }

                   

                    if (bhav != null)
                    {
                        avatar.Thread.EnqueueAction(new VMQueuedAction()
                        {
                            ActionRoutine = VM.Assemble(bhav),
                            Callee = target,
                            StackObject = target,
                            CodeOwner = target.Object,
                            InteractionNumber = (int)interaction.TTAIndex, //interactions are referenced by their tta index
                            Priority = (short)VMQueuePriority.UserDriven,
                            
                        });
                    }

                }

            }


            public void RunAction(VMEntity entity)
            {

                bool PetAction = entity.Object.GUID == VMAvatar.DOG_TEMPLATE ||
                entity.Object.GUID == VMAvatar.CAT_TEMPLATE ? true : false;

                if (Objects.Count > 0)
                {



                    Random rand = new Random();
                    int n = rand.Next(0, Objects.Count - 1);

                    Target = Objects[n];
                    Objects.RemoveAt(n);

                    SetSelected(Target, PetAction);

                    if (entity != Target)
                        ExecuteAction(entity, Target);
                }


            }

            public void Tick(VM vm)
            {

                Objects.Clear();

                CheckForUsableObjects(vm);

                for (int i = 0; i < Visitors.Count; i++)
                
                if (Visitors[i].Visitor)
                {

                   if (Visitors[i].Thread.Queue.Count > 2)
                      Visitors[i].Thread.Queue.Clear();


                    if (Visitors[i].Thread.Queue.Count < 2 ||
                        Visitors[i].Thread.Queue[0].Priority == (short)VMQueuePriority.Idle ||
                        Visitors[i].Thread.Queue[1].Priority == (short)VMQueuePriority.Idle )

                    {
                        RunAction(Visitors[i]);
                    }

                }


            }

        }
    }

