using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.QLine
{
    public class OpenGUIWindow : MainButtonWorker
    {
        private static bool hasChecked = false;
        private static bool hasQuests = false;
        static bool isOpen = false;

        public override bool Visible
        {
            get
            {
                if (!hasChecked)
                    hasQuests = DefDatabase<QL_Quest>.DefCount > 0;
                return hasQuests;
            }
        }

        public override void Activate()
        {
            if (!isOpen)
            {
                Find.WindowStack.Add(new QL_Window());
                isOpen = true;
            }
            else
            {
                if (Find.WindowStack.WindowOfType<QL_Window>() != null)
                {
                    Find.WindowStack.TryRemove(typeof(QL_Window));
                }
                isOpen = false;
            }
        }
    }
    public class QL_Window : Window
    {
        /// <summary>
        /// For adding events when a quest is dismissed.
        /// </summary>
        public static Action dismissQuest;
        public static Action acceptQuest;
        private Vector2 listScroll;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800, 500);
            }
        }

        public QL_Window()
        {
            this.draggable = true;
            listScroll = Vector2.zero;
        }
        QL_Quest quest;
        Vector2 scrollPos;
        public override void DoWindowContents(Rect inRect)
        {
            Quest_Tracker tracker= Find.World.GetComponent<Quest_Tracker>();
           #region quest info
            //COMPILED BY NESGUI
            //Prepare varibles
            
            GameFont prevFont = Text.Font;
            TextAnchor textAnchor = Text.Anchor;

            //Rect pass

            Rect list = new Rect(new Vector2(1f, 5f), new Vector2(130f, 460f));
            Rect listViewRect = new Rect(new Vector2(5f, 5f), new Vector2(160f, 460f));
            Rect PawnsListRect = new Rect(new Vector2(5, 5), new Vector2(160, 460 + (tracker.Quests.Count * 2f)));
            Rect questtitle = new Rect(new Vector2(165f, 10f), new Vector2(560f, 50f));
            Rect questdesc = new Rect(new Vector2(165f, 65f), new Vector2(560f, 345f));
            Rect delete = new Rect(new Vector2(555f, 400f), new Vector2(170f, 55f));
            Rect accept = new Rect(new Vector2(375f, 400f), new Vector2(170f, 55f));

            //Button pass

            if (quest != null)
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                bool Dismissquest = Widgets.ButtonText(delete, "Dismiss quest");
                if (Dismissquest)
                {
                    dismissQuest?.Invoke();
                    tracker.RemoveQuest(quest);
                }

                bool AcceptQuest = Widgets.ButtonText(accept, "Accept quest");
                if (AcceptQuest)
                {
                    acceptQuest?.Invoke();
                }
                Text.Font = prevFont;
                Text.Anchor = textAnchor;
                Widgets.Label(questdesc, quest.description);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(questtitle, quest.label);
                Text.Font = prevFont;
                Text.Anchor = textAnchor;
            }
            else
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(questtitle, "No quest selected");
                Text.Font = prevFont;
                Text.Anchor = textAnchor;
            }
            Widgets.DrawLine(new Vector2(questtitle.xMin, questtitle.yMax), new Vector2(questtitle.xMax, questtitle.yMax), Color.white, 1);



            //END NESGUI CODE
            #endregion

            #region quest select
            Widgets.BeginScrollView(listViewRect, ref scrollPos, list);
            int pos = 30;
            Rect label = new Rect(new Vector2(0, pos), new Vector2(list.width, 30));
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(label, "Quests");
            Widgets.DrawLine(new Vector2(label.xMin, label.yMax), new Vector2(label.xMax, label.yMax),Color.white,1);
            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            pos += 40;
            foreach (QL_Quest q in tracker.Quests)
            {
                label = new Rect(new Vector2(0, pos), new Vector2(list.width, 30));
                Widgets.Label(label, q.label);
                bool button = Widgets.ButtonInvisible(label);
                if (button)
                {
                    quest = q;
                }
                pos += 30;
            }
            Widgets.EndScrollView();

            #endregion
        }
    }
}
