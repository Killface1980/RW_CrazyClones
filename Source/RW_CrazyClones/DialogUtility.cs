﻿using System;
using UnityEngine;
using Verse;

namespace RW_CrazyClones.Utilities
{
    public static class DialogUtility
    {
        public const float BottomAreaHeight = 38f;
        private static readonly Vector2 BottomButSize = new Vector2(150f, 38f);
        public static void DoNextBackButtons(Rect innerRect, string nextLabel, Action nextAct, Action backAct)
        {
            float top = innerRect.height - 38f;
            Text.Font = GameFont.Small;
            if (backAct != null)
            {
                Rect rect = new Rect(0f, top, BottomButSize.x, BottomButSize.y);
                if (Widgets.ButtonText(rect, "Back".Translate()))
                {
                    backAct();
                }
            }
            if (nextAct != null)
            {
                Rect rect2 = new Rect(innerRect.width - BottomButSize.x, top, BottomButSize.x, BottomButSize.y);
                if (Widgets.ButtonText(rect2, nextLabel))
                {
                    nextAct();
                }
            }
        }
        public static bool DoMiddleButton(Rect innerRect, string label)
        {
            float top = innerRect.height - 38f;
            Rect rect = new Rect(innerRect.width / 2f - BottomButSize.x / 2f, top, BottomButSize.x, BottomButSize.y);
            return Widgets.ButtonText(rect, label);
        }
    }
}