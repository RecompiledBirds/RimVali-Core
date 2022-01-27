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
        public HashSet<QL_Quest> Quests
        {
            get
            {
                return quests;
            }
        }

        public HashSet<QL_Quest> FinishedQuests
        {
            get
            {
                return finishedQuests;
            }
        }

        public List<QL_Quest> QuestsLists
        {
            get
            {
                return quests.ToList();
            }
        }

        public void RemoveQuest(QL_Quest quest)
        {
            quests.Remove(quest);
        }

        int tick = 0;
        
        public QL_Quest GetRandomQuest()
        {
            return GetAvalibleQuests.RandomElementByWeight(x=>x.QuestWorker.QuestWeight());
        }

        private HashSet<QL_Quest> GetAvalibleQuests
        {
            get
            {
                return DefDatabase<QL_Quest>.AllDefs.Where(x => x.QuestWorker.IsAvalible()).ToHashSet();
            }
        }

        private bool HasAvalibleQuests
        {
            get
            {
                return GetAvalibleQuests.Count> 0; 
            }
        }

        private int tickTime = 1;
        public override void WorldComponentTick()
        {
            if(tick == tickTime && HasAvalibleQuests)
            {
                tick = 0;
                QL_Quest quest = GetRandomQuest();
                quests.Add(quest);
            }
            tick++;
            base.WorldComponentTick();
        }
    }
}
