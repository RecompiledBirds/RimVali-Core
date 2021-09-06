using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimValiCore
{

    public static class RimValiUtility
    {
        #region Pawn
        public static bool Alive(this Pawn pawn)
        {
            return !pawn.Dead;
        }

        public static bool IsOfRace(this Pawn pawn, ThingDef race) => pawn!=null ? pawn.def.defName == race.defName : false;
        #endregion

        //private static readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;

        #region asset and shader loading
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
       
          
            return assetBundle;
        }
        #endregion
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
        #region room stuff
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
        #endregion

        #region Reflection stuff
        public static void SetFieldType<T>(string fieldName, T obj, Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            obj.GetType().GetField(fieldName, flags).ChangeType<T>();
        }


        public static MethodInfo GetMethod<T>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return typeof(T).GetMethod(methodName, flags);
        }
        public static MethodInfo GetMethod<T>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, ParameterModifier[] modifiers = null)
        {
            return typeof(T).GetMethod(methodName, flags, null, new Type[0], modifiers);
        }
        public static void InvokeMethod<T>(string name, T obj, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            ParameterModifier pMod = new ParameterModifier(parameters.Count());
            for (int a = 0; a < parameters.Count() - 1; a++)
            {
                pMod[a] = true;
            }
            ParameterModifier[] mods = { pMod };
            obj.GetType().InvokeMember(name, flags, null, obj, parameters, mods, null, null);
        }
        public static void InvokeMethod<T, V>(Assembly assembly,string name, string typeName, T obj, out V result, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            ParameterModifier pMod = new ParameterModifier(parameters.Count());
            for (int a = 0; a < parameters.Count() - 1; a++)
            {
                pMod[a] = true;
            }
            ParameterModifier[] mods = { pMod };
            Type type = assembly.GetType(typeName);
            result = (V)type.InvokeMember(name, flags, null,obj, parameters, mods,null,null);
        }

        public static void InvokeMethodTMP<T>(string name, T obj, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
          {
              GetMethod<T>(name, flags)?.Invoke(obj, parameters);
          }
        public static void InvokeMethod<T>(string name, T obj, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            GetMethod<T>(name, flags)?.Invoke(obj, new object[1]);
        }
        public static T GetVar<T>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            return (T)obj.GetType().GetField(fieldName, flags).GetValue(obj);
        }

        public static void SetVar<T>(string fieldName, T val,BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,object obj = null)
        {
            obj.GetType().GetField(fieldName, flags).SetValue(obj,val);
        }

        public static T GetProp<T>(string propName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            return (T)typeof(T).GetProperty(propName, flags).GetValue(obj);
        }



        #endregion

        #region finding pawns of races and factions

        public static IEnumerable<Pawn> AllPawnsOfFactionSpawned(Faction faction) => PawnsFinder.All_AliveOrDead.Where(pawn => pawn.Faction == faction && pawn.Spawned);
        public static IEnumerable<Pawn> AllPawnsOfFactionSpawned(FactionDef faction) => PawnsFinder.All_AliveOrDead.Where(pawn => pawn.Faction.def == faction && pawn.Spawned);
        public static int PawnOfRaceCount(this Faction faction, ThingDef race) => PawnsOfRaceInFaction(faction, race).Count();
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<ThingDef> races, Map map) => map.mapPawns.AllPawns.Where(x => races.Contains(x.def));

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map) => map.mapPawns.AllPawns.Where(x => x.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && races.Contains(pawn.def));
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.def == race);
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race, Faction faction) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.Faction == faction && pawn.def == race);

        public static bool FactionHasRace(this Faction faction, ThingDef race) => PawnOfRaceCount(faction, race) > 0;

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn) => CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction).Where(x => x.def == pawn.def);

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(ThingDef race, Map map, Faction faction) => CheckAllPawnsInMapAndFaction(map, faction).Where(x => x.def == race);

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(this Faction faction, ThingDef race) => FetchPawnsSpawnedOnAllMaps().Where(x => IsOfRace(x, race) && x.Faction == faction);

        //public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x => x.Map == map);
        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => map.mapPawns.AllPawns.Where(x => x.Faction == faction);

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
                foreach (Map map in m)
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
        #endregion

    }
}
