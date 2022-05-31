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
    public static class RimValiPlantsSeasonGrowthRateTranspiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            List<CodeInstruction> codes = instructions.ToList();
            for (int a = 0; a < codes.Count; a++)
            {
                if (codes[a].opcode == OpCodes.Call && codes[a].operand == AccessTools.Method(typeof(PlantUtility), "GrowthRateFactorFor_Temperature"))
                {
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(Plant));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RimValiPlantsSeasonGrowthRateTranspiler), "GrowthRate"));
                }
                else
                {
                    yield return codes[a];
                }

            }
        }

        public static float GrowthRate(Plant plant)
        {
            IntVec3 vec = plant.Position;
            Map map = plant.Map;
            float temperature = GridsUtility.GetTemperature(vec,map);
            float result=1;
            RVPlantComp plantComp = plant.TryGetComp<RVPlantComp>();
            if(plantComp != null)
            {
                if (temperature < 6f)
                {
                    return Mathf.InverseLerp(plantComp.Props.minPreferredTemp, 6f, temperature);
                }
                if (temperature > 42f)
                {
                    return Mathf.InverseLerp(plantComp.Props.maxPreferredTemp, 42f, temperature);
                }
            }
            else
            {
                if (temperature < 6f)
                {
                    return Mathf.InverseLerp(0f, 6f, temperature);
                }
                if (temperature > 42f)
                {
                    return Mathf.InverseLerp(58f, 42f, temperature);
                }
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(PlantUtility), "GrowthSeasonNow")]
    public static class RimValiPlantsSeasonGrowthSeasonNowTranspiler
    {
        public static bool Prefix(IntVec3 c, Map map, ref bool __result, bool forSowing = false)
        {
            Plant plant = (Plant)map.thingGrid.ThingAt(c, ThingCategory.Plant);
            RVPlantComp plantComp = plant.TryGetComp<RVPlantComp>();
            if (plantComp != null)
            {
                float temperature = GridsUtility.GetTemperature(c, map);
                __result= temperature > plantComp.Props.minPreferredTemp && temperature < plantComp.Props.maxPreferredTemp;
                return false;
            }

            return true;
        }
    }
}

