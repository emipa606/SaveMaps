using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace LocationGeneration;

public class SitePartWorker_Location : SitePartWorker
{
    public override SitePartParams GenerateDefaultParams(float myThreatPoints, int tile, Faction faction)
    {
        var sitePartParams = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        sitePartParams.threatPoints = Mathf.Max(sitePartParams.threatPoints,
            faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
        return sitePartParams;
    }
}