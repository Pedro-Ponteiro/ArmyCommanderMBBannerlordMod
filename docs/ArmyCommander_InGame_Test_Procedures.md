# Army Commander - Simple In-Game Test Procedures

This document defines simple manual in-game regression tests for Army Commander.

The goal is to verify that army commands, mercenary army leadership permissions, and the **Mercenary Army Leaders Policy** behave correctly across normal gameplay, save/load cycles, and contract changes.

## Test Conventions

Use a clean test profile where possible.

For each test case, record:

- Game version
- Mod version
- Save name
- Player status: ruler, vassal, mercenary, or clan without kingdom
- Kingdom
- Existing kingdom army count before the test
- Result: Pass, Fail, or Blocked
- Notes, screenshots, or exception logs if something fails

Suggested save naming pattern:

```text
AC_TEST_<Scenario>_<Step>
Example: AC_TEST_Ruler_Commands_01
```

Suggested Cheat Menu Commands to make things easier:

- `campaign.add_hero_relation Caladog | 50`
- `campaign.add_renown_to_clan 10000`
- `campaign.add_influence 10000`


Whenever a test says **save and reload**, do the following:

1. Create a manual save.
2. alt+f4.
3. Load Game.
4. Reload the save.
5. Re-check the same condition before continuing.
6. Continue the test only if the reloaded state matches the pre-save state.

## Main Areas Covered

1. Army command persistence across sessions.
2. Mercenary army leader policy behavior.
3. Army creation eligibility for ruler, vassal, and mercenary player states.
4. Mercenary army leadership dialogue availability and permission persistence.
5. Contract end/rejoin behavior.
6. Negative cases where mercenary army leadership should be disabled.

---

# 1. Kingdom Ruler Save

## 1.1 Command Persistence Across Sessions

### 1.1.1 Player-Created Army Commands Persist After Reload

**Preconditions**

- Player is the ruler of a kingdom.
- Player can create armies.
- At least one eligible lord party is available.
- No conflicting active test command is already assigned.

**Steps**

1. Create a new army manually.
2. Assign at least one command to the army.
   - Example command types to test, if available:
     - Move to location
     - Besiege settlement
     - Defend settlement
     - Follow or gather
3. Let the game run until the army starts following the command.
4. Confirm that the army behavior matches the assigned command.
5. Save and reload.
6. After reload, inspect the army again.
7. Let the game run for a short period.
8. Confirm that the army continues following the assigned command.

**Expected Result**

- The army keeps its assigned command after reload.
- The army does not return to vanilla AI behavior immediately after reload.
- No duplicate command state is created.
- No exception or crash occurs.

---

### 1.1.2 AI-Created Army Commands Persist After Reload

**Preconditions**

- Player is the ruler of a kingdom.
- At least one AI lord can create an army.
- There are enough parties and influence for an AI army to form naturally.

**Steps**

1. Let the campaign run until an AI army is created.
2. Select the AI-created army.
3. Assign a command to the army.
4. Let the game run until the army starts following the command.
5. Confirm that the command is being followed.
6. Save and reload.
7. Confirm that the assigned command is still present.
8. Let the game run for a short period.
9. Confirm that the army continues following the command.

**Expected Result**

- Commands assigned to AI-created armies persist after reload.
- The army does not lose command ownership or command target data.
- The army continues behaving according to the player command.
- No exception or crash occurs.

---

## 1.2 Mercenary Army Leaders Policy

These tests verify whether the **Mercenary Army Leaders Policy** correctly enables or disables mercenary army leadership.

### 1.2.1 Policy Enacted Enables Mercenaries in ArmyManagementVM

**Preconditions**

- Player is the ruler of a kingdom.
- At least one mercenary clan is contracted by the kingdom.
- The **Mercenary Army Leaders Policy** is not currently enacted.

**Steps**

1. Open the army management screen.
2. Confirm whether mercenary parties are disabled or unavailable before policy enactment.
3. Enact the **Mercenary Army Leaders Policy**.
4. Reopen the army management screen.
5. Check whether eligible mercenary parties are enabled.
6. Save and reload.
7. Reopen the army management screen again.
8. Try to create an army with a mercenary party.

**Expected Result**

- Before the policy is enacted, mercenary parties should not be eligible unless another permission system allows them.
- After enactment, eligible mercenary parties should be enabled in `ArmyManagementVM`.
- After reload, the policy state is preserved.
- The player can create an army involving eligible mercenary parties.
- No exception or crash occurs.

---

