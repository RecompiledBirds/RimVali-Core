using Beds2DPatch;
using HarmonyLib;
using RimValiCore.TwoDBedPatchExtension;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace TwoDBedPatch
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		public static bool Is2DBed(ThingDef thingDef)
		{
			return thingDef.HasComp(typeof(BedComp));
		}

		static HarmonyPatches()
		{
			Log.Message("[2D Beds] Patching started");
			Type patchType = typeof(HarmonyPatches);
			Harmony harmony = new Harmony("Fluxic.Beds2DPatch");

			harmony.Patch(
				AccessTools.PropertyGetter(typeof(Building_Bed), nameof(Building_Bed.SleepingSlotsCount)),
				prefix: new HarmonyMethod(patchType, nameof(Building_Bed_SleepingSlotsCount_Prefix)));

			harmony.Patch(
				AccessTools.Method(typeof(Building_Bed), nameof(Building_Bed.GetSleepingSlotPos)),
				prefix: new HarmonyMethod(patchType, nameof(Building_Bed_GetSleepingSlotPos_Prefix)));

			harmony.Patch(
				AccessTools.Method(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.CanPotentiallyLinkTo_Static), new Type[] { typeof(ThingDef), typeof(IntVec3), typeof(Rot4), typeof(ThingDef), typeof(IntVec3), typeof(Rot4) }),
				prefix: new HarmonyMethod(patchType, nameof(CompAffectedByFacilities_CanPotentiallyLinkTo_Static_Prefix)));

			harmony.Patch(
				AccessTools.Method(typeof(CompProperties_AssignableToPawn), nameof(CompProperties_AssignableToPawn.PostLoadSpecial)),
				prefix: new HarmonyMethod(patchType, nameof(CompProperties_AssignableToPawn_PostLoadSpecial_Prefix)));

			Log.Message("[2D Beds] Patching rendering methods");
			RenderPatch.Patch(harmony);

			Log.Message("[2D Beds] Patching success");

			//PawnCacheRenderer.csc -> RenderPawn
			//Pawn.cs -> DrawAt
			//PawnRenderer.cs -> GetBodyPos (has Building_Bed)
		}

		public static int Bed2D_SleepingSlotsCount(IntVec2 size) => size.x * size.z;

		public static IntVec3 Bed2D_GetSleepingSlotPos(int index, IntVec3 bedCenter, Rot4 bedRot, IntVec2 bedSize)
		{
			int sleepingSlotsCount = Bed2D_SleepingSlotsCount(bedSize);
			if (index < 0 || index >= sleepingSlotsCount)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to get sleeping slot pos with index ",
					index,
					", but there are only ",
					sleepingSlotsCount,
					" sleeping slots available."
				}));
				return bedCenter;
			}
			CellRect cellRect = GenAdj.OccupiedRect(bedCenter, bedRot, bedSize);
			
			return bedRot == Rot4.North ? new IntVec3(cellRect.minX + index % bedSize.x, bedCenter.y, cellRect.minZ + index / bedSize.x) : 
				bedRot == Rot4.East ? new IntVec3(cellRect.minX + index / bedSize.x, bedCenter.y, cellRect.maxZ - index % bedSize.x) : 
				bedRot == Rot4.South ? new IntVec3(cellRect.minX + index % bedSize.x, bedCenter.y, cellRect.maxZ - index / bedSize.x) : new IntVec3(cellRect.maxX - index / bedSize.x, bedCenter.y, cellRect.maxZ - index % bedSize.x);

		}

		// PATCHES
		// PREFIXES
		public static bool Building_Bed_SleepingSlotsCount_Prefix(ref int __result, ref Building_Bed __instance)
		{
			if (!Is2DBed(__instance.def))
				return true;

			__result = Bed2D_SleepingSlotsCount(__instance.def.size);
			return false;
		}

		public static bool Building_Bed_GetSleepingSlotPos_Prefix(ref IntVec3 __result, ref Building_Bed __instance, int index)
		{
			if (!Is2DBed(__instance.def))
				return true;

			__result = Bed2D_GetSleepingSlotPos(index, __instance.Position, __instance.Rotation, __instance.def.size);
			return false;
		}

		public static bool CompAffectedByFacilities_CanPotentiallyLinkTo_Static_Prefix(ref bool __result, ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot, ThingDef myDef, IntVec3 myPos, Rot4 myRot)
		{
			if (!Is2DBed(myDef))
            {
				return true;
            }

			CompProperties_Facility compProperties = facilityDef.GetCompProperties<CompProperties_Facility>();
			if (compProperties.mustBePlacedAdjacent)
			{
				CellRect rect = GenAdj.OccupiedRect(myPos, myRot, myDef.size);
				CellRect rect2 = GenAdj.OccupiedRect(facilityPos, facilityRot, facilityDef.size);
				if (!GenAdj.AdjacentTo8WayOrInside(rect, rect2))
				{
					__result = false;
					return false;
				}
			}
			if (compProperties.mustBePlacedAdjacentCardinalToBedHead || compProperties.mustBePlacedAdjacentCardinalToAndFacingBedHead)
			{
				CellRect other = GenAdj.OccupiedRect(facilityPos, facilityRot, facilityDef.size);
				bool flag = false;
				// Patch
				int sleepingSlotsCount = Bed2D_SleepingSlotsCount(myDef.size);
				for (int i = 0; i < sleepingSlotsCount; i++)
				{
					// Patch
					IntVec3 sleepingSlotPos = Bed2D_GetSleepingSlotPos(i, myPos, myRot, myDef.size);
					flag = sleepingSlotPos.IsAdjacentToCardinalOrInside(other) && compProperties.mustBePlacedAdjacentCardinalToAndFacingBedHead ? other.MovedBy(facilityRot.FacingCell).Contains(sleepingSlotPos) : true;
				}
				if (!flag)
				{
					__result = false;
					return false;
				}
			}
			if (!compProperties.mustBePlacedAdjacent && !compProperties.mustBePlacedAdjacentCardinalToBedHead && !compProperties.mustBePlacedAdjacentCardinalToAndFacingBedHead)
			{
				Vector3 a = GenThing.TrueCenter(myPos, myRot, myDef.size, myDef.Altitude);
				Vector3 b = GenThing.TrueCenter(facilityPos, facilityRot, facilityDef.size, facilityDef.Altitude);
				if (Vector3.Distance(a, b) > compProperties.maxDistance)
				{
					__result = false;
					return false;
				}
			}
			__result = true;
			return false;
		}

		public static bool CompProperties_AssignableToPawn_PostLoadSpecial_Prefix(ref CompProperties_AssignableToPawn __instance, ThingDef parent)
        {
			if (!Is2DBed(parent))
            {
				return true;
            }
			__instance.maxAssignedPawnsCount = Bed2D_SleepingSlotsCount(parent.size);
			return false;
		}
    }
}