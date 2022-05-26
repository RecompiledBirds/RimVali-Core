using RimWorld;
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

        public static bool IsOfRace(this Pawn pawn, ThingDef race)
        {
            return pawn != null ? pawn.def.defName == race.defName : false;
        }

        public static bool IsOfRace(this Pawn pawn, List<ThingDef> races)
        {
            return pawn != null ? races.Contains(pawn.def) : false;
        }
        public static bool IsOfRace(this Pawn pawn, List<RimValiCore.RVR.RimValiRaceDef> races)
        {
            return pawn != null ? races.Contains(pawn.def) : false;
        }
        #endregion Pawn


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
                        Shader[] shaders = bundle.LoadAllAssets<Shader>();
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

        #endregion asset and shader loading

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

        public static bool SharesBedroomWithPawn(this Pawn pawn, Pawn other)
        {
            Room room = pawn.GetRoom();
            if (room != null && room.ContainedBeds.Count() > 0)
            {
                IEnumerable<Building_Bed> beds = room.ContainedBeds;
                return beds.Any(bed => bed.OwnersForReading != null && bed.OwnersForReading.Any(p => p==other));
            }
            return false;
        }

        public static bool SharesBedroomWithPawns(this Pawn pawn, List<Pawn> pawns)
        {
            return pawns.All(p=>pawn.SharesBedroomWithPawn(p));
        }

        #endregion room stuff

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

        public static void InvokeMethod<T, V>(Assembly assembly, string name, string typeName, T obj, out V result, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            ParameterModifier pMod = new ParameterModifier(parameters.Count());
            for (int a = 0; a < parameters.Count() - 1; a++)
            {
                pMod[a] = true;
            }
            ParameterModifier[] mods = { pMod };
            Type type = assembly.GetType(typeName);
            result = (V)type.InvokeMember(name, flags, null, obj, parameters, mods, null, null);
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

        public static void SetVar<T>(string fieldName, T val, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            obj.GetType().GetField(fieldName, flags).SetValue(obj, val);
        }

        public static T GetProp<T>(string propName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            return (T)typeof(T).GetProperty(propName, flags).GetValue(obj);
        }

        #endregion Reflection stuff

        #region finding pawns of races and factions

        public static IEnumerable<Pawn> AllPawnsOfFactionSpawned(Faction faction)
        {
            return PawnsFinder.All_AliveOrDead.Where(pawn => pawn.Faction == faction && pawn.Spawned);
        }

        public static IEnumerable<Pawn> AllPawnsOfFactionSpawned(FactionDef faction)
        {
            return PawnsFinder.All_AliveOrDead.Where(pawn => pawn.Faction.def == faction && pawn.Spawned);
        }

        public static int PawnOfRaceCount(this Faction faction, ThingDef race)
        {
            return PawnsOfRaceInFaction(faction, race).Count();
        }

        public static int PawnOfRaceCount(this Faction faction, List<ThingDef> races)
        {
            return PawnsOfRaceInFaction(faction, races).Count();
        }
        public static int PawnOfRaceCount(this Faction faction, List<RimValiCore.RVR.RimValiRaceDef> races)
        {
            return PawnsOfRaceInFaction(faction, races).Count();
        }
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<RimValiCore.RVR.RimValiRaceDef> races, Map map)
        {
            return map.mapPawns.AllPawns.Where(x => races.Contains(x.def));
        }
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<ThingDef> races, Map map)
        {
            return map.mapPawns.AllPawns.Where(x => races.Contains(x.def));
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map)
        {
            return map.mapPawns.AllPawns.Where(x => x.def == race);
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races)
        {
            return PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && races.Contains(pawn.def));
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race)
        {
            return PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.def == race);
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race, Faction faction)
        {
            return PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.Faction == faction && pawn.def == race);
        }

        public static bool FactionHasRace(this Faction faction, ThingDef race)
        {
            return PawnOfRaceCount(faction, race) > 0;
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn)
        {
            return CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction).Where(x => x.def == pawn.def);
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(ThingDef race, Map map, Faction faction)
        {
            return CheckAllPawnsInMapAndFaction(map, faction).Where(x => x.def == race);
        }

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(this Faction faction, ThingDef race)
        {
            return FetchPawnsSpawnedOnAllMaps().Where(x => IsOfRace(x, race) && x.Faction == faction);
        }

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(this Faction faction, List<ThingDef> races)
        {
            return FetchPawnsSpawnedOnAllMaps().Where(x => IsOfRace(x, races) && x.Faction == faction);
        }
        public static IEnumerable<Pawn> PawnsOfRaceInFaction(this Faction faction, List<RimValiCore.RVR.RimValiRaceDef> races)
        {
            return FetchPawnsSpawnedOnAllMaps().Where(x => IsOfRace(x, races) && x.Faction == faction);
        }
        //public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x => x.Map == map);
        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction)
        {
            return map.mapPawns.AllPawns.Where(x => x.Faction == faction);
        }

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

        #endregion finding pawns of races and factions

        #region rect extensions
        /// <summary>
        ///     Creates a copy of this <see cref="Rect" /> moved by a <see cref="Vector2" />
        /// </summary>
        /// <param name="rect">the <see cref="Rect" /> to move</param>
        /// <param name="vec">the distance to move <paramref name="rect" /></param>
        /// <returns>A copy of <paramref name="rect" />, moved by the distance specified in <paramref name="vec" /></returns>
        public static Rect MoveRect(this Rect rect, Vector2 vec)
        {
            Rect newRect = new Rect(rect);
            newRect.position += vec;
            return newRect;
        }

        /// <summary>
        ///     Devides a <see cref="Rect"/> <paramref name="rect"/> vertically into <see cref="int"/> <paramref name="times"/> amount of pieces
        /// </summary>
        /// <param name="rect">the initial <see cref="Rect"/> that is to be devided</param>
        /// <param name="times">the amount of times it should be devided</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with <paramref name="times"/> amount of pieces </returns>
        public static IEnumerable<Rect> DivideVertical(this Rect rect, int times)
        {
            for (int i = 0; i < times; i++)
            {
                yield return rect.TopPartPixels(rect.height / times).MoveRect(new Vector2(0f, rect.height / times * i));
            }
        }

        /// <summary>
        ///     Devides a <see cref="Rect"/> <paramref name="rect"/> horizontally into <see cref="int"/> <paramref name="times"/> amount of pieces
        /// </summary>
        /// <param name="rect">the initial <see cref="Rect"/> that is to be devided</param>
        /// <param name="times">the amount of times it should be devided</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with <paramref name="times"/> amount of pieces </returns>
        public static IEnumerable<Rect> DivideHorizontal(this Rect rect, int times)
        {
            for (int i = 0; i < times; i++)
            {
                yield return rect.LeftPartPixels(rect.width / times).MoveRect(new Vector2(rect.width / times * i, 0f));
            }
        }

        /// <summary>
        ///     Contracts a <see cref="Rect"/> vertically
        /// </summary>
        /// <param name="rect">the <paramref name="rect"/> to be contracted</param>
        /// <param name="amount">the <see cref="int"/> <paramref name="amount"/> by which the rect is to be contracted by</param>
        /// <returns>A new <see cref="Rect"/> that is contracted by the <see cref="int"/> <paramref name="amount"/></returns>
        public static Rect ContractVertically(this Rect rect, int amount)
        {
            Rect newRect = new Rect(rect);

            newRect.y += amount;
            newRect.height -= amount * 2;

            return newRect;
        }

        /// <summary>
        ///     Contracts a <see cref="Rect"/> horizontally
        /// </summary>
        /// <param name="rect">the <paramref name="rect"/> to be contracted</param>
        /// <param name="amount">the <see cref="int"/> <paramref name="amount"/> by which the rect is to be contracted by</param>
        /// <returns>A new <see cref="Rect"/> that is contracted by the <see cref="int"/> <paramref name="amount"/></returns>
        public static Rect ContractHorizontally(this Rect rect, int amount)
        {
            Rect newRect = new Rect(rect);

            newRect.x += amount;
            newRect.width -= amount * 2;

            return newRect;
        }
        #endregion

        #region window helper
        /// <summary>
        ///     Resets the Text.Font, Text.Anchor and GUI.color setting
        /// </summary>
        public static void ResetTextAndColor()
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
        #endregion
    }
}
