using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore
{
    public class RimValiCoreMod : Mod
    {
        public RimValiCoreMod(ModContentPack content) : base(content)
        {
            RimValiUtility.dir = content.RootDir.ToString();
        }
    }
}