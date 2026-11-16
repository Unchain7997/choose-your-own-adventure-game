# Spec: One attack-resolution module

Status: resolved

Feature slug: `attack-resolution`
Branch: `refactor/attack-resolution` (from `feature/difficulty-setting` at `1e65e1f`)
Baseline: 398 passing tests, 0 failing, 0 skipped; `dotnet build` 0 warnings, 0 errors (at `1e65e1f`)
Origin: Candidate 1 of the `/improve-codebase-architecture` review (2026-11-16), then a two-round interview (Q1–Q14). Glossary terms used here are defined in `GLOSSARY.md`: Monster, Troll Encounter, Dragon Encounter. **Attack** is added to the glossary by this work (R12).

Course context: CIDM4390 AI Agentic Coding Part 4, the separate architecture-refactoring step after Category B.

**This is a refactor. Nothing a player sees, reads or rolls changes.**

## Problem Statement

The rules of an Attack (a hit roll against the attacker's Strength, a block roll against the defender's Agility, then a damage roll with the attacker's weapon) are written twice: once in the player's attack and once in the Monster's attack. Each copy is tangled with that side's message building, so the rules can only be tested indirectly, through extreme-stat fixtures and the console text they produce. A change to the combat rules would have to be made, and tested, in two places, and the two copies could drift apart without anything noticing. This duplication is also why Category B rejected changing the hit and block checks (approach B).

## Solution

One shared module, **AttackResolution**, owns the rules of an Attack. It takes the attacker's Die, the attacker's Strength, the defender's Agility and the attacker's weapon maximum, rolls exactly as the game does today, and reports what happened: a miss, a block, or a hit with its damage. The player's attack and the Monster's attack keep everything that makes them different (their messages, taunts, damage replies, ASCII art and how they change HealthPoints) and simply act on that report.

The rules then live and are tested in one place. A test can script every roll and check the outcome, how many rolls were made, and in what order.

## Requirements

**The shared module**

- **R1.** A new static module, `AttackResolution`, with exactly two public members:
  - **`Resolve`**: takes the attacker's Die, the attacker's Strength, the defender's Agility and the attacker's weapon maximum damage, and returns an `AttackResult`.
  - **`Hits`**: takes a roll and a Strength and returns whether that roll hits (`roll ≤ Strength`).
  The block rule (`roll ≤ Agility`) is private to the module.
- **R2.** `AttackResult` holds an `AttackOutcome` (`Miss`, `Blocked` or `Hit`) and a `Damage`. Damage is the rolled damage on a Hit and 0 otherwise.
- **R3.** `Resolve` rolls in this fixed order, stopping where today's code stops:
  1. a d20 hit roll. If it doesn't hit, the result is Miss, after **1** roll;
  2. a d20 block roll. If it is at or below the defender's Agility, the result is Blocked, after **2** rolls;
  3. a damage roll with the weapon maximum as the number of sides. The result is Hit with that damage, after **3** rolls.
- **R4.** `Resolve` is pure apart from rolling the Die it is given. It changes no HealthPoints, builds no messages and reads nothing else.
- **R5.** `Resolve` performs **no input checks**. Every input, including a weapon maximum of 0 or less and Strength or Agility outside 1–20, reaches the Die and the comparisons exactly as today.

**The callers**

- **R6.** `Player.Attack` and `Monster.Attack` each call `Resolve` once, with the **attacker's own Die** (the Player's private Die; the Monster's Die, which is the shared Game Die), and branch on the result. They keep all of their own message building.
- **R7.** Each caller applies the damage itself, as today:
  - `Player.Attack` subtracts the damage from the Monster's HealthPoints with **no clamp** (Monster HealthPoints can show negative, Category A L3).
  - `Monster.Attack` subtracts the damage from the Player's HealthPoints and **clamps at 0**.
- **R8.** Every existing difference between the two attacks is preserved exactly:

  | | Player attacks a Monster | Monster attacks the Player |
  |---|---|---|
  | No weapon | Returns the hard-coded English line "You have no weapon to attack with." with **no rolls** and no call to `Resolve` | Not possible |
  | Intro | The Monster's player-attack intro | `dragon_attack_intro` |
  | ASCII art | Always added, even if empty | Added only if not blank |
  | Miss / hit lines | `player_missed_dragon` / `player_hit_dragon` | `dragon_missed_player` / `dragon_hit_player` |
  | Block | `dragon_defended_player_attack`, then a taunt | `player_defended_dragon_attack` |
  | Damage lines | `player_damage_dealt`, `dragon_health_now` | `dragon_damage_dealt`, `player_health_now` |
  | HealthPoints | No clamp | Clamped at 0 |
  | After damage | Defeated narrative, or a damage reply | Defeated narrative at 0 |
  | Extra rolls | The taunt and damage reply each roll the **Monster's** Die, after the Attack is resolved | None |

- **R9.** `RollAttack`, `AttackHits` and `RollDamage` stay public on both `Player` and `Monster`, with the same behaviour. `AttackHits` delegates to `AttackResolution.Hits`, so the hit rule is written once. `RollAttack` and `RollDamage` are unchanged; the attacks simply stop calling them.

**Unchanged**

- **R10.** All gameplay, messages, taunts, damage replies, translations, the language data, combat rules, the combat loop, Difficulty mechanics, Gold rewards, the Merchant and every encounter behave exactly as before.
- **R11.** `Die`, `Combat`, `Game`, `Dragon`, `Troll`, `Weapon`, `Messages`, `Merchant` and the program entry point are not modified. ADR 0001, the Merchant, Category A and Category B documents are not edited, and no new ADR is created.

**Glossary**

- **R12.** `GLOSSARY.md` gains a new `## Combat` section with this entry (approved in Q5):

  > **Attack**:
  > One strike in combat, by the player or a Monster: a hit roll against the attacker's Strength, a block roll against the defender's Agility, then a damage roll with the attacker's weapon.
  > _Avoid_: Swing, turn, round (a round is the player's Attack plus the Monster's)

## Acceptance Criteria

**The shared module**

- **AC1.** With scripted rolls, `Resolve` returns Miss when the hit roll is above Strength, Blocked when the hit roll is at or below Strength and the block roll is at or below Agility, and Hit when the block roll is above Agility.
- **AC2.** Boundaries: a hit roll **equal** to Strength hits, and a block roll **equal** to Agility blocks. Strength 0 or less never hits; Agility 20 or more always blocks.
- **AC3.** A Miss makes exactly 1 roll of 20 sides; a Block makes exactly 2 rolls of 20 sides; a Hit makes exactly 3 rolls, of 20, 20 and the weapon maximum sides, in that order. No other rolls are made.
- **AC4.** On a Hit, Damage equals the damage roll. On a Miss or a Block, Damage is 0.
- **AC5.** `Hits(roll, strength)` is true exactly when `roll ≤ strength`.

**The callers**

- **AC6.** `Monster.Attack`'s output, rolls and the Player's HealthPoints are identical before and after the refactor for a miss, a block, a hit that leaves the Player above 0, and a hit that drops the Player to 0 (the clamp). Checked by the characterization tests, which pass on the code at `1e65e1f` before any production change and pass unchanged after.
- **AC7.** `Player.AttackHits`, `Monster.AttackHits`, `RollAttack` and `RollDamage` remain public and behave as before.

**Regression and documents**

- **AC8.** All 398 existing tests pass, **no existing test file is modified**, and **exactly three new test files are added**: `CYOATests/ScriptedDie.cs`, `CYOATests/AttackResolutionTests.cs` and `CYOATests/MonsterAttackCharacterizationTests.cs`. Verified as follows (`git diff` alone doesn't list untracked files):
  - **Before the commit:** `git status --porcelain --untracked-files=all -- CYOATests` lists exactly those three files, each as `??` (untracked) or `A ` (staged as added), and no other entry: no `M`, `D` or `R` on any file.
  - **After the commit:** `git diff --name-status 1e65e1f HEAD -- CYOATests` lists exactly those three files, each with status `A`, and nothing else.
- **AC9.** `dotnet test` reports 398 + N passed, 0 failed, 0 skipped; `dotnet build` reports 0 warnings, 0 errors.
- **AC10.** The language data is unchanged: `git diff 1e65e1f -- language_data.json` prints nothing.
- **AC11.** ADR 0001 is byte-for-byte unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing), and the `.scratch/merchant-encounter`, `.scratch/monster-encounter` and `.scratch/difficulty-setting` documents are unchanged.
- **AC12.** `GLOSSARY.md` has the `## Combat` section with the Attack entry exactly as approved, and no other glossary change.

## User Stories

1. As a player, I want every fight to play exactly as before, so that the refactor never changes my game.
2. As a player, I want the same hit, block and damage chances against Grimtusk and Smolderfang, so that each Difficulty keeps its balance.
3. As a player, I want the same messages, taunts and damage replies in every language, so that nothing I read changes.
4. As a player, I want my HealthPoints never to show below 0, as today.
5. As a developer, I want the rules of an Attack in one module, so that a change to them is made once.
6. As a developer, I want that module to report an outcome instead of printing, so that I can test the rules without reading console text.
7. As a developer, I want to script every roll in a test, so that every outcome and boundary is checked exactly, never by chance.
8. As a developer, I want tests that prove how many rolls an Attack makes and in what order, so that a refactor can't silently shift the dice.
9. As a developer, I want the player's attack and the Monster's attack to keep their own messages, so that each side still sounds like itself.
10. As a developer, I want the existing roll helpers kept, so that nothing that uses them breaks.
11. As a developer, I want the hit rule written once, even behind `AttackHits`, so that the duplication doesn't come back.
12. As a maintainer, I want the Monster's attack output pinned by tests before any change, so that I can prove it didn't move.
13. As a maintainer, I want the existing 398 tests left untouched, so that they prove the behaviour is unchanged.
14. As a maintainer, I want the term **Attack** in the glossary, so that the module's name means one agreed thing.
15. As a reviewer, I want the refactor in one small, reviewed commit, so that it is easy to inspect.
16. As a future developer, I want the module to be the single place combat-rule changes go, so that a future difficulty or Monster change has one place to edit.

## Implementation Decisions

**Modules**

- **`AttackResolution` (new, static).** The deep module: two public members (`Resolve`, `Hits`) over the hit, block and damage rules and their roll order. The block rule is a private detail.
- **`AttackResult` and `AttackOutcome` (new).** A small value holding the outcome (`Miss`, `Blocked`, `Hit`) and the damage (0 unless Hit). Chosen over three separate result types: less for each caller to learn.
- **`Player` (modified).** `Attack` calls `Resolve` with the Player's Die, Strength, the Monster's Agility and the weapon maximum, after the existing no-weapon check, then builds the same lines as today and subtracts damage from the Monster without a clamp. `AttackHits` delegates to `AttackResolution.Hits`. `RollAttack` and `RollDamage` are unchanged.
- **`Monster` (modified).** `Attack` calls `Resolve` with the Monster's Die, Strength, the Player's Agility and the weapon maximum, then builds the same lines as today and subtracts damage from the Player with the clamp at 0. `AttackHits` delegates to `AttackResolution.Hits`. `RollAttack` and `RollDamage` are unchanged.
- **Unchanged:** see R11.

**Why the callers apply the damage.** Letting the module apply damage would need a shared "thing with HealthPoints" interface and a clamp flag, widening its interface only to carry one existing difference. Keeping HealthPoints changes in each caller keeps the module pure and the difference visible where it lives.

**Why no input checks.** Checks would quietly change behaviour for out-of-range values, which the existing extreme-stat tests rely on (Strength 0 or 20, Agility 0 or 20).

**Why keep the roll helpers.** Tests use `AttackHits` and `RollDamage`, and public functionality must be preserved. Delegating `AttackHits` to the module keeps the hit rule in one place.

**Dependency category:** in-process. The module depends only on `Die`, which already has two adapters: the real `Die` and the test-only `FixedDie` from Category B. This work adds a third test adapter, `ScriptedDie`.

**No ADR.** The change is small, easy to reverse and unsurprising. ADR 0001 is untouched.

**Delivery plan.** One ticket, implemented test-first, reviewed by the user, then one commit. Nothing is pushed without the user's explicit approval.

## Testing Decisions

**What makes a good test here.** Same rules as the earlier features: test observable behaviour (return values, HealthPoints, console output); look messages up through `Messages.GetMessage`; console tests save and restore `Console.In` and `Console.Out`; no test may fail by chance.

**`ScriptedDie` (new test double).** A test-only `Die` subclass that returns a queued list of values in order, records the number of sides each roll asked for, and **throws** if the code rolls more often than scripted. It reaches the code under test through the existing seams: `Resolve`'s Die parameter and the Monster constructor. Like `FixedDie`, it is dependency injection, not seeding.

**Seam 1: the Monster attack, characterized first.** (`MonsterAttackCharacterizationTests`)
- Written and green on the code at `1e65e1f` **before any production change**, then left unchanged.
- A Monster built with a `ScriptedDie`, attacking a Player, for: a miss; a block; a hit that leaves the Player above 0; a hit that drops the Player to 0. Each test asserts the exact output (every line, through `Messages.GetMessage`), the Player's HealthPoints afterwards, and the recorded rolls (count, sides and order).
- Prior art: `DragonTests` and `TrollTests` combat-text tests; `FixedDie` from Category B.

**Seam 2: the shared module, plain values.** (`AttackResolutionTests`)
- AC1–AC5: each outcome; the equal-roll boundaries; Strength 0 never hits and Agility 20 always blocks; the roll count, sides and order for each outcome; Damage equal to the damage roll on a Hit and 0 otherwise; `Hits`.
- Red first: these tests are written before the module exists.
- Prior art: `DifficultyTests` (plain values, no console).

**Existing tests.** All 398 stay untouched and passing. Together they cover the player's attack through extreme-stat fixtures and every encounter end to end, which is the proof that nothing changed (AC8).

**New test files only.** `ScriptedDie`, `AttackResolutionTests` and `MonsterAttackCharacterizationTests`, each in its own new file. No existing test file is edited, not even to add tests.

**Regression:** after the change, `dotnet test` reports 398 + N passed, 0 failed, 0 skipped, and `dotnet build` reports 0 warnings.

## Known Limitations

- **L1. The player's rolls can't be scripted.** The Player creates its own private Die, so `Player.Attack` can't be driven by `ScriptedDie`. Its side of the refactor is covered by the shared module's own tests plus the existing extreme-stat tests through `Player.Attack`, which fix the outcome whatever the dice roll. Making the Player's Die injectable is Candidate 4 of the architecture review and is out of scope.
- **L2. Monster HealthPoints can show negative** after a killing blow (Category A L3). Preserved deliberately.
- **L3. The unarmed player's line is hard-coded English** and isn't translated. Preserved deliberately; it can't occur in normal play, because every character gets a weapon.

## Out of Scope

- Candidates 2–5 of the architecture review: a single combat outcome, one Monster Encounter module, an injectable Player Die, and a console seam.
- Any change to combat rules, Difficulty amounts, messages, translations or the language data.
- Renaming the generic `dragon_*` / `player_*` message keys.
- Clamping Monster HealthPoints, or translating the unarmed player's line.
- Changing `Die`, `Combat`, `Game`, `Dragon`, `Troll`, `Weapon`, `Messages`, `Merchant` or the program entry point.
- Editing any existing test file, ADR 0001, or the Merchant, Category A or Category B documents; creating an ADR.
- Committing or pushing as part of this spec. The single commit is made only after user review, and nothing is pushed without explicit approval.

## Further Notes

- **Origin.** The `/improve-codebase-architecture` review (report at `%TEMP%\architecture-review-20261116.html`, outside the repository) found five candidates. Candidate 1 was chosen as the right size for the assignment: it removes real duplication of the combat rules, keeps gameplay identical when the roll order is preserved, and is exactly testable with Category B's Die seam.
- **Decision record.** Round 1: Q1 a static, pure `Resolve` returning an outcome; Q2 callers apply damage and keep their clamp behaviour; Q3 keep the six public roll helpers, `AttackHits` delegating; Q4 add `ScriptedDie`; Q5 name `AttackResolution` and the glossary entry Attack; Q6 push Category B first, then branch `refactor/attack-resolution` from `1e65e1f` (done); Q7 one spec, one ticket, one commit; Q8 no edits to existing tests. Round 2: Q9 `AttackResult(AttackOutcome, Damage)`; Q10 no input checks; Q11 `Resolve` and `Hits` public, the block rule private; Q12 `Monster.Attack` characterization tests first, and the player-side limit recorded; Q13 push only `feature/difficulty-setting` to `origin`, with no PR (done, `1e65e1f`); Q14 three new test files.
