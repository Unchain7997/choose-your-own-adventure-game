# 03: Buy Items

**What to build:** In the Merchant Encounter, choosing an Item the player can afford buys it. The price comes off their Gold, the bonus is applied to the matching stat immediately and permanently, and a translated confirmation shows the new stat value and the Gold left. There's no "are you sure?" prompt. The menu then appears again with that Item still listed, its price still shown, and a **Sold** mark (for example `3. Agility Elixir (+4 Agility) - 8 Gold [Sold]`). Menu numbers never change. Choosing a Sold Item shows a translated "already bought" message and changes nothing. A player can buy several different Items, but each only once. A bought Strength Tonic makes the player's attacks in the Dragon Encounter hit more often, with no change to combat code.

This replaces Ticket 02's temporary behaviour for options 1–3.

**Temporary behaviour for unaffordable Items (until Ticket 04):** choosing an unsold Item the player can't afford shows the existing `invalid` message and the menu again. It must **never** deduct Gold, change a stat or mark the Item Sold. Ticket 04 replaces this with the dedicated "not enough Gold" message.

**Blocked by:** 02 (Meet Old Brannoc on the South path and leave)

**Status:** resolved

Spec: `.scratch/merchant-encounter/spec.md` (R5, R6, R8 purchase and Sold, AC8, AC14, AC18; seams 1, 2 and 3)

- [x] Merchant tests: buying an affordable, unsold Item returns **Purchased**, deducts exactly its price, applies exactly its bonus to the right stat only, and marks it Sold. Covered for each of the three Items.
- [x] Merchant tests: buying a Sold Item returns **AlreadySold** and changes nothing.
- [x] Merchant tests: buying an unaffordable Item changes nothing: no Gold deducted, no stat changed, not marked Sold.
- [x] Merchant tests: a player can buy two different Items in one encounter.
- [x] Encounter tests: after a purchase, the translated confirmation shows the new stat value and the remaining Gold, and the menu appears again.
- [x] Encounter tests: a Sold Item stays listed with its price and the Sold mark, and the other options keep their numbers (AC8, AC18).
- [x] Encounter tests: choosing a Sold Item shows "already bought" and the menu again.
- [x] Player test: after buying the Strength Tonic, `AttackHits` returns true for a roll equal to the new Strength, which was a miss before the purchase (AC14).
- [x] New messages (purchase confirmation(s), the stat names used in them, the Sold mark, "already bought") exist in all four languages, with identical keys and placeholders.
- [x] The dictionary key-count test is updated to the new total.
- [x] Console-based tests save and restore `Console.In` and `Console.Out`.
- [x] `dotnet test`: all existing tests plus the new tests pass, 0 failing.
- [x] ADR 0001 unchanged (`git diff 51c1941 -- docs/0001-add-italian-language-support.md` prints nothing).
- [x] Combat code and rules are unchanged. No stat caps are added.
