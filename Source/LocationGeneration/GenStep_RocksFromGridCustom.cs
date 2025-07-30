using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration;

public class GenStep_RocksFromGridCustom : GenStep
{
    private const int MinRoofedCellsPerGroup = 20;

    private const float MaxMineableValue = float.MaxValue;

    public static Hilliness Hilliness;

    public override int SeedPart => 1182952823;

    private static ThingDef rockDefAt(IntVec3 c)
    {
        ThingDef thingDef = null;
        var num = -999999f;
        foreach (var rockNoise in RockNoises.rockNoises)
        {
            var value = rockNoise.noise.GetValue(c);
            if (!(value > num))
            {
                continue;
            }

            thingDef = rockNoise.rockDef;
            num = value;
        }

        if (thingDef != null)
        {
            return thingDef;
        }

        Log.ErrorOnce($"Did not get rock def to generate at {c}", 50812);
        thingDef = ThingDefOf.Sandstone;

        return thingDef;
    }

    public override void Generate(Map map, GenStepParams parms)
    {
        if (map.TileInfo.WaterCovered)
        {
            return;
        }

        map.regionAndRoomUpdater.Enabled = false;
        var num = 0.7f;
        var list = new List<RoofThreshold>();
        var roofThreshold = new RoofThreshold
        {
            roofDef = RoofDefOf.RoofRockThick,
            minGridVal = num * 1.14f
        };
        list.Add(roofThreshold);
        var roofThreshold2 = new RoofThreshold
        {
            roofDef = RoofDefOf.RoofRockThin,
            minGridVal = num * 1.04f
        };
        list.Add(roofThreshold2);
        var elevation = MapGenerator.Elevation;
        var caves = MapGenerator.Caves;
        foreach (var allCell in map.AllCells)
        {
            var num2 = elevation[allCell];
            if (!(num2 > num))
            {
                continue;
            }

            if (caves[allCell] <= 0f)
            {
                GenSpawn.Spawn(rockDefAt(allCell), allCell, map);
            }

            foreach (var threshold in list)
            {
                if (!(num2 > threshold.minGridVal))
                {
                    continue;
                }

                map.roofGrid.SetRoof(allCell, threshold.roofDef);
                break;
            }
        }

        var visited = new BoolGrid(map);
        var toRemove = new List<IntVec3>();
        foreach (var allCell2 in map.AllCells)
        {
            if (visited[allCell2] || !isNaturalRoofAt(allCell2, map))
            {
                continue;
            }

            toRemove.Clear();
            map.floodFiller.FloodFill(allCell2, x => isNaturalRoofAt(x, map), delegate(IntVec3 x)
            {
                visited[x] = true;
                toRemove.Add(x);
            });
            if (toRemove.Count >= 20)
            {
                continue;
            }

            foreach (var intVec3 in toRemove)
            {
                map.roofGrid.SetRoof(intVec3, null);
            }
        }

        var genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable
        {
            maxValue = MaxMineableValue
        };
        var num3 = 10f;
        switch (Hilliness)
        {
            case Hilliness.Flat:
                num3 = 4f;
                break;
            case Hilliness.SmallHills:
                num3 = 8f;
                break;
            case Hilliness.LargeHills:
                num3 = 11f;
                break;
            case Hilliness.Mountainous:
                num3 = 15f;
                break;
            case Hilliness.Impassable:
                num3 = 16f;
                break;
        }

        genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num3, num3);
        genStep_ScatterLumpsMineable.Generate(map, parms);
        map.regionAndRoomUpdater.Enabled = true;
    }

    private static bool isNaturalRoofAt(IntVec3 c, Map map)
    {
        return c.Roofed(map) && c.GetRoof(map).isNatural;
    }

    private class RoofThreshold
    {
        public float minGridVal;
        public RoofDef roofDef;
    }
}