using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimValiCore.QLine
{
    public static class QuestBackgroundHandler
    {
        private static Dictionary<string,Texture2D> textures = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture(string path)
        {
            if(textures.ContainsKey(path))
                return textures[path];
            Texture2D texture = ContentFinder<Texture2D>.Get(path);
            textures.Add(path, texture);
            return texture;
        }
    }
}
