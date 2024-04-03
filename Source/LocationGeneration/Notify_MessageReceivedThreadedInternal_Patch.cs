using HarmonyLib;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(Log), nameof(Log.Notify_MessageReceivedThreadedInternal))]
internal static class Notify_MessageReceivedThreadedInternal_Patch
{
    public static bool Prefix()
    {
        return false;
    }
}