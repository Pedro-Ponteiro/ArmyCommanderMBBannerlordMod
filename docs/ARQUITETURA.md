# Army Commander Architecture

This file documents the current project state after the changes made after `fe9f54f6d9011db1971fdf7d207ff2d6705e10d0`. Its purpose is to help future maintenance: where each behavior starts, which classes talk to each other, and which points are most fragile because they depend on Bannerlord internals.

## Overview

Army Commander is built around four axes:

1. Change vanilla army rules with Harmony.
2. Replace and extend parts of the overlay and the `ArmyManagementVM` screen with UIExtenderEx.
3. Use static contexts/stores to connect the overlay, management screen, campaign behaviors, and AI patches.
4. Persist AI-led army orders and policy/dialogue permissions in saves.

In practice, the mod turns army management from a player-party-centered experience into an experience centered on the army selected in the overlay, with the ability to command third-party armies when the player has permission.

## Initialization

`MySubModule` is the entry point.

- `OnSubModuleLoad` applies `Harmony.PatchAll(Assembly.GetExecutingAssembly())`, creates `UIExtender.Create("ArmyCommander")`, registers the assembly, and enables UIExtender.
- `OnSubModuleUnloaded` disables and deregisters UIExtenderEx, removes Harmony patches by the `ArmyCommander` id, and clears references.
- `OnGameStart` validates that the game is `Campaign`, resets `ArmyCommandsContext`, `ArmyCommandsBehaviorStore`, and `ACPermissionsStore`, and registers:
  - `ACArmyCommanderBehavior`;
  - `ACMercenaryArmyLeadershipDialogueBehavior`;
  - `ACVassalArmyCommanderDialogueBehavior`.

The file also centralizes defensive logging. Logging failures do not crash the game.

## Build And Deploy

`ArmyCommander.csproj` defines:

- `TargetFrameworkVersion`: `v4.7.2`.
- `OutputType`: `Library`.
- `AssemblyName`: `ArmyCommander`.
- `BannerlordDir`: local absolute path to the Steam Bannerlord installation.
- `OutputPath`: `Modules\ArmyCommander\bin\Win64_Shipping_Client`.

