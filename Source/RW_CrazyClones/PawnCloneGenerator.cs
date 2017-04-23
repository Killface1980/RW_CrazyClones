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

        public static Pawn GenerateClonePawn(PawnGenerationRequest request, Pawn donorPawn = null)
        {
            request.EnsureNonNullFaction();
            Pawn pawn = null;

            if (pawn == null)
            {
                pawn = GenerateNewNakedClonePawn(donorPawn, ref request);
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

        private static Pawn GenerateNewNakedClonePawn(Pawn donorPawn, ref PawnGenerationRequest request)
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
                pawn = DoGenerateNewNakedClonePawn(donorPawn, ref pawnGenerationRequest, out text, ignoreScenarioRequirements);
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
                    "Pawn generation error: ",
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

        private static Pawn DoGenerateNewNakedClonePawn(Pawn donorPawn, ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements)
        {
            error = null;
            Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race, null);
            pawnsBeingGenerated.Add(new PawnGenerationStatus(pawn, null));
            Pawn result;
            try
            {
                pawn.kindDef = request.KindDef;
                pawn.SetFactionDirect(request.Faction);
                PawnComponentsUtility.CreateInitialComponents(pawn);

                // Gender
                pawn.gender = donorPawn.gender;
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
                GenerateRandomCloneAge(pawn, request, donorPawn);
                pawn.needs.SetInitialLevels();
                if (!request.Newborn && request.CanGeneratePawnRelations)
                {
                    GenerateClonePawnRelations(donorPawn, pawn, ref request);
                }
                if (pawn.RaceProps.Humanlike)
                {
                    pawn.story.melanin = donorPawn.story.melanin;// ((!request.FixedMelanin.HasValue) ? PawnSkinColors.RandomMelanin() : request.FixedMelanin.Value);
                    pawn.story.crownType = donorPawn.story.crownType;//((Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average);
                    pawn.story.hairColor = donorPawn.story.hairColor; PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
                    //to do: Bio
                    GiveAppropriateBioAndNameTo(donorPawn, pawn, request.FixedLastName);
                    pawn.story.childhood = donorPawn.story.childhood;
                    pawn.story.childhood = donorPawn.story.adulthood;
                    pawn.story.hairDef = donorPawn.story.hairDef;// PawnHairChooser.RandomHairDefFor(clonePawn, request.Faction.def);
                    GenerateTraits(donorPawn, pawn, request.AllowGay);
                    GenerateBodyType(donorPawn, pawn);
                    GenerateSkills(donorPawn, pawn);
                }
                //To Do: Rewrite
                GenerateInitialHediffs(pawn, request);
                if (pawn.workSettings != null && request.Faction.IsPlayer)
                {
                    pawn.workSettings.EnableAndInitialize();
                }
                if (request.Faction != null && pawn.RaceProps.Animal)
                {
                    pawn.GenerateNecessaryName();
                }
                if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
                {
                    DiscardGeneratedPawn(pawn);
                    error = "Generated dead clonePawn.";
                    result = null;
                }
                else if (!request.AllowDowned && pawn.Downed)
                {
                    DiscardGeneratedPawn(pawn);
                    error = "Generated downed clonePawn.";
                    result = null;
                }
                else if (request.MustBeCapableOfViolence && pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    DiscardGeneratedPawn(pawn);
                    error = "Generated clonePawn incapable of violence.";
                    result = null;
                }
                else if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && !Find.Scenario.AllowPlayerStartingPawn(pawn))
                {
                    DiscardGeneratedPawn(pawn);
                    error = "Generated clonePawn doesn't meet scenario requirements.";
                    result = null;
                }
                else if (request.Validator != null && !request.Validator(pawn))
                {
                    DiscardGeneratedPawn(pawn);
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
                        pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(pawn);
                    }
                    result = pawn;
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

        private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
        {
            ProfilerThreadCheck.BeginSample("GenerateGearFor");
            ProfilerThreadCheck.BeginSample("GenerateStartingApparelFor");
            PawnApparelGenerator.GenerateStartingApparelFor(pawn, request);
            ProfilerThreadCheck.EndSample();
            ProfilerThreadCheck.BeginSample("TryGenerateWeaponFor");
            PawnWeaponGenerator.TryGenerateWeaponFor(pawn);
            ProfilerThreadCheck.EndSample();
            ProfilerThreadCheck.BeginSample("GenerateInventoryFor");
            PawnInventoryGenerator.GenerateInventoryFor(pawn, request);
            ProfilerThreadCheck.EndSample();
            ProfilerThreadCheck.EndSample();
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

        private static void GenerateRandomCloneAge(Pawn pawn, PawnGenerationRequest request, Pawn donorPawn)
        {
            pawn.ageTracker.AgeBiologicalTicks = donorPawn.ageTracker.AgeBiologicalTicks +
                                                 (long)Rand.Range(-donorPawn.ageTracker.AgeBiologicalTicks / 3,
                                                     donorPawn.ageTracker.AgeBiologicalTicks / 3);
            return;

            if (request.FixedBiologicalAge.HasValue && request.FixedChronologicalAge.HasValue)
            {
                float? fixedBiologicalAge = request.FixedBiologicalAge;
                bool arg_67_0;
                if (fixedBiologicalAge.HasValue)
                {
                    float? fixedChronologicalAge = request.FixedChronologicalAge;
                    if (fixedChronologicalAge.HasValue)
                    {
                        arg_67_0 = (fixedBiologicalAge.Value > fixedChronologicalAge.Value);
                        goto IL_67;
                    }
                }
                arg_67_0 = false;
                IL_67:
                if (arg_67_0)
                {
                    Log.Warning(string.Concat(new object[]
                    {
                        "Tried to generate age for clonePawn ",
                        pawn,
                        ", but clonePawn generation request demands biological age (",
                        request.FixedBiologicalAge,
                        ") to be greater than chronological age (",
                        request.FixedChronologicalAge,
                        ")."
                    }));
                }
            }
            if (request.Newborn)
            {
                pawn.ageTracker.AgeBiologicalTicks = 0L;
            }
            else if (request.FixedBiologicalAge.HasValue)
            {
                pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
            }
            else
            {
                int num = 0;
                float num2;
                while (true)
                {
                    if (pawn.RaceProps.ageGenerationCurve != null)
                    {
                        num2 = (float)Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve, 200));
                    }
                    else if (pawn.RaceProps.IsMechanoid)
                    {
                        num2 = (float)Rand.Range(0, 2500);
                    }
                    else
                    {
                        num2 = Rand.ByCurve(DefaultAgeGenerationCurve, 200) * pawn.RaceProps.lifeExpectancy;
                    }
                    num++;
                    if (num > 300)
                    {
                        break;
                    }
                    if (num2 <= (float)pawn.kindDef.maxGenerationAge && num2 >= (float)pawn.kindDef.minGenerationAge)
                    {
                        goto IL_1DB;
                    }
                }
                Log.Error("Tried 300 times to generate age for " + pawn);
                IL_1DB:
                pawn.ageTracker.AgeBiologicalTicks = (long)(num2 * 3600000f) + (long)Rand.Range(0, 3600000);
            }
            if (request.Newborn)
            {
                pawn.ageTracker.AgeChronologicalTicks = 0L;
            }
            else if (request.FixedChronologicalAge.HasValue)
            {
                pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
            }
            else
            {
                int num3;
                if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
                {
                    float value = Rand.Value;
                    if (value < 0.7f)
                    {
                        num3 = Rand.Range(0, 100);
                    }
                    else if (value < 0.95f)
                    {
                        num3 = Rand.Range(100, 1000);
                    }
                    else
                    {
                        int max = GenDate.Year((long)GenTicks.TicksAbs, 0f) - 2026 - pawn.ageTracker.AgeBiologicalYears;
                        num3 = Rand.Range(1000, max);
                    }
                }
                else
                {
                    num3 = 0;
                }
                int ticksAbs = GenTicks.TicksAbs;
                long num4 = (long)ticksAbs - pawn.ageTracker.AgeBiologicalTicks;
                num4 -= (long)num3 * 3600000L;
                pawn.ageTracker.BirthAbsTicks = num4;
            }
            if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
            {
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }
        }

        public static int RandomTraitDegree(TraitDef traitDef)
        {
            if (traitDef.degreeDatas.Count == 1)
            {
                return traitDef.degreeDatas[0].degree;
            }
            return traitDef.degreeDatas.RandomElementByWeight((TraitDegreeData dd) => dd.Commonality).degree;
        }

        private static void GenerateTraits(Pawn donorPawn, Pawn pawn, bool allowGay)
        {
            if (pawn.story == null)
            {
                return;
            }
            pawn.story.traits = donorPawn.story.traits;
            return;

            if (pawn.story.childhood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
                for (int i = 0; i < forcedTraits.Count; i++)
                {
                    TraitEntry traitEntry = forcedTraits[i];
                    if (traitEntry.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.childhood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree, false));
                    }
                }
            }
            if (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
                for (int j = 0; j < forcedTraits2.Count; j++)
                {
                    TraitEntry traitEntry2 = forcedTraits2[j];
                    if (traitEntry2.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.adulthood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry2.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree, false));
                    }
                }
            }
            int num = Rand.RangeInclusive(2, 3);
            if (allowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
            {
                Trait trait = new Trait(TraitDefOf.Gay, RandomTraitDegree(TraitDefOf.Gay), false);
                pawn.story.traits.GainTrait(trait);
            }
            while (pawn.story.traits.allTraits.Count < num)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn));
                if (!pawn.story.traits.HasTrait(newTraitDef))
                {
                    if (newTraitDef == TraitDefOf.Gay)
                    {
                        if (!allowGay)
                        {
                            continue;
                        }
                        if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                        {
                            continue;
                        }
                    }
                    if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !Enumerable.Any(newTraitDef.conflictingTraits, (TraitDef tr) => pawn.story.traits.HasTrait(tr))))
                    {
                        if (newTraitDef.requiredWorkTypes == null || !pawn.story.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
                        {
                            if (!pawn.story.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
                            {
                                int degree = RandomTraitDegree(newTraitDef);
                                if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood == null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
                                {
                                    Trait trait2 = new Trait(newTraitDef, degree, false);
                                    if (pawn.mindState == null || pawn.mindState.mentalBreaker == null || pawn.mindState.mentalBreaker.BreakThresholdExtreme + trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold) <= 40f)
                                    {
                                        pawn.story.traits.GainTrait(trait2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void GenerateBodyType(Pawn donorPawn, Pawn pawn)
        {
            pawn.story.bodyType = donorPawn.story.bodyType;
            return;

            if (pawn.story.adulthood != null)
            {
                pawn.story.bodyType = pawn.story.adulthood.BodyTypeFor(pawn.gender);
            }
            else if (Rand.Value < 0.5f)
            {
                pawn.story.bodyType = BodyType.Thin;
            }
            else
            {
                pawn.story.bodyType = ((pawn.gender != Gender.Female) ? BodyType.Male : BodyType.Female);
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
            clonePawn.relations.AddDirectRelation(DefDatabase<PawnRelationDef>.GetNamed("CloneParent"), donorPawn);
            donorPawn.relations.AddDirectRelation(DefDatabase<PawnRelationDef>.GetNamed("CloneChild"), clonePawn);
            //foreach (Pawn current in enumerable)
            //{
            //    if (current.Discarded)
            //    {
            //        Log.Warning(string.Concat(new object[]
            //        {
            //            "Warning during generating clonePawn relations for ",
            //            clonePawn,
            //            ": Pawn ",
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
            //                list.Add(new KeyValuePair<Pawn, PawnRelationDef>(current, allDefsListForReading[i]));
            //            }
            //        }
            //    }
            //}
            //PawnGenerationRequest localReq = request;
            //KeyValuePair<Pawn, PawnRelationDef> keyValuePair = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
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
            //KeyValuePair<Pawn, PawnRelationDef> keyValuePair2 = list.RandomElementByWeightWithDefault(delegate (KeyValuePair<Pawn, PawnRelationDef> x)
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
          //if ((Rand.Value < 0.25f || pawn.kindDef.factionLeader) && TryGiveSolidBioTo(pawn, requiredLastName))
          //{
          //    return;
          //}
            GiveShuffledBioTo(donorPawn, pawn, pawn.Faction.def, requiredLastName);
        }

        // RimWorld.PawnBioAndNameGenerator
        private static void GiveShuffledBioTo(Pawn donorPawn, Pawn clonePawn, FactionDef factionType, string requiredLastName)
        {
            // To do: proper clonePawn names
            clonePawn.Name = donorPawn.Name;// PawnBioAndNameGenerator.GeneratePawnName(clonePawn, NameStyle.Full, requiredLastName);
            clonePawn.story.childhood = donorPawn.story.childhood;
            clonePawn.story.adulthood = donorPawn.story.adulthood;
            return;
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
