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
            IntVec3 neighbor = loc;
            if (rot == Rot4.North)
                neighbor.z += 1 ;
            if (rot == Rot4.South)
                neighbor.z -= 1;
            if (rot == Rot4.West)
                neighbor.x += 1;
            if (rot == Rot4.East)
                neighbor.x -= 1;

            bool isBlocked = map.thingGrid.ThingsAt(neighbor).Any(x=>x.def.holdsRoof);
            return !things.EnumerableNullOrEmpty() && things.Any(x => x.def.holdsRoof) && !isBlocked;
        }  
    }
}
