using RimWorld.QuestGen;
using Verse;

namespace LocationGeneration;

public class QuestNode_SetMapSize : QuestNode
{
    private SlateRef<IntVec3> mapSize;
    private SlateRef<int> tile;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var worldComp = Find.World.GetComponent<WorldComponentGeneration>();
        worldComp.tileSizes[tile.GetValue(slate)] = mapSize.GetValue(slate);
    }
}