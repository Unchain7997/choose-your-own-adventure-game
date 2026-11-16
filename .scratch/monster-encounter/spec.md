# Spec: Troll Encounter

Status: ready-for-agent

Feature slug: `monster-encounter`
Branch: `feature/monster-encounter` (from `main` at `3a10d6b`)
Baseline: 262 passing tests, 0 failing (verified 2026-11-16 with `dotnet test`)
Requirements agreed in a `/grill-with-docs` interview (Rounds 1–4, Q1–Q28). Glossary terms used here are defined in `GLOSSARY.md`: Monster, Troll Encounter, Crossroads, Dragon Encounter, Merchant Encounter, Gold.

## Problem Statement

The Crossroads offers only two paths. North leads to the one fight in the game, and the adventure always ends there. South leads to the Merchant, who can be visited only once. Nothing in the game lets the player fight before facing the dragon, risk their health for a reward, or decide in what order to tackle the world. Combat is also written for the dragon alone, so no second creature can be fought without copying the combat code.

## Solution

A new **West path** leads to the **Troll Encounter**. There **Grimtusk**, a cave troll guarding a mountain pass, blocks the way with a club. The player can fight him or retreat. Beating him earns a one-time d10 Gold reward and returns the player to the Crossroads, so they can still shop with the Merchant or face the dragon. Losing to him ends the adventure. Retreating leaves him at the pass, still carrying his injuries, for another try.

To make this possible without copying combat, the Dragon and Grimtusk both become kinds of **Monster**. Combat is written against a Monster, and the Dragon Encounter plays exactly as it does today. Every new message is available in English, Spanish, French and Italian.

## Requirements

**Where and what**

- **R1.** The Crossroads accepts a new **West path**. It's chosen with `w` or `west`, ignoring case and surrounding whitespace, like the other paths. Only the English letters are accepted, as for `n`, `s` and `e`. East is not added, because `e` already means Exit.
- **R2.** The West path leads to the **Troll Encounter** with **Grimtusk**, a cave troll guarding a mountain pass, armed with a **club**. "Grimtusk" is identical in every language and is always inserted into messages through a `{0}`-style placeholder. No message has the name typed into its text.
- **R3.** A **Monster** is any creature the player fights. The Dragon (Smolderfang) and Grimtusk are both Monsters.

**Grimtusk's stats**

- **R4.** Grimtusk rolls Strength d12, Agility d10 and HealthPoints d12, using the Die. His club does up to 8 damage. He uses the same combat rules as the dragon.
- **R5.** One Grimtusk is created per adventure, alongside the Player and Merchant. His stats are rolled once, when he's created, and never again during that adventure.

**Encounter flow**

- **R6.** A first visit shows, in order:
  1. the West-path narrative
  2. Grimtusk's intro
  3. the "magic ring" stats intro and his stats display
  4. the attack-or-retreat prompt

  This mirrors the Dragon Encounter.
- **R7.** Choosing attack starts combat with the same rules and round structure as the dragon fight: the combat stats display, then rounds of player attack, Grimtusk's counter-attack and the attack/retreat prompt.
- **R8.** **Victory.** Grimtusk's defeated narrative is printed during combat. Then a d10 Gold reward is rolled once with the shared Game Die, added to `Player.Gold`, and shown in the victory message with the amount found and the new total. The player then returns to the Crossroads.
- **R9.** **Defeat.** The troll-specific defeat message is printed and the adventure ends, as a dragon defeat does.
- **R10.** **Retreat before combat** (`r` or `retreat` at the encounter prompt) prints the troll retreat message and returns to the Crossroads. Nothing changes for either side.
- **R11.** **Retreat during combat** prints the existing shared `retreat_combat` line (from the unchanged combat loop), then the troll retreat message, then returns to the Crossroads.
- **R12.** **Invalid input** at the encounter prompt shows the existing `invalid` message and asks again.
- **R13.** **End of input** at the encounter prompt counts as a retreat (R10) and returns to the Crossroads. This handling exists only at this new prompt. See Known Limitations.
- **R14.** The dragon's `retreat`, `victory` and `defeat` endings are never shown in the Troll Encounter.

