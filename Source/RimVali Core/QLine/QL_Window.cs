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
        static bool isOpen = false;
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

        public QL_Window()
        {
            this.resizeable = true;
            this.draggable = true;
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

            Rect list = new Rect(new Vector2(5f, 5f), new Vector2(160f, 460f));
            Rect PawnsListRect = new Rect(new Vector2(5, 5), new Vector2(160, 460 + (tracker.Quests.Count * 2f)));
            Rect questtitle = new Rect(new Vector2(165f, 5f), new Vector2(560f, 50f));
            Rect questdesc = new Rect(new Vector2(165f, 55f), new Vector2(560f, 345f));
            Rect delete = new Rect(new Vector2(555f, 400f), new Vector2(170f, 55f));
            Rect Label = new Rect(new Vector2(5f, 465f), new Vector2(720f, 45f));

            //Button pass

            if (quest != null)
            {
                bool Dismissquest = Widgets.ButtonText(delete, "Dismiss quest");
                if (Dismissquest)
                {
                    dismissQuest?.Invoke();
                    tracker.RemoveQuest(quest);
                }

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


            //Label pass

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.Label(Label, "RimVali QLine\n Built with NesGUI");

            Text.Font = prevFont;
            Text.Anchor = textAnchor;


            //END NESGUI CODE
            #endregion

            #region quest select
            Widgets.BeginScrollView(list, ref scrollPos, PawnsListRect);
            int pos = 90;
            Rect label = new Rect(new Vector2(0, pos), new Vector2(list.width, 30));
            Widgets.Label(label, "Quests");
            pos += 90;
            foreach (QL_Quest q in tracker.Quests)
            {
                label = new Rect(new Vector2(0, pos), new Vector2(list.width, 30));
                Widgets.Label(label, q.label);
                bool button = Widgets.ButtonInvisible(label);
                if (button)
                {
                    quest = q;
                }
                pos += 90;
            }
            Widgets.EndScrollView();

            #endregion
        }
    }
}
