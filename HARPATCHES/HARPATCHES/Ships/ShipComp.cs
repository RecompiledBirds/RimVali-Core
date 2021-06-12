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
					IEnumerator<FloatMenuOption> enumerator = null;
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
			yield break;
		}
		#region target labeler
		public static string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction, ShipLaunchable launchable)
		{
			if (!target.IsValid)
			{
				return null;
			}
			if (Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, 2147483647) > maxLaunchDistance)
			{
				GUI.color = ColoredText.RedReadable;
			    return "TransportPodDestinationBeyondMaximumRange".Translate();
			}
			IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : ShipLaunchable.GetOptionsForTile(target.Tile, pods, launchAction);
			if (!source.Any<FloatMenuOption>())
			{
				return string.Empty;
			}
			if (source.Count<FloatMenuOption>() == 1)
			{
				if (source.First<FloatMenuOption>().Disabled)
				{
					GUI.color = ColoredText.RedReadable;
				}
				return source.First<FloatMenuOption>().Label;
			}
			MapParent mapParent;
			if ((mapParent = (target.WorldObject as MapParent)) != null)
			{
				return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
			}
			return "ClickToSeeAvailableOrders_Empty".Translate();
		}
        #endregion
        public new void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
			Find.WorldSelector.ClearSelection();
			int tile = this.parent.Map.Tile;
			Find.WorldTargeter.BeginTargeting_NewTemp(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, ShipLaunchable.TargeterMouseAttachment, true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, 10000);
			}, (GlobalTargetInfo target) => ShipLaunchable.TargetingLabelGetter(target, tile, 10000, this.TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this), null);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			IEnumerator<Gizmo> enumerator = null;
			CompShuttle shuttleComp = this.parent.TryGetComp<CompShuttle>();
			if (this.LoadingInProgressOrReadyToLaunch && this.CanTryLaunch)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandLaunchGroup".Translate();
				command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
				command_Action.icon = ShipLaunchable.LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				if (shuttleComp != null && shuttleComp.IsMissionShuttle && !shuttleComp.AllRequiredThingsLoaded)
				{
					command_Action.Disable("ShuttleRequiredItemsNotSatisfied".Translate());
				}
				command_Action.action = delegate ()
				{
					if (this.AnyInGroupHasAnythingLeftToLoad)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(this.FirstThingLeftToLoadInGroup.LabelCapNoCount, this.FirstThingLeftToLoadInGroup), new Action(this.StartChoosingDestination), false, null));
						return;
					}
					if (shuttleComp != null && shuttleComp.IsMissionShuttle)
					{
						TransportPodsArrivalAction_Shuttle transportPodsArrivalAction_Shuttle = new TransportPodsArrivalAction_Shuttle((MapParent)shuttleComp.missionShuttleTarget);
						transportPodsArrivalAction_Shuttle.missionShuttleHome = shuttleComp.missionShuttleHome;
						transportPodsArrivalAction_Shuttle.missionShuttleTarget = shuttleComp.missionShuttleTarget;
						transportPodsArrivalAction_Shuttle.sendAwayIfQuestFinished = shuttleComp.sendAwayIfQuestFinished;
						transportPodsArrivalAction_Shuttle.questTags = this.parent.questTags;
						this.TryLaunch((this.parent.Tile == shuttleComp.missionShuttleTarget.Tile) ? shuttleComp.missionShuttleHome.Tile : shuttleComp.missionShuttleTarget.Tile, transportPodsArrivalAction_Shuttle);
						return;
					}
					this.StartChoosingDestination();
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
			if (shuttleComp != null && shuttleComp.permitShuttle)
			{
				Command_Action command_Action2 = new Command_Action
				{
					defaultLabel = "CommandShuttleDismiss".Translate(),
					defaultDesc = "CommandShuttleDismissDesc".Translate(),
					icon = ShipLaunchable.DismissTex,
					alsoClickIfOtherInGroupClicked = false,
					action = delegate ()
					{
						this.Transporter.innerContainer.TryDropAll(this.parent.Position, this.parent.Map, ThingPlaceMode.Near, null, null);
						if (!this.LoadingInProgressOrReadyToLaunch)
						{
							TransporterUtility.InitiateLoading(Gen.YieldSingle<ShipTransport>(this.Transporter));
						}
						shuttleComp.Send();
					}
				};
				yield return command_Action2;
			}
			yield break;
		}

		public new List<ShipTransport> TransportersInGroup
		{
			get
			{
				return this.Transporter.TransportersInGroup(this.parent.Map);
			}
		}
		public new void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
			if (!this.parent.Spawned)
			{
				Log.Error("Tried to launch " + this.parent + ", but it's unspawned.", false);
				return;
			}
			List<ShipTransport> transportersInGroup = this.TransportersInGroup;
			if (transportersInGroup == null)
			{
				Log.Error("Tried to launch " + this.parent + ", but it's not in any group.", false);
				return;
			}
			if (!this.LoadingInProgressOrReadyToLaunch || !this.AllInGroupConnectedToFuelingPort || !this.AllFuelingPortSourcesInGroupHaveAnyFuel)
			{
				return;
			}
			Map map = this.parent.Map;
			int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);
			CompShuttle compShuttle = this.parent.TryGetComp<CompShuttle>();
			if (num > this.MaxLaunchDistance && (compShuttle == null || !compShuttle.IsMissionShuttle))
			{
				return;
			}
			this.Transporter.TryRemoveLord(map);
			int groupID = this.Transporter.groupID;
			float amount = Mathf.Max(ShipTransport.FuelNeededToLaunchAtDist((float)num), 1f);
			Log.Error("test");
			for (int i = 0; i < transportersInGroup.Count; i++)
			{
				ShipTransport compTransporter = transportersInGroup[i];
				ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
				ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
				activeDropPod.Contents = new ActiveDropPodInfo();
				activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
				DropPodLeaving dropPodLeaving = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(this.Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeDropPod);
				dropPodLeaving.groupID = groupID;
				dropPodLeaving.destinationTile = destinationTile;
				dropPodLeaving.arrivalAction = arrivalAction;
				dropPodLeaving.worldObjectDef = ((compShuttle != null) ? WorldObjectDefOf.TravelingShuttle : WorldObjectDefOf.TravelingTransportPods);
				compTransporter.CleanUpLoadingVars(map);
				this.parent.Destroy(DestroyMode.Vanish);
				GenSpawn.Spawn(dropPodLeaving, compTransporter.parent.Position, map, WipeMode.Vanish);
			}
			CameraJumper.TryHideWorld();
		}

		


		public new ShipTransport Transporter
        {
            get
            {
				return this.parent.GetComp<ShipTransport>();
            }
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
				IEnumerator<FloatMenuOption> enumerator = null;
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
					IEnumerator<FloatMenuOption> enumerator = null;
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
			return !this.LoadingInProgressOrReadyToLaunch || ShipLaunchable.ChoseWorldTarget(target, this.parent.Map.Tile, this.TransportersInGroup.Cast<IThingHolder>(), this.MaxLaunchDistance, new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this);
		}
		public static bool ChoseWorldTarget(GlobalTargetInfo target, int tile, IEnumerable<IThingHolder> pods, int maxLaunchDistance, Action<int, TransportPodsArrivalAction> launchAction, ShipLaunchable launchable)
		{
			if (!target.IsValid)
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
				return false;
			}
			return true;
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

    #region transporter
    public class ShipTransporterProps : CompProperties_Transporter
    {
        public ShipTransporterProps()
        {
            this.compClass = typeof(ShipTransport);
        }
    }
    public class ShipTransport : CompTransporter
    {
		public static void GetTransportersInGroup(int transportersGroup, Map map, List<ShipTransport> outTransporters)
		{
			outTransporters.Clear();
			if (transportersGroup < 0)
			{
				return;
			}
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
			for (int i = 0; i < list.Count; i++)
			{
				ShipTransport compTransporter = list[i].TryGetComp<ShipTransport>();
				if (compTransporter.groupID == transportersGroup)
				{
					outTransporters.Add(compTransporter);
				}
			}
		}

		public new List<ShipTransport> TransportersInGroup(Map map)
		{
			if (!this.LoadingInProgressOrReadyToLaunch)
			{
				return null;
			}
			GetTransportersInGroup(this.groupID, map, ShipTransport.tmpTransportersInGroup);
			return ShipTransport.tmpTransportersInGroup;
		}
		private static List<ShipTransport> tmpTransportersInGroup = new List<ShipTransport>();

		new ShipLaunchable Launchable
        {
            get
            {
                return this.parent.GetComp<ShipLaunchable>();
            }
        }
		private static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter", true);

		// Token: 0x04005945 RID: 22853
		private static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters", true);

		// Token: 0x04005946 RID: 22854
		private static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter", true);
		private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);


		private void SelectPreviousInGroup()
		{
			List<ShipTransport> list = this.TransportersInGroup(this.Map);
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
		}

		// Token: 0x06008B50 RID: 35664 RVA: 0x00289740 File Offset: 0x00287940
		private void SelectAllInGroup()
		{
			List<ShipTransport> list = this.TransportersInGroup(this.Map);
			Selector selector = Find.Selector;
			selector.ClearSelection();
			for (int i = 0; i < list.Count; i++)
			{
				selector.Select(list[i].parent, true, true);
			}
		}
		public static float FuelNeededToLaunchAtDist(float dist)
		{
			return 2.25f * dist;
		}
		// Token: 0x06008B51 RID: 35665 RVA: 0x0028978C File Offset: 0x0028798C
		private void SelectNextInGroup()
		{
			List<ShipTransport> list = this.TransportersInGroup(this.Map);
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
		}


		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			IEnumerator<Gizmo> enumerator = null;
			if (this.Shuttle != null && !this.Shuttle.ShowLoadingGizmos && !this.Shuttle.permitShuttle)
			{
				yield break;
			}
			if (this.LoadingInProgressOrReadyToLaunch)
			{
				if (this.Shuttle == null || !this.Shuttle.Autoload)
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandCancelLoad".Translate(),
						defaultDesc = "CommandCancelLoadDesc".Translate(),
						icon = ShipTransport.CancelLoadCommandTex,
						action = delegate ()
						{
							SoundDefOf.Designate_Cancel.PlayOneShotOnCamera(null);
							this.CancelLoad();
						}
					};
				}
				if (!this.Props.max1PerGroup)
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandSelectPreviousTransporter".Translate(),
						defaultDesc = "CommandSelectPreviousTransporterDesc".Translate(),
						icon = SelectPreviousInGroupCommandTex,
						action = delegate ()
						{
							this.SelectPreviousInGroup();
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "CommandSelectAllTransporters".Translate(),
						defaultDesc = "CommandSelectAllTransportersDesc".Translate(),
						icon = SelectAllInGroupCommandTex,
						action = delegate ()
						{
							this.SelectAllInGroup();
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "CommandSelectNextTransporter".Translate(),
						defaultDesc = "CommandSelectNextTransporterDesc".Translate(),
						icon = SelectNextInGroupCommandTex,
						action = delegate ()
						{
							this.SelectNextInGroup();
						}
					};
				}
				if (this.Props.canChangeAssignedThingsAfterStarting && (this.Shuttle == null || !this.Shuttle.Autoload))
				{
					yield return new Command_LoadToTransporter
					{
						defaultLabel = "CommandSetToLoadTransporter".Translate(),
						defaultDesc = "CommandSetToLoadTransporterDesc".Translate(),
						icon = LoadCommandTex,
						transComp = this
					};
				}
			}
			else
			{
				Command_LoadToTransporter command_LoadToTransporter = new Command_LoadToTransporter();
				if (this.Props.max1PerGroup)
				{
					if (this.Props.canChangeAssignedThingsAfterStarting)
					{
						command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate();
						command_LoadToTransporter.defaultDesc = "CommandSetToLoadTransporterDesc".Translate();
					}
					else
					{
						command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate();
						command_LoadToTransporter.defaultDesc = "CommandLoadTransporterSingleDesc".Translate();
					}
				}
				else
				{
					int num = 0;
					for (int i = 0; i < Find.Selector.NumSelected; i++)
					{
						Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
						if (thing != null && thing.def == this.parent.def)
						{
							ShipLaunchable ShipLaunchable = thing.TryGetComp<ShipLaunchable>();
							if (ShipLaunchable == null || (ShipLaunchable.FuelingPortSource != null && ShipLaunchable.FuelingPortSourceHasAnyFuel))
							{
								num++;
							}
						}
					}
					command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate(num.ToString());
					command_LoadToTransporter.defaultDesc = "CommandLoadTransporterDesc".Translate();
				}
				command_LoadToTransporter.icon = LoadCommandTex;
				command_LoadToTransporter.transComp = this;
				ShipLaunchable launchable = this.Launchable;
				if (launchable != null)
				{
				
					if (false)
					{
						command_LoadToTransporter.Disable("test");
					}
					else if (false)
					{
						command_LoadToTransporter.Disable("CommandLoadTransporterFailNoFuel".Translate());
					}
				}
				yield return command_LoadToTransporter;
			}
			yield break;

		}


	}
	#endregion
}
