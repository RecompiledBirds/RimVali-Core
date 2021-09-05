/*
namespace RimValiCore.RimValiPlants
{
	// Token: 0x02000C24 RID: 3108
	public class JobDriver_PlantSow : JobDriver
	{
		// Token: 0x17000B80 RID: 2944
		// (get) Token: 0x0600490B RID: 18699 RVA: 0x0019C378 File Offset: 0x0019A578
		private Plant Plant
		{
			get
			{
				return (Plant)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x0600490C RID: 18700 RVA: 0x00034AE3 File Offset: 0x00032CE3
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.sowWorkDone, "sowWorkDone", 0f, false);
		}

		// Token: 0x0600490D RID: 18701 RVA: 0x0002D63A File Offset: 0x0002B83A
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x0600490E RID: 18702 RVA: 0x00034B01 File Offset: 0x00032D01
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => PlantUtility.AdjacentSowBlocker(this.job.plantDefToSow, this.TargetA.Cell, this.Map) != null).FailOn(() => !this.job.plantDefToSow.CanEverPlantAt_NewTemp(this.TargetLocA, this.Map, false));
			Toil sowToil = new Toil();
			sowToil.initAction = delegate ()
			{
				this.TargetThingA = GenSpawn.Spawn(this.job.plantDefToSow, this.TargetLocA, this.Map, WipeMode.Vanish);
				this.pawn.Reserve(this.TargetThingA, sowToil.actor.CurJob, 1, -1, null, true);
				try
				{
					Plant plant = Plant;
					plant.Growth = 0f;
					plant.sown = true;
                }
                catch
                {
					Log.Error("fcked up here");
                }
			};
			sowToil.tickAction = delegate ()
			{
				Pawn actor = sowToil.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Plants, 0.085f, false);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
				try
				{
					Plant plant = this.Plant;
					if (plant.LifeStage != PlantLifeStage.Sowing)
					{
						Log.Error(this + " getting sowing work while not in Sowing life stage.", false);
					}

					this.sowWorkDone += statValue;
					if (this.sowWorkDone >= plant.def.plant.sowWork)
					{
						plant.Growth = 0.05f;
						this.Map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
						actor.records.Increment(RecordDefOf.PlantsSown);
						this.ReadyForNextToil();
						return;
					}
                }
                catch
                {
					Log.Error("and here");
                }
			};
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => this.sowWorkDone / this.Plant.def.plant.sowWork, true, -0.5f);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (this.TargetThingA != null)
				{
					Plant plant = (Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (this.sowWorkDone < plant.def.plant.sowWork && !this.TargetThingA.Destroyed)
					{
						this.TargetThingA.Destroy(DestroyMode.Vanish);
					}
				}
			});
			sowToil.activeSkill = (() => SkillDefOf.Plants);
			yield return sowToil;
			yield break;
		}

		// Token: 0x040030A8 RID: 12456
		private float sowWorkDone;
	}
}

*/