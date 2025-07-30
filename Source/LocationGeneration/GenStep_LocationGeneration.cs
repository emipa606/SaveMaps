using Verse;
using static LocationGeneration.SettlementUtility_AttackNow;

namespace LocationGeneration;

public class GenStep_LocationGeneration : GenStep
{
    public LocationDef locationDef;
    public override int SeedPart => 341641510;

    public override void Generate(Map map, GenStepParams parms)
    {
        var filePreset = SettlementGeneration.GetPresetFor(locationDef);
        if (filePreset != null)
        {
            locationData = new LocationData { file = filePreset, locationDef = locationDef };
        }
    }
}