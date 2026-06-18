# Army Commander

Army Commander is a singleplayer mod for Mount & Blade II: Bannerlord. It expands kingdom army management, adds a new army overlay, lets the player choose which army will be managed, and adds persistent orders for AI-led armies.

## What The Mod Does

- Shows a custom overlay with all armies in the player's kingdom.
- Displays per-army indicators: parties, troops, food, influence, cohesion, and the cost to recover cohesion.
- Lets the player select an army in the overlay and open the army management screen with that army as the target.
- Lets the kingdom ruler, or an authorized vassal, command armies led by other lords.
- Lets mercenaries form/lead armies when the `Mercenary Army Leaders` policy is active or when the ruler grants permission through dialogue.
- Adds dialogues to request mercenary leadership permission and vassal army command permission.
- Redirects AI-led armies to defend or besiege settlements chosen by the player.
- Persists army orders in saves, including target, gathering point, and AI autonomy flags.
- Controls AI deviations for combat, allied support, resupply, and fleeing according to the saved orders.
- Protects armies with orders from automatic dispersion due to inactivity/objective completion and tries to recover cohesion when possible.

## Technology And Dependencies

- C# class library project targeting .NET Framework 4.7.2.
- Bannerlord mod registered through `SubModule.xml`.
- Runtime patching with Harmony.
- ViewModel and prefab injection with Bannerlord.UIExtenderEx.
- Mod dependencies declared in the manifest: `Bannerlord.Harmony`, `Bannerlord.ButterLib`, and `Bannerlord.UIExtenderEx`.

The game path is configured in `ArmyCommander.csproj` through the `BannerlordDir` property. The build copies the DLL to:

`$(BannerlordDir)\Modules\ArmyCommander\bin\Win64_Shipping_Client\`

After the build, the `DeployModFiles` target mirrors `GUI\` with `robocopy /MIR` and copies `SubModule.xml` to the Bannerlord module folder.

## Project Map

- `MySubModule.cs`: mod entry point. Applies Harmony, registers UIExtenderEx, resets stores/contexts, and registers campaign behaviors.
- `SubModule.xml`: module manifest, dependencies, mod version, and submodule class.
- `ArmyCommander.csproj`: Bannerlord, Harmony, UIExtenderEx references, source list, and deploy rule.
- `ACBehaviors/`: campaign behaviors for order persistence and permission dialogues.
- `ACBehaviors/Context/`: transient caches used by AI commands, such as last visited settlement and resupply state.
- `HarmonyPatches/`: patches that change army rules, overlay, management screen, AI, disbanding, chat log, and policies.
- `UIExtension/`: mixins, contexts, ViewModels, and prefab patches for the overlay and army management.
- `GUI/`: XML files injected or replaced through UIExtenderEx.
- `Helpers/`: helper calculations, availability checks, permissions, AI commands, and tooltips.
- `Store/`: static state used by patches, including persistable orders and granted permissions.
- `Actions/`: helper actions such as influence and item transfers.
- `CalculationModels/`: independent calculation utilities.
- `WatchAndMirror-GUI.ps1`: helper script for mirroring changes in the `GUI` folder.

## Main Flow

1. `MySubModule.OnSubModuleLoad` applies all Harmony patches and enables UIExtenderEx.
2. `MySubModule.OnGameStart` validates `Campaign`, resets `ArmyCommandsContext`, `ArmyCommandsBehaviorStore`, and `ACPermissionsStore`, and registers the persistence/dialogue behaviors.
3. The vanilla army overlay is replaced by `GUI/ArmyOverlayWindow.xml`, which keeps the original overlay as a placeholder and adds the custom list.
4. `ArmyMenuOverlayVMMixin` creates rows for the kingdom's armies, calculates indicators, updates totals, and maintains selection.
5. When a row is clicked, `ACArmyOverlayUIContext.SelectedArmy` points to the selected army.
6. When army management is opened, `ArmyManagementVMPatches` rebuilds the screen to use the selected army leader party as `currentMainParty`.
7. `ArmyManagementVMMixIn` injects order controls: target, gathering point, behavior, combat, allied support, resupply, fleeing, and order removal.
8. On confirmation, `ExecuteDonePrefix` creates/edits/disbands armies, applies costs, saves orders in `ArmyCommandsBehaviorStore`, and triggers AI recalculation when needed.
9. `SetPartyAiActionPatch`, `DefaultMobilePartyAIModelPatch`, `AiPartyThinkBehaviorPatch`, and `ACAIBehaviorHelpers` make the AI obey saved orders and handle resupply, fleeing, combat, and sieges.

## Army Orders

Persisted orders are stored in `ArmyCommandsBehaviorStore.army_commands`. Each entry stores:

- behavior type (`Defender` or `Besieger`);
- target settlement;
- gathering settlement while the army waits for members;
- whether it can engage enemies;
- whether it can help allies when general combat is blocked;
- whether it can resupply;
- whether it can flee from danger.

`ACArmyCommanderBehavior` saves these orders as XML inside the save using stable hero and settlement ids. On load, it resolves the army leader, target, and gathering point, discarding entries that no longer exist.

## Permissions

- Mercenary: can ask the ruler for permission to form/lead armies. Requires clan tier 3 and relation 25.
- Vassal: can ask the ruler for permission to command kingdom armies. Requires clan tier 4 and relation 40.
- Kingdom ruler: always passes `HasPlayerPermissionForArmyCommand`.
- `Mercenary Army Leaders` policy: also enables mercenary army leadership when active.

Granted permissions are saved in `ACPermissionsStore` as kingdom ids and are cleared when the mercenary contract ends or when the player's clan leaves the kingdom that granted vassal permission.

## Debug And Tests

The mod writes the main log to:

`%LOCALAPPDATA%\ArmyCommander\ArmyCommander_Debug.log`

`ACArmyCommanderBehavior` also has its own log at:

`%LOCALAPPDATA%\ArmyCommander\ArmyCommander_Behavior.log`

Manual regression procedures are kept in:

`docs/ArmyCommander_InGame_Test_Procedures.md`

## Technical Documentation

See [docs/ARQUITETURA.md](docs/ARQUITETURA.md) for a more detailed map of patches, data flows, and maintenance points.
