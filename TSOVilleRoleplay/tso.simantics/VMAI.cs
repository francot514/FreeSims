using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tso.world.Model;
using TSO.SimsAntics.Engine;
using TSO.Files.formats.iff.chunks;

    namespace TSO.SimsAntics
    {
        public class VMFreeWill
        {
            public VM VM;
            public List<VMEntity> Objects;
            public List<VMEntity> Entities;
            private VMEntity Target;
            private TTAB TreeTableSelected;
            private List<string> interactionList;



            public VMFreeWill(VM vm)
            {

                VM = vm;
                Entities = VM.Entities;
                Objects = new List<VMEntity>();

            }

            public void CheckForUsableObjects()
            {

                if (Entities.Count > 0)
                    foreach (var ent in Entities)
                    {
                        if (ent.Position != LotTilePos.OUT_OF_WORLD && ent.BadName == false
                            && ent.Object.GUID != VMAvatar.NPC_MAID && ent.Object.GUID != VMAvatar.NPC_GARDENER)
                            Objects.Add(ent);

                        
                    }
            }



            private void SetSelected(VMEntity entity, bool isPet)
            {
                interactionList = new List<string>();
                interactionList.Clear();
                if (entity.TreeTable != null)
                {
                    TreeTableSelected = entity.TreeTable;

                    if (isPet)
                    foreach (var interaction in entity.TreeTable.Interactions)
                    {

                       if (interaction.Flags == TTABFlags.AllowDogs)
                         interactionList.Add(entity.TreeTableStrings.GetString((int)interaction.TTAIndex));
                    }

                    else
                    foreach (var interaction in entity.TreeTable.Interactions)
                    {

                        if (!interaction.Debug)
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
                    TSO.Content.GameIffResource CodeOwner;

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
                        bhav = target.SemiGlobal.Resource.Get<BHAV>(ActionID);
                        CodeOwner = target.SemiGlobal.Resource;
                    }

                   

                    if (bhav != null)
                    {
                        avatar.Thread.EnqueueAction(new VMQueuedAction()
                        {
                            Routine = VM.Assemble(bhav),
                            Callee = target,
                            StackObject = target,
                            CodeOwner = target.Object,
                            InteractionNumber = (int)interaction.TTAIndex, //interactions are referenced by their tta index
                            Priority = (short)VMQueuePriority.UserDriven
                        });
                    }

                }

            }


            public void RunAction(VMEntity entity)
            {
                bool PetAction = entity.Object.GUID == VMAvatar.DOG_TEMPLATE ? true: false;

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

            public void Tick()
            {

                Objects.Clear();

                CheckForUsableObjects();

                for (int i = 0; i <= Entities.Count - 1; i++)

                    if (Entities[i].Object.GUID == VMAvatar.TEMPLATE_PERSON && Entities[i].Thread.Queue.Count > 0)
                    {

                        if (Entities[i].Thread.Queue.Count <= 2 && Entities[i].Thread.Queue[0].Priority == (short)VMQueuePriority.Idle)
                            RunAction(Entities[i]);
                    }

                    else if (Entities[i].Object.GUID == VMAvatar.DOG_TEMPLATE && Entities[i].Thread.Queue.Count > 0)
                    {
                        if (Entities[i].Thread.Queue.Count <= 2 && Entities[i].Thread.Queue[0].Priority == (short)VMQueuePriority.Idle)
                            RunAction(Entities[i]);

                    }

            }

        }
    }

