using System;
using RimWorld;
using Verse;

namespace RW_CrazyClones
{
    public class PawnRelationWorker_CloneChild : PawnRelationWorker
    {
        public override bool InRelation(Pawn me, Pawn other)
        {
            return me != other && (other.GetCloneParent() == me);
        }
        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {

            other.SetCloneParent(generated);
            ResolveMyName(ref request, other, other.GetCloneParent());
            //ResolveMySkinColor(ref request, other, other.GetCloneFather());
        }

        private static void ResolveMyName(ref PawnGenerationRequest request, Pawn child, Pawn otherParent)
        {
            if (request.FixedLastName != null)
            {
                return;
            }
            if (ChildRelationUtility.DefinitelyHasNotBirthName(child))
            {
                return;
            }
            if (ChildRelationUtility.ChildWantsNameOfAnyParent(child))
            {
                if (otherParent == null)
                {
                    float num = 0.9f;
                    if (Rand.Value < num)
                    {
                        request.SetFixedLastName(((NameTriple)child.Name).Last);
                    }
                }
                else
                {
                    string last = ((NameTriple)child.Name).Last;
                    string last2 = ((NameTriple)otherParent.Name).Last;
                    if (last != last2)
                    {
                        request.SetFixedLastName(last);
                    }
                }
            }
        }

    }
}
