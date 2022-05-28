using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.RVRFrameWork
{
    public class ExcludedShuffleDef : Def
    {
        public List<PawnKindDef> excludedPawnKinds;
        public List<ThingDef> excludedRaces;
    }
}
