# 03: The chosen difficulty shapes both fights

**What to build:** The Difficulty the player chooses actually changes both fights. Grimtusk and Smolderfang are created **after** the Difficulty is chosen, and each is adjusted once, before the player can meet it. On Easy their stats show 2 lower (minimum 1), on Normal as rolled, and on Hard 2 higher. The adjustment lasts for the whole adventure: going back to Grimtusk after a retreat shows the same adjusted stats, not a second adjustment. Normal still plays exactly like today's game. This completes the feature.

**Blocked by:** 01 (Monsters respond to Difficulty), 02 (Choose a difficulty when creating a character)

**Status:** resolved

Spec: `.scratch/difficulty-setting/spec.md` (R8, R10, R12, R14, R17, R20; AC1 order, AC4, AC5, AC6, AC8, AC10–AC13; Seam 5; the "FixedDie is dependency injection, not seeding" decision)

## Scope

- **`Game`:**
  - The Dragon is created in main-menu choice `1`, after the Difficulty is chosen and next to Grimtusk, using the shared Game Die. It's no longer created at the top of `StartGame`.
  - Right after each Monster is created, the Player's chosen Difficulty is applied to it with the Ticket 01 method, so both are adjusted before they can be met.
  - A constructor that takes a `Die`, which `StartGame` uses for the Monsters. The parameterless constructor keeps creating a new `Die`, so the program entry point is unchanged.
- **`Die`:** `Roll` becomes `virtual`. Its behaviour is unchanged, and `DieTests` doesn't change.
- **Test project only:** `FixedDie`, a `Die` subclass that always returns one chosen value, capped at the number of sides. It's a test double passed in through the `Game(Die)` constructor, **not** a seeded random-number generator.
- **Unchanged:** `Player` and its own Die, character creation, `Monster`, `Dragon` and `Troll` constructors, `Combat`, `Merchant`, `Messages`, `Weapon`, the language data, the program entry point, ADR 0001, the glossary and the Category A documents.

## Acceptance criteria

Seam 5: full scripted `StartGame` runs with `FixedDie`. Each run scripts the language, main-menu `1`, the Difficulty, full character creation, then West and retreat at the Troll Encounter prompt, then North and retreat at the Dragon Encounter prompt, which ends the adventure. No combat takes place. The scripted input must be complete, so the game reaches its own ending and never waits on exhausted input (L1).

- [ ] **Run 1:** die value 10, Hard. Grimtusk shows 12 / 12 / 12 and Smolderfang shows 12 / 12 / 12. This run visits West **twice**, retreating each time, and Grimtusk shows 12 / 12 / 12 on **both** visits (AC8).
- [ ] **Run 2:** die value 10, Normal. Both show 10 / 10 / 10.
- [ ] **Run 3:** die value 10, Easy. Both show 8 / 8 / 8.
- [ ] **Run 4:** die value 1, Easy. Both show 1 / 1 / 1 (the minimum).
- [ ] **Each Monster is checked individually** (AC12). Grimtusk's block is found by `troll_stats_header` and Smolderfang's by `dragon_stats_header`, and the `stats_strength`, `stats_agility` and `stats_health` lines are asserted **inside that block**. The tests never search for a number anywhere in the output.
- [ ] Neither Monster's stats block contains the `stats_difficulty` text (AC6).
- [ ] The difficulty prompt appears before the race prompt (AC1).
- [ ] The parameterless `Game()` and the program entry point behave as before; the only change to `Die.Roll` is `virtual` (AC13).
- [ ] All other existing tests pass unchanged: the previous total + N passed, 0 failed, 0 skipped. `dotnet build` reports 0 warnings (AC10).
- [ ] `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing (AC11).
- [ ] **A focused diff review over the whole branch** (all three tickets) confirms that Difficulty doesn't alter player-stat generation, starting Gold, Grimtusk's Gold reward, Merchant prices or Items, or weapon damage for the player or either Monster (AC5).

## TDD approach

1. Run the full suite and confirm the post-Ticket-02 baseline.
2. Write the Hard run first and watch it fail: without the wiring, the Monsters show 10s. Then move the Dragon, apply the Difficulty, and add the Die seam to make it pass.
3. Add the Normal, Easy and minimum runs, then the second West visit.
4. Console tests save and restore `Console.In` and `Console.Out`. Look up every message through `Messages.GetMessage`.
5. If the game is ever run by hand, use `timeout` and an output cap, then check that no `dotnet` process is left running.
6. Report honestly any test that passed without a red step.

## Review gate

Stop before committing. The user reviews the diff. Stage only the files the user approves, by explicit path. **Push only with the user's explicit approval**, after all three tickets are committed.
