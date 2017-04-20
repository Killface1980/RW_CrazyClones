using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RW_CrazyClones
{
    class Designator_TakeBloodSample : Designator
    {
        public bool didWeDesignateAnything = false;

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public Designator_TakeBloodSample()
        {
            this.defaultLabel = "Take Blood";
            this.icon = ContentFinder<Texture2D>.Get("Items/BloodBag", true);
            this.defaultDesc = "Quickly designate Pawns to add the take blood medical bill.";
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.DesignateHaul;
            DesignationCategoryDef named = DefDatabase<DesignationCategoryDef>.GetNamed("Orders", true);
            Type type = named.specialDesignatorClasses.Find((Type x) => x == base.GetType());
            if (type == null)
            {
                named.specialDesignatorClasses.Add(base.GetType());
                named.ResolveReferences();
                DesignationCategoryDef named2 = DefDatabase<DesignationCategoryDef>.GetNamed("OrdersTakeBloodSampleAll", true);
                List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
                allDefsListForReading.Remove(named2);
                DefDatabase<DesignationCategoryDef>.ResolveAllReferences();
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            AcceptanceReport result;
            if (!GenGrid.InBounds(c, base.Map) || GridsUtility.Fogged(c, base.Map))
            {
                result = false;
            }
            else
            {
                bool flag = false;
                foreach (Thing current in GridsUtility.GetThingList(c, base.Map))
                {
                    if (this.CanDesignateThing(current).Accepted)
                    {
                        flag = true;
                    }
                }
                result = flag;
            }
            return result;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            AcceptanceReport result;
            if (!(t is Pawn))
            {
                result = false;
            }
            else
            {

                // added the prisoner below

                Pawn pawn = (Pawn)t;
                if (pawn.IsColonist || pawn.IsPrisonerOfColony && !pawn.BillStack.Bills.Any((Bill x) => x.recipe.defName == "TakeDNASample"))

                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (Thing current in GridsUtility.GetThingList(c, base.Map))
            {
                if (this.CanDesignateThing(current).Accepted)
                {
                    this.DesignateThing(current);
                }
            }
            this.notifyResult();
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            foreach (IntVec3 current in cells)
            {
                foreach (Thing current2 in GridsUtility.GetThingList(current, base.Map))
                {
                    if (this.CanDesignateThing(current2).Accepted)
                    {
                        this.DesignateThing(current2);
                    }
                }
            }
            this.notifyResult();
        }

        public void notifyResult()
        {
            if (this.didWeDesignateAnything)
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.DesignateDeconstruct);
            }
            else
            {
                Messages.Message("Must designate colonists that aren't already designated.", Verse.MessageSound.RejectInput);
            }
            this.didWeDesignateAnything = false;
        }

        public override void DesignateThing(Thing t)
        {
            Pawn pawn = (Pawn)t;
            foreach (RecipeDef current in pawn.def.AllRecipes)
            {
                if (current.AvailableNow)
                {
                    IEnumerable<BodyPartRecord> partsToApplyOn = current.Worker.GetPartsToApplyOn(pawn, current);
                    if (partsToApplyOn.Any<BodyPartRecord>())
                    {
                        foreach (BodyPartRecord current2 in partsToApplyOn)
                        {
                            RecipeDef recipeDef = current;
                            BodyPartRecord part = current2;
                            if (current.defName == "TakeDNASample")
                            {
                                Bill_Medical bill_Medical = new Bill_Medical(recipeDef);
                                pawn.BillStack.AddBill(bill_Medical);
                                bill_Medical.Part = part;
                                this.didWeDesignateAnything = true;
                            }
                        }
                    }
                }
            }
            this.didWeDesignateAnything = true;
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            Messages.Message("Pawns designated for blood taking.", Verse.MessageSound.Standard);
            this.didWeDesignateAnything = false;
        }

        protected override void FinalizeDesignationFailed()
        {
            base.FinalizeDesignationFailed();
            Messages.Message("Must designate pawns.", Verse.MessageSound.RejectInput);
            this.didWeDesignateAnything = false;
        }
    }

}