**State across visits**

- **R15.** **After a victory**, Grimtusk stays defeated for the rest of the adventure. Every later West visit shows only the translated "pass is clear" message (name from `{0}`) and returns to the Crossroads. There's no narrative, intro, stats, prompt, fight or reward, and no stat changes. The reward is therefore given exactly once.
- **R16.** **After a retreat**, the same Grimtusk stays at the pass with his remaining HealthPoints and his original stats. The next West visit plays the full encounter again.
- **R17.** The player's HealthPoints carry over between encounters, including from the Troll Encounter into the Dragon Encounter. Nothing restores them automatically.
- **R18.** A new adventure brings a new Grimtusk with freshly rolled stats.

**Monster architecture and compatibility**

- **R19.** A shared `Monster` base class holds what every Monster has, and `Dragon` and a new `Troll` extend it. `Combat` and `Player.Attack` take a `Monster`.
- **R20.** Each Monster supplies its own stats header, combat stats header, 4 defence taunts, 4 damage replies, defeated narrative and player attack intro. Generic combat messages are shared.
- **R21.** The Dragon's public API (`new Dragon(die)`, its properties with public setters, and its methods) and its gameplay are unchanged. That covers the North path, its text, stats, rules and its victory, defeat and retreat endings.
- **R22.** `Combat`'s public `dragon` property is renamed `monster` and typed `Monster`.

  **Tests that existed before this feature** (before Ticket 01) may change only in these three approved ways, each keeping the test's original intent:
  1. **The renamed property.** The one assertion that reads it (`CombatTests.Combat_Constructor_InitializesPlayerAndDragon`) is updated to `combat.monster`.
  2. **The invalid-path example.** `GameTests.IsValidPathChoice_ReturnsFalse_ForInvalidInput` used `"west"` as its example of invalid input, which R1 makes valid. Its example becomes `"east"`, which is still invalid, so the test still checks that an unoffered direction is rejected.
  3. **The dictionary key-count test.** It is renamed and its expected total updated as keys are added: 91 → 97 in Ticket 02 (including the temporary `troll_fight_not_ready` key), then 97 → 110 in Ticket 03.

  Every other test that existed before this feature is unchanged and passes.

  **Tests introduced by this feature's own tickets** (Tickets 01 and 02) may be updated or removed by a later ticket only when the approved specification requires it. This covers:
  - removing Ticket 02's temporary attack-notice test, along with the temporary behaviour it tested
  - updating the language tests that listed the temporary `troll_fight_not_ready` key, once Ticket 03 removes that key

  No unrelated existing test may be weakened, removed or modified to make this feature pass.
- **R23.** The shared combat loop and the existing input handling of the Crossroads, the Dragon Encounter and character creation are unchanged, including what they do when input runs out.
- **R24.** The Merchant Encounter is unchanged. Gold found in Grimtusk's stash can be spent at the Merchant like any other Gold.

**Language**

- **R25.** The 19 new `dictionary` keys listed under Message Catalogue are added to all four languages. The dictionary grows from 91 to 110 keys.
- **R26.** `path_prompt_full` and `path_invalid` are rewritten in all four languages to include the West path. They keep their key names and their placeholders (none). No other existing key is renamed or reworded.
- **R27.** `club` is added to each language's `displayWeaponMap`. The language data keeps **exactly six maps** per language.
- **R28.** All four languages keep identical key sets and identical placeholders per key. Spanish and French are written without accents, matching their existing text; Italian keeps its accents. The Spanish, French and Italian text is written in the implementing ticket and reviewed by the user in that diff before commit.
- **R29.** ADR 0001 (`docs/0001-add-italian-language-support.md`) is **not modified**. No new ADR is created; the Monster decision is recorded in this spec.

## Message Catalogue

New `dictionary` keys (English wording approved in the interview, Q24). `{0}`, `{1}` and `{2}` are `string.Format` placeholders.

