using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;

namespace RW_CrazyClones
{
    public class Recipe_TakeBloodSample : Recipe_InstallImplant
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer != null)
            {

                // removed surgeryfail check to make 100% sucess

                //				if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part))
                //				{
                //					
                //					return;
                //				}
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
            }
            pawn.health.AddHediff(this.recipe.addsHediff, part, null);
            pawn.health.DropBloodFilth();
        }
    }

}
