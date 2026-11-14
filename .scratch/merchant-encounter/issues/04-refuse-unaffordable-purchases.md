# 04: Refuse purchases the player can't afford

**What to build:** In the Merchant Encounter, choosing an unsold Item that costs more than the player's Gold shows a translated "not enough Gold" message stating the price and the player's current Gold (for example "You need 8 Gold but have only 5"). Nothing changes: no Gold deducted, no stat changed, the Item isn't marked Sold. The menu then appears again. If an Item is both Sold and unaffordable, the player gets "already bought", because Sold is checked first. Gold never goes below 0.

**Blocked by:** 03 (Buy Items)

**Status:** resolved

Spec: `.scratch/merchant-encounter/spec.md` (R8 not enough Gold, AC3, AC7; seams 1 and 2)

- [x] Merchant tests: buying an unaffordable, unsold Item returns **InsufficientGold**, and Gold, HealthPoints, Strength, Agility and Sold status are exactly as before (AC7).
- [x] Merchant tests: an Item that is both Sold and unaffordable returns **AlreadySold**, not InsufficientGold (AC3).
- [x] Merchant tests: Gold is never negative after any sequence of purchase attempts (AC7).
- [x] Encounter tests: choosing an unaffordable Item shows the translated "not enough Gold" message with the correct price and current Gold, then the menu again.
- [x] Encounter tests: after a refusal, the player can still buy an Item they can afford in the same encounter.
- [x] The "not enough Gold" message (with price and current Gold as placeholders) exists in all four languages, with identical placeholders.
- [x] The dictionary key-count test is updated to the new total.
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.
- [x] `dotnet test`: all existing tests plus the new tests pass, 0 failing.
- [x] ADR 0001 unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing).
- [x] No unrelated changes.
