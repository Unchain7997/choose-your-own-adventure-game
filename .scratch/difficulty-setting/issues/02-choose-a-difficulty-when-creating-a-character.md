# 02: Choose a difficulty when creating a character

**What to build:** After the player chooses `1` (Create Character) at the main menu, the game asks them to choose a **Difficulty** before the race prompt. The prompt is a numbered menu, `1` Easy, `2` Normal, `3` Hard, followed by the usual "Enter choice" text.
- Surrounding spaces are ignored.
- Anything else prints the existing `invalid` message and asks again.
- If input runs out, the game says Normal was selected automatically, picks Normal and moves on to the race prompt. It never loops.

The chosen Difficulty is stored on the Player, and the Character Summary shows it on its own translated line, immediately after Gold. The prompt, the names and the messages are available in English, Spanish, French and Italian.

**The chosen Difficulty does not change combat yet. The feature is incomplete until Ticket 03.**

**Blocked by:** 01 (Monsters respond to Difficulty)

**Status:** resolved

Spec: `.scratch/difficulty-setting/spec.md` (R1–R6, R13, R18, R19, R21–R24; Message Catalogue; AC1, AC2, AC3, AC6 label part, AC7, AC9, AC10, AC11; Seams 2, 3, 4, 6)

## Scope

- **`Game`:**
  - A public pure parsing method: trimmed `1`/`2`/`3` → Easy/Normal/Hard; everything else, including null, → not valid.
  - A public console prompt method that takes Messages: shows `difficulty_prompt`, then `enter_choice` without a newline. Invalid input prints `invalid` and asks again. End of input prints `difficulty_auto_normal` and returns Normal.
  - In main-menu choice `1`: prompt for the Difficulty, create the Player, set `Player.Difficulty`, then create the character. The Monsters are **not** adjusted yet (Ticket 03).
- **`Player`:** a `Difficulty` property, defaulting to Normal. The Character Summary prints `stats_difficulty` plus the translated name immediately after Gold, and before the Weapon line when a weapon is shown. Character rolling is unchanged.
- **Translated names:** a fixed mapping from each Difficulty to `difficulty_easy`, `difficulty_normal` or `difficulty_hard`. An unknown value is **rejected**, not silently treated as Normal.
- **Language data:** the 6 keys from the spec's Message Catalogue (`difficulty_prompt`, `difficulty_auto_normal`, `stats_difficulty`, `difficulty_easy`, `difficulty_normal`, `difficulty_hard`) in all four languages, in the existing `dictionary` map. There are still exactly six maps, and no existing key is renamed or reworded.
  - English text exactly as in the Message Catalogue.
  - Spanish, French and Italian names and labels as approved in Q21. French `Difficulte: ` follows the existing French stats labels (e.g. `Or: `).
  - Spanish and French without accents; Italian keeps its accents. No placeholders.
- **Unchanged:** the race, name and occupation prompts and their end-of-input behaviour (L1), the main menu, the Crossroads, every encounter, `Combat`, `Monster`, `Dragon`, `Troll`, `Die`, `Merchant`, `Messages`, ADR 0001, the glossary and the Category A documents.

## Acceptance criteria

- [ ] Parsing: `1` gives Easy, `2` Normal, `3` Hard, and ` 2 ` (with spaces) Normal; `0`, `4`, `easy`, `Easy`, an empty string, whitespace and null are not valid (AC2).
- [ ] Prompt: an invalid entry then `3` returns Hard, and `invalid` appears exactly once (AC2).
- [ ] Prompt: end of input returns Normal and prints `difficulty_auto_normal`, without looping (AC3).
- [ ] Prompt: shows `difficulty_prompt` followed by `enter_choice`; at least one run in Spanish, French or Italian checks the translated prompt (AC1, language).
- [ ] Character Summary: for each of Easy, Normal and Hard, the translated `stats_difficulty` line with the translated name is the line immediately after Gold and immediately before Weapon (the test Player has a weapon) (AC7).
- [ ] A new Player's Difficulty is Normal.
- [ ] The Ticket 01 combat stats display test also asserts that the display contains no `stats_difficulty` text (AC6, label part).
- [ ] Language data: `Language_HasAll110DictionaryKeys` is renamed and updated to `Language_HasAll116DictionaryKeys` (reported as a changed test, not a new one). A new `DifficultyKeys` list, following the `TrollEncounterKeys` precedent, checks that all 6 keys exist and are non-empty in all four languages (AC9).
- [ ] All other existing tests pass unchanged: 345 + Ticket 01's tests + N passed, 0 failed, 0 skipped. `dotnet build` reports 0 warnings (AC10).
- [ ] `git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing (AC11).
- [ ] A focused diff review confirms that the Difficulty doesn't alter player-stat generation, Gold, Merchant prices or weapon damage (AC5).

## TDD approach

1. Run the full suite and confirm the post-Ticket-01 baseline.
2. Red → green at Seam 2 (parsing), then Seam 3 (prompt, console input and output, saving and restoring `Console.In` and `Console.Out`), then Seam 4 (Character Summary), then Seam 6 (language data).
3. Look up every message through `Messages.GetMessage`; don't hard-code English.
4. Don't run the game by hand without `timeout` and an output cap. The prompts after this one still loop on end of input (L1).
5. Report honestly any test that passed without a red step, and the renamed key-count test.

## Review gate

Stop before committing. **The user reviews all 24 new language entries (6 keys × 4 languages, including English and the Q21 names) in the diff, and this ticket is not committed until they are approved** (R23). Stage only the files the user approves, by explicit path. Do not push.
