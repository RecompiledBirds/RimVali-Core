using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimValiCore.Ships;
using RimWorld;
using UnityEngine;
using Verse;
namespace RimValiCore.Ships
{
    [StaticConstructorOnStartup]
    public static class shipPatches
    {
		public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);
		static shipPatches()
        {
           // Harmony rVC_Ships_Harmony = new Harmony("RimValiCore.Ships");
            try
            {
                Log.Message("[RimVali: Core/Ships & Drones]: Ready for launch!");
               // rVC_Ships_Harmony.PatchAll();
              //  Log.Message($"[RimVali: Core/Ships]: Patched {rVC_Ships_Harmony.GetPatchedMethods().Count()} methods succesfully!");
            }catch(Exception e)
            {
              //  Log.Error($"[RimVali: Core/Ships]: Patches failed!!! Patched {rVC_Ships_Harmony.GetPatchedMethods().Count()} methods. \n Error: {e}");
            }
        }
    }

    [HarmonyPatch(typeof(CompTransporter), "CompGetGizmosExtra")]
    public static class GizmosPatch
    {
        [HarmonyPostfix]
        public static void patch(ref IEnumerable<Gizmo> __result, CompTransporter __instance)
        {
            ShipLaunchable shipLaunchable = __instance.parent.TryGetComp<ShipLaunchable>();
            bool isRVCship = shipLaunchable != null;
            if (isRVCship)
            {
                List<Gizmo> gizmos = __result.ToList();
                for(int a =0; a<gizmos.Count; a++)
                {

                    bool isAction = gizmos[a] is Command_LoadToTransporter;
                    
                    if (isAction)
                    {
						int num = 0;
						for (int i = 0; i < Find.Selector.NumSelected; i++)
						{
							Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
							if (thing != null && thing.def == __instance.parent.def)
							{
								CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
								if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
								{
									num++;
								}
							}
						}
						Command_LoadToTransporter action = gizmos[a] as Command_LoadToTransporter;
						if ((action.defaultLabel == "CommandLoadTransporter".Translate(num.ToString())))
						{
							Command_LoadToTransporter command_LoadToTransporter = new Command_LoadToTransporter();
							if (__instance.Props.max1PerGroup)
							{
								if (__instance.Props.canChangeAssignedThingsAfterStarting)
								{
									command_LoadToTransporter.defaultLabel = "CommandSetToLoadTransporter".Translate();
									command_LoadToTransporter.defaultDesc = "CommandSetToLoadTransporterDesc".Translate();
								}
								else
								{
									command_LoadToTransporter.defaultLabel = "CommandLoadTransporterSingle".Translate();
									command_LoadToTransporter.defaultDesc = "CommandLoadTransporterSingleDesc".Translate();
								}
							}
							else
							{
								
								command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate(num.ToString());
								command_LoadToTransporter.defaultDesc = "CommandLoadTransporterDesc".Translate();
							}
							command_LoadToTransporter.icon = shipPatches.LoadCommandTex;
							command_LoadToTransporter.transComp = __instance;


							if (false)
							{
								command_LoadToTransporter.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
							}
							else if (false)
							{
								command_LoadToTransporter.Disable("CommandLoadTransporterFailNoFuel".Translate());
							}
							command_LoadToTransporter.defaultLabel = "test";
							gizmos.Add(command_LoadToTransporter);
							gizmos.Remove(gizmos[a]);
						}
						

					}
					
				}
				__result = gizmos;
			}
		
        }
    }
}
