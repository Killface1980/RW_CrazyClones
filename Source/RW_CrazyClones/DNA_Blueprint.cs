using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_CrazyClones
{
    public class DNA_Blueprint : ThingWithComps
    {
        public Pawn donorPawn;
        public PawnKindDef kindDef;
        public float melanin;
        public CrownType crownType;

        public override string Label
        {
            get { return base.Label + " " + nameInt; }
        }

        public long AgeBiologicalTicks;

        public long AgeChronologicalTicks;

        public Name nameInt;
        public Color hairColor;
        public TraitSet traits;
        public Backstory childhood;
        public Backstory adulthood;
        public HairDef hairDef;
        public List<SkillRecord> skills = new List<SkillRecord>();
#if FS
        //FS
        public BeardDef BeardDef;
        public EyeDef EyeDef;
        public BrowDef BrowDef;
        public MouthDef MouthDef;
        public WrinkleDef WrinkleDef;
        public string headGraphicIndex;
        public string type;
#endif
        public string SkinColorHex;
        public Color HairColorOrg;

        public bool optimized;
        public bool drawMouth;

        public int amountToClone;
        public Gender gender; // NIU

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.LookDef<PawnKindDef>(ref kindDef, "kindDef");
            Scribe_Values.LookValue<Gender>(ref gender, "gender", Gender.Male, false);
            Scribe_Deep.LookDeep<Name>(ref nameInt, "name", new object[0]);

            Scribe_Collections.LookList<SkillRecord>(ref skills, "skills", LookMode.Deep, new object[0]);

            string text = (this.childhood == null) ? null : this.childhood.identifier;
            Scribe_Values.LookValue<string>(ref text, "childhood", null, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars && !text.NullOrEmpty() && !BackstoryDatabase.TryGetWithIdentifier(text, out this.childhood))
            {
                Log.Error("Couldn't load child backstory with identifier " + text + ". Giving random.");
                this.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
            }
            string text2 = (this.adulthood == null) ? null : this.adulthood.identifier;
            Scribe_Values.LookValue<string>(ref text2, "adulthood", null, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars && !text2.NullOrEmpty() && !BackstoryDatabase.TryGetWithIdentifier(text2, out this.adulthood))
            {
                Log.Error("Couldn't load adult backstory with identifier " + text2 + ". Giving random.");
                this.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
            }

            Scribe_Values.LookValue<CrownType>(ref this.crownType, "crownType", CrownType.Undefined, false);
            Scribe_Defs.LookDef<HairDef>(ref this.hairDef, "hairDef");
            Scribe_Values.LookValue<Color>(ref this.hairColor, "hairColor", default(Color), false);
            Scribe_Values.LookValue<float>(ref this.melanin, "melanin", 0f, false);
            Scribe_Deep.LookDeep<TraitSet>(ref this.traits, "traits", new object[]
            {
        this.donorPawn
            });
            Scribe_References.LookReference<Pawn>(ref donorPawn, "donorPawn", true);
            Scribe_Values.LookValue(ref AgeChronologicalTicks, "AgeChronologicalTicks");
            Scribe_Values.LookValue(ref AgeBiologicalTicks, "AgeBiologicalTicks");


#if FS
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
#endif
        }
    }
}
