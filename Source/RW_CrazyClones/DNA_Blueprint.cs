using RimWorld;
using RW_FacialStuff.Defs;
using UnityEngine;
using Verse;

namespace RW_CrazyClones
{
    public class DNA_Blueprint : ThingWithComps
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


        //FS
        public BeardDef BeardDef;
        public EyeDef EyeDef;
        public BrowDef BrowDef;
        public MouthDef MouthDef;
        public WrinkleDef WrinkleDef;
        public string headGraphicIndex;
        public string type;

        public string SkinColorHex;
        public Color HairColorOrg;

        public bool optimized;
        public bool drawMouth;

        public override void ExposeData()
        {
            Scribe_References.LookReference(ref donorPawn, "donorPawn");
            Scribe_Values.LookValue(ref melanin, "melanin");
            Scribe_Values.LookValue(ref crownType, "crownType");
            Scribe_Deep.LookDeep(ref Name, "Name");
            Scribe_Values.LookValue(ref hairColor, "hairColor");
            Scribe_Deep.LookDeep(ref traits, "traits");
            Scribe_Values.LookValue(ref childhood, "childhood");
            Scribe_Values.LookValue(ref adulthood, "adulthood");
            Scribe_Defs.LookDef(ref hairDef, "hairDef");
            Scribe_Deep.LookDeep(ref skills, "skills");
            Scribe_Values.LookValue(ref AgeChronologicalTicks, "AgeChronologicalTicks");
            Scribe_Values.LookValue(ref AgeBiologicalTicks, "AgeBiologicalTicks");


            // FS
            Scribe_Defs.LookDef(ref EyeDef, "EyeDef");
            Scribe_Defs.LookDef(ref BrowDef, "BrowDef");
            Scribe_Defs.LookDef(ref MouthDef, "MouthDef");
            Scribe_Defs.LookDef(ref WrinkleDef, "WrinkleDef");
            Scribe_Defs.LookDef(ref BeardDef, "BeardDef");
            Scribe_Values.LookValue(ref optimized, "optimized");
            Scribe_Values.LookValue(ref drawMouth, "drawMouth");

            Scribe_Values.LookValue(ref headGraphicIndex, "headGraphicIndex");
            Scribe_Values.LookValue(ref type, "type");
            Scribe_Values.LookValue(ref SkinColorHex, "SkinColorHex");
            Scribe_Values.LookValue(ref HairColorOrg, "HairColorOrg");
        }
    }
}
