
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimValiCore
{
    public class DrawCompProps : CompProperties
    {
        public string texPath;
        public Vector3 offset;

        public bool isAnimated = false;
        public int ticksBetweenTexture;
        public bool lockAtLastTex;
        public List<string> textures = new List<string>();
        public DrawCompProps()
        {
            this.compClass = typeof(DrawComp);
        }
    }

    public class DrawComp : ThingComp
    {
        public DrawCompProps Props
        {
            get
            {
                return (DrawCompProps)props;
            }
        }
        public Graphic graphic;
        public int tex;
        public int tick;
        public override void CompTick()
        {
            if (Props.isAnimated)
            {
                tick++;
                if (tick == Props.ticksBetweenTexture)
                {
                    tick = 0;
                    if (tex < Props.textures.Count - 1)
                    {
                        tex++;
                    }else if (!Props.lockAtLastTex)
                    {
                        tex = 0;
                    }
                }
            }
            base.CompTick();
        }
        public override void PostDraw()
        {
            Vector3 offset = Props.offset;
            Vector3 pos = parent.DrawPos;
            pos.y += 0.02f + offset.y;
            pos.z += offset.z;
            pos.x += offset.x;
           
            Quaternion quaternion = Quaternion.AngleAxis(this.parent.Graphic.DrawRotatedExtraAngleOffset, Vector3.up);
            if (!Props.isAnimated)
            {
                if (graphic == null)
                {
                    graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(Props.texPath, AvaliShaderDatabase.Tricolor, this.parent.Graphic.drawSize, this.parent.Graphic.color);
                }
                
                if (parent.TryGetComp<CompPowerTrader>() != null)
                {
                    if (parent.TryGetComp<CompPowerTrader>().PowerOn && FlickUtility.WantsToBeOn(parent))
                    {
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(parent.Rotation), pos,
                        Quaternion.identity, graphic.MatAt(parent.Rotation), false);
                    }
                }
                else
                {
                    GenDraw.DrawMeshNowOrLater(graphic.MeshAt(parent.Rotation), pos,
                        Quaternion.identity, graphic.MatAt(parent.Rotation), false);
                }
            }
            else
            {
                graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(Props.textures[tex], AvaliShaderDatabase.Tricolor, this.parent.Graphic.drawSize, this.parent.Graphic.color);
                
                if (parent.TryGetComp<CompPowerTrader>() != null)
                {
                    if (parent.TryGetComp<CompPowerTrader>().PowerOn && FlickUtility.WantsToBeOn(parent))
                    {
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(parent.Rotation), pos,
                        Quaternion.identity, graphic.MatAt(parent.Rotation), false);
                    }
                }
                else
                {
                    GenDraw.DrawMeshNowOrLater(graphic.MeshAt(parent.Rotation), pos,
                        Quaternion.identity, graphic.MatAt(parent.Rotation), false);
                }
            }
            base.PostDraw();
        }
    }
}
