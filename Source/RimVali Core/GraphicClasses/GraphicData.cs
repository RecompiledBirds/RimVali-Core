using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimValiCore
{
    public class AvaliGraphicData
    {
        public Color color = Color.white;
        public Color colorTwo = Color.white;
        public Color colorThree = Color.white;
        public Vector2 drawSize = Vector2.one;
        public Vector3 drawOffset = Vector3.zero;
        public bool drawRotated = true;
        public bool allowFlip = true;
        [NoTranslate]
        public string texPath;
        public System.Type graphicClass;
        public ShaderTypeDef shaderType;
        public List<ShaderParameter> shaderParameters;
        public Vector3? drawOffsetNorth;
        public Vector3? drawOffsetEast;
        public Vector3? drawOffsetSouth;
        public Vector3? drawOffsetWest;
        public float onGroundRandomRotateAngle;
        public float flipExtraRotation;
        public ShadowData shadowData;
        public DamageGraphicData damageData;
        public LinkDrawerType linkType;
        public LinkFlags linkFlags;
        [Unsaved(false)]
        private AvaliGraphic cachedGraphic;

        public bool Linked => (uint)linkType > 0U;

        public AvaliGraphic Graphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    Init();
                }

                return cachedGraphic;
            }
        }

        public void CopyFrom(AvaliGraphicData other)
        {
            texPath = other.texPath;
            graphicClass = other.graphicClass;
            shaderType = other.shaderType;
            color = other.color;
            colorTwo = other.colorTwo;
            colorThree = other.colorThree;
            drawSize = other.drawSize;
            drawOffset = other.drawOffset;
            drawOffsetNorth = other.drawOffsetNorth;
            drawOffsetEast = other.drawOffsetEast;
            drawOffsetSouth = other.drawOffsetSouth;
            drawOffsetWest = other.drawOffsetSouth;
            onGroundRandomRotateAngle = other.onGroundRandomRotateAngle;
            drawRotated = other.drawRotated;
            allowFlip = other.allowFlip;
            flipExtraRotation = other.flipExtraRotation;
            shadowData = other.shadowData;
            damageData = other.damageData;
            linkType = other.linkType;
            linkFlags = other.linkFlags;
            cachedGraphic = null;
        }

        private void Init()
        {
            if (graphicClass == null)
            {
                cachedGraphic = null;
            }
            else
            {
                cachedGraphic = AvaliGraphicDatabase.Get(graphicClass, texPath, (shaderType ?? ShaderTypeDefOf.Cutout).Shader, drawSize, color, colorTwo, colorThree, this, shaderParameters);
                if (onGroundRandomRotateAngle > 0.00999999977648258)
                {
                    cachedGraphic = new AvaliGraphic_RandomRotated(cachedGraphic, onGroundRandomRotateAngle);
                }

                if (!Linked)
                {
                    return;
                }

                cachedGraphic = AvaliGraphicUtility.WrapLinked(cachedGraphic, linkType);
            }
        }

        public void ResolveReferencesSpecial()
        {
            if (damageData == null)
            {
                return;
            }

            damageData.ResolveReferencesSpecial();
        }


        public AvaliGraphic GraphicColoredFor(Thing t)
        {
            return t.DrawColor.IndistinguishableFrom(Graphic.Color) && t.DrawColorTwo.IndistinguishableFrom(Graphic.ColorTwo) ? Graphic : Graphic.GetColoredVersion(Graphic.Shader, t.DrawColor, t.DrawColorTwo, t.DrawColorTwo);
        }
    }
}