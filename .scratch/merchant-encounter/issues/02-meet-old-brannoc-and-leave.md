# 02: Meet Old Brannoc on the South path and leave

**What to build:** This is the tracer bullet for the **Merchant Encounter**. Choosing South no longer ends with "nothing adventurous happens". Instead, the player sees:

1. the rewritten South-path narrative, which describes meeting a travelling merchant on the forest road without naming him
2. **Old Brannoc**'s greeting, with his name coming from the Merchant
3. their current Gold
4. the Merchant menu: `1` Healing Potion, `2` Strength Tonic, `3` Agility Elixir, `4` Leave, each Item with its translated name, effect and price (for example `1. Healing Potion (+6 Health) - 5 Gold`)

Choosing **Leave** shows Old Brannoc's farewell, and the player goes back to the path prompt, where North and Exit work as before. Any input other than 1–4 shows the existing `invalid` message and the menu again. Input is trimmed before matching.

A new `Merchant` class holds the Merchant's name and the fixed Item catalogue (price, bonus and affected stat for menu numbers 1–3), with no console I/O. One new public `Game` method runs the encounter for a given Player, Merchant and Messages, and the South branch calls it. A Merchant is created each time a new Player is created.

**Temporary behaviour for options 1–3 (until Ticket 03):** choosing `1`, `2` or `3` is treated exactly like invalid input. It shows the existing `invalid` message and the menu again. It **grants no Item, deducts no Gold, and changes no stat**. Ticket 03 replaces this with real purchases.

**Blocked by:** 01 (Roll and show Gold at character creation)

**Status:** ready-for-agent

Spec: `.scratch/merchant-encounter/spec.md` (R1, R7 without the Sold mark, R8 invalid input only, R10, R11, R12, R13, AC6, AC19, AC22; seams 1, 2 and 4)

- [ ] Merchant tests (no console): the name is "Old Brannoc"; each Item's price and bonus match the spec table exactly (Healing Potion 5 Gold / +6 HealthPoints, Strength Tonic 8 / +4 Strength, Agility Elixir 8 / +4 Agility).
- [ ] Encounter tests (scripted input, captured output): South shows the narrative, then the greeting with the Merchant's name, then the Gold line, then the menu, in that order.
- [ ] The menu shows all three Items with translated names, translated effect descriptions and prices, plus Leave.
- [ ] Choosing `4` shows the farewell (with the Merchant's name) and returns.
- [ ] An invalid entry shows `invalid` and the menu again. ` 4 ` (with spaces) works the same as `4` (AC19).
- [ ] Choosing `1`, `2` or `3` shows `invalid` and the menu again, and leaves Gold, HealthPoints, Strength and Agility exactly as they were (temporary behaviour).
- [ ] The South-path narrative key is still present in all four languages, with rewritten text that doesn't name the Merchant and has no placeholders (R1, AC22).
- [ ] The greeting and farewell take the Merchant's name as a `{0}` placeholder. "Old Brannoc" is never typed into message text (AC6).
- [ ] New messages (greeting, Gold display line, the three Item names, the three effect descriptions, the menu line format, the Leave option, farewell) exist in all four languages. The language data still has exactly six maps (R12).
- [ ] The dictionary key-count test is updated to the new total.
- [ ] Console-based tests save and restore `Console.In` and `Console.Out`.
- [ ] `dotnet test`: all existing tests plus the new tests pass, 0 failing.
- [ ] ADR 0001 unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing).
- [ ] The North path, Exit, the main menu and combat are unchanged. The only new public member on `Game` is the encounter method.
