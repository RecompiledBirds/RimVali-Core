using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace RimValiCore.Ships
{
    public class ShipTransporterProps : CompProperties_Transporter
    {
        public ShipTransporterProps()
        {
            this.compClass = typeof(ShipTransporter);
        }
    }
    public class ShipTransporter : CompTransporter
    {
        public new bool LoadingInProgressOrReadyToLaunch
        {
            get
            {
                return true;
            }
        }
    }
}
