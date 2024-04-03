using System.Collections.Generic;
using System.IO;
using RimWorld;
using UnityEngine;
using Verse;

namespace LocationGeneration;

public class Dialog_NameBlueprint : Window
{
    public static List<IntVec3> terrainKeys = [];
    public static List<IntVec3> roofsKeys = [];
    public static List<TerrainDef> terrainValues = [];
    public static List<RoofDef> roofsValues = [];
    private readonly bool includePawns;
    protected string curName;
    private bool focusedRenameField;

    private string name;
    private int startAcceptingInputAtFrame;

    public Dialog_NameBlueprint(string name, bool includePawns)
    {
        this.name = name;
        this.includePawns = includePawns;
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        closeOnClickedOutside = true;
    }

    protected int MaxNameLength => 50;

    public override Vector2 InitialSize => new Vector2(280f, 175f);

    private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

    public void WasOpenedByHotkey()
    {
        startAcceptingInputAtFrame = Time.frameCount + 1;
    }

    protected AcceptanceReport NameIsValid(string nameToCheck)
    {
        return nameToCheck.Length != 0;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var returnPressed = false;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            returnPressed = true;
            Event.current.Use();
        }

        GUI.SetNextControlName("RenameField");
        var text = Widgets.TextField(new Rect(0f, 15f, inRect.width, 35f), curName);
        if (AcceptsInput && text.Length < MaxNameLength)
        {
            curName = text;
        }
        else if (!AcceptsInput)
        {
            ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectAll();
        }

        if (!focusedRenameField)
        {
            UI.FocusControl("RenameField", this);
            focusedRenameField = true;
        }

        if (!Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 15f, inRect.width - 15f - 15f, 35f), "OK") &&
            !returnPressed)
        {
            return;
        }

        var acceptanceReport = NameIsValid(curName);
        if (!acceptanceReport.Accepted)
        {
            if (acceptanceReport.Reason.NullOrEmpty())
            {
                Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
            return;
        }

        SetName(curName);
        Find.WindowStack.TryRemove(this);
    }


    public void GetRocks(Map map, ref List<Thing> rocks, ref List<Thing> processedRocks)
    {
        var rocksToProcess = new List<Thing>();
        foreach (var rock in rocks)
        {
            if (processedRocks.Contains(rock))
            {
                continue;
            }

            foreach (var pos in GenAdj.CellsAdjacent8Way(rock))
            {
                var things = map.thingGrid.ThingsListAt(pos);
                if (things is not { Count: > 0 })
                {
                    continue;
                }

                foreach (var thing in things)
                {
                    if (thing is Mineable && !processedRocks.Contains(thing))
                    {
                        rocksToProcess.Add(thing);
                    }
                }
            }

            processedRocks.Add(rock);
        }

        if (rocksToProcess.Count > 0)
        {
            GetRocks(map, ref rocksToProcess, ref processedRocks);
        }
    }

    protected void SetName(string nameToCheck)
    {
        name = GenText.SanitizeFilename(nameToCheck);
        var map = Find.CurrentMap;
        var pawns = new List<Pawn>();
        var corpses = new List<Corpse>();
        var pawnCorpses = new List<Pawn>();
        var filths = new List<Filth>();
        var buildings = new List<Building>();
        var things = new List<Thing>();
        var plants = new List<Plant>();
        var terrains = new Dictionary<IntVec3, TerrainDef>();
        var roofs = new Dictionary<IntVec3, RoofDef>();
        var tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();
        var mapSeed = Gen.HashCombineInt(Find.World.info.Seed, map.Tile);
        var hilliness = Find.WorldGrid[map.Tile].hilliness;
        foreach (var thing in map.listerThings.AllThings)
        {
            switch (thing)
            {
                case Gas or Mote:
                    continue;
                case Corpse corpse when map.areaManager.Home[thing.Position]:
                    corpses.Add(corpse);
                    pawnCorpses.Add(corpse.InnerPawn);
                    break;
                case Filth filth when map.areaManager.Home[thing.Position]:
                    filths.Add(filth);
                    break;
                case Pawn when !includePawns:
                    continue;
                case Pawn pawn when !map.areaManager.Home[pawn.Position]:
                    continue;
                case Pawn pawn:
                    Log.Message($"0 Adding {pawn}");
                    pawns.Add(pawn);
                    break;
                case Plant plant:
                {
                    var zone = map.zoneManager.ZoneAt(thing.Position);
                    if (zone is not Zone_Growing)
                    {
                        continue;
                    }

                    Log.Message($"1 Adding {plant}");
                    plants.Add(plant);
                    break;
                }
                case Building building when thing.Map.areaManager.Home[building.Position]:
                    buildings.Add(building);
                    break;
                default:
                {
                    if (thing.IsInAnyStorage())
                    {
                        things.Add(thing);
                    }

                    break;
                }
            }
        }

        var rocks = new List<Thing>();
        var processedRocks = new List<Thing>();
        foreach (var thing in buildings)
        {
            foreach (var pos in GenAdj.CellsAdjacent8Way(thing))
            {
                var things2 = map.thingGrid.ThingsListAt(pos);
                if (things2 is not { Count: > 0 })
                {
                    continue;
                }

                foreach (var thing2 in things2)
                {
                    if (thing2 is Mineable)
                    {
                        rocks.Add(thing2);
                    }
                }
            }
        }

        GetRocks(map, ref rocks, ref processedRocks);

        foreach (var rock in processedRocks)
        {
            things.Add(rock);
        }

        foreach (var intVec in map.AllCells)
        {
            if (!map.areaManager.Home[intVec])
            {
                continue;
            }

            var terrain = intVec.GetTerrain(map);
            if (terrain != null && map.terrainGrid.CanRemoveTopLayerAt(intVec))
            {
                terrains[intVec] = terrain;
            }

            var roof = intVec.GetRoof(map);
            if (roof != null && !map.roofGrid.RoofAt(intVec).isNatural)
            {
                roofs[intVec] = roof;
            }
        }

        foreach (var homeCell in map.areaManager.Home.ActiveCells)
        {
            tilesToSpawnPawnsOnThem.Add(homeCell);
        }

        var path = BlueprintUtility.GetConfigPath();
        var exists = Directory.Exists(path);
        if (!exists)
        {
            Directory.CreateDirectory(path);
        }

        path = BlueprintUtility.GetConfigPath(name);
        Scribe.saver.InitSaving(path, "Blueprint");
        Scribe_Collections.Look(ref pawnCorpses, "PawnCorpses", LookMode.Deep);
        Scribe_Collections.Look(ref corpses, "Corpses", LookMode.Deep);
        if (includePawns)
        {
            Scribe_Collections.Look(ref pawns, "Pawns", LookMode.Deep);
        }

        Scribe_Collections.Look(ref buildings, "Buildings", LookMode.Deep);
        Scribe_Collections.Look(ref things, "Things", LookMode.Deep);
        Scribe_Collections.Look(ref filths, "Filths", LookMode.Deep);
        Scribe_Collections.Look(ref plants, "Plants", LookMode.Deep);
        Scribe_Collections.Look(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys,
            ref terrainValues);
        Scribe_Collections.Look(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
        Scribe_Collections.Look(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);
        Scribe_Values.Look(ref mapSeed, "mapSeed");
        Scribe_Values.Look(ref hilliness, "hilliness");
        Scribe.saver.FinalizeSaving();
    }
}