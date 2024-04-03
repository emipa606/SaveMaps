using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(MapGenerator))]
[HarmonyPatch("GenerateMap")]
public static class GenerateMapPatch
{
    public static void Prefix(ref IntVec3 mapSize, MapParent parent)
    {
        var worldComp = Find.World.GetComponent<WorldComponentGeneration>();
        if (!worldComp.tileSizes.TryGetValue(parent.Tile, out var siz))
        {
            return;
        }

        mapSize = siz;
        worldComp.tileSizes.Remove(parent.Tile);
        Log.Message($"Changing map size to {mapSize}");
    }

    public static void Postfix(MapParent parent)
    {
        Log.Message($"GetOrGenerateMapPatch.caravanArrival: {GetOrGenerateMapPatch.caravanArrival}");
        if (GetOrGenerateMapPatch.caravanArrival)
        {
            return;
        }

        var preset = SettlementGeneration.GetPresetFor(parent, out var locationDef);
        if (preset != null && locationDef != null)
        {
            SettlementGeneration.DoSettlementGeneration(parent.Map, preset.FullName, locationDef, parent.Faction,
                false);
        }
    }
}