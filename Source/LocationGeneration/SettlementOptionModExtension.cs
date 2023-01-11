using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LocationGeneration;

public class SettlementOptionModExtension : DefModExtension
{
    public List<string> biomeDefnames;

    public IntRange distanceToPlayerColony;

    public IntRange numberOfSettlers;

    public List<PawnGenOption> pawnsToGenerate = new List<PawnGenOption>();

    public bool removeVanillaGeneratedPawns;

    public Dictionary<PawnKindDef, int> settlers;
}