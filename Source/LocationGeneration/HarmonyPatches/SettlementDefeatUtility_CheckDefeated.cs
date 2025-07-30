using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated))]
public static class SettlementDefeatUtility_CheckDefeated
{
    private static bool isDefeated(Map map, Faction faction)
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
        return !factionBase.HasMap || isDefeated(factionBase.Map, factionBase.Faction);
    }
}