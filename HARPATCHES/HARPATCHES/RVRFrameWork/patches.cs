
using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    #endregion
    [StaticConstructorOnStartup]
    public static class Restrictions
    {
     
        public static bool checkRestrictions<T, V>(Dictionary<T, List<V>> pairs, T item, V race, bool keyNotInReturn = true)
        {
            if (pairs.ContainsKey(item))
            {
                if (!pairs[item].NullOrEmpty() && pairs[item].Contains(race)){return true;}
            }
            return !pairs.ContainsKey(item) && keyNotInReturn;
        }

        // Token: 0x060000EA RID: 234 RVA: 0x00006D2C File Offset: 0x00004F2C
        public static bool AddRestriction<T, V>(ref Dictionary<T, List<V>> pairs, T item, V race)
        {

            bool flag = !pairs.ContainsKey(item);
            if (flag)
            {
                pairs.Add(item, new List<V>());
                pairs[item].Add(race);
            }
            else
            {
                bool flag2 = pairs[item] != null;
                if (flag2)
                {
                    pairs[item].Add(race);
                    return true;
                }
            }
            return false;
        }

        // Token: 0x060000EB RID: 235 RVA: 0x00006D94 File Offset: 0x00004F94
        static Restrictions()
        {
            Harmony harmony = new Harmony("RimVali.Core");
            try
            {
                harmony.PatchAll();
                Log.Message($"[RimVali Core] Patches completed. {harmony.GetPatchedMethods().EnumerableCount()} methods patched.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimVali Core] A patch has failed! Patches completed: {harmony.GetPatchedMethods().EnumerableCount()}");
                Log.Error(ex.ToString());
            }
            Log.Message("[RimVali Core/RVR]: Setting up race restrictions.");
            foreach (RimValiRaceDef raceDef in DefDatabase<RimValiRaceDef>.AllDefs)
            {
                bool flag = raceDef.restrictions.buildables.Count > 0;
                if (flag)
                {
                    foreach (ThingDef item in raceDef.restrictions.buildables)
                    {
                        bool flag2 = !Enumerable.ToList<ThingDef>(DefDatabase<ThingDef>.AllDefs).Contains(item);
                        if (flag2)
                        {
                            return;
                        }
                        Restrictions.AddRestriction<BuildableDef, string>(ref Restrictions.buildingRestrictions, item, raceDef.defName);
                    }
                }
                bool flag3 = raceDef.restrictions.consumables.Count > 0;
                if (flag3)
                {
                    foreach (ThingDef item2 in raceDef.restrictions.consumables)
                    {
                        Restrictions.AddRestriction<ThingDef, ThingDef>(ref Restrictions.consumableRestrictions, item2, raceDef);
                    }
                }
                bool flag4 = raceDef.restrictions.equippables.Count > 0;
                if (flag4)
                {
                    foreach (ThingDef item3 in raceDef.restrictions.equippables)
                    {
                        Restrictions.AddRestriction<ThingDef, ThingDef>(ref Restrictions.equipmentRestrictions, item3, raceDef);
                    }
                }
                bool flag5 = raceDef.restrictions.researchProjectDefs.Count > 0;
                if (flag5)
                {
                    foreach (ResearchProjectDef item4 in raceDef.restrictions.researchProjectDefs)
                    {
                        Restrictions.AddRestriction<ResearchProjectDef, ThingDef>(ref Restrictions.researchRestrictions, item4, raceDef);
                    }
                }
                bool flag6 = raceDef.restrictions.traits.Count > 0;
                if (flag6)
                {
                    foreach (TraitDef item5 in raceDef.restrictions.traits)
                    {
                        Restrictions.AddRestriction<TraitDef, ThingDef>(ref Restrictions.traitRestrictions, item5, raceDef);
                    }
                }
                bool flag7 = raceDef.restrictions.thoughtDefs.Count > 0;
                if (flag7)
                {
                    foreach (ThoughtDef item6 in raceDef.restrictions.thoughtDefs)
                    {
                        Restrictions.AddRestriction<ThoughtDef, ThingDef>(ref Restrictions.thoughtRestrictions, item6, raceDef);
                    }
                }
                bool flag8 = raceDef.restrictions.equippablesWhitelist.Count > 0;
                if (flag8)
                {
                    foreach (ThingDef item7 in raceDef.restrictions.equippablesWhitelist)
                    {
                        Restrictions.AddRestriction<ThingDef, ThingDef>(ref Restrictions.equipabblbleWhiteLists, item7, raceDef);
                    }
                }
                bool flag9 = raceDef.restrictions.bedDefs.Count > 0;
                if (flag9)
                {
                    foreach (ThingDef item8 in raceDef.restrictions.bedDefs)
                    {
                        Restrictions.AddRestriction<ThingDef, ThingDef>(ref Restrictions.bedRestrictions, item8, raceDef);
                    }
                }
                bool flag10 = raceDef.restrictions.bodyTypes.Count > 0;
                if (flag10)
                {
                    foreach (BodyTypeDef item9 in raceDef.restrictions.bodyTypes)
                    {
                        Restrictions.AddRestriction<BodyTypeDef, ThingDef>(ref Restrictions.bodyTypeRestrictions, item9, raceDef);
                    }
                }
                if (raceDef.restrictions.modContentRestrictionsApparelWhiteList.Count > 0)
                {
                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.Name) || raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.PackageId)))
                    {

                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel)))
                        {

                            AddRestriction(ref equipabblbleWhiteLists, def, raceDef);
                        }
                    }
                }


                foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modContentRestrictionsApparelList.Contains(x.Name) || raceDef.restrictions.modContentRestrictionsApparelList.Contains(x.PackageId.ToLower())))
                {
                    foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel)))
                    {
                        AddRestriction(ref equipmentRestrictions, def, raceDef);
                    }
                }


                if (raceDef.restrictions.modResearchRestrictionsList.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modResearchRestrictionsList.Contains(x.Name) || raceDef.restrictions.modResearchRestrictionsList.Contains(x.PackageId)))
                    {
                        foreach (ResearchProjectDef research in mod.AllDefs.Where(x => x is ResearchProjectDef))
                        {
                            AddRestriction(ref researchRestrictions, research, raceDef);
                        }
                    }
                }


                if (raceDef.restrictions.modTraitRestrictions.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modTraitRestrictions.Contains(x.Name) || raceDef.restrictions.modTraitRestrictions.Contains(x.PackageId)))
                    {
                        foreach (TraitDef trait in mod.AllDefs.Where(x => x is TraitDef))
                        {
                            AddRestriction(ref traitRestrictions, trait, raceDef);
                        }
                    }
                }


                if (raceDef.restrictions.modBuildingRestrictions.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modBuildingRestrictions.Contains(x.Name) || raceDef.restrictions.modBuildingRestrictions.Contains(x.PackageId)))
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef))
                        {
                            AddRestriction(ref buildingRestrictions, def, raceDef.defName);
                        }
                    }
                }
                foreach (BodyTypeDef race in raceDef.bodyTypes)
                {
                    Restrictions.AddRestriction<ThingDef, BodyTypeDef>(ref Restrictions.bodyDefs, raceDef, race);
                }
                bool useHumanRecipes = raceDef.useHumanRecipes;
                if (useHumanRecipes)
                {
                    foreach (RecipeDef recipeDef in Enumerable.Where<RecipeDef>(DefDatabase<RecipeDef>.AllDefsListForReading, (RecipeDef x) => x.recipeUsers != null && x.recipeUsers.Contains(ThingDefOf.Human)))
                    {
                        recipeDef.recipeUsers.Add(raceDef);
                        recipeDef.recipeUsers.RemoveDuplicates();
                    }
                    bool flag16 = raceDef.recipes == null;
                    if (flag16)
                    {
                        raceDef.recipes = new List<RecipeDef>();
                    }
                    List<BodyPartDef> list = new List<BodyPartDef>();
                    foreach (BodyPartRecord bodyPartRecord in raceDef.race.body.AllParts)
                    {
                        list.Add(bodyPartRecord.def);
                    }
                    foreach (RecipeDef recipeDef2 in Enumerable.Where<RecipeDef>(ThingDefOf.Human.recipes, (RecipeDef recipe) => recipe.targetsBodyPart || !recipe.appliedOnFixedBodyParts.NullOrEmpty<BodyPartDef>()))
                    {
                        foreach (BodyPartDef bodyPartDef in Enumerable.Intersect<BodyPartDef>(recipeDef2.appliedOnFixedBodyParts, list))
                        {
                            raceDef.recipes.Add(recipeDef2);
                        }
                    }
                    raceDef.recipes.RemoveDuplicates();
                }
            }
            Log.Message("[RimVali Core/RVR]: Setting up faction restrictions.");
            foreach (FactionResearchRestrictionDef factionResearchRestrictionDef in DefDatabase<FactionResearchRestrictionDef>.AllDefsListForReading)
            {
                foreach (FactionResearchRestriction factionResearchRestriction in factionResearchRestrictionDef.factionResearchRestrictions)
                {
                    FacRes item15 = new FacRes(factionResearchRestriction.researchProj, factionResearchRestriction.isHackable);
                    bool flag17 = !Restrictions.factionResearchRestrictions.ContainsKey(factionResearchRestriction.factionDef);
                    if (flag17)
                    {
                        Restrictions.factionResearchRestrictions.Add(factionResearchRestriction.factionDef, new List<FacRes>());
                    }
                    Restrictions.factionResearchRestrictions[factionResearchRestriction.factionDef].Add(item15);
                }
                foreach (FactionResearchRestriction factionResearchRestriction2 in factionResearchRestrictionDef.factionResearchRestrictionBlackList)
                {
                    FacRes item16 = new FacRes(factionResearchRestriction2.researchProj, factionResearchRestriction2.isHackable);
                    bool flag18 = !Restrictions.factionResearchBlacklist.ContainsKey(factionResearchRestriction2.factionDef);
                    if (flag18)
                    {

                        Restrictions.factionResearchBlacklist.Add(factionResearchRestriction2.factionDef, new List<FacRes>());
                    }

                    Restrictions.factionResearchBlacklist[factionResearchRestriction2.factionDef].Add(item16);
                }
            }
            Log.Message($"Loaded {DefDatabase<RimValiRaceDef>.AllDefs.Count()} races");
        }
        
        // Token: 0x04000116 RID: 278
        public static Dictionary<ThingDef, List<ThingDef>> equipmentRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000117 RID: 279
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000118 RID: 280
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictionsWhiteList = new Dictionary<ThingDef, List<ThingDef>>();

        // Token: 0x04000119 RID: 281
        public static Dictionary<BuildableDef, List<string>> buildingRestrictions = new Dictionary<BuildableDef, List<string>>();

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
    }
    #endregion

    #region Apparel score gain patch
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
    public static class ApparelScorePatch
    {
        [HarmonyPostfix]
        public static void ApparelScoreGain_NewTmp(Pawn pawn, Apparel ap, List<float> wornScoresCache, ref float __result)
        {
            ThingDef def = ap.def;
            if (!ApparelPatch.CanWearHeavyRestricted(def, pawn))
            {
                __result = -100;
                return;
            }
        }
    }
    #endregion

    #region Butcher patch
    [HarmonyPatch(typeof(Corpse), "ButcherProducts")]
    public static class ButcherPatch
    {
        //Gets the thought for butchering.
        static void ButcheredThoughAdder(Pawn pawn, Pawn butchered, bool butcher = true)
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
                #endregion
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
                #endregion 
                //If the pawn is not from RVR.
                if (!(pawn.def is RimValiRaceDef) && pawn.RaceProps.Humanlike)
                {
                    if (butcher)

                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse, null);
                    return;
                }
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
            }
            #endregion
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
                if (butcher.def is RimValiRaceDef def)
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
    #endregion
    #region Backstory patch

    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
    public class storyPatch
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
                Log.Error(string.Concat(new object[]
                {
                    "No shuffled ",
                    slot,
                    " found for ",
                    pawn.ToStringSafe<Pawn>(),
                    " of ",
                    factionType.ToStringSafe<FactionDef>(),
                    ". Choosing random."
                }), false);
                backstory = (from kvp in BackstoryDatabase.allBackstories
                             where kvp.Value.slot == slot
                             select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
                foreach (RVRBackstory story in DefDatabase<RVRBackstory>.AllDefsListForReading)
                {
                    if (story.defName == backstory.identifier)
                    {
                        if (!story.CanSpawn(pawn))
                        {
                            FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, pawn.Faction.def.backstoryFilters, factionType);
                        }
                    }
                }
            }
        }
        [HarmonyPostfix]
        public static void checkStory(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
        {
            foreach (RVRBackstory story in DefDatabase<RVRBackstory>.AllDefsListForReading)
            {
                if (story.defName == backstory.identifier)
                {
                    if (!story.CanSpawn(pawn))
                    {
                        FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, pawn.Faction.def.backstoryFilters, factionType);
                    }

                }
            }
        }
    }
    #endregion
    #region Base Head Offset patch
    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class HeadPatch
    {

        [HarmonyPostfix]
        public static void setPos(ref Vector3 __result, Rot4 rotation, PawnRenderer __instance)
        {
            Pawn pawn = __instance.graphics.pawn;
            PawnGraphicSet set = __instance.graphics;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                rimValiRaceDef.HeadOffsetPawn(__instance, ref __result);

            }
        }
    }
    #endregion
    #region Body gen patch
    //Generation patch for bodytypes
    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
    public static class BodyPatch
    {
        public static IEnumerable<BodyTypeDef> bTypes(Pawn p)
        {
            List<BodyTypeDef> getAllAvalibleBodyTypes = new List<BodyTypeDef>();
            if (Restrictions.bodyDefs.ContainsKey(p.def)) { getAllAvalibleBodyTypes.AddRange(Restrictions.bodyDefs[p.def]); }
            if (getAllAvalibleBodyTypes.NullOrEmpty()) { getAllAvalibleBodyTypes.AddRange(new List<BodyTypeDef> { BodyTypeDefOf.Fat, BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin }); }
            getAllAvalibleBodyTypes.AddRange(getAllAvalibleBodyTypes.NullOrEmpty() ? new List<BodyTypeDef> { BodyTypeDefOf.Fat, BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin }: new List<BodyTypeDef>());
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
                    if (pawn.story.bodyType == null || !bTypes(pawn).Contains(pawn.story.bodyType)) { pawn.story.bodyType = bTypes(pawn).RandomElement(); };
                    Log.Message(pawn.story.bodyType.defName);
                }
            }
        }
    }
    #endregion
    #region Bed patch
    [HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
    public class BedPatch
    {
        [HarmonyPostfix]
        public static void BedPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            __result = __result && Restrictions.checkRestrictions(Restrictions.bedRestrictions, bedDef, p.def);
        }
    }
    #endregion

    #region Research restriction patch

    [HarmonyPatch(typeof(WorkGiver_Researcher), "ShouldSkip")]
    public class ResearchPatch
    {
        [HarmonyPostfix]
        static void Research(Pawn pawn, ref bool __result)
        {
            //Log.Message("test");
            if (Find.ResearchManager.currentProj != null)
            {
                // Log.Message($"Is blacklisted: {(Restrictions.factionResearchBlacklist.ContainsKey(pawn.Faction.def) && Restrictions.factionResearchBlacklist[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj))}");
                if (!Restrictions.checkRestrictions(Restrictions.researchRestrictions, Find.ResearchManager.currentProj, pawn.def) || (Restrictions.factionResearchRestrictions.ContainsKey(pawn.Faction.def) && !Restrictions.factionResearchRestrictions[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj)) || (Restrictions.factionResearchBlacklist.ContainsKey(pawn.Faction.def) && Restrictions.factionResearchBlacklist[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj)))
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
    #endregion

    #region Pawnkind replacement
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
    public static class generatorPatch
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
    #endregion

    #region Apparel gen patch
    [HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
    public class ApparelGenPatch
    {
        [HarmonyPrefix]
        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            try
            {
                Traverse apparelInfo = Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs");
                List<ThingStuffPair> pairs = apparelInfo.GetValue<List<ThingStuffPair>>().ListFullCopy();
                if (!pairs.NullOrEmpty())
                {
                    pairs.RemoveAll(x => !ApparelPatch.CanWearHeavyRestricted(x.thing, pawn));
                    apparelInfo.SetValue(pairs);
                }
            }
            catch (Exception e){Log.Error($"Oops! RV:C had an issue generating apparel: {e.Message}");}
        }
    }
    #endregion
    #region Trait patch
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public class traitPatch
    {
        [HarmonyPrefix]
        public static bool traitGain(Trait trait, TraitSet __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            return pawn.def is RimValiRaceDef rDef ? (!rDef.restrictions.disabledTraits.NullOrEmpty() && rDef.restrictions.disabledTraits.Contains(trait.def)) || Restrictions.checkRestrictions(Restrictions.traitRestrictions, trait.def, pawn.def) : Restrictions.checkRestrictions(Restrictions.traitRestrictions, trait.def, pawn.def);
        }
    }
    #endregion
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
                if (pawn.needs.mood != null)
                {
                    if (pawn != victim)
                    {
                        if (pawn.def is RimValiRaceDef rDef)
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
        }
    }
    #endregion
    #region FactionGen patch
    [HarmonyPatch(typeof(Faction), "TryMakeInitialRelationsWith")]
    public static class FactionGenPatch
    {
        [HarmonyPostfix]
        public static void patch(Faction __instance, Faction other)
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
    #endregion
    #region Health offset patch
    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    public static partial class BodyPartHealthPatch
    {

        [HarmonyPostfix]
        public static void patch(ref float __result, Pawn pawn, BodyPartDef __instance)
        {
            float num = 0f;
            float otherNum = 0f;
            if (pawn.health.hediffSet.hediffs != null)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part != null && hediff.Part.def == __instance))
                {
                    if (hediff.CurStage != null && !hediff.CurStage.statOffsets.NullOrEmpty<StatModifier>())
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
                __result = (float)Mathf.CeilToInt((float)__instance.hitPoints * pawn.HealthScale * num) + otherNum;
            }
            else
            {
                __result = (float)Mathf.CeilToInt((float)__instance.hitPoints * pawn.HealthScale) + otherNum;
            }
            return;
        }
    }
    #endregion
    #region Cannibalism patch
  /*  [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class IngestingPatch
    {

      //  [HarmonyPostfix]
        public static void Patch(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<ThoughtDef> __result)
        {

            bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);
            if (ingester.def is RimValiRaceDef rDef)
            {
                for (int a = 0; a < __result.Count - 1; a++)
                {
                    ThoughtDef t = __result[a];
                    #region raw
                    if (t == ThoughtDefOf.AteHumanlikeMeatDirectCannibal || t == ThoughtDefOf.AteHumanlikeMeatDirect)
                    {
                        ThingDef r = foodDef.ingestible.sourceDef;
                        if (r != null)
                        {
                            if (rDef.getEatenThought(r, true, cannibal) != null)
                                __result[a] = rDef.getEatenThought(r, true, cannibal);
                            else if (!rDef.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                            {
                                __result.RemoveAt(a);
                            }
                        }
                    }
                    #endregion
                    #region cooked
                    if (t == ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal || t == ThoughtDefOf.AteHumanlikeMeatAsIngredient)
                    {
                        ThingDef r = foodSource.TryGetComp<CompIngredients>().ingredients.Where(food => food.ingestible != null && rDef.butcherAndHarvestThoughts.butcherThoughts.Any(x => x.race == food.ingestible.sourceDef)).ToList()[0];
                        if (foodSource.TryGetComp<CompIngredients>() != null && !foodSource.TryGetComp<CompIngredients>().ingredients.NullOrEmpty())
                        {
                            for (int b = 0; b < foodSource.TryGetComp<CompIngredients>().ingredients.Count - 1; b++)
                            {
                                ThingDef ing = foodSource.TryGetComp<CompIngredients>().ingredients[b];
                                if (rDef.getEatenThought(ing.ingestible.sourceDef, false, cannibal) != null)
                                {
                                    __result.Replace(
                                                     cannibal ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient,
                                                     rDef.getEatenThought(ing.ingestible.sourceDef, false, cannibal));
                                }
                            }
                        }

                    }
                    #endregion
                }

            }
        }
    }
  */
    #endregion

    #region Thought patches
    [HarmonyPatch(typeof(ThoughtUtility), "CanGetThought")]
    public static class thoughtPatch
    {
        [HarmonyPostfix]
        public static void CanGetPatch(Pawn pawn, ThoughtDef def, bool checkIfNullified, ref bool __result)
        {
            __result = __result && (pawn.def is RimValiRaceDef rDef ? rDef.canHavethoughts && !(!rDef.restrictions.thoughtBlacklist.NullOrEmpty() && rDef.restrictions.thoughtBlacklist.Contains(def)) : Restrictions.checkRestrictions(Restrictions.thoughtRestrictions, def, pawn.def));
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "GetFirstMemoryOfDef")]
    public static class thoughtReplacerPatchGetFirstMemoriesOfDef
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
    public static class thoughtReplacerPatchNumMemoriesOfDef
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
    public static class thoughtReplacerPatchOldestMemoriesOfDef
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
    public static class thoughtReplacerPatchRemoveRemoriesOfDef
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
    public static class thoughtReplacerPatchRemoveRemoriesOfDefIf
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
        [HarmonyPrefix]
        public static bool Patch(Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                Thought_Memory nT = newThought;
                RVDef.ReplaceThought(ref nT.def);

                newThought = ThoughtMaker.MakeThought(nT.def, newThought.CurStageIndex);


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
    #endregion
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
                __result = new NameTriple(name.First, name.Nick != null ? name.Nick : name.First, name.Last);
            }
            else
            {
                return true;
            }
            return false;
        }
    }
    #endregion

    #region Food Eating
    //I dont think these patches interefere with HAR, nor should HAR patches interefere with these?

    //Was going to patch WillEat, but this seems better? I'd imagine they still *could* eat it by force if i patched WillEat.
    [HarmonyPatch(typeof(RaceProperties), "CanEverEat", new[] { typeof(ThingDef) })]
    public static class FoodPatch
    {
        [HarmonyPostfix]
        public static void EdiblePatch(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            ThingDef pawn = DefDatabase<ThingDef>.AllDefs.First(x => x.race == __instance);
            if (pawn != null && !Restrictions.checkRestrictions(Restrictions.consumableRestrictions, t, pawn) && !Restrictions.checkRestrictions(Restrictions.consumableRestrictionsWhiteList, t, pawn))
            {
                JobFailReason.Is(pawn.label + " " + "CannotEatRVR".Translate(pawn.label.Named("RACE")));
                __result = false;
            }
            //No "Consume grass" for you.
            __result = __result && true;
        }
    }
    #endregion
    #region Apparel Equipping
    //Cant patch CanEquip, apparently. This still works though.
   [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new[] { typeof(Thing),typeof(Pawn)})]
    public static class ApparelPatch
    {

        public static bool CanWearHeavyRestricted(ThingDef def, Pawn pawn) => pawn.def is RimValiRaceDef rDef ? Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, def, pawn.def, !rDef.restrictions.canOnlyUseApprovedApparel)|| Restrictions.checkRestrictions(Restrictions.equipabblbleWhiteLists, def, pawn.def, false): Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, def, pawn.def, true) || Restrictions.checkRestrictions(Restrictions.equipabblbleWhiteLists, def, pawn.def, false); //Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, def, pawn.def, pawn.def is RimValiRaceDef rDef ? !rDef.restrictions.canOnlyUseApprovedApparel : true) || Restrictions.checkRestrictions(Restrictions.equipabblbleWhiteLists, def, pawn.def, pawn.def is RimValiRaceDef rDef2 ? !rDef2.restrictions.canOnlyUseApprovedApparel : false);
       [HarmonyPostfix]
        public static void equipable(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            __result = __result && CanWearHeavyRestricted(thing.def, pawn);
            if (thing.def.IsApparel&&__result==false)
            {
             
                cantReason = "CannotWearRVR".Translate(pawn.def.label.Named("RACE"));
            }
           
        }
    }
    #endregion
    #region Construction
    [HarmonyPatch(typeof(GenConstruct), "CanConstruct")]
    //This was confusing at first, but it works.
    public static class ConstructPatch
    {
        [HarmonyPostfix]
        public static void constructable(Thing t, Pawn p, bool checkSkills,bool forced,ref bool __result)
        {
            //Log.Message(t.def.ToString());
            if (!Restrictions.checkRestrictions<BuildableDef, string>(Restrictions.buildingRestrictions, t.def.entityDefToBuild, p.def.defName))
            {
                __result = false;
                JobFailReason.Is(p.def.label + " " + "CannotBuildRVR".Translate(p.def.label.Named("RACE")));
            }
            __result = true && __result;

        }
    }
    #endregion
    #region ResolveAllGraphics patch
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolvePatch
    {
        [HarmonyPrefix]
        public static bool ResolveGraphics(PawnGraphicSet __instance)
        {
            
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimvaliRaceDef)
            {
                try
                {
                    
                    raceColors graphics = rimvaliRaceDef.graphics;
                    colorComp colorComp = pawn.TryGetComp<colorComp>();

                    if (colorComp.colors == null || colorComp.colors.Count() == 0)
                    {
                        rimvaliRaceDef.GenGraphics(pawn);
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
                        if (!rimvaliRaceDef.hasHair)
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
                        if (!rimvaliRaceDef.hasHair)
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
            return true;
        }
    }
    #endregion
    #region Portraits patch
    //Render renderables the correct way in the portrait. 
    //[HarmonyPatch(typeof(PortraitRenderer), "RenderPortrait")]
    static class RenderPatch
    {
       // [HarmonyPostfix]
        static void Portrait(PawnRenderer __instance)
        {
            Vector3 zero = Vector3.zero;
            float angle;
            if (__instance.graphics.pawn.Dead || __instance.graphics.pawn.Downed)
            {
                angle = 85f;
                zero.x -= 0.18f;
                zero.z -= 0.18f;
            }
            else
            {
                angle = 0f;
            }
            try
            {


                Pawn pawn = __instance.graphics.pawn;
                if (RenderPatchTwo.renders.ContainsKey(pawn))
                {
                    RenderPatchTwo.RSet s = RenderPatchTwo.renders[pawn];

                    if (!pawn.Dead && !pawn.Downed)
                    {
                        RenderPatchTwo.RenderBodyPartsPortrait(angle, Vector3.zero, __instance, Rot4.South, s.mode);

                    }
                    else
                    {
                        RenderPatchTwo.RenderBodyPartsPortrait(angle, new Vector3(-0.2f, 0, -0.2f), __instance, Rot4.South, s.mode);
                    }
                }
            }
            catch (Exception error)
            {
                //Achivement get! How did we get here?
                Log.Error("Something has gone terribly wrong! Error: \n" + error.Message);
            }

        }
    }
    #endregion

    public static class ColorInfo
    {
        public static Dictionary<string, PawnGraphicSet> sets = new Dictionary<string, PawnGraphicSet>();
    }


    #region Rendering patch
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof( Vector3) , typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags)  })]
    static class RenderPatchTwo
    {
        
        public class RSet
        {
            public RotDrawMode mode;
            public PawnRenderer renderer;
        }
        public static Dictionary<Pawn, RSet> renders = new Dictionary<Pawn, RSet>();
        public static Dictionary<Pawn, List<RenderableDef>> pawnRenderables = new Dictionary<Pawn, List<RenderableDef>>();


        #region portrait version

        public static void RenderBodyPartsPortrait(float angle, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation, RotDrawMode mode)
        {
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Pawn pawn = pawnRenderer.graphics.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                List<RenderableDef> renderables = rimValiRaceDef.renderableDefs.Where(x => x.CanShow(pawn, mode, true)).ToList();
                renderables.AddRange((pawnRenderables.ContainsKey(pawn) ? pawnRenderables[pawn] : new List<RenderableDef>()));
                foreach (RenderableDef renderable in renderables)
                {


                    colorComp colorComp = pawn.TryGetComp<colorComp>();
                    Vector3 offset = new Vector3();
                    Vector2 size = new Vector2();
                    #region Direction / size / layering stuff
                    if (renderable.west == null)
                    {
                        renderable.west = new BodyPartGraphicPos();
                        renderable.west.position.x = -renderable.east.position.x;
                        renderable.west.position.y = -renderable.east.position.y;
                        renderable.west.size = renderable.east.size;
                        renderable.west.layer = renderable.east.layer;
                    }
                    if (rotation == Rot4.East)
                    {
                        offset = new Vector3(renderable.east.position.x, renderable.east.layer, renderable.east.position.y);
                        size = renderable.east.size;
                    }
                    else if (rotation == Rot4.North)
                    {
                        offset = new Vector3(renderable.north.position.x, renderable.north.layer, renderable.north.position.y);
                        size = renderable.north.size;
                    }
                    else if (rotation == Rot4.South)
                    {
                        offset = new Vector3(renderable.south.position.x, renderable.south.layer, renderable.south.position.y);
                        size = renderable.south.size;
                    }
                    else if (rotation == Rot4.West)
                    {
                        offset = new Vector3(renderable.west.position.x, renderable.west.layer, renderable.west.position.y);
                        size = renderable.west.size;
                    }
                    #endregion
                    string path = renderable.texPath(pawn);
                    AvaliGraphic graphic = Renders.getTex(renderable, path);
                    if (renderable.useColorSet != null)
                    {
                        raceColors graphics = rimValiRaceDef.graphics;
                        List<Colors> colors = graphics.colorSets;
                        TriColor_ColorGenerators generators = colors.First<Colors>(x => x.name == graphics.skinColorSet).colorGenerator;
                        /*Color color1 = generators.firstColor.NewRandomizedColor();
                        Color color2 = generators.secondColor.NewRandomizedColor();
                        Color color3 = generators.thirdColor.NewRandomizedColor();*/
                        Color color1 = Color.red;
                        Color color2 = Color.green;
                        Color color3 = Color.blue;

                        string colorSetToUse = renderable.useColorSet;
                        if (colorComp.colors.ContainsKey(colorSetToUse))
                        {
                            color1 = colorComp.colors[colorSetToUse].colorOne;
                            color2 = colorComp.colors[colorSetToUse].colorTwo;
                            color3 = colorComp.colors[colorSetToUse].colorThree;
                        }
                        else
                        {
                            Log.ErrorOnce("Pawn graphics does not contain color set: " + renderable.useColorSet + " for " + renderable.defName + ", going to fallback RGB colors. (These should look similar to your mask colors)", 1);
                        }
                        #region Rotting/Dessicated Graphic changes
                        if (pawn.Dead)
                        {



                            if (mode == RotDrawMode.Dessicated)
                            {
                                if (pawnRenderer.graphics.dessicatedGraphic.Color != null)
                                {
                                    //                This will be changed eventually
                                    color1 = color1 * (pawnRenderer.graphics.rottingGraphic.Color);
                                    color2 = color2 * (pawnRenderer.graphics.rottingGraphic.Color);
                                    color3 = color3 * (pawnRenderer.graphics.rottingGraphic.Color);
                                }
                                if (renderable.dessicatedTex != null)
                                {
                                    path = renderable.dessicatedTex;
                                }
                            }
                            else if (mode == RotDrawMode.Rotting)
                            {
                                if (pawnRenderer.graphics.rottingGraphic.color != null)
                                {
                                    color1 = color1 * new Color(0.34f, 0.32f, 0.3f);
                                    color2 = color2 * new Color(0.34f, 0.32f, 0.3f);
                                    color3 = color3 * new Color(0.34f, 0.32f, 0.3f);
                                }
                                if (renderable.rottingTex != null)
                                {
                                    path = renderable.rottingTex;
                                }
                            }
                        }
                        #endregion


                        graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), AvaliShaderDatabase.Tricolor, size, color1, color2, color3);
                       
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f), quaternion, graphic.MatAt(rotation), true);
                    }
                    else
                    {
                       
                        graphic.drawSize = size;
                        graphic.color = pawn.story.SkinColor;
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f), quaternion, graphic.MatAt(rotation), true);

                    }
                }
            }
            else
            {
                List<RenderableDef> renderables =((pawnRenderables.ContainsKey(pawn) ? pawnRenderables[pawn] : new List<RenderableDef>()));
                foreach (RenderableDef renderable in renderables)
                {


                    colorComp colorComp = pawn.TryGetComp<colorComp>();
                    Vector3 offset = new Vector3();
                    Vector2 size = new Vector2();
                    #region Direction / size / layering stuff
                    if (renderable.west == null)
                    {
                        renderable.west = new BodyPartGraphicPos();
                        renderable.west.position.x = -renderable.east.position.x;
                        renderable.west.position.y = -renderable.east.position.y;
                        renderable.west.size = renderable.east.size;
                        renderable.west.layer = renderable.east.layer;
                    }
                    if (rotation == Rot4.East)
                    {
                        offset = new Vector3(renderable.east.position.x, renderable.east.layer, renderable.east.position.y);
                        size = renderable.east.size;
                    }
                    else if (rotation == Rot4.North)
                    {
                        offset = new Vector3(renderable.north.position.x, renderable.north.layer, renderable.north.position.y);
                        size = renderable.north.size;
                    }
                    else if (rotation == Rot4.South)
                    {
                        offset = new Vector3(renderable.south.position.x, renderable.south.layer, renderable.south.position.y);
                        size = renderable.south.size;
                    }
                    else if (rotation == Rot4.West)
                    {
                        offset = new Vector3(renderable.west.position.x, renderable.west.layer, renderable.west.position.y);
                        size = renderable.west.size;
                    }
                    #endregion
                    string path = renderable.texPath(pawn);
                    AvaliGraphic graphic = Renders.getTex(renderable, path);
                    if (renderable.useColorSet != null)
                    {
                 
                        Color color1 = pawn.Graphic.color;
                        Color color2 = pawn.Graphic.colorTwo;
                        Color color3 = pawn.story.SkinColor;

                        string colorSetToUse = renderable.useColorSet;
                        if (colorComp.colors.ContainsKey(colorSetToUse))
                        {
                            color1 = colorComp.colors[colorSetToUse].colorOne;
                            color2 = colorComp.colors[colorSetToUse].colorTwo;
                            color3 = colorComp.colors[colorSetToUse].colorThree;
                        }
                        else
                        {
                            Log.ErrorOnce("Pawn graphics does not contain color set: " + renderable.useColorSet + " for " + renderable.defName + ", going to fallback RGB colors. (These should look similar to your mask colors)", 1);
                        }
                        #region Rotting/Dessicated Graphic changes
                        if (pawn.Dead)
                        {



                            if (mode == RotDrawMode.Dessicated)
                            {
                                if (pawnRenderer.graphics.dessicatedGraphic.Color != null)
                                {
                                    //                This will be changed eventually
                                    color1 = color1 * (pawnRenderer.graphics.rottingGraphic.Color);
                                    color2 = color2 * (pawnRenderer.graphics.rottingGraphic.Color);
                                    color3 = color3 * (pawnRenderer.graphics.rottingGraphic.Color);
                                }
                                if (renderable.dessicatedTex != null)
                                {
                                    path = renderable.dessicatedTex;
                                }
                            }
                            else if (mode == RotDrawMode.Rotting)
                            {
                                if (pawnRenderer.graphics.rottingGraphic.color != null)
                                {
                                    color1 = color1 * new Color(0.34f, 0.32f, 0.3f);
                                    color2 = color2 * new Color(0.34f, 0.32f, 0.3f);
                                    color3 = color3 * new Color(0.34f, 0.32f, 0.3f);
                                }
                                if (renderable.rottingTex != null)
                                {
                                    path = renderable.rottingTex;
                                }
                            }
                        }
                        #endregion


                        graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), AvaliShaderDatabase.Tricolor, size, color1, color2, color3);

                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f), quaternion, graphic.MatAt(rotation), true);
                    }
                    else
                    {

                        graphic.drawSize = size;
                        graphic.color = pawn.story.SkinColor;
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f), quaternion, graphic.MatAt(rotation), true);

                    }
                }
            }
        }

        #endregion


        public static void RenderBodyParts(bool portrait, float angle, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation, RotDrawMode mode)
        {
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Pawn pawn = pawnRenderer.graphics.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                List<RenderableDef> renderables = rimValiRaceDef.renderableDefs.Where(x => x.CanShow(pawn, mode, portrait)).ToList();
                renderables.AddRange((pawnRenderables.ContainsKey(pawn) ? pawnRenderables[pawn] : new List<RenderableDef>()));
                foreach (RenderableDef renderable in renderables)
                {


                    colorComp colorComp = pawn.TryGetComp<colorComp>();
                    Vector3 offset = new Vector3();
                    Vector2 size = new Vector2();
                    #region Direction / size / layering stuff
                    if (renderable.west == null)
                    {
                        renderable.west = new BodyPartGraphicPos();
                        renderable.west.position.x = -renderable.east.position.x;
                        renderable.west.position.y = -renderable.east.position.y;
                        renderable.west.size = renderable.east.size;
                        renderable.west.layer = renderable.east.layer;
                    }
                    if (rotation == Rot4.East)
                    {
                        offset = new Vector3(renderable.east.position.x, renderable.east.layer, renderable.east.position.y);
                        size = renderable.east.size;
                    }
                    else if (rotation == Rot4.North)
                    {
                        offset = new Vector3(renderable.north.position.x, renderable.north.layer, renderable.north.position.y);
                        size = renderable.north.size;
                    }
                    else if (rotation == Rot4.South)
                    {
                        offset = new Vector3(renderable.south.position.x, renderable.south.layer, renderable.south.position.y);
                        size = renderable.south.size;
                    }
                    else if (rotation == Rot4.West)
                    {
                        offset = new Vector3(renderable.west.position.x, renderable.west.layer, renderable.west.position.y);
                        size = renderable.west.size;
                    }
                    #endregion
                    string path = renderable.texPath(pawn);
                    AvaliGraphic graphic = Renders.getTex(renderable, path);
                    if (renderable.useColorSet != null)
                    {
                        raceColors graphics = rimValiRaceDef.graphics;
                        List<Colors> colors = graphics.colorSets;
                        TriColor_ColorGenerators generators = colors.First<Colors>(x => x.name == graphics.skinColorSet).colorGenerator;
                        /*Color color1 = generators.firstColor.NewRandomizedColor();
                        Color color2 = generators.secondColor.NewRandomizedColor();
                        Color color3 = generators.thirdColor.NewRandomizedColor();*/
                        Color color1 = Color.red;
                        Color color2 = Color.green;
                        Color color3 = Color.blue;
                       
                        string colorSetToUse = renderable.useColorSet;
                        if (colorComp.colors.ContainsKey(colorSetToUse))
                        {
                            color1 = colorComp.colors[colorSetToUse].colorOne;
                            color2 = colorComp.colors[colorSetToUse].colorTwo;
                            color3 = colorComp.colors[colorSetToUse].colorThree;
                        }
                        else
                        {
                            Log.ErrorOnce("Pawn graphics does not contain color set: " + renderable.useColorSet + " for " + renderable.defName + ", going to fallback RGB colors. (These should look similar to your mask colors)", 1);
                        }
                        #region Rotting/Dessicated Graphic changes
                        if (pawn.Dead)
                        {



                            if (mode == RotDrawMode.Dessicated)
                            {
                                if (pawnRenderer.graphics.dessicatedGraphic.Color != null)
                                {
                                    //                This will be changed eventually
                                    color1 = color1 * (pawnRenderer.graphics.rottingGraphic.Color);
                                    color2 = color2 * (pawnRenderer.graphics.rottingGraphic.Color);
                                    color3 = color3 * (pawnRenderer.graphics.rottingGraphic.Color);
                                }
                                if (renderable.dessicatedTex != null)
                                {
                                    path = renderable.dessicatedTex;
                                }
                            }
                            else if (mode == RotDrawMode.Rotting)
                            {
                                if (pawnRenderer.graphics.rottingGraphic.color != null)
                                {
                                    color1 = color1 * new Color(0.34f, 0.32f, 0.3f);
                                    color2 = color2 * new Color(0.34f, 0.32f, 0.3f);
                                    color3 = color3 * new Color(0.34f, 0.32f, 0.3f);
                                }
                                if (renderable.rottingTex != null)
                                {
                                    path = renderable.rottingTex;
                                }
                            }
                        }
                        #endregion
                        graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), AvaliShaderDatabase.Tricolor, size, color1, color2, color3);
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f),
                        quaternion, graphic.MatAt(rotation), portrait);
                    }
                    else
                    {

                        graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), AvaliShaderDatabase.Tricolor, size, pawn.story.SkinColor);
                        GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f),
                         quaternion, graphic.MatAt(rotation), portrait);
                    }
                }
            }
            else
            {
                return;
            }
        }

        static Vector3 southHeadOffset(PawnRenderer __instance)
        {
            return __instance.BaseHeadOffsetAt(Rot4.South);
        }
        [HarmonyPostfix]
        static void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, PawnRenderer __instance)
        {
            void Render()
            {
                

                if (!(__instance.graphics.pawn.def is RimValiRaceDef))
                    return;

                Pawn pawn = __instance.graphics.pawn;

                Rot4 rot = __instance.graphics.pawn.Rotation;
                // angle = pawn.Graphic.DrawRotatedExtraAngleOffset;
                //angle = pawn.Position.AngleFlat;
                angle = __instance.BodyAngle();
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                if (pawn.GetPosture() != PawnPosture.Standing)
                {

                    rot = __instance.LayingFacing();
                    Building_Bed building_Bed = __instance.graphics.pawn.CurrentBed();
                    if (building_Bed != null && __instance.graphics.pawn.RaceProps.Humanlike)
                    {
                      
                        AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 17);
                        Vector3 vector2;
                        Vector3 a3 = vector2 = __instance.graphics.pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                        vector2.y += 0.024489796f;
                        Rot4 rotation2 = building_Bed.Rotation;
                        rotation2.AsInt += 2;
                        float d = -__instance.BaseHeadOffsetAt(Rot4.South).z;
                        Vector3 a2 = rotation2.FacingCell.ToVector3();
                        rootLoc = a3 + a2 * d;
                        rootLoc.y += 0.009183673f;
                    }
                    else if (!pawn.Dead && pawn.CarriedBy == null)
                    {
                        rootLoc.y = AltitudeLayer.LayingPawn.AltitudeFor() + 0.009183673f;
                    }

                }
                RenderBodyParts(true, angle, rootLoc, __instance, rot, bodyDrawType);
                if (pawn.Spawned && !pawn.Dead)
                {
                   pawn.stances.StanceTrackerDraw();
                   pawn.pather.PatherDraw();
                }
                Vector3 vector = rootLoc;
                Vector3 a = rootLoc;
                if (bodyFacing != Rot4.North)
                {
                    a.y += 0.024489796f;
                    vector.y += 0.021428572f;
                }
                else
                {
                    a.y += 0.021428572f;
                    vector.y += 0.024489796f;
                }
                List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                if (__instance.graphics.headGraphic != null)
                {
                    Vector3 b = quaternion * __instance.BaseHeadOffsetAt(bodyFacing);
                    Material material = __instance.graphics.HeadMatAt(bodyFacing, bodyDrawType, false, false);
                    if (material != null)
                    {
                        GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(bodyFacing), a + b, quaternion, material, false);
                    }
                    Vector3 loc2 = rootLoc + b;
                    loc2.y += 0.030612245f;
                    bool flag = false;
                    if (!Prefs.HatsOnlyOnMap)
                    {
                        Mesh mesh2 = __instance.graphics.HairMeshSet.MeshAt(bodyFacing);
                        for (int j = 0; j < apparelGraphics.Count; j++)
                        {
                            if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead && pawn.def is RimValiRaceDef rDef)
                            {
                                if (!apparelGraphics[j].sourceApparel.def.apparel.hatRenderedFrontOfFace)
                                {
                                    flag = true;
                                    Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                    material2 = OverrideMaterialIfNeeded_NewTemp(material2, __instance.graphics.pawn, __instance, false);
                                    GenDraw.DrawMeshNowOrLater(mesh2, loc2, quaternion, material2, false);
                                }
                                else
                                {
                                    Material material3 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                    material3 = OverrideMaterialIfNeeded_NewTemp(material3, __instance.graphics.pawn, __instance, false);
                                    Vector3 loc3 = rootLoc + b;
                                    loc3.y += ((bodyFacing == Rot4.North) ? 0.0030612245f : 0.03367347f);
                                    GenDraw.DrawMeshNowOrLater(mesh2, loc3, quaternion, material3, false);
                                }
                            }
                        }
                    }
                    if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                    {
                        Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(bodyFacing);
                        Material mat2 = __instance.graphics.HeadMatAt(bodyFacing, bodyDrawType, false);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, mat2, false);
                    }
                }
                else if (__instance.graphics.headGraphic != null)
                {
                    Vector3 b = quaternion * southHeadOffset(__instance);
                    Material material = __instance.graphics.HeadMatAt(bodyFacing, bodyDrawType, false, false);
                    if (material != null)
                    {
                        GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(bodyFacing), a + b, quaternion, material, false);
                    }
                    Vector3 loc2 = rootLoc + b;
                    loc2.y += 0.030612245f;
                    bool flag = false;
                    if (!Prefs.HatsOnlyOnMap)
                    {
                        Mesh mesh2 = __instance.graphics.HairMeshSet.MeshAt(bodyFacing);
                        for (int j = 0; j < apparelGraphics.Count; j++)
                        {
                            if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                            {
                                if (!apparelGraphics[j].sourceApparel.def.apparel.hatRenderedFrontOfFace)
                                {
                                    flag = true;
                                    Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                    material2 = OverrideMaterialIfNeeded_NewTemp(material2, __instance.graphics.pawn, __instance, false);
                                    GenDraw.DrawMeshNowOrLater(mesh2, loc2, quaternion, material2, false);
                                }
                                else
                                {
                                    Material material3 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                    material3 = OverrideMaterialIfNeeded_NewTemp(material3, __instance.graphics.pawn, __instance, false);
                                    Vector3 loc3 = rootLoc + b;
                                    loc3.y += ((bodyFacing == Rot4.North) ? 0.0030612245f : 0.03367347f);
                                    GenDraw.DrawMeshNowOrLater(mesh2, loc3, quaternion, material3, false);
                                }
                            }
                        }
                    }
                    if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                    {
                        Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(bodyFacing);
                        Material mat2 = __instance.graphics.HairMatAt(bodyFacing, false);
                        GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, mat2, false);
                    }
                }
            }
            Render();
            //Log.Message("test");
        }

        static Material OverrideMaterialIfNeeded_NewTemp(Material original, Pawn pawn, PawnRenderer instance, bool portrait = false)
        {
            Material baseMat = (!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return instance.graphics.flasher.GetDamagedMat(baseMat);
        }

    }
    #endregion
}