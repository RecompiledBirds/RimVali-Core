using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimValiCore.Windows.GUIUtils;
using Verse.Sound;

namespace RimValiCore.QLine
{
    public class QL_Window : MainTabWindow
    {
        /// <summary>
        ///     Stores what quests are expanded
        /// </summary>
        private static readonly HashSet<QL_Quest> expandedQuests = new HashSet<QL_Quest>();

        private readonly Rect rectFull = new Rect(0f, 0f, 240f, 440f);
        private readonly Rect rectMain;

        //Quest Item stuff
        private readonly Rect rectContentPartOuter;

        private Rect rectContentPartInner;
        private Rect rectQuestBase;
        private Rect rectQuestStageBase;

        //Title stuff
        private readonly Rect rectTopPart;
        private readonly Rect rectTitle;

        //consts
        private const float CommonMargin = 5f;
        private const float ItemHeight = 30f;
        private const float ExpandCollapseIconSize = 18f;

        //
        private Vector2 listScroll;

        public override Vector2 InitialSize => rectFull.size;

        protected override float Margin => 0f;

        public float RequiredHeightForInnerScrollRect => (ItemHeight + CommonMargin) * DefDatabase<QL_Quest>.AllDefsListForReading.Sum(def => 1 + (expandedQuests.Contains(def) ? def.QuestWorker.Stages().Count : 0));

        public QL_Window()
        {
            def = DefDatabase<MainButtonDef>.GetNamed("QuestQUI");

            rectMain = rectFull.ContractedBy(CommonMargin * 2f);
            rectTopPart = rectMain.TopPartPixels(30f);
            rectTitle = new Rect(rectTopPart.x, rectTopPart.y, rectTopPart.width, rectTopPart.height - 5f);

            rectContentPartOuter = new Rect(rectMain.x, rectMain.y + rectTopPart.height, rectMain.width, rectMain.height - rectTopPart.height);
            RefreshScrollRects();
        }

