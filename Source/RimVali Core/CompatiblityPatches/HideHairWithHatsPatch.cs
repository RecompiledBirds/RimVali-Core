using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.RVR
{
    public class HideHairWithHatsPatch
    {
        public static bool Prefix(Pawn pawn, Rot4 facing, out Material mat)
        {
            mat = null;
            return pawn.def is RimValiRaceDef rDef && rDef.hasHair;
        }
    }
}
