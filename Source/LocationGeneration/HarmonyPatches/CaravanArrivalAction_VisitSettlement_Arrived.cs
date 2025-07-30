using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement), nameof(CaravanArrivalAction_VisitSettlement.Arrived))]
public static class CaravanArrivalAction_VisitSettlement_Arrived
{
    public static void Prefix()
    {
        SettlementUtility_AttackNow.CaravanArrival = true;
        Log.Message("GetOrGenerateMapPatch.caravanArrival true");
    }

    public static void Postfix(Caravan caravan, Settlement ___settlement)
    {
        if (!___settlement.HasMap)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                var filePreset = SettlementGeneration.GetPresetFor(___settlement, out var locationDef);
                if (filePreset != null)
                {
                    SettlementUtility_AttackNow.CustomSettlementGeneration = true;
                    SettlementUtility_AttackNow.locationData = new SettlementUtility_AttackNow.LocationData
                        { file = filePreset, locationDef = locationDef };
                }

                var orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
                CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true);

                if (filePreset != null)
                {
                    SettlementGeneration.InitialiseLocationGeneration(orGenerateMap, filePreset, locationDef);
                }
            }, "GeneratingMapForNewEncounter", false, null);
            return;
        }

        var orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
        CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true);
    }
}