# GitHub Copilot Instructions for Save Maps (Continued) Mod

## Mod Overview and Purpose
Save Maps (Continued) is a mod that allows players to save and load map blueprints within RimWorld. The mod provides various options for saving different aspects of the map, including structures, items, and colonists. This functionality enables players to share maps, reuse their creations across different saves, and avoid losing valuable map blueprints due to Workshop updates.

## Key Features and Systems
### Save Options
1. **Save Everything**: Saves the entire map, including items and colonists.
2. **Save Everything w/o Colonists**: Saves the entire map, excluding colonists.
3. **Save in Home Area with Colonists**: Saves structures within the home area, including colonists.
4. **Save in Home Area w/o Colonists**: Saves structures within the home area, excluding colonists.

### Load Options
1. **Load Blueprint**: Loads a saved blueprint and carries over items and colonists.
2. **Load Blueprint (override)**: Loads a saved blueprint and does not carry over anything from the current map.
3. **Load Blueprint (override, except colonists)**: Loads a saved blueprint and carries over only colonists.

### Additional Features
- Automatically adjusts map size during load.
- Saves map blueprints in the game's configuration files location, ensuring they are safe from Workshop updates.
- Loads maps with mod-added ores.

## Coding Patterns and Conventions
### C# Code Structure
- **Namespace**: All classes are part of the `Mlie.SaveMaps` namespace.
- **Directory Structure**:
  - **LocationGeneration**: Contains classes for handling map generation and blueprint actions.
  - **HarmonyPatches**: Contains Harmony patches for modifying existing RimWorld behavior.

### Class and Method Naming
- **Classes**: PascalCase (e.g., `Actions`, `CaravanArrivalAction_VisitSite_Arrived`).
- **Methods**: PascalCase, descriptive (e.g., `CreateBlueprint`, `LoadBlueprint`).

### Code Style
- Use of `async` and `await` for asynchronous operations.
- Proper exception handling.
- Logging for debugging and error reporting.

## XML Integration
### About.xml
- Contains metadata about the mod, including the author, display name, and package ID.
- Example:
  xml
  <About>
    <ModIdentifier>Mlie.SaveMaps</ModIdentifier>
    <Name>Save Maps (Continued)</Name>
    <Author>Jacob, Taranchuk</Author>
    <Description>
      Update of Jacob, Taranchuks mod. Allows players to save and load map blueprints.
    </Description>
    <Version>1.3</Version>
  </About>
  

## Harmony Patching
### Example Harmony Patches
#### CaravanArrivalAction_VisitSite_Arrived.cs
csharp
[HarmonyPatch(typeof(CaravanArrivalAction_VisitSite_Arrived), "Arrived")]
public static class CaravanArrivalAction_VisitSite_Arrived_Patch
{
    [Prefix]
    public static bool Prefix(Caravan caravan, Map map, CaravanArrivalAction_VisitSite_Arrived __instance)
    {
        // Custom logic before the original method runs
        return true;
    }

    [Postfix]
    public static void Postfix(Caravan caravan, Map map, CaravanArrivalAction_VisitSite_Arrived __instance)
    {
        // Custom logic after the original method runs
    }
}


#### Log_Notify_MessageReceivedThreadedInternal.cs
csharp
[HarmonyPatch(typeof(Log), "Notify_MessageReceivedThreadedInternal")]
public static class Log_Notify_MessageReceivedThreadedInternal_Patch
{
    [Prefix]
    public static bool Prefix(MessageTypeDef type, string text)
    {
        // Custom logic before the original method runs
        return true;
    }
}


## Suggestions for Copilot
1. **Code Generation**: Generate methods for saving and loading different aspects of the map.
2. **Harmony Patches**: Suggest Harmony patches for modifying existing RimWorld behavior.
3. **XML Generation**: Generate XML metadata files for the mod.
4. **Error Handling**: Suggest best practices for exception handling and logging.
5. **Code Review**: Review existing code for potential improvements and optimizations.

## Contribution Guidelines
- **Report Bugs**: Use the Discord channel for error reporting.
- **Solve Problems**: Post solutions to problems in the GitHub repository.
- **Sort Mods**: Use RimSort to sort your mods for better compatibility.

By following these guidelines, contributors can effectively utilize GitHub Copilot to enhance the Save Maps (Continued) mod and improve the overall player experience.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

