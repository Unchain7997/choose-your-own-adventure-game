# 01: Turn the Dragon into a Monster

**Blocked by:** None (can start immediately)

**Status:** resolved

Spec: `.scratch/monster-encounter/spec.md` (R3, R19–R23; AC18, AC19 for the Dragon; seams 1 and 5)

## Scope

This is a refactor, so a player sees **no difference at all**. It makes the Troll Encounter easy to add. Combat stops being written for the dragon alone and works on any **Monster**.

- **A shared `Monster` base class** holds what every Monster has:
  - name, Strength, Agility, HealthPoints and weapon, all with public setters
  - the attack rules, the counter-attack against the player, random taunt and damage-reply selection, and the stats display
  - which messages that Monster uses: stats header, combat stats header, defeated narrative, player attack intro, 4 defence taunts and 4 damage replies
- **Message keys stay internal** to each Monster (protected). Other classes ask the Monster for its finished text. No public member is added only for tests.
- **`Dragon`** extends `Monster` with its **existing** name, d20 stats, claws and message keys. Its constructor and public members stay the same.
- **Combat and the player's attack** take a Monster. Combat's public `dragon` property is renamed `monster`, typed `Monster`.
- **Only one existing test changes:** the assertion in the Combat constructor test that reads that property now reads `monster`.
- The Dragon Encounter, North path, Merchant Encounter, Exit and main menu behave exactly as before. No message text or language data changes.
- **Glossary:** this ticket's commit includes the approved `GLOSSARY.md` changes from the `/grill-with-docs` interview (the new Monster, Troll Encounter and Crossroads entries, and the updated Merchant Encounter and Gold entries).

## Expected files affected

- **New:** `Monster.cs`
- **Modified:** `Dragon.cs`, `Combat.cs`, `Player.cs`, `GLOSSARY.md`
- **Tests:** `CYOATests/CombatTests.cs` (the one renamed assertion, plus new tests). New tests may also go in `CYOATests/DragonTests.cs`.
- **Not touched:** `Game.cs`, `Merchant.cs`, `Messages.cs`, `Die.cs`, `Weapon.cs`, `language_data.json`, ADR 0001

## TDD approach

1. Run the full suite and confirm the 262-test baseline.
2. **Characterisation tests (green before the refactor).** Pin down the dragon's visible combat lines with plain-value fixtures, so the move can't change them:
   - the combat stats display shows `combat_dragon_header`
   - the player's attack shows the formatted `player_attack_intro`
   - a killing blow shows `dragon_defeated_narrative` (player Strength 20, dragon Agility 0, HealthPoints 1)
   - a blocked attack shows one of the dragon's 4 taunts (player Strength 20, dragon Agility 20)
   - an unkilled hit shows one of its 4 damage replies (player Strength 20, dragon Agility 0, high HealthPoints)
   - the stats display shows `dragon_stats_header`

   Write only the ones the existing tests don't already cover.
3. **Red:**
   - a test that the Dragon is a Monster
   - a test that Combat's `monster` property returns the Dragon it was given

   Both fail to compile until the base class and the renamed property exist. Rename the one existing assertion here.
4. **Green:** add `Monster`, move the Dragon's shared behaviour up, and switch Combat and the player's attack to Monster. Do the smallest change that makes every test pass.
5. **Refactor**, with the whole suite green after each step.

Messages are looked up through `Messages.GetMessage`. Console tests save and restore `Console.In` and `Console.Out`. No test may depend on a lucky roll.

## Acceptance criteria

- [x] A shared Monster base class exists and the Dragon is a Monster. Combat and the player's attack accept any Monster.
- [x] Combat's public property is named `monster`, and it returns the Dragon it was given (AC19, Dragon part).
- [x] Combat against the dragon still shows `combat_dragon_header`, `player_attack_intro`, the dragon's own taunts and damage replies, and `dragon_defeated_narrative`. The stats display still shows `dragon_stats_header`.
- [x] The Dragon's constructor and public members are unchanged. Object-initializer setups in existing tests still compile.
- [x] Message keys aren't exposed as public members. No public member exists only for tests.
- [x] Only one existing test line changes: the Combat constructor assertion now reads `combat.monster` (R22).
- [x] No language data or message text changes. The key-count test still asserts 91.
- [x] `GLOSSARY.md` interview changes are included in this ticket's commit.
- [x] The Dragon Encounter, North path, Merchant Encounter, Exit and main menu are unchanged (AC18).

## Verification commands

```
dotnet build --no-incremental        # 0 warnings, 0 errors
dotnet test                          # 262 existing + new tests, 0 failed, 0 skipped
git diff 51c1941 -- docs/0001-add-italian-language-support.md   # prints nothing
git diff --stat                      # only the expected files
git diff -- CYOATests/               # only the one renamed assertion among existing test lines
```

## Definition of done

- Every acceptance criterion above is ticked, and `Status:` is set to `resolved`.
- Build is clean (0 warnings) and `dotnet test` shows 0 failed.
- A code review (standards and spec) has run, and its findings are reported.
- The user has reviewed the diff and approved the commit. Only approved files are staged, by explicit path. Nothing is pushed.
