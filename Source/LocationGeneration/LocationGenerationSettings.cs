using Verse;

namespace LocationGeneration;

public class LocationGenerationSettings : ModSettings
{
    public bool allowVisitingSettlements;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref allowVisitingSettlements, "allowVisitingSettlements");
    }
}