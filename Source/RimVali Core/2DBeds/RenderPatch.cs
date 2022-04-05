using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoDBedPatch;
using UnityEngine;
using Verse;

namespace Beds2DPatch
{
    class RenderPatch
    {
        private static Dictionary<Pawn, RenderData> renderDatas = new Dictionary<Pawn, RenderData>();
        private static bool skipPatch = false;

        internal static void Patch(Harmony harmony)
        {
            // Patch render stuff for position and rotation
            Type patchType = typeof(RenderPatch);

            // Prefixes
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "RenderPawnAt"),
                prefix: new HarmonyMethod(patchType, nameof(PawnRenderer_RenderPawnAt_Prefix)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts"),
                prefix: new HarmonyMethod(patchType, nameof(PawnRenderer_DrawDynamicParts_Prefix)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal"),
                prefix: new HarmonyMethod(patchType, nameof(PawnRenderer_RenderPawnInternal_Prefix)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "RenderCache"),
                prefix: new HarmonyMethod(patchType, nameof(PawnRenderer_RenderCache_Prefix)));

            // Postfixes
            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "GetBodyPos"),
                postfix: new HarmonyMethod(patchType, nameof(PawnRenderer_GetBodyPos_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "BodyAngle"),
                postfix: new HarmonyMethod(patchType, nameof(PawnRenderer_BodyAngle_Postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(PawnRenderer), "LayingFacing"),
                postfix: new HarmonyMethod(patchType, nameof(PawnRenderer_LayingFacing_Postfix)));
        }

