using Mlie;
using UnityEngine;
using Verse;

namespace LocationGeneration;

public class LocationGenerationMod : Mod
{
    private static string currentVersion;

    public LocationGenerationMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<LocationGenerationSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public static LocationGenerationSettings Settings { get; private set; }

    public override string SettingsCategory()
    {
        return "Save Maps";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("SaMa.visit".Translate(), ref Settings.allowVisitingSettlements,
            "SaMa.visitTT".Translate());
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("SaMa.modVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }
}