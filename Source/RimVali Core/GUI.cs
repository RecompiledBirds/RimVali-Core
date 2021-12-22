using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
namespace RimValiCore
{
    
    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "DoWindowContents")]
    public static class ConfigurePatch
    {
        [HarmonyPostfix]
        public static void Patch()
        {
            bool button = Widgets.ButtonText(new Rect(new Vector2(870, 0), new Vector2(100, 30)),"Edit pawn");
            if (button)
            {
                RVGUI.FindFirstPawn();
                Find.WindowStack.Add(new EditorWindow());
            }
        }

       
    }

    public class EditorWindow : Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1000, 1000);
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            RVGUI.Draw();
        }
    }

    public static class RVGUI
    {
        static string EditableNicknameMiddleName = "";
        static string EditableLastName = "";
        static string EditableFirstName = "";
        static bool portraitEnabled = true;
        public static readonly Vector2 PawnPortraitSize = new Vector2(250f, 250f);
        private static Pawn pawn;
        private static Vector2 scrollPos;

        public static void FindFirstPawn()
        {
            SetPawn(Find.GameInitData.startingAndOptionalPawns[0]);
        }

        public static void SetPawn(Pawn p)
        {
            pawn = p;
        }
        public static void Draw()
        {
            //COMPILED BY NESGUI
            //Prepare varibles

            GameFont prevFont = Text.Font;
            TextAnchor textAnchor = Text.Anchor;

            //Rect pass
            //-400
            Rect PawnBeingEdited = new Rect(new Vector2(100f, 100f), new Vector2(250f, 250f));
            Rect PawnSelector = new Rect(new Vector2(0f, 100f), new Vector2(95, 350f));
            Rect ColourSelect = new Rect(new Vector2(350f, 100f), new Vector2(250f, 350f));
            Rect PawnViewSettings = new Rect(new Vector2(100f, 350f), new Vector2(250f, 25f));
            Rect PawnFirstName = new Rect(new Vector2(600f, 100f), new Vector2(150f, 50f));
            Rect PawnMiddleName = new Rect(new Vector2(600f, 150f), new Vector2(150f, 50f));
            Rect PawnLastName = new Rect(new Vector2(600f, 200f), new Vector2(150f, 50f));
            Rect SaveButton = new Rect(new Vector2(700f, 450f), new Vector2(50f, 50f));
            Rect Editor = new Rect(new Vector2(0f, 0f), new Vector2(100f, 100f));
            Rect ModName = new Rect(new Vector2(100f, 0f), new Vector2(250f, 50f));
            Rect CreditsRect = new Rect(new Vector2(0f, 450f), new Vector2(600f, 50f));



            Listing_Standard ls = new Listing_Standard();
            Rect PawnsListRect = new Rect(new Vector2(0,100), new Vector2(95,520+(Find.GameInitData.startingAndOptionalPawns.Count*2f)));
            Widgets.BeginScrollView(PawnSelector, ref scrollPos, PawnsListRect);
            int pos = 120;
            foreach(Pawn p in Find.GameInitData.startingAndOptionalPawns)
            {
                Rect pawnRect = new Rect(new Vector2(0, pos), new Vector2(95, 95));
                Rect label = new Rect(new Vector2(10, pawnRect.yMax - 5), new Vector2(95, 30));
                RenderTexture image = PortraitsCache.Get(p, PawnPortraitSize, Rot4.South, new Vector2(0, 0), 1f, true, true, true, true, null, null, true);
                Widgets.DrawTextureFitted(pawnRect,image,1);
                Widgets.Label(label, p.Name.ToStringShort);
                bool button =Widgets.ButtonInvisible(new Rect(new Vector2(0, pos),new Vector2(95, 95 + 30)));
                if (button)
                {
                    pawn = p;
                } 
                pos += 120;
            }
            Widgets.EndScrollView();
            //Button pass

            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;

            bool SaveChanges = Widgets.ButtonText(SaveButton, "Save Changes");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;

            //Checkbox pass

            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.CheckboxLabeled(PawnViewSettings, "View Pawn w/ Clothes", ref portraitEnabled);

            if (portraitEnabled)
            {
                RenderTexture image = PortraitsCache.Get(pawn, PawnPortraitSize, Rot4.South,default(Vector2), 1f, true, true, true, true, null, null, true);
                GUI.DrawTexture(PawnBeingEdited, image);
            }

            Text.Font = prevFont;
            Text.Anchor = textAnchor;

            //Label pass

            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;

            Widgets.Label(PawnSelector, "Pawn Select");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.Label(ModName, "Generic Mod Name");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.Label(CreditsRect, "Baseline UI Designed by Willows Wulf");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(CreditsRect, "UI made using NesGui");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;

            Widgets.Label(CreditsRect, "NesGUI made by Nesi & Danimineiro");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;

            //Textfield pass
            NameTriple pawnName = (NameTriple)pawn.Name;
            string first = pawnName.First;
            string nick = pawnName.Nick;
            string last = pawnName.Last;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            //ohboy how do we pawn name thing a
            first=Widgets.TextField(PawnFirstName,first);
           
            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            
            nick = Widgets.TextField(PawnMiddleName, nick);

            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            prevFont = Text.Font;
            textAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            
            last = Widgets.TextField(PawnLastName, last);
            pawnName= new NameTriple(first,nick,last);

            pawn.Name = pawnName;
            Text.Font = prevFont;
            Text.Anchor = textAnchor;

            //END NESGUI CODE

        }
    }
}
