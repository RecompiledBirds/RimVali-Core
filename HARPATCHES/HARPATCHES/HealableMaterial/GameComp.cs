using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace RimValiCore.HealableMaterial
{
    public class HealableGameComp : GameComponent
    {
        public HealableGameComp(Game game){ticks = new Dictionary<Thing, int>();}

        Dictionary<Thing, int> ticks = new Dictionary<Thing, int>();
        HashSet<Thing> things = new HashSet<Thing>();
        int refreshData = 0;
        public override void GameComponentTick()
        {
            if (!Current.Game.Maps.NullOrEmpty()&&Current.Game.Maps.Any(x=>x.spawnedThings.Any(y=>HealableMatFinder.thingDefs.ContainsKey(y.def)||y.Stuff!=null&& HealableMatFinder.thingDefs.ContainsKey(y.Stuff))))
            {
                if (refreshData == 120)
                {
                    foreach (Map map in Current.Game.Maps.Where(x => x.spawnedThings.Any(y => HealableMatFinder.thingDefs.ContainsKey(y.def) || y.Stuff != null && HealableMatFinder.thingDefs.ContainsKey(y.Stuff))))
                    {
                        things.AddRange(map.spawnedThings.Where(y => !things.Contains(y) && (HealableMatFinder.thingDefs.ContainsKey(y.def) || y.Stuff != null && HealableMatFinder.thingDefs.ContainsKey(y.Stuff)) && y.HitPoints < y.MaxHitPoints));
                    }
                    refreshData = 0;
                }
                refreshData++;
                foreach (Thing thing in things.Where(x=>x!=null))
                {
                    try
                    {
                        if (!ticks.ContainsKey(thing)) {
                            Log.Message("Adding thing");
                            ticks.Add(thing, 0);
                            Log.Message("added thing");
                        }
                        else
                        {
                            if ((thing.Stuff!=null &&( HealableMatFinder.thingDefs.ContainsKey(thing.Stuff) && ticks[thing] == HealableMatFinder.thingDefs[thing.Stuff].ticks)) || (HealableMatFinder.thingDefs.ContainsKey(thing.def) && ticks[thing] == HealableMatFinder.thingDefs[thing.def].ticks) && thing.HitPoints < thing.MaxHitPoints)
                            {
                                Log.Message("Getting thing");
                                ThingDef def = (thing.Stuff != null ? thing.Stuff : thing.def);
                                int amount = HealableMatFinder.thingDefs[def].amount;
                                Log.Message("Got thing");
                                thing.HitPoints = thing.HitPoints + amount <= thing.MaxHitPoints ? HealableMatFinder.thingDefs[def].amount + thing.HitPoints : thing.MaxHitPoints;
                                ticks[thing] = 0;
                            }
                            ticks[thing]++;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }
                }
                ticks.RemoveAll(x => !x.Key.Spawned|| x.Key.Destroyed || x.Key.HitPoints == x.Key.MaxHitPoints);
                things.RemoveWhere(x => !x.Spawned || x.Destroyed || x.HitPoints==x.MaxHitPoints);

            }
        }
    }
}
