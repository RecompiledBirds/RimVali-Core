using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

// This was decompiled from <somewhere>

namespace RimValiCore
{
    public struct AvaliMaterialRequest : IEquatable<AvaliMaterialRequest>
    {
        public Shader shader;
        public Texture2D mainTex;
        public Color color;
        public Color colorTwo;
        public Texture2D maskTex;
        public int renderQueue;
        public List<ShaderParameter> shaderParameters;
        public Color colorThree;

        public string BaseTexPath
        {
            set => mainTex = ContentFinder<Texture2D>.Get(value);
        }

        public AvaliMaterialRequest(Texture2D tex)
        {
            Log.Message("this mat req");
            shader = ShaderDatabase.Cutout;
            mainTex = tex;
            color = Color.red;
            colorTwo = Color.green;
            colorThree = Color.blue;
            maskTex = null;
            renderQueue = 0;
            shaderParameters = null;
        }

        public AvaliMaterialRequest(Texture2D tex, Shader shader)
        {
            Log.Message("matreq2");
            this.shader = shader;
            mainTex = tex;
            color = Color.green;
            colorTwo = Color.blue;
            colorThree = Color.red;
            maskTex = null;
            renderQueue = 0;
            shaderParameters = null;
        }

        public AvaliMaterialRequest(Texture2D tex, Shader shader, Color color)
        {
            Log.Message("matreq3");
            this.shader = shader;
            mainTex = tex;
            this.color = color;
            colorTwo = Color.red;
            colorThree = Color.blue;
            maskTex = null;
            renderQueue = 0;
            shaderParameters = null;
        }

        public override int GetHashCode()
        {
            return Gen.HashCombine(Gen.HashCombineInt(Gen.HashCombine(Gen.HashCombine(Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombine(0, shader), color), colorTwo), mainTex), maskTex), renderQueue), shaderParameters);
        }

        public override bool Equals(object obj)
        {
            return obj is AvaliMaterialRequest request && Equals(request);
        }

        public bool Equals(AvaliMaterialRequest other)
        {
            return other.shader == shader && other.mainTex == mainTex && other.color == color && other.colorTwo == colorTwo && other.maskTex == maskTex && other.renderQueue == renderQueue && other.shaderParameters == shaderParameters;
        }

        public static bool operator ==(AvaliMaterialRequest lhs, AvaliMaterialRequest rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(AvaliMaterialRequest lhs, AvaliMaterialRequest rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return $"AvaliMaterialRequest({shader.name}, {mainTex.name}, {color}, {colorTwo}, {colorThree}, {maskTex}, {renderQueue})";
        }
    }
}