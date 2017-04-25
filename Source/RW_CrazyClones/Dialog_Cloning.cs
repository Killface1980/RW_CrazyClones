using System;
using System.Collections.Generic;
using System.IO;
using RimWorld;
using RW_CrazyClones.Utilities;
#if FS
using RW_FacialStuff;
using RW_FacialStuff.Defs;
using RW_FacialStuff.Detouring;
#endif
using UnityEngine;
using Verse;


namespace RW_CrazyClones
{
    [StaticConstructorOnStartup]
    public class Dialog_Cloning : Window
    {



        private static string _title;
        private static float _titleHeight;
        private static float _previewSize;
        private static float _iconSize;
        //     private static Texture2D _icon;
        private static float _margin;
        private static float _listWidth;
        private static int _columns;






        static Dialog_Cloning()
        {
            _title = "CloneLabTitle".Translate();
            _titleHeight = 30f;
            _previewSize = 250f;
            //       _previewSize = 100f;
            _iconSize = 24f;
            //    _icon = ContentFinder<Texture2D>.Get("ClothIcon");
            _margin = 6f;
            _listWidth = 450f;
            //   _listWidth = 200f;
            _columns = 5;


        }

        public Dialog_Cloning(Pawn p)
        {
            absorbInputAroundWindow = false;
            forcePause = true;
            closeOnClickedOutside = false;
        }

        public override void PostOpen()
        {

        }

        public override void PreClose()
        {
        }


        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(_iconSize + _margin, 0f, inRect.width - _iconSize - _margin, inRect.height);

            GUILayout.BeginArea(rect);
            GUILayout.BeginVertical();
            GUILayout.Label(_title);
            GUILayout.EndVertical();

            //      Rect iconPosition = new Rect(0f, 0f, _iconSize, _iconSize).CenteredOnYIn(rect);
            //     GUI.DrawTexture(iconPosition, _icon);

            foreach (var thing in Find.VisibleMap.listerThings.ThingsOfDef(ThingDef.Named("CCBloodBag")))
            {
                DNA_Blueprint bloodbag = (DNA_Blueprint)thing;
                DrawCloneRow(bloodbag);
            }

            GUILayout.EndArea();

            DialogUtility.DoNextBackButtons(inRect, "AcceptAndClone".Translate(), delegate
            {

                foreach (var thing in Find.VisibleMap.listerThings.ThingsOfDef(ThingDef.Named("CCBloodBag")))
                {
                    DNA_Blueprint bloodbag = (DNA_Blueprint)thing;
                    if (bloodbag.amountToClone > 0)
                    {
                        for (int i = 0; i < bloodbag.amountToClone; i++)
                        {
                            Initializer.Test(bloodbag);

                        }
                    }
                }
                // force colonist bar to update
                // xxx
                Close();
            }, delegate
            {
                Close();
            });
        }



        private void DrawCloneRow(DNA_Blueprint bloodbag)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(bloodbag.nameInt.ToStringFull);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            bloodbag.amountToClone = (int)GUILayout.HorizontalSlider(bloodbag.amountToClone, 0, 10);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
