using System.Collections.Generic;
using System.IO;
using RimWorld;
using Verse;

namespace LocationGeneration;

public static class BlueprintUtility
{
    public static List<IntVec3> terrainKeys = [];
    public static List<IntVec3> roofsKeys = [];
    public static List<TerrainDef> terrainValues = [];
    public static List<RoofDef> roofsValues = [];

    public static string GetConfigPath()
    {
        return Path.Combine(GenFilePaths.ConfigFolderPath, "SavedMapPresets");
    }

    public static string GetConfigPath(string fileName)
    {
        return Path.Combine(GetConfigPath(), $"{fileName}.xml");
    }

    public static void SaveEverything(string path, Map map, bool includeColonists = true)
    {
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path) ?? string.Empty);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        var riverOffsetMap = map.waterInfo.riverOffsetMap;
        var pawns = new List<Pawn>();
        var pawnCorpses = new List<Pawn>();
        var corpses = new List<Corpse>();
        var filths = new List<Filth>();
        var buildings = new List<Building>();
        var things = new List<Thing>();
        var plants = new List<Plant>();
        var terrains = new Dictionary<IntVec3, TerrainDef>();
        var roofs = new Dictionary<IntVec3, RoofDef>();
        var tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();
        var mapSeed = Gen.HashCombineInt(Find.World.info.Seed, map.Tile);
        var hilliness = Find.WorldGrid[map.Tile].hilliness;
        var mapSize = map.Size;
        foreach (var thing in map.listerThings.AllThings)
        {
            switch (thing)
            {
                case Gas or Mote:
                case Corpse corpse when corpse.InnerPawn.Faction == Faction.OfPlayer && !includeColonists:
                    continue;
                case Corpse corpse:
                    corpses.Add(corpse);
                    pawnCorpses.Add(corpse.InnerPawn);
                    break;
                case Filth filth:
                    filths.Add(filth);
                    break;
                case Pawn pawn when pawn.Faction == Faction.OfPlayer && !includeColonists:
                    continue;
                case Pawn pawn:
                    pawns.Add(pawn);
                    break;
                case Plant plant:
                    plants.Add(plant);
                    break;
                case Building building:
                    buildings.Add(building);
                    break;
                default:
                    things.Add(thing);
                    break;
            }
        }

        foreach (var intVec in map.AllCells)
        {
            var terrain = intVec.GetTerrain(map);
            if (terrain != null)
            {
                terrains[intVec] = terrain;
            }

            var roof = intVec.GetRoof(map);
            if (roof != null)
            {
                roofs[intVec] = roof;
            }
        }

        foreach (var homeCell in map.areaManager.Home.ActiveCells)
        {
            tilesToSpawnPawnsOnThem.Add(homeCell);
        }

        Scribe.saver.InitSaving(path, "Blueprint");
        Scribe_Collections.Look(ref pawnCorpses, "PawnCorpses", LookMode.Deep);
        Scribe_Collections.Look(ref corpses, "Corpses", LookMode.Deep);
        Scribe_Collections.Look(ref pawns, "Pawns", LookMode.Deep);
        Scribe_Collections.Look(ref buildings, "Buildings", LookMode.Deep);
        Scribe_Collections.Look(ref filths, "Filths", LookMode.Deep);
        Scribe_Collections.Look(ref things, "Things", LookMode.Deep);
        Scribe_Collections.Look(ref plants, "Plants", LookMode.Deep);
        Scribe_Collections.Look(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys,
            ref terrainValues);
        Scribe_Collections.Look(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
        Scribe_Collections.Look(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);
        Scribe_Values.Look(ref mapSeed, "mapSeed");
        Scribe_Values.Look(ref hilliness, "hilliness");
        Scribe_Values.Look(ref mapSize, "mapSize");
        if (riverOffsetMap != null)
        {
            var riverOffsetSize = riverOffsetMap.Length;
            Scribe_Values.Look(ref riverOffsetSize, "riverOffsetSize");
            DataExposeUtility.LookByteArray(ref riverOffsetMap, "riverOffsetMap");
        }

        Scribe.saver.FinalizeSaving();
    }
}