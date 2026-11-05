# ADR 0001: Add Italian as a Fourth Supported Language

## Status

**Accepted.** Implemented on branch `feature/italian-language` in commit `dd230c5`, and pushed to `origin` (the personal fork). No pull request to `upstream` has been opened yet.

- **Date:** 2026-11-05
- **Decider:** Diana Benitez
- **Related commit:** `a2fa7a0`, *Fix uninitialized display dictionaries in Messages*. This commit fixed three CS8618 warnings in `Messages.cs` before the language work began, so the baseline was 0 warnings and 94 passing tests.

## Context

Before this change, the game supported English, Spanish, and French. Its localization works like this:

- **`language_data.json`** holds one top-level section per language. Each section has the same six maps:

  | Map | Purpose | Example (Spanish) |
  |---|---|---|
  | `dictionary` | Message key → displayed text (69 keys) | `"goodbye": "Adios!"` |
  | `raceMap` | Lowercase player input → canonical English value | `"enano": "Dwarf"` |
  | `occupationMap` | Lowercase player input → canonical English value | `"guerrero": "Fighter"` |
  | `displayRaceMap` | Canonical value → translated display name | `"Dwarf": "Enano"` |
  | `displayOccupationMap` | Canonical value → translated display name | `"Fighter": "Guerrero"` |
  | `displayWeaponMap` | Canonical weapon type → translated display name | `"claws": "garras"` |

- **`Messages.cs`** is language-agnostic. `ReadDictionary()` loads the section whose name matches `CurrentLanguage` (`doc.RootElement.GetProperty(CurrentLanguage)`). `GetMessage(key)` returns `[key]` when a key is missing.
- **Game logic always uses canonical English values.** For example, `Player` checks `Race == "Halfling"` and assigns a weapon with `switch (Occupation.ToLower())`. The input and display maps translate at the edges, so the rules don't depend on the language.
- **`Game.cs`** was the only place that listed the languages. An inline `switch` inside `StartGame()` mapped `"2"` to Spanish and `"3"` to French; anything else became English. Because that logic read from the console, it couldn't be unit tested.
- **The existing translations used only plain ASCII letters**, with no accents (for example `"Espanol"` and `"sueno"`), probably to avoid display problems in the Windows console.
- **Formatting placeholders** (`{0}`, `{1}`, `{2}`) in the dictionary strings are filled in with `string.Format`. If a translation has a malformed placeholder, the game throws a `FormatException` at runtime, possibly in the middle of combat.
- **The existing tests** checked loading for each language, a hand-picked list of 11 required keys, and `EndGame` in each language. Nothing checked that all languages had the same keys or the same placeholders.

The goal was to add Italian with authentic spelling, accents included, without changing how the game plays in English, Spanish, or French.

## Decision

### 1. Extend the existing JSON-based localization architecture

We added an `"Italian"` section to `language_data.json` with all 69 dictionary keys and all six maps, matching the structure of the other languages exactly. **`Messages.cs` was not changed.**

We chose this because the existing design already handles any number of languages: adding a language is a data change, not a code change. A separate translation system would have duplicated working infrastructure and given the project two different ways to do the same thing.

### 2. Use authentic Italian spelling and turn on UTF-8 console output

The Italian text uses proper accented characters (`è`, `à`, `ù`, `ì`, `ò`), as in "Qual è il nome del tuo personaggio?" and "Agilità: ". `language_data.json` is read as UTF-8 by default, so the text loads correctly.

To display those characters correctly on the Windows console, `Program.cs` sets:

```csharp
Console.OutputEncoding = System.Text.Encoding.UTF8;
```

We put this in `Program.cs`, the application's entry point, instead of in `Game`. That way the setting applies only when the real game runs and never affects unit tests, which redirect `Console.Out`. The existing translations are plain ASCII, which is encoded identically in UTF-8, so English, Spanish, and French display exactly as before.

### 3. Move language selection into a testable method

