# Spec: Difficulty Setting

Status: ready-for-agent

Feature slug: `difficulty-setting`
Branch: `feature/difficulty-setting` (from `feature/monster-encounter` at `8217a67`)
Baseline: 345 passing tests, 0 failing, 0 skipped (verified 2026-11-16 with `dotnet test`)
Requirements agreed in a `/grill-with-docs` interview (Round 1 Q2–Q10, a disposable simulation, then Rounds 2–4 Q11–Q30 and the final decisions). Glossary terms used here are defined in `GLOSSARY.md`: Monster, Troll Encounter, Dragon Encounter, Crossroads, Merchant, Gold. **Difficulty** is added to the glossary by Ticket 01 (R27).

Course context: CIDM4390 AI Agentic Coding Part 4, Category B. The requirement is "A difficulty setting (easy/normal/hard) that changes combat balance." It sets no amounts and doesn't say which stats change; both were decided in the interview.

**The feature is complete only after Ticket 03.** Ticket 02 makes the difficulty prompt and the Character Summary line visible before the chosen Difficulty has any effect on combat (see Delivery plan).

## Problem Statement

Every player faces the same fights. Grimtusk is beaten about 84% of the time and Smolderfang about 55% of the time, whatever the player wants from the game. A player who wants to see the story through has no gentler option, and a player who finds the game too easy has no harder one. The game offers no way to choose how hard the fights are.

## Solution

When the player chooses to create a character, the game first asks them to choose a **Difficulty**: Easy, Normal or Hard. The choice lasts for the whole adventure.

- **Easy** makes Grimtusk and Smolderfang weaker: 2 less Strength, Agility and HealthPoints each, never below 1.
- **Normal** is the game exactly as it is today.
- **Hard** makes both Monsters stronger: 2 more Strength, Agility and HealthPoints each.

The player sees the effect in each Monster's stats before every fight, and sees the chosen Difficulty on their Character Summary. Nothing else changes: the player's own stats, weapons, Gold and Merchant prices are the same on every Difficulty. The prompt and every new message are available in English, Spanish, French and Italian.

## Requirements

**Choosing a Difficulty**

- **R1.** After the player chooses `1` (Create Character) at the main menu, the game asks for a Difficulty **before** the race prompt. It is asked once per adventure.
- **R2.** The prompt is a numbered menu in the style of the main menu: the translated `difficulty_prompt` line, then the existing `enter_choice` text written without a newline.
- **R3.** `1` selects Easy, `2` Normal and `3` Hard. Surrounding whitespace is trimmed. Nothing else is accepted: no words, in any language.
- **R4.** **Invalid input** (anything other than `1`, `2` or `3` after trimming, including an empty line) prints the existing `invalid` message and shows the prompt again.
- **R5.** **End of input** (no line could be read) prints the translated `difficulty_auto_normal` message, selects Normal, and moves on to the race prompt. The difficulty prompt never loops on end of input. The race prompt that follows keeps its existing behaviour (Known Limitation L1).
- **R6.** The chosen Difficulty is fixed for the whole adventure. Nothing changes it after it's chosen.

**What Difficulty changes**

- **R7.** Difficulty changes **only** each Monster's Strength, Agility and HealthPoints, by the same amount for each of the three stats:

  | Difficulty | Adjustment |
  |---|---|
  | Easy | −2 |
  | Normal | 0 |
  | Hard | +2 |

- **R8.** It applies to **both** Monsters: Smolderfang and Grimtusk.
- **R9.** Each adjusted stat has a **minimum of 1**, applied after the adjustment. Only Easy can reach it. There is **no maximum** on Hard (Known Limitation L2).
- **R10.** Each Monster is adjusted **once**, right after it is created and before the player can meet it. Its rolled stats are not re-rolled.
- **R11.** **Unchanged on every Difficulty:** Monster weapons and their damage, the hit and block rules, the combat loop, the player's stats and how they're rolled, starting Gold, Grimtusk's Gold reward, the Merchant, its Items and their prices.
- **R12.** **Normal plays exactly like today's game.** Normal Monsters roll the same dice for the same stats and get no adjustment.

