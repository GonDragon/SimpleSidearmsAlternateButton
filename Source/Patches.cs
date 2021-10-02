using PeteTimesSix.SimpleSidearms;
using PeteTimesSix.SimpleSidearms.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using Verse.Sound;
using SimpleSidearms.rimworld;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;

namespace GD.SSAB
{
    // Intercept Sidearms Gizmos, and change them with mine.
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    class Pawn_GetGizmos_Postfix
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__instance.IsValidSidearmsCarrier() && (__instance.IsColonistPlayerControlled
                || DebugSettings.godMode) && __instance.equipment != null && __instance.inventory != null
                )
            {
                IEnumerable<ThingWithComps> carriedWeapons = __instance.getCarriedWeapons(includeTools: true);

                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(__instance);
                if (pawnMemory != null)
                {
                    //yield return new SidearmSmallButtons_Gizmo(__instance,carriedWeapons);
                    yield return new SidearmStance_Gizmo(__instance, carriedWeapons);
                }

                

            }

            foreach (var aGizmo in __result)
            {
                if (aGizmo.GetType() == typeof(Gizmo_SidearmsList)) continue;
                yield return aGizmo;
            }
        }
    }
}
