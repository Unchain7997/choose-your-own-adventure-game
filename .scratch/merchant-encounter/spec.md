# Spec: Merchant Encounter

Status: ready-for-agent

Feature slug: `merchant-encounter`
Branch: `feature/merchant-encounter`
Baseline: 143 passing tests, 0 failing (verified 2026-11-12 with `dotnet test`)

## Problem Statement

When a player takes the South path, nothing happens: the narrative says they get bored and go home, then the game sends them back to the path choice anyway. The South path is a dead branch with no reason to pick it, and the player has no way to influence their odds before facing the dragon on the North path. Every character walks into the Dragon Encounter with exactly the stats they rolled.

## Solution

The South path leads to a one-time **Merchant Encounter** with **Old Brannoc**, a travelling **Merchant**. Every character now rolls a starting amount of **Gold**. Old Brannoc sells three **Items** (a **Healing Potion**, a **Strength Tonic** and an **Agility Elixir**). Each one permanently raises one stat the moment it's bought. Gold is scarce: nobody can afford all three, so the player has to decide how to prepare. After leaving the Merchant, the player goes back to the path choice. If they head North, they face the dragon with their improved stats. Every new message is available in English, Spanish, French and Italian.

## Requirements

**Where and when**

- **R1.** The Merchant Encounter takes place on the **South path**, replacing the old "nothing adventurous happens" outcome. The existing South-path narrative message keeps its key, and its text is rewritten in all four languages to describe meeting a travelling merchant on the forest road. The narrative **does not name** the Merchant and keeps its current placeholders (none). Old Brannoc is introduced by name in the greeting that follows (AC6).
- **R2.** The Merchant Encounter happens **once per adventure**. Every later South visit shows a translated "Old Brannoc has moved on" message instead. The player then goes back to the path prompt, and North is still available.

**Gold**

- **R3.** The Player has a **Gold** stat, set by a d20 roll (1–20) with **no race or occupation modifier**. It's rolled with the same "type 'roll'" prompt pattern as the other stats, **right after Health** during character creation.
- **R4.** Gold appears in the Player stats display (`DisplayStats`). It does **not** appear in the combat stats display.

**Items**

- **R5.** Old Brannoc sells exactly three Items, with fixed prices and fixed bonuses:

  | Menu # | Item | Price | Effect |
  |---|---|---|---|
  | 1 | Healing Potion | 5 Gold | +6 HealthPoints |
  | 2 | Strength Tonic | 8 Gold | +4 Strength |
  | 3 | Agility Elixir | 8 Gold | +4 Agility |

  Effects apply **immediately** and permanently. There's no inventory, no stat cap, and no change to combat mechanics.
- **R6.** Each Item can be bought **at most once** per adventure. A player can buy several different Items in one encounter.

**Encounter flow**

- **R7.** The encounter shows, in order: the South-path narrative, Old Brannoc's greeting, the player's current Gold, then the Merchant menu. The menu has fixed options: `1` Healing Potion, `2` Strength Tonic, `3` Agility Elixir, `4` Leave. A bought Item stays listed and is marked **Sold**.
- **R8.** Results of a menu choice:
  - **Affordable, unsold Item:** Gold goes down by the price and the stat goes up by the bonus. A translated confirmation shows the new stat value and the Gold left. There's no y/n confirmation prompt.
  - **Unaffordable Item:** a translated "not enough Gold" message appears. Gold and stats don't change, and the menu appears again.
  - **Sold Item:** a translated "already bought" message appears and the menu appears again.
  - **Any other input:** the existing `invalid` message appears and the menu appears again.
- **R9.** The encounter **ends automatically** when no unsold, affordable Item remains, including when the player starts with fewer than 5 Gold. The order is: a translated reason line, then Old Brannoc's farewell, then back to the path prompt.
- **R10.** Choosing **Leave** shows Old Brannoc's farewell, then returns to the path prompt.

**Language**

- **R11.** Every new message exists in English, Spanish, French and Italian. Item names are translated. Each Item's effect description in the menu (for example "+4 Agility") and the stat names in purchase confirmations (for example "Strength is now 18") are translated dictionary messages too. They don't reuse the existing stats labels, because those end in `": "`. **"Old Brannoc" is identical in every language.**
- **R12.** The language data keeps **exactly six maps** per language. All new text goes in as `dictionary` keys only. No seventh map is added for Items.

**Design and tests**

