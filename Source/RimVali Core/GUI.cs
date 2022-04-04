using System;
using System.Collections.Generic;
using System.Linq;
using RimValiCore.Windows;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using RimValiCore.RVR;
using Verse.Sound;

namespace RimValiCore
{
    
    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "DoWindowContents")]
    public static class ConfigurePatch
    {
        [HarmonyPostfix]
        public static void Patch()
        {
            bool button = Widgets.ButtonText(new Rect(new Vector2(870, 0), new Vector2(100, 30)), "##Edit pawn");
            if (button)
            {
                RVGUI.FindFirstPawn();
                Find.WindowStack.Add(new EditorWindow());
            }
        }

       
    }

    public class EditorWindow : Window
    {
        private const float RectColorFieldHeight = 40f;

        private readonly List<Pawn> pawns = Find.GameInitData.startingAndOptionalPawns;

        private readonly Rect RectWindowMain = new Rect(0f, 0f, 1000f, 400f);
        private readonly Rect RectWindowSub;
        private readonly Rect RectWindowEdit;
        private readonly Rect RectPawnSelectOuter;
        private readonly Rect RectColorSelectOuter;

        private readonly Rect[] RectEditSections;
        private readonly Rect[] RectNamingRects;
        private readonly Rect RectColoringPart;
        private readonly Rect RectPawnBig;

        private Dictionary<string, ColorSet> colorSets = new Dictionary<string, ColorSet>();
        private Rect[] RectColorFields;
        private Rect RectColorSelectInner;
        private Rect RectPawnSelectInner;
        private Vector2 PawnSelectorScroll;
        private Vector2 ColorSelectorScroll;

        private Pawn selectedPawn;

        public override Vector2 InitialSize => RectWindowMain.size;

        protected override float Margin => 0f;

        public EditorWindow()
        {
            doCloseX = true;
            closeOnClickedOutside = true;

            SelectedPawn = Find.GameInitData.startingAndOptionalPawns[0];

            RectWindowSub = RectWindowMain.ContractedBy(25f);
            RectPawnSelectOuter = RectWindowSub.LeftPartPixels(172f);
            RectPawnSelectInner = RectPawnSelectOuter.LeftPartPixels(172f - 17f);

            RectPawnSelectInner.height = 55f * pawns.Count;
            if (RectPawnSelectInner.height < RectPawnSelectOuter.height)
            {
                RectPawnSelectInner.width += 17f;
            }

            RectWindowEdit = RectWindowSub.RightPartPixels(RectWindowSub.width - RectPawnSelectOuter.width - 5f);

            RectEditSections = RectWindowEdit.DivideVertical(2).ToArray();
            RectColoringPart = RectEditSections[0];
            RectNamingRects = RectEditSections[1].TopPartPixels(39f).ContractVertically(5).DivideHorizontal(3).ToArray();
            RectColorSelectOuter = RectColoringPart.RightPartPixels(300f).ContractVertically(5);
            RectColorSelectOuter.width -= 5;

            CalcInnerRect();

            RectPawnBig = RectColoringPart.LeftPartPixels(RectEditSections[0].height);
        }

        private void CalcInnerRect()
        {
            List<Rect> rectList = new List<Rect>();

            RectColorSelectInner = new Rect(RectColorSelectOuter)
            {
                height = (colorSets.Count) * RectColorFieldHeight - 5f
            };

            if (HasOpenColorField) RectColorSelectInner.height += RectColorFieldHeight * 3f;

            if (RectColorSelectInner.height > RectColorSelectOuter.height) RectColorSelectInner.width -= 17f;

            RectColorSelectInner.height += 5f;

            for (int i = 0; i < colorSets.Count; i++)
            {
                Vector2 mod = new Vector2(0f, RectColorFieldHeight * i + ((HasOpenColorField && (i > OpenColorField)) ? RectColorFieldHeight * 3f : 0f));
                Rect tempRect = RectColorSelectInner.TopPartPixels(RectColorFieldHeight).MoveRect(mod);
                tempRect.height -= 5f;

                rectList.Add(tempRect);
            }

            RectColorFields = rectList.ToArray();
        }

        private bool HasOpenColorField => OpenColorField > -1;

        private int OpenColorField { get; set; }

        private Pawn SelectedPawn
        {
            get => selectedPawn;
            set
            {
                selectedPawn = value;

                if (SelectedPawn.def is RimValiRaceDef && SelectedPawn.GetComp<ColorComp>() is ColorComp comp)
                {
                    colorSets = comp.colors;
                }
                else
                {
                    colorSets = new Dictionary<string, ColorSet>();
                }

                OpenColorField = -1;
                CalcInnerRect();
            }
        }

        public override void DoWindowContents(Rect _)
        {
            DrawPawnSelectionArea();
            DrawPawn();
            //DrawColorSelection();

            Widgets.BeginScrollView(RectColorSelectOuter, ref ColorSelectorScroll, RectColorSelectInner);
            int pos = 0;
            foreach (KeyValuePair<string, ColorSet> kvp in colorSets)
            {
                string name = $"##{kvp.Key}";
                Rect rectTemp = RectColorFields[pos];
                Rect rectName = rectTemp;
                Rect rectExpandCollapseIcon = rectTemp.LeftPartPixels(rectTemp.height);

                Text.Anchor = TextAnchor.MiddleLeft;

                Widgets.DrawLightHighlight(rectName);
                Widgets.DrawHighlightIfMouseover(rectTemp);
                Widgets.DrawBox(rectName, 2);
                Widgets.Label(rectName.RightPartPixels(rectName.width - rectTemp.height - 5f), name);
                Widgets.DrawTextureFitted(rectExpandCollapseIcon.ContractedBy(11f), pos == OpenColorField ? TexButton.Collapse : TexButton.Reveal, 1f);
                
                if (Widgets.ButtonInvisible(rectTemp))
                {
                    if (pos == OpenColorField)
                    {
                        OpenColorField = -1;
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                    }
                    else
                    {
                        if (HasOpenColorField) SoundDefOf.TabClose.PlayOneShotOnCamera();

                        OpenColorField = pos;
                        SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    }

                    CalcInnerRect();
                }

                if (pos == OpenColorField)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float indent = 15f;

                        string colorName = $"##Color_{i}";
                        Rect rectColorField = rectTemp.MoveRect(new Vector2(indent, RectColorFieldHeight * (i + 1)));
                        Rect rectColorLabel = rectColorField.RightPartPixels(rectColorField.width - 5f);
                        Rect rectColorColor = rectColorLabel.RightPartPixels(rectColorLabel.width - 100f - 5f);

                        rectColorField.width -= indent;

                        Widgets.DrawLightHighlight(rectColorField);
                        Widgets.DrawBoxSolidWithOutline(rectColorColor, kvp.Value.Colors[i], new Color(255f, 255f, 255f, 0.5f), 3);
                        Widgets.DrawHighlightIfMouseover(rectColorColor);
                        Widgets.DrawBox(rectColorField);
                        Widgets.Label(rectColorLabel, colorName);
                        
                        if (Widgets.ButtonInvisible(rectColorColor))
                        {
                            int k = i; //save the current i to k so that the value of i isn't overridden during the for loop
                            Find.WindowStack.Add(new ColorPickerWindow(color => SetColor(color, kvp, k), (_0) => { }, kvp.Value.Colors[k], new Color[10]));
                        }
                        TooltipHandler.TipRegion(rectColorColor, $"##Change {colorName}");
                    }
                }

                RimValiUtility.ResetTextAndColor();

                pos++;
            }

            RimValiUtility.ResetTextAndColor();
            Widgets.EndScrollView();

            DrawNameEdit();

            //RVGUI.Draw();
        }

        private void DrawNameEdit()
        {
            if (SelectedPawn.Name is NameTriple name)
            {
                string first = Widgets.TextField(RectNamingRects[0], name.First, 25, CharacterCardUtility.ValidNameRegex);

                if (name.First.Equals(name.Nick) || name.Last.Equals(name.Nick)) GUI.color = new Color(255f, 255f, 255f, 0.4f);
                string nick = Widgets.TextField(RectNamingRects[1].ContractHorizontally(5), name.Nick, 25, CharacterCardUtility.ValidNameRegex);
                GUI.color = Color.white;

                string last = Widgets.TextField(RectNamingRects[2], name.Last, 25, CharacterCardUtility.ValidNameRegex);

                SelectedPawn.Name = new NameTriple(first, nick, last);
            }
            else
            {
                //Fixes names that aren't NameTriple based by converting them into one
                string[] fullName = SelectedPawn.Name.ToString().Split(' ');

                string first = fullName[0];
                string nick = "";
                string last = "";

                if (fullName.Length > 1)
                {
                    last = fullName[fullName.Length - 1];

                    for (int i = 1; i < fullName.Length - 1; i++)
                    {
                        nick += $"{fullName[i]} ";
                    }

                    if (nick.EndsWith(" "))
                    {
                        nick.Substring(0, nick.Length - 1);
                    }
                }

                SelectedPawn.Name = new NameTriple(first, nick, last);
            }
        }

        private void SetColor(Color color, KeyValuePair<string, ColorSet> kvp, int index)
        {
            Color[] colors = kvp.Value.Colors;
            colors[index] = color;
            kvp.Value.Colors = colors;

            SelectedPawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        private void DrawPawn()
        {
            Widgets.DrawBox(RectColoringPart);
            RenderTexture image = PortraitsCache.Get(SelectedPawn, new Vector2(1024f, 1024f), Rot4.South, new Vector3(0f, 0f, 0.14f), cameraZoom: 2f, supersample: false);
            GUI.DrawTexture(RectPawnBig, image, ScaleMode.StretchToFill);
        }

        private void DrawPawnSelectionArea()
        {
            Widgets.BeginScrollView(RectPawnSelectOuter, ref PawnSelectorScroll, RectPawnSelectInner);
            GUI.BeginGroup(RectPawnSelectInner);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                Rect rectPawnBox = new Rect(5f, 55f * i, RectPawnSelectInner.width - 10f, 50f);
                Rect rectPawnContent = rectPawnBox.ContractedBy(5f);
                rectPawnContent.height += 5f;

                Rect rectPawnPortraitArea = rectPawnContent.RightPartPixels(rectPawnContent.height);

                RenderTexture image = PortraitsCache.Get(pawn, new Vector2(256f, 256f), Rot4.South, new Vector3(0f, 0f, 0.25f), stylingStation: true, cameraZoom: 2.5f, supersample: false);

                Widgets.DrawBox(rectPawnBox);
                Widgets.DrawHighlight(rectPawnBox);
                Widgets.DrawHighlightIfMouseover(rectPawnBox);

                Text.Font = GameFont.Tiny;

                if (pawn.Name is NameTriple name && name.Nick is string nick)
                {
                    Widgets.Label(rectPawnContent, nick);
                }
                else
                {
                    Widgets.Label(rectPawnContent, pawn.Name.ToString());
                }

                Text.Anchor = TextAnchor.LowerLeft;

                Widgets.Label(rectPawnContent.MoveRect(new Vector2(0f, -5f)), pawn.story.TitleCap);

                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawTextureFitted(rectPawnPortraitArea, image, 1f);

                RimValiUtility.ResetTextAndColor();

                if (SelectedPawn == pawn)
                {
                    Widgets.DrawBoxSolid(rectPawnBox, new Color(181f, 141f, 0f, 0.2f));
                }

                if (Widgets.ButtonInvisible(rectPawnBox))
                {
                    SelectedPawn = pawn;
                }
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
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