**Showing the Difficulty**

- **R13.** The Character Summary shows one translated line, `stats_difficulty` followed by the translated Difficulty name, **immediately after the Gold line, and before the Weapon line when a weapon is shown**.
- **R14.** The Monster stats screens and the combat stats display show the adjusted numbers, with **no** Difficulty label.

**Code structure**

- **R15.** A `Difficulty` enumeration with the values `Easy`, `Normal` and `Hard`.
- **R16.** `Monster` gains one shared public method that applies a Difficulty to that Monster's Strength, Agility and HealthPoints (R7, R9). It works for every Monster without per-subclass code. The `Dragon` and `Troll` constructors are unchanged.
- **R17.** The Dragon is created in the main-menu choice `1` branch, after the Difficulty is chosen, next to Grimtusk. It is no longer created at the top of `StartGame`. It still uses the shared Game Die.
- **R18.** `Player` gains a `Difficulty` property, defaulting to Normal. Game sets it from the prompt **before** character creation, so the Character Summary can show it. How the character is rolled doesn't change.
- **R19.** `Game` gains a pure parsing method that turns the prompt input into Easy, Normal, Hard or "not valid", and a console prompt method that uses it (R2–R5). Both are public, for the same reason as the Category A Troll Encounter method: reflection breaks without warning when a method is renamed.
- **R20.** **Die injection for tests.** `Die.Roll` becomes `virtual`, with unchanged behaviour. `Game` gains a constructor that takes a `Die`, and `StartGame` uses that Die for the Monsters. The parameterless `Game()` keeps creating a new `Die`, so the program entry point is unchanged. `Player` keeps its own Die.

**Language data**

- **R21.** The 6 new `dictionary` keys in the Message Catalogue are added to all four languages. The dictionary grows from 110 to 116 keys. The language data keeps **exactly six maps** per language; Difficulty names are dictionary keys, not a new map.
- **R22.** No existing key is renamed or reworded. The existing `invalid` and `enter_choice` messages are reused at the difficulty prompt.
- **R23.** All four languages keep identical key sets and identical placeholders per key. None of the 6 new keys has a placeholder. Spanish and French are written without accents, matching their existing text; Italian keeps its accents. The Spanish, French and Italian text is written in Ticket 02. **The user reviews all 24 new entries (6 keys × 4 languages, including English and the names approved in Q21) in the Ticket 02 diff, and Ticket 02 is not committed until they are approved.**

**Unchanged**

- **R24.** The main menu, language selection, character creation prompts, the Crossroads, the Merchant Encounter, the Troll Encounter and the Dragon Encounter keep their existing input handling, including what they do when input runs out.
- **R25.** The Category A spec and tickets (`.scratch/monster-encounter/`) are not edited.
- **R26.** ADR 0001 (`docs/0001-add-italian-language-support.md`) is **not modified**. No new ADR is created; this spec records the decisions (see Implementation Decisions and Further Notes).

**Glossary**

- **R27.** `GLOSSARY.md` gains a new `## Adventure` section, placed after `## Encounters`, with this entry (approved in Q28), committed with Ticket 01:

  > **Difficulty**:
  > The setting, Easy, Normal or Hard, that the player chooses once for each adventure, right after choosing to create a character. It makes every Monster weaker or stronger for that whole adventure. Normal is the game as it was before Difficulty existed.
  > _Avoid_: Level, mode, game mode, challenge rating

  The amounts (±2, minimum 1) are rules, so they live in this spec and not in the glossary.

## Message Catalogue

New `dictionary` keys. None has a placeholder.

