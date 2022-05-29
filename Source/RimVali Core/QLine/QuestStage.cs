using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimValiCore.QLine
{
    public class QuestStage
    {
        public string label;
        public string description;
        public List<QuestStageButtonDecision> buttons;

        public string LabelCap => label.CapitalizeFirst();
    }
}
