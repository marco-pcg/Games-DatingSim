# DatingSim

An IEEE visual novel built by WolfByte with Unity. The project contains branching character dialogue, gender/name selection, a diary, affection state, and word-search, wire, rocket, and jigsaw minigame scenes.

## Open the project

1. Install **Unity 2021.3.23f1** through Unity Hub. This version is recorded in `ProjectSettings/ProjectVersion.txt`; use it before considering an editor upgrade.
2. Clone this repository and add its root directory in Unity Hub. The root contains `Assets`, `Packages`, and `ProjectSettings`.
3. Open the project and let Unity import the assets and restore the versions in `Packages/manifest.json` and `Packages/packages-lock.json`. The project uses URP 12.1.11, TextMesh Pro 3.0.6, and Unity Test Framework 1.1.31.
4. Open `Assets/Scenes/MainMenu.unity` and press **Play**. Inspect the Console for errors before building.

Unity generates `Library` and IDE project files locally; they are intentionally ignored by Git. Keep Unity `.meta` files when adding or moving assets. A standalone .NET build is not a substitute for importing and compiling this project in Unity.

## Gameplay and current scope

The scene-routing, save, and dialogue repair PRs establish the following flow. Those fixes must be merged for the documented behavior to be available.

| Entry or day | Available flow |
| --- | --- |
| **INICIAR** | `MainMenu` → `GenderSelection` → enter a name → `dia1` |
| Day 1 | PC (`dia1pc`) or Solda (`dia1solda`); terminal dialogue goes to `dia5` |
| Day 5 | Mesa (`dia5mesa`) or Flip (`dia5flip`); terminal dialogue returns to `dia5` selection |
| **CONTINUAR** | Restores saved name, gender, affection, and the most recently entered story scene; starts setup if no usable character save exists |

Day 1 Flip/Mesa and day 5 Solda/PC have no implemented scenes and are disabled by the routing fix. Days 2–4 and a final ending are not implemented. `dia5mesa` currently shares the PC dialogue asset used by day 1; it does not have a separate completed narrative. Dialogue completion is configured on each scene's `DialogueManager`; an individual dialogue asset can override its destination with `nextScene`.

Saves use `Application.persistentDataPath/game_save_slot`. They resume at the beginning of the saved scene, not at a specific dialogue line or option. Entering a story scene creates a checkpoint. Selecting **INICIAR** resets character state in memory; confirming the new name writes the new character save. Appearance is identified by saved gender; scene-local dialogue boxes and sprites must be configured in the Inspector rather than stored as runtime object references. The supplied gender-selection scene currently has no assigned gender sprites or dialogue boxes.

## Build a standalone player

1. Open **File → Build Settings** and choose **PC, Mac & Linux Standalone**. Install the corresponding build-support module in Unity Hub if needed, then switch platform.
2. Keep `MainMenu` first in **Scenes In Build**. The enabled scenes tracked in `ProjectSettings/EditorBuildSettings.asset` are:

   `MainMenu`, `GenderSelection`, `WordSearch`, `dia1`, `dia1pc`, `dia1solda`, `dia5`, `dia5flip`, and `dia5mesa`.

3. Choose **Build** or **Build And Run**, and output to an ignored `Builds/` directory.
4. Run the built player from its menu. Check character setup, every enabled location, terminal dialogue, application restart, and **CONTINUAR**. Editor play alone does not verify a standalone build.

`MiniGameRocket`, `MiniGameWires`, `Puzzle`, `WordSearchPlay`, `Demo`, `DiarySample`, `SampleScene`, and the thermometer prototype are available as scene assets but are not in the current build list. Open them individually for development; add them to Build Settings and implement an explicit entry/exit route before linking them from the main game. `WordSearch` is included in the build but has no complete documented narrative integration.

## Project layout

| Path | Contents |
| --- | --- |
| `Assets/Scenes` | Menu, character setup, story scenes, minigames, and examples |
| `Assets/Scripts/Core` | Shared state, saving, routing, diary, and UI helpers |
| `Assets/Scripts/dialogos` | ScriptableObject dialogue runner and option handling |
| `Assets/Scripts/dialscriptables` | Dialogue graphs and character profiles |
| `Assets/Scripts/NewCore` | Scenery selection, diary animation, and alternate text rendering |
| `Assets/Arts` | Art, minigame content, and related scripts |
| `Assets/Dialogos` | JSON dialogue material and character assets |
| `Assets/_TESTING` | Manual debug helpers; these are not automated regression tests |

## Verification and contribution

Use feature branches and PRs for changes. Preserve existing asset GUIDs and keep scene changes scoped so Unity YAML remains reviewable. Do not upgrade Unity or packages as part of an unrelated fix.

The automated-verification PR adds `TESTING.md`, focused EditMode/PlayMode regression tests, a Python scene validator, and GitHub Actions. Merge the routing, save, and dialogue fixes before that suite. Unity CI requires owner-configured license/account secrets; without them, only static checks run. Follow `TESTING.md` after it is merged. Until the Unity suites and standalone smoke check run, source review and static validation do not establish runtime correctness.

This project is distributed under the [MIT license](LICENSE). Bundled third-party assets include their own notices where supplied.
