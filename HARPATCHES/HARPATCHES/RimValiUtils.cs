using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Verse;
using System;
using RimWorld.Planet;
using System.Reflection;

namespace RimValiCore
{
    
    public static class RimValiUtility
    {
        public static string build = "Kesuni 1.0.0";
        public static string modulesFound = "Modules:\n";





        //private static readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        public static string dir;
        public static void AssetBundleFinder(DirectoryInfo info)
        {
            foreach (FileInfo file in info.GetFiles())
            {
                if (file.Extension.NullOrEmpty())
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(file.FullName);
                    if (!(bundle == null))
                    {
                        Log.Message("RimVali loaded bundle: " + bundle.name);
                        UnityEngine.Shader[] shaders = bundle.LoadAllAssets<UnityEngine.Shader>();
                    }
                    else
                    {
                        Log.Message("RimVali was unable to load the bundle: " + file.FullName);
                    }
                }
            }
        }

        public static AssetBundle shaderLoader(string info)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(info);
            Log.Message("-----------------------------------------");
            Log.Message("Loaded bundle: " + assetBundle.name);
            Log.Message(assetBundle.GetAllAssetNames()[0], false);
            return assetBundle;
        }


        public static Dictionary<Trait, int> Traits(this Pawn pawn)
        {
            IEnumerable<Trait> traits = pawn.story.traits.allTraits;
            Dictionary<Trait, int> traitDataToReturn = new Dictionary<Trait, int>();
            foreach (Trait trait in traits)
            {
                traitDataToReturn.Add(trait, trait.Degree);
            }
            return traitDataToReturn;
        }

        public static float GetRoomQuality(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            RoomStatWorker_Beauty b = new RoomStatWorker_Beauty();
            return room.GetStat(RoomStatDefOf.Impressiveness);

        }
        public static bool SharedBedroom(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            if (room != null && room.ContainedBeds.Count() > 0)
            {
                IEnumerable<Building_Bed> beds = room.ContainedBeds;
                return beds.Any(bed => bed.OwnersForReading != null && bed.OwnersForReading.Any(p => p != pawn));
            }
            return false;
        }
    
        public static MethodInfo GetMethod<T>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            ParameterModifier modifer;
            return typeof(T).GetMethod(methodName, flags);
        }
        public static MethodInfo GetMethod<T>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, ParameterModifier[] modifiers = null)
        {
            return typeof(T).GetMethod(methodName, flags,null,null,modifiers);
        }
        public static void InvokeMethod<T>(string name, T obj, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            ParameterModifier pMod = new ParameterModifier(parameters.Count());
            for(int a = 0; a<parameters.Count()-1; a++)
            {
                pMod[a] = true;
            }
            ParameterModifier[] mods = { pMod };
            obj.GetType().InvokeMember(name, flags, null, obj, parameters, mods, null, null);
        }
       
      /*  public static void InvokeMethod<T>(string name, T obj, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            GetMethod<T>(name, flags)?.Invoke(obj, parameters);
        }*/
        public static void InvokeMethod<T>(string name, T obj, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            GetMethod<T>(name, flags)?.Invoke(obj, new object[1]);
        }
        public static T GetVar<T>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            T val = default(T);
            if ((T)typeof(T).GetField(fieldName, flags).GetValue(obj) != null)
            {
                val = (T)typeof(T).GetField(fieldName, flags).GetValue(obj);
            }

            return val;
        }

        public static T GetProp<T>(string propName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            return (T)typeof(T).GetProperty(propName, flags).GetValue(obj);
        }
      



     
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<ThingDef> races, Map map) => map.mapPawns.AllPawns.Where(x => races.Contains(x.def));

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map) => map.mapPawns.AllPawns.Where(x => x.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races)=> PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && races.Contains(pawn.def));
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.def == race);
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race, Faction faction) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.Faction == faction && pawn.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn) => CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction).Where(x => x.def == pawn.def);

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(ThingDef race, Map map, Faction faction) => CheckAllPawnsInMapAndFaction(map, faction).Where(x => x.def == race);


        //public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x => x.Map == map);
        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => map.mapPawns.AllPawns.Where(x=>x.Faction==faction);

        public static List<Pawn> FetchPawnsOnAllMaps()
        {
            List<Pawn> val = new List<Pawn>();
            if (Current.ProgramState != ProgramState.Entry)
            {
                List<Map> m = Find.Maps;
                if (m.Count == 1)
                {
                    return m[0].mapPawns.AllPawns;
                }
                foreach(Map map in m)
                {
                    val.AddRange(map.mapPawns.AllPawns);
                }
            }
            return val;
        }
        public static List<Pawn> FetchAllAliveOrDeadPawns()
        {
            List<Pawn> val = new List<Pawn>();
            val.AddRange(Find.WorldPawns.AllPawnsAliveOrDead);
            return val;
        }
        
        public static List<Pawn> FetchPawnsSpawnedOnAllMaps()
        {
            List<Pawn> val = new List<Pawn>();
            if (Current.ProgramState != ProgramState.Entry)
            {
                List<Map> m = Find.Maps;
                if (m.Count == 1)
                {
                    return m[0].mapPawns.AllPawns;
                }
                foreach (Map map in m)
                {
                    val.AddRange(map.mapPawns.AllPawnsSpawned);
                }
            }
            return val;
        }
        public static bool IsOfRace(Pawn pawn, ThingDef race) => pawn.def.defName == race.defName;






        public static void RemovePackRelationIfDead(Pawn pawn, List<Pawn> packmates, PawnRelationDef relationDef)
        {
            foreach (Pawn packmate in packmates)
            {
                if (packmate.DestroyedOrNull())
                {
                    pawn.relations.RemoveDirectRelation(relationDef, packmate);
                }
            }
        }


        public static int PawnOfRaceCount(Faction faction, ThingDef race) => PawnsOfRaceInFaction(race, faction).Count();

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(ThingDef race, Faction faction) => FetchPawnsSpawnedOnAllMaps().Where(x => IsOfRace(x, race) && x.Faction==faction);


        public static bool FactionHasRace(ThingDef race, Faction faction) => PawnOfRaceCount(faction, race) > 0;

        
        
    }
}