The `DeployModFiles` target, executed after the build, uses `robocopy` to mirror `GUI\` into the installed module and copies `SubModule.xml`.

`WatchAndMirror-GUI.ps1` is a separate utility for mirroring changes from the `GUI` folder during iteration.

## Manifest

`SubModule.xml` declares the module as a singleplayer community module, version `v2.2.0`, with required dependencies:

- `Bannerlord.Harmony`
- `Bannerlord.ButterLib`
- `Bannerlord.UIExtenderEx`
- `Native`, `SandBoxCore`, `CustomBattle`, `Sandbox`

`StoryMode` and `NavalDLC` appear as optional dependencies. The loaded class is `ArmyCommander.MySubModule`.

## Shared State

The project uses static stores/contexts as glue between patches and ViewModels.

- `ACArmyOverlayUIContext`: active army overlay instance. Stores `SelectedArmy`, aggregate counters, page button control, and overlay expanded state.
- `ACArmyManagementUIContext`: active army management screen instance. Stores `currentMainParty`, `mainPartyHasArmy`, target, gathering point, army behavior, command flags, sent influence, and `movieIsLoaded`.
- `ACArmyLineUIContext`: per-overlay-row context. Loads leader party, counters, food, influence, cohesion, costs, and helper lists.
- `ArmyCommandsBehaviorStore.army_commands`: `Army -> command tuple` dictionary used to save and reapply player orders.
- `ArmyCommandsContext`: transient AI caches, including `ArmyLastVisitedSettlementCache` and `ArmyIsResupplyingDic`.
- `ACPermissionsStore`: stores kingdom ids that granted mercenary leadership permission or vassal command permission.
- `ACPolicyStore.MercenaryArmyLeadersPolicy`: static reference to the policy created in the `DefaultPolicies` patch.

UI contexts are rebuilt at runtime by the UI and events. Orders and permissions relevant to save/load are persisted by campaign behaviors.

## Campaign Behaviors

### ACArmyCommanderBehavior

`ACArmyCommanderBehavior` handles army order persistence and maintenance.

Events:

- `OnSettlementOwnerChangedEvent`: reviews orders when settlement ownership changes.
- `OnPeaceOfferResolvedEvent`: reviews orders when peace changes the validity of an enemy target.
- `PartyAttachedAnotherParty`: re-enables AI when a member joins a commanded army.
- `HourlyTickEvent`: workaround to re-enable AI for commanded armies that ended up in behavior `0`.

Persistence:

- Uses the `ArmyCommander.ArmyCommands.v1` key.
- Serializes XML with `leaderHeroId`, `armyType`, `targetSettlementId`, `gatherSettlementId`, and boolean flags.
- On load, finds the army by leader within `Clan.PlayerClan.Kingdom.Armies`.
- Discards commands without leader, target, or supported type.
- Supported types: `Besieger` and `Defender`.

Validation:

- `RefreshArmyCommandsStore` calls `ACAIBehaviorHelpers.ValidatePlayerCommandAndAskIfNeeded`.
- If a siege target stops being enemy-owned, or a defense target stops belonging to the player's kingdom, the mod asks whether the army should wait at a safe settlement or return to vanilla AI.

### ACMercenaryArmyLeadershipDialogueBehavior

Adds dialogue for the mercenary player to request permission from the ruler of the contracted kingdom.

- Requires active mercenary service.
- The conversation hero must be the leader of the player's kingdom.
- Does not appear if the permission already exists.
- Clicking requires minimum relation 25 and minimum clan tier 3.
- Saves permission in `ACPermissionsStore._acKingdomIdThatAllowedPlayerMercenaryArmyLeadership`.
- Clears permission when mercenary service ends.

### ACVassalArmyCommanderDialogueBehavior

Adds dialogue for the vassal player to request permission from the ruler to command kingdom armies.

- The conversation hero must be the leader of the player's kingdom.
- Does not appear for mercenaries.
- Does not appear if `ACHelpers.HasPlayerPermissionForArmyCommand()` is already true.
- Clicking requires minimum relation 40 and minimum clan tier 4.
- Saves permission in `ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand`.
- Clears permission when the player's clan leaves the kingdom that granted it.

## Army Overlay

The custom overlay is assembled by UIExtenderEx.

- `UIExtension/UIPatches/ACArmyOverlayArmyListPatch.cs` replaces the `Window` of the `ArmyOverlay` prefab.
- The patch loads the original XML from `SandBox/GUI/Prefabs/Map/ArmyOverlay.xml`, extracts `ArmyOverlayWidget`, and injects it into the `ArmyCommanderOriginalArmyOverlayWidgetPlaceholder`.
- `GUI/ArmyOverlayWindow.xml` adds the custom `ACOverlayWidget` and keeps the original overlay right after it.
- `HarmonyPatches/ArmyOverlayWidgetPatch.cs` adjusts positioning/pagination for the custom overlay and propagates the expanded state to `ACArmyOverlayUIContext.IsExtended`.
- `HarmonyPatches/ChatLogWidgetPatch.cs` registers the `ChatLogWidget` and adjusts `MarginBottom` dynamically to make room for the overlay.

`ArmyMenuOverlayVMMixin` is the main mixin for this UI:

- Creates `ACArmyOverlayUIContext`.
- Maintains `ArmyOverlayArmiesList`.
- Rebuilds rows with `RenewLeftArmyOverlay`.
- Updates top totals in `UpdateTopWidgets`.
- Listens to `CampaignEvents.HourlyTickEvent` to update the overlay.
- Reacts to armies being created/disbanded through callbacks called by patches in `ArmyPatch.cs`.

Each row is made of:

- `SelectableArmyLineVM`: clickable item, selection/hover, and `OnArmyOverlaySetDirty` execution.
- `SelectableArmyLeaderVisualVM`: portrait, banner, tooltip, encyclopedia link, and map camera behavior.
- `SelectableArmyPropertiesRow`: groups metrics into rows.
- `SelectableArmyItemPropertyVM`: individual metric with sprite, value, delta, warning, and tooltip.
- `ACArmyLineWidgetBuilders`: builder for parties, troops, food, influence, cohesion, and cohesion-cost widgets.

## Army Selection

The active UI army is stored in `ACArmyOverlayUIContext.SelectedArmy`.

When the user clicks a row:

1. `SelectableArmyLineVM.ExecuteClickFunction` sets `SelectedArmy = LeaderParty.Army`.
2. The vanilla overlay is marked dirty through `CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty()`.
3. `ArmyMenuOverlayVM_get_ArmyToUse_Patch` makes the `ArmyToUse` getter return the selected army, or fall back to the main party army, or to the kingdom's first army.
4. `ArmyMenuOverlayVM_GetIsPlayerArmyLeader_Patch` always returns `true`, unlocking UI paths that normally depend on the player being the leader.
5. `ArmyMenuOverlayVM_ExecuteOpenArmyManagement_Patch` calls `OpenArmyManagement` directly.

`MapScreen_OnRefreshState_Patch` recreates or removes the army overlay according to `ACHelpers.ShouldShowArmyOverlayForPlayer()`. `MapBarVM_GetIsGatherArmyVisible_Patch` hides the vanilla gather button when the custom overlay should be visible.

## Army Management Screen

`HarmonyPatches/ArmyManagementVMPatch.cs` is the most central piece of the project.

It creates reverse patches to call original `ArmyManagementVM` methods when needed:

- `OnRefresh`
- `OnAddToCart`
- `OnRemove`
- `OnFocus`

In the constructor postfix, the patch rebuilds the VM:

- Clears/recreates `PartyList`, `PartiesInCart`, `_partiesToRemove`, and other internal fields.
- Chooses `currentMainParty`:
  - if the player cannot command armies, uses the player's party;
  - if no army is selected, uses the player's party;
  - otherwise, uses the leader party of the selected army.
- Creates the main item (`_mainPartyItem`) and adds it to the cart.
- Populates the left list with parties from the same map/faction.
- If the main party already leads an army, moves existing members into the cart with zero cost.
- Also adds the main party to the left side when the player can command armies.
- Reorders lists and calls the original refresh.

Important flows:

- `PlayerHasArmySetterPrefix`: only lets `PlayerHasArmy` be true for an army led by the main party.
- `ManagementItemComparerComparePrefix` and `OrderPartiesInPlace`: keep the current leader at the top, followed by cart items, main party, leaders, eligible parties, and blocked members.
- `OnFirstPartyAdded`: when the first item is added, it becomes the main party/context for the army being created or edited.
- `OnArmyLeaderRemoved`: removing the leader clears selection and returns the screen to creation state.
- `CustomOnAddToCart` and `CustomOnRemove`: replace vanilla flows to support editing other leaders' armies.
- `ExecuteDonePrefix`: applies cohesion, creates a new army, adds members, saves/updates orders, recalculates AI, deducts influence, removes parties, and closes the screen.
- `CustomDisbandArmy`: disbands a player-led army by setting `Army = null`; for another leader, uses `DisbandArmyAction.ApplyByReleasedByPlayerAfterBattle`.
- `ExecuteResetPrefix` and `ExecuteCancelPrefix`: restore initial influence and clear temporary changes.
- `OnFinalizePostfix`: finalizes the mixin and removes `ACArmyManagementUIContext.Instance`.

## Custom Management Controls

`ArmyManagementVMMixIn` injects/exposes extra controls used by `GUI/ACArmyManagementWidgets.xml` and the right-panel wrapper.

Controlled state:

- army behavior (`Defender` or `Besieger`);
- target settlement;
- gathering settlement;
- `CanEngageEnemyParties`;
- `CanHelpAlliedParties`;
- `CanResupply`;
- `CanRunFromDanger`;
- influence sending;
- order removal.

Main rules:

- For a new army, uses the kingdom's possible capital as the initial target/gathering point and permissive defaults.
- For an existing army with saved orders, loads the order from `ArmyCommandsBehaviorStore`.
- For an existing army without saved orders, uses `ACAIBehaviorHelpers.GetDefaultAiCommands`.
- Choosing an allied settlement as target sets behavior to `Defender`.
- Choosing an enemy settlement as target sets behavior to `Besieger`.
- The gathering point is enabled when the army is waiting for members.
- `CanHelpAlliedParties` is only enabled when `CanEngageEnemyParties` is disabled.
- `ExecuteRemoveOrders` removes the `ArmyCommandsBehaviorStore` entry and returns the context to the AI default commands.
- `ExecuteSendInfluence` transfers 50 influence from the player to the selected army leader's clan.

`UIExtension/UIPatches/ACArmyManagementRightPanelDisbandButtonPatch.cs` replaces the right panel's `DisbandButton` with a wrapper that includes `Remove Orders` next to the original button.

`OpenArmyManagement_All_Patch` runs after openings from the map bar, map overlay, or kingdom screen, marks `movieIsLoaded = true`, and calls `UpdateWidgets`.

## AI-Led Army Orders

The command flow uses `ArmyCommandsBehaviorStore`.

Each saved order contains:

- `ArmyType`: `Besieger` or `Defender`;
- `TargetSettlement`: main target;
- `GatherSettlement`: place used while the army waits for members;
- `CanEngageEnemyParties`;
- `CanHelpAlliedParties`;
- `CanResupply`;
- `CanRunFromDanger`.

When `ExecuteDonePrefix` creates or updates an army not led by the main party, it compares the new commands with the AI default commands and previous commands. If there is a difference, it saves the order and calls `ACAIBehaviorHelpers.OnPlayerArmyCommandChanged` when the army is available.

`SetPartyAiActionPatch.cs` intercepts `SetPartyAiAction.ApplyInternal` and delegates the decision to `ACAIBehaviorHelpers.AiBehaviorRecalculated`.

`DefaultMobilePartyAIModelPatch.cs` cuts off AI initiative at the `GetBestInitiativeBehavior` level:

- blocks `EngageParty` when `CanEngageEnemyParties` is false;
- preserves allied support when `CanHelpAlliedParties` is true and the target party is fighting an ally;
- blocks fleeing behaviors when `CanRunFromDanger` is false.

`AiPartyThinkBehaviorPatch.cs` has two transpilers:

- treats mercenaries with the active policy as non-mercenaries for gathering logic;
- replaces `SiegeEvent.FinalizeSiegeEvent` with `FinalizeSiegeEventIfAllowed`, preventing a siege from ending when the player order says to continue and the situation allows it.

## AI Recalculation And Resupply

`ACAIBehaviorHelpers` centralizes order execution logic.

Main functions:

- `GetDefaultAiCommands`: captures the army's current vanilla state.
- `ValidatePlayerCommandAndAskIfNeeded`: validates target after peace/ownership changes and asks whether the army should wait or return to vanilla AI.
- `ApplyDefaultFallBackBehavior`: turns the order into passive defense of a safe settlement, with combat/support/resupply/fleeing disabled.
- `ReEnableAI`: unlocks decisions and schedules rethink.
- `NewArmyCommandApplied`: applies a new order for waiting, besieger, or defender states.
- `FindBestSettlementForResupplying`: chooses a nearby settlement for food/troops without repeating the last or penultimate visited settlement.
- `FindBestSettlementForWaiting`: chooses a safe town/castle for waiting.
- `AiBehaviorRecalculated`: decides whether a vanilla command should be replaced by the saved order.
- `ACShouldAttackerEndSiege`: prevents ending a siege when the order says to continue and food needs do not require ending it.
- `ACShouldArmyContinueOrStartResupply`: uses hysteresis to decide resupply.

`ArmyCommandsContext.ArmyLastVisitedSettlementCache` is updated by `MobileParty_LastVisitedSettlement_Setter_Patch` to avoid resupply loops. `ArmyCommandsContext.ArmyIsResupplyingDic` records whether the army is already in a resupply cycle and adjusts food/troop thresholds.

Current thresholds:

- Besieger outside a siege: food below 15 days to start, 20 to continue.
- Defender/non-besieger outside a siege: food below 10 days to start, 15 to continue.
- Troops ratio below 0.65 to start, 0.75 to continue.
- Besieger in a siege: food below 5 days; does not look for troops in this case.

## Eligibility And Creation Rules

`DefaultArmyManagementCalculationModelPatch.cs` changes party eligibility and army creation rules.

`CheckPartyEligibility` is fully replaced by a prefix:

- blocks null parties;
- blocks mercenaries without the policy when the selection does not yet have a `currentMainParty`;
- blocks busy parties, busy player, ruler when inappropriate, members of another army, and small parties;
- allows selecting existing army leaders when the player has command permission.

`CanLordCreateArmy` is also replaced:

- allows mercenaries only when the `Mercenary Army Leaders` policy is active;
- filters available parties;
- limits how much of the kingdom can be committed to armies with a 70% heuristic;
- requires minimum total strength 1000 to create an AI army.

`CanPlayerCreateArmy` receives a transpiler to replace the simple `Clan.IsUnderMercenaryService` check with `IsUnderMercenaryServiceAndNoPermission`, which considers dialogue permission and policy.

`CampaignUIHelper_GetCanManageCurrentArmyWithReason_Patch` allows or blocks access to army management based on:

- busy player;
- army command permission;
- mercenary service without permission/policy;
- player already being a member of another army.

## Mercenary Policy

`DefaultPoliciesPatch.cs` creates the `army_commander_mercenary_army_leaders` policy during `DefaultPolicies.InitializeAll`.

Declared impact:

- mercenaries can form and lead armies while serving the kingdom;
- the ruling clan pays 100 influence when a mercenary army is formed.

The 100 influence cost is applied in the `Army.Gather` postfix when the policy is active for the army leader clan.

`ACHelpers.HasPlayerPermissionForMercenaryArmyLeadership` returns true when:

- the player is under mercenary service in the current kingdom; and
- the kingdom saved in `ACPermissionsStore` is the current kingdom; or
- the `Mercenary Army Leaders` policy is active for the clan.

## Dispersion, Cohesion, And Army Events

`ArmyPatch.cs` covers gather/disperse events:

- `Army_DisperseInternal_Patch` removes saved orders from the dispersed army and updates the overlay.
- `Army_Gather_Patch` charges influence from the ruler for a mercenary army with the active policy and updates the overlay.
- `Army_SendLeaderPartyToReachablePointAroundPosition_ReversePatch` allows reusing the vanilla send-to-gathering-point behavior.

`DisbandArmyActionPatch.cs` protects armies with orders:

- prevents dispersion due to `Inactivity` and `ObjectiveFinished`;
- on `CohesionDepleted`, tries to spend the leader clan's influence to recover cohesion before dispersing.

## Other Relevant Patches

- `ArmyManagementItemVMPatch.cs`: adjusts distance to the selected leader party, recalculates time by speed, and replaces the name/strength of army leaders in the list.
- `CampaignUIHelperPatch.cs`: replaces `GetCanManageCurrentArmyWithReason`.
- `MapBarVMPatch.cs`: hides the vanilla gather army button when the custom overlay should appear.
- `MapScreenPatch.cs`: replaces `IMapStateHandler.OnRefreshState` to create/remove the army overlay.
- `MobilePartyPatch.cs`: records the penultimate settlement visited by commanded leaders, used by resupply.
- `ChatLogWidgetPatch.cs`: adjusts the chat log bottom margin according to overlay expanded/collapsed state and row count.
- `ACArmyManagementPatch.cs`: injects `GUI/ACArmyManagementWidgets.xml` into the management screen.
- `ACArmyManagementRightPanelDisbandButtonPatch.cs`: injects the `Remove Orders` + original disband button wrapper.
- `OpenArmyManagement_All_Patch.cs`: ensures custom widgets update after the screen opens.

## Helpers And Calculations

`ACHelpers` centralizes availability rules and metrics:

- safe `MBObjectBase` comparison;
- army availability for receiving orders;
- whether the player/party/army is busy;
- whether a settlement is in an acceptable condition;
- whether the overlay should appear;
- command permission and mercenary leadership permission;
- party/troop counts;
- distance in days;
- food, influence, cohesion, and costs;
- days until food runs out;
- possible kingdom capital;
- troop grouping by `FormationClass`.

`ACHintHelpers` builds tooltips for:

- kingdom totals at the top of the overlay;
- parties/troops/food/cohesion/influence for each army.

`ACCalculationModel.DistributeToSmallestKeepOriginalOrder` distributes an integer increment by raising the smallest values first and returning the original order.

`ACActions` contains helpers for sending items, transferring influence, subtracting/adding influence, and moving resources between parties/clans.

## Manual Tests

`docs/ArmyCommander_InGame_Test_Procedures.md` covers manual regression for:

- order persistence after save/load;
- `Mercenary Army Leaders` policy;
- army creation by ruler, vassal, and mercenary;
- mercenary permission dialogue;
- ending/rejoining a mercenary contract;
- negative cases and known risks.

This document should still be extended to specifically cover vassal dialogue and the new command flags (`CanEngageEnemyParties`, `CanHelpAlliedParties`, `CanResupply`, `CanRunFromDanger`).

## Observed Fragile Points

- Many patches access private fields/methods through `AccessTools`. Bannerlord version changes may break names such as `_partiesToRemove`, `_mainPartyItem`, `_armyOverlay`, `ApplyInternal`, `ArmyToUse`, `SendLeaderPartyToReachablePointAroundPosition`, and `GetInfluenceBudgetWhileCreatingArmy`.
- Reverse patches intentionally throw `NotImplementedException` if called without Harmony replacing the body. This is expected, but makes ordinary unit tests harder.
- Order persistence depends on hero and settlement ids. If the leader stops leading the army, if the player's kingdom is null, or if the target disappears, the command is discarded on load.
- `FindArmyByLeaderHeroId` assumes `Clan.PlayerClan.Kingdom` is available during restore.
- `OnSettlementOwnerChanged` accesses `oldOwner.Clan.Kingdom` in some paths; events with null `oldOwner`/`Clan` would be dangerous.
- `OpenArmyManagement_All_Patch.Postfix` assumes `ACArmyManagementUIContext.Instance` exists after opening.
- `FindBestSettlementForResupplying` can return null; resupply calls should tolerate this to avoid applying an order without a settlement.
- Important magic values: 50 influence sent, 100 cost for a mercenary army, 70% kingdom-party army limit, 1000 minimum strength, food/troop thresholds, and dialogue relation/tier requirements.
- `BannerlordDir` is hardcoded to a local installation. Other environments need to adjust the `.csproj` property.
- `ACPolicyStore.MercenaryArmyLeadersPolicy` depends on the `DefaultPolicies.InitializeAll` patch; code that queries it before then must tolerate null.
- `ACActions.SendItemQuantityOneToOne` calculates `amount_to_give`, but calls `SendItem(..., quantity)` inside the loop. This looks suspicious if the intent was to send only that item's quantity.
- The XML and mixin have several hardcoded English strings, and some strings still use partial ids/localization.

## Where To Change Common Things

- Change overlay visuals: `GUI/ArmyOverlayWindow.xml` and `GUI/Brushes/ArmyCommanderBrushes.xml`.
- Change row metrics: `ACArmyLineWidgetBuilders`, `ACArmyLineUIContext`, `ACHelpers`, and `ACHintHelpers`.
- Change management screen behavior: `HarmonyPatches/ArmyManagementVMPatch.cs`.
- Change management screen controls: `UIExtension/MixIns/ACArmyManagementVMMixIn.cs`, `GUI/ACArmyManagementWidgets.xml`, and `GUI/ArmyManagementRightPanelDisbandButtonWrapper.xml`.
- Change permission/eligibility rules: `DefaultArmyManagementCalculationModelPatch.cs`, `CampaignUIHelperPatch.cs`, `ACHelpers.cs`, and dialogue behaviors.
- Change AI-led army commands: `ACAIBehaviorHelpers.cs`, `SetPartyAiActionPatch.cs`, `DefaultMobilePartyAIModelPatch.cs`, `AiPartyThinkBehaviorPatch.cs`, and the `ExecuteDonePrefix` section in `ArmyManagementVMPatch.cs`.
- Change order persistence: `ACArmyCommanderBehavior.cs` and `ArmyCommandsBehaviorStore.cs`.
- Change mercenary/vassal permission: `ACMercenaryArmyLeadershipDialogueBehavior.cs`, `ACVassalArmyCommanderDialogueBehavior.cs`, `ACPermissionsStore.cs`, and `ACHelpers.cs`.
- Change custom policy: `DefaultPoliciesPatch.cs`, `AiPartyThinkBehaviorPatch.cs`, `DefaultArmyManagementCalculationModelPatch.cs`, and `ArmyPatch.cs`.
