using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace RimValiCore
{
    [StaticConstructorOnStartup]
    public static class ClassTMPDecompiler
    {
        static int dC;
        static string path = $"{Application.dataPath}";
        static ClassTMPDecompiler()
        {
            path = Path.GetFullPath(Path.Combine(path, @"..\"));
            path = $"{path}/RIMVALICORE/DecompiledTex";
            Log.Message("Starting texture decompiler");
            foreach(AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                int count = 0;
                foreach (Texture2D tex in bundle.LoadAllAssets<Texture2D>())
                {
                    Rect renderRect = new Rect(new Vector2(100, 100), new Vector2(tex.width, tex.height));
                    RenderTexture rTex = new RenderTexture(tex.width, tex.height, 3);

                    Graphics.DrawTexture(renderRect,tex);

                    Texture2D copyTex = new Texture2D(tex.width, tex.height);
                    Graphics.CopyTexture(tex, copyTex);
                    WriteTex(copyTex, $"{bundle.name}_dectexture_{count}");
                    count++;
                    dC++;
                }
            }
            Log.Message($"Decompiled {dC} textures");
        }


        public static void WriteTex(Texture2D tex, string name)
        {
            if (!Directory.Exists(path)){Directory.CreateDirectory(path);}
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes($"{path}/{name}.png",data);
        }
    }


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