| Key | English |
|---|---|
| `difficulty_prompt` | `Choose a difficulty: 1. Easy  2. Normal  3. Hard. Type '1', '2' or '3'` |
| `difficulty_auto_normal` | `No choice was entered. Normal difficulty was selected automatically.` |
| `stats_difficulty` | `Difficulty: ` (with a trailing space, like `stats_gold`) |
| `difficulty_easy` | `Easy` |
| `difficulty_normal` | `Normal` |
| `difficulty_hard` | `Hard` |

Approved Difficulty names and label (Q21):

| | `difficulty_easy` | `difficulty_normal` | `difficulty_hard` | `stats_difficulty` |
|---|---|---|---|---|
| Spanish | Facil | Normal | Dificil | Dificultad: |
| French | Facile | Normal | Difficile | Difficulte: |
| Italian | Facile | Normale | Difficile | Difficoltà: |

Each label ends with a colon and a trailing space, like the existing `stats_*` labels in that language (for example French `Or: `). The full `difficulty_prompt` and `difficulty_auto_normal` sentences in Spanish, French and Italian are written in Ticket 02 and reviewed by the user before commit (R23).

Reused unchanged: `invalid`, `enter_choice`, the `stats_*` labels and every Monster stats header.

## Acceptance Criteria

**Prompt**

- **AC1.** After choosing `1` at the main menu, the difficulty prompt appears **before** the race prompt, in the chosen language.
- **AC2.** `1`, `2` and `3`, with surrounding spaces trimmed, select Easy, Normal and Hard. Anything else prints `invalid` and asks again.
- **AC3.** When input ends at the difficulty prompt, it prints the translated `difficulty_auto_normal` message, returns Normal, and the game proceeds to the race prompt. The existing L1 limitation of the prompts after it stays out of scope.

**Combat balance**

- **AC4.** On Easy, Grimtusk and Smolderfang each have Strength, Agility and HealthPoints 2 lower than rolled, with a minimum of 1. On Hard, they are 2 higher. On Normal, they are unchanged.
- **AC5.** Weapon damage, hit and block rules, Gold, Merchant prices and the player's stats are the same on every Difficulty. Checked by the existing tests, Seam 1 (weapon damage) and the focused diff review in Testing Decisions; no Die is injected into `Player`.
- **AC6.** The Monster stats screens and the combat stats display show the adjusted numbers, with no Difficulty label.

**Display and duration**

- **AC7.** The Character Summary shows the translated Difficulty line immediately after the Gold line, and before the Weapon line when a weapon is shown.
- **AC8.** The Difficulty stays the same for the whole adventure.

**Language data**

- **AC9.** All four languages have 116 dictionary keys, including the 6 Difficulty keys, each non-empty.

**Regression and documents**

