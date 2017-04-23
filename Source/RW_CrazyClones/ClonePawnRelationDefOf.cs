using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RW_CrazyClones
{
   static class ClonePawnRelationDefOf
   {
       public static PawnRelationDef CloneParent = DefDatabase<PawnRelationDef>.GetNamed("CloneParent");
       public static PawnRelationDef CloneChild = DefDatabase<PawnRelationDef>.GetNamed("CloneChild");
        public static PawnRelationDef CloneSibling = DefDatabase<PawnRelationDef>.GetNamed("CloneSibling");
   }
}
