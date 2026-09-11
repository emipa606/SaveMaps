# GitHub Copilot Instructions for "Save Maps (Continued)" Mod

## Mod Overview and Purpose

The "Save Maps (Continued)" mod is designed to enhance the player experience in RimWorld by allowing users to save and load map blueprints across different saves. The core function of the mod is to provide players with greater flexibility and reusability of their maps and bases. Whether you want to share a map with others, reuse a favorite base design, or ensure that your map is never lost due to subscription changes, this mod is here to solve these issues.

## Key Features and Systems

- **Blueprint System**: Save entire maps, including all player-placed structures, items, and colonists, as blueprint files.
- **Dev Menu Integration**: Access the mod's features via a new category in RimWorld's Dev Mode.
- **Future-Proofing**: Loaded maps can dynamically adapt to include content from other mods, ensuring compatibility with future updates and expansions.
- **Customizable Saving Options**: Save maps with or without colonists, save selected home areas, or save everything on the map.
- **Map Resizing**: Automatically adjust the loaded map size to prevent issues with map dimension mismatches.
- **Persistency**: Blueprint files are saved in a location that won't be affected by Steam Workshop changes.

## Coding Patterns and Conventions

- **C# Structure**: The mod uses well-defined C# classes such as `GenStep_LocationGeneration` for map generation steps.
- **Separation of Concerns**: Different concerns like map generation and loading are isolated into separate classes and files for maintainability.
- **Descriptive Naming**: Class and method names are descriptive and follow CamelCase convention, e.g., `SaveEverything`, `LoadBlueprint`.

## XML Integration

- The mod includes an `About.xml` file, which contains metadata about the mod such as its name, author, and package ID. This file ensures the mod is correctly recognized by RimWorld.

## Harmony Patching

The mod leverages the Harmony library for method patching to extend or modify RimWorld's vanilla functionality. Key patches include:

- **Map Generation**: Hooks into the map generation process to support blueprint-based map loading.
  - Files: `MapGenerator_GenerateMap.cs`
- **Caravan Arrival**: Adjustments to caravan behavior upon reaching a site with a saved map.
  - Files: `CaravanArrivalAction_VisitSite_Arrived.cs`
- **Logging**: Enhanced logging support for tracking messages during asynchronous operations.
  - Files: `Log_Notify_MessageReceivedThreadedInternal.cs`

## Suggestions for Copilot

To enhance the coding experience with GitHub Copilot while developing or extending this mod, consider the following suggestions:

1. **Autocomplete for Method Signatures**: Copilot can autocomplete commonly used method signatures for Harmony patches, especially those involving Prefix and Postfix methods.

2. **Pattern Recognition**: Utilize Copilot's ability to recognize existing coding patterns within the project to suggest new method implementations that align with current practices.

3. **Error Handling Snippets**: Ensure that Copilot suggests robust error handling snippets, especially when dealing with file operations and blueprint loading.

4. **XML Tag Suggestions**: While editing XML files, Copilot can assist in suggesting correct tag structures and attributes, which can streamline the process of updating mod metadata.

5. **Adaptive Refactoring**: Leverage Copilot’s refactoring suggestions to maintain and improve code quality as new features are introduced or existing ones are enhanced.

By adhering to these guidelines and leveraging Copilot's capabilities, you can ensure that the Save Maps mod remains robust, maintainable, and user-friendly for the RimWorld community.

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

