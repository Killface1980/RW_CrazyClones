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
                pawn.health.AddHediff(recipe.addsHediff, part, null);
                pawn.health.DropBloodFilth();
                CCBloodBag dnaSample = ThingMaker.MakeThing(ThingDef.Named("CCBloodBag")) as CCBloodBag;
                if (dnaSample != null)
                {
                    dnaSample.donorPawn = pawn;
                    dnaSample.pawnKind = pawn.kindDef;
                    dnaSample.melanin = pawn.story.melanin;
                    dnaSample.crownType = pawn.story.crownType;
                    dnaSample.Name = pawn.Name;
                    dnaSample.hairColor = pawn.story.hairColor;
                    dnaSample.traits = pawn.story.traits;
                    dnaSample.childhood = pawn.story.childhood;
                    dnaSample.adulthood = pawn.story.adulthood;
                    dnaSample.hairDef = pawn.story.hairDef;
                    dnaSample.skills = pawn.skills;
                    dnaSample.AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
                    dnaSample.AgeChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
                    GenSpawn.Spawn(dnaSample, billDoer.Position, pawn.Map);
                    Initializer.Test(dnaSample);
                }
            }
        }
    }

}
