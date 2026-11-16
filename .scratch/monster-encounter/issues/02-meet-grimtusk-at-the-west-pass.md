# 02: Meet Grimtusk at the West pass and retreat

**Blocked by:** 01 (Turn the Dragon into a Monster)

**Status:** resolved

Spec: `.scratch/monster-encounter/spec.md` (R1, R2, R4–R6, R10, R12, R13, R18, R25–R28; AC1–AC5, AC8, AC10, AC11, AC15, AC17, AC22, AC23 for its own keys; seams 1–4)

## Scope

This is the tracer bullet for the **Troll Encounter**.

**At the Crossroads:**
- West is a third path. `w`, `west`, `W`, `WEST` and ` west ` are all accepted, and everything accepted today still works.
- The path prompt and the "invalid path" message are rewritten in all four languages to include West, with the approved English wording.

**Grimtusk:**
- A cave troll who is a Monster.
- He's created once per adventure, alongside the Player and Merchant, using the shared Game Die.
- He rolls Strength d12, Agility d10 and HealthPoints d12, and carries a club (max damage 8, ASCII art `(@)======`).
- He supplies his own stats header.

**A first West visit shows, in order:**
1. the West-path narrative
2. Grimtusk's intro, with his name from a `{0}` placeholder
3. the "magic ring" troll stats intro
4. his stats under the troll stats header
5. the existing attack-or-retreat prompt

**At that prompt:**
- `r` or `retreat` (trimmed, any case) prints the troll retreat message and returns to the Crossroads.
- **End of input** counts as a retreat. This handling exists only at this prompt.
- **Invalid input** shows the existing `invalid` message and the prompt again.
- After a retreat, the next West visit plays the full encounter again with every stat unchanged.

**Code changes:**
- One new public `Game` method runs the encounter for a Player, Grimtusk and Messages, and reports the outcome. The West branch of the main loop calls it.
- In this ticket the only possible outcome is a retreat, and the Crossroads follows.

**Temporary behaviour for attack (until Ticket 03):**
- `a` or `attack` is a valid choice, so it is **not** answered with `invalid`.
- Instead it shows a temporary, translated notice, `troll_fight_not_ready`, saying that fighting Grimtusk isn't available yet and that `r` retreats. Then the prompt appears again. The notice takes his name as `{0}`.
- It **starts no combat and changes no stat**.
- Ticket 03 deletes this notice and its key, and replaces it with real combat.

**New language data, in all four languages:**
- 5 permanent dictionary keys: `west_path_narrative`, `troll_intro`, `troll_stats_intro`, `troll_stats_header`, `troll_retreat`.
- 1 temporary key: `troll_fight_not_ready`. The dictionary goes from 91 to 97 for this ticket only.
- 2 rewritten keys: `path_prompt_full`, `path_invalid`.
- 1 new `displayWeaponMap` entry: `club` (club / garrote / massue / clava).

Spanish and French are written without accents; Italian keeps them. Translations must be complete, with matching placeholders and wording suited to each language. They don't need to differ from English.

## Expected files affected

- **New:** `Troll.cs`, `CYOATests/TrollTests.cs` (Troll and Troll Encounter tests)
- **Modified:** `Game.cs` (Crossroads parsing, a new Grimtusk per adventure, the West branch, the Troll Encounter method), `language_data.json`
- **Tests:** `CYOATests/GameTests.cs` (new West-path parsing tests), `CYOATests/LanguageDataTests.cs` (renamed count test, new key-list checks)
- **Not touched:** `Dragon.cs`, `Combat.cs`, `Merchant.cs`, `Messages.cs`, `Die.cs`, `Weapon.cs`, ADR 0001

## TDD approach

One vertical slice at a time, each test red before green:

1. **Crossroads parsing (seam 3).** `w`, `west`, case and padding are rejected first; make them accepted.
2. **Grimtusk (seam 1).** Name, club and stat ranges over many new Grimtusks. Each run asserts the value is in range, so the test can't fail by chance.
3. **First-visit output order (seam 2),** with typed-in input and captured output.
4. **Retreat, then invalid input, then end of input, then the temporary attack notice, then the second visit (seam 2).**
5. **Language-data checks (seam 4):** key count 97, the new keys, `club` in the weapon map, the `'w'` path text, and no "Grimtusk" typed into any message.

Messages are looked up through `Messages.GetMessage`, never hard-coded English. Console tests save and restore `Console.In` and `Console.Out`. No test may assume a favourable roll.

## Acceptance criteria

- [x] `w`, `west`, `W`, `WEST` and ` west ` are accepted as West. All inputs accepted today are still accepted, and invalid input is still rejected (AC1).
- [x] In every language, the path prompt mentions `'w'` and the invalid-path message lists `'n'`, `'s'`, `'w'` and `'e'` (AC2).
- [x] A new Grimtusk is named "Grimtusk" and carries a `club` with max damage 8. Over many new Grimtusks, Strength is 1–12, Agility 1–10 and HealthPoints 1–12 (AC3).
- [x] Grimtusk's attack rules follow his stats: a roll at or below Strength hits, and damage is 1–8 (AC4).
- [x] A first visit prints the narrative, intro with his name, stats intro, stats under `troll_stats_header`, then the prompt, in that order (AC5).
- [x] `r` and `retreat` print `troll_retreat` and report a retreat. Player and Grimtusk stats are unchanged and no combat output appears (AC8).
- [x] Invalid input prints `invalid` and the prompt again. ` r ` (padded) is accepted (AC10).
- [x] Running out of input at the prompt prints `troll_retreat` and reports a retreat, with no infinite loop (AC11).
- [x] After a retreat, a second visit plays the full encounter again with all stats unchanged (AC15).
- [x] A new adventure creates a new Grimtusk who isn't defeated, with freshly rolled stats (AC17).
- [x] Temporary: `a` and `attack` show `troll_fight_not_ready` (not `invalid`) and the prompt again, start no combat and change no stat.
- [x] `club` exists in every language's `displayWeaponMap`, and exactly six maps remain (AC22).
- [x] The new keys exist in all four languages with matching placeholders. No dictionary value contains "Grimtusk" typed in (AC23, for these keys).
- [x] The key-count test is renamed and asserts 97.
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.
- [x] The Dragon Encounter, North path, Merchant Encounter, Exit and main menu are unchanged. The only new public member on `Game` is the Troll Encounter method.

## Verification commands

```
dotnet build --no-incremental        # 0 warnings, 0 errors
dotnet test                          # all existing + new tests, 0 failed, 0 skipped
git diff 51c1941 -- docs/0001-add-italian-language-support.md   # prints nothing
git diff --stat                      # only the expected files
```

Also, by hand: a scripted run of the game in all four languages, with a time limit and an output cap. It goes West, retreats, goes West again, then exits.

## Definition of done

- Every acceptance criterion above is ticked, and `Status:` is set to `resolved`.
- Build is clean (0 warnings) and `dotnet test` shows 0 failed.
- A code review (standards and spec) has run, and its findings are reported.
- The user has reviewed the diff, including the translations, and approved the commit. Only approved files are staged, by explicit path. Nothing is pushed.
