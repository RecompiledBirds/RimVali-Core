﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string maskPath;
        public AvaliGraphic tex;
    }

    public static class Renders
    {
        public static Dictionary<RenderableDef, List<RenderTex>> graphics = new Dictionary<RenderableDef, List<RenderTex>>();
        public static IEnumerable<RenderableDef> renderableDefs = DefDatabase<RenderableDef>.AllDefs;


        static Renders()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[RimVali Core/RVR]: Started renderabledef loading, please standby!");
            builder.AppendLine($"[RimVali Core/RVR]: Loading  {renderableDefs.Count()} renderableDefs.");
            int cont = 0;
            foreach (RenderableDef render in renderableDefs)
            {
                foreach (string tex in render.FindAllTextures())
                {
                    if (!graphics.ContainsKey(render)) { graphics.Add(render, new List<RenderTex>()); }
                    RenderTex rTex = new RenderTex
                    {
                        path = tex,
                        tex = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(tex, AvaliShaderDatabase.Tricolor)
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