### 1.2.2 Policy Enacted Allows AI Mercenary Army Creation After Reload

**Preconditions**

- Player is the ruler of a kingdom.
- At least one mercenary clan is contracted by the kingdom.
- The **Mercenary Army Leaders Policy** is enacted.
- Mercenary parties are available and not locked by other armies.

**Steps**

1. Save the game with the policy enacted.
2. Let the campaign run until a mercenary army is created by the AI, or until a reasonable observation window passes.
3. Record whether a mercenary-created army appears.
4. Reload the save from step 1.
5. Let the campaign run again under similar conditions.
6. Record whether a mercenary-created army appears again.

**Expected Result**

- Mercenary AI parties are allowed to create armies while the policy is enacted.
- Reloading does not disable this behavior.
- No invalid army is created with missing leader, missing clan, or invalid kingdom data.
- No exception or crash occurs.

**Notes**

This test can be affected by campaign AI randomness. If no mercenary army appears, verify whether the mercenary parties had enough influence, available parties, valid targets, and no conflicting army membership.

---

### 1.2.3 Policy Disavowed Prevents AI Mercenary Army Creation

**Preconditions**

- Player is the ruler of a kingdom.
- At least one mercenary clan is contracted by the kingdom.
- The **Mercenary Army Leaders Policy** was previously enacted and is now disavowed.

**Steps**

1. Disavow the **Mercenary Army Leaders Policy**.
2. Save the game.
3. Let the campaign run for a reasonable observation window.
4. Check whether any mercenary clan creates an army.
5. Reload the save from step 2.
6. Let the campaign run again.
7. Check again whether any mercenary clan creates an army.

**Expected Result**

- Mercenary clans should not create armies through the policy after it has been disavowed.
- Reloading should not restore the old enacted policy behavior.
- Existing armies should not become corrupted if the policy changes while they exist.
- No exception or crash occurs.

**Additional Check**

If a mercenary army already existed before the policy was disavowed:

- Existing mercenary armies are allowed to continue until disbanded

Record the actual behavior.

---

### 1.2.4 Policy Disavowed Disables Mercenaries in ArmyManagementVM After Reload

**Preconditions**

- Player is the ruler of a kingdom.
- At least one mercenary clan is contracted by the kingdom.
- The **Mercenary Army Leaders Policy** is currently enacted.

**Steps**

1. Open the army management screen and confirm eligible mercenary parties are enabled.
2. Disavow the **Mercenary Army Leaders Policy**.
3. Reopen the army management screen.
4. Confirm that mercenary parties are disabled or unavailable.
5. Save and reload.
6. Reopen the army management screen.
7. Confirm again that mercenary parties are disabled or unavailable.

**Expected Result**

- Mercenary parties are disabled after the policy is disavowed.
- The disabled state persists after reload.
- Disabled reason or tooltip is correct, if available.
- No exception or crash occurs.

---

# 2. Vassal Save

This section currently covers future-feature gating.

## 2.1 Army Creation as Vassal Before and After Existing Kingdom Armies

**Preconditions**

- Player is a vassal, not ruler.
- Kingdom starts with zero active armies if possible.

**Steps**

1. Confirm the kingdom has zero active armies.
2. Check whether the player can gather an army.
3. Let the campaign run or create conditions so the kingdom has more than zero active armies.
4. Check again whether the player can gather an army.
5. Save and reload.
6. Re-check army creation availability.

**Expected Result**

- Army creation availability follows the intended vassal rules.
- Existing kingdom army count does not accidentally bypass feature requirements.
- Reloading does not change eligibility.
- Disabled reason or tooltip is correct when army creation is unavailable.
- No exception or crash occurs.

**Future Feature Coverage**

- Army Commander Renown
- Army Commander Legend Trait
- King-granted army command permission for vassal player

---

# 3. No Kingdom Save

Use a save where the player has no kingdom and is ready to speak to a kingdom leader.

## 3.1 Mercenary Army Leaders Policy

### 3.1.1 Mercenary Contract Without Policy Permission

**Preconditions**

- Player has no kingdom.
- Player is ready to speak to a kingdom leader.
- Target kingdom has zero active armies if possible.
- Player does not have mercenary army leadership permission and requirements met.
- **Mercenary Army Leaders Policy** is not enacted in the target kingdom.

**Steps**

1. Sign a mercenary contract with the target kingdom.
2. Confirm the player is now under mercenary service.
3. Check whether the player can gather an army when the kingdom has zero active armies.
4. Let the game reach a state where the kingdom has more than zero active armies.
5. Check whether the player can gather an army again.
6. Save and reload.
7. Re-check army creation availability.