The inline `switch` moved out of `StartGame()` into a public method:

```csharp
public string GetLanguageFromChoice(string? input)
{
    return input?.Trim() switch
    {
        "2" => "Spanish",
        "3" => "French",
        "4" => "Italian",
        _ => "English"
    };
}
```

This follows the pattern of the existing `IsValidMenuChoice`, `IsValidPathChoice`, and `IsValidCombatChoice` helpers. The language prompt now shows `4. Italiano` and adds `Seleziona la lingua` to its header. The existing behavior is unchanged: `1`, `2`, and `3` select the same languages as before, and invalid or empty input still falls back to English.

### 4. Translation maps keep canonical English values

- **`raceMap`:** `umano`, `elfo`, `nano`, `halfling` → `Human`, `Elf`, `Dwarf`, `Halfling`
- **`occupationMap`:** `guerriero`, `mago`, `ladro`, `arciere` → `Fighter`, `Magician`, `Thief`, `Archer`
- **Display maps:** canonical value → Italian, for example `long sword` → `spada lunga` and `claws` → `artigli`

"Halfling" stays as a loanword (as in Italian D&D usage) rather than "mezzuomo". Some Italian words also exist in Spanish ("elfo", "mago"), but each language has its own maps, so they don't conflict.

**Game commands stay in English**, as they do in Spanish and French. Players still type `roll`, `n`/`s`/`e`, and `a`/`r`, and the Italian prompts tell them so (for example `Digita 'roll'`). Translating the commands would have required parser changes in `Game`, `Player`, and `Combat`, which is outside the scope of this change.

### 5. Keep every formatting placeholder

Every Italian string has exactly the same placeholders as its English version. In two strings, the Italian sentence was reworded after review so the grammar works with the inserted values, and the placeholders stayed the same:

| Key | Final Italian | Reason |
|---|---|---|
| `player_attack_intro` | `{0}, sfoderi la tua arma ({1}) e attacchi il drago.` | Starting the sentence with the occupation `{0}`, addressing the player by class, makes its capital letter correct. The weapon name goes in parentheses because Italian articles depend on the noun's gender. |
| `dragon_attack_intro` | `{0} attacca {1} con gli {2}!` | Reads "con gli artigli", which is idiomatic Italian. |

### 6. Automated tests guard completeness and placeholders

We added 49 tests (counting each `DataRow` as one test), taking the suite from 94 to 143:

| File | Tests | What they cover |
|---|---|---|
| `CYOATests/GameTests.cs` | +13 | `EndGame` in Italian (win, lose, retreat). `GetLanguageFromChoice`: `"4"` and `"  4  "` give Italian; `1`, `2`, `3` give the original languages; `null`, `""`, whitespace, `"5"`, and `"Italian"` give English. |
| `CYOATests/MessagesTests.cs` | +15 | Setting the language to Italian. The Italian welcome and goodbye text. Accented characters loading intact. The 11 required keys in Italian. Race and occupation input in Italian, any capitalization. English words rejected in Italian mode. Translated display names. |
| `CYOATests/LanguageDataTests.cs` (new) | +21 | Checks of the JSON file across all languages, described below. |

`LanguageDataTests` reads `language_data.json` directly and checks every language against English:

- Dictionary keys are the same as English, and the test failure lists any missing or extra keys.
- Italian has exactly 69 dictionary keys.
- All six required maps exist.
- `raceMap` and `occupationMap` produce the same canonical values as English.
- The display maps have the same keys as English.
- The `{n}` placeholders match English for every key.
- Every message can be formatted with `string.Format` without throwing.

These tests make silent mistakes, such as a missing key showing `[key]` on screen or a broken placeholder causing a runtime `FormatException`, fail the build instead. They protect English, Spanish, French, and any language added in the future.

## Alternatives Considered

