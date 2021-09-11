using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimValiCore
{
    public class WallPlaceworker : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            IEnumerable<Thing> things = map.thingGrid.ThingsAt(loc);
            return !things.EnumerableNullOrEmpty() && things.Any(x => x.def.holdsRoof);
        }  
    }
}
