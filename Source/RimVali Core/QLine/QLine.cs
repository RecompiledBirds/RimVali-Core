using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimValiCore.QLine
{
    /// <summary>
    /// RimValiCore's custom quest system.
    /// </summary>
    public static class QLine
    {
        public static void Trigger_Quest(QL_Quest quest)
        {
            quest.QuestWorker.Action();
        }

        public static void EndQuestPositive(QL_Quest quest)
        {
            //Do more with this later to trigger quests
            //quest.positiveQuestResult;

            //Reward actions and the like
            quest.QuestWorker.PositiveEndAction();
        }

        public static void EndQuestNegative(QL_Quest quest)
        {
            //Do more with this later to trigger quests
            //quest.positiveQuestResult;

            //Reward actions and the like
            quest.QuestWorker.NegativeEndAction();
        }
    }
}
