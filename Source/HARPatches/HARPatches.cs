using AlienRace;
using HarmonyLib;
using RimValiCore.RVR;
using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;
namespace RimValiCore.HARTweaks
{

    //There is probably a much better way to do this, but these patches are combinations of whatever we do and the stuff HAR does.
    //   [StaticConstructorOnStartup]
    public static class test
    {

        static test()

        {
            Harmony harmony = new Harmony("Rimvali.HarPatches");
            Log.Message("[RimVali Core/Compatiblity] Starting HAR patches.");
            try
            {
                //The only one actually related to rimvali.
                //  harmony.Patch(AccessTools.Method(typeof(RimValiDefChecks),"setup"), null, new HarmonyMethod(typeof(ReturnDataPatch), "ReturnDataRaces"));

                //The rest is all RVR stuff.
                harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "BaseHeadOffsetAt"), null, new HarmonyMethod(typeof(HeadOffsetPatch), "setPos"));
                harmony.Patch(AccessTools.Method(typeof(Corpse), "ButcherProducts"), new HarmonyMethod(typeof(ButcherPatch), "Patch"));
                harmony.Patch(AccessTools.Method(typeof(BodyPatch), "Patch"), new HarmonyMethod(typeof(BodyGenPatch), "Patch"));
                Log.Message($"[RimVali Core/Compatiblity] Patches completed: {harmony.GetPatchedMethods().Count()}");
                RestrictionsPatch.AddRestrictions();
            }
            catch (Exception error)
            {
                Log.Error("[RimVali Core/Compatiblity] Patches failed!");
                Log.Error(error.Message);
                try
                {
                    RestrictionsPatch.AddRestrictions();
                }
                catch (Exception error2)
                {
                    Log.Error("[RimVali Core/Compatiblity] Backup restrictions attempt failed. \n Error: " + error2.Message);
                }
            }
        }
    }


    public class BodyGenPatch
    {
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
                        BodyPatch.SetBody(story, ref pawn);
                        return;
                    }
                    else if (DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).Count() > 0)
                    {

                        RVRBackstory story = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).FirstOrDefault();
                        BodyPatch.SetBody(story, ref pawn);
                        return;
                    }
                    else
                    {
                        //Log.Message(rimValiRace.bodyTypes.RandomElement().defName);
                        Log.Message(rimValiRace.bodyTypes.Count.ToString());
                        pawn.story.bodyType = rimValiRace.bodyTypes.RandomElement();

                    }
                    return;
                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                    Log.Message("Trying again...");
                    Patch(ref pawn);
                }
            }
            else if (pawn.def is ThingDef_AlienRace alienRace)
            {
                AlienRace.HarmonyPatches.GenerateBodyTypePostfix(ref pawn);
                return;
            }
            else
            {
                if (pawn.story.bodyType == null || !BodyPatch.bTypes(pawn).Contains(pawn.story.bodyType)) { pawn.story.bodyType = BodyPatch.bTypes(pawn).RandomElement(); };
            }
        }
    }
    public class HeadOffsetPatch
    {
        [HarmonyAfter(new string[] { "RimVali.patches.headPatch" })]
        public static void setPos(ref Vector3 __result, Rot4 rotation, PawnRenderer __instance)
        {
            Pawn pawn = __instance.graphics.pawn;
            PawnGraphicSet set = __instance.graphics;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (!(rimValiRaceDef.renderableDefs.Where<RenderableDef>(x => x.defName.ToLower() == "head").Count() > 0))
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
                    if (pawn.Rotation == Rot4.South)
                    {
                        pos.x = headDef.south.position.x + offset.x;
                        pos.y = headDef.south.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.y = headDef.north.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.y = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.y = headDef.west.position.y + offset.y;
                    }
                    // Log.Message(pos.ToString());
                    __result = __result + pos;
                }

            }
            else
            {
                Vector2 offset = (pawn.def as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).headOffsetDirectional?.GetOffset(rotation) ?? Vector2.zero;
                __result += new Vector3(offset.x, y: 0, offset.y);
            }
        }
    }
    public class RestrictionsPatch
    {
        public static void AddRestrictions()
        {
            Log.Message("[RimValiCore/RVR]: HAR is loaded, merging race restrictions.");
            //I ended up using the same method of storing restrictions that HAR does to make this easier.
            foreach (AlienRace.ThingDef_AlienRace raceDef in DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where<AlienRace.ThingDef_AlienRace>(x => x is AlienRace.ThingDef_AlienRace))
            {
                foreach (ThingDef thing in raceDef.alienRace.raceRestriction.buildingList)
                {
                    Restrictions.AddRestriction(ref Restrictions.buildingRestrictions, thing, raceDef);

                }
                if (raceDef.alienRace.raceRestriction.foodList.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.alienRace.raceRestriction.foodList)
                    {
                        Restrictions.AddRestriction(ref Restrictions.consumableRestrictions, thing, raceDef);
                    }
                }
                if (raceDef.alienRace.raceRestriction.apparelList.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.alienRace.raceRestriction.apparelList)
                    {
                        Restrictions.AddRestriction(ref Restrictions.equipmentRestrictions, thing, raceDef);
                    }
                }
                if (raceDef.alienRace.raceRestriction.traitList.Count > 0)
                {
                    foreach (TraitDef trait in raceDef.alienRace.raceRestriction.traitList)
                    {
                        Restrictions.AddRestriction(ref Restrictions.traitRestrictions, trait, raceDef);
                    }
                }
                if (raceDef.alienRace.raceRestriction.researchList.Count > 0)
                {
                    foreach (AlienRace.ResearchProjectRestrictions research in raceDef.alienRace.raceRestriction.researchList)
                    {
                        foreach (ResearchProjectDef researchProject in research.projects)
                        {
                            Restrictions.AddRestriction(ref Restrictions.researchRestrictions, researchProject, raceDef);
                        }
                    }
                }
            }
        }
    }
    /*   public class ReturnDataPatch
       {
           public static List<AlienRace.ThingDef_AlienRace> potentialPackRaces = DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefsListForReading;
           public static void ReturnDataRaces()
           {
               RimValiDefChecks.potentialPackRaces= DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null).ToList();
               RimValiDefChecks.potentialPackRaces.AddRange(potentialPackRaces);

           }
       }*/
}