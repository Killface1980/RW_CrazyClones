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
        Pawn pawn;

        public void Test()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(pawn.kindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, null, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
            Pawn generatePawn = PawnCloneGenerator.GenerateClonePawn(request, pawn);

            GenSpawn.Spawn(generatePawn, pawn.Position.RandomAdjacentCell8Way(), pawn.Map);
            string text = "WandererJoin".Translate(new object[]
            {
                generatePawn.kindDef.label,
                generatePawn.story.Title.ToLower()
            });
        }
    }
}
