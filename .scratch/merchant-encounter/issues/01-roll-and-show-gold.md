# 01: Roll and show Gold at character creation

**What to build:** When a player creates a character, they're asked to type "roll" for their **Gold** right after rolling Health, the same way as the other stats. Gold is a d20 roll (1–20) with no race or occupation modifier. Viewing stats during character creation now shows a translated Gold line in all four languages. The combat stats display is unchanged and never shows Gold. A brand-new Player starts with 0 Gold before creation.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

Spec: `.scratch/merchant-encounter/spec.md` (R3, R4, AC20, AC21; seam 3 and seam 4)

- [ ] A new Player has 0 Gold (extend the existing default-values test).
- [ ] Character creation prompts for the Gold roll right after Health and before the next-action menu, using the existing "type 'roll'" pattern.
- [ ] Running scripted character creation about 20 times on fresh Players always gives Gold between 1 and 20 (R3).
- [ ] The stats display shows the translated Gold label and value in English, Spanish, French and Italian (AC20).
- [ ] The combat stats display, called directly on Combat, contains no Gold label (AC21).
- [ ] The Gold roll prompt and the Gold stats label exist as dictionary messages in all four languages, with identical keys and placeholders.
- [ ] The dictionary key-count test is renamed and updated to the new total.
- [ ] Console-based tests save and restore `Console.In` and `Console.Out`.
- [ ] `dotnet test`: all 143 existing tests plus the new tests pass, 0 failing.
- [ ] ADR 0001 unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing).
- [ ] No changes to combat rules, the North path, or unrelated code.
