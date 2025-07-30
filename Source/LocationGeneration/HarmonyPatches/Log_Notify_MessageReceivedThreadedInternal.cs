using HarmonyLib;
using Verse;

namespace LocationGeneration;

[HarmonyPatch(typeof(Log), nameof(Log.Notify_MessageReceivedThreadedInternal))]
internal static class Log_Notify_MessageReceivedThreadedInternal
{
    public static bool Prefix()
    {
        return false;
    }
}