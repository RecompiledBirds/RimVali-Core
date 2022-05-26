using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

//using RimValiCore.RimValiPlants;
namespace RimValiCore.RVR
{
    public class RimValiRaceDef : ThingDef
    {
        public HashSet<RenderableDef> RenderableDefs { get; private set; } = new HashSet<RenderableDef>();

        public HashSet<RenderableDef> GetRenderableDefsThatShow(Pawn pawn, RotDrawMode mode, bool portrait)
        {
            HashSet<RenderableDef> output = new HashSet<RenderableDef>();
            foreach (RenderableDef def in RenderableDefs)
            {
                if (def.CanShow(pawn, mode, portrait))
                {
                    output.Add(def);
                }
            }
            return output;
        }

        public Vector2 textureSize = new Vector2(1000, 1000);
        public List<RenderableDef> renderableDefs = new List<RenderableDef>();
        public raceColors graphics = new raceColors();
        public bool hasHair = false;
        public restrictions restrictions = new restrictions();
        public Main mainSettings = new Main();
        public bool useHumanRecipes = true;
        public RVRRaceInsertion raceInsertion = new RVRRaceInsertion();

        //Thoughts
        public List<ReplaceableThoughts> replaceableThoughts = new List<ReplaceableThoughts>();

        public cannibalismThoughts cannibalismThoughts = new cannibalismThoughts();
        public bool canHavethoughts = true;

        public List<BodyTypeDef> bodyTypes = new List<BodyTypeDef>();

        public butcherAndHarvestThoughts butcherAndHarvestThoughts = new butcherAndHarvestThoughts();
        public ThingDef corpseToUse = null;
        public ThingDef meatToUse = null;
        public List<ThingCategoryDef> corpseThingCategories = null;

        //public plantClass RimValiPlant;

        public override void ResolveReferences()
        {
            RenderableDefs = renderableDefs.ToHashSet();
            if (corpseThingCategories != null)
            {
                race.corpseDef.thingCategories = new List<ThingCategoryDef>();
                race.corpseDef.thingCategories.AddRange(corpseThingCategories);
            }
            if (corpseToUse != null)
            {
                race.corpseDef.statBases = new List<StatModifier>() { };

                race.corpseDef.alwaysHaulable = false;
                race.corpseDef.ingestible.preferability = FoodPreferability.NeverForNutrition;
                race.corpseDef.category = ThingCategory.None;
                race.corpseDef.ResolveReferences();

                race.corpseDef = corpseToUse;
            }
            if (meatToUse != null)
            {
                race.meatDef.statBases = new List<StatModifier>() { };

                race.meatDef.alwaysHaulable = false;
                race.meatDef.ingestible.preferability = FoodPreferability.NeverForNutrition;
                race.meatDef.category = ThingCategory.None;
                race.meatDef.ResolveReferences();

                race.meatDef = meatToUse;
            }
            butcherAndHarvestThoughts.Resolve();
            cannibalismThoughts.Resolove();
            comps.Add(new colorCompProps());
            base.ResolveReferences();
        }

        private readonly Dictionary<ThoughtDef, ThoughtDef> cachedReplacementThoughts = new Dictionary<ThoughtDef, ThoughtDef>();

        public bool ReplaceThought(ref ThoughtDef thought, bool log = false)
        {
            //Log.Message(replaceableThoughts.Count.ToString());
            //Log.Message("checking thought list..", true);
            if (cachedReplacementThoughts.ContainsKey(thought))
            {
                thought = cachedReplacementThoughts[thought];
                return true;
            }
            foreach (ReplaceableThoughts replaceable in replaceableThoughts)
            {
                //The issue seems to be in this check, although i cannot imagine why
                if (replaceable.thoughtToReplace.defName == thought.defName)
                {
                    cachedReplacementThoughts[thought] = replaceable.replacementThought;
                    thought = replaceable.replacementThought;
                    return true;
                }
            }
            return false;
        }

        public ThoughtDef GetEatenThought(ThingDef race, bool raw = true, bool cannibal = false)
        {
            return cannibalismThoughts.thoughts.Any(x => x.race == race) ? raw ? cannibal ? cannibalismThoughts.thoughts.First(x => x.race == race).ateRawCannibal : cannibalismThoughts.thoughts.First(x => x.race == race).ateRaw : cannibal ? cannibalismThoughts.thoughts.First(x => x.race == race).ateCookedCannibal : cannibalismThoughts.thoughts.First(x => x.race == race).ateCooked : butcherAndHarvestThoughts.careAboutUndefinedRaces ? raw ? cannibal ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect : cannibal ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient : null;
        }

        public ThoughtDef GetEatenThoughtFromIngestible(ThingDef ingestible, bool raw = false, bool cannibal = false)
        {
            if (GetAllCannibalThoughtRaces().Any(race => race.race.meatDef == ingestible))
            {
                return GetEatenThought(GetAllCannibalThoughtRaces().Where(race => race.race.meatDef == ingestible).ToList()[0], raw, cannibal);
            }
            return null;
        }

