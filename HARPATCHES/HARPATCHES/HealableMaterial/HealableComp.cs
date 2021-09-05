using Verse;
namespace RimValiCore.HealableMaterial
{
    public class HealableCompProps : CompProperties
    {
        public int ticksBetweenHeal = 120;
        public int amountHealed = 2;
        public HealableCompProps() { compClass = typeof(HealableComp); }
    }
    public class HealableComp : ThingComp
    {
        public HealableCompProps Props => (HealableCompProps)props;

    }
}