**Expected Result**

- Without the policy or special permission, the mercenary player should not be allowed to gather armies.
- Existing kingdom army count should not accidentally enable army creation.
- Reloading should not change eligibility.
- Disabled reason or tooltip is correct.
- No exception or crash occurs.

---

### 3.1.2 Mercenary Contract With All Policies Activated

**Preconditions**

- Player has no kingdom.
- Player is ready to speak to a kingdom leader.
- Target kingdom has at least one mercenary clan, or the player is joining as a mercenary.
- Console/debug tools are available.

**Steps**

1. Sign a mercenary contract with the target kingdom.
2. Activate all policies for the player kingdom using the relevant debug command.
   - Example: `campaign.activate_all_policies_for_player_kingdom`
3. Confirm that the **Mercenary Army Leaders Policy** is active.
4. Check whether the player can gather an army when the kingdom has zero active armies.
5. Let the game reach a state where the kingdom has more than zero active armies.
6. Check whether the player can gather an army again.
7. Save and reload.
8. Confirm the policy is still active.
9. Create an army.

**Expected Result**

- With the policy active, the mercenary player should be allowed to gather armies.
- Army creation availability persists after reload.
- Army creation succeeds without invalid state.
- No exception or crash occurs.

---

## 3.2 Mercenary Army Leadership Dialogue

### 3.2.1 Dialogue Is Disabled Until Requirements Are Met

**Preconditions**

- Player has no kingdom.
- Player is ready to speak to a kingdom leader.
- Player does not meet mercenary army leadership requirements.
- Target kingdom leader is available for conversation.

**Steps**

1. Sign a mercenary contract with the target kingdom.
2. Speak to the kingdom leader.
3. Open the dialogue path where the army leadership request should appear.
4. Check whether **Mercenary Army Leadership Dialogue** is disabled.
5. Verify that the disabled hint explains the missing requirements.
6. Increase the required values using a controlled test method.
   - Required values:
     - Clan renown (cheat ex: campaign.add_renown_to_clan 10000)
     - Relationship with the kingdom leader (cheat ex: campaign.add_hero_relation Caladog | 50)
7. Speak to the kingdom leader again.
8. Open the same dialogue path.
9. Check whether the dialogue option is now enabled.

**Expected Result**

- The dialogue option appears in the correct dialogue list.
- It is disabled while requirements are not met.
- The hint clearly explains why it is disabled.
- After requirements are met, the option becomes enabled.
- No duplicate dialogue options appear.
- No exception or crash occurs.

---

### 3.2.2 Granted Permission Enables Army Creation and Persists After Reload

**Preconditions**

- Continue from test 3.2.1.
- Player is a mercenary.
- Player now meets the requirements for mercenary army leadership permission.
- The permission has not already been granted.

**Steps**

1. Request mercenary army leadership permission from the kingdom leader.
2. Confirm the request is accepted.
3. Check whether the player can gather an army when the kingdom has zero active armies.
4. Let the game reach a state where the kingdom has more than zero active armies.
5. Check whether the player can gather an army again.
6. Save and reload.
7. Re-check whether the player can gather an army.
8. Create an army.

**Expected Result**

- The permission is granted only after the request succeeds.
- The gather army button becomes available when the permission applies.
- Existing kingdom army count does not break eligibility.
- Permission persists after reload.
- Army creation succeeds.
- No exception or crash occurs.

---

### 3.2.3 Permission Behavior After Ending and Rejoining Mercenary Contract

**Preconditions**

- Continue from test 3.2.2.
- Player is a mercenary with granted mercenary army leadership permission.
- Player can end the mercenary contract.

**Steps**

1. End the mercenary contract.
2. Confirm the player is no longer under mercenary service.
3. Check gather army availability.
4. Save and reload.
5. Re-check the same conditions after reload.
6. Sign a mercenary contract again.
7. Check gather army availability before making a new request.
8. Speak to the kingdom leader.
9.  Request mercenary army leadership permission again.
10. Check gather army availability after the request.
11. Create an army.

**Expected Result**

- Ending the mercenary contract removes or suspends permission.
- After rejoining, the player must request permission again.
- Army creation is disabled before the new request.
- Army creation is enabled after the new request.
- Save/reload preserves the current contract-scoped state.

---

# 4. Additional Regression Tests

## 4.1 Existing Mercenary Army When Policy Is Removed

**Preconditions**

