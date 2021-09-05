
using UnityEngine;
using Verse;

namespace RimValiCore
{
    [StaticConstructorOnStartup]
    public static class AvaliBaseContent
    {
        public static readonly string BadTexPath;
        public static readonly string PlaceholderImagePath;
        public static readonly Material BadMat;
        public static readonly Texture2D BadTex;
        public static readonly AvaliGraphic BadGraphic;
        public static readonly Texture2D BlackTex;
        public static readonly Texture2D GreyTex;
        public static readonly Texture2D WhiteTex;
        public static readonly Texture2D ClearTex;
        public static readonly Texture2D YellowTex;
        public static readonly Material BlackMat;
        public static readonly Material WhiteMat;
        public static readonly Material ClearMat;

        public static bool NullOrBad(this Material mat)
        {
            return mat == null || mat == BadMat;
        }

        public static bool NullOrBad(this Texture2D tex)
        {
            return tex == null || tex == BadTex;
        }
    }
}