using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(CaravanArrivalAction_VisitSite), nameof(CaravanArrivalAction_VisitSite.Arrived))]
public static class CaravanArrivalAction_VisitSite_Arrived
{
    public static void Prefix()
    {
        SettlementUtility_AttackNow.CaravanArrival = true;
        Log.Message("GetOrGenerateMapPatch.caravanArrival true");
    }

    public static void Postfix(Caravan caravan, Site ___site)
    {
        if (___site.HasMap)
        {
            return;
        }

        LongEventHandler.QueueLongEvent(delegate
        {
            var filePreset = SettlementGeneration.GetPresetFor(___site, out var locationDef);
            if (filePreset != null)
            {
                SettlementUtility_AttackNow.CustomSettlementGeneration = true;
                SettlementUtility_AttackNow.locationData = new SettlementUtility_AttackNow.LocationData
                    { file = filePreset, locationDef = locationDef };
            }

            var orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true);

            if (filePreset != null)
            {
                SettlementGeneration.InitialiseLocationGeneration(orGenerateMap, filePreset, locationDef);
            }
        }, "GeneratingMapForNewEncounter", false, null);
    }
}