using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
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
        private string buttonText;
        private Action buttonAction;
        private List<DisableReason> disableReasons;

        public QuestStageButtonDecision(string buttonText, Action buttonAction)
        {
            this.buttonText = buttonText;
            this.buttonAction = buttonAction;
        }

        public QuestStageButtonDecision(string buttonText, Action buttonAction, List<DisableReason> disableReasons) : this(buttonText, buttonAction)
        {
            this.disableReasons = disableReasons;
        }

        public string ButtonText { get => buttonText; set => buttonText = value; }

        public bool Disabled => !disableReasons.NullOrEmpty() && DisableReasons.Any(reason => reason.ShouldDisable);

        public string DisableReason => DisableReasons.Join(reason => $"{reason.Reason}: {reason.ShouldDisable}", "\n");

        public Action ButtonAction { get => buttonAction; set => buttonAction = value; }
        
        public List<DisableReason> DisableReasons { get => disableReasons; set => disableReasons = value; }

        public override string ToString() => $"[QuestStageButtonDecision] buttonText: {buttonText}, hasAction: {buttonAction != null}";
    }

    public class DisableReason
    {
        private readonly Func<bool> shouldDisable = () => false;
        private readonly Func<string> reason = () => "No Reason Given";

        public DisableReason(Func<bool> shouldDisable, Func<string> reason)
        {
            this.reason = reason;
            this.shouldDisable = shouldDisable;
        }

        public bool ShouldDisable => shouldDisable();

        public string Reason => reason();
    }
}
