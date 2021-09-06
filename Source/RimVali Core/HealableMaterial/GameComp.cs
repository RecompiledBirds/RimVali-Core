using System.Collections.Generic;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.HealableMaterial
{
    public class HealableGameComp : GameComponent
    {
        public HealableGameComp(Game game)
        {
            ticks = new Dictionary<Thing, int>();
            refreshTick = refreshATTick - 2;
        }

        private void CleanupThing(Thing thing)
        {
            if (ticks.ContainsKey(thing))
            {
                ticks.Remove(thing);
            }
            things.Remove(thing);
        }

        private int refreshTick;
        private readonly int refreshATTick = 480;
        private float slowDown()
        {
            int area = Current.Game.AnyPlayerHomeMap.Area;
            return area / (float)(Current.Game.Maps.Count + 0.2) * area;
        }
        private List<Thing> GetThings
        {
            get
            {
                List<Thing> val = new List<Thing>();
                foreach (Map map in Current.Game.Maps)
                {
                    foreach (Thing thing in map.spawnedThings)
                    {
                        if (thing.def.race == null && thing.def.projectile == null)
                        {
                            val.Add(thing);
                        }
                    }
                }
                return val;
            }
        }

        private readonly Dictionary<Thing, int> ticks = new Dictionary<Thing, int>();
        private List<Thing> things = new List<Thing>();
        private bool threadIsRunning;
        public override void GameComponentTick()
        {
            void update()
            {
                if (!Current.Game.Maps.NullOrEmpty())
                {
                    if (refreshTick == refreshATTick)
                    {
                        things = GetThings;
                        refreshTick = 0;
                    }
                    for (int i = 0; i < things.Count; i++)
                    {
                        Thing thing = things[i];

                        if (thing != null && thing.Spawned)
                        {
                            if (!ticks.ContainsKey(thing))
                            {
                                ticks.Add(thing, 0);
                            }
                            HealStuff targ = HealableMatFinder.FindThing(thing);
                            if (targ != null && thing.HitPoints < thing.MaxHitPoints && targ.ticks == ticks[thing])
                            {
                                int wantedHP = thing.HitPoints + targ.amount;
                                thing.HitPoints = wantedHP > thing.MaxHitPoints ? thing.MaxHitPoints : wantedHP;
                                ticks[thing] = 0;
                            }
                            ticks[thing]++;
                        }
                        else
                        {
                            CleanupThing(thing);
                        }
                    }
                    refreshTick++;
                }
                threadIsRunning = false;
            }
            if (!threadIsRunning)
            {
                threadIsRunning = true;
                Task task = new Task(update);
                task.Start();
            }
        }
    }
}
