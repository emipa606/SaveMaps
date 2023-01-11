using HarmonyLib;
using Verse;

namespace LocationGeneration;

[StaticConstructorOnStartup]
internal static class HarmonyContainer
{
    static HarmonyContainer()
    {
        var harmony = new Harmony("LocationGeneration.HarmonyPatches");
        harmony.PatchAll();
    }
}