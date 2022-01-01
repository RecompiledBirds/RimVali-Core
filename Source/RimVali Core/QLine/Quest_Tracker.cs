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
            base.ExposeData();
        }

        private HashSet<QL_Quest> quests = new HashSet<QL_Quest>();
        public HashSet<QL_Quest> Quests
        {
            get
            {
                return quests;
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
    }
}