| Key | English | Placeholders |
|---|---|---|
| `west_path_narrative` | You take the West path. The trail climbs between grey cliffs to a narrow mountain pass, where a huge shape rises from a cave mouth and blocks the way. | none |
| `troll_intro` | The troll growls 'I am {0}. Nobody crosses my pass!' | {0} name |
| `troll_stats_intro` | Your magic ring gives you the stats on the troll. | none |
| `troll_stats_header` | ----- Troll Stats ----- | none |
| `combat_troll_header` | --- Troll --- | none |
| `troll_player_attack_intro` | As a {0}, you withdraw your {1} and attack the troll. | {0} occupation, {1} weapon |
| `troll_defense_taunt_1` | Ha! My grandmother swings faster than that! | none |
| `troll_defense_taunt_2` | You tickle my warts, {0}! | {0} race |
| `troll_defense_taunt_3` | Rocks hit harder than you! | none |
| `troll_defense_taunt_4` | Is that a weapon or a toothpick? | none |
| `troll_damage_reply_1` | Oof! That one hurt! | none |
| `troll_damage_reply_2` | Grr! You will pay for that! | none |
| `troll_damage_reply_3` | Lucky hit! It won't happen twice! | none |
| `troll_damage_reply_4` | Hrmph. Stronger than you look. | none |
| `troll_defeated_narrative` | With a final groan, the troll crashes to the ground. The pass is yours. | none |
| `troll_victory` | You search {0}'s cave and find {1} Gold in his stash. You now have {2} Gold. You head back down to the crossroads. | {0} name, {1} Gold found, {2} new total |
| `troll_defeat` | {0} roars in triumph as everything goes dark. You are defeated. Your family mourns their loss. The End. | {0} name |
| `troll_retreat` | You back away down the trail and return to the crossroads. | none |
| `troll_pass_clear` | The mountain pass is quiet. {0} lies defeated, and nothing blocks the way. | {0} name |

Rewritten existing keys (same names, no placeholders):

| Key | New English |
|---|---|
| `path_prompt_full` | You are walking in the mountains. You see three paths: one leading north and higher into the mountains, one leading south into the woods, and one leading west toward a narrow mountain pass. Which do you take? Enter 'n' for North. Enter 's' for South. Enter 'w' for West. Enter 'e' for exit game. |
| `path_invalid` | Invalid choice. Please enter 'n', 's', 'w', or 'e'. |

New `displayWeaponMap` entry: `club` → English `club`, Spanish `garrote`, French `massue`, Italian `clava`, lowercase like the existing weapon names.

