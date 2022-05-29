using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.QLine
{
    public class QuestStage
    {
        public string label;
        public string description;
        public List<QuestStageButtonDecision> buttons;

        public QuestStageButtonDecision this[int index] 
        { 
            get => buttons[index]; 
            set => buttons[index] = value; 
        }

        public string LabelCap => label.CapitalizeFirst();
    }

    public class QuestStageButtonDecision
    {
        public  QuestStageButtonDecision(string buttonText, Action action)
        {
            this.buttonText = buttonText;
            this.action = action;
        }

        private string buttonText;
        private Action action;

        public string ButtonText { get => buttonText; set => buttonText = value; }
        public Action Action { get => action; set => action = value; }
    }
}
