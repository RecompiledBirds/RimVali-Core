using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimValiCore
{
    [StaticConstructorOnStartup]
    public static class ClassTMPDecompiler
    {
        private static readonly string path = $"{Application.dataPath}";

        static ClassTMPDecompiler()
        {
            path = Path.GetFullPath(Path.Combine(path, "..", "RimValiCore", "DecompiledTex"));
            Log.Message("Starting texture decompiler");

            int totalCount = 0;
            foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                int count = 0;
                foreach (Texture2D tex in bundle.LoadAllAssets<Texture2D>())
                {
                    Rect renderRect = new Rect(new Vector2(100, 100), new Vector2(tex.width, tex.height));
                    RenderTexture rTex = new RenderTexture(tex.width, tex.height, 3);

                    Graphics.DrawTexture(renderRect, tex);

                    Texture2D copyTex = new Texture2D(tex.width, tex.height);
                    Graphics.CopyTexture(tex, copyTex);
                    WriteTex(copyTex, $"{bundle.name}_dectexture_{count}");
                    count++;
                    totalCount++;
                }
            }
            Log.Message($"Decompiled {totalCount} textures");
        }

        public static void WriteTex(Texture2D tex, string name)
        {
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes($"{path}/{name}.png", data);
        }
    }

    [StaticConstructorOnStartup]
    public class AvaliShaderDatabase
    {
        static AvaliShaderDatabase()
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