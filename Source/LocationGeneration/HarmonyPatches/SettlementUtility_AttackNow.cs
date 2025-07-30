using System.IO;
using HarmonyLib;
using RimWorld.Planet;

namespace LocationGeneration;

[HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
public class SettlementUtility_AttackNow
{
    public static bool CustomSettlementGeneration;
    public static bool CaravanArrival;
    public static LocationData locationData;

    public static void Prefix(ref Settlement settlement)
    {
        var filePreset = SettlementGeneration.GetPresetFor(settlement, out var locationDef);
        if (filePreset == null)
        {
            return;
        }

        locationData = new LocationData { file = filePreset, locationDef = locationDef };
        CustomSettlementGeneration = true;
    }

    public static void Postfix()
    {
        if (CustomSettlementGeneration)
        {
            CustomSettlementGeneration = false;
        }
    }

    public class LocationData
    {
        public FileInfo file;
        public LocationDef locationDef;
    }
}