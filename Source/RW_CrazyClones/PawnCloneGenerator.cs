using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace RW_CrazyClones
{
    public static class PawnCloneGenerator
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct PawnGenerationStatus
        {
            public Pawn Pawn
            {
                get;
                private set;
            }

            public List<Pawn> PawnsGeneratedInTheMeantime
            {
                get;
                private set;
            }

            public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime)
            {
                Pawn = pawn;
                PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
            }
        }

        public const int MaxStartMentalStateThreshold = 40;

        private static List<PawnGenerationStatus> pawnsBeingGenerated = new List<PawnGenerationStatus>();

        private static SimpleCurve DefaultAgeGenerationCurve = new SimpleCurve
        {
            new CurvePoint(0.05f, 0f),
            new CurvePoint(0.1f, 100f),
            new CurvePoint(0.675f, 100f),
            new CurvePoint(0.75f, 30f),
            new CurvePoint(0.875f, 18f),
            new CurvePoint(1f, 10f),
            new CurvePoint(1.125f, 3f),
            new CurvePoint(1.25f, 0f)
        };

        private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(10f, 0.7f),
            new CurvePoint(35f, 1f),
            new CurvePoint(60f, 1.6f)
        };

        private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(10f, 10f),
            new CurvePoint(20f, 16f),
            new CurvePoint(27f, 20f)
        };

        private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.5f, 150f),
            new CurvePoint(4f, 150f),
            new CurvePoint(5f, 25f),
            new CurvePoint(10f, 5f),
            new CurvePoint(15f, 0f)
        };

        public static Pawn GenerateClonePawn(PawnGenerationRequest request, CCBloodBag pawnDna)
        {
            request.EnsureNonNullFaction();
            Pawn pawn = null;

            if (pawn == null)
            {
                pawn = GenerateNewNakedClonePawn(pawnDna, ref request);
                if (pawn == null)
                {
                    return null;
                }

            }
            if (Find.Scenario != null)
            {
                Find.Scenario.Notify_PawnGenerated(pawn, request.Context);
            }
            return pawn;
        }

        private static Pawn GenerateNewNakedClonePawn(CCBloodBag pawnDna, ref PawnGenerationRequest request)
        {
            Pawn pawn = null;
            string text = null;
            bool ignoreScenarioRequirements = false;
            for (int i = 0; i < 100; i++)
            {
                if (i == 70)
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Could not generate a clonePawn after ",
                        70,
                        " tries. Last error: ",
                        text,
                        " Ignoring scenario requirements."
                    }));
                    ignoreScenarioRequirements = true;
                }
                PawnGenerationRequest pawnGenerationRequest = request;
                pawn = DoGenerateNewNakedClonePawn(pawnDna, ref pawnGenerationRequest, out text, ignoreScenarioRequirements);
                if (pawn != null)
                {
                    request = pawnGenerationRequest;
                    break;
                }
            }
            if (pawn == null)
            {
                Log.Error(string.Concat(new object[]
                {
                    "pawnDna generation error: ",
                    text,
                    " Too many tries (",
                    100,
                    "), returning null. Generation request: ",
                    request
                }));
                return null;
            }
            return pawn;
        }

        private static Pawn DoGenerateNewNakedClonePawn(CCBloodBag donorDNA, ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements)
        {
            error = null;
            Pawn clonePawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race, null);
            pawnsBeingGenerated.Add(new PawnGenerationStatus(clonePawn, null));
            Pawn result;
            try
            {
                clonePawn.kindDef = request.KindDef;
                clonePawn.SetFactionDirect(request.Faction);
                PawnComponentsUtility.CreateInitialComponents(clonePawn);

                // Gender
                clonePawn.gender = donorDNA.donorPawn.gender;
                //if (request.FixedGender.HasValue)
                //{
                //    clonePawn.gender = request.FixedGender.Value;
                //}
                //else if (clonePawn.RaceProps.hasGenders)
                //{
                //    if (Rand.Value < 0.5f)
                //    {
                //        clonePawn.gender = Gender.Male;
                //    }
                //    else
                //    {
                //        clonePawn.gender = Gender.Female;
                //    }
                //}
                //else
                //{
                //    clonePawn.gender = Gender.None;
                //}

                // Age
                GenerateExactCloneAge(clonePawn, donorDNA);
                clonePawn.needs.SetInitialLevels();
                if (!request.Newborn && request.CanGeneratePawnRelations)
                {
                    GenerateClonePawnRelations(donorDNA.donorPawn, clonePawn, ref request);
                }
                if (clonePawn.RaceProps.Humanlike)
                {
                    clonePawn.story.melanin = donorDNA.melanin;// ((!request.FixedMelanin.HasValue) ? PawnSkinColors.RandomMelanin() : request.FixedMelanin.Value);
                    clonePawn.story.crownType = donorDNA.crownType;//((Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average);
                    clonePawn.story.hairColor = donorDNA.hairColor;
                  //  PawnHairColors.RandomHairColor(clonePawn.story.SkinColor, clonePawn.ageTracker.AgeBiologicalYears);
                    //to do: Bio
                    //GiveAppropriateBioAndNameTo(pawnDna, clonePawn, request.FixedLastName);
                    clonePawn.Name = donorDNA.Name;
                    clonePawn.story.childhood = donorDNA.childhood;
                    clonePawn.story.adulthood = donorDNA.adulthood;
                    clonePawn.story.hairDef = donorDNA.hairDef;// PawnHairChooser.RandomHairDefFor(clonePawn, request.Faction.def);
                    clonePawn.story.traits = donorDNA.traits;
                    PassTraitsFromDonor(donorDNA, clonePawn);
                    GenerateBodyType(donorDNA, clonePawn);
                    clonePawn.skills = donorDNA.skills;
                //    GenerateSkills(donorDNA, clonePawn);
                }
                //To Do: Rewrite
            
                // GenerateInitialHediffs(clonePawn, request);
                if (clonePawn.workSettings != null && request.Faction.IsPlayer)
                {
                    clonePawn.workSettings.EnableAndInitialize();
                }
                if (request.Faction != null && clonePawn.RaceProps.Animal)
                {
                    clonePawn.GenerateNecessaryName();
                }
                if (!request.AllowDead && (clonePawn.Dead || clonePawn.Destroyed))
                {
                    DiscardGeneratedPawn(clonePawn);
                    error = "Generated dead clonePawn.";
                    result = null;
                }
                else if (!request.AllowDowned && clonePawn.Downed)
                {
                    DiscardGeneratedPawn(clonePawn);
                    error = "Generated downed clonePawn.";
                    result = null;
                }
                else if (request.MustBeCapableOfViolence && clonePawn.story != null && clonePawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    DiscardGeneratedPawn(clonePawn);
                    error = "Generated clonePawn incapable of violence.";
                    result = null;
                }
                else if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && !Find.Scenario.AllowPlayerStartingPawn(clonePawn))
                {
                    DiscardGeneratedPawn(clonePawn);
                    error = "Generated clonePawn doesn't meet scenario requirements.";
                    result = null;
                }
                else if (request.Validator != null && !request.Validator(clonePawn))
                {
                    DiscardGeneratedPawn(clonePawn);
                    error = "Generated clonePawn didn't pass validator check.";
                    result = null;
                }
                else
                {
                    for (int i = 0; i < pawnsBeingGenerated.Count - 1; i++)
                    {
                        if (pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime == null)
                        {
                            pawnsBeingGenerated[i] = new PawnGenerationStatus(pawnsBeingGenerated[i].Pawn, new List<Pawn>());
                        }
                        pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(clonePawn);
                    }
                    result = clonePawn;
                }
            }
            finally
            {
                pawnsBeingGenerated.RemoveLast<PawnGenerationStatus>();
            }
            return result;
        }

        private static void DiscardGeneratedPawn(Pawn pawn)
        {
            if (Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.RemovePawn(pawn);
            }
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            List<Pawn> pawnsGeneratedInTheMeantime = pawnsBeingGenerated.Last<PawnGenerationStatus>().PawnsGeneratedInTheMeantime;
            if (pawnsGeneratedInTheMeantime != null)
            {
                for (int i = 0; i < pawnsGeneratedInTheMeantime.Count; i++)
                {
                    Pawn pawn2 = pawnsGeneratedInTheMeantime[i];
                    if (Find.WorldPawns.Contains(pawn2))
                    {
                        Find.WorldPawns.RemovePawn(pawn2);
                    }
                    Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
                    for (int j = 0; j < pawnsBeingGenerated.Count; j++)
                    {
                        pawnsBeingGenerated[j].PawnsGeneratedInTheMeantime.Remove(pawn2);
                    }
                }
            }
        }

        private static float ChanceToRedressAnyWorldPawn()
        {
            int pawnsBySituationCount = Find.WorldPawns.GetPawnsBySituationCount(WorldPawnSituation.Free);
            return Mathf.Min(0.02f + 0.01f * ((float)pawnsBySituationCount / 25f), 0.8f);
        }

        private static float WorldPawnSelectionWeight(Pawn p)
        {
            if (p.RaceProps.IsFlesh && !p.relations.everSeenByPlayer && p.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
            {
                return 0.1f;
            }
            return 1f;
        }

        private static void GenerateInitialHediffs(Pawn pawn, PawnGenerationRequest request)
        {
            int num = 0;
            while (true)
            {
                GenerateRandomOldAgeInjuries(pawn, !request.AllowDead);
                PawnTechHediffsGenerator.GeneratePartsAndImplantsFor(pawn);
                PawnAddictionHediffsGenerator.GenerateAddictionsFor(pawn);
                if (request.AllowDead && pawn.Dead)
                {
                    break;
                }
                if (request.AllowDowned || !pawn.Downed)
                {
                    return;
                }
                pawn.health.Reset();
                num++;
                if (num > 80)
                {
                    goto Block_4;
                }
            }
            return;
            Block_4:
            Log.Warning(string.Concat(new object[]
            {
                "Could not generate old age injuries for ",
                pawn.ThingID,
                " of age ",
                pawn.ageTracker.AgeBiologicalYears,
                " that allow clonePawn to move after ",
                80,
                " tries. request=",
                request
            }));
        }

        private static void GenerateExactCloneAge(Pawn clonePawn, CCBloodBag donorDNA)
        {
            clonePawn.ageTracker.AgeBiologicalTicks = donorDNA.AgeBiologicalTicks;//pawnDna.ageTracker.AgeBiologicalTicks;
            clonePawn.ageTracker.AgeChronologicalTicks = 0L;// donorDNA.AgeChronologicalTicks;

        }

        public static int RandomTraitDegree(TraitDef traitDef)
        {
            if (traitDef.degreeDatas.Count == 1)
            {
                return traitDef.degreeDatas[0].degree;
            }
            return traitDef.degreeDatas.RandomElementByWeight((TraitDegreeData dd) => dd.Commonality).degree;
        }

        private static void PassTraitsFromDonor(CCBloodBag donorPawn, Pawn pawn)
        {
            if (donorPawn == null || pawn.story== null)
            {
                return;
            }
            pawn.story.traits = donorPawn.traits;
            return;
        }

        private static void GenerateBodyType(CCBloodBag donorPawn, Pawn clonePawn)
        {
            if (donorPawn.adulthood != null)
            {
                clonePawn.story.bodyType = clonePawn.story.adulthood.BodyTypeFor(clonePawn.gender);
            }
            else if (Rand.Value < 0.5f)
            {
                clonePawn.story.bodyType = BodyType.Thin;
            }
            else
            {
                clonePawn.story.bodyType = ((clonePawn.gender != Gender.Female) ? BodyType.Male : BodyType.Female);
            }
        }

        private static void GenerateSkills(Pawn donorPawn, Pawn pawn)
        {
            pawn.skills = donorPawn.skills;

            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                int num = FinalLevelOfSkill(pawn, skillDef);
                SkillRecord skill = pawn.skills.GetSkill(skillDef);
                skill.Level = num;
                if (!skill.TotallyDisabled)
                {
                    float num2 = (float)num * 0.11f;
                    float value = Rand.Value;
                    if (value < num2)
                    {
                        if (value < num2 * 0.2f)
                        {
                            skill.passion = Passion.Major;
                        }
                        else
                        {
                            skill.passion = Passion.Minor;
                        }
                    }
                    skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
                }
            }
        }

        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num;
            if (sk.usuallyDefinedInBackstories)
            {
                num = (float)Rand.RangeInclusive(0, 4);
            }
            else
            {
                num = Rand.ByCurve(LevelRandomCurve, 100);
            }
            foreach (Backstory current in from bs in pawn.story.AllBackstories
                                          where bs != null
                                          select bs)
            {
                foreach (KeyValuePair<SkillDef, int> current2 in current.skillGainsResolved)
                {
                    if (current2.Key == sk)
                    {
                        num += (float)current2.Value * Rand.Range(1f, 1.4f);
                    }
                }
            }
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                int num2 = 0;
                if (pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out num2))
                {
                    num += (float)num2;
                }
            }
            float num3 = Rand.Range(1f, AgeSkillMaxFactorCurve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears));
            num *= num3;
            num = LevelFinalAdjustmentCurve.Evaluate(num);
            return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
        }

        public static void PostProcessGeneratedGear(Thing gear, Pawn pawn)
        {
            CompQuality compQuality = gear.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                compQuality.SetQuality(QualityUtility.RandomGeneratedGearQuality(pawn.kindDef), ArtGenerationContext.Outsider);
            }
            if (gear.def.useHitPoints)
            {
                float randomInRange = pawn.kindDef.gearHealthRange.RandomInRange;
                if (randomInRange < 1f)
                {
                    int num = Mathf.RoundToInt(randomInRange * (float)gear.MaxHitPoints);
                    num = Mathf.Max(1, num);
                    gear.HitPoints = num;
                }
            }
        }

        private static void GenerateClonePawnRelations(Pawn donorPawn, Pawn clonePawn, ref PawnGenerationRequest request)
        {
            if (!clonePawn.RaceProps.Humanlike)
            {
                return;
            }
            List<KeyValuePair<Pawn, PawnRelationDef>> list = new List<KeyValuePair<Pawn, PawnRelationDef>>();
            List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
            IEnumerable<Pawn> enumerable = from x in PawnsFinder.AllMapsAndWorld_AliveOrDead
                                           where x.def == donorPawn.def
                                           select x;
            clonePawn.relations.AddDirectRelation(ClonePawnRelationDefOf.CloneParent, donorPawn);
            donorPawn.relations.AddDirectRelation(ClonePawnRelationDefOf.CloneChild, clonePawn);
            //foreach (pawnDna current in enumerable)
            //{
            //    if (current.Discarded)
            //    {
            //        Log.Warning(string.Concat(new object[]
            //        {
            //            "Warning during generating clonePawn relations for ",
            //            clonePawn,
            //            ": pawnDna ",
            //            current,
            //            " is discarded, yet he was yielded by PawnUtility. Discarding a clonePawn means that he is no longer managed by anything."
            //        }));
            //    }
            //    else
            //    {
            //        for (int i = 0; i < allDefsListForReading.Count; i++)
            //        {
            //            if (allDefsListForReading[i].generationChanceFactor > 0f)
            //            {
            //                list.Add(new KeyValuePair<pawnDna, PawnRelationDef>(current, allDefsListForReading[i]));
            //            }
            //        }
            //    }
            //}
            //PawnGenerationRequest localReq = request;
            //KeyValuePair<pawnDna, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<pawnDna, PawnRelationDef> x)
            //{
            //    if (!x.Value.familyByBloodRelation)
            //    {
            //        return 0f;
            //    }
            //    return x.Value.generationChanceFactor * x.Value.Worker.GenerationChance(clonePawn, x.Key, localReq);
            //}, 82f);
            //if (keyValuePair.Key != null)
            //{
            //    keyValuePair.Value.Worker.CreateRelation(clonePawn, keyValuePair.Key, ref request);
            //}
            //KeyValuePair<pawnDna, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<pawnDna, PawnRelationDef> x)
            //{
            //    if (x.Value.familyByBloodRelation)
            //    {
            //        return 0f;
            //    }
            //    return x.Value.generationChanceFactor * x.Value.Worker.GenerationChance(clonePawn, x.Key, localReq);
            //}, 82f);
            //if (keyValuePair2.Key != null)
            //{
            //    keyValuePair2.Value.Worker.CreateRelation(clonePawn, keyValuePair2.Key, ref request);
            //}
        }
        // Imported

        public static void GenerateRandomOldAgeInjuries(Pawn pawn, bool tryNotToKillPawn)
        {
            int num = 0;
            for (int i = 10; i < Mathf.Min(pawn.ageTracker.AgeBiologicalYears, 120); i += 10)
            {
                if (Rand.Value < 0.15f)
                {
                    num++;
                }
            }
            for (int j = 0; j < num; j++)
            {
                DamageDef dam = RandomOldInjuryDamageType();
                int num2 = Rand.RangeInclusive(2, 6);
                IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                                                     where x.depth == BodyPartDepth.Outside && !Mathf.Approximately(x.def.oldInjuryBaseChance, 0f) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x)
                                                     select x;
                if (source.Any<BodyPartRecord>())
                {
                    BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.absoluteFleshCoverage);
                    HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dam, pawn, bodyPartRecord);
                    if (bodyPartRecord.def.oldInjuryBaseChance > 0f && hediffDefFromDamage.CompPropsFor(typeof(HediffComp_GetsOld)) != null)
                    {
                        Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
                        hediff_Injury.Severity = (float)num2;
                        hediff_Injury.TryGetComp<HediffComp_GetsOld>().IsOld = true;
                        pawn.health.AddHediff(hediff_Injury, bodyPartRecord, null);
                    }
                }
            }
            for (int k = 1; k < pawn.ageTracker.AgeBiologicalYears; k++)
            {
                foreach (HediffGiver_Birthday current in RandomHediffsToGainOnBirthday(pawn, k))
                {
                    current.TryApplyAndSimulateSeverityChange(pawn, (float)k, tryNotToKillPawn);
                    if (pawn.Dead)
                    {
                        break;
                    }
                }
                if (pawn.Dead)
                {
                    break;
                }
            }
        }
        public static IEnumerable<HediffGiver_Birthday> RandomHediffsToGainOnBirthday(Pawn pawn, int age)
        {
            return RandomHediffsToGainOnBirthday(pawn.def, age);
        }
        private static IEnumerable<HediffGiver_Birthday> RandomHediffsToGainOnBirthday(ThingDef raceDef, int age)
        {
            List<HediffGiverSetDef> sets = raceDef.race.hediffGiverSets;
            if (sets != null)
            {
                for (int i = 0; i < sets.Count; i++)
                {
                    List<HediffGiver> givers = sets[i].hediffGivers;
                    for (int j = 0; j < givers.Count; j++)
                    {
                        HediffGiver_Birthday agb = givers[j] as HediffGiver_Birthday;
                        if (agb != null)
                        {
                            float ageFractionOfLifeExpectancy = (float)age / raceDef.race.lifeExpectancy;
                            if (Rand.Value < agb.ageFractionChanceCurve.Evaluate(ageFractionOfLifeExpectancy))
                            {
                                yield return agb;
                            }
                        }
                    }
                }
            }
        }
        // RimWorld.AgeInjuryUtility
        private static DamageDef RandomOldInjuryDamageType()
        {
            switch (Rand.RangeInclusive(0, 3))
            {
                case 0:
                    return DamageDefOf.Bullet;
                case 1:
                    return DamageDefOf.Scratch;
                case 2:
                    return DamageDefOf.Bite;
                case 3:
                    return DamageDefOf.Stab;
                default:
                    throw new Exception();
            }
        }

        public static void GiveAppropriateBioAndNameTo(Pawn donorPawn, Pawn pawn, string requiredLastName)
        {
          if ((Rand.Value < 0.25f || pawn.kindDef.factionLeader) && TryGiveSolidBioTo(pawn, requiredLastName))
          {
              return;
          }
            GiveShuffledBioTo(donorPawn, pawn, pawn.Faction.def, requiredLastName);
        }

        // RimWorld.PawnBioAndNameGenerator
        private static void GiveShuffledBioTo(Pawn donorPawn, Pawn clonePawn, FactionDef factionType, string requiredLastName)
        {
            // To do: proper clonePawn names
             PawnBioAndNameGenerator.GeneratePawnName(clonePawn, NameStyle.Full, requiredLastName);

            SetBackstoryInSlot(clonePawn, BackstorySlot.Childhood, ref clonePawn.story.childhood, factionType);
            if (clonePawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
            {
                SetBackstoryInSlot(clonePawn, BackstorySlot.Adulthood, ref clonePawn.story.adulthood, factionType);
            }
        }
        // RimWorld.PawnBioAndNameGenerator
        private static void SetBackstoryInSlot(Pawn pawn, BackstorySlot slot, ref Backstory backstory, FactionDef factionType)
        {
            if (!(from kvp in BackstoryDatabase.allBackstories
                  where kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(factionType.backstoryCategory) && kvp.Value.slot == slot && (slot != BackstorySlot.Adulthood || !kvp.Value.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))
                  select kvp.Value).TryRandomElement(out backstory))
            {
                Log.Error(string.Concat(new object[]
                {
            "No shuffled ",
            slot,
            " found for ",
            pawn,
            " of ",
            factionType,
            ". Defaulting."
                }));
                backstory = (from kvp in BackstoryDatabase.allBackstories
                             where kvp.Value.slot == slot
                             select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
            }
        }

        // RimWorld.PawnBioAndNameGenerator
        private static bool TryGiveSolidBioTo(Pawn pawn, string requiredLastName)
        {
            PawnBio pawnBio = TryGetRandomUnusedSolidBioFor(pawn.Faction.def.backstoryCategory, pawn.kindDef, pawn.gender, requiredLastName);
            if (pawnBio == null)
            {
                return false;
            }
            if (pawnBio.name.First == "Tynan" && pawnBio.name.Last == "Sylvester" && Rand.Value < 0.5f)
            {
                pawnBio = TryGetRandomUnusedSolidBioFor(pawn.Faction.def.backstoryCategory, pawn.kindDef, pawn.gender, requiredLastName);
            }
            if (pawnBio == null)
            {
                return false;
            }
            pawn.Name = pawnBio.name;
            pawn.story.childhood = pawnBio.childhood;
            if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
            {
                pawn.story.adulthood = pawnBio.adulthood;
            }
            return true;
        }
        // RimWorld.PawnBioAndNameGenerator
        private static List<PawnBio> tempBios = new List<PawnBio>();

        private static PawnBio TryGetRandomUnusedSolidBioFor(string backstoryCategory, PawnKindDef kind, Gender gender, string requiredLastName)
        {
            NameTriple nameTriple = null;
            if (Rand.Value < 0.5f)
            {
                nameTriple = Prefs.RandomPreferredName();
                if (nameTriple != null && (nameTriple.UsedThisGame || (requiredLastName != null && nameTriple.Last != requiredLastName)))
                {
                    nameTriple = null;
                }
            }
            while (true)
            {
                int i = 0;
                while (i < SolidBioDatabase.allBios.Count)
                {
                    PawnBio pawnBio = SolidBioDatabase.allBios[i];
                    if (pawnBio.gender == GenderPossibility.Either)
                    {
                        goto IL_8F;
                    }
                    if (gender != Gender.Male || pawnBio.gender == GenderPossibility.Male)
                    {
                        if (gender != Gender.Female || pawnBio.gender == GenderPossibility.Female)
                        {
                            goto IL_8F;
                        }
                    }
                    IL_14E:
                    i++;
                    continue;
                    IL_8F:
                    if (!requiredLastName.NullOrEmpty() && pawnBio.name.Last != requiredLastName)
                    {
                        goto IL_14E;
                    }
                    if (pawnBio.name.UsedThisGame)
                    {
                        goto IL_14E;
                    }
                    if (nameTriple != null && !pawnBio.name.Equals(nameTriple))
                    {
                        goto IL_14E;
                    }
                    if (kind.factionLeader && !pawnBio.pirateKing)
                    {
                        goto IL_14E;
                    }
                    for (int j = 0; j < pawnBio.adulthood.spawnCategories.Count; j++)
                    {
                        if (pawnBio.adulthood.spawnCategories[j] == backstoryCategory)
                        {
                            tempBios.Add(pawnBio);
                            break;
                        }
                    }
                    goto IL_14E;
                }
                if (tempBios.Count != 0 || nameTriple == null)
                {
                    break;
                }
                nameTriple = null;
            }
            PawnBio result;
            tempBios.TryRandomElement(out result);
            tempBios.Clear();
            return result;
        }

    }
}
