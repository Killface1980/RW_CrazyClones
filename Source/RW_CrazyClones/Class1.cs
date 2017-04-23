using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RW_CrazyClones
{
    class CCBloodBag:ThingWithComps
    {
        public Pawn donorPawn;

        public override string Label
        {
            get { return base.Label + " " + donorPawn.Name; }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference(ref donorPawn, "donorPawn");

        }
    }
}
