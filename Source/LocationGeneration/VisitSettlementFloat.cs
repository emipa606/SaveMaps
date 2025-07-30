using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace LocationGeneration;

[HarmonyPatch(typeof(SymbolResolver_Settlement), "Resolve")]
public class VisitSettlementFloat
{
    public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);

    private static bool Prefix(ResolveParams rp)
    {
        var map = BaseGen.globalSettings.map;
        if (GetOrGenerateMapPatch.customSettlementGeneration)
        {
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            SettlementGeneration.DoSettlementGeneration(map, GetOrGenerateMapPatch.locationData.file.FullName,
                GetOrGenerateMapPatch.locationData.locationDef, faction, false);

            rp.rect = rp.rect.MovedBy(map.Center - rp.rect.CenterCell);

            var singlePawnLord = rp.singlePawnLord ??
                                 LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell, 0),
                                     map);
            TraverseParms.For(TraverseMode.PassDoors);
            var resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
            if (resolveParams.pawnGroupMakerParams == null)
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.tile = map.Tile;
                resolveParams.pawnGroupMakerParams.faction = faction;
                resolveParams.pawnGroupMakerParams.points =
                    rp.settlementPawnGroupPoints ?? DefaultPawnsPoints.RandomInRange;
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }

            BaseGen.symbolStack.Push("pawnGroup", resolveParams);
            return false;
        }

        GetOrGenerateMapPatch.customSettlementGeneration = false;
        return true;
    }
}