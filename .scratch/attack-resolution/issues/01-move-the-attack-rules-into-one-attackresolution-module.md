# 01: Move the Attack rules into one AttackResolution module

**What to build:** The rules of an **Attack** (a d20 hit roll against the attacker's Strength, a d20 block roll against the defender's Agility, then a damage roll with the attacker's weapon, in that order and stopping as soon as the Attack misses or is blocked) live in one shared module, `AttackResolution`, which reports the result: Miss, Blocked, or Hit with its damage. The player's Attack and the Monster's Attack both use it, and each keeps everything that makes it different: its messages, taunts, damage replies, ASCII art and how it changes HealthPoints. A player sees, reads and rolls exactly what they do today.

**Blocked by:** None (can start immediately)

**Status:** resolved

Spec: `.scratch/attack-resolution/spec.md` (R1–R12; AC1–AC12; Testing Decisions Seams 1 and 2; Known Limitations L1–L3)

## Scope

- **New:** `AttackResolution` (static) with exactly two public members, `Resolve` and `Hits`; the block rule is private. `AttackResult` holding an `AttackOutcome` (`Miss`, `Blocked`, `Hit`) and a `Damage` (0 unless Hit). `Resolve` makes no input checks and changes nothing but the Die's rolls (R1–R5).
- **Modified:** `Player.Attack` and `Monster.Attack` call `Resolve` once with the attacker's own Die and branch on the result. Each applies damage itself: no clamp on Monster HealthPoints, a clamp at 0 on the Player's. Every difference in the spec's R8 table is preserved, including the unarmed player's English line with no rolls, and the Monster Die rolls for taunts and damage replies after the Attack is resolved (R6–R8).
- **Modified:** `AttackHits` on `Player` and `Monster` delegates to `AttackResolution.Hits`. `RollAttack` and `RollDamage` stay public and unchanged (R9).
- **Glossary:** the new `## Combat` section with the **Attack** entry exactly as in spec R12.
- **New test files only:** `ScriptedDie`, `AttackResolutionTests`, `MonsterAttackCharacterizationTests`, each in its own file in the test project.
- **Unchanged:** every existing test file, `Die`, `Combat`, `Game`, `Dragon`, `Troll`, `Weapon`, `Messages`, `Merchant`, the program entry point, the language data, ADR 0001, and the Merchant, Category A and Category B documents (R10, R11).

## Implementation steps (test-first, in this order)

1. **Baseline.** Run `dotnet test` and `dotnet build`; confirm 398 passed, 0 failed, 0 skipped, and 0 warnings.
2. **Add `ScriptedDie`.** A test-only `Die` subclass that returns a queued list of values in order, records the number of sides each roll asked for, and throws if the code rolls more often than scripted. Dependency injection, not seeding.
3. **Characterize `Monster.Attack` first (Seam 1).** Before any production change, write tests with a Monster built on a `ScriptedDie` attacking a Player, for: a miss; a block; a hit that leaves the Player above 0; a hit that drops the Player to 0 (the clamp). Each asserts every output line (through `Messages.GetMessage`), the Player's HealthPoints afterwards, and the complete recorded roll sequence (count, sides and order). **Run them and confirm they pass on the unchanged production code.** Report them as characterization tests that are green by design.
4. **Write the `AttackResolution` tests (Seam 2) and confirm they fail for the expected reason**: the module and its types don't exist yet, so the test project doesn't compile. Report that red step honestly.
5. **Implement the module and update both callers.** Add `AttackResolution`, `AttackResult` and `AttackOutcome`; switch `Player.Attack` and `Monster.Attack` to `Resolve`; point both `AttackHits` at `Hits`. Keep the roll order and every R8 difference exactly.
6. **Add the glossary entry** (R12).
7. **Run the complete test suite, build and regression checks** (the acceptance criteria below). The characterization tests must still pass, unchanged.

## Acceptance criteria

**The shared module**
- [ ] With scripted rolls, `Resolve` returns Miss, Blocked or Hit according to the hit and block rolls (AC1).
- [ ] A hit roll equal to Strength hits; a block roll equal to Agility blocks; Strength 0 or less never hits; Agility 20 or more always blocks (AC2).
- [ ] A Miss rolls exactly `[20]`, a Block exactly `[20, 20]`, a Hit exactly `[20, 20, weapon maximum]`, in that order, with no other rolls (AC3).
- [ ] On a Hit, Damage equals the damage roll; on a Miss or a Block, Damage is 0 (AC4).
- [ ] `Hits(roll, strength)` is true exactly when `roll ≤ strength` (AC5).

**The callers**
- [ ] The `Monster.Attack` characterization tests pass before the production change and pass unchanged after it: identical output, final Player HealthPoints and complete roll sequence for the miss, block, hit and clamp cases (AC6).
- [ ] `Player.AttackHits`, `Monster.AttackHits`, `RollAttack` and `RollDamage` remain public and behave as before (AC7).

**Regression and documents**
- [ ] All 398 existing tests pass, no existing test file is modified, and exactly the three approved new test files are added (AC8):
  - before the commit, `git status --porcelain --untracked-files=all -- CYOATests` lists exactly `CYOATests/ScriptedDie.cs`, `CYOATests/AttackResolutionTests.cs` and `CYOATests/MonsterAttackCharacterizationTests.cs`, each as `??` or `A `, and no `M`, `D` or `R` entry;
  - after the commit, `git diff --name-status 1e65e1f HEAD -- CYOATests` lists exactly those three files, each `A`, and nothing else.
- [ ] `dotnet test` reports 398 + N passed, 0 failed, 0 skipped; `dotnet build` reports 0 warnings, 0 errors (AC9).
- [ ] `git diff 1e65e1f -- language_data.json` prints nothing (AC10).
- [ ] `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing, and the `.scratch/merchant-encounter`, `.scratch/monster-encounter` and `.scratch/difficulty-setting` documents are unchanged (AC11).
- [ ] `GLOSSARY.md` has the `## Combat` section with the Attack entry exactly as approved, and no other glossary change (AC12).
- [ ] A focused diff review confirms that no gameplay, message, translation, combat rule, Difficulty amount, Gold reward or Merchant behaviour changed (R10).

## Notes

- **Player side (L1).** The Player's Die is private, so `Player.Attack` can't be driven by `ScriptedDie`. It is covered by the module's own tests plus the existing extreme-stat tests through `Player.Attack`. Making the Player's Die injectable is out of scope (architecture review Candidate 4).
- **Don't run the game by hand** without `timeout` and an output cap; afterwards, check that no `dotnet` process is left running.
- **Report honestly** any test that passed without a red step.

## Review gate

Stop before staging or committing. The user reviews the diff. Stage only the files the user approves, by explicit path, for one commit. Do not push without the user's explicit approval.
