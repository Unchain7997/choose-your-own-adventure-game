# 04: Grimtusk's results carry through the adventure

**Blocked by:** 03 (Fight Grimtusk)

**Status:** resolved

Spec: `.scratch/monster-encounter/spec.md` (R17, R24, R28, R29; AC16, AC18, AC20, AC24–AC26; seams 2 and 5)

## Scope

This ticket proves the Troll Encounter fits the rest of the game, and finishes the language work.

- **Gold** found in Grimtusk's stash can be spent with Old Brannoc, so going West before South is a real strategy.
- **Injuries** the player takes from Grimtusk are the HealthPoints they carry into the Dragon Encounter. Nothing restores them.
- **A full Troll Encounter victory** runs correctly in Spanish, French and Italian, printing that language's messages.
- **A final review** of every new and rewritten message in all four languages:
  - Translations are complete, with matching placeholders and wording suited to each language.
  - Spanish and French are written without accents; Italian keeps them.
  - A translation may match English where that's the right word, such as "Troll" in French and Italian.
  - The review may fix wording. It doesn't add or rename keys.
- **Regression:** the Dragon Encounter, North path, Merchant Encounter, Exit and main menu behave exactly as before.

This ticket is mainly tests and translation review. Any code change must be needed by one of the criteria below.

## Expected files affected

- **Tests:** `CYOATests/TrollTests.cs` (cross-feature and translated-run tests)
- **Possibly modified:** `language_data.json` (wording fixes from the review only, no new or renamed keys)
- **Not touched:** application code, unless a criterion below needs it (report first), ADR 0001

## TDD approach

- Each cross-feature check is its own test, red before green where it exposes missing behaviour. Where the behaviour already exists from Tickets 01–03, the test is a characterisation test and is reported as green from the start.
- Use the Troll Encounter method and the Merchant Encounter method with typed-in input (seam 2). For the Dragon hand-off, set up stats directly and use the existing public combat API.
- Tests use the spec's fixed-stat fixtures, never a lucky roll.
- Wording quality is checked by the user's review of the translation diff, not by an automated test.

## Acceptance criteria

- [x] After a Troll Encounter victory, the troll Gold is spent on an Item through the Merchant Encounter, and Gold and the stat change as the Merchant rules say (AC20).
- [x] The player's HealthPoints after a Troll Encounter are the values used in the next encounter, including the dragon fight. Nothing restores them (AC16).
- [x] A full victory run in each of Spanish, French and Italian prints that language's messages (AC24).
- [x] All 19 new keys and the 2 rewritten keys are reviewed by the user in all four languages: complete, matching placeholders, suitable wording, and the Spanish/French no-accent convention (AC24).
- [x] The Dragon Encounter, North path, Merchant Encounter, Exit and main menu behave exactly as before. All existing tests pass, with no changes except the three explicitly approved in R22: the combat.monster assertion, the invalid-path example changed from west to east, and the language key-count test updates (AC18).
- [x] `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 262 + N passed, 0 failed, 0 skipped (AC25).
- [x] ADR 0001 unchanged: `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing. Checked by hand (AC26).
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.

## Verification commands

```
dotnet build --no-incremental        # 0 warnings, 0 errors
dotnet test                          # 262 + N passed, 0 failed, 0 skipped
git diff 51c1941 -- docs/0001-add-italian-language-support.md   # prints nothing
git diff main...HEAD --stat          # the whole feature's footprint
```

Also, by hand: scripted runs in all four languages (time limit and output cap) covering West, South, North and Exit.

## Definition of done

- Every acceptance criterion above is ticked, `Status:` is set to `resolved`, and the spec's status is set to `resolved` with a completion note.
- Build is clean (0 warnings) and `dotnet test` shows 0 failed.
- A final code review (standards and spec) over the whole feature has run, and its findings are reported.
- The user has reviewed the diff, including the translations, and approved the commit. Only approved files are staged, by explicit path. Nothing is pushed, merged or rebased without separate approval.
