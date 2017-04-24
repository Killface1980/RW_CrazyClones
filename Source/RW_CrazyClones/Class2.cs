using System;
using System.Collections.Generic;
using FaceStyling;
using Verse;
using Verse.AI;

namespace RW_CrazyClones
{
    class CloneLab : Building
    {

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

        }

        public void Cloning(Pawn pawn)
        {
            Find.WindowStack.Add(new Dialog_Cloning(pawn));
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            {
                if (!myPawn.CanReserve(this))
                {

                    FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                    return new List<FloatMenuOption>
                {
                    item
                };
                }
                if (!myPawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    return new List<FloatMenuOption>
                {
                    item2
                };

                }

                Action action2 = delegate
                {
                    // IntVec3 InteractionSquare = (this.Position + new IntVec3(0, 0, 1)).RotatedBy(this.Rotation);
                    Job FaceStyleChanger = new Job(DefDatabase<JobDef>.GetNamed("ClonePawnAtCloneLab"), this, InteractionCell);
                    if (myPawn.jobs.CanTakeOrderedJob())//This is used to force go job, it will work even when drafted
                    {
                        myPawn.jobs.TryTakeOrderedJob(FaceStyleChanger);
                    }
                    else
                    {
                        myPawn.QueueJob(FaceStyleChanger);
                        myPawn.jobs.StopAll();
                    }

                };

                list.Add(new FloatMenuOption("ClonePawnAtCloneLab".Translate(), action2));
            }
            return list;
        }

    }
}
