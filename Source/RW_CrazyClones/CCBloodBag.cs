using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_CrazyClones
{
    public class CCBloodBag : ThingWithComps
    {
        public Pawn donorPawn;
        public PawnKindDef pawnKind;
        public float melanin;
        public CrownType crownType;

        public override string Label
        {
            get { return base.Label + " " + donorPawn.Name; }
        }

        public long AgeBiologicalTicks;

        public long AgeChronologicalTicks;

        public Name Name;
        public Color hairColor;
        public TraitSet traits;
        public Backstory childhood;
        public Backstory adulthood;
        public HairDef hairDef;
        public Pawn_SkillTracker skills;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference(ref donorPawn, "donorPawn");
            Scribe_Values.LookValue(ref melanin, "melanin");
            Scribe_Values.LookValue(ref crownType, "crownType");
            Scribe_Values.LookValue(ref Name, "Name");
            Scribe_Values.LookValue(ref hairColor, "hairColor");
            Scribe_Values.LookValue(ref traits, "traits");
            Scribe_Values.LookValue(ref childhood, "childhood");
            Scribe_Values.LookValue(ref adulthood, "adulthood");
            Scribe_Defs.LookDef(ref hairDef, "hairDef");
            Scribe_Values.LookValue(ref skills, "skills");
            Scribe_Values.LookValue(ref AgeChronologicalTicks, "AgeChronologicalTicks");
            Scribe_Values.LookValue(ref AgeBiologicalTicks, "AgeBiologicalTicks");
        }
    }
}
