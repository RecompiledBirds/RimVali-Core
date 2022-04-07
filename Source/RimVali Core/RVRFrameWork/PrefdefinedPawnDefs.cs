using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.RVR
{
    public class PrefdefinedPawnDefs : Def
    {
        public RimValiRaceDef race;

        public string firstName;
        public string lastName;
        public string middleName;

        public List<DefinedColorSet> definedColorSets = new List<DefinedColorSet>();

        public bool usesSetTexture;
        public int textureIndex;

        public bool usesSetMask;
        public int maskIndex;


        public int chanceToOccur;
    }

    public class DefinedColorSet
    {
        public string name;
        public Color colorOne;
        public Color colorTwo;
        public Color colorThree;
    }
}
