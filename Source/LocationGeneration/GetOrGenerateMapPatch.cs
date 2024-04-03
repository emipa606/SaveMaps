using System.IO;
using HarmonyLib;
using RimWorld.Planet;

namespace LocationGeneration;

[HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
public class GetOrGenerateMapPatch
{
    public static bool customSettlementGeneration;
    public static bool caravanArrival;
    public static LocationData locationData;

    public static void Prefix(ref Settlement settlement)
    {
        var filePreset = SettlementGeneration.GetPresetFor(settlement, out var locationDef);
        if (filePreset == null)
        {
            return;
        }

        locationData = new LocationData { file = filePreset, locationDef = locationDef };
        customSettlementGeneration = true;
    }

    public static void Postfix(ref Caravan caravan, ref Settlement settlement)
    {
        if (customSettlementGeneration)
        {
            customSettlementGeneration = false;
        }
    }

    public class LocationData
    {
        public FileInfo file;
        public LocationDef locationDef;
    }
}