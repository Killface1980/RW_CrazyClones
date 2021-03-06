﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;

namespace RW_CrazyClones
{
    internal class Recipe_TakeBloodSample : Recipe_InstallImplant
    {

        public override void ApplyOnPawn(Pawn donorPawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
        {
            if (billDoer != null)
            {

                // removed surgeryfail check to make 100% sucess

                //				if (base.CheckSurgeryFail(billDoer, donorPawn, ingredients, part))
                //				{
                //					
                //					return;
                //				}
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    donorPawn
                });
                donorPawn.health.AddHediff(recipe.addsHediff, part, null);
                donorPawn.health.DropBloodFilth();
                DNA_Blueprint dnaBlueprint = ThingMaker.MakeThing(ThingDef.Named("CCBloodBag")) as DNA_Blueprint;
                if (dnaBlueprint != null)
                {
                    dnaBlueprint.donorPawn = donorPawn;
                    dnaBlueprint.nameInt = donorPawn.Name;
                    dnaBlueprint.gender = donorPawn.gender;
                    dnaBlueprint.kindDef = donorPawn.kindDef;
                    dnaBlueprint.AgeBiologicalTicks = donorPawn.ageTracker.AgeBiologicalTicks;
                    dnaBlueprint.AgeChronologicalTicks = donorPawn.ageTracker.AgeChronologicalTicks;

                    //FS
                    if (donorPawn.RaceProps.Humanlike)
                    {
                        dnaBlueprint.hairColor = donorPawn.story.hairColor;
                        dnaBlueprint.melanin = donorPawn.story.melanin;
                        dnaBlueprint.crownType = donorPawn.story.crownType;
                        dnaBlueprint.childhood = donorPawn.story.childhood;
                        dnaBlueprint.adulthood = donorPawn.story.adulthood;
                        dnaBlueprint.traits = donorPawn.story.traits;
                        dnaBlueprint.hairDef = donorPawn.story.hairDef;
                        dnaBlueprint.skills = donorPawn.skills.skills;

#if FS
                        CompFace faceComp = donorPawn.TryGetComp<CompFace>();
                        if (faceComp != null)
                        {
                            dnaBlueprint.BeardDef = faceComp.BeardDef;
                            dnaBlueprint.EyeDef = faceComp.EyeDef;
                            dnaBlueprint.BrowDef = faceComp.BrowDef;
                            dnaBlueprint.MouthDef = faceComp.MouthDef;
                            dnaBlueprint.WrinkleDef = faceComp.WrinkleDef;
                            dnaBlueprint.headGraphicIndex = faceComp.headGraphicIndex;
                            dnaBlueprint.type = faceComp.type;
                            dnaBlueprint.SkinColorHex = faceComp.SkinColorHex;
                            dnaBlueprint.HairColorOrg = faceComp.HairColorOrg;
                            dnaBlueprint.optimized = faceComp.optimized;
                            dnaBlueprint.drawMouth = faceComp.drawMouth;
                        }
#endif
                    }

                    GenSpawn.Spawn(dnaBlueprint, billDoer.Position, billDoer.Map);

                }
            }
        }
    }

}
