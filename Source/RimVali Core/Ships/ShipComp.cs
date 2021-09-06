using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimValiCore.Ships
{
    #region caravan maker

    public class ShipArrivalAction_MakeCaravan : TransportPodsArrivalAction_FormCaravan
    {
        public ShipArrivalAction_MakeCaravan(Thing pod, string arrivalMsg)
        {
            this.pod = pod;
            arrivalMessageKey = arrivalMsg;
        }

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            tmpPawns.Clear();
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner innerContainer = pods[i].innerContainer;
                for (int j = innerContainer.Count - 1; j >= 0; j--)
                {
                    if (innerContainer[j] is Pawn pawn)
                    {
                        tmpPawns.Add(pawn);
                        innerContainer.Remove(pawn);
                    }
                }
            }
            if (!GenWorldClosest.TryFindClosestPassableTile(tile, out int startingTile))
            {
                startingTile = tile;
            }
            Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, Faction.OfPlayer, startingTile, true);
            for (int k = 0; k < pods.Count; k++)
            {
                tmpContainedThings.Clear();
                tmpContainedThings.AddRange(pods[k].innerContainer);
                for (int l = 0; l < tmpContainedThings.Count; l++)
                {
                    pods[k].innerContainer.Remove(tmpContainedThings[l]);
                    CaravanInventoryUtility.GiveThing(caravan, tmpContainedThings[l]);
                }
            }
            Log.Message(pod.def.defName);
            //pod.TryGetComp<CompTransporter>().innerContainer.RemoveAll(x=>true);
            CaravanInventoryUtility.GiveThing(caravan, pod);
            tmpPawns.Clear();
            tmpContainedThings.Clear();
            Messages.Message(arrivalMessageKey.Translate(), caravan, MessageTypeDefOf.TaskCompletion, true);
        }

        private static readonly List<Pawn> tmpPawns = new List<Pawn>();

        private static readonly List<Thing> tmpContainedThings = new List<Thing>();

        private readonly string arrivalMessageKey = "MessageTransportPodsArrived";
        private readonly Thing pod;
    }

    #endregion caravan maker

    #region launchable

    public class ShipLauncherProps : CompProperties_Launchable
    {
        public bool mustBeConnectedToFuelingPort = false;

        public ShipLauncherProps()
        {
            compClass = typeof(ShipLaunchable);
        }
    }

    public class ShipLaunchable : CompLaunchable
    {
        public new IEnumerable<FloatMenuOption> GetOptionsForTile(int tile, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction)
        {
            bool anything = false;
            if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(pods, tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
            {
                anything = true;
                Log.Message("making caravan");
                yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
                {
                    launchAction(tile, new ShipArrivalAction_MakeCaravan(parent, "MessageShuttleArrived"));
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
                FuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(100);
            }
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            int num;
            for (int i = 0; i < worldObjects.Count; i = num + 1)
            {
                if (worldObjects[i].Tile == tile)
                {
                    foreach (FloatMenuOption floatMenuOption in worldObjects[i].GetShuttleFloatMenuOptions(pods, launchAction))
                    {
                        anything = true;
                        yield return floatMenuOption;
                    }
                }
                num = i;
            }
            if (!anything && !Find.World.Impassable(tile))
            {
                yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate ()
                {
                    launchAction(tile, null);
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            yield break;
        }

        #region target labeler

        public string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction, ShipLaunchable launchable)
        {
            if (!target.IsValid) { return null; }
            if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, 2147483647) > maxLaunchDistance)
            {
                GUI.color = Color.red;
                return "TransportPodDestinationBeyondMaximumRange".Translate();
            }
            IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : GetOptionsForTile(target.Tile, pods, launchAction);
            if (!source.Any()) { return string.Empty; }
            if (source.Count() == 1)
            {
                if (source.First().Disabled) { GUI.color = Color.red; }
                return source.First().Label;
            }
            if (target.WorldObject is MapParent mapParent) { return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap); }
            return "ClickToSeeAvailableOrders_Empty".Translate();
        }

        #endregion target labeler

        public new void StartChoosingDestination()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            int tile = parent.Map.Tile;
            Log.Message("picking destination");
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(ChoseWorldTarget), true, TargeterMouseAttachment, true, delegate
            {
                GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
            }, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, MaxLaunchDistance, TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(TryLaunch), this), null);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (LoadingInProgressOrReadyToLaunch && CanTryLaunch)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "CommandLaunchGroup".Translate(),
                    defaultDesc = "CommandLaunchGroupDesc".Translate(),
                    icon = LaunchCommandTex,
                    alsoClickIfOtherInGroupClicked = false,

                    action = delegate ()
                    {
                        if (AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(FirstThingLeftToLoadInGroup.LabelCapNoCount, FirstThingLeftToLoadInGroup), new Action(StartChoosingDestination), false, null));
                            return;
                        }

                        StartChoosingDestination();
                    }
                };
                if (!AllInGroupConnectedToFuelingPort)
                {
                    command_Action.Disable("CommandLaunchGroupFailNotConnectedToFuelingPort".Translate());
                }
                else if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
                {
                    command_Action.Disable("CommandLaunchGroupFailNoFuel".Translate());
                }
                else if (AnyInGroupIsUnderRoof)
                {
                    command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                }
                yield return command_Action;
            }
            yield break;
        }

        public new List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

        public new void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
        {
            ;
            if (!parent.Spawned)
            {
                Log.Error("Tried to launch " + parent + ", but it's unspawned.");
                return;
            }
            List<CompTransporter> transportersInGroup = TransportersInGroup;
            if (transportersInGroup == null)
            {
                Log.Error("Tried to launch " + parent + ", but it's not in any group.");
                return;
            }
            if (!LoadingInProgressOrReadyToLaunch || !AllInGroupConnectedToFuelingPort || !AllFuelingPortSourcesInGroupHaveAnyFuel) { return; }
            Map map = parent.Map;
            int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile);

            if (num > MaxLaunchDistance)
            {
                return;
            }
            Transporter.TryRemoveLord(map);
            int groupID = Transporter.groupID;
            // float amount = Mathf.Max(FuelNeededToLaunchAtDist(num), 1f);
            for (int i = 0; i < transportersInGroup.Count; i++)
            {
                CompTransporter compTransporter = transportersInGroup[i];
                ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
                ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
                activeDropPod.Contents = new ActiveDropPodInfo();
                activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
                FlyShipLeaving dropPodLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
                dropPodLeaving.groupID = groupID;
                dropPodLeaving.destinationTile = destinationTile;
                dropPodLeaving.arrivalAction = arrivalAction;
                dropPodLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
                compTransporter.CleanUpLoadingVars(map);
                parent.DeSpawn(DestroyMode.Vanish);
                GenSpawn.Spawn(dropPodLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
            }
            CameraJumper.TryHideWorld();
        }

        #region menu options

        private IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile)
        {
            Log.Message("getting options");
            if (parent.TryGetComp<CompShuttle>() != null)
            {
                IEnumerable<FloatMenuOption> optionsForTile = GetOptionsForTile(tile, TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(TryLaunch));
                foreach (FloatMenuOption floatMenuOption in optionsForTile)
                {
                    Log.Message(floatMenuOption.Label);
                    yield return floatMenuOption;
                }

                yield break;
            }
            bool anything = false;
            if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(TransportersInGroup.Cast<IThingHolder>(), tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
            {
                anything = true;
                yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
                {
                    TryLaunch(tile, new ShipArrivalAction_MakeCaravan(parent, "MessageShuttleArrived"));
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            int num;
            for (int i = 0; i < worldObjects.Count; i = num + 1)
            {
                if (worldObjects[i].Tile == tile)
                {
                    foreach (FloatMenuOption floatMenuOption2 in worldObjects[i].GetTransportPodsFloatMenuOptions(TransportersInGroup.Cast<IThingHolder>(), this))
                    {
                        anything = true;
                        yield return floatMenuOption2;
                        Log.Message("loop2: " + floatMenuOption2.Label);
                    }
                }
                num = i;
            }
            if (!anything && !Find.World.Impassable(tile))
            {
                yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate ()
                {
                    TryLaunch(tile, null);
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            yield break;
        }

        #endregion menu options

        #region picking world targ

        public bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            return ChoseWorldTarget(target, parent.Map.Tile, TransportersInGroup.Cast<IThingHolder>(), MaxLaunchDistance, new Action<int, TransportPodsArrivalAction>(TryLaunch), this);
        }

        public bool ChoseWorldTarget(GlobalTargetInfo target, int tile, IEnumerable<IThingHolder> pods, int maxLaunchDistance, Action<int, TransportPodsArrivalAction> launchAction, ShipLaunchable launchable)
        {
            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            //if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, 2147483647) > 100)
            {
                //Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, false);
                //return false;
            }
            IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : GetOptionsForTile(target.Tile, pods, launchAction);
            if (!source.Any())
            {
                if (Find.World.Impassable(target.Tile))
                {
                    Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                launchAction(target.Tile, null);
                return true;
            }
            else
            {
                if (source.Count() != 1)
                {
                    Find.WindowStack.Add(new FloatMenu(source.ToList()));
                    return false;
                }
                if (!source.First().Disabled)
                {
                    source.First().action();
                    return true;
                }
                return false;
            }
        }

        #endregion picking world targ

        public new bool AllFuelingPortSourcesInGroupHaveAnyFuel => true;
        public new bool AllInGroupConnectedToFuelingPort => true;

        private new int MaxLaunchDistance => Props.fixedLaunchDistanceMax;

        public static new int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
        {
            int range = Mathf.FloorToInt(fuelLevel / 2.25f);
            return range;
        }

        public new ShipLauncherProps Props => props as ShipLauncherProps;
        public new bool ConnectedToFuelingPort => !Props.mustBeConnectedToFuelingPort || (!Props.requireFuel || FuelingPortSource != null);

        public new Building FuelingPortSource => parent.TryGetComp<CompRefuelable>() != null ? parent as Building : FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map);

        public bool CanTryLaunch
        {
            get
            {
                CompShuttle compShuttle = parent.TryGetComp<CompShuttle>();
                return compShuttle == null || ((compShuttle.permitShuttle) && Transporter.innerContainer.Any());
            }
        }

        public static readonly Texture2D DismissTex = ContentFinder<Texture2D>.Get("UI/Commands/DismissShuttle", true);
    }

    #endregion launchable
}