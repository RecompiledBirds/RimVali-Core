using HarmonyLib;
using RimValiCore.RVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.CompatiblityPatches
{
    public static class RVCCompatiblityPatches
    {
        public static void DoPatches()
        {
            Harmony harmony = new Harmony("RimValiCore.Compatiblity");
            if (!ModLister.HasActiveModWithName("Vanilla Expanded Framework"))
            {
                HarmonyMethod transpiler = new HarmonyMethod(typeof(RenderTextureTranspiler), nameof(RenderTextureTranspiler.Transpile));
                harmony.Patch(original: AccessTools.Constructor(typeof(PawnTextureAtlas)), transpiler: transpiler);
            }
            if (ModLister.HasActiveModWithName("[KV] Show Hair With Hats or Hide All Hats"))
            {
                HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(HideHairWithHatsPatch), nameof(HideHairWithHatsPatch.Prefix));
                harmony.Patch(original: AccessTools.Method(AccessTools.TypeByName("HairUtility"), "TryGetCustomHairMat"), prefix: harmonyMethod);
            }

            Log.Message($"<color=green>[RimVali Core]: Ran {harmony.GetPatchedMethods().Count()} compatiblity patches!</color>");
        }
    }
}
