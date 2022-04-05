using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimValiCore
{
     public class AvaliShaderDatabase
    {
        internal static void LoadShaders()
        {
            string path = Path.Combine(RimValiUtility.dir, "RimValiAssetBundles", "shader");
            AssetBundle bundle = RimValiUtility.shaderLoader(path);
            Tricolor = (Shader)bundle.LoadAsset("assets/resources/materials/avalishader.shader");
            lookup.Add(Tricolor.name, Tricolor);
        }

        public static Shader Tricolor;
        public static Dictionary<string, Shader> lookup = new Dictionary<string, Shader>();

        public static Shader DefaultShader => Tricolor;
    }
}
