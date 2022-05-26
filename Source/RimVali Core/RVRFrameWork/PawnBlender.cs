using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimValiCore.RVRFrameWork
{
    [HarmonyPatch(typeof(PawnGenerator),nameof(PawnGenerator.GeneratePawn),new Type[] { typeof(PawnGenerationRequest) })]
    public static class PawnBlender
    {
       [HarmonyPostfix]
       public static void Postfix(PawnGenerationRequest request,ref Pawn __result)
        {

            if (__result.RaceProps.Humanlike && request.Context != PawnGenerationContext.PlayerStarter)
            {
                PawnKindDef selectedDef = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race != null && x.race.race.Humanlike).RandomElement();
                request = new PawnGenerationRequest(selectedDef);
              
            }
                /*   
                 *
                ThingDef selectedDef = DefDatabase<ThingDef>.AllDefs.Where(x=>x.race!=null && x.race.Humanlike).RandomElement();

                    __result.def =selectedDef;
                    Faction faction;
                    Faction faction2;
                    if (request.Faction != null)
                    {
                        faction = request.Faction;
                    }
                    else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction2, false, true, TechLevel.Undefined, false))
                    {
                        faction = faction2;
                    }
                    else
                    {
                        faction = Faction.OfAncients;
                    }
                    try
                    {

                        PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(__result, request.FixedLastName, faction.def, request.ForceNoBackstory);
                        typeof(PawnGenerator).GetMethod("GenerateTraits", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { __result, request });
                        typeof(PawnGenerator).GetMethod("GenerateBodyType", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { __result, request });
                        typeof(PawnGenerator).GetMethod("GenerateSkills", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { __result });
                        PawnApparelGenerator.GenerateStartingApparelFor(__result, request);
                        PawnWeaponGenerator.TryGenerateWeaponFor(__result, request);
                        PawnInventoryGenerator.GenerateInventoryFor(__result, request);
                    }catch (Exception ex)
                    {
                        Log.Warning($"RimVali Pawn blender: {ex.Message}");
                    }

                }
                */

        }
    }
}
