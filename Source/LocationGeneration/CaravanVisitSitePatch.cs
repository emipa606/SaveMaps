using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(CaravanArrivalAction_VisitSite))]
[HarmonyPatch("Arrived")]
public static class CaravanVisitSitePatch
{
    public static void Prefix()
    {
        GetOrGenerateMapPatch.caravanArrival = true;
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
                GetOrGenerateMapPatch.customSettlementGeneration = true;
                GetOrGenerateMapPatch.locationData = new GetOrGenerateMapPatch.LocationData
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