using System;
using UnityEngine;
using Verse;
namespace RimValiCore
{
    public static class AvaliGraphicUtility
    {
        public static AvaliGraphic ExtractInnerGraphicFor(this AvaliGraphic outerGraphic, Thing thing)
        {
            switch (outerGraphic)
            {
                case AvaliGraphic_Random graphicRandom:
                    return graphicRandom.SubGraphicFor(thing);
                case AvaliGraphic_Appearances graphicAppearances:
                    return graphicAppearances.SubGraphicFor(thing);
                default:
                    return outerGraphic;
            }
        }

        public static AvaliGraphic_Linked WrapLinked(
          AvaliGraphic subGraphic,
          LinkDrawerType linkDrawerType)
        {
            switch (linkDrawerType)
            {
                case LinkDrawerType.None:
                    return null;
                case LinkDrawerType.Basic:
                    return new AvaliGraphic_Linked(subGraphic);
                case LinkDrawerType.CornerFiller:
                    return new AvaliGraphic_LinkedCornerFiller(subGraphic);
                case LinkDrawerType.Transmitter:
                    return new AvaliGraphic_LinkedTransmitter(subGraphic);
                case LinkDrawerType.TransmitterOverlay:
                    return new AvaliGraphic_LinkedTransmitterOverlay(subGraphic);
                default:
                    throw new ArgumentException();
            }
        }
    }
    public class AvaliGraphic_Linked : AvaliGraphic
    {
        protected AvaliGraphic subGraphic;

        public virtual LinkDrawerType LinkerType => LinkDrawerType.Basic;

        public override Material MatSingle => MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingle, LinkDirections.None);

        public AvaliGraphic_Linked()
        {
        }

        public AvaliGraphic_Linked(AvaliGraphic subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.Message("couldbethisone");
            AvaliGraphic_Linked graphicLinked = new AvaliGraphic_Linked(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo, newColorThree))
            {
                data = data
            };
            return graphicLinked;
        }


        public override Material MatSingleFor(Thing thing)
        {
            return LinkedDrawMatFrom(thing, thing.Position);
        }

        protected Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
        {
            int num1 = 0;
            int num2 = 1;
            for (int index = 0; index < 4; ++index)
            {
                if (ShouldLinkWith(cell + GenAdj.CardinalDirections[index], parent))
                {
                    num1 += num2;
                }

                num2 *= 2;
            }
            LinkDirections LinkSet = (LinkDirections)num1;
            return MaterialAtlasPool.SubMaterialFromAtlas(subGraphic.MatSingleFor(parent), LinkSet);
        }

        public virtual bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            if (!parent.Spawned)
            {
                return false;
            }

            return !c.InBounds(parent.Map) ? (uint)(parent.def.graphicData.linkFlags & LinkFlags.MapEdge) > 0U : (uint)(parent.Map.linkGrid.LinkFlagsAt(c) & parent.def.graphicData.linkFlags) > 0U;
        }
    }

    public class AvaliGraphic_RandomRotated : AvaliGraphic
    {
        private readonly AvaliGraphic subGraphic;
        private readonly float maxAngle;

        public override Material MatSingle => subGraphic.MatSingle;

        public AvaliGraphic_RandomRotated(AvaliGraphic subGraphic, float maxAngle)
        {
            this.subGraphic = subGraphic;
            this.maxAngle = maxAngle;
            drawSize = subGraphic.drawSize;
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            Mesh mesh = MeshAt(rot);
            float num = 0.0f;
            if (thing != null)
            {
                num = (float)(-maxAngle + thing.thingIDNumber * 542 % (maxAngle * 2.0));
            }

            float angle = num + extraRotation;
            Material matSingle = subGraphic.MatSingle;
            Vector3 position = loc;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Material material = matSingle;
            Graphics.DrawMesh(mesh, position, rotation, material, 0, null, 0);
        }

        public override string ToString()
        {
            return "RandomRotated(subGraphic=" + subGraphic.ToString() + ")";
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            AvaliGraphic_RandomRotated graphicRandomRotated = new AvaliGraphic_RandomRotated(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo, newColorThree), maxAngle)
            {
                data = data,
                drawSize = drawSize
            };
            return graphicRandomRotated;
        }
    }
}