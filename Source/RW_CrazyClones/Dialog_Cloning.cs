using System;
using System.Collections.Generic;
using System.IO;
using RimWorld;
using RW_FacialStuff;
using RW_FacialStuff.Defs;
using RW_FacialStuff.Detouring;
using RW_FacialStuff.Utilities;
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


        private static HairDef _newHair;
        private static HairDef originalHair;
        private static BeardDef _newBeard;
        private static MouthDef _newMouth;
        private static MouthDef originalMouth;
        private static EyeDef _newEye;
        private static EyeDef originalEye;
        private static BrowDef _newBrow;
        private static Color _newColour;



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
            Rect rect = new Rect(_iconSize + _margin, 0f, inRect.width - _iconSize - _margin, _titleHeight);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUILayout.BeginArea(rect);
            GUILayout.Label(_title);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            //      Rect iconPosition = new Rect(0f, 0f, _iconSize, _iconSize).CenteredOnYIn(rect);
            //     GUI.DrawTexture(iconPosition, _icon);

            var cloneList = new Dictionary<CCBloodBag, int>(); ;
            foreach (var thing in Find.VisibleMap.listerThings.ThingsOfDef(ThingDef.Named("CCBloodBag")))
            {
                CCBloodBag bloodbag = (CCBloodBag)thing;
                DrawCloneRow(bloodbag);
            }

            GUILayout.EndArea();

            DialogUtility.DoNextBackButtons(inRect, "AcceptAndClone".Translate(), delegate
            {

                foreach (var thing in Find.VisibleMap.listerThings.ThingsOfDef(ThingDef.Named("CCBloodBag")))
                {
                    CCBloodBag bloodbag = (CCBloodBag)thing;
                    Initializer.Test(bloodbag);
                }
                // force colonist bar to update
                // xxx
                Close();
            }, delegate
            {
                Close();
            });
        }



        private void DrawCloneRow(CCBloodBag bloodbag)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(bloodbag.Name.ToStringFull);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
