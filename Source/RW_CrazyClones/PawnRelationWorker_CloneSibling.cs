using RimWorld.Planet;
using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_CrazyClones
{
    public class PawnRelationWorker_CloneSibling : PawnRelationWorker
    {
        public override bool InRelation(Pawn me, Pawn other)
        {
            if (me == other) return false;
            if (me.GetCloneParent() == null) return false;
            if (me.GetCloneParent() == other.GetCloneParent()) return true;
            return false;
        }

        public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            return 0f;
            float num = 1f;
            float num2 = 1f;
            if (other.GetCloneParent() != null)
            {
                num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other.GetCloneParent(), other.GetCloneParent(), new PawnGenerationRequest?(request), null, null);
            }
            else if (request.FixedMelanin.HasValue)
            {
                num2 = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
            }
            else
            {
                num2 = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
            }
            float num3 = Mathf.Abs(generated.ageTracker.AgeChronologicalYearsFloat - other.ageTracker.AgeChronologicalYearsFloat);
            float num4 = 1f;
            if (num3 > 40f)
            {
                num4 = 0.2f;
            }
            else if (num3 > 10f)
            {
                num4 = 0.65f;
            }
            return num * num2 * num4 * base.BaseGenerationChanceFactor(generated, other, request);
        }

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            Pawn cloneParent = generated.GetCloneParent();

            if (cloneParent != null && cloneParent == other.GetCloneParent())
            {
                generated.relations.AddDirectRelation(ClonePawnRelationDefOf.CloneSibling, other);
            }
        }

    }
}
