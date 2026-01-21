# Dungeon Generation
A modular dungeon generation system built using C# and Unity that assembles pre-built module prefabs and connects them procedurally. Inspired by titles such as *Lethal Company* and *R.E.P.O.*, this tool focuses on generation using constraints, weighted randomness, and backtracking to ensure valid dungeon layouts.

https://github.com/user-attachments/assets/5ef808e9-4a24-45ac-8174-13953274371c

## System Overview
This project was design to study the dungeon generation systems in titles such as *Lethal Company* and *R.E.P.O.*, which feature consistent, hand-crafted rooms reused across playthroughs.

A core development goal was to build a flexible, configurable system suitable for future projects.

The system is organized into three main layers:
1. **Themes** - High-level dungeon templates that define visual style, atmosphere, and generation rules
2. **Module Categories** - Logical groupings of modules that share behavior (e.g. Hallways, Rooms, Spawn)
3. **Module Assets** - Individual prefabs with associated spawning rules such as spawn rate, uniqueness, etc.

Each module prefab must contain have a `DungeonModule` MonoBehaviour script at its root which defines the module bounds and its entrance/exit connection points.

The `DungeonGenerator` MonoBehaviour script drives the overall generation process, using a selected theme and user-defined constraints to assemble a comprehensive dungeon layout.

### User-Defined Constraints
| Constraint | Description |
| ---------- | ----------- |
| Dungeon Size | Defines the maximum and minimum number of modules per dungeon |
| Categorical Beahvior | Groups modules into categories with shared spawning rules |
| Spawn Limits | Defines the maximum and minimum number of modules per category |
| Spawn Rates | Weighted probabilities for themes, categories, and individual modules |
| Required Modules | Categories that must appear in every dungeon (e.g. spawn rooms) |
| Unique Modules | Modules that can appear only once per dungeon (e.g. exit room) |
| Optional Looping | Enables circular connections for non-linear level design |

<details>
  <summary><b>Configuration Examples</b></summary>
  <img width="824" height="426" alt="image" src="https://github.com/user-attachments/assets/76b8f034-9013-48f5-84dc-bf40fabafe92" />
  <img width="826" height="735" alt="image" src="https://github.com/user-attachments/assets/7a1811ac-176a-4987-9e56-e973ebfe6605" />
  <img width="549" height="386" alt="image" src="https://github.com/user-attachments/assets/d27be10a-c3a5-4005-bcec-ee654fb0886b" />
</details>

## The Algorithm
The generation process is handled by the `DungeonGenerator` MonoBehaviour script and proceeds as follows:
### 1. Initialization
- Builds an alias probability table containing all available themes for weighted random selection
- Samples a theme from this table
- For the chosen theme, builds alias probability tables for:
    - Module Categories
    - Module Assets (within each category)
- Initializes internal structures such as the target module count, a stack for module placement history, and more
### 2. Generation Loop
The generator then enters a loop that continues until the target module count is reached or a generation error occurs (which then triggers an optional failure callback).
#### Step A. Module Selection
- If the number of remaining open connections equals the number of required modules left to instantiate, a required module is selected
- Otherwise, a module is chosen randomly using weighted alias sampling
#### Step B. Placement Attempt
- If this is **not** the first module:
    - Find an open connection from an already-placed module
    - If no open connections exist, attempt backtracking:
        - Pop the most recently placed module from the history stack
        - Reopen its connections
        - Retry placement
    - If an open connection is found:
        - Align the active module's entrance with the selected open connection
        - Check for intersections with existing modules (via bounds checking)
        - If an intersection is detected, discard and try another module
        - If there is no intersection detected, place the module and mark the used connection as closed
#### Step C. Loop Resolution (Optional)
- If the `Enable Loops` option is active, the generator may attempt to create secondary connections between the active module and other open connections in the dungeon, allowing circular pathways
### 3. Completion
- Once the target module count is met, generation stops
- A success callback is triggered (if configured)
- The resulting dungeon is fully ocnnected and respects all constraints

## License
This project is licensed under the MIT License. For more information, see the LICENSE file.
