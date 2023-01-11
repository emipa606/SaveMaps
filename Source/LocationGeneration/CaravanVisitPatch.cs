using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement))]
[HarmonyPatch("Arrived")]
public static class CaravanVisitPatch
{
    public static void Prefix()
    {
        GetOrGenerateMapPatch.caravanArrival = true;
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
                    GetOrGenerateMapPatch.customSettlementGeneration = true;
                    GetOrGenerateMapPatch.locationData = new GetOrGenerateMapPatch.LocationData
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