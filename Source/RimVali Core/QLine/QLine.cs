using System;
using Verse;

namespace RimValiCore.QLine
{
    /// <summary>
    /// RimValiCore's custom quest system.
    /// </summary>
    public class QLine : IExposable
    {
        public QLine(QL_Quest quest)
        {
            this.quest = quest;
            worker = quest.QuestWorker;
        }
        private QL_Quest quest;
        private QuestWorker worker;

        public QuestWorker Worker => worker;
        public QL_Quest Quest => quest;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref quest, "quest");
            Scribe_Deep.Look(ref worker, "worker");
        }
    }
}
