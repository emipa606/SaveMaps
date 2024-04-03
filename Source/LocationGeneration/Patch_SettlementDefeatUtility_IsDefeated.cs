using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated))]
public static class Patch_SettlementDefeatUtility_IsDefeated
{
    private static bool IsDefeated(Map map, Faction faction)
    {
        var list = map.mapPawns.SpawnedPawnsInFaction(faction);
        foreach (var pawn in list)
        {
            if (pawn.RaceProps.Humanlike)
            {
                return false;
            }
        }

        return true;
    }

    private static bool Prefix(Settlement factionBase)
    {
        return !factionBase.HasMap || IsDefeated(factionBase.Map, factionBase.Faction);
    }
}