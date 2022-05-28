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
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(PawnGenerationRequest));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnGeneratorTranspiler), "GetHumanoidRace",new Type[] {typeof(PawnGenerationRequest)}));
                }
                else
                {
                    yield return codes[a];
                }
            }

        }
        public static bool ShouldSwitchPawnkindBased(PawnGenerationRequest request)
        {
            return (DefDatabase<RaceSwapDef>.AllDefs.Any(x=>x.targetRaces.Contains(request.KindDef.race))) && request.KindDef.RaceProps.Humanlike;
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

                def = defs.RandomElementByWeight(x=>x==ThingDefOf.Human?50 :30);
            }
            else
            {
                def = request.KindDef.race;
            }

            if (ShouldSwitchPawnkindBased(request))
            {
                RaceSwapDef randomSwapDef = DefDatabase<RaceSwapDef>.AllDefsListForReading.Where(x => x.targetRaces.Contains(def)).RandomElement();
                def = randomSwapDef.replacementRaces.RandomElement();
            }

            return ThingMaker.MakeThing(def);
        }
    }
}
