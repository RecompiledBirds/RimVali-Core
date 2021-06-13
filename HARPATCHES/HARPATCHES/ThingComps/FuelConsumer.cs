using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace RimValiCore.ThingComps
{
    public class FuelConsumerProps : CompProperties
    {
        public int ticksBetweenConsumption = 120;
        public float amountConsumed = 1;
        public HediffDef effectAppliedOnNoFuel;
        FuelConsumerProps() {this.compClass = typeof(FuelConsumer);}
    }
    public class FuelConsumer : ThingComp
    {
        public FuelConsumerProps Props => (FuelConsumerProps)this.props;
        public CompRefuelable fuelComp
        {
            get
            {
                return this.parent.TryGetComp<CompRefuelable>();
            }
        }
        int ticks = 0;
        public override void CompTick()
        {

            if (ticks == Props.ticksBetweenConsumption)
            {
                float fuel = RimValiUtility.GetVar<float>(fieldName: "fuel", obj: fuelComp);
                ticks = 0;
                if (fuel > 0 && (fuel -= Props.amountConsumed) > 0)
                {
                    Pawn p = this.parent as Pawn;
                    if (p.health.hediffSet.HasHediff(Props.effectAppliedOnNoFuel)){p.health.RemoveHediff(HediffMaker.MakeHediff(Props.effectAppliedOnNoFuel, p));}
                    fuel -= Props.amountConsumed;
                    RimValiUtility.SetVar<float>(fieldName: "fuel", obj: fuelComp, val: fuel);
                }
                else if(Props.effectAppliedOnNoFuel !=null)
                {
                    Pawn p = this.parent as Pawn;
                    p.health.AddHediff(Props.effectAppliedOnNoFuel);
                }
            }
            ticks++;
            base.CompTick();
        }

    }
}
