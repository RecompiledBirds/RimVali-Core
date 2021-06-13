using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace RimValiCore
{
    [StaticConstructorOnStartup]
    public class AvaliShaderDatabase
    {
        static AvaliShaderDatabase()
        {
            string dir = RimValiUtility.dir;
            string path = dir + "/RimValiAssetBundles/shader";
            AssetBundle bundle = RimValiUtility.shaderLoader(path);
            Tricolor = (Shader)bundle.LoadAsset("assets/resources/materials/avalishader.shader");
            lookup.Add(Tricolor.name, Tricolor);
        }

        public static Shader Tricolor;
        public static Dictionary<string, Shader> lookup = new Dictionary<string, Shader>();

        public static Shader DefaultShader
        {
            get
            {
                return AvaliShaderDatabase.Tricolor;
            }
        }
    }
}