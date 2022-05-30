using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimValiCore.QLine
{
    public class Quest_Tracker : WorldComponent
    {
        private List<QLine> finishedQuests = new List<QLine>();
        private List<QLine> quests = new List<QLine>();

        public List<QLine> Quests => quests;

        public List<QLine> FinishedQuests => finishedQuests;

        public Quest_Tracker(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
        }

        public bool IsFinished(QLine quest) => finishedQuests.Contains(quest);

        public bool IsQueued(QLine quest) => quests.Contains(quest);

        public bool RemoveQuest(QLine quest) => quests.Remove(quest);

        public void FinishQuest(QLine quest)
        {
            if (!quest.Quest.repeatable) finishedQuests.Add(quest);
            RemoveQuest(quest);
        }

        public void QueueQuest(QL_Quest quest) => QueueQuest(QuestMaker.MakeQuest(quest));

        public void QueueRandomQuest()
        {

            QueueQuest(DefDatabase<QL_Quest>.AllDefsListForReading.Where(x=>x.QuestWorker.CanBeShuffled&& !finishedQuests.Any(y=>y.Quest==x)||x.repeatable).RandomElementByWeight(new Func<QL_Quest, float>(z=>z.QuestWorker.GetWeight)));
        }

        public void QueueQuest(QLine quest) => quests.Add(quest);

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref quests, "QL_Quests");
            Scribe_Collections.Look(ref finishedQuests, "finishedQuests");
            base.ExposeData();
        }
    }
    public static class DebugActions
    {
        [DebugAction("RimVali", "Add Random Quest", allowedGameStates = AllowedGameStates.Playing)]
        public static void AddRandomQuest()
        {
            Find.World.GetComponent<Quest_Tracker>().QueueRandomQuest();
        }
    }
}
