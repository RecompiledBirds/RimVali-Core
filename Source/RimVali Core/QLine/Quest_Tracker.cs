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
        public Quest_Tracker(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref quests, "QL_Quests");
            Scribe_Collections.Look(ref finishedQuests, "finishedQuests");
            base.ExposeData();
        }
        private HashSet<QLine> finishedQuests =  new HashSet<QLine>();
        private HashSet<QLine> quests = new HashSet<QLine>();
        public HashSet<QLine> Quests => quests;
            
        public bool IsFinished(QLine quest)
        {
            return finishedQuests.Contains(quest);
        }

        public bool IsQueued(QLine quest)
        {
            return quests.Contains(quest);
        }
        public HashSet<QLine> FinishedQuests => finishedQuests;
        
        public List<QLine> QuestsLists => quests.ToList();
 
        public void RemoveQuest(QLine quest) => quests.Remove(quest);

        public void FinishQuest(QLine quest)
        {
            if (!quest.Quest.repeatable)
                finishedQuests.Add(quest);


            if (quests.Contains(quest))
                RemoveQuest(quest);
        }

        public void QueueQuest(QL_Quest quest)=>QueueQuest(QuestMaker.MakeQuest(quest));

        public void QueueQuest(QLine quest)
        {
            if(!IsQueued(quest))
                quests.Add(quest);
        }




        int tick = 0;
        
        private int tickTime = 1;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
        }
    }
}
