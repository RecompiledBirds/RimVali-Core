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

		public static new IEnumerable<FloatMenuOption> GetOptionsForTile(int tile, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction)
		{
			bool anything = false;
			if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(pods, tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				anything = true;
				yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
				{
					launchAction(tile, new TransportPodsArrivalAction_FormCaravan("MessageShuttleArrived"));
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
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
		public static string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction, ShipLaunchable launchable)
		{
			if (!target.IsValid){return null;}
			if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, 2147483647) > maxLaunchDistance)
			{
				GUI.color = ColoredText.RedReadable;
			    return "TransportPodDestinationBeyondMaximumRange".Translate();
			}
			IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : ShipLaunchable.GetOptionsForTile(target.Tile, pods, launchAction);
			if (!source.Any<FloatMenuOption>()){return string.Empty;}
			if (source.Count<FloatMenuOption>() == 1)
			{
				if (source.First<FloatMenuOption>().Disabled){GUI.color = ColoredText.RedReadable;}
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
			Log.Message("start test");
			Find.WorldTargeter.BeginTargeting_NewTemp(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, ShipLaunchable.TargeterMouseAttachment, true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, 10000);
			}, (GlobalTargetInfo target) => ShipLaunchable.TargetingLabelGetter(target, tile, 10000, this.TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this), null);
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
		public new List<CompTransporter> TransportersInGroup
		{
			get
			{
				return this.Transporter.TransportersInGroup(parent.Map);
			}
		}
		public new void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
			Log.Message("test launch");
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
			Log.Message("test launch 2");
			if (num > this.MaxLaunchDistance)
			{
				return;
			}
			Log.Message("test launch 3");
			this.Transporter.TryRemoveLord(map);
			int groupID = this.Transporter.groupID;
			float amount = 100;
			//float amount = Mathf.Max(ShipTransport.FuelNeededToLaunchAtDist((float)num), 1f);
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				Log.Message("in launch loop");
				CompTransporter compTransporter = transportersInGroup[i];
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				activeDropPod.Contents = new ActiveDropPodInfo();
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				DropPodLeaving dropPodLeaving = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
				dropPodLeaving.groupID = groupID;
				dropPodLeaving.destinationTile = destinationTile;
				dropPodLeaving.arrivalAction = arrivalAction;
				dropPodLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
				compTransporter.CleanUpLoadingVars(map);
				this.parent.Destroy(DestroyMode.Vanish);
				GenSpawn.Spawn(dropPodLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
			}
			Log.Message("we're here");
			CameraJumper.TryHideWorld();
		}

		
		#region menu options
		private IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile)
		{
			if (this.parent.TryGetComp<CompShuttle>() != null)
			{
				IEnumerable<FloatMenuOption> optionsForTile = ShipLaunchable.GetOptionsForTile(tile, this.TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(this.TryLaunch));
				foreach (FloatMenuOption floatMenuOption in optionsForTile)
				{
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
					this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan());
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
			Log.Message("world targ test");
			return ShipLaunchable.ChoseWorldTarget(target, this.parent.Map.Tile, this.TransportersInGroup.Cast<IThingHolder>(), this.MaxLaunchDistance, new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this);
		}
		public static bool ChoseWorldTarget(GlobalTargetInfo target, int tile, IEnumerable<IThingHolder> pods, int maxLaunchDistance, Action<int, TransportPodsArrivalAction> launchAction, ShipLaunchable launchable)
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
			IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : ShipLaunchable.GetOptionsForTile(target.Tile, pods, launchAction);
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
        private int MaxLaunchDistance
		{
			get
			{
				return 10000;
			}
		}
		public static new int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
		{
			return 100;
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
        public new bool CanTryLaunch
        {
            get
            {
                return true;
            }
        }

		private static readonly Texture2D DismissTex = ContentFinder<Texture2D>.Get("UI/Commands/DismissShuttle", true);
	}
    #endregion

 
}
