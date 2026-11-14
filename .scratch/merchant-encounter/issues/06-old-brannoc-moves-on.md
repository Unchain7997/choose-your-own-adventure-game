# 06: Old Brannoc moves on after one visit

**What to build:** The Merchant Encounter happens **once per adventure**. The first South visit counts as met, even if the encounter ends automatically straight away. On every later South visit, the player sees only a translated "Old Brannoc has moved on" message (his name from the Merchant through a placeholder) and goes back to the path prompt. There's no narrative, greeting or menu. North and Exit still work afterwards, and a player who shopped first fights the dragon with their raised stats. A fresh Merchant comes with each new adventure, so the state never carries over.

**Blocked by:** 05 (End the encounter automatically)

**Status:** resolved

Spec: `.scratch/merchant-encounter/spec.md` (R2, R13 once-per-adventure, AC4, AC9; seams 1 and 2)

- [x] Merchant tests: a new Merchant hasn't been met; once marked met, it stays met.
- [x] Encounter tests: the second call to the encounter method with the same Merchant prints the "moved on" message and nothing else from the encounter (no narrative, greeting, Gold line or menu), and changes no Player stat.
- [x] Encounter tests: a player who starts below 5 Gold (automatic end at once) still gets "moved on" on the second visit (AC4).
- [x] Encounter tests: a player who chose Leave without buying anything still gets "moved on" on the second visit.
- [x] The "moved on" message takes the Merchant's name as a `{0}` placeholder and exists in all four languages.
- [x] The dictionary key-count test is updated to the new total.
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.
- [x] `dotnet test`: all existing tests plus the new tests pass, 0 failing (AC9, AC10).
- [x] ADR 0001 unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing).
- [x] The North path, Exit, the main menu, combat rules and the combat stats display are unchanged (AC9).
