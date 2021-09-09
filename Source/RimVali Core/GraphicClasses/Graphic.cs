using UnityEngine;
using Verse;

namespace RimValiCore
{
    public class AvaliGraphic : Graphic
    {
        public Color colorThree = Color.white;
        public new AvaliGraphicData data;
        public new string path;
        private AvaliGraphic cachedShadowlessGraphicInt;

        public Color ColorThree => colorThree;

        public virtual void Init(AvaliGraphicRequest req)
        {
            Log.ErrorOnce("Cannot init Graphic of class " + GetType().ToString(), 658928);
        }

        public virtual AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + GetType().ToString(), 66300);
            return AvaliBaseContent.BadGraphic;
        }

        public new virtual AvaliGraphic GetCopy(Vector2 newDrawSize)

        {
            return AvaliGraphicDatabase.Get(GetType(),
                                            path,
                                            Shader,
                                            newDrawSize,
                                            color,
                                            colorTwo,
                                            colorThree);
        }

        public new virtual AvaliGraphic GetShadowlessGraphic()
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