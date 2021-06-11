using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
/*
using Verse;
namespace RimValiCore.RimValiPlants
{
    public class plantClass : PlantProperties
    {
        public int maxTemp;
        public int minTemp;


    }

    public class Plant : RimWorld.Plant
    {
        public override string GetInspectString()
        {
            return "test";
        }
        public float RC_GRF(RimValiCore.RVR.RimValiRaceDef rDef)
        {

               
                float num;
                if (!GenTemperature.TryGetTemperatureForCell(base.Position, base.Map, out num))
                {
                    return 1f;
                }
                if (num < 10f)
                {
                    return Mathf.InverseLerp(0f+rDef.RimValiPlant.minTemp, rDef.RimValiPlant.minTemp, num);
                }
                if (num > 42f)
                {
                    return Mathf.InverseLerp(58f+rDef.RimValiPlant.maxTemp, rDef.RimValiPlant.maxTemp, num);
                }
                return 1f;
        }
        public override float GrowthRate
        {
            get
            {
                if (this.Blighted)
                {
                    return 0f;
                }
                if (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map, false))
                {
                    return 0f;
                }
                if (this.def is RimValiCore.RVR.RimValiRaceDef)
                {
                    return this.GrowthRateFactor_Fertility * this.RC_GRF(this.def as RimValiCore.RVR.RimValiRaceDef) * this.GrowthRateFactor_Light;
                }
                return this.GrowthRateFactor_Fertility * this.GrowthRateFactor_Temperature * this.GrowthRateFactor_Light;
            }
        }
    }
}
*/