        public List<ThingDef> GetAllCannibalThoughtRaces()
        {
            List<ThingDef> result = new List<ThingDef>();
            foreach (cannibalsimThought cannibalismThought in cannibalismThoughts.thoughts)
            {
                result.Add(cannibalismThought.race);
            }
            if (cannibalismThoughts.careAbountUndefinedRaces)
            {
                result.AddRange(DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null && x.race.Humanlike));
            }
            return result;
        }


        public void HeadOffsetPawn(Rot4 rot, Pawn pawn, ref Vector3 __result)
        {
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (rimValiRaceDef.renderableDefs.Where(x => x.defName.ToLower() == "head").Count() > 0)
                {
                    Vector2 offset = new Vector2(0, 0);

                    RenderableDef headDef = rimValiRaceDef.renderableDefs.First(x => x.defName.ToLower() == "head");
                    Vector3 pos = new Vector3(0, 0, 0)
                    {
                        y = __result.y
                    };
                    if (headDef.west == null)
                    {
                        headDef.west = headDef.east;
                    }
                    if (rot == Rot4.South)
                    {
                        pos.x = headDef.south.position.x + offset.x;
                        pos.z = headDef.south.position.y + offset.y;
                    }
                    else if (rot == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.z = headDef.north.position.y + offset.y;
                    }
                    else if (rot == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.z = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.z = headDef.west.position.y + offset.y;
                    }
                    //Log.Message(pos.ToString());
                    __result += pos;
                }
            }
        }

        public void HeadOffsetPawn(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn pawn = __instance.graphics.pawn;
            PawnGraphicSet set = __instance.graphics;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (rimValiRaceDef.renderableDefs.Where(x => x.defName.ToLower() == "head").Count() > 0)
                {
                    Vector2 offset = new Vector2(0, 0);

                    RenderableDef headDef = rimValiRaceDef.renderableDefs.First(x => x.defName.ToLower() == "head");
                    __instance.graphics.headGraphic.drawSize = headDef.south.size;
                    Vector3 pos = new Vector3(0, 0, 0)
                    {
                        y = __result.y
                    };
                    if (headDef.west == null)
                    {
                        headDef.west = headDef.east;
                    }
                    if (pawn.Rotation == Rot4.South)
                    {
                        pos.x = headDef.south.position.x + offset.x;
                        pos.z = headDef.south.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.z = headDef.north.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.z = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.z = headDef.west.position.y + offset.y;
                    }
                    //Log.Message(pos.ToString());
                    __result += pos;
                }
            }
        }

        private static bool GenerationHasFailed = false;
        public void GenGraphics(Pawn pawn)
        {
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                try
                {
                    ColorComp colorcomp = pawn.TryGetComp<ColorComp>();

                    PrefdefinedPawnDefs prefdefinedPawnDef = null;

                    System.Random Rand = new System.Random();

                    foreach (PrefdefinedPawnDefs def in DefDatabase<PrefdefinedPawnDefs>.AllDefsListForReading)
                    {
                        if (def.chanceToOccur >= Rand.Next(0, 100) && (def.race == null || def.race == this))
                            prefdefinedPawnDef = def;
                        if (prefdefinedPawnDef != null)
                            break;
                    }

                    if (colorcomp.renderableDefIndexes == null)
                        colorcomp.renderableDefIndexes = new Dictionary<string, int>();
                    if (colorcomp.renderableDefMaskIndexes == null)
                        colorcomp.renderableDefMaskIndexes = new Dictionary<string, int>();

                    bool preDefinedTexture = prefdefinedPawnDef != null && prefdefinedPawnDef.usesSetTexture;
                    bool predefinedMask = prefdefinedPawnDef != null && prefdefinedPawnDef.usesSetMask;
                    foreach (RenderableDef renderableDef in renderableDefs)
                    {

                        if (!colorcomp.renderableDefIndexes.ContainsKey(renderableDef.defName))
                        {
                            if (renderableDef.linkIndexWithDef != null)
                            {
                                if (colorcomp.renderableDefIndexes.ContainsKey(renderableDef.linkIndexWithDef.defName))
                                {
                                    colorcomp.renderableDefIndexes.Add(renderableDef.defName, colorcomp.renderableDefIndexes[renderableDef.linkIndexWithDef.defName]);
                                }
                                else
                                {
                                    int index = Rand.Next(renderableDef.textures.Count);
                                    if (preDefinedTexture)
                                        index = Mathf.Clamp(prefdefinedPawnDef.textureIndex, 0, renderableDef.textures.Count - 1);
                                    colorcomp.renderableDefIndexes.Add(renderableDef.linkIndexWithDef.defName, index);
                                    colorcomp.renderableDefIndexes.Add(renderableDef.defName, index);
                                }
                            }
                            else
                            {
                                int index = Rand.Next(renderableDef.textures.Count);
                                if (preDefinedTexture)
                                    index = Mathf.Clamp(prefdefinedPawnDef.textureIndex, 0, renderableDef.textures.Count - 1);
                                colorcomp.renderableDefIndexes.Add(renderableDef.defName, index);
                            }

                            if (renderableDef.linkMaskIndexWith != null)
                            {
                                if (colorcomp.renderableDefMaskIndexes.ContainsKey(renderableDef.linkIndexWithDef.defName))
                                {
                                    colorcomp.renderableDefMaskIndexes.Add(renderableDef.defName, colorcomp.renderableDefMaskIndexes[renderableDef.linkMaskIndexWith.defName]);
                                }
                                else
                                {
                                    int count = renderableDef.linkMaskIndexWith.textures[colorcomp.renderableDefIndexes[renderableDef.linkMaskIndexWith.defName]].GetMasks(pawn).Count;
                                    int index = Rand.Next(count);
                                    if (!predefinedMask)
                                        index = Mathf.Clamp(prefdefinedPawnDef.textureIndex, 0, count - 1);

                                    colorcomp.renderableDefMaskIndexes.Add(renderableDef.linkMaskIndexWith.defName, index);
                                    colorcomp.renderableDefMaskIndexes.Add(renderableDef.defName, index);
                                }
                            }
                            else
                            {
                                int count = renderableDef.GetCurrentTexture(pawn, renderableDef.GetMyIndex(pawn)).GetMasks(pawn).Count;
                                int index = Rand.Next(count);
                                if (predefinedMask)
                                    index = Mathf.Clamp(prefdefinedPawnDef.textureIndex, 0, count - 1);
                                colorcomp.renderableDefMaskIndexes.Add(renderableDef.defName, index);
                            }
                        }
                    }
                    if (!DefDatabase<RVRBackstory>.AllDefs.Where(x => (pawn.story.adulthood != null && x.defName == pawn.story.adulthood.identifier) || x.defName == pawn.story.childhood.identifier).EnumerableNullOrEmpty())
                    {
                        List<RVRBackstory> backstories = DefDatabase<RVRBackstory>.AllDefs.Where(x => (pawn.story.adulthood != null && x.defName == pawn.story.adulthood.identifier) || x.defName == pawn.story.childhood.identifier).ToList();
                        RVRBackstory story = backstories[0];

                        if (!story.colorGenOverrides.NullOrEmpty())
                        {
                            foreach (Colors color in story.colorGenOverrides)
                            {
                                if (!colorcomp.colors.ContainsKey(color.name))
                                {
                                    Color color1 = color.Generator(pawn).firstColor.NewRandomizedColor();
                                    Color color2 = color.Generator(pawn).secondColor.NewRandomizedColor();
                                    Color color3 = color.Generator(pawn).thirdColor.NewRandomizedColor();
                                    colorcomp.colors.Add(color.name, new ColorSet(color1, color2, color3, color.isDyeable));
                                }
                            }
                        }
                    }

                    foreach (Colors color in rimValiRaceDef.graphics.colorSets)
                    {

                        if (!colorcomp.colors.ContainsKey(color.name))
                        {
                            bool generatePreDefinedColors = prefdefinedPawnDef != null && prefdefinedPawnDef.definedColorSets.Any(colorset => colorset.name == color.name);

                            if (!generatePreDefinedColors)
                            {
                                Color color1 = color.Generator(pawn).firstColor.NewRandomizedColor();
                                Color color2 = color.Generator(pawn).secondColor.NewRandomizedColor();
                                Color color3 = color.Generator(pawn).thirdColor.NewRandomizedColor();
                                colorcomp.colors.Add(color.name, new ColorSet(color1, color2, color3, color.isDyeable));
                            }
                            else
                            {
                                DefinedColorSet colorSet = prefdefinedPawnDef.definedColorSets.Find(colorset => colorset.name == color.name);

                                colorcomp.colors.Add(color.name, new ColorSet(colorSet.colorOne, colorSet.colorTwo, colorSet.colorThree, color.isDyeable));
                            }
                        }
                    }
                }
                catch
                {
                    //This code means the comp itself has failed.
                    //Attempt 5 times to self-repair.
                    if (!GenerationHasFailed)
                    {
                        for (int a = 0; a < 5; a++)
                        {
                            try
                            {
                                ColorComp colorcomp = pawn.TryGetComp<ColorComp>();
                                FieldInfo field = pawn.GetType().GetField("comps");
                                List<ThingComp> thingComps = (field.GetValue(pawn) as List<ThingComp>);
                                thingComps.Remove(colorcomp);
                                colorcomp = (ColorComp)Activator.CreateInstance(typeof(ColorComp));
                                colorcomp.parent = pawn;
                                thingComps.Add(colorcomp);
                                colorcomp.Initialize(new colorCompProps());
                                field.SetValue(pawn, thingComps);
                                //If we get here, we've reset everything. Break.
                                break;
                            }
                            catch
                            {
                                //Stop trying if a = 4.
                                GenerationHasFailed = a == 4;
                            }
                        }
                    }
                }
            }
        }

        public class ReplaceableThoughts
        {
            public ThoughtDef thoughtToReplace;
            public ThoughtDef replacementThought;
        }
    }
}