        // PREFIX
        public static void PawnRenderer_RenderPawnAt_Prefix(PawnRenderer __instance, Pawn ___pawn, ref Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            try
            {
                if (___pawn.Dead ||
                    ___pawn.GetPosture() == PawnPosture.Standing ||
                    ___pawn.CurJob == null ||
                    (___pawn.CurJob.def.defName != "LayDown" &&
                    ___pawn.CurJob.def.defName != "Lovin"))
                {
                    renderDatas.Remove(___pawn);
                    return;
                }
                Building_Bed bed = ___pawn.CurrentBed();
                if (bed is null || !HarmonyPatches.Is2DBed(bed.def))
                {
                    renderDatas.Remove(___pawn);
                    return;
                }

                if (!renderDatas.TryGetValue(___pawn, out RenderData renderData))
                {
                    renderDatas[___pawn] = new RenderData();
                    renderData = renderDatas[___pawn];
                }

                int seed = (Find.TickManager.TicksGame + ___pawn.thingIDNumber * 600) / 20000 + ___pawn.thingIDNumber * 600;

                // Facing
                switch (Rand.RangeSeeded(0, 4, seed))
                {
                    case 0:
                        renderData.rot = Rot4.East;
                        break;
                    case 1:
                        renderData.rot = Rot4.West;
                        break;
                    case 2:
                        renderData.rot = Rot4.South;
                        break;
                    case 3:
                        renderData.rot = Rot4.North;
                        break;
                }

                // Angle
                renderData.angle = Rand.RangeSeeded(-180f, 180f, seed + 200);

                // Position
                int slot = bed.GetCurOccupantSlotIndex(___pawn);
                IntVec2 bedSize = bed.def.size;

                const float maxOffset = 0.35f,  // Random placement inward to bed
                    edgeShift = 0.5f,           // Shift the bottom row inward
                    maxEdgeOffset = 0.2f;       // Random placement relative to bed edges
                float minx = -maxOffset,
                    maxx = maxOffset,
                    minz = -maxOffset,
                    maxz = maxOffset;
                bool zIsOne = bedSize.z == 1;
                bool otherFlag = slot / bedSize.x == bedSize.z - 1;
                bool flag = zIsOne || otherFlag;
                bool thirdFlag = (slot + 1) % bedSize.x == 0;
                bool fourthFlag = slot % bedSize.x == 0;
                bool fithFlag = slot/bedSize.x == 0;
                float negativeEdgeMinusMaxEdge = -edgeShift - maxEdgeOffset;
                float negativeEdgePlusMaxEdge = -edgeShift + maxEdgeOffset;
                float edgeShiftMinusMaxEdge = edgeShift - maxEdgeOffset;
                float edgePlusMaxEdge = edgeShift + maxEdgeOffset;
                if (bed.Rotation == Rot4.North)
                {
                    minx = fourthFlag ? -maxEdgeOffset : -maxOffset;
                    maxx = thirdFlag ? maxEdgeOffset : maxOffset;
                    
                    minz = flag ? negativeEdgeMinusMaxEdge : fithFlag ? -maxEdgeOffset : -maxOffset;
                    maxz = flag ? negativeEdgePlusMaxEdge : maxOffset;maxz = -edgeShift + maxEdgeOffset;
                }
                else if (bed.Rotation == Rot4.East)
                {
                    minx = flag ? negativeEdgeMinusMaxEdge : fithFlag? -maxEdgeOffset :minx;
                    maxx = flag ? negativeEdgePlusMaxEdge : maxx;

                    minx = otherFlag? negativeEdgeMinusMaxEdge : minx;
                    maxx = otherFlag ? negativeEdgePlusMaxEdge : maxx;

                    minz = otherFlag ? -maxEdgeOffset : minz;
                    maxz = fourthFlag ? maxEdgeOffset : maxz;
                }
                else if (bed.Rotation == Rot4.South)
                {
                    minx = fourthFlag ? -maxEdgeOffset : minx;

                    maxx = otherFlag? maxEdgeOffset : maxx;

                    minz = flag ? edgeShiftMinusMaxEdge : minz;
                    maxz = flag ? edgePlusMaxEdge : fithFlag? maxEdgeOffset : maxz;

                }
                else // West
                {
                    minx = flag ? edgeShiftMinusMaxEdge : minx;
                    maxx = zIsOne ? edgePlusMaxEdge : fithFlag? maxEdgeOffset:maxx;
                    minz = thirdFlag ? -maxEdgeOffset: minz;
                    maxz = fourthFlag ? maxEdgeOffset : maxz;
                }

                renderData.pos = new Vector3(Rand.RangeSeeded(minx, maxx, seed + 900), 0f, Rand.RangeSeeded(minz, maxz, seed + 1200));

                drawLoc += renderData.pos;
            }
            catch
            {
                renderDatas.Remove(___pawn);
            }
        }

        public static void PawnRenderer_DrawDynamicParts_Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, ref Rot4 pawnRotation, PawnRenderFlags flags, Pawn ___pawn)
        {
            if (renderDatas.TryGetValue(___pawn, out RenderData renderData))
            {
                angle += renderData.angle;
                pawnRotation = renderData.rot ?? pawnRotation;
            }
        }