Club ASCII art (word-free, so it isn't translated): `(@)======`

Reused unchanged in the Troll Encounter:
- **Prompts:** `dragon_encounter_prompt` (its text is generic), `attack_prompt`, `invalid`, `retreat_combat`
- **Combat stats headers:** `combat_stats_header`, `combat_player_header`
- **Generic hit, miss, damage and health lines:** `player_hit_dragon`, `player_missed_dragon`, `dragon_defended_player_attack`, `player_damage_dealt`, `dragon_health_now`, `dragon_attack_intro`, `dragon_missed_player`, `dragon_hit_player`, `player_defended_dragon_attack`, `dragon_damage_dealt`, `player_health_now`, `player_defeated_narrative`

Their key names say "dragon", but their text takes the Monster's name through `{0}`. They aren't renamed.

## Acceptance Criteria

Each criterion is checked by at least one automated test unless it says otherwise.

**Crossroads**

- **AC1.** `w`, `west`, `W`, `WEST` and ` west ` are accepted as the West path. Every input accepted today (`n`, `north`, `s`, `south`, `e`, `exit` in any case, padded) is still accepted, and invalid input is still rejected.
- **AC2.** In every language, `path_prompt_full` mentions `'w'` and `path_invalid` lists `'n'`, `'s'`, `'w'` and `'e'`.

**Grimtusk**

- **AC3.** A new Grimtusk is named "Grimtusk" and carries a `club` with a maximum damage of 8. Over many new Grimtusks, Strength is always 1–12, Agility 1–10 and HealthPoints 1–12.
- **AC4.** Grimtusk's attack rules follow his stats exactly as the dragon's follow its own: a roll at or below Strength hits, and damage stays between 1 and 8.

**First visit and outcomes**

- **AC5.** A first visit prints the West-path narrative, Grimtusk's intro (with his name), the stats intro, his stats under `troll_stats_header`, then the attack-or-retreat prompt, in that order.
- **AC6.** **Victory.** Using the victory fixture (see Testing Decisions), choosing attack wins. The output shows `troll_defeated_narrative` and then `troll_victory`. The Gold gained is 1–10 and equals the amount printed, and the printed total equals the new `Player.Gold`. The encounter method reports a victory, and the player isn't sent to an ending.
- **AC7.** **Defeat.** Using the defeat fixture, choosing attack loses. The output ends with `troll_defeat`, the method reports a defeat, and none of the dragon's `victory`, `defeat` or `retreat` texts appear.
- **AC8.** **Retreat before combat.** `r` or `retreat` at the encounter prompt prints `troll_retreat` and reports a retreat. Player and Grimtusk stats are unchanged, and no combat output appears.
- **AC9.** **Retreat during combat.** Using the retreat fixture, attacking and then entering `r` prints `retreat_combat` followed by `troll_retreat`, and reports a retreat.
- **AC10.** **Invalid input.** Invalid input at the encounter prompt prints `invalid` and the prompt again. Padded input (` a `, ` r `) is accepted.
- **AC11.** **End of input.** Running out of input at the encounter prompt prints `troll_retreat` and reports a retreat, with no infinite loop.
- **AC12.** Combat against Grimtusk shows `combat_troll_header` and his own attack intro, taunts and damage replies. Combat against the dragon still shows `combat_dragon_header`, `player_attack_intro` and the dragon's own lines.

**Repeat visits and persistence**

- **AC13.** **After a victory**, a second visit prints only `troll_pass_clear` (name from `{0}`) and reports that the pass is clear. It shows no narrative, intro, stats, prompt, combat or reward, and changes no Player stat, including Gold.
- **AC14.** **After a retreat during combat**, using the injury fixture, Grimtusk's HealthPoints are below their starting value. A second visit shows that same remaining value in his stats display and plays the full encounter again. His Strength and Agility are unchanged.
- **AC15.** **After a retreat before combat**, a second visit plays the full encounter again with all stats unchanged.
- **AC16.** The player's HealthPoints after a Troll Encounter are the values they carry into the next encounter. Nothing restores them.
- **AC17.** A new adventure creates a new Grimtusk who hasn't been defeated and has freshly rolled stats.

**Compatibility**

- **AC18.** The Dragon Encounter, the North path, the South path and Merchant Encounter, Exit and the main menu behave exactly as before. All existing tests pass. Tests that existed before this feature change only in the three ways approved in R22 (the `combat.monster` assertion, the `"east"` invalid-path example and the key-count test). Tests introduced by Tickets 01 and 02 change only where R22 allows it (the temporary attack-notice test and the language tests for its key). No unrelated test is weakened, removed or modified.
- **AC19.** Combat's `monster` property returns the Monster it was given, for both a Dragon and a Grimtusk.
- **AC20.** Gold found in Grimtusk's stash can be spent at the Merchant. Checked by a test that gives a player troll Gold and buys an Item.

**Language**

- **AC21.** All 19 new keys exist in all four languages, with identical placeholders per key. The renamed key-count test asserts 110 keys.
- **AC22.** `club` exists in every language's `displayWeaponMap`. Exactly six maps remain.
- **AC23.** No dictionary value in any language contains "Grimtusk" typed in.
- **AC24.** At least one full Troll Encounter run (victory) in each of Spanish, French and Italian prints that language's messages. Every new key has a complete translation with the same placeholders as English and wording suited to that language. A translation may match English where that's the right word (for example "Troll" in French and Italian). Wording quality is checked by the user's review of the translation diff, not by an automated test.

**Regression and documents**

- **AC25.** `dotnet test` reports 262 + N passed, 0 failed, 0 skipped, and `dotnet build` reports 0 warnings.
- **AC26.** ADR 0001 is byte-for-byte unchanged: `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing. Checked by hand.

## User Stories

1. As a player, I want a third path at the Crossroads, so that I have more choices than "dragon" or "shop".
2. As a player, I want to choose West with `w` or `west`, so that it works like the other paths.
3. As a player, I want to meet a named troll, Grimtusk, so that the fight feels part of the story.
4. As a player, I want to see Grimtusk's stats before deciding, so that I can judge the risk like I do with the dragon.
5. As a player, I want to retreat before the fight, so that I can come back better prepared.
6. As a player, I want to retreat mid-fight and return to the Crossroads, so that a bad fight isn't the end of my adventure.
7. As a player, I want Grimtusk to keep his injuries when I come back, so that my earlier effort counts.
8. As a player, I want a Gold reward for beating Grimtusk, so that the risk pays off.
9. As a player, I want to spend that Gold at the Merchant, so that the order I visit places matters.
10. As a player, I want to be told the pass is clear after I've won, so that a repeat visit still makes sense.
11. As a player, I want losing to Grimtusk to end the adventure, so that the fight is a real risk.
12. As a player, I want my injuries to carry into the dragon fight, so that I have to weigh fighting Grimtusk first.
13. As a player, I want Grimtusk to taunt me in his own voice, so that he feels different from the dragon.
14. As a Spanish-, French- or Italian-speaking player, I want the whole Troll Encounter in my language, so that it feels as complete as the rest of the game.
15. As a player who never goes West, I want the game to play exactly as before.
16. As a developer, I want combat written against a Monster, so that a new creature doesn't need copied combat code.
17. As a developer, I want deterministic tests for every combat outcome, so that the suite never fails by chance.
18. As a maintainer, I want ADR 0001 untouched and the Dragon's public API unchanged, so that earlier work and its tests stay valid.

## Implementation Decisions

**Modules**

- **`Monster` (new, abstract base class).**
  - **Holds:** the shared Die, `Name`, `Strength`, `Agility`, `HealthPoints` and `Weapon`, all with public setters, as on `Dragon` today.
  - **Shared behaviour:** the attack rules (`RollAttack`, `AttackHits`, `RollDamage`), the counter-attack (`Attack(Player, Messages)`), random taunt and damage-reply selection, and the stats display.
  - **Per-Monster message keys:** stats header, combat stats header, defeated narrative, player attack intro, 4 taunts and 4 damage replies.
  - This is today's `Dragon` code moved up a level, with only the dragon-specific keys turned into per-Monster values.
- **`Dragon` (modified).**
  - Extends `Monster`. Keeps its constructor `Dragon(Die die)`, its name, d20 stats and claws.
  - Supplies its existing keys: `dragon_stats_header`, `combat_dragon_header`, `dragon_defeated_narrative`, `player_attack_intro`, `dragon_defense_taunt_1–4` and `dragon_damage_reply_1–4`.
  - Its observable behaviour and public members are unchanged.
- **`Troll` (new).**
  - Extends `Monster`. Its constructor takes the shared Die and sets the name "Grimtusk" and the stats d12/d10/d12.
  - Weapon: `new Weapon("club", 8, "(@)======")`.
  - Supplies the `troll_*` keys and `combat_troll_header`.
  - Is defeated when its HealthPoints are 0 or below. There's no separate "defeated" flag. In normal gameplay his HealthPoints start at 1–12 (R4), so 0 or below happens only after a killing blow. A Grimtusk who starts at 0 or below exists only through an artificial test setup, and he is treated as already defeated (pass clear).
  - Rolls the d10 stash reward with that same Die, which is the shared Game Die.
- **`Combat` (modified).** Takes a `Monster`, and the public property `dragon` becomes `monster`. The stats display reads the Monster's combat stats header. The loop itself, including how it handles invalid and missing input, is unchanged.
- **`Player` (modified).** `Attack` takes a `Monster`. It reads the attack intro, defeated narrative, taunts and replies from the Monster. Nothing else changes.
- **`Game` (modified).**
  - **New Grimtusk per adventure:** main-menu choice 1 creates a new Troll (with the shared Die), alongside the new Player and Merchant.
  - **Crossroads parsing:** `PromptForPath` and `IsValidPathChoice` accept `w` and `west`.
  - **New public method:** a Troll Encounter method that takes the Player, the Troll and Messages, and returns an outcome: victory, defeat, retreat or pass clear.
    - If Grimtusk is already defeated, it prints `troll_pass_clear` and returns.
    - Otherwise it runs R6–R13.
  - **Main loop:** the West branch calls that method. On defeat it ends the adventure, as the North branch does. On every other outcome it returns to the Crossroads.
  - **Unchanged:** `HandleDragonEncounter` keeps its private `(Player, Dragon, Messages)` signature and behaviour, and `HandleMerchantEncounter` is unchanged.
  - The method is public, not private-with-reflection, for the same reason the Merchant spec gave: reflection breaks without warning when a method is renamed.
- **`Messages`, `Die`, `Weapon`, `Merchant`:** unchanged.
- **Language data:** the 19 new keys, 2 rewritten keys and 1 `displayWeaponMap` entry from the Message Catalogue, in all four languages.

**Why a shared Monster base class.** The alternative was a troll-only copy of `Combat` and `Player.Attack`. That doubles the combat rules and would leave Category B (Difficulty) two places to change. A base class keeps one set of rules and leaves the Dragon's public API in place, so existing tests compile unchanged. It's easy to reverse and not surprising, so it doesn't need an ADR.

## Testing Decisions

**What makes a good test here.** Same rules as the Merchant feature:
- Test observable behaviour: return values, stats after an action, and console output.
- Look messages up through `Messages.GetMessage`, formatted where needed, instead of hard-coding English.
- Console tests save and restore `Console.In` and `Console.Out`.

**Deterministic combat without changing `Die`.** The combat rules give fixed results at extreme stats, whatever the dice roll:
- An attack hits when a d20 roll (1–20) is ≤ Strength: Strength 20 always hits and Strength 0 never does.
- A defender blocks when a d20 roll is ≤ Agility: Agility 0 never blocks.
- Damage is at least 1.

Every outcome test uses one of these **fixtures**. Out-of-range values such as Grimtusk Strength 20, Agility 0, or starting HealthPoints of 0 or below are test setups only. The game never generates them (R4).

| Fixture | Setup | Why the result is certain |
|---|---|---|
| Victory | Player Strength 20 with a weapon; Grimtusk Agility 0, HealthPoints 1 | The player strikes first, always hits, is never blocked, and any damage kills him |
| Defeat | Player Strength 0, Agility 0, HealthPoints 1; Grimtusk Strength 20 | The player always misses; he always hits, is never blocked, and any damage is fatal |
| Retreat during combat | Player Strength 0; Grimtusk Strength 0 | Nobody can hit, so both survive the round and `r` is read |
| Injury kept after retreat | Player Strength 20 with a weapon of max damage ≤ 12; Grimtusk Agility 0, Strength 0, HealthPoints 20 | The player always hits for 1–12, so he always survives below 20 and can't hit back |

**Range checks.** Grimtusk's stats and the d10 reward are checked over many runs (for example 200 new Grimtusks, and 50 victories). Each run asserts the value is in range, which is always true, so these tests can't fail by chance.

**Seam 1: Monster and Grimtusk, plain values, no console.**
- Grimtusk's name, club and stat ranges (AC3).
- His attack rules (AC4).
- Each Monster's own messages, checked through what it prints (AC12). Grimtusk's output uses the `troll_*` messages, and the Dragon's uses its existing ones. The per-Monster message keys stay protected: tests never read them directly, and no public member is added only for testing.
- The stash reward range.
- Prior art: `DragonTests`, `CombatTests`.

**Seam 2: the Game Troll Encounter method, with typed-in console input and captured output.**
- AC5–AC11 and AC13–AC15, each using the fixtures above.
- At least one victory run in each of Spanish, French and Italian (AC24).
- AC20: troll Gold spent through the existing Merchant Encounter method.
- AC12: Grimtusk's combat text. The combat stats display is also checked directly through the existing public `Combat.GetCombatStatsDisplay()`, as the Merchant spec did; that's not a new seam.
- Prior art: `MerchantEncounterTests`.

**Seam 3: Crossroads parsing.**
- `IsValidPathChoice` (AC1).
- Prior art: `GameTests`.

**Seam 4: LanguageDataTests (existing).**
- **Picked up automatically:** the parity, placeholder, map-structure and format-validity tests cover the new keys.
- **Renamed:** `Language_HasAll91DictionaryKeys` becomes `Language_HasAll110DictionaryKeys`.
- **New:** a troll key-list test, the `club` weapon-map check, the `'w'` path-text check (AC2), and the no-typed-name check (AC23).

**Seam 5: existing Dragon, Combat and Merchant tests.**
- They are unchanged apart from R22's three approved changes. Together they are the check that nothing broke (AC18, AC19).
- AC19 adds one new `Combat` test using Grimtusk.

**Not tested end to end:** `StartGame` as a whole, as before. AC17 (a new Grimtusk per adventure) is checked by constructing new Trolls, because each adventure needs a new program run (see Known Limitations).

**Regression:** `dotnet test` reports 262 + N passed, 0 failed (AC25).

## Known Limitations

These come from existing, deliberately unchanged code (R23). They're recorded so nobody mistakes them for defects in this feature. Fixing them is out of scope.

- **L1. End of input at the Crossroads loops forever.** `PromptForPath` treats missing input as invalid and asks again indefinitely, as it does on `main` today. The same is true of the character-creation and Dragon Encounter prompts. When R13 sends a player back to the Crossroads after running out of input, this loop isn't resolved.
- **L2. End of input during troll combat keeps fighting.** The shared combat loop treats missing input as invalid and continues attacking until one side falls. If Grimtusk falls, the player returns to the Crossroads and hits L1. If the player falls, the adventure ends normally.
- **L3. Monster HealthPoints can show negative.** After a killing blow, a Monster's HealthPoints can print as a negative number, as the dragon's already can. Only the player's HealthPoints are clamped at 0. This is shared, unchanged behaviour.
- **L4. One adventure per run.** North, Exit and a defeat all end the program, so a new adventure means running the game again. "A new Grimtusk per adventure" is therefore seen across runs.

## Out of Scope

- **Category B: Difficulty settings.** These get their own interview and spec. Monster stats just have to live in one place.
- An East path, or non-English path letters.
- Fixing L1–L4 or changing the shared combat loop.
- Healing, health restoration, or HealthPoints caps.
- Changes to the Dragon (stats, text, rules, endings), the Merchant, the Items or their prices.
- More than one troll per adventure, random encounters, ambushes, or monsters on other paths.
- Weapon or loot rewards other than the d10 Gold.
- Renaming any existing message key.
- A seventh language-data map, or new languages.
- Changes to ADR 0001, or a new ADR.
- Committing, pushing or opening a PR as part of this spec. Each ticket is committed only after user review.

## Further Notes

- The Merchant spec's Out of Scope ruled out "any other use for Gold (rewards from the dragon…)". That limited only the Merchant feature. This feature adds a Gold source, and the Merchant spec stays unchanged as a record of its own scope.
- `GLOSSARY.md` was updated during the interview with Monster, Troll Encounter and Crossroads, and the Gold and Merchant Encounter entries were aligned. Those changes ship with the first implementation ticket's commit (approved by the user).
- **Balance:** Grimtusk hits on a d20 roll ≤ his d12 Strength, so at most 60% of the time, against up to 100% for the dragon. His club does at most 8 damage against the dragon's 12. On its own, a d10 reward buys at most one Item: the Healing Potion from 5 Gold, or the Strength Tonic or Agility Elixir from 8.
  - **Combined with starting Gold, it can buy all three Items.** The d20 starting Gold plus Grimtusk's d10 reward can reach 30 Gold. The three Merchant Items cost 21 Gold together (5 + 8 + 8), so a player who beats Grimtusk before visiting the Merchant can buy all three. This needs starting Gold + reward ≥ 21, which is 55 of the 200 equally likely combinations (27.5%).
  - **The Merchant spec's limit applies to starting Gold alone.** Its statement that nobody can buy all three Items ("above the maximum roll of 20") was written when the d20 roll was the only source of Gold, and it is still true of that roll on its own. The Merchant spec and Merchant gameplay are unchanged.
