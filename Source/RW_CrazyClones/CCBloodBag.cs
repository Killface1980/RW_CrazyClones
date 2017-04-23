using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_CrazyClones
{
    public class CCBloodBag : DNA_Blueprint
    {
        public override string Label
        {
            get { return base.Label + " " + donorPawn.Name; }
        }
    }
}
