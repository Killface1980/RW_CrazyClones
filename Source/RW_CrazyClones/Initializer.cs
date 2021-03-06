﻿using System;
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

        public static void Test(DNA_Blueprint dnaBlueprint)
        {

            PawnGenerationRequest request = new PawnGenerationRequest(dnaBlueprint.kindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, null, null, null, null, null, null);
            Pawn clonePawn = PawnCloneGenerator.GenerateClonePawn(request, dnaBlueprint);

            GenSpawn.Spawn(clonePawn, dnaBlueprint.Position.RandomAdjacentCell8Way(), dnaBlueprint.Map);

            string text = "WandererJoin".Translate(new object[]
            {
                clonePawn.kindDef.label,
                clonePawn.story.Title.ToLower()
            });
            text = text.AdjustedFor(clonePawn);
            string label = "LetterLabelWandererJoin".Translate();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, clonePawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Good, clonePawn, null);
        }
    }
}
