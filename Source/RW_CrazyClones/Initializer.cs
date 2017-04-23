using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RW_CrazyClones;
using Verse;

namespace RW_CrazyClones
{
    class Initializer
    {

        public static void Test(CCBloodBag bag)
        {

            PawnGenerationRequest request = new PawnGenerationRequest(bag.donorPawn.kindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, null, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
            Pawn generatePawn = PawnCloneGenerator.GenerateClonePawn(request, bag);

            GenSpawn.Spawn(generatePawn, bag.Position.RandomAdjacentCell8Way(), bag.Map);
            string text = "WandererJoin".Translate(new object[]
            {
                generatePawn.kindDef.label,
                generatePawn.story.Title.ToLower()
            });
        }
    }
}