- **AC10.** Normal plays exactly like today's game. All existing tests pass; the only existing test that changes is the key-count test, renamed from 110 to 116.
- **AC11.** ADR 0001 is byte-for-byte unchanged: `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing.

**Wiring**

- **AC12.** With a fixed test Die, a full scripted adventure shows Grimtusk and Smolderfang at the rolled value +2 on Hard, the rolled value on Normal, and the rolled value −2 on Easy, down to a minimum of 1, for Strength, Agility and HealthPoints. Each Monster is checked **individually**, in its own stats block (see Testing Decisions, Seam 5).
- **AC13.** The program entry point and the game's behaviour without the test Die are unchanged. The only change to `Die.Roll` is that it becomes `virtual`.

## User Stories

1. As a player, I want to choose how hard the fights are, so that the game suits how I like to play.
2. As a new player, I want an Easy option, so that I can see the story through without losing to the dragon most of the time.
3. As an experienced player, I want a Hard option, so that beating the Monsters feels like an achievement.
4. As a player who is happy with the game, I want Normal to play exactly as before, so that nothing I know changes.
5. As a player, I want to choose the Difficulty when I create a character, so that I decide before the adventure starts.
6. As a player, I want to choose by typing `1`, `2` or `3`, so that it works like the main menu.
7. As a player, I want spaces around my choice ignored, so that a stray space isn't treated as a mistake.
8. As a player who makes a typo, I want to be told it was invalid and asked again, so that I don't end up on a Difficulty I didn't choose.
9. As a player whose input runs out, I want the game to pick Normal and tell me so, so that the prompt doesn't hang and I know what happened.
10. As a player, I want the Difficulty to stay the same for the whole adventure, so that the challenge is consistent.
11. As a player on Easy, I want Grimtusk and Smolderfang to be weaker, so that fights are more forgiving.
12. As a player on Hard, I want Grimtusk and Smolderfang to be stronger, so that fights are tougher.
13. As a player on Easy, I want Monsters never to be weakened below 1 in any stat, so that Grimtusk still has to be fought and the dragon can't be beaten without a hit.
14. As a player, I want to see each Monster's adjusted stats before deciding to attack or retreat, so that I can judge the risk on my chosen Difficulty.
15. As a player, I want to see my Difficulty on my Character Summary, so that I can confirm what I chose.
16. As a player, I want my own stats, weapon, Gold and the Merchant's prices to stay the same on every Difficulty, so that only the fights change.
17. As a Spanish-, French- or Italian-speaking player, I want the difficulty prompt, names and messages in my language, so that the feature feels as complete as the rest of the game.
18. As a player, I want retreat, victory, defeat and the Gold reward to work the same on every Difficulty, so that the rules I know still hold.
19. As a developer, I want one shared Monster method to apply a Difficulty, so that a future Monster gets difficulty support without extra code.
20. As a developer, I want deterministic tests that prove the chosen Difficulty reaches both Monsters through the real game, so that the suite never fails by chance and a wiring mistake can't go unnoticed.
21. As a developer, I want the test Die passed in through Game's constructor, so that the game itself still rolls real dice.
22. As a maintainer, I want ADR 0001 and the Category A documents untouched, so that earlier work stays as it was approved.
23. As a reviewer, I want each ticket to compile and pass the full suite, so that every commit is safe to review on its own.

## Implementation Decisions

**Modules**

- **`Difficulty` (new enumeration).** `Easy`, `Normal`, `Hard`.
- **`Monster` (modified).** Gains one public method that applies a Difficulty: it adds −2, 0 or +2 to Strength, Agility and HealthPoints, then raises any of the three that is below 1 to 1. Normal leaves the stats untouched. The method knows nothing about which Monster it is. Weapon, attack rules, messages and the stats display are unchanged.
- **`Dragon`, `Troll` (unchanged).** Their constructors still roll their stats with the shared Die. Their existing range tests stay valid.
- **`Player` (modified).** Gains a `Difficulty` property (default Normal). The Character Summary prints the `stats_difficulty` line with the translated Difficulty name immediately after Gold and before Weapon. Character rolling is unchanged.
- **`Game` (modified).**
  - **Constructors:** a parameterless constructor that creates a new `Die` (today's behaviour), and a constructor that takes a `Die`. `StartGame` uses the Game's Die for the Monsters instead of creating its own.
  - **Parsing:** a public pure method that maps trimmed `1`/`2`/`3` to Easy/Normal/Hard and returns "not valid" (null) for everything else, including null input.
  - **Prompt:** a public method that takes Messages, shows `difficulty_prompt` and `enter_choice`, re-prompts with `invalid` on invalid input, and on end of input prints `difficulty_auto_normal` and returns Normal.
  - **Main-menu choice `1`:** prompt for the Difficulty; create the Player and set its Difficulty; create the character; create the Merchant; create Grimtusk and Smolderfang with the shared Die and apply the Difficulty to each.
  - **Translated Difficulty names:** a fixed mapping from each Difficulty value to its `difficulty_*` key, with no silent fallback for an unknown value (the same rule as the Merchant's purchase-result handling).
- **`Die` (modified).** `Roll` becomes `virtual`. Its behaviour is unchanged.
- **`Combat`, `Merchant`, `Messages`, `Weapon`, the program entry point:** unchanged.
- **Language data:** the 6 new keys in all four languages.

**Why Monster stats (approach A).** The interview compared four approaches with a 20,000-fight simulation per case (Further Notes):
- **A, Monster stats:** the widest, most even spread between Easy and Hard, and the only approach the player can see working, because the adjusted stats appear on each Monster's stats screen before every fight.
- **B, roll modifiers:** smaller effect per point, uneven changes in fight length, invisible to the player, and it would change the hit and block checks that Category A kept identical for every Monster.
- **C, Monster weapon damage:** the weakest effect (about ±4–5 points against the dragon at ±4 damage).
- **E, A + C:** almost nothing over A alone.

**Why ±2.** ±4 made the tiers more distinct but made Hard punishing (fewer than 1 dragon win in 3). ±2 keeps each tier about 13 points from Normal against the dragon.

**Why a minimum of 1.** Without it, Easy could produce a Grimtusk who starts at 0 or below and counts as already beaten (pass clear, no fight, no Gold), a dragon beaten without landing a hit, and Monsters that can never hit or block. The simulation figures already assumed a minimum of 1.

**Why no maximum on Hard.** A maximum would add a second rule and change the approved ±2 amounts, and a maximum on every Difficulty would change Normal. The unbeatable-dragon case already exists on Normal and is recorded as L2.

**Why the Dragon moves.** It was created at the top of `StartGame`, before the Difficulty is chosen. Creating it in the choice `1` branch means both Monsters are created and adjusted in one place, and no Monster ever exists at an unchosen Difficulty. Each run plays at most one adventure (L4), so this creates one Dragon per run, as before. It still rolls d20 three times from the same shared, unseeded Die; only the order of rolls changes, not the odds.

**Why a shared method, not a constructor parameter.** A constructor parameter would change both constructors, and the result couldn't be tested exactly because the stats are rolled inside the constructor. A method applied after creation can be tested on fixed stats.

**`FixedDie` is dependency injection, not seeding.** `FixedDie` is a **test double**: a `Die` subclass that lives only in the test project and always returns one chosen value, capped at the number of sides. It reaches the game through the `Game(Die)` constructor. It is **not** a seeded random-number generator: `Die` still can't be seeded, and the game's own Die still uses unseeded randomness. This is a narrow exception to the "Die can't be seeded, use extreme-stat fixtures" rule, for the Game-level wiring tests only. All other tests keep the extreme-stat fixtures.

**No ADR.** Every decision here is easy to reverse, each in one place: the approach, the amounts, the minimum, the missing maximum and the Die seam. Only the A-vs-B trade-off is a real trade-off, and it is recorded here. ADR 0001 stays unchanged.

**Delivery plan (three tickets, in order).** Each ticket must compile and pass the full existing test suite before it is complete. Each is committed separately, after user review.

| # | Ticket | Contents | Blocked by |
|---|---|---|---|
| 01 | Monsters respond to Difficulty | `Difficulty`, the Monster method with the minimum, its tests, the glossary entry (R27) | none |
| 02 | Choose a difficulty when creating a character | Parsing and prompt methods, the prompt called in main-menu choice `1` before character creation, `Player.Difficulty` set from it, the Character Summary line, the 6 keys in 4 languages, the key-count rename and the Difficulty key test | 01 |
| 03 | The chosen difficulty shapes both fights | Move the Dragon's creation, apply the Difficulty to both Monsters, `virtual` `Die.Roll`, `Game(Die)`, `FixedDie` and the scripted wiring runs | 01, 02 |

After Ticket 02 the prompt and summary line work, but the chosen Difficulty does **not** yet change combat. **The feature is incomplete until Ticket 03.** Nothing is pushed until all three tickets are reviewed and committed.

## Testing Decisions

**What makes a good test here.** Same rules as the Merchant and Troll Encounter features:
- Test observable behaviour: return values, stats after an action, and console output.
- Look messages up through `Messages.GetMessage`, formatted where needed, instead of hard-coding English.
- Console tests save and restore `Console.In` and `Console.Out`.
- No test may fail by chance.

**Seam 1: the Monster Difficulty method, plain values, no console.** (Ticket 01)
- A Dragon and a Troll set to fixed stats (for example 10/10/10): Easy gives 8/8/8, Normal 10/10/10, Hard 12/12/12.
- The minimum: a Monster set to 2/1/1 on Easy gives 1/1/1, and one at 3 gives 1 on that stat.
- Weapon and its maximum damage are unchanged by every Difficulty.
- **Combat stats display (AC6):** a Monster at 10/10/10 with Hard applied, passed to `Combat`, makes the existing public `Combat.GetCombatStatsDisplay()` show Strength, Agility and HealthPoints 12 in the Monster's section. The assertion that the display contains no `stats_difficulty` text is added to this test in Ticket 02, because that key doesn't exist until then.
- Prior art: `DragonTests`, `TrollTests`, `CombatTests`.

**Seam 2: the parsing method, plain values.** (Ticket 02)
- `1`, `2`, `3` → Easy, Normal, Hard; ` 2 ` with spaces → Normal.
- `0`, `4`, `easy`, `Easy`, an empty string, whitespace and null → not valid.
- Prior art: `GameTests` (`IsValidMenuChoice`, `IsValidPathChoice`).

**Seam 3: the prompt method, typed-in console input and captured output.** (Ticket 02)
- An invalid entry then `3` returns Hard, and `invalid` appears exactly once.
- End of input returns Normal and prints `difficulty_auto_normal`, without looping.
- The prompt shows `difficulty_prompt` followed by `enter_choice`.
- At least one run in Spanish, French or Italian checks the translated prompt.
- Prior art: the Merchant Encounter and Troll Encounter console tests.

**Seam 4: the Character Summary.** (Ticket 02)
- The translated `stats_difficulty` line with the translated name appears on the line immediately after the Gold line and immediately before the Weapon line (the Player in the test has a weapon), for each of Easy, Normal and Hard.
- A new Player defaults to Normal.
- Prior art: `PlayerTests`.

**Seam 5: full scripted adventures with a fixed Die.** (Ticket 03)
- `FixedDie` is a test-only `Die` subclass passed to `Game(Die)` (see Implementation Decisions). It returns one chosen value, capped at the number of sides.
- Each run scripts: the language, main-menu `1`, the Difficulty, full character creation, then West and retreat at the Troll Encounter prompt, then North and retreat at the Dragon Encounter prompt, which ends the adventure. No combat takes place.
- **Each Monster is checked individually.** The test finds Grimtusk's stats block by `troll_stats_header` and Smolderfang's by `dragon_stats_header`, and asserts the `stats_strength`, `stats_agility` and `stats_health` lines **inside that block**. It never searches for a number anywhere in the output, so the player's own stats, or the other Monster's block, can't satisfy the assertion.
- **No label on the Monster stats screens (AC6):** each run also asserts that neither Monster's stats block contains the `stats_difficulty` text.
- **Adjusted only once (AC8):** run 1 (Hard) visits West **twice**, retreating at the Troll Encounter prompt each time, before going North. Grimtusk's block must show 12 / 12 / 12 on **both** visits, proving the Difficulty is not applied again on a later visit. The other runs visit West once.
- The four runs:

  | Run | Die value | Difficulty | Grimtusk | Smolderfang |
  |---|---|---|---|---|
  | 1 | 10 | Hard | 12 / 12 / 12 | 12 / 12 / 12 |
  | 2 | 10 | Normal | 10 / 10 / 10 | 10 / 10 / 10 |
  | 3 | 10 | Easy | 8 / 8 / 8 | 8 / 8 / 8 |
  | 4 | 1 | Easy | 1 / 1 / 1 | 1 / 1 / 1 |

  (Grimtusk's d10 Agility also rolls 10 with a die value of 10.)
- The runs also confirm that the difficulty prompt appears before the race prompt (AC1).
- These are the first tests that run `StartGame` end to end. Their scripted input must be complete, so the game reaches its own ending and never waits on an exhausted input (L1).
- Prior art: the console-capture helpers in the Merchant and Troll Encounter tests.

**Seam 6: LanguageDataTests (existing).** (Ticket 02)
- **Picked up automatically:** the parity, placeholder, map-structure and format-validity tests cover the new keys.
- **Renamed:** `Language_HasAll110DictionaryKeys` becomes `Language_HasAll116DictionaryKeys`. This is reported as a changed test, not a new one.
- **New:** a `DifficultyKeys` list, following the `TrollEncounterKeys` precedent, with a test that all 6 keys exist and are non-empty in all four languages.

**Existing tests.** Every other existing test stays unchanged and passing; together they show that Normal and every other part of the game behave as before (AC10). In particular, `NewDragon_InitializesRequiredProperties` and the Troll range tests still hold, because the constructors don't change. `DieTests` stays unchanged.

**Focused diff review (AC5).** Before each ticket is committed, and once more over the whole branch after Ticket 03, the diff is reviewed to confirm that Difficulty does **not** alter:
- the player's stat generation (character creation rolls and `Player`'s own Die),
- Gold: starting Gold and Grimtusk's Gold reward,
- Merchant prices and Items,
- weapon damage, for the player and for both Monsters.

No Die is injected into `Player` for this; the review and the existing tests are the check.

**Regression:** `dotnet test` reports 345 + N passed, 0 failed, 0 skipped, and `dotnet build` reports 0 warnings, after each ticket.

## Known Limitations

These come from existing, deliberately unchanged behaviour, or from the approved rules. Fixing them is out of scope.

- **L1. Other prompts still loop forever on end of input.** This is the Category A limitation L1, unchanged. Only the new difficulty prompt handles end of input (R5). After it auto-selects Normal, the race prompt that follows still loops if input has run out, as do the other character-creation prompts, the main menu, the Crossroads and the Dragon Encounter prompt.
- **L2. Some Hard dragons can't be beaten.** A hit is blocked when a d20 roll is at or below the defender's Agility, so a Monster with Agility 20 or more blocks every hit. A fight against it ends only in defeat or retreat. This already happens to 1 dragon in 20 on Normal (Agility roll of 20). On Hard it happens to 3 in 20 (rolls of 18–20 become 20–22). Grimtusk's highest Hard Agility is 12, so he isn't affected. The simulation's Hard win rates already include these dragons.
- **L3. Easy is not a pure −2 shift at the bottom.** Because of the minimum of 1, low rolls collapse onto 1: on Easy, Grimtusk's HealthPoints are 1 three times in twelve.
- **L4. One adventure per run.** North, Exit and a defeat all end the program, so a new adventure, and a new Difficulty choice, means running the game again.
- **L5. Some characters can never hit.** About 3% of new characters start with Strength 0 or below (Halfling, d20 − d4), and can never land a hit on any Difficulty. This is existing behaviour; Difficulty changes only the Monsters.

## Out of Scope

- Changing the player's stats, character creation rolls, starting Gold, Grimtusk's Gold reward, the Merchant, its Items or their prices on any Difficulty.
- Changing weapon damage, hit or block rules, the combat loop, or retreat behaviour.
- Accepting Difficulty names as words, in any language.
- Changing the Difficulty during an adventure, or remembering it between runs.
- A Difficulty label on the Monster stats screens or the combat stats display.
- A maximum on Hard Monster stats, or any fix for L1–L5.
- Fixing L1 at any prompt other than the new difficulty prompt.
- New Monsters, new paths, or a seventh language-data map; new languages.
- Renaming or rewording any existing message key.
- Editing the Category A spec or tickets, ADR 0001, or creating a new ADR.
- Committing or pushing as part of this spec. Each ticket is committed only after user review, and the branch is pushed only with explicit user approval.

## Further Notes

**Simulation (disposable, not in the repository).** Monte Carlo, 20,000 fights per case, fixed seed 4390, on the rules at `8217a67`. The player never retreats and buys nothing from the Merchant. Approach A uses a minimum of 1. Win % / average rounds:

| Case | vs Grimtusk | vs Smolderfang | Grimtusk → Dragon |
|---|---|---|---|
| **Normal (today)** | **83.6% / 11.9** | **54.6% / 21.1** | **49.4% / 26.3** |
| **A −2 (approved Easy)** | **89.1% / 9.8** | **67.9% / 13.2** | **63.8% / 17.9** |
| **A +2 (approved Hard)** | **75.8% / 11.7** | **41.7% / 31.8** | **35.0% / 36.0** |
| A −4 | 92.0% / 10.8 | 77.3% / 12.5 | 73.8% / 16.2 |
| A +4 | 65.0% / 13.4 | 28.9% / 41.4 | 23.7% / 49.0 |
| A HealthPoints only −4 | 88.0% / 9.0 | 61.0% / 16.4 | 56.4% / 23.5 |
| A HealthPoints only +4 | 79.9% / 12.8 | 48.0% / 22.7 | 42.8% / 28.2 |
| B Monster to-hit −3 | 88.2% / 17.1 | 61.8% / 35.2 | 58.3% / 38.6 |
| B Monster to-hit +3 | 79.8% / 11.1 | 49.2% / 18.1 | 43.6% / 22.1 |
| B player to-hit +3 | 89.0% / 4.9 | 59.3% / 16.7 | 54.8% / 18.5 |
| B player to-hit −3 | 74.6% / 23.7 | 46.4% / 31.6 | 41.9% / 37.9 |
| C Monster weapon −4 | 88.2% / 12.9 | 59.4% / 22.4 | 55.8% / 29.5 |
| C Monster weapon +4 | 79.9% / 10.4 | 50.9% / 20.4 | 45.9% / 24.4 |
| E A(−2) + C(−2) | 90.2% / 11.3 | 69.4% / 14.4 | 66.8% / 18.8 |
| E A(+2) + C(+2) | 72.7% / 11.8 | 39.6% / 31.3 | 33.0% / 35.3 |

- **Caveat on fight length.** 0.5–2% of fights stall because neither side can land damage, mostly when a character has Strength 0 or below. The simulation stopped those at 1,000 rounds and counted them as losses. That inflates the average-rounds column by several rounds; win rates move by about 1% at most.
- **The Grimtusk → Dragon chain** is roughly the product of the two fights, a few points below the dragon alone, because injuries carry over.

**Decision record.** Round 1: Q2 branch, Q3 prompt placement, Q4 Normal unchanged, Q5 approach and amounts decided after the exploration, Q6 fixed for the adventure, Q7 the term Difficulty, Q8 the simulation's scope, Q9 combat balance only, Q10 both Monsters. Round 2: Q11 approach A ±2, Q12 numbered prompt, Q13 automatic Normal with a message, Q14 Character Summary line. Round 3: Q15 Dragon timing, Q16 shared Monster method, Q17 minimum of 1, Q18 no maximum, Q19 `Player.Difficulty`, Q20 parse and prompt methods, Q21 keys, Q22 key-count test, Q23 test seams, Q24 acceptance criteria. Round 4: Q25 Die injection and the four scripted runs, Q26 AC12 and AC13, Q27 three tickets, Q28 glossary, Q29 no ADR, Q30 location. Final decisions: the revised AC3 wording, the Difficulty line placed after Gold and before Weapon, `FixedDie` described as dependency injection rather than seeding, and the branch. Spec review: the five added details approved; AC5 checked by existing tests and a focused diff review; a combat stats display test and no-label checks for AC6; the Hard run visits Grimtusk twice for AC8; the AC7 weapon wording; 0 build warnings kept in Testing Decisions, not added as an AC.
