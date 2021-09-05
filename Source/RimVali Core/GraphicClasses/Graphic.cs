using UnityEngine;
using Verse;
namespace RimValiCore
{
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    public class AvaliGraphic : Graphic
    {
        public Color colorThree = Color.white;
        public AvaliGraphicData data;
        public string path;
        private readonly Graphic_Shadow cachedShadowGraphicInt;
        private AvaliGraphic cachedShadowlessGraphicInt;

        public Color ColorThree => colorThree;




        public virtual void Init(AvaliGraphicRequest req)
        {
            Log.ErrorOnce("Cannot init Graphic of class " + GetType().ToString(), 658928, false);
        }

        public virtual AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + GetType().ToString(), 66300, false);
            return AvaliBaseContent.BadGraphic;
        }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public virtual AvaliGraphic GetCopy(Vector2 newDrawSize)

        {
            return AvaliGraphicDatabase.Get(GetType(),
                                            path,
                                            Shader,
                                            newDrawSize,
                                            color,
                                            colorTwo,
                                            colorThree);
        }

        public virtual AvaliGraphic GetShadowlessGraphic()
        {
            if (data == null || data.shadowData == null)
            {
                return this;
            }

            if (cachedShadowlessGraphicInt == null)
            {
                AvaliGraphicData graphicData = new AvaliGraphicData();
                graphicData.CopyFrom(data);
                graphicData.shadowData = null;
                cachedShadowlessGraphicInt = graphicData.Graphic;
            }
            return cachedShadowlessGraphicInt;
        }
    }
}
