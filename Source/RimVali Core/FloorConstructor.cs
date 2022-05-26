using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace RimValiCore
{
    public static class FloorConstructor
    {
        private static List<DesignationCategoryDef> toUpdateDesignationCatDefs = new List<DesignationCategoryDef>();
        private static List<DesignatorDropdownGroupDef> toUpdateDropdownDesDefs = new List<DesignatorDropdownGroupDef>();
        private static List<string> materials = new List<string>();
        private static HashSet<TerrainDef> floorsMade = new HashSet<TerrainDef>();
        private static bool canGenerate = true;
        private static StringBuilder builder = new StringBuilder();
        /// <summary>
        /// Creates all versions of a floor from a material; it's on the label
        /// </summary>
        /// <param name="def">The terrain def we are "duplicating"</param>
        /// <param name="name">The NAME of the category we want to duplicate.</param>
        public static void CreateAllVersions(TerrainDef def, string name)
        {
            IEnumerable<ThingDef> floors = DefDatabase<ThingDef>.AllDefs
                .Where(d => d.stuffProps?.categories?.Any(cat => cat.defName == name) ?? false);
            foreach (ThingDef tDef in floors)
            {
                if (!materials.Contains(tDef.defName))
                {
                    materials.Add(tDef.defName);
                }

                //I have NO IDEA why, but one of those archotech mods has something called archotechmatteraddingsomecraptoavoidproblems and it hates me.
                //So lets assume they arent a special case
                //And do this?
                if (!DefDatabase<TerrainDef>.AllDefs.Any(terrain => terrain.defName == $"{def.defName}_{tDef.defName}"))
                {
                    bool hasmaxedout = false;
                    bool hasminedout = false;
                    ushort uS = (ushort)$"{def.defName}_{tDef.defName}".GetHashCode();
                    while (DefDatabase<TerrainDef>.AllDefs.Any(terrain => terrain.shortHash == uS) || floorsMade.Any(t => t.shortHash == uS) && canGenerate)
                    {
                        if (uS < ushort.MaxValue && !hasmaxedout)
                        {
                            uS += 1;
                        }
                        else if (uS == ushort.MaxValue)
                        {
                            hasmaxedout = true;
                        }
                        if (uS > ushort.MinValue && hasmaxedout && !hasminedout)
                        {
                            uS -= 1;
                        }
                        else if (uS == ushort.MinValue && hasmaxedout)
                        {
                            hasminedout = true;
                        }
                        if (hasminedout && hasmaxedout)
                        {
                            //If you ever see this i'll be impressed
                            Log.Warning($"[RimVali Core/FloorConstructor] Could not generate tile {string.Format(def.label, tDef.label)}'s unique short hash, aborting..");
                            canGenerate = false;
                            return;
                        }
                    }

                    //Sets up some basic stuff
                    //shortHash  & defName are the very important
                    TerrainDef output = new TerrainDef()
                    {
                        color = tDef.GetColorForStuff(tDef),
                        uiIconColor = tDef.GetColorForStuff(tDef),
                        defName = $"{def.defName}_{tDef.defName}",
                        label = string.Format(def.label, tDef.label),
                        debugRandomId = uS,
                        index = uS,
                        shortHash = uS,
                        costList = ((Func<List<ThingDefCountClass>>)delegate
                        {
                            List<ThingDefCountClass> costList = new List<ThingDefCountClass>();
                            int amount = 0;
                            foreach (ThingDefCountClass thingDefCountClass in def.costList)
                            {
                                amount += thingDefCountClass.count;
                            }
                            costList.Add(new ThingDefCountClass()
                            {
                                thingDef = tDef,
                                count = amount
                            });
                            return costList;
                        })(),
                        designationCategory = def.designationCategory,
                        designatorDropdown = def.designatorDropdown,
                        ignoreIllegalLabelCharacterConfigError = def.ignoreIllegalLabelCharacterConfigError
                    };

                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    //This copies some of the varibles from the floor we are duplicating over
                    //We don't want it to touch the fields we've already set, so I keep a list here to help.

                    List<string> avoidFields = new List<string>() { "color", "defname", "label", "debugrandomid", "index", "shorthash", "costlist", "uiiconcolor", "designatordropdown" };
                    foreach (FieldInfo field in def.GetType().GetFields(bindingFlags).Where(f => !avoidFields.Contains(f.Name.ToLower())))
                    {
                        foreach (FieldInfo f2 in output.GetType().GetFields(bindingFlags).Where(f => f.Name == field.Name))
                        {
                            f2.SetValue(output, field.GetValue(def));
                        }
                    }

                    List<string> toRemove = new List<string>();
                    foreach (string str in output.tags)
                    {
                        //This looks for a DesignationCategoryDef with a defname that matches the string between AddDesCat_ and [ENDDESNAME]
                        if (str.Contains("AddDesCat_"))
                        {
                            string cS = string.Copy(str);
                            string res = cS.Substring(cS.IndexOf("AddDesCat_") + "AddDesCat_".Length, (cS.IndexOf("[ENDDESNAME]") - ("[ENDDESNAME]".Length - 2)) - cS.IndexOf("AddDesCat_"));
                            if (DefDatabase<DesignationCategoryDef>.AllDefs.Any(cat => cat.defName == res))
                            {
                                if (!toUpdateDesignationCatDefs.Contains(DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]))
                                {
                                    toUpdateDesignationCatDefs.Add(DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]);
                                }
                                output.designationCategory = DefDatabase<DesignationCategoryDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0];
                            }
                        }
                        //This looks for a DesignationCategoryDef with a defname that matches the string between AddDesDropDown_ and [ENDDNAME]
                        if (str.Contains("AddDesDropDown_"))
                        {
                            string cS = string.Copy(str);
                            string res = cS.Substring(cS.IndexOf("AddDesDropDown_") + "AddDesDropDown_".Length, (cS.IndexOf("[ENDDNAME]") - ("[ENDDNAME]".Length + 5)) - cS.IndexOf("AddDesDropDown_"));
                            if (DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Any(cat => cat.defName == res))
                            {
                                if (!toUpdateDropdownDesDefs.Contains(DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]))
                                {
                                    toUpdateDropdownDesDefs.Add(DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0]);
                                }
                                output.designatorDropdown = DefDatabase<DesignatorDropdownGroupDef>.AllDefs.Where(cat => cat.defName == res).ToList()[0];
                            }
                        }
                        //This removes the tag from clones.
                        if (str.EndsWith("RemoveFromClones") || str.EndsWith("_RFC"))
                        {
                            toRemove.Add(str);
                        }
                    }
                    foreach (string str in toRemove)
                    {
                        output.tags.Remove(str);
                    }
                    //How vanilla RW sets up some stuff

                    //Blueprint
                    ThingDef thingDef = new ThingDef()
                    {
                        category = ThingCategory.Ethereal,
                        label = "Unspecified blueprint",
                        altitudeLayer = AltitudeLayer.Blueprint,
                        useHitPoints = false,
                        selectable = true,
                        seeThroughFog = true,
                        comps =
                        {
                            new CompProperties_Forbiddable()
                         },
                        drawerType = DrawerType.MapMeshAndRealTime,
                        ignoreIllegalLabelCharacterConfigError = true
                    };
                    thingDef.thingClass = typeof(Blueprint_Build);
                    thingDef.defName = ThingDefGenerator_Buildings.BlueprintDefNamePrefix + output.defName;
                    thingDef.label = output.label + "BlueprintLabelExtra".Translate();
                    thingDef.entityDefToBuild = output;
                    thingDef.graphicData = new GraphicData
                    {
                        shaderType = ShaderTypeDefOf.MetaOverlay,
                        texPath = "Things/Special/TerrainBlueprint",
                        graphicClass = typeof(Graphic_Single)
                    };
                    thingDef.constructionSkillPrerequisite = output.constructionSkillPrerequisite;
                    thingDef.artisticSkillPrerequisite = output.artisticSkillPrerequisite;
                    thingDef.clearBuildingArea = false;
                    thingDef.modContentPack = output.modContentPack;
                    output.blueprintDef = thingDef;

                    //Framedef
                    ThingDef frameDef = new ThingDef()
                    {
                        isFrameInt = true,
                        category = ThingCategory.Building,
                        label = "Unspecified building frame",
                        thingClass = typeof(Frame),
                        altitudeLayer = AltitudeLayer.Building,
                        useHitPoints = true,
                        selectable = true,
                        building = new BuildingProperties(),
                        comps =
                         {
                             new CompProperties_Forbiddable()
                         },
                        scatterableOnMapGen = false,
                        leaveResourcesWhenKilled = true
                    };
                    frameDef.building.artificialForMeditationPurposes = false;
                    frameDef.defName = ThingDefGenerator_Buildings.BuildingFrameDefNamePrefix + output.defName;
                    frameDef.label = output.label + "FrameLabelExtra".Translate();
                    frameDef.entityDefToBuild = output;
                    frameDef.useHitPoints = false;
                    frameDef.fillPercent = 0f;
                    frameDef.description = "Terrain building in progress.";
                    frameDef.passability = Traversability.Standable;
                    frameDef.selectable = true;
                    frameDef.constructEffect = output.constructEffect;
                    frameDef.building.isEdifice = false;
                    frameDef.constructionSkillPrerequisite = output.constructionSkillPrerequisite;
                    frameDef.artisticSkillPrerequisite = output.artisticSkillPrerequisite;
                    frameDef.clearBuildingArea = false;
                    frameDef.modContentPack = output.modContentPack;
                    frameDef.category = ThingCategory.Ethereal;
                    frameDef.entityDefToBuild = output;
                    output.frameDef = frameDef;
                    frameDef.ignoreIllegalLabelCharacterConfigError = true;
                    //This makes sure everything is setup how it should be
                    output.PostLoad();
                    output.ResolveReferences();

                    builder.AppendLine($"[RimVali Core/FloorConstructor] Generated {output.label}");
                    builder.AppendLine($" Mat color: {tDef.stuffProps.color}");
                    builder.AppendLine($" Floor color: {output.color}");
                    builder.AppendLine($" UI Icon color: {output.uiIconColor}");
                    
                    floorsMade.Add(output);
                }
            }
        }

        internal static void Initalize()
        {

            List<TerrainDef> workOn = new List<TerrainDef>();
            workOn.AddRange(DefDatabase<TerrainDef>.AllDefs);
            //Tells us to clone a terrain
            foreach (TerrainDef def in workOn)
            {
                bool hasDoneTask = false;
                if (def.tags.NullOrEmpty())
                {
                    continue;
                }
                if (def.tags.Any(str => str.Contains("cloneMaterial")))
                {
                    List<string> tags = def.tags.Where(x => x.Contains("cloneMaterial") && !x.NullOrEmpty()).ToList();
                    foreach (string s in tags)
                    {
                        //Gets the category name between cloneMaterial_ and [ENDCATNAME]
                        string cS = string.Copy(s);
                        int startIndex = cS.IndexOf("cloneMaterial_") + "cloneMaterial_".Length;
                        int endIndex = cS.IndexOf("[ENDCATNAME]");
                        int length = endIndex - startIndex;
                        string res = cS.Substring(startIndex, length);
                        CreateAllVersions(def, res);
                    }
                }

                if (def.tags.Any(str => str.Contains("removeFromResearch")))
                {
                    List<string> tags = def.tags.Where(x => x.Contains("removeFromResearch_") && !x.NullOrEmpty()).ToList();
                    for (int a = 0; a < tags.Count; a++)
                    {
                        string s = tags[a];
                        hasDoneTask = true;
                        //Gets the category name between cloneMaterial_ and [ENDCATNAME]
                        string cS = string.Copy(s);
                        int startIndex = cS.IndexOf("removeFromResearch_") + "removeFromResearch_".Length;
                        int endIndex = cS.IndexOf("[ENDRESNAME]");
                        int length = endIndex - startIndex;
                        string res = cS.Substring(startIndex, length);
                        ResearchProjectDef proj = def.researchPrerequisites.Find(x => x.defName == res);
                        def.researchPrerequisites.Remove(proj);
                        proj.PostLoad();
                        proj.ResolveReferences();
                    }
                }
                if (hasDoneTask)
                {
                    def.PostLoad();
                    def.ResolveReferences();
                }
            }
            //Ensures we are adding to the DefDatabase. Just a saftey check.
            foreach (TerrainDef def in floorsMade)
            {
                if (!DefDatabase<TerrainDef>.AllDefs.Select(terrainDef => terrainDef.defName).Contains(def.defName))
                {
                    def.PostLoad();
                    DefDatabase<TerrainDef>.Add(def);
                }
            }

            Log.Message("[RimVali Core/FloorConstructor] Updating architect menu..");

            //Updates/refreshes menus
            foreach (DesignationCategoryDef def in toUpdateDesignationCatDefs)
            {
                def.PostLoad();
                def.ResolveReferences();
            }

            foreach (DesignatorDropdownGroupDef def in toUpdateDropdownDesDefs)
            {
                def.PostLoad();
                def.ResolveReferences();
            }
         //   Log.Message(builder.ToString());
            Log.Message($"[RimVali Core/FloorConstructor] Updated {toUpdateDesignationCatDefs.Count} designation categories & {toUpdateDropdownDesDefs.Count} dropdown designations.");
            //We need to do this or RW has a fit
            WealthWatcher.ResetStaticData();

            Log.Message($"[RimVali Core/FloorConstructor] Built  {floorsMade.Count} floors from {materials.Count} materials.");
        }
    }
}