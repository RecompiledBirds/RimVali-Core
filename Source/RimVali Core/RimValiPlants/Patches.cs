// Does this need fixing or can it be deleted?
using HarmonyLib;
using RimValiCore;
using RimWorld;
using System;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace RimValiCore.RimValiPlants
{

	public static class RimValiPlantsSeasonTranspiler
    {
        [HarmonyPatch(typeof(Plant), "get_GrowthRateFactor_Temperature")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            List<CodeInstruction> codes = instructions.ToList();
            for (int a = 0; a < codes.Count; a++)
            {
                if (codes[a].opcode == OpCodes.Call && codes[a].operand == AccessTools.Method(typeof(PlantUtility), "GrowthRateFactorFor_Temperature"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing), "get_Map"));
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(Map));
                }
                else
                {
                    yield return codes[a];
                }

            }
        }

        public static bool IsGrowthSeason(IntVec3 vec, Map map)
        {
            float temperature = GridsUtility.GetTemperature(vec,map);

            return temperature > 0;
        }
    }
	
}

