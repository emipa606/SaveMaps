using RimWorld;
using Verse;

namespace LocationGeneration;

public class LocationDef : Def
{
    public IntVec3 additionalCenterCellOffset;

    public bool destroyEverythingOnTheMapBeforeGeneration;

    public bool disableCenterCellOffset;
    public FactionDef factionBase;

    public FactionDef factionDefForNPCsAndTurrets;

    public string filePreset;

    public string folderWithPresets;

    public bool moveThingsToShelves;

    public FloatRange? percentOfDamagedFurnitures;

    public FloatRange? percentOfDamagedWalls;

    public FloatRange? percentOfDestroyedWalls;
}