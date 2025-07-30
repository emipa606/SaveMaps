using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Unity.Collections;
using Verse;
using Verse.AI;

namespace LocationGeneration;

public static class SettlementGeneration
{
    private static List<IntVec3> terrainKeys = [];
    private static List<TerrainDef> terrainValues = [];
    private static List<IntVec3> roofsKeys = [];
    private static List<RoofDef> roofsValues = [];
    private static readonly FieldInfo fogGridField = AccessTools.Field(typeof(FogGrid), "fogGrid");

    private static LocationDef GetLocationDefForMapParent(MapParent mapParent)
    {
        if (SettlementUtility_AttackNow.locationData?.locationDef != null)
        {
            return SettlementUtility_AttackNow.locationData.locationDef;
        }

        foreach (var locationDef in DefDatabase<LocationDef>.AllDefs)
        {
            if (mapParent is Settlement && mapParent.Faction != null &&
                locationDef.factionBase == mapParent.Faction.def)
            {
                return locationDef;
            }
        }

        return null;
    }

    public static FileInfo GetPresetFor(MapParent mapParent, out LocationDef locationDef)
    {
        locationDef = GetLocationDefForMapParent(mapParent);
        return GetPresetFor(locationDef);
    }

    public static FileInfo GetPresetFor(LocationDef locationDef)
    {
        if (locationDef == null)
        {
            return null;
        }

        string path;
        FileInfo file = null;
        if (locationDef.filePreset is { Length: > 0 })
        {
            path = Path.GetFullPath($"{locationDef.modContentPack.RootDir}/{locationDef.filePreset}");
            file = new FileInfo(path);
        }
        else if (locationDef.folderWithPresets is { Length: > 0 })
        {
            path = Path.GetFullPath($"{locationDef.modContentPack.RootDir}/{locationDef.folderWithPresets}");
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                file = directoryInfo.GetFiles().RandomElement();
            }
        }

