# 01: Monsters respond to Difficulty

**What to build:** Any Monster (Grimtusk or Smolderfang) can be given a **Difficulty**: Easy, Normal or Hard. Easy makes its Strength, Agility and HealthPoints 2 lower, with a minimum of 1 for each. Normal leaves them unchanged. Hard makes them 2 higher, with no maximum. The adjusted numbers are what the Monster's stats screen and the combat stats display show. Nothing else about the Monster changes: its weapon, damage, attack rules and messages stay the same.

The player sees no difference yet, because nothing in the game applies a Difficulty until Ticket 03. This ticket is the "make the change easy" prefactor for Tickets 02 and 03.

**Blocked by:** None (can start immediately)

**Status:** resolved

Spec: `.scratch/difficulty-setting/spec.md` (R7–R11, R15, R16, R27; AC4, AC5 weapon part, AC6, AC10, AC11; Seam 1)

## Scope

- **A `Difficulty` enumeration** with the values `Easy`, `Normal` and `Hard`.
- **One shared public method on `Monster`** that applies a Difficulty: −2, 0 or +2 on Strength, Agility and HealthPoints, then raises any of the three below 1 to 1. It works for every Monster without per-subclass code.
- **Unchanged:** the `Dragon` and `Troll` constructors and their existing range tests, `Combat`, `Player`, `Game`, `Die`, `Merchant`, `Messages`, `Weapon`, the language data, ADR 0001 and the Category A documents.
- **Glossary:** add the approved new `## Adventure` section, placed after `## Encounters`, with the **Difficulty** entry worded exactly as in spec R27. It's committed with this ticket.

## Acceptance criteria

- [ ] A Dragon and a Troll each set to 10/10/10 show 8/8/8 after Easy, 10/10/10 after Normal and 12/12/12 after Hard (AC4).
- [ ] A Monster set to 2/1/1 shows 1/1/1 after Easy, and a stat of 3 becomes 1 (the minimum, R9).
- [ ] Hard has no maximum: a stat of 20 becomes 22.
- [ ] Weapon type and maximum damage are unchanged by every Difficulty (AC5, weapon part).
- [ ] A Monster at 10/10/10 with Hard applied makes the existing public `Combat.GetCombatStatsDisplay()` show Strength, Agility and HealthPoints 12 in the Monster's section, with labels looked up through `Messages.GetMessage` (AC6, adjusted numbers). The "no `stats_difficulty` label" assertion is added to this test in Ticket 02, because that key doesn't exist until then.
- [ ] `GLOSSARY.md` has the `## Adventure` section with the Difficulty entry exactly as approved, and no other glossary change.
- [ ] All existing tests pass unchanged: 345 + N passed, 0 failed, 0 skipped. `dotnet build` reports 0 warnings (AC10).
- [ ] `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing (AC11).
- [ ] A focused diff review confirms that nothing touches player-stat generation, Gold, Merchant prices or weapon damage (AC5).

## TDD approach

1. Run the full suite and confirm the 345-test baseline.
2. Red → green, one behaviour at a time, at Seam 1 (plain values, no console): Easy, Normal, Hard, the minimum, no maximum, weapon unchanged, then the combat stats display.
3. Use fixed stats set through the Monsters' public setters. No change to `Die`, and no test that can fail by chance.
4. Report honestly any test that passed without a red step.

## Review gate

Stop before committing. The user reviews the diff, including the glossary wording. Stage only the files the user approves, by explicit path. Do not push.
