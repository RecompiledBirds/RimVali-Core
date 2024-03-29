﻿using System.Collections.Generic;
using UnityEngine;
using Verse;

// This was decompiled from <somewhere>

namespace RimValiCore
{
    public static class AvaliMaterialPool
    {
        private static readonly Dictionary<AvaliMaterialRequest, Material> matDictionary = new Dictionary<AvaliMaterialRequest, Material>();

        public static Material MatFrom(AvaliMaterialRequest req)
        {
            //Log.Message("thisine");
            if (!UnityData.IsInMainThread)
            {
                Log.Error("Tried to get a material from a different thread.");
                return null;
            }
            if (req.mainTex == null)
            {
                Log.Error("MatFrom with null sourceTex.");
                return AvaliBaseContent.BadMat;
            }
            if (req.shader == null)
            {
                Log.Warning("Matfrom with null shader.");
                return AvaliBaseContent.BadMat;
            }
            /*
			if (req.maskTex != null && !req.shader.SupportsMaskTex())
			{
				Log.Error("MaterialRequest has maskTex but shader does not support it. req=" + req.ToString(), false);
				req.maskTex = null;
			}
			*/
            // What's the reasoning behind this?
#pragma warning disable CS1717 // Assignment made to same variable
            req.color = req.color;
            req.colorTwo = req.colorTwo;
            req.colorThree = req.colorThree;
#pragma warning restore CS1717
            if (!matDictionary.TryGetValue(req, out Material material))
            {
                material = AvaliMaterialAllocator.Create(req.shader);
                material.name = req.shader.name + "_" + req.mainTex.name;
                material.mainTexture = req.mainTex;
                material.color = req.color;
                material.SetTexture(AvaliShaderPropertyIDs.MaskTex, req.maskTex);
                material.SetColor(AvaliShaderPropertyIDs.ColorTwo, req.colorTwo);
                material.SetColor(AvaliShaderPropertyIDs.ColorThree, req.colorThree);
                // liQd Comment there it is
                material.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
                //Log.Message("thisthinghascolor3: " + req.colorThree);
                if (req.renderQueue != 0)
                {
                    material.renderQueue = req.renderQueue;
                }
                if (!req.shaderParameters.NullOrEmpty())
                {
                    for (int i = 0; i < req.shaderParameters.Count; i++)
                    {
                        req.shaderParameters[i].Apply(material);
                    }
                }
                matDictionary.Add(req, material);
                if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
                {
                    WindManager.Notify_PlantMaterialCreated(material);
                }
            }
            return material;
        }
    }
}