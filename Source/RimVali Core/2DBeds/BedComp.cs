using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.TwoDBedPatchExtension
{
    public class ResizedBedCompProperties : CompProperties
    {
        public ResizedBedCompProperties()
        {
            this.compClass=typeof(BedComp);
        }
    }
    public class BedComp : ThingComp
    {
    }
}
