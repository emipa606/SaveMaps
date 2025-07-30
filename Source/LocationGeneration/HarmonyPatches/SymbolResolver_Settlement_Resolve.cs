using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace LocationGeneration;

[HarmonyPatch(typeof(SymbolResolver_Settlement), nameof(SymbolResolver_Settlement.Resolve))]
public class SymbolResolver_Settlement_Resolve
{
    private static readonly FloatRange defaultPawnsPoints = new(1150f, 1600f);

    public static bool Prefix(ResolveParams rp)
    {
        var map = BaseGen.globalSettings.map;
        if (SettlementUtility_AttackNow.CustomSettlementGeneration)
        {
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            SettlementGeneration.DoSettlementGeneration(map, SettlementUtility_AttackNow.locationData.file.FullName,
                SettlementUtility_AttackNow.locationData.locationDef, faction, false);

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
                    rp.settlementPawnGroupPoints ?? defaultPawnsPoints.RandomInRange;
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }

            BaseGen.symbolStack.Push("pawnGroup", resolveParams);
            return false;
        }

        SettlementUtility_AttackNow.CustomSettlementGeneration = false;
        return true;
    }
}