        return file;
    }

    private static bool isChunk(Thing item)
    {
        if (item?.def?.thingCategories == null)
        {
            return false;
        }

        foreach (var category in item.def.thingCategories)
        {
            if (category == ThingCategoryDefOf.Chunks || category == ThingCategoryDefOf.StoneChunks)
            {
                return true;
            }
        }

        return false;
    }

    private static IntVec3 getCellCenterFor(List<IntVec3> cells)
    {
        var xAverages = cells.OrderBy(x => x.x);
        var xAverage = xAverages.ElementAt(xAverages.Count() / 2).x;
        var zAverages = cells.OrderBy(x => x.z);
        var zAverage = zAverages.ElementAt(zAverages.Count() / 2).z;
        var middleCell = new IntVec3(xAverage, 0, zAverage);
        return middleCell;
    }

    private static IntVec3 getOffsetPosition(LocationDef locationDef, IntVec3 cell, IntVec3 offset)
    {
        if (locationDef == null)
        {
            return cell + offset;
        }

        if (locationDef.disableCenterCellOffset)
        {
            return cell;
        }

        return cell + offset + locationDef.additionalCenterCellOffset;
    }

    public static void DoSettlementGeneration(Map map, string path, LocationDef locationDef,
        Faction faction, bool disableFog, bool destroyEverything = false, bool excludeColonists = false)
    {
        SettlementUtility_AttackNow.locationData = null;
        SettlementUtility_AttackNow.CaravanArrival = false;
        var mapComp = map.GetComponent<MapComponentGeneration>();
        try
        {
            if (locationDef is { destroyEverythingOnTheMapBeforeGeneration: true } || destroyEverything)
            {
                var thingsToDespawn = map.listerThings.AllThings;
                if (thingsToDespawn is { Count: > 0 })
                {
                    for (var i = thingsToDespawn.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            if (excludeColonists && thingsToDespawn[i] is Pawn &&
                                (thingsToDespawn[i].Faction?.IsPlayer ?? false))
                            {
                                continue;
                            }

                            if (thingsToDespawn[i].Spawned)
                            {
                                thingsToDespawn[i].DeSpawn(DestroyMode.WillReplace);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(
                                $"4 Cant despawn: {thingsToDespawn[i]} - {thingsToDespawn[i].Position}error: {ex}");
                        }
                    }
                }

                var rooves = map.AllCells.Where(x => x.Roofed(map)).ToList();
                foreach (var cell in rooves)
                {
                    map.roofGrid.SetRoof(cell, null);
                }
            }

            if (locationDef is { factionDefForNPCsAndTurrets: not null })
            {
                faction = Find.FactionManager.FirstFactionOfDef(locationDef.factionDefForNPCsAndTurrets);
            }

            var thingsToDestroy = new List<Thing>();
            var tilesToProcess = new HashSet<IntVec3>();
            var mapSize = IntVec3.Invalid;
            var corpses = new List<Corpse>();
            var pawnCorpses = new List<Pawn>();
            var pawns = new List<Pawn>();
            var buildings = new List<Building>();
            var things = new List<Thing>();
            var filths = new List<Filth>();
            var plants = new List<Plant>();
            var terrains = new Dictionary<IntVec3, TerrainDef>();
            var roofs = new Dictionary<IntVec3, RoofDef>();
            var tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();
            var mapSeed = -1;
            var hilliness = Hilliness.Flat;
            Scribe.loader.InitLoading(path);
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
            Scribe_Values.Look(ref mapSize, "mapSize", IntVec3.Invalid);
            var riverOffsetSize = 0;
            Scribe_Values.Look(ref riverOffsetSize, "riverOffsetSize");
            var riverOffsetMap = new byte[riverOffsetSize];
            DataExposeUtility.LookByteArray(ref riverOffsetMap, "riverOffsetMap");
            Scribe.loader.FinalizeLoading();


            if (mapSize.IsValid && map.info.Size != mapSize)
            {
                map.info.Size = mapSize;
                var otherPawns = map.mapPawns.AllPawns;
                var toSpawn = new List<Pawn>();
                for (var i = otherPawns.Count - 1; i >= 0; i--)
                {
                    var pawn = otherPawns[i];
                    if (pawn.IsColonistPlayerControlled && (excludeColonists || !destroyEverything))
                    {
                        toSpawn.Add(pawn);
                    }

                    if (pawn.Spawned)
                    {
                        pawn.DeSpawn();
                    }
                }

                map.ConstructComponents();
                var cellIndices = map.cellIndices;
                if (fogGridField.GetValue(map.fogGrid) == null)
                {
                    fogGridField.SetValue(map.fogGrid, new NativeArray<bool>[cellIndices.NumGridCells]);
                }

                var fogGrid = (NativeArray<bool>)fogGridField.GetValue(map.fogGrid);
                foreach (var allCell in map.AllCells)
                {
                    fogGrid[cellIndices.CellToIndex(allCell)] = true;
                }

                fogGridField.SetValue(map.fogGrid, fogGrid);

                if (Current.ProgramState == ProgramState.Playing)
                {
                    map.roofGrid.Drawer.SetDirty();
                }

                var terrainGrid = map.terrainGrid;
                foreach (var allCell in map.AllCells)
                {
                    try
                    {
                        terrainGrid.SetTerrain(allCell, TerrainDefOf.Sand);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                map.areaManager.AddStartingAreas();
                map.weatherDecider.StartInitialWeather();
                map.FinalizeInit();

                foreach (var pawn in toSpawn)
                {
                    GenSpawn.Spawn(pawn, pawn.Position, map);
                }
            }

            if (corpses is null)
            {
                corpses = [];
            }
            else
            {
                corpses.RemoveAll(x => x is null);
            }

            pawnCorpses?.RemoveAll(x => x is null);

            if (pawns is null)
            {
                pawns = [];
            }
            else
            {
                pawns.RemoveAll(x => x is null);
            }

            if (buildings is null)
            {
                buildings = [];
            }
            else
            {
                buildings.RemoveAll(x => x is null);
            }

            if (things is null)
            {
                things = [];
            }
            else
            {
                things.RemoveAll(x => x is null);
            }

            if (filths is null)
            {
                filths = [];
            }
            else
            {
                filths.RemoveAll(x => x is null);
            }

            if (plants is null)
            {
                plants = [];
            }
            else
            {
                plants.RemoveAll(x => x is null);
            }

            var cells = new List<IntVec3>(tilesToSpawnPawnsOnThem);
            cells.AddRange(buildings.Select(x => x.Position).ToList());
            var centerCell = getCellCenterFor(cells);
            var offset = locationDef is { disableCenterCellOffset: false }
                ? map.Center - centerCell
                : IntVec3.Zero;

            if (corpses is { Count: > 0 })
            {
                foreach (var corpse in corpses)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, corpse.Position, offset);
                        if (position.InBounds(map))
                        {
                            GenSpawn.Spawn(corpse, position, map);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"1 Error in map generating, cant spawn {corpse} - {ex}");
                    }
                }
            }

            if (pawns is { Count: > 0 })
            {
                foreach (var pawn in pawns)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, pawn.Position, offset);
                        if (!position.InBounds(map))
                        {
                            continue;
                        }

                        pawn.pather = new Pawn_PathFollower(pawn);
                        GenSpawn.Spawn(pawn, position, map);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"2 Error in map generating, cant spawn {pawn} - {ex}");
                    }
                }
            }

            foreach (var pawn in map.mapPawns.AllPawns)
            {
                pawn.jobs?.StopAll();
                pawn.pather?.StopDead();
            }

            if (tilesToSpawnPawnsOnThem is { Count: > 0 })
            {
                foreach (var tile in tilesToSpawnPawnsOnThem)
                {
                    var position = getOffsetPosition(locationDef, tile, offset);
                    try
                    {
                        if (position.InBounds(map))
                        {
                            var things2 = map.thingGrid.ThingsListAt(position);
                            foreach (var thing in things2)
                            {
                                if (thing is Building || thing is Plant plant && plant.def != ThingDefOf.Plant_Grass ||
                                    isChunk(thing))
                                {
                                    thingsToDestroy.Add(thing);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"3 Error in map generating, cant spawn {position} - {ex}");
                    }
                }
            }

            if (thingsToDestroy is { Count: > 0 })
            {
                for (var i = thingsToDestroy.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (!thingsToDestroy[i].Spawned)
                        {
                            continue;
                        }

                        Log.Message($"2 Despawning: {thingsToDestroy[i]}");
                        thingsToDestroy[i].DeSpawn(DestroyMode.WillReplace);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            $"4 Cant despawn: {thingsToDestroy[i]} - {thingsToDestroy[i].Position}error: {ex}");
                    }
                }
            }

            if (buildings is { Count: > 0 })
            {
                foreach (var building in buildings)
                {
                    var position = getOffsetPosition(locationDef, building.Position, offset);
                    try
                    {
                        if (position.InBounds(map))
                        {
                            GenSpawn.Spawn(building, position, map, building.Rotation);
                            if (building is IThingHolder holder)
                            {
                                var innerThings = holder.GetDirectlyHeldThings();
                                foreach (var thing in innerThings)
                                {
                                    if (thing is Corpse { InnerPawn: null } corpse)
                                    {
                                        corpse.InnerPawn = PawnGenerator.GeneratePawn(
                                            new PawnGenerationRequest(PawnKindDefOf.Villager, faction));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"5 Error in map generating, cant spawn {building} - {position} - {ex}");
                    }
                }
            }

            if (filths is { Count: > 0 })
            {
                foreach (var filth in filths)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, filth.Position, offset);
                        if (position.InBounds(map))
                        {
                            GenSpawn.Spawn(filth, position, map);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"6.5 Error in map generating, cant spawn {filth} - {ex}");
                    }
                }
            }

            if (plants is { Count: > 0 })
            {
                foreach (var plant in plants)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, plant.Position, offset);
                        if (position.InBounds(map) &&
                            map.fertilityGrid.FertilityAt(position) >= plant.def.plant.fertilityMin)
                        {
                            GenSpawn.Spawn(plant, position, map);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"6 Error in map generating, cant spawn {plant} - {ex}");
                    }
                }
            }

            var containers = map.listerThings.AllThings.Where(x => x is Building_Storage).ToList();
            if (things is { Count: > 0 })
            {
                foreach (var thing in things)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, thing.Position, offset);
                        if (!position.InBounds(map))
                        {
                            continue;
                        }

                        GenSpawn.Spawn(thing, position, map);
                        if (locationDef is { moveThingsToShelves: true })
                        {
                            tryDistributeTo(thing, map, containers, faction != Faction.OfPlayer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"7 Error in map generating, cant spawn {thing} - {ex}");
                    }
                }
            }

            if (locationDef is { moveThingsToShelves: true })
            {
                foreach (var item in map.listerThings.AllThings)
                {
                    if (item.IsForbidden(Faction.OfPlayer))
                    {
                        tryDistributeTo(item, map, containers, faction != Faction.OfPlayer);
                    }
                }
            }

            if (terrains is { Count: > 0 })
            {
                foreach (var terrain in terrains)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, terrain.Key, offset);
                        if (position.InBounds(map))
                        {
                            map.terrainGrid.SetTerrain(position, terrain.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"8 Error in map generating, cant spawn {terrain.Key} - {ex}");
                    }
                }
            }

            if (roofs is { Count: > 0 })
            {
                foreach (var roof in roofs)
                {
                    try
                    {
                        var position = getOffsetPosition(locationDef, roof.Key, offset);
                        if (position.InBounds(map))
                        {
                            map.roofGrid.SetRoof(position, roof.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"9 Error in map generating, cant spawn {roof.Key} - {ex}");
                    }
                }
            }

            if (mapSeed != -1)
            {
                Rand.PushState();
                var seed = Rand.Seed = mapSeed;

                try
                {
                    Rand.Seed = seed;
                    RockNoises.Init(map);
                    var mapGeneratorDef = map.Parent.MapGeneratorDef;
                    var tmpGenSteps = (from x in mapGeneratorDef.genSteps orderby x.order, x.index select x).ToList();
                    MapGenerator.mapBeingGenerated = map;
                    foreach (var genStepDef in tmpGenSteps)
                    {
                        DeepProfiler.Start($"GenStep - {genStepDef}");
                        try
                        {
                            if (genStepDef == DefDatabase<GenStepDef>.GetNamed("RocksFromGrid"))
                            {
                                Rand.Seed = Gen.HashCombineInt(seed, getSeedPart(mapGeneratorDef, genStepDef));
                                GenStep_RocksFromGridCustom.Hilliness = hilliness;
                                var genStep = new GenStep_RocksFromGridCustom();
                                genStep.Generate(map, default);
                            }
                        }
                        catch (Exception arg)
                        {
                            Log.Error($"Error in GenStep: {arg}");
                        }
                        finally
                        {
                            DeepProfiler.End();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to generate gen steps: {ex}");
                }
            }

            if (locationDef != null && (locationDef.percentOfDamagedWalls.HasValue ||
                                        locationDef.percentOfDestroyedWalls.HasValue ||
                                        locationDef.percentOfDamagedFurnitures.HasValue))
            {
                var walls = map.listerThings.AllThings
                    .Where(x => x.def.IsEdifice() && x.def.defName.ToLower().Contains("wall")).ToList();
                if (locationDef.percentOfDestroyedWalls.HasValue)
                {
                    var percent = locationDef.percentOfDestroyedWalls.Value.RandomInRange * 100f;
                    var countToTake = (int)(percent * walls.Count / 100f);
                    var wallsToDestroy = walls.InRandomOrder().Take(countToTake).ToList();
                    for (var num = wallsToDestroy.Count - 1; num >= 0; num--)
                    {
                        walls.Remove(wallsToDestroy[num]);
                        wallsToDestroy[num].DeSpawn();
                    }
                }

                if (locationDef.percentOfDamagedWalls.HasValue)
                {
                    var percent = locationDef.percentOfDamagedWalls.Value.RandomInRange * 100f;
                    var countToTake = (int)(percent * walls.Count / 100f);
                    var wallsToDamage = walls.InRandomOrder().Take(countToTake).ToList();
                    for (var num = wallsToDamage.Count - 1; num >= 0; num--)
                    {
                        var damagePercent = Rand.Range(0.3f, 0.6f);
                        var hitpointsToTake = (int)(wallsToDamage[num].MaxHitPoints * damagePercent);
                        wallsToDamage[num].HitPoints = hitpointsToTake;
                    }
                }

                if (locationDef.percentOfDamagedFurnitures.HasValue)
                {
                    var furnitures = map.listerThings.AllThings
                        .Where(x => !walls.Contains(x) && x.def.IsBuildingArtificial).ToList();
                    var percent = locationDef.percentOfDamagedFurnitures.Value.RandomInRange * 100f;
                    var countToTake = (int)(percent * furnitures.Count / 100f);
                    var furnituresToDamage = furnitures.InRandomOrder().Take(countToTake).ToList();
                    for (var num = furnituresToDamage.Count - 1; num >= 0; num--)
                    {
                        var damagePercent = Rand.Range(0.3f, 0.6f);
                        var hitpointsToTake = (int)(furnituresToDamage[num].MaxHitPoints * damagePercent);
                        furnituresToDamage[num].HitPoints = hitpointsToTake;
                    }
                }
            }

            if (faction.def.HasModExtension<SettlementOptionModExtension>())
            {
                var options = faction.def.GetModExtension<SettlementOptionModExtension>();
                if (options.removeVanillaGeneratedPawns)
                {
                    for (var i = map.mapPawns.PawnsInFaction(faction).Count - 1; i >= 0; i--)
                    {
                        Log.Message($"3 Despawning: {thingsToDestroy[i]}");
                        map.mapPawns.PawnsInFaction(faction)[i].DeSpawn();
                    }
                }

                if (options.pawnsToGenerate is { Count: > 0 } &&
                    tilesToSpawnPawnsOnThem is { Count: > 0 })
                {
                    foreach (var pawn in options.pawnsToGenerate)
                    {
                        foreach (var unused in Enumerable.Range(1, (int)pawn.selectionWeight))
                        {
                            var settler = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawn.kind, faction));
                            try
                            {
                                var pos = tilesToSpawnPawnsOnThem.Where(x => map.thingGrid
                                    .ThingsListAt(x).Count(y => y is Building) == 0).RandomElement();
                                GenSpawn.Spawn(settler, pos, map);
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"10 Error in map generating, cant spawn {settler} - {ex}");
                            }
                        }
                    }
                }
            }

            if (!disableFog)
            {
                try
                {
                    FloodFillerFog.DebugRefogMap(map);
                }
                catch
                {
                    foreach (var cell in map.AllCells)
                    {
                        if (tilesToProcess.Contains(cell) || cell.GetFirstBuilding(map) is Mineable)
                        {
                            continue;
                        }

                        var item = cell.GetFirstItem(map);

                        var room = item?.GetRoom();
                        if (room is { PsychologicallyOutdoors: true })
                        {
                            FloodFillerFog.FloodUnfog(cell, map);
                        }
                    }
                }

                mapComp.reFog = true;
            }

            mapComp.doGeneration = false;
            mapComp.path = null;
            SettlementUtility_AttackNow.CaravanArrival = false;
            map.regionAndRoomUpdater.Enabled = true;

            if (riverOffsetSize <= 0)
            {
                tilesToSpawnPawnsOnThem.Select(x => getOffsetPosition(locationDef, x, offset)).ToHashSet();
                return;
            }

            tilesToSpawnPawnsOnThem.Select(x => getOffsetPosition(locationDef, x, offset)).ToHashSet();
            return;
        }
        catch (Exception ex)
        {
            Log.Error($"Error in DoSettlementGeneration: {ex}");
        }

        mapComp.doGeneration = false;
        mapComp.path = null;
        map.regionAndRoomUpdater.Enabled = true;
    }

    private static int getSeedPart(MapGeneratorDef def, GenStepDef genStepDef)
    {
        var seedPart = genStepDef.genStep.SeedPart;
        var num = 0;
        var firstDef = def.genSteps.OrderBy(x => x.index).FirstOrDefault(x => x == genStepDef);
        var index = def.genSteps.OrderBy(x => x.index).ToList().IndexOf(firstDef);
        for (var i = 0; i < index; i++)
        {
            if (def.genSteps[i].genStep.SeedPart == seedPart)
            {
                num++;
            }
        }

        return seedPart + num;
    }

    public static void InitialiseLocationGeneration(Map map, FileInfo file, LocationDef locationDef)
    {
        if (locationDef == null || file == null)
        {
            return;
        }

        var comp = map.GetComponent<MapComponentGeneration>();
        if (comp.path.Length != 0)
        {
            return;
        }

        comp.doGeneration = true;
        comp.path = file.FullName;
        comp.locationDef = locationDef;
    }

    private static void tryDistributeTo(Thing thing, Map map, List<Thing> containers, bool setForbidden)
    {
        var containerPlaces = new Dictionary<Thing, List<IntVec3>>();
        for (var num = containers.Count - 1; num >= 0; num--)
        {
            var c = containers[num];
            foreach (var pos in c.OccupiedRect().Cells)
            {
                var canPlace = true;
                foreach (var t in pos.GetThingList(map))
                {
                    if (t == c || t is Filth)
                    {
                        continue;
                    }

                    canPlace = false;
                    break;
                }

                if (!canPlace)
                {
                    continue;
                }

                if (containerPlaces.ContainsKey(c))
                {
                    containerPlaces[c].Add(pos);
                }
                else
                {
                    containerPlaces[c] = [pos];
                }
            }
        }

        if (!containerPlaces.Any())
        {
            return;
        }

        var container =
            (Building_Storage)GenClosest.ClosestThing_Global(thing.Position, containerPlaces.Keys, 9999f);
        if (container == null || !containerPlaces.TryGetValue(container, out var positions))
        {
            return;
        }

        var randomPosition = positions.RandomElement();
        containerPlaces[container].Remove(randomPosition);
        thing.Position = randomPosition;
        if (setForbidden)
        {
            thing.SetForbidden(true);
        }

        if (!containerPlaces[container].Any())
        {
            containerPlaces.Remove(container);
        }
    }
}