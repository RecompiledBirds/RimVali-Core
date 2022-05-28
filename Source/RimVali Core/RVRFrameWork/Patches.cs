using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimValiCore.RVR
{


    #region Restrictions and patching

    //Eventually I want to switch from dictionaries to this, and potentially keep a dictionary of restriction types and and objects instead. Eg. Dictionary<Type,RestrictionObject> restrictions
    public class RestrictionObject<T, V>
    {
        public T obj;
        public List<V> allowed;
    }

    public static class RVR
    {
        internal static void DoPatches()
        {
            Log.Message("[RimVali Core]: Starting RVR patches.");
            Harmony harmony = new Harmony("RimVali.Core");
            try
            {
                harmony.PatchAll();
                harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }), postfix: new HarmonyMethod(typeof(ApparelPatch), "Equipable"));
                Log.Message($"[RimVali Core] Patches completed. {harmony.GetPatchedMethods().EnumerableCount()} methods patched.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimVali Core] A patch has failed! Patches completed: {harmony.GetPatchedMethods().EnumerableCount()}");
                Log.Error(ex.ToString());
            }

        }
    }


    #region FactionResearch

    public class FacRes
    {
        public ResearchProjectDef proj;
        public bool hackable;

        public FacRes(ResearchProjectDef projectDef, bool canBeHacked)
        {
            hackable = canBeHacked;
            proj = projectDef;
        }
    }

    #endregion FactionResearch

    public static class Restrictions
    {
        public static Hashtable expRes = new Hashtable();
        public static bool CheckRestrictions<T, V>(Dictionary<T, List<V>> pairs, T item, V race, bool keyNotInReturn = true, bool raceNotFound = false) where V : Def where T : Def
        {
            return pairs.ContainsKey(item) ? pairs[item] is List<V> ? !pairs[item].NullOrEmpty() && pairs[item].Contains(race) : false : keyNotInReturn;
        }

        // Token: 0x060000EA RID: 234 RVA: 0x00006D2C File Offset: 0x00004F2C
        public static bool AddRestriction<T, V>(ref Dictionary<T, List<V>> pairs, T item, V race) where T : Def where V : Def
        {
            if (!RimValiCoreMod.Settings.expMode)
            {
                if (!pairs.ContainsKey(item))
                {
                    pairs.Add(item, new List<V>());
                    pairs[item].Add(race);
                }
                else
                {
                    if (pairs[item] != null)
                    {
                        pairs[item].Add(race);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                if (!expRes.ContainsKey(race))
                {
                    expRes.Add(race, new HashSet<Def>());
                    HashSet<Def> defs = new HashSet<Def> { item };
                    expRes[race] = defs;
                }
                else
                {
                    try
                    {
                        HashSet<Def> defs = (HashSet<Def>)expRes[race];
                        if (defs != null)
                        {
                            defs.Add(item);
                            expRes[race] = defs;
                        }
                        else
                        {
                            defs = new HashSet<Def>
                            {
                                item
                            };
                            expRes[race] = defs;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error adding {item.defName} to {race.defName}:  {e.Message}");
                    }
                }

                return true;
            }
        }


        private static ModContentPack GetPackByID(string id)
        {
            return LoadedModManager.RunningModsListForReading.Find(x=>x.Name == id||x.PackageId.ToLower()==id.ToLower()||x.PackageId==$"{id.ToLower()}_steam");
        }

        // Token: 0x04000116 RID: 278
        public static Dictionary<ThingDef, List<ThingDef>> equipmentRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000117 RID: 279
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000118 RID: 280
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictionsWhiteList = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000119 RID: 281
        public static Dictionary<BuildableDef, List<ThingDef>> buildingRestrictions = new Dictionary<BuildableDef, List<ThingDef>>();

        // Token: 0x0400011A RID: 282
        public static Dictionary<ResearchProjectDef, List<ThingDef>> researchRestrictions = new Dictionary<ResearchProjectDef, List<ThingDef>>();

        // Token: 0x0400011B RID: 283
        public static Dictionary<TraitDef, List<ThingDef>> traitRestrictions = new Dictionary<TraitDef, List<ThingDef>>();

        // Token: 0x0400011C RID: 284
        public static Dictionary<BodyTypeDef, List<ThingDef>> bodyTypeRestrictions = new Dictionary<BodyTypeDef, List<ThingDef>>();

        // Token: 0x0400011D RID: 285
        public static Dictionary<ThingDef, List<ThingDef>> bedRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x0400011E RID: 286
        public static Dictionary<ThoughtDef, List<ThingDef>> thoughtRestrictions = new Dictionary<ThoughtDef, List<ThingDef>>();

        // Token: 0x0400011F RID: 287
        public static Dictionary<ThingDef, List<ThingDef>> buildingWhitelists = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000120 RID: 288
        public static Dictionary<ThingDef, List<ThingDef>> equipabblbleWhiteLists = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000121 RID: 289
        public static Dictionary<ResearchProjectDef, bool> hackedProjects = new Dictionary<ResearchProjectDef, bool>();

        // Token: 0x04000122 RID: 290
        public static Dictionary<ThingDef, List<BodyTypeDef>> bodyDefs = new Dictionary<ThingDef, List<BodyTypeDef>>();

        // Token: 0x04000123 RID: 291
        public static Dictionary<FactionDef, List<FacRes>> factionResearchRestrictions = new Dictionary<FactionDef, List<FacRes>>();

        // Token: 0x04000124 RID: 292
        public static Dictionary<FactionDef, List<FacRes>> factionResearchBlacklist = new Dictionary<FactionDef, List<FacRes>>();

        internal static void InitRestrictions()
        {
            List<RimValiRaceDef> raceDefs = DefDatabase<RimValiRaceDef>.AllDefsListForReading;
            int raceCount = raceDefs.Count;
            for (int race = 0; race < raceCount; race++)
            {
                Log.Message($"[RimVali Core]: Setting up race {race}/{raceCount}");
                RimValiRaceDef raceDef = raceDefs[race];
                if (raceDef.restrictions.buildables.Count > 0)
                {
                    foreach (ThingDef item in raceDef.restrictions.buildables)
                    {
                        AddRestriction(ref buildingRestrictions, item, raceDef);
                    }
                }

                if (raceDef.restrictions.consumables.Count > 0)
                {
                    foreach (ThingDef item2 in raceDef.restrictions.consumables)
                    {
                        AddRestriction(ref consumableRestrictions, item2, raceDef);
                    }
                }

                if (raceDef.restrictions.equippables.Count > 0)
                {
                    foreach (ThingDef item3 in raceDef.restrictions.equippables)
                    {
                        AddRestriction(ref equipmentRestrictions, item3, raceDef);
                    }
                }

                if (raceDef.restrictions.researchProjectDefs.Count > 0)
                {
                    foreach (ResearchProjectDef item4 in raceDef.restrictions.researchProjectDefs)
                    {
                        AddRestriction(ref researchRestrictions, item4, raceDef);
                    }
                }

                if (raceDef.restrictions.traits.Count > 0)
                {
                    foreach (TraitDef item5 in raceDef.restrictions.traits)
                    {
                        AddRestriction(ref traitRestrictions, item5, raceDef);
                    }
                }
                if (raceDef.restrictions.thoughtDefs.Count > 0)
                {
                    foreach (ThoughtDef item6 in raceDef.restrictions.thoughtDefs)
                    {
                        AddRestriction(ref thoughtRestrictions, item6, raceDef);
                    }
                }
                if (raceDef.restrictions.equippablesWhitelist.Count > 0)
                {
                    foreach (ThingDef item7 in raceDef.restrictions.equippablesWhitelist)
                    {
                        AddRestriction(ref equipabblbleWhiteLists, item7, raceDef);
                    }
                }

                if (raceDef.restrictions.bedDefs.Count > 0)
                {
                    foreach (ThingDef item8 in raceDef.restrictions.bedDefs)
                    {
                        AddRestriction(ref bedRestrictions, item8, raceDef);
                    }
                }

                if (raceDef.restrictions.bodyTypes.Count > 0)
                {
                    foreach (BodyTypeDef item9 in raceDef.restrictions.bodyTypes)
                    {

                        AddRestriction(ref bodyTypeRestrictions, item9, raceDef);
                    }
                }
                foreach (string id in raceDef.restrictions.modContentRestrictionsApparelWhiteList)
                {
                    ModContentPack mod = GetPackByID(id);
                    if (mod != null)
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel)))
                        {
                            AddRestriction(ref equipabblbleWhiteLists, def, raceDef);
                        }
                    }
                }
                foreach (string id in raceDef.restrictions.modContentRestrictionsApparelList)
                {
                    ModContentPack mod = GetPackByID(id);
                    if (mod != null)
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel)))
                            AddRestriction(ref equipmentRestrictions, def, raceDef);
                    }
                }

                foreach (string id in raceDef.restrictions.modResearchRestrictionsList)
                {
                    ModContentPack mod = GetPackByID(id);
                    if (mod != null)
                    {
                        foreach (ResearchProjectDef def in mod.AllDefs.Where(x => x is ResearchProjectDef))
                            AddRestriction(ref researchRestrictions, def, raceDef);
                    }
                }


                foreach (string id in raceDef.restrictions.modTraitRestrictions)
                {
                    ModContentPack mod = GetPackByID(id);
                    if (mod != null)
                    {
                        foreach (TraitDef def in mod.AllDefs.Where(x => x is TraitDef))
                            AddRestriction(ref traitRestrictions, def, raceDef);
                    }
                }

                foreach (string id in raceDef.restrictions.modBuildingRestrictions)
                {
                    ModContentPack mod = GetPackByID(id);
                    if (mod != null)
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && !thingDef.IsApparel))
                            AddRestriction(ref buildingRestrictions, def, raceDef);
                    }
                }

                foreach (string id in raceDef.restrictions.modConsumables)
                {
                    ModContentPack mod = GetPackByID(id);
                    if (mod != null)
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef))
                            AddRestriction(ref consumableRestrictions, def, raceDef);
                    }
                }
                foreach (BodyTypeDef bodyType in raceDef.bodyTypes)
                {
                    AddRestriction(ref bodyDefs, raceDef, bodyType);
                }

                bool useHumanRecipes = raceDef.useHumanRecipes;
                if (useHumanRecipes)
                {
                    foreach (RecipeDef recipeDef in Enumerable.Where(DefDatabase<RecipeDef>.AllDefsListForReading, (RecipeDef x) => x.recipeUsers != null && x.recipeUsers.Contains(ThingDefOf.Human)))
                    {
                        recipeDef.recipeUsers.Add(raceDef);
                        recipeDef.recipeUsers.RemoveDuplicates();
                    }
                    if (raceDef.recipes == null)
                    {
                        raceDef.recipes = new List<RecipeDef>();
                    }
                    List<BodyPartDef> list = new List<BodyPartDef>();
                    foreach (BodyPartRecord bodyPartRecord in raceDef.race.body.AllParts)
                    {
                        list.Add(bodyPartRecord.def);
                    }
                    foreach (RecipeDef recipeDef2 in Enumerable.Where(ThingDefOf.Human.recipes, (RecipeDef recipe) => recipe.targetsBodyPart || !recipe.appliedOnFixedBodyParts.NullOrEmpty()))
                    {
                        foreach (BodyPartDef bodyPartDef in Enumerable.Intersect(recipeDef2.appliedOnFixedBodyParts, list))
                        {
                            raceDef.recipes.Add(recipeDef2);
                        }
                    }
                    raceDef.recipes.RemoveDuplicates();
                }
            }
            Log.Message("[RimVali Core/RVR]: Setting up faction restrictions.");
            List<FactionResearchRestrictionDef> factionDefs = DefDatabase<FactionResearchRestrictionDef>.AllDefsListForReading;
            int factionResDefCount = factionDefs.Count;
            for (int faction = 0; faction < factionResDefCount; faction++)
            {
                Log.Message($"[RimVali Core]: Doing faction restrictions for faction {faction}/{factionResDefCount} ");
                FactionResearchRestrictionDef factionResearchRestrictionDef = factionDefs[faction];
                foreach (FactionResearchRestriction factionResearchRestriction in factionResearchRestrictionDef.factionResearchRestrictions)
                {
                    FacRes res = new FacRes(factionResearchRestriction.researchProj, factionResearchRestriction.isHackable);
                    if (!factionResearchRestrictions.ContainsKey(factionResearchRestriction.factionDef))
                    {
                        factionResearchRestrictions.Add(factionResearchRestriction.factionDef, new List<FacRes>());
                    }
                    factionResearchRestrictions[factionResearchRestriction.factionDef].Add(res);
                }
                foreach (FactionResearchRestriction factionResearchRestriction2 in factionResearchRestrictionDef.factionResearchRestrictionBlackList)
                {
                    FacRes res = new FacRes(factionResearchRestriction2.researchProj, factionResearchRestriction2.isHackable);

                    if (!factionResearchBlacklist.ContainsKey(factionResearchRestriction2.factionDef))
                    {
                        factionResearchBlacklist.Add(factionResearchRestriction2.factionDef, new List<FacRes>());
                    }

                    factionResearchBlacklist[factionResearchRestriction2.factionDef].Add(res);
                }
            }
        }
    }

    #endregion Restrictions and patching

   
    #region Apparel score gain patch

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
    public static class ApparelScorePatch
    {
        [HarmonyPostfix]
        public static void ApparelScoreGain_NewTmp(Pawn pawn, Apparel ap, List<float> wornScoresCache, ref float __result)
        {
            ThingDef def = ap.def;
            if (!ApparelPatch.CanWear(def, pawn.def))
            {
                __result = -100;
                return;
            }
        }
    }

    #endregion Apparel score gain patch

    #region Butcher patch

    [HarmonyPatch(typeof(Corpse), "ButcherProducts")]
    public static class ButcherPatch
    {
        //Gets the thought for butchering.
        private static void ButcheredThoughAdder(Pawn pawn, Pawn butchered, bool butcher = true)
        {
            if (butchered.RaceProps.Humanlike)
            {
                #region stories

                try
                {
                    //Backstories
                    if (!DefDatabase<RVRBackstory>.AllDefs.Where(x => x.hasButcherThoughtOverrides == true && (x.defName == pawn.story.adulthood.identifier || x.defName == pawn.story.childhood.identifier)).EnumerableNullOrEmpty())
                    {
                        butcherAndHarvestThoughts butcherAndHarvestThoughts = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == pawn.story.adulthood.identifier || x.defName == pawn.story.childhood.identifier).First().butcherAndHarvestThoughtOverrides;
                        try
                        {
                            if (butcherAndHarvestThoughts.butcherThoughts.Any(x => x.race == butchered.def))
                            {
                                raceButcherThought rBT = butcherAndHarvestThoughts.butcherThoughts.Find(x => x.race == butchered.def);
                                if (pawn.RaceProps.Humanlike)
                                {
                                    if (butcher)
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.butcheredPawnThought);
                                        return;
                                    }
                                    else
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.knowButcheredPawn);
                                        return;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.Message);
                        }

                        if (butcherAndHarvestThoughts.careAboutUndefinedRaces && pawn.RaceProps.Humanlike)
                        {
                            if (butcher)
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                                return;
                            }
                            else
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                                return;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                #endregion stories

                #region races

                #region RVR races

                //Races
                if (pawn.def is RimValiRaceDef def)
                {
                    butcherAndHarvestThoughts butcherAndHarvestThoughts = def.butcherAndHarvestThoughts;
                    if (butcherAndHarvestThoughts.butcherThoughts.Any(x => x.race == butchered.def) && pawn.RaceProps.Humanlike)
                    {
                        raceButcherThought rBT = butcherAndHarvestThoughts.butcherThoughts.Find(x => x.race == butchered.def);
                        if (butcher)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.butcheredPawnThought);
                            return;
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.knowButcheredPawn);
                            return;
                        }
                    }
                    if (def.butcherAndHarvestThoughts.careAboutUndefinedRaces && pawn.RaceProps.Humanlike)
                    {
                        if (butcher)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                            return;
                        }
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                        return;
                    }
                }

                #endregion RVR races

                //If the pawn is not from RVR.

                if (!(pawn.def is RimValiRaceDef) && pawn.RaceProps.Humanlike)
                {
                    if (butcher)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse, null);
                    }

                    return;
                }
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
            }

            #endregion races
        }

        [HarmonyPrefix]
        public static bool Patch(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            if (Harmony.HasAnyPatches("rimworld.erdelf.alien_race.main"))
            {
                return true;
            }
            TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, new object[] { butcher });
            Pawn deadPawn = __instance.InnerPawn;

            __result = deadPawn.ButcherProducts(butcher, efficiency);
            /*
            if (!(deadPawn.def is RimValiRaceDef))
            {
                return false;
            }
            */
            bool butcheredThought = false;
            if (deadPawn.RaceProps.Humanlike)
            {
                if (butcher.def is RimValiRaceDef)
                {
                    ButcheredThoughAdder(butcher, deadPawn, true);
                    butcheredThought = true;
                }
                foreach (Pawn targetPawn in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction))
                {
                    if (targetPawn != butcher)
                    {
                        Log.Message(targetPawn.Name.ToStringFull);
                        ButcheredThoughAdder(targetPawn, deadPawn, false);
                    }
                    else if (!butcheredThought)
                    {
                        Log.Message($"Butcher: {targetPawn.Name.ToStringFull}");
                        ButcheredThoughAdder(targetPawn, deadPawn);
                    }
                }
            }

            return false;
        }
    }

    #endregion Butcher patch

    #region Backstory patch

    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
    public class StoryPatch
    {
        private static float SelectionWeightFactorFromWorkTagsDisabled(WorkTags wt)
        {
            float num = 1f;
            if ((wt & WorkTags.ManualDumb) != WorkTags.None)
            {
                num *= 0.5f;
            }
            if ((wt & WorkTags.ManualSkilled) != WorkTags.None)
            {
                num *= 1f;
            }
            if ((wt & WorkTags.Violent) != WorkTags.None)
            {
                num *= 0.6f;
            }
            if ((wt & WorkTags.Social) != WorkTags.None)
            {
                num *= 0.7f;
            }
            if ((wt & WorkTags.Intellectual) != WorkTags.None)
            {
                num *= 0.4f;
            }
            if ((wt & WorkTags.Firefighting) != WorkTags.None)
            {
                num *= 0.8f;
            }
            return num;
        }

        private static float BackstorySelectionWeight(Backstory bs)
        {
            return SelectionWeightFactorFromWorkTagsDisabled(bs.workDisables);
        }

        private static void FillBackstorySlotShuffled(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
        {
            BackstoryCategoryFilter backstoryCategoryFilter = backstoryCategories.RandomElementByWeight((BackstoryCategoryFilter c) => c.commonality);
            if (backstoryCategoryFilter == null)
            {
                Log.Error("Backstory category filter was null");
            }
            if (!(from bs in BackstoryDatabase.ShuffleableBackstoryList(slot, backstoryCategoryFilter).TakeRandom(20)
                  where slot != BackstorySlot.Adulthood || !bs.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables)
                  select bs).TryRandomElementByWeight(new Func<Backstory, float>(BackstorySelectionWeight), out backstory))
            {
                Log.Error($"No shuffled {slot} found for {pawn.ToStringSafe()} of {factionType.ToStringSafe()}. Choosing random.");
                backstory = (from kvp in BackstoryDatabase.allBackstories
                             where kvp.Value.slot == slot
                             select kvp).RandomElement().Value;
                string id = backstory.identifier;
                if (DefDatabase<RVRBackstory>.AllDefsListForReading.Any(x => x.defName == id))
                {
                    RVRBackstory story = DefDatabase<RVRBackstory>.AllDefsListForReading.First(x => x.defName == id);
                    if (story.defName == backstory.identifier && !story.CanSpawn(pawn))
                    {
                        FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, pawn.Faction.def.backstoryFilters, factionType);
                    }
                }
            }
        }

        [HarmonyPostfix]
        public static void CheckStory(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
        {
            string id = backstory.identifier;
            if (DefDatabase<RVRBackstory>.AllDefsListForReading.Any(x => x.defName == id))
            {
                RVRBackstory story = DefDatabase<RVRBackstory>.AllDefsListForReading.First(x => x.defName == id);
                if (story.defName == backstory.identifier && !story.CanSpawn(pawn))
                {
                    FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, pawn.Faction.def.backstoryFilters, factionType);
                }
            }
        }
    }

    #endregion Backstory patch

    #region Base Head Offset patch

    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class HeadPatch
    {
        [HarmonyPostfix]
        public static void SetPos(ref Vector3 __result, PawnRenderer __instance)
        {
            Pawn pawn = __instance.graphics.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                rimValiRaceDef.HeadOffsetPawn(__instance, ref __result);
            }
        }
    }

    #endregion Base Head Offset patch

    #region Body gen patch

    //Generation patch for bodytypes
    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
    public static class BodyPatch
    {
        public static IEnumerable<BodyTypeDef> BodyTypes(Pawn p)
        {
            List<BodyTypeDef> getAllAvalibleBodyTypes = new List<BodyTypeDef>();
            if (Restrictions.bodyDefs.ContainsKey(p.def)) { getAllAvalibleBodyTypes.AddRange(Restrictions.bodyDefs[p.def]); }
            if (getAllAvalibleBodyTypes.NullOrEmpty()) { getAllAvalibleBodyTypes.AddRange(new List<BodyTypeDef> { BodyTypeDefOf.Fat, BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin }); }
            getAllAvalibleBodyTypes.AddRange(getAllAvalibleBodyTypes.NullOrEmpty() ? new List<BodyTypeDef> { BodyTypeDefOf.Fat, BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin } : new List<BodyTypeDef>());
            getAllAvalibleBodyTypes.Add(p.gender == Gender.Female ? BodyTypeDefOf.Female : BodyTypeDefOf.Male);

            return getAllAvalibleBodyTypes;
        }

        public static void SetBody(RVRBackstory story, ref Pawn pawn)
        {
            RimValiRaceDef rimValiRace = pawn.def as RimValiRaceDef;
            if (story.bodyDefOverride != null) { pawn.RaceProps.body = story.bodyDefOverride; }
            if (story.bodyType != null) { pawn.story.bodyType = story.bodyType; }
            else { pawn.story.bodyType = rimValiRace.bodyTypes.RandomElement(); }
            Log.Message($"Pawn bodytype: {pawn.story.bodyType}");
        }

        [HarmonyPostfix]
        public static void Patch(ref Pawn pawn)
        {
            Pawn p2 = pawn;
            if (pawn.def is RimValiRaceDef rimValiRace)
            {
                try
                {
                    pawn.story.crownType = CrownType.Average;
                    if ((pawn.story.adulthood != null && DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.adulthood.identifier).Count() > 0))
                    {
                        RVRBackstory story = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.adulthood.identifier).FirstOrDefault();
                        SetBody(story, ref pawn);
                        return;
                    }
                    else if (DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).Count() > 0)
                    {
                        RVRBackstory story = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).FirstOrDefault();
                        SetBody(story, ref pawn);
                        return;
                    }
                    else { pawn.story.bodyType = rimValiRace.bodyTypes.RandomElement(); }
                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                    Log.Message("Trying again...");
                    Patch(ref pawn);
                }
                if (!rimValiRace.hasHair) { pawn.story.hairDef = DefDatabase<HairDef>.AllDefs.ToList().Find(x => x.defName.ToLower() == "rvrnohair"); }
            }
            else
            {
                if (pawn.def.GetType().Name != "ThingDef_AlienRace")
                {
                    if (pawn.story.bodyType == null || !BodyTypes(pawn).Contains(pawn.story.bodyType)) { pawn.story.bodyType = BodyTypes(pawn).RandomElement(); };
                    Log.Message(pawn.story.bodyType.defName);
                }
            }
        }
    }

    #endregion Body gen patch

    #region Bed patch

    [HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
    public class BedPatch
    {
        [HarmonyPostfix]
        public static void BedPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            __result = __result && Restrictions.CheckRestrictions(Restrictions.bedRestrictions, bedDef, p.def);
        }
    }

    #endregion Bed patch

    #region Research restriction patch

    [HarmonyPatch(typeof(WorkGiver_Researcher), "ShouldSkip")]
    public class ResearchPatch
    {
        [HarmonyPostfix]
        public static void Research(Pawn pawn, ref bool __result)
        {
            //Log.Message("test");
            if (Find.ResearchManager.currentProj != null)
            {
                // Log.Message($"Is blacklisted: {(Restrictions.factionResearchBlacklist.ContainsKey(pawn.Faction.def) && Restrictions.factionResearchBlacklist[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj))}");
                if (!Restrictions.CheckRestrictions(Restrictions.researchRestrictions, Find.ResearchManager.currentProj, pawn.def) || (Restrictions.factionResearchRestrictions.ContainsKey(pawn.Faction.def) && !Restrictions.factionResearchRestrictions[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj)) || (Restrictions.factionResearchBlacklist.ContainsKey(pawn.Faction.def) && Restrictions.factionResearchBlacklist[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj)))
                {
                    bool isHacked;
                    isHacked = !Restrictions.hackedProjects.EnumerableNullOrEmpty() && !(Restrictions.hackedProjects.ContainsKey(Find.ResearchManager.currentProj) || Restrictions.hackedProjects[Find.ResearchManager.currentProj] == false);
                    if (!isHacked)
                    {
                        __result = false;
                    }
                }
                __result = true && __result;
            }
        }
    }

    #endregion Research restriction patch

    #region Pawnkind replacement

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
    public static class GeneratorPatch
    {
        [HarmonyPrefix]
        public static void GeneratePawn(ref PawnGenerationRequest request)
        {
            if (request.KindDef != null)
            {
                PawnKindDef pawnKindDef = request.KindDef;
                IEnumerable<RimValiRaceDef> races = DefDatabase<RimValiRaceDef>.AllDefsListForReading;
                for (int raceIndex = 0; raceIndex < races.Count() - 1; raceIndex++)
                {
                    RimValiRaceDef race = races.ToList()[raceIndex];
                    RVRRaceInsertion inserter = race.raceInsertion;
                    if (Rand.Range(0, 100) < inserter.globalChance)
                    {
                        if (pawnKindDef == PawnKindDefOf.Slave)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isSlave && Rand.Range(0, 100) < entry.chance)
                                {
                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        request.ForceBodyType = race.bodyTypes.RandomElement();
                                    }
                                    break;
                                }
                            }
                        }
                        else if (pawnKindDef == PawnKindDefOf.Villager)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isVillager && Rand.Range(0, 100) < entry.chance)
                                {
                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        race.bodyTypes.RandomElement();
                                    }
                                    break;
                                }
                            }
                        }
                        else if (pawnKindDef == PawnKindDefOf.SpaceRefugee)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isRefugee && Rand.Range(0, 100) < entry.chance)
                                {
                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        race.bodyTypes.RandomElement();
                                    }
                                    break;
                                }
                            }
                        }
                        else if (pawnKindDef == PawnKindDefOf.Drifter)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isWanderer && Rand.Range(0, 100) < entry.chance)
                                {
                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        race.bodyTypes.RandomElement();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion Pawnkind replacement

    #region Apparel gen patch

    [HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
    public class ApparelGenPatch
    {
        private static List<ThingStuffPair> pairs = new List<ThingStuffPair>();
        public static void GenerateStartingApparelForPostfix() =>
            Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs").GetValue<List<ThingStuffPair>>().AddRange(pairs);
        [HarmonyPrefix]
        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            try
            {
                Traverse apparelInfo = Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs");
                List<ThingStuffPair> thingStuffPairs = apparelInfo.GetValue<List<ThingStuffPair>>().Where(x => ApparelPatch.CanWear(x.thing, pawn.def)).ToList();
                apparelInfo.SetValue(thingStuffPairs);
            }
            catch (Exception e) { Log.Error($"Oops! RV:C had an issue generating apparel: {e.Message}"); }
        }
    }

    #endregion Apparel gen patch

    #region Trait patch

    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public class TraitPatch
    {
        [HarmonyPrefix]
        public static bool TraitGain(Trait trait, TraitSet __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            bool value = pawn.def is RimValiRaceDef rDef ? (!rDef.restrictions.disabledTraits.NullOrEmpty() && rDef.restrictions.disabledTraits.Contains(trait.def)) || Restrictions.CheckRestrictions(Restrictions.traitRestrictions, trait.def, pawn.def) : Restrictions.CheckRestrictions(Restrictions.traitRestrictions, trait.def, pawn.def);
            return value;
        }
    }

    #endregion Trait patch

    #region Organ harvest patch

    [HarmonyPatch(typeof(ThoughtUtility), "GiveThoughtsForPawnOrganHarvested")]
    public static class OrganPatch
    {
        [HarmonyPostfix]
        public static void Patch(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            if (victim.def is RimValiRaceDef raceDef)
            {
                victim.needs.mood.thoughts.memories.TryGainMemory(raceDef.butcherAndHarvestThoughts.myOrganHarvested, null);
            }
            else
            {
                victim.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested, null);
            }
            foreach (Pawn pawn in victim.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.needs.mood != null && pawn != victim && pawn.def is RimValiRaceDef rDef)
                {

                    foreach (raceOrganHarvestThought thoughts in rDef.butcherAndHarvestThoughts.harvestedThoughts)
                    {
                        if (victim.def == thoughts.race)
                        {
                            if (victim.IsColonist && (thoughts.colonistThought != null))
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(thoughts.colonistThought);
                            }
                            else if (!victim.IsColonist && thoughts.guestThought != null)
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(thoughts.guestThought);
                            }
                            else if (thoughts.colonistThought != null)
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(thoughts.colonistThought);
                            }
                            else
                            {
                                Log.Error("Undefined thought in " + rDef.defName + " butcherAndHarvestThoughts/harvestedThoughts!");
                            }
                        }
                        else if (rDef.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                        {
                            if (victim.IsColonist)
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowColonistOrganHarvested);
                            }
                            else
                            {
                                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowGuestOrganHarvested);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion Organ harvest patch

    #region FactionGen patch

    [HarmonyPatch(typeof(Faction), "TryMakeInitialRelationsWith")]
    public static class FactionGenPatch
    {
        [HarmonyPostfix]
        public static void Patch(Faction __instance, Faction other)
        {
            foreach (FactionStartRelationDef def in DefDatabase<FactionStartRelationDef>.AllDefs.Where(fac => fac.faction == __instance.def))
            {
                foreach (FacRelation relation in def.relations)
                {
                    if (other.def == relation.otherFaction)
                    {
                        FactionRelation rel = other.RelationWith(__instance);
                        rel.baseGoodwill = relation.relation;
                    }
                }
            }
        }
    }

    #endregion FactionGen patch

    #region Health offset patch

    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    public static partial class BodyPartHealthPatch
    {
        [HarmonyPostfix]
        public static void Patch(ref float __result, Pawn pawn, BodyPartDef __instance)
        {
            float num = 0f;
            float otherNum = 0f;
            if (pawn.health.hediffSet.hediffs != null)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part != null && hediff.Part.def == __instance))
                {
                    if (hediff.CurStage != null && !hediff.CurStage.statOffsets.NullOrEmpty())
                    {
                        foreach (StatModifier statModifier in hediff.CurStage.statOffsets.Where((StatModifier x) => x.stat != null && x.stat.defName == "HealthIncreasePercent"))
                        {
                            num += statModifier.value;
                        }
                        foreach (StatModifier statModifier in hediff.CurStage.statOffsets.Where((StatModifier x) => x.stat != null && x.stat.defName == "HealthIncreaseAdd"))
                        {
                            otherNum += statModifier.value;
                        }
                    }
                }
            }
            if (num > 0)
            {
                __result = Mathf.CeilToInt(__instance.hitPoints * pawn.HealthScale * num) + otherNum;
            }
            else
            {
                __result = Mathf.CeilToInt(__instance.hitPoints * pawn.HealthScale) + otherNum;
            }
            return;
        }
    }

    #endregion Health offset patch

    #region Cannibalism patch

    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class IngestingPatch
    {
        [HarmonyPostfix]
        public static void Patch(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<FoodUtility.ThoughtFromIngesting> __result)
        {
            bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);
            if (ingester.def is RimValiRaceDef rDef)
            {
                for (int a = 0; a < __result.Count - 1; a++)
                {
                    ThoughtDef t = __result[a].thought;

                    #region raw

                    if (t == ThoughtDefOf.AteHumanlikeMeatDirectCannibal || t == ThoughtDefOf.AteHumanlikeMeatDirect)
                    {
                        ThingDef r = foodDef.ingestible.sourceDef;
                        if (r != null)
                        {
                            if (rDef.GetEatenThought(r, true, cannibal) != null)
                            {
                                __result[a] = new FoodUtility.ThoughtFromIngesting
                                {
                                    thought = rDef.GetEatenThought(r, true, cannibal)
                                };
                            }
                            else if (!rDef.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                            {
                                __result.RemoveAt(a);
                            }
                        }
                    }

                    #endregion raw

                    #region cooked

                    if (t == ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal || t == ThoughtDefOf.AteHumanlikeMeatAsIngredient)
                    {
                        ThingDef r = foodSource.TryGetComp<CompIngredients>().ingredients.Where(food => food.ingestible != null && rDef.butcherAndHarvestThoughts.butcherThoughts.Any(x => x.race == food.ingestible.sourceDef)).ToList()[0];
                        if (foodSource.TryGetComp<CompIngredients>() != null && !foodSource.TryGetComp<CompIngredients>().ingredients.NullOrEmpty())
                        {
                            for (int b = 0; b < foodSource.TryGetComp<CompIngredients>().ingredients.Count - 1; b++)
                            {
                                ThingDef ing = foodSource.TryGetComp<CompIngredients>().ingredients[b];
                                if (rDef.GetEatenThought(ing.ingestible.sourceDef, false, cannibal) != null)
                                {
                                    int pos = __result.FindIndex(x => cannibal ? x.thought == ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : x.thought == ThoughtDefOf.AteHumanlikeMeatAsIngredient);
                                    __result[pos] = new FoodUtility.ThoughtFromIngesting { thought = rDef.GetEatenThought(ing.ingestible.sourceDef, false, cannibal) };
                                }
                            }
                        }
                    }

                    #endregion cooked
                }
            }
        }
    }

    #endregion Cannibalism patch

    #region Thought patches

    [HarmonyPatch(typeof(ThoughtUtility), "CanGetThought")]
    public static class ThoughtPatch
    {
        [HarmonyPostfix]
        public static void CanGetPatch(Pawn pawn, ThoughtDef def, bool checkIfNullified, ref bool __result)
        {
            __result = __result && (pawn.def is RimValiRaceDef rDef ? rDef.canHavethoughts && !(!rDef.restrictions.thoughtBlacklist.NullOrEmpty() && rDef.restrictions.thoughtBlacklist.Contains(def)) : Restrictions.CheckRestrictions(Restrictions.thoughtRestrictions, def, pawn.def));
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "GetFirstMemoryOfDef")]
    public static class ThoughtReplacerPatchGetFirstMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "NumMemoriesOfDef")]
    public static class ThoughtReplacerPatchNumMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "OldestMemoryOfDef")]
    public static class ThoughtReplacerPatchOldestMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemoriesOfDef")]
    public static class ThoughtReplacerPatchRemoveRemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemoriesOfDefIf")]
    public static class ThoughtReplacerPatchRemoveRemoriesOfDefIf
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class MemGain
    {
        // Is newThought supposed to be `ref`? The MakeThought and assignment are useless right now.
        [HarmonyPrefix]
        public static bool Patch(Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref newThought.def);

                newThought = ThoughtMaker.MakeThought(newThought.def, newThought.CurStageIndex);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SituationalThoughtHandler), "TryCreateThought")]
    public static class ThoughtReplacerPatchSituational
    {
        [HarmonyPrefix]
        public static void ReplaceThought(ref ThoughtDef def, SituationalThoughtHandler __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                rimValiRaceDef.ReplaceThought(ref def);
            }
        }
    }

    #endregion Thought patches

    #region Name patch

    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
    public static class NameFix
    {
        [HarmonyPrefix]
        public static bool Patch(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
        {
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                string nameString = NameGenerator.GenerateName(rimValiRaceDef.race.GetNameGenerator(pawn.gender));
                NameTriple name = NameTriple.FromString(nameString);
                if (pawn.def.defName == "RimVali" && UnityEngine.Random.Range(1, 100) == 30)
                {

                    __result = new NameTriple(UnityEngine.Random.Range(1, 100) != 30 ? name.First : SteamUtility.SteamPersonaName, name.Nick ?? name.First, name.Last);
                    return true;
                }
                __result = new NameTriple(name.First, name.Nick ?? name.First, name.Last);

            }
            else
            {
                return true;
            }
            return false;
        }
    }

    #endregion Name patch

    #region Food Eating

    //I dont think these patches interefere with HAR, nor should HAR patches interefere with these?

    //Was going to patch WillEat, but this seems better? I'd imagine they still *could* eat it by force if i patched WillEat.
    [HarmonyPatch(typeof(RaceProperties), "CanEverEat", new[] { typeof(ThingDef) })]
    public static class FoodPatch
    {
        private static readonly Dictionary<RaceProperties, ThingDef> cachedDefs = new Dictionary<RaceProperties, ThingDef>();

        [HarmonyPostfix]
        public static void EdiblePatch(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            ThingDef pawn;
            if (cachedDefs.NullOrEmpty() || !cachedDefs.ContainsKey(__instance))
            {
                pawn = DefDatabase<ThingDef>.AllDefs.First(x => x.race == __instance);
                cachedDefs.Add(__instance, pawn);
            }
            else
            {
                pawn = cachedDefs[__instance];
            }
            bool canEat = pawn != null && (Restrictions.CheckRestrictions(Restrictions.consumableRestrictions, t, pawn) || Restrictions.CheckRestrictions(Restrictions.consumableRestrictionsWhiteList, t, pawn,false));
            if (!canEat)
            {
                JobFailReason.Is($"{pawn.label} {"CannotEatRVR".Translate(pawn.label.Named("RACE"))}");
            }
            //No "Consume grass" for you.
            __result = __result && canEat;
        }
    }

    #endregion Food Eating

    #region Apparel Equipping

    public static class ApparelPatch
    {
        public static bool CanWear(ThingDef def, ThingDef race)
        {
            bool whiteListed = (Restrictions.equipabblbleWhiteLists.ContainsKey(def) && Restrictions.equipabblbleWhiteLists[def].Contains(race));
            bool blackListed = Restrictions.equipmentRestrictions.ContainsKey(def) && Restrictions.equipmentRestrictions[def].Contains(race);
            bool isRestrictedToOnlyApprovedClothes = race is RimValiRaceDef rimValiRace && rimValiRace.restrictions.canOnlyUseApprovedApparel;
            bool itemIsRestricted = !isRestrictedToOnlyApprovedClothes && !Restrictions.equipmentRestrictions.ContainsKey(def);
            
            bool result = blackListed || whiteListed || itemIsRestricted;
            return result;
        }

        public static void Equipable(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (thing.def.IsApparel)
            {
                __result = __result && CanWear(thing.def, pawn.def);
                if (!__result)
                {
                    cantReason = "CannotWearRVR".Translate(pawn.def.label.Named("RACE"));
                }
            }
        }
    }

    #endregion Apparel Equipping

    #region Construction

    [HarmonyPatch(typeof(GenConstruct), "CanConstruct", new[] { typeof(Thing), typeof(Pawn), typeof(WorkTypeDef), typeof(bool) })]
    //This was confusing at first, but it works.
    public static class ConstructPatch
    {
        [HarmonyPostfix]
        public static void Constructable(Thing t, Pawn pawn, WorkTypeDef workType, bool forced, ref bool __result)
        {
            //Log.Message(t.def.ToString());
            if (!Restrictions.CheckRestrictions(Restrictions.buildingRestrictions, t.def.entityDefToBuild, pawn.def))
            {
                __result = false;
                JobFailReason.Is(pawn.def.label + " " + "CannotBuildRVR".Translate(pawn.def.label.Named("RACE")));
            }
            __result = true && __result;
        }
    }

    #endregion Construction

    #region ResolveAllGraphics patch

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolvePatch
    {
        [HarmonyPrefix]
        public static bool ResolveGraphics(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (!(pawn.def is RimValiRaceDef))
                return true;

            RimValiRaceDef rimValiRaceDef = (RimValiRaceDef)pawn.def;
            try
            {
                raceColors graphics = rimValiRaceDef.graphics;
                ColorComp colorComp = pawn.TryGetComp<ColorComp>();

                if (colorComp.colors == null || colorComp.colors.Count() == 0)
                {
                    rimValiRaceDef.GenGraphics(pawn);
                }
                if (!ColorInfo.sets.ContainsKey(pawn.GetHashCode().ToString()))
                {
                    ColorInfo.sets.Add(pawn.GetHashCode().ToString(), __instance);
                }
                List<Colors> colors = graphics.colorSets;
                if (graphics.skinColorSet != null)
                {
                    TriColor_ColorGenerators generators = colors.First(x => x.name == graphics.skinColorSet).colorGenerator;
                    Color color1 = generators.firstColor.NewRandomizedColor();
                    Color color2 = generators.secondColor.NewRandomizedColor();
                    Color color3 = generators.thirdColor.NewRandomizedColor();
                    AvaliGraphic nakedGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.bodyTex, ContentFinder<Texture2D>.Get(graphics.bodyTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.bodySize, color1, color2, color3);
                    __instance.nakedGraphic = nakedGraphic;

                    //Find the pawns head graphic and set it..
                    AvaliGraphic headGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, color1, color2, color3);
                    __instance.headGraphic = headGraphic;
                    __instance.desiccatedHeadGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                             AvaliShaderDatabase.Tricolor, graphics.headSize, color1, color2, color3);
                    __instance.dessicatedGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, color1, color2, color3);
                    __instance.rottingGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, PawnGraphicSet.RottingColorDefault, PawnGraphicSet.RottingColorDefault, PawnGraphicSet.RottingColorDefault);
                    //First, let's get the pawns hair texture.
                    AvaliGraphic hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(__instance.pawn.story.hairDef.texPath + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                AvaliShaderDatabase.Tricolor, graphics.headSize, pawn.story.SkinColor);
                    //Should the race have hair?
                    if (!rimValiRaceDef.hasHair)
                    {
                        //This leads to a blank texture. So the pawn doesnt have hair, visually. I might (and probably should) change this later.
                        hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");
                    }
                    __instance.hairGraphic = hairGraphic;

                    __instance.headStumpGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");
                    __instance.desiccatedHeadStumpGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");
                    __instance.skullGraphic = headGraphic;

                    __instance.MatsBodyBaseAt(pawn.Rotation);
                }
                else
                {
                    //This is the "body" texture of the pawn.

                    AvaliGraphic nakedGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.bodyTex, ContentFinder<Texture2D>.Get(graphics.bodyTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.bodySize, pawn.story.SkinColor, Color.green, Color.red);
                    __instance.nakedGraphic = nakedGraphic;

                    //Find the pawns head graphic and set it..
                    AvaliGraphic headGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, pawn.story.SkinColor, Color.green, Color.red);
                    __instance.headGraphic = headGraphic;

                    //First, let's get the pawns hair texture.
                    AvaliGraphic hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, pawn.story.SkinColor);

                    //Should the race have hair?
                    if (!rimValiRaceDef.hasHair)
                    {
                        //This leads to a blank texture. So the pawn doesnt have hair, visually. I might (and probably should) change this later.
                        hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");
                    }
                    __instance.hairGraphic = hairGraphic;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            __instance?.ResolveApparelGraphics();
            PortraitsCache.SetDirty(pawn);
            return false;
        }
    }

    #endregion ResolveAllGraphics patch

    public static class ColorInfo
    {
        public static Dictionary<string, PawnGraphicSet> sets = new Dictionary<string, PawnGraphicSet>();
    }

    #region Rendering patch



  

   [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags) })]
    internal static class RendererPatch
    {
 
        public class RSet
        {
            public RotDrawMode mode = default;
            public PawnRenderer renderer = default;

        }

        public static Dictionary<Pawn, RSet> renders = new Dictionary<Pawn, RSet>();
        public static Dictionary<Pawn, List<RenderableDef>> pawnRenderables = new Dictionary<Pawn, List<RenderableDef>>();

        private static Vector3 GetPosition(RenderableDef renderable, Rot4 rotation)
        {
            if (renderable.west == null)
            {
                renderable.west = new BodyPartGraphicPos();
                renderable.west.position.x = -renderable.east.position.x;
                renderable.west.position.y = -renderable.east.position.y;
                renderable.west.size = renderable.east.size;
                renderable.west.layer = renderable.east.layer;
            }

            Vector3 offset = rotation == Rot4.East ? new Vector3(renderable.east.position.x, renderable.east.layer, renderable.east.position.y) :
                rotation == Rot4.North ? new Vector3(renderable.north.position.x, renderable.north.layer, renderable.north.position.y) :
                rotation == Rot4.South ? new Vector3(renderable.south.position.x, renderable.south.layer, renderable.south.position.y) :
                new Vector3(renderable.west.position.x, renderable.west.layer, renderable.west.position.y);

            return offset;
        }

        public static Vector2 GetSize(RenderableDef renderable, Rot4 rotation)
        {
            Vector2 size = rotation == Rot4.East ? renderable.east.size : rotation == Rot4.North ? renderable.north.size : rotation == Rot4.South ? renderable.south.size : renderable.west.size;
            return size;
        }

        public static void CalculateDeadColors(ref Color colorOne, ref Color colorTwo, ref Color colorThree, PawnRenderer renderer, RotDrawMode mode)
        {
            Pawn pawn = renderer.graphics.pawn;
            if (pawn.Dead)
            {
                if (mode == RotDrawMode.Dessicated)
                {
                    if (renderer.graphics.dessicatedGraphic.Color != null)
                    {
                        //                This will be changed eventually
                        colorOne *= (renderer.graphics.rottingGraphic.Color);
                        colorTwo *= (renderer.graphics.rottingGraphic.Color);
                        colorThree *= (renderer.graphics.rottingGraphic.Color);
                    }
                }
                else if (mode == RotDrawMode.Rotting && renderer.graphics.rottingGraphic.color != null)
                {
                    colorOne *= new Color(0.34f, 0.32f, 0.3f);
                    colorTwo *= new Color(0.34f, 0.32f, 0.3f);
                    colorThree *= new Color(0.34f, 0.32f, 0.3f);
                }
            }
        }

        public static void GetColors(ref Color colorOne, ref Color colorTwo, ref Color colorThree, ColorComp colorComp, RenderableDef def)
        {
            ColorSet set = colorComp.TryGetColorset(def.useColorSet);
            if (set != null)
            {
                colorOne = set.colorOne;
                colorTwo = set.colorTwo;
                colorThree = set.colorThree;
            }
        }

        public static void RenderBodyParts(bool portrait, float angle, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation, RotDrawMode mode, Pawn pawn, PawnRenderFlags flags)
        {
           

            if (!(pawn.def is RimValiRaceDef))
                return;

            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

            RimValiRaceDef rimValiRaceDef = (RimValiRaceDef)pawn.def;
            HashSet<RenderableDef> renderables = rimValiRaceDef.GetRenderableDefsThatShow(pawn, mode, portrait);
            renderables.AddRange((pawnRenderables.ContainsKey(pawn) ? pawnRenderables[pawn] : new List<RenderableDef>()));
            
            ColorComp colorComp = pawn.TryGetComp<ColorComp>();

            foreach (RenderableDef renderable in renderables)
            { 
                Vector3 offset = GetPosition(renderable, rotation);
                Vector2 size = GetSize(renderable, rotation);

                AvaliGraphic graphic = null;
                if (renderable.useColorSet != null)
                {

                    Color color1 = Color.red;
                    Color color2 = Color.green;
                    Color color3 = Color.blue;

                    GetColors(ref color1, ref color2, ref color3, colorComp, renderable);

                    CalculateDeadColors(ref color1, ref color2, ref color3, pawnRenderer, mode);
                    BaseTex tex = renderable.GetCurrentTexture(pawn, renderable.GetMyIndex(pawn));
                    string mPath = null;
                    if (tex.ProvidesAlternateMask(pawn))
                    {
                        mPath = $"{tex.GetMasks(pawn)[renderable.GetMyMaskIndex(pawn)]}";
                    }
                    graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.TexPath(pawn), AvaliShaderDatabase.Tricolor, size, color1, color2, color3,mPath);
                }
                else
                {
                    BaseTex tex = renderable.GetCurrentTexture(pawn, renderable.GetMyIndex(pawn));
                    string mPath = null;
                    if (tex.ProvidesAlternateMask(pawn))
                    {
                       
                        mPath = $"{tex.GetMasks(pawn)[renderable.GetMyMaskIndex(pawn)]}";
                        graphic.maskPath = $"{mPath}";
                    }
                   
                    graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.TexPath(pawn), AvaliShaderDatabase.Tricolor, size, pawn.story.SkinColor,maskPath:mPath);
                    
                  
                }
                                
                GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f),
                   quaternion, graphic.MatAt(rotation), flags.FlagSet(PawnRenderFlags.DrawNow));
               
            }
        }

        [HarmonyPostfix]
        public static void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, PawnRenderer __instance)
        {
            if (!(__instance.graphics.pawn.def is RimValiRaceDef))
                return;

            bool portrait = flags.HasFlag(PawnRenderFlags.Portrait);
            Rot4 rot4 = portrait ? Rot4.South : bodyFacing;
            RenderBodyParts(portrait, angle, rootLoc, __instance, rot4, bodyDrawType, __instance.graphics.pawn, flags);
        }
    }

    #endregion Rendering patch

    #region RenderTexture

    public class RenderTexturePatch
    {
        private static readonly int texSize = 8000;

        public static RenderTexture NewTexture()
        {
            Vector2Int size = new Vector2Int(GetAtlasSizeWithPawnsOnMap(), GetAtlasSizeWithPawnsOnMap());
            return new RenderTexture(size.x, size.y, 40, RenderTextureFormat.ARGBFloat, 0)
            {
                antiAliasing = 0,
                useMipMap = true,
                mipMapBias = 50f
            };
        }

        public static int GetAtlasSizeWithPawnsOnMap()
        {
            if (RimValiCoreMod.Settings.smartPawnScaling)
            {
                int pawnCount = Find.CurrentMap.mapPawns.AllPawnsSpawnedCount;
                float texSizeDivider = pawnCount / RimValiCoreMod.Settings.textureSizeScaling;
                int textureSize = texSize;
                if (texSizeDivider > 1)
                {
                    textureSize /= (int)(texSizeDivider);
                    return textureSize > RimValiCoreMod.Settings.smallestTexSize ? textureSize : RimValiCoreMod.Settings.smallestTexSize;
                }
            }
            return texSize;
        }
    }

    #endregion RenderTexture

    #region RenderTextureTranspiler

    public static class RenderTextureTranspiler
    {
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            List<CodeInstruction> codes = instructions.ToList();
            int cont = instructions.Count();
            for (int index = 0; index < cont; index++)
            {
                if (VerifyLocation(codes, index))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderTexturePatch),
                                                                                      nameof(RenderTexturePatch.NewTexture)));
                    index += 5;
                }
                else
                {
                    yield return codes[index];
                }
            }
        }

        private static bool VerifyLocation(List<CodeInstruction> codes, int i)
        {
            return i < codes.Count - 5 &&
                   codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 0x800 &&
                   codes[i + 1].opcode == OpCodes.Ldc_I4 && (int)codes[i + 1].operand == 0x800 &&
                   codes[i + 2].opcode == OpCodes.Ldc_I4_S &&
                   codes[i + 3].opcode == OpCodes.Ldc_I4_0 &&
                   codes[i + 4].opcode == OpCodes.Ldc_I4_0 &&
                   codes[i + 5].opcode == OpCodes.Newobj;
        }
    }

    #endregion RenderTextureTranspiler
}
