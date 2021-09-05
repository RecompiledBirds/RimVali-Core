using System.Collections.Generic;
using Verse;
namespace RimValiCore.RVR
{
    public class DyeComp : HediffComp
    {
        public DyeCompProps Props => (DyeCompProps)props;

        public Dictionary<string, ColorSet> oldColors = new Dictionary<string, ColorSet>();
        public List<string> colorKey = new List<string>();
        public List<ColorSet> colorValue = new List<ColorSet>();

        public override void CompExposeData()
        {
            Scribe_Collections.Look<string, ColorSet>(ref oldColors, "colors", LookMode.Value, LookMode.Deep, ref colorKey, ref colorValue);
            if (oldColors == null)
            {
                oldColors = new Dictionary<string, ColorSet>();
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)

        {
            Pawn pawn = parent.pawn;
            if (pawn.def is RimValiRaceDef def)
            {
                ColorComp colorComp = pawn.GetComp<ColorComp>();
                foreach (string set in colorComp.colors.Keys)
                {
                    if (Props.targetsSpecifcSets == true && Props.setsToChange.Contains(set))
                    {
                        if (colorComp.colors[set].dyeable)
                        {
                            oldColors.Add(set, new ColorSet(colorComp.colors[set].colorOne, colorComp.colors[set].colorTwo, colorComp.colors[set].colorThree, colorComp.colors[set].dyeable));

                            if (Props.changesFirstColor) { colorComp.colors[set].colorOne = Props.colors.firstColor.NewRandomizedColor(); }
                            if (Props.changesSecondColor) { colorComp.colors[set].colorTwo = Props.colors.secondColor.NewRandomizedColor(); }
                            if (Props.changesThirdColor) { colorComp.colors[set].colorThree = Props.colors.thirdColor.NewRandomizedColor(); }
                        }
                    }
                    else if (Props.targetsSpecifcSets != true)
                    {
                        if (colorComp.colors[set].dyeable)
                        {
                            oldColors.Add(set, new ColorSet(colorComp.colors[set].colorOne, colorComp.colors[set].colorTwo, colorComp.colors[set].colorThree, colorComp.colors[set].dyeable));

                            if (Props.changesFirstColor) { colorComp.colors[set].colorOne = Props.colors.firstColor.NewRandomizedColor(); }
                            if (Props.changesSecondColor) { colorComp.colors[set].colorTwo = Props.colors.secondColor.NewRandomizedColor(); }
                            if (Props.changesThirdColor) { colorComp.colors[set].colorThree = Props.colors.thirdColor.NewRandomizedColor(); }
                        }
                    }
                }
                PawnRenderer render = new PawnRenderer(pawn);
                render.graphics.ResolveAllGraphics();
            }
        }

        public override void CompPostPostRemoved()
        {
            Pawn pawn = parent.pawn;
            if (pawn.def is RimValiRaceDef def)
            {
                ColorComp colorComp = pawn.GetComp<ColorComp>();
                colorComp.colors = oldColors;
            }
            base.CompPostPostRemoved();
        }
    }
    public class DyeCompProps : HediffCompProperties
    {
        public TriColor_ColorGenerators colors;
        public bool changesFirstColor = true;
        public bool changesSecondColor = true;
        public bool changesThirdColor = true;

        public bool targetsSpecifcSets = false;
        public List<string> setsToChange;

        public DyeCompProps()
        {
            compClass = typeof(DyeComp);
        }
    }
}
