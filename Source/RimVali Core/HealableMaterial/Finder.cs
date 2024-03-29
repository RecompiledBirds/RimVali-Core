﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimValiCore.HealableMaterial
{
    public class HealStuff
    {
        public int amount;
        public int ticks;
    }

    public static class HealableMats
    {
        private static readonly Dictionary<ThingDef, HealStuff> thingDefs = new Dictionary<ThingDef, HealStuff>();

        public static HealStuff FindThing(Thing thing)
        {
            HealStuff healStuff = null;

            if (thingDefs.ContainsKey(thing.def))
            {
                return thingDefs[thing.def];
            }
            if (thing.Stuff != null && thingDefs.ContainsKey(thing.Stuff))
            {
                return thingDefs[thing.Stuff];
            }

            return healStuff;
        }

        internal static void Intialize()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp(typeof(HealableComp))))
            {
                thingDefs.Add(def, new HealStuff()
                {
                    amount = def.GetCompProperties<HealableCompProps>().amountHealed,
                    ticks = def.GetCompProperties<HealableCompProps>().ticksBetweenHeal
                });
            }
            Log.Message(($"[RimVali Core]: We found {thingDefs.Count} healable materials."));
        }
    }
}
