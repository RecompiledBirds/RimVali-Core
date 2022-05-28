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
        private HashSet<QL_Quest> finishedQuests =  new HashSet<QL_Quest>();
        private HashSet<QL_Quest> quests = new HashSet<QL_Quest>();
        public HashSet<QL_Quest> Quests => quests;
            
        public bool IsFinished(QL_Quest quest)
        {
            return finishedQuests.Contains(quest);
        }

        public bool IsQueued(QL_Quest quest)
        {
            return quests.Contains(quest);
        }
        public HashSet<QL_Quest> FinishedQuests => finishedQuests;
        
        public List<QL_Quest> QuestsLists =>quests.ToList();
 
        public void RemoveQuest(QL_Quest quest) => quests.Remove(quest);

        public void FinishQuest(QL_Quest quest)
        {
            if (!quest.repeatable)
                finishedQuests.Add(quest);


            if (quests.Contains(quest))
                RemoveQuest(quest);

        }

        public void QueueQuest(QL_Quest quest)
        {
            if(!IsQueued(quest))
                quests.Add(quest);
        }

        public void 



        int tick = 0;
        
        private int tickTime = 1;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
        }
    }
}
