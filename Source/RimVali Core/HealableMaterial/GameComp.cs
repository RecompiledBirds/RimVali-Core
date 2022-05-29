using RimWorld.Planet;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
namespace RimValiCore.HealableMaterial
{
    [HarmonyPatch(typeof(Thing), "SpawnSetup")]
    public static class HealableSpawnPatch
    {
        public static void Postfix(Thing __instance)
        {
            if (__instance.def.race == null && __instance.def.projectile == null)
            {
                HealStuff targ = HealableMats.FindThing(__instance);
                if (targ != null)
                {
                    HealableGameComp.AddThing(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "DeSpawn")]
    public static class HealableDeSpawnPatch
    {
        public static void Postfix(Thing __instance)
        {
            if (__instance.def.race == null && __instance.def.projectile == null)
            {
                HealStuff targ = HealableMats.FindThing(__instance);
                if (targ != null)
                {
                    HealableGameComp.RemoveThing(__instance);
                }
            }
        }
    }
    public class HealableGameComp : WorldComponent
    {
       public static void AddThing(Thing thing)
        {
            HealStuff mat = HealableMats.FindThing(thing);
            if (mat != null)
            {
                things.Add(thing);
                ticks.Add(thing, mat.ticks);
                healStuff.Add(thing, mat);
            }
            
        }

        public static void RemoveThing(Thing thing)
        {
            if (things.Contains(thing))
            {
                things.Remove(thing);
                ticks.Remove(thing);
                healStuff.Remove(thing);
            }
        }

        private static List<Thing> things = new List<Thing>();
        private static Dictionary<Thing, int> ticks = new Dictionary<Thing, int>();
        private static Dictionary<Thing, HealStuff> healStuff = new Dictionary<Thing,HealStuff>();

        public HealableGameComp(World world) : base(world)
        {
            ticks = new Dictionary<Thing, int>();
            things = new List<Thing>();
            healStuff = new Dictionary<Thing, HealStuff>();
        }
        void Update()
        {

            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing != null && thing.Spawned)
                {
                    HealStuff mat = healStuff[thing];
                    if (ticks[thing] == 0)
                    {
                        int wantedHP = thing.HitPoints + mat.amount;
                        thing.HitPoints = wantedHP > thing.MaxHitPoints ? thing.MaxHitPoints : wantedHP;
                        ticks[thing] = mat.ticks;
                    }
                    else
                    {
                        ticks[thing]--;
                    }
                }
            }
        }
        public override void WorldComponentTick()
        {
            Update();
        }
    }
}
