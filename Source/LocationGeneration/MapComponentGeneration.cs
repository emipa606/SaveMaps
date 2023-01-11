using Verse;

namespace LocationGeneration;

public class MapComponentGeneration : MapComponent
{
    public bool doGeneration;
    public LocationDef locationDef;
    public string path = "";
    public bool reFog;

    public MapComponentGeneration(Map map) : base(map)
    {
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        if (doGeneration && path?.Length > 0)
        {
            SettlementGeneration.DoSettlementGeneration(map, path, locationDef, map.ParentFaction, false);
            doGeneration = false;
        }

        if (!reFog)
        {
            return;
        }

        Log.Message($"Refog{map}");
        FloodFillerFog.DebugRefogMap(map);
        reFog = false;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref doGeneration, "DoGeneration");
        Scribe_Values.Look(ref path, "path", "");
    }
}