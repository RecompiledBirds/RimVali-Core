using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace RimValiCore.HealableMaterial
{
    public class HealStuff
    {
        public int amount;
        public int ticks;
    }
    [StaticConstructorOnStartup]
    public static class HealableMatFinder
    {
        public static Dictionary<ThingDef, HealStuff> thingDefs = new Dictionary<ThingDef, HealStuff>();
        static HealableMatFinder()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp(typeof(HealableComp))))
            {
                thingDefs.Add(def, new HealStuff() { amount = def.GetCompProperties<HealableCompProps>().amountHealed, ticks = def.GetCompProperties<HealableCompProps>().ticksBetweenHeal });
            }
            Log.Message(($"[RimVali Core/ Healable mats]: found {thingDefs.Count()} valid materials."));
        }
    }
}
