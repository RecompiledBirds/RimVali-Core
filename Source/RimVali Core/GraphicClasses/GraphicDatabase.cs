﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimValiCore
{
    [StaticConstructorOnStartup]
    public static class AvaliGraphicDatabase
    {
        private static readonly Dictionary<AvaliGraphicRequest, AvaliGraphic> allGraphics = new Dictionary<AvaliGraphicRequest, AvaliGraphic>();

        // Is this actually used somehow? Idk what all this is, so could be some weird meta-programming thing
#pragma warning disable IDE0051 // Remove unused private members
        private static readonly Shader shadertest;
#pragma warning restore IDE0051 // Remove unused private members

        public static AvaliGraphic Get<T>(string path, string maskPath = null) where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got8");
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white, Color.white, null, 0, null,maskPath));
        }

        public static AvaliGraphic Get<T>(string path, Shader shader, string maskPath= null) where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got7" + shader.name + " Color3: ");
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, Color.white, null, 0, null));
        }

        public static AvaliGraphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, string maskPath=null) where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got6" + shader.name + " Color3: ");
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, Color.white, null, 0, null,maskPath));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          int renderQueue,
          string maskPath = null)
          where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got5");
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, Color.white, null, renderQueue, null,maskPath));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          string maskPath = null)
          where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got4");
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, null, 0, null,maskPath));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          string maskPath = null)
          where T : AvaliGraphic, new()
        {
            //Log.Message("Was Got3: " + shader.name + " Color3: ");
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, Color.white, null, 0, null,maskPath));
        }

        public static AvaliGraphic Get<T>(
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          AvaliGraphicData data,
          string maskPath = null)
          where T : AvaliGraphic, new()
        {
            //if (shader.name != "Custom/Cutout") { shadertest = shader; }
            //if (shader.name == "Custom/Cutout") { shader = shadertest; }
            //Log.Message("Was Got2: " + shader.name + " Color3: " + colorThree);
            return GetInner<T>(new AvaliGraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, colorThree, data, 0, null,maskPath));
        }

        public static AvaliGraphic Get(
          Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          string maskPath = null)
        {
            //Log.Message("Was Got1");
            return Get(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, null, null,maskPath);
        }

        public static AvaliGraphic Get(
          Type graphicClass,
          string path,
          Shader shader,
          Vector2 drawSize,
          Color color,
          Color colorTwo,
          Color colorThree,
          AvaliGraphicData data,
          List<ShaderParameter> shaderParameters,
          string maskPath = null)
        {
            AvaliGraphicRequest req = new AvaliGraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, colorThree, data, 0, shaderParameters,maskPath);
            // liQdComment 2 This is what the game requests
            if (req.graphicClass == typeof(Graphic_Multi))
            {
                //Log.Message("AvaliGraphic request of type Graphic_Multi");
                return GetInner<AvaliGraphic_Multi>(req);
            }
            if (req.graphicClass == typeof(AvaliGraphic_Single))
            {
                return GetInner<AvaliGraphic_Single>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_Terrain))
            {
                return GetInner<AvaliGraphic_Terrain>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_Multi))
            {
                return GetInner<AvaliGraphic_Multi>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_Mote))
            {
                return GetInner<AvaliGraphic_Mote>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_Random))
            {
                return GetInner<AvaliGraphic_Random>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_Flicker))
            {
                return GetInner<AvaliGraphic_Flicker>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_Appearances))
            {
                return GetInner<AvaliGraphic_Appearances>(req);
            }

            if (req.graphicClass == typeof(AvaliGraphic_StackCount))
            {
                return GetInner<AvaliGraphic_StackCount>(req);
            }

            try
            {
                return (AvaliGraphic)GenGeneric.InvokeStaticGenericMethod(typeof(AvaliGraphicDatabase), req.graphicClass, "GetInner", req);
            }
            catch (Exception ex)
            {
                Log.Error("Exception getting " + graphicClass + " at " + path + ": " + ex.ToString());
            }
            return AvaliBaseContent.BadGraphic;
        }

        private static T GetInner<T>(AvaliGraphicRequest req) where T : AvaliGraphic, new()
        {
            req.color = (Color32)req.color;
            req.colorTwo = (Color32)req.colorTwo;
            req.colorThree = (Color32)req.colorThree;
            if (!allGraphics.TryGetValue(req, out AvaliGraphic graphic))
            {
                graphic = new T();
                graphic.Init(req);
                allGraphics.Add(req, graphic);
            }
            return (T)graphic;
        }

        public static void Clear()
        {
            allGraphics.Clear();
        }

        [DebugOutput("System")]
        public static void AllGraphicsLoaded()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("There are " + allGraphics.Count + " graphics loaded.");
            int num = 0;
            foreach (AvaliGraphic graphic in allGraphics.Values)
            {
                stringBuilder.AppendLine(num.ToString() + " - " + graphic.ToString());
                if (num % 50 == 49)
                {
                    Log.Message(stringBuilder.ToString());
                    stringBuilder = new StringBuilder();
                }
                ++num;
            }
            Log.Message(stringBuilder.ToString());
        }
    }
}