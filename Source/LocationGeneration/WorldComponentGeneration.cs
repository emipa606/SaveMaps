using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

public class WorldComponentGeneration(World world) : WorldComponent(world)
{
    private List<int> intKeys;
    private List<IntVec3> intVecValues;
    public Dictionary<int, IntVec3> tileSizes = new();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref tileSizes, "tileSizes", LookMode.Value, LookMode.Value, ref intKeys,
            ref intVecValues);
    }
}