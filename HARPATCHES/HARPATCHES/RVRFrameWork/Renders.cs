using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace RimValiCore.RVR
{
    public class RenderObject
    {
        public Pawn pawn;
        public RenderableDef rDef;
    }

    public class RenderTex
    {
        public string path;
        public AvaliGraphic tex;
    }

    public static class Renders
    {
        public static Dictionary<RenderableDef, List<RenderTex>> graphics = new Dictionary<RenderableDef, List<RenderTex>>();
        public static IEnumerable<RenderableDef> renderableDefs = DefDatabase<RenderableDef>.AllDefs;

        public static AvaliGraphic getTex(RenderableDef def, string path)
        {
            if (!graphics.ContainsKey(def))
            {
                Log.Warning($"{def.defName} not loaded! Returning null...");
                return null;
            }
            if (!graphics[def].Any(x => x.path == path))
            {
                Log.Warning($"{path} is not in any loaded paths for {def.defName}! Returning null...");
                return null;
            }
            return graphics[def].Find(x => x.path == path).tex;
        }

        static Renders()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[RimVali Core/RVR]: Started renderabledef loading, please standby!");
            builder.AppendLine($"[RimVali Core/RVR]: Loading  {renderableDefs.Count()} renderableDefs.");
            int cont = 0;
            foreach(RenderableDef render in renderableDefs)
            {
                foreach(string tex in render.findAllTextures())
                {
                    if (!graphics.ContainsKey(render)){graphics.Add(render, new List<RenderTex>());}
                    RenderTex rTex = new RenderTex
                    {
                        path = tex,
                        tex = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(tex,AvaliShaderDatabase.Tricolor)
                    };
                    graphics[render].Add(rTex);
                    builder.AppendLine($"[RimVali Core/RVR]: Loaded tex: {tex} for {render.defName}");
                    cont++;
                }
            }
            builder.AppendLine($"[RimVali Core/RVR]: Successfully loaded {cont} textures.");
            Log.Message($"{builder}");
        }

    }
}
