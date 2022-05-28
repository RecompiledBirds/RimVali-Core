using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimValiCore.RVRFrameWork
{

    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class PawnGeneratorTranspiler
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            for (int a = 0; a < codes.Count; a++)
            {
                //Look for where the pawn is created.
                if (codes[a].opcode == OpCodes.Call && codes[a].Calls(typeof(ThingMaker).GetMethod("MakeThing")))
                {
                    //Load argument 0 from stack
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    //Load it as a PawnGenerationRequest
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(PawnGenerationRequest));
                    //Call our function.
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnGeneratorTranspiler), "GetHumanoidRace",new Type[] {typeof(PawnGenerationRequest)}));
                }
                else
                {
                    yield return codes[a];
                }
            }

        }

        public static bool ShouldSwitch(PawnGenerationRequest request)
        {
           return  request.Context != PawnGenerationContext.PlayerStarter && request.KindDef.RaceProps.Humanlike;
        }
        public static Thing GetHumanoidRace(PawnGenerationRequest request)
        {
            ThingDef def;
            
            if (ShouldSwitch(request))
            {
                IEnumerable<ThingDef> defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race != null && x.race.Humanlike);
                def = defs.RandomElementByWeight(x=>x==ThingDefOf.Human?50 :30/defs.Count());
            }
            else
            {
                def = request.KindDef.race;
            }
            
            return ThingMaker.MakeThing(def);
        }
    }
}
