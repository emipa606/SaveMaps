using HarmonyLib;
using RimWorld.BaseGen;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(SymbolResolver_SinglePawn), nameof(SymbolResolver_SinglePawn.TryFindSpawnCell))]
public static class Patch_TryFindSpawnCell
{
    public static void Postfix(ResolveParams rp, out IntVec3 cell)
    {
        var map = BaseGen.globalSettings.map;
        _ = CellFinder.TryFindRandomCellInsideWith(rp.rect,
            x => x.Standable(map) &&
                 (rp.singlePawnSpawnCellExtraPredicate == null || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
    }
}