- **R13.** A new `Merchant` class holds the shop rules and the encounter state (whether the encounter has happened, which Items are Sold). It does no console input or output. A thin encounter method on `Game` handles the prompts and printing. A new Merchant is created with each adventure, alongside the new Player. Gold is a Player stat, not Merchant state.
- **R14.** All new behaviour is covered by tests at the four agreed seams (see Testing Decisions). The dictionary key-count test is renamed and updated to the new count.
- **R15.** All 143 existing tests keep passing. ADR 0001 (`docs/0001-add-italian-language-support.md`) is **not modified**, and no new ADR is created.

## Acceptance Criteria

Each criterion must be checked by at least one automated test unless it says otherwise.

**Clarifications agreed after Round 4**

- **AC1. Starting below 5 Gold.** The player sees the South-path narrative, the greeting and their Gold line, but **no menu**. Then the reason line, then the farewell, then the path prompt.
- **AC2. When the automatic end is checked.** It's checked when the encounter starts and **after each successful purchase**. A refused choice (unaffordable, Sold, invalid) never ends the encounter.
- **AC3. Check order.** For an Item that is both Sold and unaffordable, **Sold is checked first**, so the player gets the "already bought" message.
- **AC4. What counts as "met".** The encounter counts as met on the **first** South visit, even if it ends automatically at once. The next South visit shows the "moved on" message.
- **AC5. The word "Gold" on screen.** It's translated per language (Spanish *Oro*, French *Or*, Italian *Oro*). In code and in the glossary it stays `Gold`.
- **AC6. Old Brannoc's name.** It comes from `Merchant.Name` and is inserted into messages as a `{0}`-style placeholder, the same way `dragon_intro` uses the dragon's name. The placeholder parity test then covers it in every language. No message has the name typed into its text; in particular, the South-path narrative doesn't name him (R1).
- **AC7. Gold never goes negative.** Gold is always ≥ 0. A refused purchase leaves Gold, HealthPoints, Strength and Agility exactly as they were.
- **AC8. How a Sold Item is shown.** It keeps its price and adds a Sold mark, for example `3. Agility Elixir (+4 Agility) - 8 Gold [Sold]`.
- **AC9. The rest of the game is unchanged.** The North path, Exit, the main menu, combat rules and the combat stats display behave exactly as before.

**Confirmed by the user**

