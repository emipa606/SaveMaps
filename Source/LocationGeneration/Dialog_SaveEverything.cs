using RimWorld;
using UnityEngine;
using Verse;

namespace LocationGeneration;

public class Dialog_SaveEverything : Window
{
    private readonly bool includePawns;
    private string curName;

    private bool focusedRenameField;

    private string name;
    private int startAcceptingInputAtFrame;

    public Dialog_SaveEverything(string name, bool includePawns = true)
    {
        this.name = name;
        this.includePawns = includePawns;
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        closeOnClickedOutside = true;
    }

    private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;

    private static int MaxNameLength => 28;
    public override Vector2 InitialSize => new(280f, 175f);

    public void WasOpenedByHotkey()
    {
        startAcceptingInputAtFrame = Time.frameCount + 1;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var returnPressed = false;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            returnPressed = true;
            Event.current.Use();
        }

        GUI.SetNextControlName("RenameField");
        var text = Widgets.TextField(new Rect(0f, 15f, inRect.width, 35f), curName);
        switch (AcceptsInput)
        {
            case true when text.Length < MaxNameLength:
                curName = text;
                break;
            case false:
                ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectAll();
                break;
        }

        if (!focusedRenameField)
        {
            UI.FocusControl("RenameField", this);
            focusedRenameField = true;
        }

        if (!Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 15f, inRect.width - 15f - 15f, 35f), "OK") &&
            !returnPressed)
        {
            return;
        }

        var acceptanceReport = nameIsValid(curName);
        if (!acceptanceReport.Accepted)
        {
            if (acceptanceReport.Reason.NullOrEmpty())
            {
                Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
            return;
        }

        setName(curName);
        Find.WindowStack.TryRemove(this);
    }

    private void setName(string nameToCheck)
    {
        name = GenText.SanitizeFilename(nameToCheck);
        var map = Find.CurrentMap;
        var path = BlueprintUtility.GetConfigPath(name);
        BlueprintUtility.SaveEverything(path, map, includePawns);
    }

    private static AcceptanceReport nameIsValid(string nameToCheck)
    {
        return nameToCheck.Length != 0;
    }
}