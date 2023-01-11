using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace LocationGeneration;

[StaticConstructorOnStartup]
public static class Actions
{
    [DebugAction("Save Maps", "Save everything")]
    public static void SaveEverything()
    {
        var name = "";
        var dialog = new Dialog_SaveEverything(name);
        Find.WindowStack.Add(dialog);
    }

    [DebugAction("Save Maps", "Save everything w/o colonists")]
    public static void SaveEverythingWithoutColonists()
    {
        var name = "";
        var dialog = new Dialog_SaveEverything(name, false);
        Find.WindowStack.Add(dialog);
    }

    [DebugAction("Save Maps", "Save in home area with colonists")]
    public static void CreateBlueprint()
    {
        var name = "";
        var dialog = new Dialog_NameBlueprint(name, true);
        Find.WindowStack.Add(dialog);
    }

    [DebugAction("Save Maps", "Save in home area w/o colonists")]
    public static void CreateBlueprintWithoutColonists()
    {
        var name = "";
        var dialog = new Dialog_NameBlueprint(name, false);
        Find.WindowStack.Add(dialog);
    }

    [DebugAction("Save Maps", "Load blueprint")]
    public static void LoadBlueprint()
    {
        var unused = LoadedModManager.RunningMods
            .FirstOrDefault(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly()))?.Name;
        var path = BlueprintUtility.GetConfigPath();
        var directoryInfo = new DirectoryInfo(path);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        var list = new List<DebugMenuOption>();
        using (var enumerator = directoryInfo.GetFiles().AsEnumerable().GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var name = enumerator.Current?.Name;
                list.Add(new DebugMenuOption(name, 0, delegate
                {
                    path = $"{path}/{name}";
                    var map = Find.CurrentMap;
                    SettlementGeneration.DoSettlementGeneration(map, path, null, Faction.OfPlayer, false);
                }));
            }
        }

        if (list.Any())
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }

    [DebugAction("Save Maps", "Load blueprint (override)")]
    public static void LoadBlueprintDestroyEverything()
    {
        var unused = LoadedModManager.RunningMods
            .FirstOrDefault(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly()))
            ?.Name;
        var path = BlueprintUtility.GetConfigPath();
        var directoryInfo = new DirectoryInfo(path);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        var list = new List<DebugMenuOption>();
        using (var enumerator = directoryInfo.GetFiles().AsEnumerable().GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var name = enumerator.Current?.Name;
                list.Add(new DebugMenuOption(name, 0, delegate
                {
                    path = $"{path}/{name}";
                    var map = Find.CurrentMap;
                    SettlementGeneration.DoSettlementGeneration(map, path, null, Faction.OfPlayer, false, true);
                }));
            }
        }

        if (list.Any())
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }

    [DebugAction("Save Maps", "Load blueprint (override, except colonists)")]
    public static void LoadBlueprintDestroyEverythingExceptColonists()
    {
        var unused = LoadedModManager.RunningMods
            .FirstOrDefault(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly()))?.Name;
        var path = BlueprintUtility.GetConfigPath();
        var directoryInfo = new DirectoryInfo(path);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        var list = new List<DebugMenuOption>();
        using (var enumerator = directoryInfo.GetFiles().AsEnumerable().GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var name = enumerator.Current?.Name;
                list.Add(new DebugMenuOption(name, 0, delegate
                {
                    path = $"{path}/{name}";
                    var map = Find.CurrentMap;
                    SettlementGeneration.DoSettlementGeneration(map, path, null, Faction.OfPlayer, false, true, true);
                }));
            }
        }

        if (list.Any())
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }
    }
}