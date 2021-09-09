using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimValiCore.Ships
{
    [StaticConstructorOnStartup]
    public static class ShipPatches
    {
        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);

        static ShipPatches()
        {
            Log.Message("[RimVali: Core/Ships & Drones]: Ready for launch!");
        }
    }

    [HarmonyPatch(typeof(CompTransporter), "CompGetGizmosExtra")]
    public static class GizmosPatch
    {
        [HarmonyPostfix]
        public static void Patch(ref IEnumerable<Gizmo> __result, CompTransporter __instance)
        {
            ShipLaunchable shipLaunchable = __instance.parent.TryGetComp<ShipLaunchable>();
            bool isRVCship = shipLaunchable != null;
            if (isRVCship)
            {
                List<Gizmo> gizmos = new List<Gizmo>();

                if (__instance.LoadingInProgressOrReadyToLaunch)
                {
                    if (__instance.Shuttle == null || !__instance.Shuttle.Autoload)
                    {
                        Command_Action action = new Command_Action()
                        {
                            defaultLabel = "CommandCancelLoad".Translate(),
                            defaultDesc = "CommandCancelLoadDesc".Translate(),
                            icon = CompTransporter.CancelLoadCommandTex,
                            action = delegate ()
                            {
                                SoundDefOf.Designate_Cancel.PlayOneShotOnCamera(null);
                                __instance.CancelLoad();
                            }
                        };
                        gizmos.Add(action);
                    }
                }
                else
                {
                    int num = 0;
                    for (int i = 0; i < Find.Selector.NumSelected; i++)
                    {
                        if (Find.Selector.SelectedObjectsListForReading[i] is Thing thing && thing.def == __instance.parent.def)
                        {
                            CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
                            if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
                            {
                                num++;
                            }
                        }
                    }

                    #region launch gizmo

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
                    command_LoadToTransporter.icon = ShipPatches.LoadCommandTex;
                    command_LoadToTransporter.transComp = __instance;

                    command_LoadToTransporter.defaultLabel = "test";
                    gizmos.Add(command_LoadToTransporter);

                    #endregion launch gizmo
                }
                __result = gizmos;
            }
        }
    }
}