# 03: Fight Grimtusk

**Blocked by:** 02 (Meet Grimtusk at the West pass and retreat)

**Status:** resolved

Spec: `.scratch/monster-encounter/spec.md` (R7–R9, R11, R14–R16, R20, R25, R28; AC6, AC7, AC9, AC12–AC14, AC19 for Grimtusk, AC21, AC23 complete; seams 1, 2, 4, 5)

## Scope

Choosing `a` or `attack` at the Troll Encounter prompt now starts combat against Grimtusk. This **removes Ticket 02's temporary "fight not ready" notice**, including its `troll_fight_not_ready` key in all four languages.

**Combat** has the same rules and round structure as the dragon fight, but Grimtusk speaks his own lines:
- his combat stats header (`--- Troll ---`)
- his attack intro ("…and attack the troll")
- 4 defence taunts and 4 damage replies
- his defeated narrative

The generic hit, miss, damage and health lines are shared with the dragon. The dragon's own lines are unchanged.

**Outcomes:**
- **Victory:**
  - Grimtusk's defeated narrative is printed during combat.
  - A d10 Gold reward is rolled **once**, with the shared Game Die, and added to the player's Gold.
  - The victory message shows his name, the Gold found and the new total. The player returns to the Crossroads.
  - From then on, every West visit shows only the "pass is clear" message (name from `{0}`) and returns to the Crossroads. It shows no narrative, intro, stats, prompt, combat or reward, and changes no stat.
  - Grimtusk counts as defeated when his HealthPoints are 0 or below. Starting at 0 or below happens only in an artificial test setup.
- **Defeat:** the troll defeat message is printed and the adventure ends, as a dragon defeat does.
- **Retreat during combat:** the existing shared `retreat_combat` line, then the troll retreat message, then the Crossroads. The same Grimtusk stays at the pass with his remaining HealthPoints and original stats. The next visit shows that remaining value and plays the full encounter again.
- The dragon's `victory`, `defeat` and `retreat` endings never appear in the Troll Encounter.

The shared combat loop is unchanged, including how it handles invalid or missing input (spec Known Limitation L2).

**Language data, in all four languages:** 14 new dictionary keys are added and the temporary key is removed, so the dictionary goes from 97 to 110:
- `combat_troll_header`, `troll_player_attack_intro`
- `troll_defense_taunt_1–4` (taunt 2 takes `{0}` = race)
- `troll_damage_reply_1–4`
- `troll_defeated_narrative`
- `troll_victory` (`{0}` name, `{1}` Gold found, `{2}` new total)
- `troll_defeat` (`{0}` name)
- `troll_pass_clear` (`{0}` name)

The text uses the approved English wording. Translations must be complete, with matching placeholders and wording suited to each language. They don't need to differ from English.

## Expected files affected

- **Modified:** `Troll.cs` (his combat lines and the stash reward), `Game.cs` (combat in the Troll Encounter method, the defeat branch in the main loop), `language_data.json`
- **Tests:** `CYOATests/TrollTests.cs`, `CYOATests/LanguageDataTests.cs` (count 110, troll key list), `CYOATests/CombatTests.cs` (one new test of `monster` with Grimtusk)
- **Not touched:** `Monster.cs` and `Combat.cs` (unless a Grimtusk-only need appears, which must be reported first), `Dragon.cs`, `Merchant.cs`, `Messages.cs`, `Die.cs`, ADR 0001

## TDD approach

Each outcome is a vertical slice, red before green, using the spec's fixed-stat **fixtures** so no test depends on a dice roll:

| Fixture | Setup |
|---|---|
| Victory | Player Strength 20 with a weapon; Grimtusk Agility 0, HealthPoints 1 |
| Defeat | Player Strength 0, Agility 0, HealthPoints 1; Grimtusk Strength 20 |
| Retreat during combat | Player Strength 0; Grimtusk Strength 0 |
| Injury kept | Player Strength 20 with a weapon of max damage ≤ 12; Grimtusk Agility 0, Strength 0, HealthPoints 20 |

Out-of-range values are test setups only. Slice order:
1. Grimtusk's combat lines
2. Victory and the reward
3. Pass clear
4. Defeat and the main-loop end
5. Retreat during combat
6. The injury kept for the next visit
7. Language data

The d10 reward is checked over many victories, each asserting 1–10 and matching the printed amount. Console tests save and restore `Console.In` and `Console.Out`.

## Acceptance criteria

- [x] Victory (victory fixture): output shows `troll_defeated_narrative`, then `troll_victory`. The Gold gained is 1–10 and equals the printed amount, the printed total equals the new Player Gold, a victory is reported, and the adventure doesn't end (AC6).
- [x] Over many victories, the reward is always 1–10.
- [x] Defeat (defeat fixture): output ends with `troll_defeat`, a defeat is reported, and none of the dragon's `victory`, `defeat` or `retreat` texts appear (AC7).
- [x] The main loop ends the adventure on a troll defeat and returns to the Crossroads on every other outcome.
- [x] Retreat during combat (retreat fixture): `retreat_combat`, then `troll_retreat`, and a retreat is reported (AC9).
- [x] Combat against Grimtusk shows `combat_troll_header` and his own attack intro, taunts and replies. Combat against the dragon still shows its own lines (AC12).
- [x] After a victory, a second visit prints only `troll_pass_clear`, reports pass clear, and changes no Player stat, including Gold (AC13).
- [x] After a retreat during combat (injury fixture), Grimtusk's HealthPoints are below their start. A second visit shows that same value and plays the full encounter, and his Strength and Agility are unchanged (AC14).
- [x] Combat's `monster` property returns Grimtusk when given Grimtusk (AC19, Grimtusk part).
- [x] The 14 new keys exist in all four languages with matching placeholders. The key-count test is renamed and asserts 110 (AC21).
- [x] No dictionary value in any language contains "Grimtusk" typed in (AC23).
- [x] Ticket 02's temporary attack notice is fully removed: the behaviour, the `troll_fight_not_ready` key in all four languages, and its tests (replaced by the combat tests above).
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.
- [x] The Dragon Encounter, its text and endings, the North path, the Merchant Encounter, Exit and the main menu are unchanged. The shared combat loop is unchanged.

## Verification commands

```
dotnet build --no-incremental        # 0 warnings, 0 errors
dotnet test                          # all existing + new tests, 0 failed, 0 skipped
git diff 51c1941 -- docs/0001-add-italian-language-support.md   # prints nothing
grep -c troll_fight_not_ready language_data.json                # 0
git diff --stat                      # only the expected files
```

Also, by hand: scripted runs (time limit and output cap) that go West and fight. These play out by chance, so they're demonstrations, not the test of record.

## Definition of done

- Every acceptance criterion above is ticked, and `Status:` is set to `resolved`.
- Build is clean (0 warnings) and `dotnet test` shows 0 failed.
- A code review (standards and spec) has run, and its findings are reported.
- The user has reviewed the diff, including the translations, and approved the commit. Only approved files are staged, by explicit path. Nothing is pushed.
