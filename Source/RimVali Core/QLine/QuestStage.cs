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

        public override string ToString()
        {
            return $"[QuestStage] label: {label}, description: {description}, buttons:\n    {string.Join("\n    ", buttons)}";
        }
    }

    public class QuestStageButtonDecision
    {
        public QuestStageButtonDecision(string buttonText, Action buttonAction)
        {
            this.buttonText = buttonText;
            this.buttonAction = buttonAction;
        }

        public QuestStageButtonDecision(string buttonText, Action buttonAction, Func<bool> disableButtonFunc, Func<string> disableReason) : this(buttonText, buttonAction)
        {
            this.disableButtonFunc = disableButtonFunc;
            this.disableReason = disableReason;
        }

        private string buttonText;
        private Action buttonAction;
        private Func<bool> disableButtonFunc;
        private Func<string> disableReason;

        public string ButtonText { get => buttonText; set => buttonText = value; }
        public Action ButtonAction { get => buttonAction; set => buttonAction = value; }
        public Func<bool> DisableButtonFunc { get => disableButtonFunc; set => disableButtonFunc = value; }
        public Func<string> DisableReason { get => disableReason; set => disableReason = value; }

        public override string ToString()
        {
            return $"[QuestStageButtonDecision] buttonText: {buttonText}, hasAction: {buttonAction != null}";
        }
    }
}
