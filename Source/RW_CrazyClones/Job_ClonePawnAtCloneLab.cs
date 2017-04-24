﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RW_CrazyClones
{
    // ReSharper disable once UnusedMember.Global
    public class Job_ClonePawnAtCloneLab : JobDriver
    {
        private const TargetIndex ColorChanger = TargetIndex.A;
        private const TargetIndex CellInd = TargetIndex.B;
        private static string ErrorMessage = "Hairstyling job called on building that is not Cabinet";
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_WaitWithSoundAndEffect();
            yield break;
        }
        private Toil Toils_WaitWithSoundAndEffect()
        {
            return new Toil
            {
                initAction = delegate
                {
                    CloneLab clone = TargetA.Thing as CloneLab;
                    if (clone != null)
                    {
                        CloneLab rainbowSquieerl2 = (CloneLab)TargetA.Thing;
                        if (GetActor().Position == TargetA.Thing.InteractionCell)
                        {
                            rainbowSquieerl2.Cloning(GetActor());
                        }
                    }
                    else
                    {
                        Log.Error(ErrorMessage.Translate());
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
