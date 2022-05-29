using RimValiCore.RVR;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimValiCore.RVRFrameWork
{
    public static class RaceRestrictor
    {
        public static HashSet<Def> restrictedDefs = new HashSet<Def>();

        private static ModContentPack GetPackByID(string id)
        {
            return LoadedModManager.RunningModsListForReading.Find(x => x.Name == id || x.PackageId.ToLower() == id.ToLower() || x.PackageId == $"{id.ToLower()}_steam");
        }

        public static void AllowRaceToUseItem<T>(T def, RimValiRaceDef race) where T : Def
        {
            race.restrictions.AllowDef(def);
        }
        public static void AllowRaceToUseItems<T>(List<T> defs, RimValiRaceDef race) where T : Def
        {
            race.restrictions.AllowDefs(defs);
        }

        public static void AllowRaceToUseModItems<T>(Type defType,string modName, RimValiRaceDef race, Func<T,bool> validator=null) where T : Def
        {
            ModContentPack mod = GetPackByID(modName);
            if (mod != null)
            {
                AllowRaceToUseItems(DefDatabase<T>.AllDefsListForReading.Where(x => x.modContentPack == mod && (validator == null || validator.Invoke(x))).ToList(), race);
            }
        }

        public static void AllowRaceToUseMultipleModItems<T>(Type defType, List<string> modNames, RimValiRaceDef race, Func<T, bool> validator = null) where T : Def
        {
            foreach(string modName in modNames)
                AllowRaceToUseModItems(defType, modName, race, validator); 
        }
        /// <summary>
        /// Restrict an item to a race.
        /// </summary>
        /// <param name="def"></param>
        /// <param name="race"></param>
        public static void AddRestriction<T>(T def, RimValiRaceDef race) where T : Def
        {
            restrictedDefs.Add(def);
            AllowRaceToUseItem(def,race);

        }

        /// <summary>
        /// Restrict a list of items to a race
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defs"></param>
        /// <param name="race"></param>
        public static void AddRestrictions<T>(IEnumerable<T> defs, RimValiRaceDef race) where T : Def
        {
            if(defs.Count() > 0)
            {
                restrictedDefs.AddRange(defs);
                race.restrictions.AllowDefs(defs);
            }
        }

        /// <summary>
        /// Restrict all defs of T from a mod to a race.
        /// </summary>
        /// <typeparam name="T">The def type</typeparam>
        /// <param name="modName">The mod id</param>
        /// <param name="race">The race</param>
        /// <param name="validator">A validator to check items</param>
        public static void AddModRestrictions<T>(string modName, RimValiRaceDef race, Func<T,bool> validator = null) where T : Def
        {
            ModContentPack mod = GetPackByID(modName);
            if (mod != null)
            {
                AddRestrictions(DefDatabase<T>.AllDefsListForReading.Where(x=>x.modContentPack==mod && (validator==null || validator.Invoke(x))),race);
            }
        }

        /// <summary>
        /// Restrict all defs of T from a list of mods to a race.
        /// </summary>
        /// <typeparam name="T">The def type</typeparam>
        /// <param name="modName">A list of mod ids</param>
        /// <param name="race">The race</param>
        /// <param name="validator">A validator to check items</param>
        public static void AddMultipleModRestrictions<T>(List<string> modName, RimValiRaceDef race, Func<T,bool> validator = null) where T : Def
        {
            foreach(string mod in modName)
                AddModRestrictions(mod, race, validator);
        }

        public static bool IsAllowed(Def def, ThingDef race, bool notInLists = true)
        {
            RaceRestrictions restrictions = (race as RimValiRaceDef)?.restrictions;
            bool allowed = restrictions?.allowedDefsToUse.Contains(def) ?? false;
            return restrictedDefs.Contains(def) || allowed|| notInLists;
        }

        public static List<T> GetAllRestrictedDefs<T>(Type defType, ThingDef race) where T: Def
        {
            List<T> result = new List<T>();
            if (race is RimValiRaceDef def)
            {
                 result = (List<T>)def.restrictions.allowedDefsToUse.Where(x => x.GetType() == defType);
            }
            return result;
        }

        public static void RunRestrictions()
        {
            foreach (RimValiRaceDef def in DefDatabase<RimValiRaceDef>.AllDefsListForReading)
            {
                AddRestrictions(def.restrictions.buildables, def);
                AddRestrictions(def.restrictions.consumables, def);
                AddRestrictions(def.restrictions.researchProjectDefs, def);
                AddRestrictions(def.restrictions.traits, def);
                AddRestrictions(def.restrictions.thoughtDefs, def);
                AddRestrictions(def.restrictions.bedDefs, def);
                AddRestrictions(def.restrictions.bodyTypes, def);

                AddMultipleModRestrictions(def.restrictions.modContentRestrictionsApparelList, def, new Func<ThingDef, bool>(x => x.IsApparel));
                AddMultipleModRestrictions<ResearchProjectDef>(def.restrictions.modResearchRestrictionsList, def);
                AddMultipleModRestrictions<TraitDef>(def.restrictions.modTraitRestrictions, def);
                AddMultipleModRestrictions(def.restrictions.modBuildingRestrictions, def, new Func<ThingDef, bool>(x => !x.IsApparel));
                AddMultipleModRestrictions(def.restrictions.modConsumables, def, new Func<ThingDef, bool>(x => x.IsIngestible));

                AllowRaceToUseItems(def.restrictions.buildablesWhitelist,def);
                AllowRaceToUseItems(def.restrictions.consumablesWhitelist, def);
                AllowRaceToUseItems(def.restrictions.equippablesWhitelist, def);
                AllowRaceToUseMultipleModItems<ThingDef>(typeof(ThingDef),def.restrictions.modContentRestrictionsApparelWhiteList, def);
                AllowRaceToUseMultipleModItems<ThingDef>(typeof(ThingDef),def.restrictions.modResearchRestrictionsWhiteList, def);


                bool useHumanRecipes = def.useHumanRecipes;
                if (useHumanRecipes)
                {
                    foreach (RecipeDef recipeDef in Enumerable.Where(DefDatabase<RecipeDef>.AllDefsListForReading, (RecipeDef x) => x.recipeUsers != null && x.recipeUsers.Contains(ThingDefOf.Human)))
                    {
                        recipeDef.recipeUsers.Add(def);
                        recipeDef.recipeUsers.RemoveDuplicates();
                    }
                    if (def.recipes == null)
                    {
                        def.recipes = new List<RecipeDef>();
                    }
                    List<BodyPartDef> list = new List<BodyPartDef>();
                    foreach (BodyPartRecord bodyPartRecord in def.race.body.AllParts)
                    {
                        list.Add(bodyPartRecord.def);
                    }
                    foreach (RecipeDef recipeDef2 in Enumerable.Where(ThingDefOf.Human.recipes, (RecipeDef recipe) => recipe.targetsBodyPart || !recipe.appliedOnFixedBodyParts.NullOrEmpty()))
                    {
                        foreach (BodyPartDef bodyPartDef in Enumerable.Intersect(recipeDef2.appliedOnFixedBodyParts, list))
                        {
                            def.recipes.Add(recipeDef2);
                        }
                    }
                    def.recipes.RemoveDuplicates();
                }
            }
            Log.Message("[RimVali Core]: Finished restriction setup.");
        }
    }
}
