using System.Reflection;
using HarmonyLib;
using Verse;

namespace LocationGeneration;

[StaticConstructorOnStartup]
internal static class HarmonyContainer
{
    static HarmonyContainer()
    {
        new Harmony("LocationGeneration.HarmonyPatches").PatchAll(Assembly.GetExecutingAssembly());
    }
}