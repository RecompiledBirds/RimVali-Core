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
        public override void GameComponentTick()
        {
            if (!Current.Game.Maps.NullOrEmpty()&&Current.Game.Maps.Any(x=>x.spawnedThings.Any(y=>HealableMatFinder.thingDefs.ContainsKey(y.def)||y.Stuff!=null&& HealableMatFinder.thingDefs.ContainsKey(y.Stuff))))
            {
                
                foreach(Map map in Current.Game.Maps.Where(x => x.spawnedThings.Any(y => HealableMatFinder.thingDefs.ContainsKey(y.def) || y.Stuff != null && HealableMatFinder.thingDefs.ContainsKey(y.Stuff)))){things.AddRange(map.spawnedThings.Where(y => !things.Contains(y)&&(HealableMatFinder.thingDefs.ContainsKey(y.def) || y.Stuff != null && HealableMatFinder.thingDefs.ContainsKey(y.Stuff))&&y.HitPoints<y.MaxHitPoints));}
                foreach(Thing thing in things)
                {
                    if (!ticks.ContainsKey(thing)){ticks.Add(thing, 0);}
                    else
                    {
                        if ((HealableMatFinder.thingDefs.ContainsKey(thing.Stuff)&&ticks[thing]==HealableMatFinder.thingDefs[thing.Stuff].ticks&&thing.HitPoints<thing.MaxHitPoints) ||(HealableMatFinder.thingDefs.ContainsKey(thing.def) && ticks[thing] == HealableMatFinder.thingDefs[thing.def].ticks && thing.HitPoints < thing.MaxHitPoints))
                        {
                            int amount = HealableMatFinder.thingDefs[thing.Stuff].amount;
                            thing.HitPoints = thing.HitPoints + amount <= thing.MaxHitPoints ? HealableMatFinder.thingDefs[thing.Stuff].amount + thing.HitPoints : thing.MaxHitPoints;
                            ticks[thing] = 0;
                        }
                        ticks[thing]++;
                    }
                }
                ticks.RemoveAll(x => !x.Key.Spawned|| x.Key.Destroyed || x.Key.HitPoints == x.Key.MaxHitPoints);
                things.RemoveWhere(x => !x.Spawned || x.Destroyed || x.HitPoints==x.MaxHitPoints);

            }
        }
    }
}