        /// <summary>
        ///     This function refreshes the height of <see cref="rectContentPartInner"/>, so that the scrollbar doesn't end up too short/long
        /// </summary>
        public void RefreshScrollRects()
        {
            rectContentPartInner = rectContentPartOuter.GetInnerScrollRect(RequiredHeightForInnerScrollRect);
            rectQuestBase = new Rect(rectContentPartInner.x, rectContentPartInner.y, rectContentPartInner.width, ItemHeight);
            rectQuestStageBase = new Rect(rectContentPartInner.x + CommonMargin * 2f, rectContentPartInner.y, rectContentPartInner.width - CommonMargin * 2f, ItemHeight);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;

            Widgets.Label(rectTitle, "##Quests:");
            Widgets.DrawLineHorizontal(rectTitle.x, rectTitle.yMax, rectTitle.width);

            Text.Font = GameFont.Small;

            //Quest Listing
            Widgets.BeginScrollView(rectContentPartOuter, ref listScroll, rectContentPartInner);

            int count = 0;
            for (int i = 0; i < DefDatabase<QL_Quest>.AllDefsListForReading.Count; i++)
            {
                QL_Quest quest = DefDatabase<QL_Quest>.AllDefsListForReading[i];
                Vector2 baseVector = new Vector2(0f, (rectQuestBase.height + CommonMargin) * (i + count));
                Rect rectQuestItem = new Rect(rectQuestBase).MoveRect(baseVector);
                Rect rectExpandCollapseIcon = rectQuestItem.RightPartPixels(ExpandCollapseIconSize).BottomPartPixels(ExpandCollapseIconSize).MoveRect(new Vector2(- CommonMargin, - CommonMargin));

                Widgets.DrawBox(rectQuestItem);
                rectQuestItem.DoRectHighlight((i + count) % 2 == 1);
                Widgets.Label(rectQuestItem, quest.LabelCap);
                Widgets.DrawTextureFitted(rectExpandCollapseIcon, expandedQuests.Contains(quest) ? TexButton.Collapse : TexButton.Reveal, 1f);
                Widgets.DrawHighlightIfMouseover(rectQuestItem);
                
                if (Widgets.ButtonInvisible(rectQuestItem))
                {
                    if (expandedQuests.Add(quest))
                    {
                        SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    }
                    else
                    {
                        expandedQuests.Remove(quest);
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                    }

                    RefreshScrollRects();
                }

                //Quest stage Listing
                if (expandedQuests.Contains(quest))
                {
                    for (int j = 0; j < quest.QuestWorker.Stages().Count; j++)
                    {
                        count++;
                        Rect rectQuestStage = new Rect(rectQuestStageBase).MoveRect(baseVector + new Vector2(0f, rectQuestBase.height + CommonMargin + (rectQuestBase.height + CommonMargin) * j));
                        Widgets.DrawBox(rectQuestStage);
                        rectQuestStage.DoRectHighlight((i + count) % 2 == 1);
                        Widgets.Label(rectQuestStage, quest.QuestWorker.Stages()[j].LabelCap);
                        Widgets.DrawHighlightIfMouseover(rectQuestStage);

                        if (Widgets.ButtonInvisible(rectQuestStage))
                        {

                        }
                    }
                }
            }

            Widgets.EndScrollView();


            // Quest_Tracker tracker = Find.World.GetComponent<Quest_Tracker>();
            #region quest info
            // //COMPILED BY NESGUI
            // //Prepare varibles

            // GameFont prevFont = Text.Font;
            // TextAnchor textAnchor = Text.Anchor;

            // //Rect pass

            // Rect list = new Rect(new Vector2(1f, 5f), new Vector2(130f, 460f));
            // Rect listViewRect = new Rect(new Vector2(5f, 5f), new Vector2(160f, 460f));
            // Rect PawnsListRect = new Rect(new Vector2(5, 5), new Vector2(160, 460 + (tracker.Quests.Count * 2f)));
            // Rect questtitle = new Rect(new Vector2(165f, 10f), new Vector2(560f, 50f));
            // Rect questdesc = new Rect(new Vector2(165f, 65f), new Vector2(560f, 345f));
            // Rect delete = new Rect(new Vector2(555f, 400f), new Vector2(170f, 55f));
            // Rect accept = new Rect(new Vector2(375f, 400f), new Vector2(170f, 55f));

            // //Button pass

            // if (quest != null)
            // {
            //     Text.Font = GameFont.Small;
            //     Text.Anchor = TextAnchor.MiddleCenter;
            //     bool Dismissquest = Widgets.ButtonText(delete, "Dismiss quest");
            //     if (Dismissquest)
            //     {
            //         dismissQuest?.Invoke();
            //     }

            //     bool AcceptQuest = Widgets.ButtonText(accept, "Accept quest");
            //     if (AcceptQuest)
            //     {
            //         acceptQuest?.Invoke();
            //     }
            //     Text.Font = prevFont;
            //     Text.Anchor = textAnchor;
            //     Widgets.Label(questdesc, quest.description);
            //     Text.Font = GameFont.Medium;
            //     Text.Anchor = TextAnchor.MiddleLeft;
            //     Widgets.Label(questtitle, quest.label);
            //     Text.Font = prevFont;
            //     Text.Anchor = textAnchor;
            // }
            // else
            // {
            //     Text.Font = GameFont.Medium;
            //     Text.Anchor = TextAnchor.MiddleLeft;
            //     Widgets.Label(questtitle, "No quest selected");
            //     Text.Font = prevFont;
            //     Text.Anchor = textAnchor;
            // }
            // Widgets.DrawLine(new Vector2(questtitle.xMin, questtitle.yMax), new Vector2(questtitle.xMax, questtitle.yMax), Color.white, 1);



            // //END NESGUI CODE
            #endregion

            #region quest select
            //Widgets.BeginScrollView(listViewRect, ref scrollPos, list);
            //int pos = 30;
            //Rect label = new Rect(new Vector2(0, pos), new Vector2(list.width, 30));
            //Text.Font = GameFont.Medium;
            //Text.Anchor = TextAnchor.MiddleLeft;
            //Widgets.Label(label, "Quests");
            //Widgets.DrawLine(new Vector2(label.xMin, label.yMax), new Vector2(label.xMax, label.yMax),Color.white,1);
            //Text.Font = prevFont;
            //Text.Anchor = textAnchor;
            //pos += 40;
           
            //Widgets.EndScrollView();

            #endregion
        }
    }
}
