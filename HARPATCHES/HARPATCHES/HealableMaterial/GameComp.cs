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
        public override void GameComponentTick()
        {
            if (!Current.Game.Maps.NullOrEmpty()&&Current.Game.Maps.Any(x=>x.spawnedThings.Any(y=>HealableMatFinder.thingDefs.ContainsKey(y.def)||y.Stuff!=null&& HealableMatFinder.thingDefs.ContainsKey(y.Stuff))))
            {
                List<Thing> things = new List<Thing>();
                foreach(Map map in Current.Game.Maps.Where(x => x.spawnedThings.Any(y => HealableMatFinder.thingDefs.ContainsKey(y.def) || y.Stuff != null && HealableMatFinder.thingDefs.ContainsKey(y.Stuff)))){things.AddRange(map.spawnedThings.Where(y => HealableMatFinder.thingDefs.ContainsKey(y.def) || y.Stuff != null && HealableMatFinder.thingDefs.ContainsKey(y.Stuff)));}
                foreach(Thing thing in things)
                {
                    if (!ticks.ContainsKey(thing)){ticks.Add(thing, 0);}
                    else
                    {
                        if (HealableMatFinder.thingDefs.ContainsKey(thing.Stuff)&&ticks[thing]==HealableMatFinder.thingDefs[thing.Stuff].ticks&&thing.HitPoints<thing.MaxHitPoints)
                        {
                            int amount = HealableMatFinder.thingDefs[thing.Stuff].amount;
                            thing.HitPoints = thing.HitPoints + amount <= thing.MaxHitPoints ? HealableMatFinder.thingDefs[thing.Stuff].amount + thing.HitPoints : thing.MaxHitPoints;
                            ticks[thing] = 0;
                        }
                        else if (HealableMatFinder.thingDefs.ContainsKey(thing.def)&&ticks[thing]==HealableMatFinder.thingDefs[thing.def].ticks&&thing.HitPoints<thing.MaxHitPoints)
                        {
                            int amount = HealableMatFinder.thingDefs[thing.def].amount;
                            thing.HitPoints = thing.HitPoints + amount <= thing.MaxHitPoints ? HealableMatFinder.thingDefs[thing.def].amount + thing.HitPoints : thing.MaxHitPoints;
                            ticks[thing] = 0;
                        }
                        ticks[thing]++;
                    }
                }
                ticks.RemoveAll(x => !x.Key.Spawned);

            }
            base.GameComponentTick();
        }
    }
}