| Alternative | Why we rejected it |
|---|---|
| **A separate translation system** (`.resx` resource files with `ResourceManager`, a localization library, or a separate `italian.json`) | It would add a second localization method next to a working one, require changes to `Messages.cs`, and require rewriting or duplicating tests. It's too big a change for a student project that already has a clean, data-driven design. |
| **Plain ASCII Italian** (`e'` instead of `è`) to match the existing languages | Consistent with the other languages and needs no console change, but the result is not authentic Italian. The project owner chose proper spelling. |
| **Setting UTF-8 output inside `Game.StartGame()`** | That would tie a process-wide console setting to a class the tests use. `Program.cs` keeps the setting at the application's entry point. |
| **Only adding `"4" => "Italian"` to the inline switch** | It's the smallest code change, but language selection would still be impossible to unit test. |
| **Translated game commands** (for example `tira` instead of `roll`) | It would require parser changes across several classes and make Italian behave differently from the other languages. We deferred it. |
| **Putting articles in the display maps** (`"claws": "gli artigli"`) | The display maps are also used for stat lines (`Arma: artigli`), where an article reads badly. We put the grammar fixes in the sentence templates instead. |

## Consequences

### Positive

- Italian is fully supported. The change touched one data file and two small code files, and the localization class (`Messages.cs`) didn't change.
- Language selection now has unit tests.
- The new tests across all languages protect every language from missing keys and broken placeholders.
- The future process for adding a language is documented: add a JSON section, add a `switch` case, and add a `DataRow` to each test in `LanguageDataTests`.
- English, Spanish, and French behave the same as before.

### Negative / Risks

- **`Console.OutputEncoding` can throw an `IOException`** on Windows when the process has no console at all. It worked in an interactive terminal and with piped input. It is not guarded by `try/catch`, which would be a possible follow-up.
- **Console input encoding was not changed.** No race or occupation word needs an accent, but a player who types an accented name (for example "Nicolò") might see it come out garbled in some Windows consoles.
- **`dragon_attack_intro` assumes the dragon's weapon is plural and masculine** ("gli"). That's correct for `claws`, the dragon's only weapon. If the dragon ever gets another weapon, the template needs review.
- **`Language_HasAll69DictionaryKeys` hard-codes the number 69.** If a message key is added, this test needs updating. The test that compares each language's keys with English stays correct without changes.
- **The translations were written with AI assistance.** A fluent Italian speaker should review them before the change is merged upstream.
- **Line endings:** `CYOATests/LanguageDataTests.cs` was created with LF endings, while the other test files use CRLF. Git normalizes this, so there is no functional effect.

## Validation

All results below are from the final implementation (commit `dd230c5`):

| Check | Result |
|---|---|
| `dotnet build` (full rebuild with `--no-incremental`) | 0 warnings, 0 errors |
| `dotnet test` | **143 passed**, 0 failed, 0 skipped (baseline: 94) |
| JSON structure check | Italian: 69 dictionary keys. Display maps have the same keys as English. 0 placeholder mismatches. |
| Italian playthrough: retreat at the dragon | Exit code 0. Accents display ("Qual è…"). Character creation works (`umano`, `Marco`, `guerriero`). The north-path dragon encounter appears, and retreating ends the game. No `[key]` markers. |
| Italian playthrough: one combat round, then retreat | Exit code 0. Both revised lines appear correctly: "Guerriero, sfoderi la tua arma (spada lunga) e attacchi il drago." and "Smolderfang attacca Marco con gli artigli!" No `[key]` markers. |
| UTF-8 output | `è` is written as the correct UTF-8 bytes `c3 a8` |
| Existing languages (quick check) | `1` gives English, `2` gives Spanish, and invalid input gives English, as before |

### Files changed

- `language_data.json`: added the `Italian` section
- `Game.cs`: added `GetLanguageFromChoice()` and updated the language prompt
- `Program.cs`: turned on UTF-8 console output
- `CYOATests/GameTests.cs`, `CYOATests/MessagesTests.cs`: added Italian tests
- `CYOATests/LanguageDataTests.cs`: new file with the JSON checks across all languages
