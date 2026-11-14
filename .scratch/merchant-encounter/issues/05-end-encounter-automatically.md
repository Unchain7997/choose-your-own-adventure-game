# 05: End the encounter automatically

**What to build:** The Merchant Encounter ends on its own when the player can't afford any unsold Item. That covers every Item bought, the remaining Items costing more than the player's Gold, and a player starting with fewer than 5 Gold. When it ends this way, the player sees a translated reason line, then Old Brannoc's farewell, then the path prompt.

- **When it's checked:** at the start of the encounter and after each successful purchase. A refused choice (unaffordable, Sold or invalid) never ends the encounter.
- **Starting below 5 Gold:** the player sees the narrative, the greeting and their Gold line, but **no menu**, followed by the reason line and the farewell.

This ticket also adds full encounter runs in Spanish, French and Italian, now that the whole flow exists.

**Blocked by:** 04 (Refuse purchases the player can't afford)

**Status:** resolved

Spec: `.scratch/merchant-encounter/spec.md` (R9, AC1, AC2, AC15, AC16, AC17; seams 1 and 2)

- [x] Merchant tests: "can afford any unsold Item" is false below 5 Gold, false when every affordable Item is Sold, and true when only the Healing Potion is affordable.
- [x] Encounter test, below 5 Gold: the narrative, the greeting and the Gold line appear, **no menu** appears, then the reason line, then the farewell (AC1).
- [x] Encounter test AC15, 13 Gold: buy the Healing Potion (8 Gold left, +6 HealthPoints), then the Strength Tonic (0 left, +4 Strength), then the encounter ends automatically: reason line, then farewell.
- [x] Encounter test AC16, 20 Gold: buy the Strength Tonic (12 left), then the Agility Elixir (4 left), then the encounter ends automatically. Not all three Items can be bought.
- [x] Encounter test AC17, 7 Gold: choose the Strength Tonic and get "not enough Gold" with nothing changed and the menu again; buy the Healing Potion (2 left); the encounter ends automatically.
- [x] Encounter tests: refused choices (unaffordable, Sold, invalid) never trigger the automatic end (AC2).
- [x] Encounter tests: the reason line comes before the farewell; choosing Leave shows only the farewell, with no reason line.
- [x] A full encounter run (greeting, menu, a purchase, and Leave or the automatic end) passes in Spanish, French and Italian, with all messages looked up through Messages.
- [x] The reason-line message exists in all four languages.
- [x] The dictionary key-count test is updated to the new total.
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.
- [x] `dotnet test`: all existing tests plus the new tests pass, 0 failing.
- [x] ADR 0001 unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing).
- [x] No unrelated changes.