        public static void PawnRenderer_RenderPawnInternal_Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Pawn ___pawn)
        {
            if (skipPatch)
            {
                skipPatch = false;
                return;
            }
            if (renderDatas.TryGetValue(___pawn, out RenderData renderData))
            {
                angle += renderData.angle;
                bodyFacing = renderData.rot ?? bodyFacing;
            }
        }

        public static bool PawnRenderer_RenderCache_Prefix(PawnRenderer __instance, Pawn ___pawn, Dictionary<Apparel, (Color, bool)> ___tmpOriginalColors, Rot4 rotation, ref float angle, Vector3 positionOffset, bool renderHead, bool renderBody, bool portrait, bool renderHeadgear, bool renderClothes, Dictionary<Apparel, Color> overrideApparelColor = null, Color? overrideHairColor = null, bool stylingStation = false)
        {
            // Lots of game source code just to change 2 lines to stop fix body parts separating
            Vector3 zero = Vector3.zero;
            PawnRenderFlags pawnRenderFlags = (PawnRenderFlags)AccessTools.Method(typeof(PawnRenderer), "GetDefaultRenderFlags").Invoke(__instance, new object[] { ___pawn });
            if (portrait)
            {
                pawnRenderFlags |= PawnRenderFlags.Portrait;
            }
            pawnRenderFlags |= PawnRenderFlags.Cache;
            pawnRenderFlags |= PawnRenderFlags.DrawNow;
            if (!renderHead)
            {
                pawnRenderFlags |= PawnRenderFlags.HeadStump;
            }
            if (renderHeadgear)
            {
                pawnRenderFlags |= PawnRenderFlags.Headgear;
            }
            if (renderClothes)
            {
                pawnRenderFlags |= PawnRenderFlags.Clothes;
            }
            if (stylingStation)
            {
                pawnRenderFlags |= PawnRenderFlags.StylingStation;
            }
            ___tmpOriginalColors.Clear();
            try
            {
                if (overrideApparelColor != null)
                {
                    foreach (KeyValuePair<Apparel, Color> keyValuePair in overrideApparelColor)
                    {
                        Apparel key = keyValuePair.Key;
                        CompColorable compColorable = key.TryGetComp<CompColorable>();
                        if (compColorable != null)
                        {
                            ___tmpOriginalColors.Add(key, new ValueTuple<Color, bool>(compColorable.Color, compColorable.Active));
                            key.SetColor(keyValuePair.Value, true);
                        }
                    }
                }
                Color hairColor = Color.white;
                if (___pawn.story != null)
                {
                    hairColor = ___pawn.story.hairColor;
                    if (overrideHairColor != null)
                    {
                        ___pawn.story.hairColor = overrideHairColor.Value;
                        ___pawn.Drawer.renderer.graphics.CalculateHairMats();
                    }
                }
                skipPatch = true;
                RotDrawMode CurRotDrawMode = Traverse.Create(__instance).Property("CurRotDrawMode").GetValue<RotDrawMode>();
                AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { zero + positionOffset, angle, renderBody, rotation, CurRotDrawMode, pawnRenderFlags });
                foreach (KeyValuePair<Apparel, ValueTuple<Color, bool>> keyValuePair2 in ___tmpOriginalColors)
                {
                    if (!keyValuePair2.Value.Item2)
                    {
                        keyValuePair2.Key.TryGetComp<CompColorable>().Disable();
                    }
                    else
                    {
                        keyValuePair2.Key.SetColor(keyValuePair2.Value.Item1, true);
                    }
                }
                if (___pawn.story != null && overrideHairColor != null)
                {
                    ___pawn.story.hairColor = hairColor;
                    ___pawn.Drawer.renderer.graphics.CalculateHairMats();
                }
            }
            catch (Exception arg)
            {
                Log.Error("Error rendering pawn portrait: " + arg);
            }
            finally
            {
                ___tmpOriginalColors.Clear();
            }
            return false;
        }


        // POSTFIX
        public static void PawnRenderer_GetBodyPos_Postfix(PawnRenderer __instance, ref Vector3 __result, Vector3 drawLoc, ref bool showBody, Pawn ___pawn)
        {
            if (renderDatas.TryGetValue(___pawn, out RenderData renderData))
            {
                __result += renderData.pos;
            }
        }

        public static void PawnRenderer_BodyAngle_Postfix(PawnRenderer __instance, ref float __result, Pawn ___pawn)
        {
            if (renderDatas.TryGetValue(___pawn, out RenderData renderData))
            {
                __result += renderData.angle;
            }
        }

        public static void PawnRenderer_LayingFacing_Postfix(PawnRenderer __instance, ref Rot4 __result, Pawn ___pawn)
        {
            if (renderDatas.TryGetValue(___pawn, out RenderData renderData))
            {
                __result = renderData.rot ?? __result;
            }
        }
    }
}