- **AC10.** The baseline of 143 passing and 0 failing tests is preserved. After implementation the suite is 143 plus the new tests, with 0 failing.
- **AC11.** Every new behaviour is covered by tests.
- **AC12.** ADR 0001 is byte-for-byte unchanged: `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing. `51c1941` is the commit this branch started from. Don't compare with `main`, because `main` doesn't contain the Italian work. This one is checked by hand, not by an automated test.
- **AC13.** The North path, combat mechanics and existing game behaviour stay intact.
- **AC14.** A player who buys Items on the South path benefits from them in the Dragon Encounter that follows. Combat reads the Player's raised Strength, Agility and HealthPoints with no change to combat code.

**Behaviour criteria derived from R1–R12**

- **AC15.** With 13 Gold: buying the Healing Potion leaves 8 Gold and +6 HealthPoints. Then buying the Strength Tonic leaves 0 Gold and +4 Strength, and the encounter ends automatically (R9, AC2).
- **AC16.** With 20 Gold: Strength Tonic and Agility Elixir (16 total) leave 4 Gold. The Potion costs 5, so the encounter ends automatically. Nobody can buy all three Items (they total 21 Gold, above the maximum roll of 20).
- **AC17.** With 7 Gold: choosing the Strength Tonic shows "not enough Gold" with nothing changed. The menu appears again, the Healing Potion can still be bought, and buying it leaves 2 Gold and ends the encounter automatically.
- **AC18.** Menu numbering never changes between redisplays. Choice `2` always means Strength Tonic, whether or not other Items are Sold.
- **AC19.** Input is trimmed before matching, the same as the existing menus. ` 2 ` is the same as `2`.
- **AC20.** Gold is shown by `DisplayStats` in all four languages, with a translated label.
- **AC21.** Gold is never shown in the combat stats display.
- **AC22.** The South-path narrative message key is still present in all four languages, with rewritten text. No existing key is removed.

## User Stories

1. As a player, I want the South path to lead somewhere meaningful, so that choosing it is a real decision and not a dead end.
2. As a player, I want to meet a named Merchant, Old Brannoc, on the South path, so that the encounter feels like part of the story.
3. As a player, I want to roll my starting Gold during character creation, so that money feels like part of my character just like Strength and Agility.
4. As a player, I want to type "roll" for my Gold the same way I do for my other stats, so that character creation feels consistent.
5. As a player, I want to see my Gold when I view my stats, so that I know what I can afford before choosing a path.
6. As a player, I want the combat display to stay focused on fighting stats, so that Gold doesn't clutter the battle.
7. As a player, I want to see my current Gold when the Merchant's menu opens, so that I don't have to remember it.
8. As a player, I want to see every Item's name, effect and price in the menu, so that I can compare them before buying.
9. As a player, I want to buy a Healing Potion that raises my HealthPoints, so that I can survive more dragon attacks.
10. As a player, I want to buy a Strength Tonic that raises my Strength, so that my attacks hit the dragon more often.
11. As a player, I want to buy an Agility Elixir that raises my Agility, so that I block more of the dragon's attacks.
12. As a player, I want a purchase to take effect immediately, so that I don't need to manage an inventory or remember to use it later.
13. As a player, I want a confirmation showing my new stat and remaining Gold after each purchase, so that I can see exactly what I bought.
14. As a player, I want purchases to happen without an extra "are you sure?" prompt, so that shopping is quick.
15. As a player, I want to buy more than one different Item if I can afford them, so that I can combine upgrades.
16. As a player, I want each Item limited to one purchase, so that the game stays balanced and I have to choose.
17. As a player, I want bought Items to stay in the menu marked Sold, so that I can see what I've already bought.
18. As a player, I want the menu numbers to stay the same after a purchase, so that I'm never surprised by a shifted list.
19. As a player, I want a clear message when I choose an Item I already bought, so that I understand why nothing happened.
20. As a player, I want a clear message telling me how much Gold I need when I can't afford an Item, so that I can choose something else.
21. As a player, I want a refused purchase to cost me nothing, so that a mistyped choice never punishes me.
22. As a player, I want an invalid menu entry to show the familiar "invalid choice" message, so that the Merchant menu behaves like the other menus.
23. As a player, I want to leave the Merchant at any time without buying anything, so that I'm never forced into a purchase.
24. As a player, I want the encounter to end on its own when there's nothing left I can afford, so that I'm not stuck at a menu where only Leave makes sense.
25. As a player who rolled very little Gold, I want to be told straight away that I can't afford anything, so that I don't waste time in a menu.
26. As a player, I want to know why the Merchant closed up shop, so that the automatic end doesn't feel like a bug.
27. As a player, I want Old Brannoc to say farewell whether I leave or the shop closes on its own, so that the scene ends clearly.
28. As a player, I want to return to the path choice after the Merchant, so that I can still go North and fight the dragon.
29. As a player, I want my purchased upgrades to be in effect during the Dragon Encounter, so that my preparation pays off.
30. As a player, I want the Merchant to appear only once per adventure, so that the scarcity of Gold and Items means something.
31. As a player who goes South a second time, I want to be told that Old Brannoc has moved on, so that the repeat visit still makes sense in the story.
32. As a player, I want to choose Exit after shopping, so that I can still end the adventure whenever I like.
33. As a Spanish-, French- or Italian-speaking player, I want every Merchant message, Item name and the Gold label in my language, so that the new feature feels as complete as the rest of the game.
34. As a player in any language, I want the Merchant's name to stay "Old Brannoc", so that he's recognisably the same character, like Smolderfang.
35. As a player who never goes South, I want the game to play exactly as before, so that the new feature doesn't change my existing experience.
36. As a player, I want combat to work exactly as it always has, so that purchases change my stats but not the rules.
37. As a developer, I want the shop rules in a `Merchant` class with no console I/O, so that every rule can be tested with plain values.
38. As a developer, I want the encounter's prompts in one small `Game` method, so that the flow can be tested end to end with scripted input and doesn't expose other game logic.
39. As a developer, I want new text added only as dictionary keys, so that the six-map language structure and its tests stay valid.
40. As a developer, I want the key-count test updated and renamed to the new total, so that its name stays truthful.
41. As a developer, I want all 143 existing tests to keep passing, so that I know the feature introduced no regressions.
42. As a developer, I want console-based tests to restore `Console.In` and `Console.Out`, so that tests don't leak into each other.
43. As a maintainer, I want ADR 0001 left untouched, so that the record of the Italian-language decision stays accurate to its time.

## Implementation Decisions

**Modules**

- **`Merchant` (new).** Holds the encounter rules and state. No console I/O. Its public surface is kept to what the encounter flow and tests need:
  - `Name`: always "Old Brannoc".
  - Whether the encounter has happened, plus a way to mark it as happened.
  - Lookups for each Item's price, bonus and Sold status, by menu number 1–3.
  - A purchase operation that takes a Player and an Item, and returns one of three outcomes: **Purchased**, **InsufficientGold** or **AlreadySold**. On Purchased, it deducts the price from the Player's Gold, applies the bonus to the matching stat, and marks the Item Sold. On any other outcome it changes nothing. Sold is checked before affordability (AC3).
  - A query for whether the Player can afford any unsold Item (drives R9 and AC1–AC2).
  - The Item catalogue (three Items, prices, bonuses, affected stat) is fixed data inside Merchant, not loaded from JSON.
- **`Player` (modified).** Gains a `Gold` property, default 0 on a new Player. `CreateCharacter` rolls it with the Player's Die (d20, no modifier), using the existing roll-prompt pattern, right after Health and before the next-action menu. `DisplayStats` prints a translated Gold line. Nothing in the Player's combat methods changes.
- **`Game` (modified).**
  - A new Merchant is created each time a new Player is created (main-menu choice 1), so the once-per-adventure state resets with each adventure.
  - The South branch calls a new encounter method instead of printing the old narrative directly.
  - That method is the **only new public member** on `Game`. It takes the Player, the Merchant and Messages, and does:
    - **Repeat visit:** if the encounter has already happened, print "moved on" and return.
    - **Otherwise:** mark the encounter as happened, print the narrative, greeting and Gold, then run the menu loop until Leave or the automatic end.
  - The method doesn't touch the Dragon Encounter, combat or path parsing.
  - It's public instead of private-with-reflection (the Dragon Encounter test's approach), because reflection breaks without warning when a method is renamed.
- **`Combat`, `Dragon`, `Die`, `Weapon`, `Messages` code:** unchanged. `Messages` already supports `{0}`-style formatting through `string.Format` at the call site.
- **Language data (modified).**
  - New `dictionary` keys are added to all four language sections, with identical key sets and identical placeholders.
  - The South-path narrative key keeps its name and gets rewritten text. The text doesn't name the Merchant and has no placeholders.
  - No keys are removed, and no maps are added.
  - The new keys cover:
    - the Gold roll prompt
    - the Gold stats label
    - the greeting (`{0}` = Merchant name)
    - the Gold display line
    - the three Item names
    - each Item's effect description for the menu (for example "+6 Health", "+4 Strength", "+4 Agility")
    - the stat names used in purchase confirmations
    - the menu line format, the Sold mark and the Leave option
    - "not enough Gold" (with the price and current Gold as placeholders)
    - "already bought"
    - purchase confirmations showing the new stat value and remaining Gold
    - the automatic-end reason line
    - the farewell (`{0}` = Merchant name)
    - "moved on" (`{0}` = Merchant name)
  - Exact key names and the final count are set during implementation. The renamed count test must assert that exact total.
- **Existing message reuse.** The menu prompt reuses `enter_choice`, and invalid input reuses `invalid`.

**Why item names are dictionary keys, not a display map.** Weapons use a dedicated display map, but adding a seventh map would break the "all six required maps" structure test and expand the language data schema for three strings. Dictionary keys keep the schema stable. This is easy to reverse, so it doesn't warrant an ADR.

## Testing Decisions

**What makes a good test here.**
- Test what a player or a caller can see: return values, Player stats after an action, and console output.
- Don't test private fields or the internal order of method calls.
- Assert on messages by looking them up through `Messages.GetMessage` (formatted where needed), not by hard-coding English text. Then the same test runs in every language, and the key will exist in every language.
- Because no dice are involved, every Merchant test is deterministic: set the Player's Gold and stats directly before acting.

**Seam 1: Merchant (no console).** This is the main seam.
- **Prices and bonuses:** each of the three Items' price and bonus is checked exactly.
- **Purchasing:** Purchased deducts the price, applies the bonus to the right stat only, and marks the Item Sold.
- **Refused purchases:** InsufficientGold and AlreadySold change nothing (AC7). Sold is checked before affordability (AC3).
- **Encounter state:** the encounter starts as not happened and stays happened once marked (AC4).
- **Affordability:**
  - false below 5 Gold
  - false once everything affordable is Sold
  - true when only the Potion is affordable
- **Scenarios:** AC15, AC16 and AC17 are each a test.
- **Prior art:** the plain-value tests in `PlayerTests`, `CombatTests` and the validation tests in `GameTests`.

**Seam 2: the Game encounter method, with scripted console input and captured output.**
- **What each test does:** feeds input with `Console.SetIn`, captures output with `Console.SetOut`, then checks the output (and its order where it matters) plus the Player's final stats.
- **What's covered:**
  - the menu layout and the Sold mark (R7, AC8)
  - the "not enough Gold", "already bought" and invalid-input paths
  - the order on automatic end: reason line, then farewell (R9)
  - Leave: farewell (R10)
  - below 5 Gold: no menu is printed (AC1)
  - a second call prints "moved on" and nothing else from the encounter (R2, AC4)
  - trimmed input (AC19)
  - at least one full run in Spanish, French and Italian
- **Prior art:** `DragonTests.HandleDragonEncounter_AttackPath_DisplaysEncounterFlowText`. These tests **must restore `Console.In` and `Console.Out` after each test** (save the originals at setup and restore at cleanup), matching the save/restore already in `DragonTests`.

**Seam 3: Player.**
- **Defaults:** a new Player has 0 Gold (extend the existing default-values test).
- **Gold roll:** run `CreateCharacter` with scripted input several times (for example 20) on fresh Players, and check that each Gold value is between 1 and 20 (R3). The only existing range test, `DieTests`, rolls once; repeating the run here is new and catches out-of-range values more reliably.
- **Stats display:** `DisplayStats` output contains the translated Gold label and value in each language (AC20).
- **Benefit in the Dragon Encounter (AC14):** after buying the Strength Tonic, `AttackHits` returns true for a roll equal to the new Strength, which was a miss before the purchase. The Agility and HealthPoints bonuses are covered by seam 1's exact-bonus tests.
- **Combat stats display (AC21):** tested directly by calling the existing public `Combat.GetCombatStatsDisplay()` and checking the output contains no Gold label, following `CombatTests`. It's an existing public method checked with plain values, so it isn't a fifth seam.
- **Prior art:** `PlayerTests`, `CombatTests` and `DieTests`. Console capture is restored as in seam 2.

**Seam 4: LanguageDataTests (existing).**
- **Unchanged and still passing:** the parity, map-structure, placeholder-match and format-validity tests pick up the new keys automatically.
- **Renamed and updated:** `Language_HasAll69DictionaryKeys` gets the new total in its name and its assertion.
- **New check:** the South-path narrative key still exists in every language (AC22).

**Not tested end to end:** `StartGame` as a whole, because the dragon's rolls are random. Seam 2 covers all the South-path behaviour without dice, and seam 3 proves that boosted stats reach combat.

**Regression:** `dotnet test` reports 143 + N passed, 0 failed (AC10).

## Out of Scope

- An inventory, carrying Items, or using Items during combat.
- Weapon upgrades or any change to `Weapon` or the weapon display map.
- Stat caps of any kind.
- Any other use for Gold (rewards from the dragon, a second shop, selling).
- Changes to combat rules, the dragon's stats or the combat stats display.
- Showing Gold in combat.
- Random Item bonuses or randomised prices.
- A seventh map in the language data, or any change to the six-map structure.
- Changes to ADR 0001, or a new ADR.
- New languages.
- End-to-end tests of `StartGame`.
- Committing, pushing or opening a PR as part of this spec.

## Further Notes

- **Branch base:** this branch starts from `feature/italian-language` (commit `51c1941`), not from `main`. The 143-test baseline exists only there; `main` still has the 94-test suite. A future PR from this branch to `main` would include the Italian work as well.
- Glossary terms used in this spec are defined in `GLOSSARY.md`: Merchant, Merchant Encounter, Gold, Item, Healing Potion, Strength Tonic, Agility Elixir, Dragon Encounter.
- ADR 0001 records "69 dictionary keys" and names the hard-coded count test. That reflects the state when it was written and stays as-is. The ADR itself anticipates that the count test will need updating when keys are added.
- Balance check: an average roll (~10 Gold) buys one Item, or the Potion plus one other. Rolls of 1–4 (a 20% chance) buy nothing. No roll can buy all three.
- Creation can already produce Strength or Agility up to 24 (d20 + d4), so values above 20 after a Tonic or Elixir are consistent with existing behaviour.