- Player is ruler.
- Mercenary army exists.
- **Mercenary Army Leaders Policy** is enacted.

**Steps**

1. Confirm a mercenary-led army exists.
2. Disavow the policy.
3. Observe the existing army.
4. Save and reload.
5. Observe the existing army again.

**Expected Result**

- Existing mercenary armies are allowed to continue until disbanded
- No invalid leader state appears.
- No crash occurs.
- The army either continues validly or disbands cleanly.

---

## 4.2 Mercenary Clan Leaves Kingdom While Leading Army

**Preconditions**

- A mercenary clan is contracted by a kingdom.
- A party from that clan leads an army.
- The contract can end naturally or through test manipulation.

**Steps**

1. Confirm the mercenary-led army exists.
2. End the mercenary clan contract or wait until it ends.
3. Observe the army state.
4. Save and reload.
5. Observe again.

**Expected Result**

- The army does not remain attached to an invalid kingdom.
- The army does not keep members from an enemy or neutral kingdom in an invalid way.
- The game does not crash.
- Any disbanding or command clearing behavior is clean.

---

## 4.3 Dialogue Does Not Appear for Invalid Conversation Targets

**Preconditions**

- Player is a mercenary or potential mercenary.
- Several lord conversation targets are available.

**Steps**

1. Speak to a normal lord who is not the kingdom ruler.
2. Check the relevant dialogue branch.
3. Speak to the kingdom ruler.
4. Check the relevant dialogue branch.
5. Speak to enemy or neutral rulers if available.
6. Check the relevant dialogue branch.

**Expected Result**

- The mercenary army leadership request appears only for valid kingdom leader targets.
- It does not appear for unrelated lords.
- It does not appear for invalid kingdoms.
- Disabled hints are used where appropriate.
- No duplicate or orphan dialogue options appear.

---

## 4.4 ArmyManagementVM Disabled Reason

**Preconditions**

- Player is in a state where army creation should be blocked.
- At least one mercenary party is visible in army management.

**Steps**

1. Open army management.
2. Hover or inspect disabled mercenary parties.
3. Record the disabled reason.
4. Change the relevant condition.
   - Enact policy, or
   - Gain permission, or
   - Rejoin contract, depending on the scenario.
5. Reopen army management.
6. Check whether the disabled reason disappears or changes correctly.

**Expected Result**

- Disabled reason matches the actual blocking condition.
- Text does not show raw localization IDs.
- Text does not mention the wrong system.
- UI updates correctly after condition changes.
- No exception or crash occurs.

---

# 5. Suggested Priority Order

Run these tests first after any change to army eligibility, policy storage, or save/load behavior:

1. `1.2.1` Policy enacted enables mercenaries in `ArmyManagementVM`.
2. `1.2.4` Policy disavowed disables mercenaries after reload.
3. `3.2.1` Dialogue disabled/enabled based on requirements.
4. `3.2.2` Granted permission enables army creation and persists after reload.
5. `3.2.3` Ending and rejoining mercenary contract.
6. `1.1.1` Player-created army command persistence.
7. `1.1.2` AI-created army command persistence.

---

# 6. Known Risk Areas

Pay special attention to these areas while testing:

- Policy object storage after reload.
- `Clan.PlayerClan.IsUnderMercenaryService` checks.
- Kingdom active policy lookups.
- Army creation eligibility when `Kingdom` is null.
- Mercenary clans leaving the kingdom.
- AI army creation randomness.
- Dialogue options appearing for invalid heroes.
- Permission state becoming stale after contract changes.
- UI disabled reasons not updating after state changes.
- Commands assigned to AI-created armies being overwritten by vanilla AI.

---

# 7. Final Release Checklist

Before publishing a build, verify:

- [ ] No crash during save/load tests.
- [ ] No crash when opening `ArmyManagementVM`.
- [ ] No crash when creating a mercenary-led army.
- [ ] No crash when disavowing the policy.
- [ ] No duplicate dialogue options.
- [ ] Disabled hints are clear.
- [ ] Policy state persists after reload.
- [ ] Permission state follows the chosen design.
- [ ] Army commands persist after reload.
- [ ] Mercenary contract end/rejoin behavior is consistent.
- [ ] The mod still works when the feature is never used.
- [ ] Existing saves without the new data load correctly.

---

# 8. Future Feature Placeholder

Future tests should be added for:

- Army Commander Renown
- Army Commander Legend Trait
- Vassal request to king for army command permission
- Permission revocation
- Permission costs, cooldowns, or relationship penalties
- AI response variation based on ruler personality or relationship
