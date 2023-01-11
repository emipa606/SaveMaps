using Verse;

namespace LocationGeneration;

public class Dialog_SaveEverything : Dialog_Rename
{
    private readonly bool includePawns;

    private string name;

    public Dialog_SaveEverything(string name, bool includePawns = true)
    {
        this.name = name;
        this.includePawns = includePawns;
    }

    protected override void SetName(string name)
    {
        this.name = GenText.SanitizeFilename(name);
        var map = Find.CurrentMap;
        var path = BlueprintUtility.GetConfigPath(this.name);
        BlueprintUtility.SaveEverything(path, map, includePawns);
    }
}