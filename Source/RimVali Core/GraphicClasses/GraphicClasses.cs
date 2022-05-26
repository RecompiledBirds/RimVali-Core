using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimValiCore
{
    public class AvaliGraphic_Appearances : AvaliGraphic
    {
        protected AvaliGraphic[] subGraphics;

        public override Material MatSingle => subGraphics[StuffAppearanceDefOf.Smooth.index].MatSingle;

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return SubGraphicFor(thing).MatAt(rot, thing);
        }

        public override void Init(AvaliGraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            color = req.color;
            drawSize = req.drawSize;
            List<StuffAppearanceDef> defsListForReading = DefDatabase<StuffAppearanceDef>.AllDefsListForReading;
            subGraphics = new AvaliGraphic[defsListForReading.Count];
            for (int index = 0; index < subGraphics.Length; ++index)
            {
                StuffAppearanceDef stuffAppearance = defsListForReading[index];
                string folderPath = req.path;
                if (!stuffAppearance.pathPrefix.NullOrEmpty())
                {
                    folderPath = stuffAppearance.pathPrefix + "/" + ((IEnumerable<string>)folderPath.Split('/')).Last();
                }

                Texture2D texture2D = ContentFinder<Texture2D>.GetAllInFolder(folderPath).Where(x => x.name.EndsWith(stuffAppearance.defName)).FirstOrDefault();
                if (texture2D != null)
                {
                    subGraphics[index] = AvaliGraphicDatabase.Get<AvaliGraphic_Single>(folderPath + "/" + texture2D.name, req.shader, drawSize, color);
                }
            }
            for (int index = 0; index < subGraphics.Length; ++index)
            {
                if (subGraphics[index] == null)
                {
                    subGraphics[index] = subGraphics[StuffAppearanceDefOf.Smooth.index];
                }
            }
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            if (newColorTwo != Color.white)
            {
                Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910251);
            }

            return AvaliGraphicDatabase.Get<AvaliGraphic_Appearances>(path, newShader, drawSize, newColor, Color.white, Color.white, data);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return SubGraphicFor(thing).MatSingleFor(thing);
        }

        /* public override void DrawWorker(
           Vector3 loc,
           Rot4 rot,
           ThingDef thingDef,
           Thing thing,
           float extraRotation)
         {
             this.SubGraphicFor(thing).DrawWorker(loc, rot, thingDef, thing, extraRotation);
         }
        */

        public AvaliGraphic SubGraphicFor(Thing thing)
        {
            StuffAppearanceDef smooth = StuffAppearanceDefOf.Smooth;
            return thing != null ? SubGraphicFor(thing.Stuff) : subGraphics[smooth.index];
        }

        public AvaliGraphic SubGraphicFor(ThingDef stuff)
        {
            StuffAppearanceDef stuffAppearanceDef = StuffAppearanceDefOf.Smooth;
            if (stuff != null && stuff.stuffProps.appearance != null)
            {
                stuffAppearanceDef = stuff.stuffProps.appearance;
            }

            return subGraphics[stuffAppearanceDef.index];
        }

        public override string ToString()
        {
            return "Appearance(path=" + path + ", color=" + color + ", colorTwo=unsupported)";
        }
    }

    public class AvaliGraphic_Cluster : AvaliGraphic_Collection
    {
#pragma warning disable IDE0051 // Remove unused private member
        private const float PositionVariance = 0.45f;
        private const float SizeVariance = 0.2f;
        private const float SizeFactorMin = 0.8f;
        private const float SizeFactorMax = 1.2f;
#pragma warning restore IDE0051 // Remove unused private member

        public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

        /*   public override void DrawWorker(
             Vector3 loc,
             Rot4 rot,
             ThingDef thingDef,
             Thing thing,
             float extraRotation)
           {
               Log.ErrorOnce("Graphic_Scatter cannot draw realtime.", 9432243, false);
           }

           public override void Print(SectionLayer layer, Thing thing)
           {
               Vector3 vector3 = thing.TrueCenter();
               Rand.PushState();
               Rand.Seed = thing.Position.GetHashCode();
               int num = thing is Filth filth ? filth.thickness : 3;
               for (int index = 0; index < num; ++index)
               {
                   Material matSingle = this.MatSingle;
                   Vector3 center = vector3 + new Vector3(Rand.Range(-0.45f, 0.45f), 0.0f, Rand.Range(-0.45f, 0.45f));
                   Vector2 size = new Vector2(Rand.Range(this.data.drawSize.x * 0.8f, this.data.drawSize.x * 1.2f), Rand.Range(this.data.drawSize.y * 0.8f, this.data.drawSize.y * 1.2f));
                   float rot = (float)Rand.RangeInclusive(0, 360);
                   bool flipUv = (double)Rand.Value < 0.5;
                   Printer_Plane.PrintPlane(layer, center, size, matSingle, rot, flipUv, (Vector2[])null, (Color32[])null, 0.01f, 0.0f);
               }
               Rand.PopState();
           }
           */

        public override string ToString()
        {
            return "Scatter(subGraphic[0]=" + subGraphics[0].ToString() + ", count=" + subGraphics.Length + ")";
        }
    }

    public abstract class AvaliGraphic_Collection : AvaliGraphic
    {
        protected AvaliGraphic[] subGraphics;

        public override void Init(AvaliGraphicRequest req)
        {
            data = req.graphicData;
            if (req.path.NullOrEmpty())
            {
                throw new ArgumentNullException("folderPath");
            }

            if (req.shader == null)
            {
                throw new ArgumentNullException("shader");
            }

            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            List<Texture2D> list = ContentFinder<Texture2D>.GetAllInFolder(req.path).Where(x => !x.name.EndsWith(Graphic_Single.MaskSuffix)).OrderBy(x => x.name).ToList();
            if (list.NullOrEmpty())
            {
                Log.Error("Collection cannot init: No textures found at path " + req.path);
                subGraphics = new AvaliGraphic[1]
                {
          AvaliBaseContent.BadGraphic
                };
            }
            else
            {
                subGraphics = new AvaliGraphic[list.Count];
                for (int index = 0; index < list.Count; ++index)
                {
                    string path = req.path + "/" + list[index].name;
                    subGraphics[index] = AvaliGraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, drawSize, color, colorTwo, colorThree, null, req.shaderParameters);
                }
            }
        }
    }

    public class AvaliGraphic_Single : AvaliGraphic
    {
        public static readonly string MaskSuffix = "_m";
        protected Material mat;

        public override Material MatSingle => mat;

        public override Material MatWest => mat;

        public override Material MatSouth => mat;

        public override Material MatEast => mat;

        public override Material MatNorth => mat;

        public override bool ShouldDrawRotated => data == null || data.drawRotated;

        public override void Init(AvaliGraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            MaterialRequest req1 = new MaterialRequest
            {
                mainTex = ContentFinder<Texture2D>.Get(req.path, true),
                shader = req.shader,
                color = color,
                colorTwo = colorTwo,
                renderQueue = req.renderQueue,
                shaderParameters = req.shaderParameters
            };
            if (req.shader.SupportsMaskTex())
            {
                req1.maskTex = ContentFinder<Texture2D>.Get(req.path + Graphic_Single.MaskSuffix, false);
            }

            mat = MaterialPool.MatFrom(req1);
        }

        public override AvaliGraphic GetColoredVersion(
        Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            return AvaliGraphicDatabase.Get<AvaliGraphic_Single>(path, newShader, drawSize, newColor, newColorTwo, Color.white, data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return mat;
        }

        public override string ToString()
        {
            return "Single(path=" + path + ", color=" + color + ", colorTwo=" + colorTwo + ")";
        }
    }

    public abstract class AvaliGraphic_WithPropertyBlock : AvaliGraphic_Single
    {
        protected MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        protected override void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
        {
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(loc, quat, new Vector3(drawSize.x, 1f, drawSize.y)), mat, 0, null, 0, propertyBlock);
        }
    }

    public class AvaliGraphic_FadesInOut : AvaliGraphic_WithPropertyBlock
    {
        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            CompFadesInOut comp = thing.TryGetComp<CompFadesInOut>();
            if (comp == null)
            {
                Log.ErrorOnce(thingDef.defName + ": Graphic_FadesInOut requires CompFadesInOut.", 5643893);
            }
            else
            {
                propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * comp.Opacity()));
                base.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            }
        }
    }

    public class AvaliGraphic_Terrain : AvaliGraphic_Single
    {
        public override void Init(AvaliGraphicRequest req)
        {
            base.Init(req);
        }

        public override string ToString()
        {
            return "Terrain(path=" + path + ", shader=" + Shader + ", color=" + color + ")";
        }
    }

    [StaticConstructorOnStartup]
    public class AvaliGraphic_Mote : AvaliGraphic_Single
    {
        protected static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        protected virtual bool ForcePropertyBlock => false;

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            const int layer = 0;
            Mote mote = (Mote)thing;
            float alpha = mote.Alpha;
            if (alpha <= 0.0)
            {
                return;
            }

            Color colA = Color * mote.instanceColor;
            colA.a *= alpha;
            Vector3 exactScale = mote.exactScale;
            exactScale.x *= data.drawSize.x;
            exactScale.z *= data.drawSize.y;
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(mote.DrawPos, Quaternion.AngleAxis(mote.exactRotation, Vector3.up), exactScale);
            Material matSingle = MatSingle;
            if (!ForcePropertyBlock && colA.IndistinguishableFrom(matSingle.color))
            {
                Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, layer, null, 0);
            }
            else
            {
                propertyBlock.SetColor(ShaderPropertyIDs.Color, colA);
                Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, layer, null, 0, propertyBlock);
            }
        }

        public override string ToString()
        {
            return "Mote(path=" + path + ", shader=" + Shader + ", color=" + color + ", colorTwo=unsupported)";
        }
    }

    public class AvaliGraphic_Multi : AvaliGraphic
    {
        private readonly Material[] mats = new Material[4];
        private bool westFlipped;
        private bool eastFlipped;
        private float drawRotatedExtraAngleOffset;

        public string GraphicPath => path;

        public override Material MatSingle => MatSouth;

        public override Material MatWest => mats[3];

        public override Material MatSouth =>
                //this.mats[2].shader = ShaderDatabase.CutoutSkin; //No mask
                //this.mats[2].shader = ShaderDatabase.CutoutComplex; //No change
                //this.mats[2].shader = ShaderDatabase.Cutout;
                //this.mats[2].SetColor("_Color", Color.red);
                //this.mats[2].SetColor("_ColorTwo", Color.blue);
                //this.mats[2].SetColor("_ColorThree", Color.green);
                //Log.Message("Mat with shader: " + this.mats[2].shader.name + "C: " + this.mats[2].GetColor("_Color"));
                mats[2];

        public override Material MatEast => mats[1];

        public override Material MatNorth => mats[0];

        public override bool WestFlipped => westFlipped;

        public override bool EastFlipped => eastFlipped;

        public override bool ShouldDrawRotated
        {
            get
            {
                if (data != null && !data.drawRotated)
                {
                    return false;
                }

                return MatEast == MatNorth || MatWest == MatNorth;
            }
        }

        public override float DrawRotatedExtraAngleOffset => drawRotatedExtraAngleOffset;

        public override void Init(AvaliGraphicRequest req)
        {
            if(req.maskPath==null)
                req.maskPath=req.path;

            data = req.graphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            colorThree = req.colorThree;
            drawSize = req.drawSize;
            Texture2D[] texture2DArray1 = new Texture2D[mats.Length];
            texture2DArray1[0] = ContentFinder<Texture2D>.Get(req.path + "_north", false);
            texture2DArray1[1] = ContentFinder<Texture2D>.Get(req.path + "_east", false);
            texture2DArray1[2] = ContentFinder<Texture2D>.Get(req.path + "_south", false);
            texture2DArray1[3] = ContentFinder<Texture2D>.Get(req.path + "_west", false);
            if (texture2DArray1[0] == null)
            {
                if (texture2DArray1[2] != null)
                {
                    texture2DArray1[0] = texture2DArray1[2];
                    drawRotatedExtraAngleOffset = 180f;
                }
                else if (texture2DArray1[1] != null)
                {
                    texture2DArray1[0] = texture2DArray1[1];
                    drawRotatedExtraAngleOffset = -90f;
                }
                else if (texture2DArray1[3] != null)
                {
                    texture2DArray1[0] = texture2DArray1[3];
                    drawRotatedExtraAngleOffset = 90f;
                }
                else
                {
                    texture2DArray1[0] = ContentFinder<Texture2D>.Get(req.path, false);
                }
            }
            if (texture2DArray1[0] == null)
            {
                Log.Error("Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe());
            }
            else
            {
                if (texture2DArray1[2] == null)
                {
                    texture2DArray1[2] = texture2DArray1[0];
                }

                if (texture2DArray1[1] == null)
                {
                    if (texture2DArray1[3] != null)
                    {
                        texture2DArray1[1] = texture2DArray1[3];
                        eastFlipped = DataAllowsFlip;
                    }
                    else
                    {
                        texture2DArray1[1] = texture2DArray1[0];
                    }
                }
                if (texture2DArray1[3] == null)
                {
                    if (texture2DArray1[1] != null)
                    {
                        texture2DArray1[3] = texture2DArray1[1];
                        westFlipped = DataAllowsFlip;
                    }
                    else
                    {
                        texture2DArray1[3] = texture2DArray1[0];
                    }
                }
                Texture2D[] texture2DArray2 = new Texture2D[mats.Length];
                //if (req.shader.SupportsMaskTex())
                if (req.shader == AvaliShaderDatabase.Tricolor)
                {
                    //Log.Message("Generating MaskTex");
                    texture2DArray2[0] = ContentFinder<Texture2D>.Get(req.maskPath + "_northm", false);
                    texture2DArray2[1] = ContentFinder<Texture2D>.Get(req.maskPath + "_eastm", false);
                    texture2DArray2[2] = ContentFinder<Texture2D>.Get(req.maskPath + "_southm", false);
                    texture2DArray2[3] = ContentFinder<Texture2D>.Get(req.maskPath + "_westm", false);
                    if (texture2DArray2[0] == null)
                    {
                        if (texture2DArray2[2] != null)
                        {
                            texture2DArray2[0] = texture2DArray2[2];
                        }
                        else if (texture2DArray2[1] != null)
                        {
                            texture2DArray2[0] = texture2DArray2[1];
                        }
                        else if (texture2DArray2[3] != null)
                        {
                            texture2DArray2[0] = texture2DArray2[3];
                        }
                    }
                    if (texture2DArray2[2] == null)
                    {
                        texture2DArray2[2] = texture2DArray2[0];
                    }

                    if (texture2DArray2[1] == null)
                    {
                        texture2DArray2[1] = !(texture2DArray2[3] != null) ? texture2DArray2[0] : texture2DArray2[3];
                    }

                    if (texture2DArray2[3] == null)
                    {
                        texture2DArray2[3] = !(texture2DArray2[1] != null) ? texture2DArray2[0] : texture2DArray2[1];
                    }
                }
                for (int index = 0; index < mats.Length; ++index)
                {
                    //this.mats[index] = MaterialPool.MatFrom(new MaterialRequest()
                    mats[index] = AvaliMaterialPool.MatFrom(new AvaliMaterialRequest()
                    {
                        mainTex = texture2DArray1[index],
                        shader = req.shader,
                        color = color,
                        colorTwo = colorTwo,
                        colorThree = colorThree,
                        maskTex = texture2DArray2[index],
                        shaderParameters = req.shaderParameters
                    });
                };
            }
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            //Log.Message("Imtryingtogetthis");
            return AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(path, newShader, drawSize, newColor, newColorTwo, newColorThree, data);
        }

        public override string ToString()
        {
            return "Multi(initPath=" + path + ", color=" + color + ", colorTwo=" + colorTwo + ")";
        }

        public override int GetHashCode()
        {
            return Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombineStruct(Gen.HashCombine(0, path), color), colorTwo), ColorThree);
        }
    }

    public class AvaliGraphic_Random : AvaliGraphic_Collection
    {
        public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            //Log.Message("butalsothert");
            if (newColorTwo != Color.white)
            {
                Log.ErrorOnce("Cannot use Graphic_Random.GetColoredVersion with a non-white colorTwo.", 9910251);
            }

            return AvaliGraphicDatabase.Get<AvaliGraphic_Random>(path, newShader, drawSize, newColor, Color.white, Color.white, data);
        }

        /*
        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return thing == null ? this.MatSingle : this.MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return thing == null ? this.MatSingle : this.SubGraphicFor(thing).MatSingle;
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            (thing == null ? this.subGraphics[0] : this.SubGraphicFor(thing)).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
        */

        public AvaliGraphic SubGraphicFor(Thing thing)
        {
            return thing == null ? subGraphics[0] : subGraphics[thing.thingIDNumber % subGraphics.Length];
        }

        public AvaliGraphic FirstSubgraphic()
        {
            return subGraphics[0];
        }

        public override string ToString()
        {
            return "Random(path=" + path + ", count=" + subGraphics.Length + ")";
        }
    }

    public class AvaliGraphic_Flicker : AvaliGraphic_Collection
    {
#pragma warning disable IDE0051 // Remove unused private member
        private const int BaseTicksPerFrameChange = 15;
        private const int ExtraTicksPerFrameChange = 10;
        private const float MaxOffset = 0.05f;
#pragma warning restore IDE0051 // Remove unused private member

        public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

        /*
        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            if (thingDef == null)
                Log.ErrorOnce("Fire DrawWorker with null thingDef: " + (object)loc, 3427324, false);
            else if (this.subGraphics == null)
            {
                Log.ErrorOnce("Graphic_Flicker has no subgraphics " + (object)thingDef, 358773632, false);
            }
            else
            {
                int ticksGame = Find.TickManager.TicksGame;
                if (thing != null)
                    ticksGame += Mathf.Abs(thing.thingIDNumber ^ 8453458);
                int num1 = ticksGame / 15;
                int index = Mathf.Abs(num1 ^ (thing != null ? thing.thingIDNumber : 0) * 391) % this.subGraphics.Length;
                float num2 = 1f;
                CompProperties_FireOverlay propertiesFireOverlay = (CompProperties_FireOverlay)null;
                if (thing is Fire fire)
                    num2 = fire.fireSize;
                else if (thingDef != null)
                {
                    propertiesFireOverlay = thingDef.GetCompProperties<CompProperties_FireOverlay>();
                    if (propertiesFireOverlay != null)
                        num2 = propertiesFireOverlay.fireSize;
                }
                if (index < 0 || index >= this.subGraphics.Length)
                {
                    Log.ErrorOnce("Fire drawing out of range: " + (object)index, 7453435, false);
                    index = 0;
                }
                AvaliGraphic subGraphic = this.subGraphics[index];
                float num3 = Mathf.Min(num2 / 1.2f, 1.2f);
                Vector3 vector3 = GenRadial.RadialPattern[num1 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius * 0.05f;
                Vector3 pos = loc + vector3 * num2;
                if (propertiesFireOverlay != null)
                    pos += propertiesFireOverlay.offset;
                Vector3 s = new Vector3(num3, 1f, num3);
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(pos, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, subGraphic.MatSingle, 0);
            }
        }
        */

        public override string ToString()
        {
            return "Flicker(subGraphic[0]=" + subGraphics[0].ToString() + ", count=" + subGraphics.Length + ")";
        }
    }

    public class AvaliGraphic_StackCount : AvaliGraphic_Collection
    {
        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            //Log.Message("ormaybhetisone");
            return AvaliGraphicDatabase.Get<AvaliGraphic_StackCount>(path, newShader, drawSize, newColor, newColorTwo, newColorThree
                , data);
        }

        /*
        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return thing == null ? this.MatSingle : this.MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            return thing == null ? this.MatSingle : this.SubGraphicFor(thing).MatSingle;
        }

        public AvaliGraphic SubGraphicFor(Thing thing)
        {
            return this.SubGraphicForStackCount(thing.stackCount, thing.def);
        }

        public override void DrawWorker(
          Vector3 loc,
          Rot4 rot,
          ThingDef thingDef,
          Thing thing,
          float extraRotation)
        {
            (thing == null ? this.subGraphics[0] : this.SubGraphicFor(thing)).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
        */

        public AvaliGraphic SubGraphicForStackCount(int stackCount, ThingDef def)
        {
            switch (subGraphics.Length)
            {
                case 1:
                    return subGraphics[0];

                case 2:
                    return stackCount == 1 ? subGraphics[0] : subGraphics[1];

                case 3:
                    if (stackCount == 1)
                    {
                        return subGraphics[0];
                    }

                    return stackCount == def.stackLimit ? subGraphics[2] : subGraphics[1];

                default:
                    if (stackCount == 1)
                    {
                        return subGraphics[0];
                    }

                    return stackCount == def.stackLimit ? subGraphics[subGraphics.Length - 1] : subGraphics[Mathf.Min(1 + Mathf.RoundToInt((float)(stackCount / (double)def.stackLimit * (subGraphics.Length - 3.0) + 9.99999974737875E-06)), subGraphics.Length - 2)];
            }
        }

        public override string ToString()
        {
            return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
        }
    }
}