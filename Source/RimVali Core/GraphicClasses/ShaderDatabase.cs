using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimValiCore
{
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
                    RenderTexture rTex = new RenderTexture(tex.width, tex.height,0,RenderTextureFormat.Default,RenderTextureReadWrite.Linear);
                    RenderTexture previous = RenderTexture.active;
                    Graphics.Blit(tex, rTex);
                    RenderTexture.active = rTex;
                    Texture2D nTex = new Texture2D(tex.width,tex.height);
                    nTex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                    nTex.Apply();
                    WriteTex(nTex, $"{bundle.name}_dectexture_{count}");
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(rTex);
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
