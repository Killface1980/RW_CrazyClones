using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RW_CrazyClones
{
    static class CloneParentRelationUtility
    {
        // RimWorld.ParentRelationUtility
        public static Pawn GetCloneParent(this Pawn pawn)
        {
            if (!pawn.RaceProps.IsFlesh)
            {
                return null;
            }
            return pawn.relations.GetFirstDirectRelationPawn(ClonePawnRelationDefOf.CloneParent);
        }

        // RimWorld.ParentRelationUtility
        public static void SetCloneParent(this Pawn pawn, Pawn newFather)
        {

            Pawn parent = pawn.GetCloneParent();
            if (parent != newFather)
            {
                if (parent != null)
                {
                    pawn.relations.RemoveDirectRelation(ClonePawnRelationDefOf.CloneParent, parent);
                }
                if (newFather != null)
                {
                    pawn.relations.AddDirectRelation(ClonePawnRelationDefOf.CloneParent, newFather);
                }
            }
        }
    }
}
