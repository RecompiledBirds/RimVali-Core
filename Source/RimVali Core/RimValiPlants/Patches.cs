// Does this need fixing or can it be deleted?
using HarmonyLib;
using RimValiCore;
using RimWorld;
using System;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RimValiCore.RimValiPlants
{

    public class RVPlantCompProperties : CompProperties
    {
        public float minPreferredTemp;
        public float maxPreferredTemp;
        public RVPlantCompProperties()
        {
            this.compClass = typeof(RVPlantComp);
        }
    }

    public class RVPlantComp : ThingComp
    {
        public RVPlantCompProperties Props
        {
            get
            {
                return this.props as RVPlantCompProperties;
            }
        }
    }
    [HarmonyPatch(typeof(Plant), "get_GrowthRateFactor_Temperature")]
    public static class RimValiPlantsSeasonGrowthRatePrefix
    {
        public static bool Prefix(ref float __result, Plant __instance)
        {
            RVPlantComp plantComp = __instance.TryGetComp<RVPlantComp>();
            if (plantComp != null)
            {
                IntVec3 vec = __instance.Position;
                Map map = __instance.Map;
                float temperature = GridsUtility.GetTemperature(vec, map);
                if (temperature < 6f)
                {
                    __result = Mathf.InverseLerp(plantComp.Props.minPreferredTemp, plantComp.Props.maxPreferredTemp +6f, temperature);
                }
                if (temperature > 42f)
                {
                    __result = Mathf.InverseLerp(plantComp.Props.maxPreferredTemp, plantComp.Props.maxPreferredTemp+ 42f, temperature);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlantUtility), "GrowthSeasonNow")]
    public static class RimValiPlantsSeasonGrowthSeasonNowPrefix
    {
        public static bool Prefix(IntVec3 c, Map map, ref bool __result,bool forSowing)
        {
            Plant plant = (Plant)map.thingGrid.ThingAt(c,ThingCategory.Plant);
            if(plant != null)
            {
                __result = CanGrow(plant.def, c, map,forSowing);
                return false;
            }

            return true;
        }

        public static bool CanGrowSimplified(Plant plant, bool forSowing = false)
        {
            return CanGrow(plant.def, plant.Position, plant.Map, forSowing);
        }
        public static bool CanGrow(ThingDef plant, IntVec3 c, Map map, bool forSowing = false)
        {
            RVPlantCompProperties props = (RVPlantCompProperties)plant.comps.Find(x=>x.GetType()==typeof(RVPlantCompProperties));
            float temperature = c.GetTemperature(map);
            if (props != null)
            {
                Log.Message("test2");
                return  temperature > props.minPreferredTemp && temperature < props.maxPreferredTemp;
            }

            Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
            if (roomOrAdjacent == null)
            {
                return false;
            }
            if (!roomOrAdjacent.UsesOutdoorTemperature)
            {
                return temperature > 0f && temperature < 58f;
            }
            return forSowing ? map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNowForSowing : map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow;
          }
    }

    [HarmonyPatch(typeof(Plant), "get_LeaflessTemperatureThresh")]
    public static class RimValiPlantsLeaflessPrefix
    {
        public static bool Prefix(Plant __instance, ref float __result)
        {
            RVPlantComp plantComp = __instance.TryGetComp<RVPlantComp>();
            if(plantComp != null)
            {
                __result = plantComp.Props.minPreferredTemp-8;
               
                
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Zone_Growing), "GetInspectString")]
    public static class RimValiZone_GrowingPrefix
    {
        public static bool Prefix(Zone_Growing __instance, ref string __result)
        {
            string text = "";
            if (!__instance.Cells.NullOrEmpty())
            {
                if (__instance.Cells.First().UsesOutdoorTemperature(__instance.Map))
                {
                    text += "OutdoorGrowingPeriod".Translate() + ": " + Zone_Growing.GrowingQuadrumsDescription(__instance.Map.Tile) + "\n";
                }
                if (IsSeason(__instance))
                {
                    text += "GrowSeasonHereNow".Translate();
                }
                else
                {
                    text += "CannotGrowBadSeasonTemperature".Translate();
                }
            }
            __result = text;
            return false;
        }

        public static bool IsSeason(Zone_Growing grower)
        {
            ThingDef def = grower.GetPlantDefToGrow();
            return RimValiPlantsSeasonGrowthSeasonNowPrefix.CanGrow(def, grower.Position, grower.Map, true);
        }
    }

  //  [HarmonyPatch(typeof(Plant),"TickLong")]
    public static class RimValiPlantTickLongTranspiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int end = 0;
            bool found = false;
            List<CodeInstruction> codes = instructions.ToList();
            for(int a = 0; a < codes.Count; a++)
            {
                if(a+4<codes.Count && codes[a + 4].opcode == OpCodes.Call && codes[a + 4].operand == AccessTools.Method(typeof(PlantUtility), "GrowthSeasonNow"))
                {
                    end=a+4;
                    found=true;
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(Plant));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RimValiPlantsSeasonGrowthSeasonNowPrefix), "CanGrowSimplified"));
                }
                else if(a>end||!found)
                {
                    yield return codes[a];
                }
            }
        }
    }
}

