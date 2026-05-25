# GitHub Copilot Instructions for Save Maps (Continued) Mod

## Mod Overview and Purpose

The "Save Maps (Continued)" mod for RimWorld enhances the game's flexibility by allowing players to save and load maps as blueprints across different saves. This functionality caters to players wishing to re-use or share maps without manually managing seeds or coordinates. It is ideal for those who enjoy certain map features or have intricately built bases they wish to preserve and utilize in multiple playthroughs.

## Key Features and Systems

- **Map Saving and Loading**: Save map blueprints at any time using the Dev menu. Blueprints are saved in the game's configuration files, unaffected by Steam Workshop changes.
- **Resource Generation Compatibility**: Supports dynamic mod-added ores on loaded maps, ensuring future-proof compatibility.
- **Map and Base Size Adjustment**: Automatically adjusts map sizes to prevent errors during loading.
- **Partial Base Saving**: Optionally save specific areas, like your home area, either with or without colonists.
- **Complete Item Preservation**: All items and resources on a map are preserved within blueprints.
- **Colonist Integration**: Optionally save colonists alongside maps for continuity.

## Coding Patterns and Conventions

- **Class Structure**: Use public static classes for methods not requiring instance-specific data. Instance classes are used for components needing map or world context.
- **Method Visibility**: Methods within classes are mostly public, reflecting interactions expected across the mod's systems.
- **Naming Conventions**: Stick to PascalCase for class and method names, following C# standards. Parameters and local variables use camelCase.
- **Extensibility**: Utilize inheritance and interface implementation (such as `DefModExtension`) for modular code extending game definitions.

## XML Integration

- **Def Mod Extensions**: Extend vanilla definitions using XML to incorporate new functionalities, ensuring mod compatibility and easy data-driven adjustments.
- **Mod-Specific Definitions**: Create mod-specific definitions (e.g., `LocationDef`) to handle uniquely customized gameplay elements within XML configurations.

## Harmony Patching

- **Patching Approach**: Use Harmony to alter or extend base game methods safely, enabling integration without direct modification. Project centralizes Harmony patches within `HarmonyContainer.cs`.
- **Avoiding Conflicts**: Target specific methods and use descriptive patch classes to ensure clarity and minimal conflict with other mods.
- **Performance Consideration**: Apply patches judiciously to minimize overhead and maintain game performance.

## Suggestions for Copilot

- **Autocompletion for Class and Method Names**: Suggest class names based on file names and purpose descriptions, such as `CaravanArrivalAction` related logic.
- **Method Stubs**: When introducing new methods in classes like `Dialog_SaveEverything`, use existing method patterns for consistent dialog behavior.
- **XML Skeleton Generation**: Generate XML definition files using current conventions, referencing `DefModExtension` and similar structures.
- **Harmony Patch Templates**: Provide template code for adding new Harmony patches with common tasks and parameters in mind.

By adhering to these guidelines, contributors can more effectively work with the mod's architecture, extend its functionality, and integrate new features seamlessly.


This copilot-instructions.md file provides necessary information to guide contributors when working on the "Save Maps (Continued)" mod, ensuring consistent development practices and promoting ease of contribution.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
