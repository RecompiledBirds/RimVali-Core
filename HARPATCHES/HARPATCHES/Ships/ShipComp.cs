using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimValiCore.Ships
{
    #region caravan maker
	public class ShipArrivalAction_MakeCaravan : TransportPodsArrivalAction_FormCaravan
    {
		public ShipArrivalAction_MakeCaravan(Thing pod, string arrivalMsg)
        {
			this.pod = pod;
			this.arrivalMessageKey = arrivalMsg;
        }
		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
			tmpPawns.Clear();
			for (int i = 0; i < pods.Count; i++)
			{
				ThingOwner innerContainer = pods[i].innerContainer;
				for (int j = innerContainer.Count - 1; j >= 0; j--)
				{
					Pawn pawn = innerContainer[j] as Pawn;
					if (pawn != null)
					{
						tmpPawns.Add(pawn);
						innerContainer.Remove(pawn);
					}
				}
			}
			int startingTile;
			if (!GenWorldClosest.TryFindClosestPassableTile(tile, out startingTile))
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
			Messages.Message(this.arrivalMessageKey.Translate(), caravan, MessageTypeDefOf.TaskCompletion, true);
		}
		private static List<Pawn> tmpPawns = new List<Pawn>();

		// Token: 0x04007CF4 RID: 31988
		private static List<Thing> tmpContainedThings = new List<Thing>();
		private string arrivalMessageKey = "MessageTransportPodsArrived";
		Thing pod;
	}
    #endregion
    #region launchable
    public class ShipLauncherProps : CompProperties_Launchable
    {
        public bool mustBeConnectedToFuelingPort = false;
        public ShipLauncherProps()
        {
			
            this.compClass = typeof(ShipLaunchable);
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
					launchAction(tile, new ShipArrivalAction_MakeCaravan(this.parent,"MessageShuttleArrived"));
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
			
			if (!target.IsValid){return null;}
			if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, 2147483647) > maxLaunchDistance)
			{
				GUI.color = Color.red;
			    return "TransportPodDestinationBeyondMaximumRange".Translate();
			}
			IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : GetOptionsForTile(target.Tile, pods, launchAction);
			if (!source.Any<FloatMenuOption>()){return string.Empty;}
			if (source.Count<FloatMenuOption>() == 1)
			{
				if (source.First<FloatMenuOption>().Disabled){GUI.color = Color.red;}
				return source.First<FloatMenuOption>().Label;
			}
			MapParent mapParent=target.WorldObject as MapParent;
			if (mapParent != null){return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);}
			return "ClickToSeeAvailableOrders_Empty".Translate();
		}
        #endregion
        public new void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
			Find.WorldSelector.ClearSelection();
			int tile = this.parent.Map.Tile;
			Log.Message("picking destination");
			Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, ShipLaunchable.TargeterMouseAttachment, true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile,MaxLaunchDistance);
			}, (GlobalTargetInfo target) => TargetingLabelGetter(target, tile, MaxLaunchDistance, this.TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this), null);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			
			if (this.LoadingInProgressOrReadyToLaunch && this.CanTryLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandLaunchGroup".Translate();
				command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
				command_Action.icon = ShipLaunchable.LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;

				command_Action.action = delegate ()
				{
					if (this.AnyInGroupHasAnythingLeftToLoad)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(this.FirstThingLeftToLoadInGroup.LabelCapNoCount, this.FirstThingLeftToLoadInGroup), new Action(this.StartChoosingDestination), false, null));
						return;
					}
					
					StartChoosingDestination();
				};
				if (!this.AllInGroupConnectedToFuelingPort)
				{
					command_Action.Disable("CommandLaunchGroupFailNotConnectedToFuelingPort".Translate());
				}
				else if (!this.AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					command_Action.Disable("CommandLaunchGroupFailNoFuel".Translate());
				}
				else if (this.AnyInGroupIsUnderRoof)
				{
					command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
				}
				yield return command_Action;
			}
			yield break;
		}

		public new List<CompTransporter> TransportersInGroup
		{

			get
			{
				return this.Transporter.TransportersInGroup(parent.Map);
			}
		}
		public new void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
		;
			if (!parent.Spawned)
			{
				Log.Error("Tried to launch " + parent + ", but it's unspawned.", false);
				return;
			}
			List<CompTransporter> transportersInGroup = TransportersInGroup;
			if (transportersInGroup == null)
			{
				Log.Error("Tried to launch " + parent + ", but it's not in any group.", false);
				return;
			}
			if (!this.LoadingInProgressOrReadyToLaunch || !AllInGroupConnectedToFuelingPort || !AllFuelingPortSourcesInGroupHaveAnyFuel){return;}
			Map map = parent.Map;
			int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);

			if (num > this.MaxLaunchDistance)
			{
				return;
			}
			this.Transporter.TryRemoveLord(map);
			int groupID = this.Transporter.groupID;
			float amount = Mathf.Max(FuelNeededToLaunchAtDist((float)num), 1f);
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				CompTransporter compTransporter = transportersInGroup[i];
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				activeDropPod.Contents = new ActiveDropPodInfo();
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				FlyShipLeaving dropPodLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
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
			if (this.parent.TryGetComp<CompShuttle>() != null)
			{
				IEnumerable<FloatMenuOption> optionsForTile = GetOptionsForTile(tile, this.TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(this.TryLaunch));
				foreach (FloatMenuOption floatMenuOption in optionsForTile)
				{
					Log.Message(floatMenuOption.Label);
					yield return floatMenuOption;
				}
				
				yield break;
			}
			bool anything = false;
			if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.TransportersInGroup.Cast<IThingHolder>(), tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				anything = true;
				yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
				{
					this.TryLaunch(tile, new ShipArrivalAction_MakeCaravan(this.parent, "MessageShuttleArrived"));
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
			int num;
			for (int i = 0; i < worldObjects.Count; i = num + 1)
			{
				if (worldObjects[i].Tile == tile)
				{
					foreach (FloatMenuOption floatMenuOption2 in worldObjects[i].GetTransportPodsFloatMenuOptions(this.TransportersInGroup.Cast<IThingHolder>(), this))
					{
						anything = true;
						yield return floatMenuOption2;
						Log.Message("loop2: " +floatMenuOption2.Label);
					}
				
				}
				num = i;
			}
			if (!anything && !Find.World.Impassable(tile))
			{
				yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate ()
				{
					this.TryLaunch(tile, null);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
			}
			yield break;
		}
        #endregion

        #region picking world targ

        private bool ChoseWorldTarget(GlobalTargetInfo target)
		{

			return ChoseWorldTarget(target, this.parent.Map.Tile, this.TransportersInGroup.Cast<IThingHolder>(), this.MaxLaunchDistance, new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this);
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
			if (!source.Any<FloatMenuOption>())
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
				if (source.Count<FloatMenuOption>() != 1)
				{
					Find.WindowStack.Add(new FloatMenu(source.ToList<FloatMenuOption>()));
					return false;
				}
				if (!source.First<FloatMenuOption>().Disabled)
				{
					source.First<FloatMenuOption>().action();
					return true;
				}
				return false;
			}
		}
		#endregion



		public new bool AllFuelingPortSourcesInGroupHaveAnyFuel
		{
			get
			{
				return true;
			}
		}
		public new bool AllInGroupConnectedToFuelingPort
		{
			get
			{
				return true;
			}
		}

		private int MaxLaunchDistance
		{
			get
			{
				return Props.fixedLaunchDistanceMax;
			}
		}
		public static new int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
		{
			int range = Mathf.FloorToInt(fuelLevel / 2.25f);
			return range;
		}


		public new ShipLauncherProps Props
        {
            get
            {
                return this.props as ShipLauncherProps;
            }
        }
        public new bool ConnectedToFuelingPort
        {
            get
            {
                return !Props.mustBeConnectedToFuelingPort || (!this.Props.requireFuel || this.FuelingPortSource != null);
            }
        }


		public new Building FuelingPortSource
		{
			get
			{
				return this.parent.TryGetComp<CompRefuelable>()!= null ? this.parent as Building : FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(this.parent.Position, this.parent.Map);
			}
		}



		public new bool CanTryLaunch
        {
            get
            {
				CompShuttle compShuttle = this.parent.TryGetComp<CompShuttle>();
				return compShuttle == null || ((compShuttle.permitShuttle) && this.Transporter.innerContainer.Any<Thing>());
			}
        }

		private static readonly Texture2D DismissTex = ContentFinder<Texture2D>.Get("UI/Commands/DismissShuttle", true);
	}
    #endregion

 
}
