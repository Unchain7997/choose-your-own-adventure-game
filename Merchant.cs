public enum PurchaseResult { Purchased, InsufficientGold, AlreadySold }

// The Player stats an Item can raise.
public enum PlayerStat { HealthPoints, Strength, Agility }

// The Merchant's rules and Item catalogue. No console input or output, and no translation keys
// beyond each Item's own Key, happen here.
public class Merchant
{
    // Key identifies the Item's translations: "item_<Key>" (name) and "effect_<Key>" (effect description).
    private readonly record struct Item(string Key, int Price, int Bonus, PlayerStat AffectedStat);

    // The single source of truth for the Items and their order. Menu number = index + 1.
    private static readonly Item[] Catalogue =
    {
        new Item("healing_potion", 5, 6, PlayerStat.HealthPoints),
        new Item("strength_tonic", 8, 4, PlayerStat.Strength),
        new Item("agility_elixir", 8, 4, PlayerStat.Agility),
    };

    // Sold status per Item, indexed like Catalogue. A new Merchant (one per adventure) starts with nothing Sold.
    private readonly bool[] _sold = new bool[Catalogue.Length];

    public string Name { get; } = "Old Brannoc";

    public int ItemCount => Catalogue.Length;

    // Leave is always the option after the last Item.
    public int LeaveOption => Catalogue.Length + 1;

    public string GetItemKey(int menuNumber) => ItemAt(menuNumber).Key;

    public int GetPrice(int menuNumber) => ItemAt(menuNumber).Price;

    public int GetBonus(int menuNumber) => ItemAt(menuNumber).Bonus;

    public PlayerStat GetAffectedStat(int menuNumber) => ItemAt(menuNumber).AffectedStat;

    // The Player's current value of a stat. Throws for a stat no Item rule supports.
    public static int GetStatValue(Player player, PlayerStat stat) => StatAccessor(stat).Get(player);

    public bool IsSold(int menuNumber)
    {
        ItemAt(menuNumber);
        return _sold[menuNumber - 1];
    }

    // Sold is checked before affordability (AC3). Only Purchased changes anything.
    public PurchaseResult Purchase(Player player, int menuNumber)
    {
        Item item = ItemAt(menuNumber);

        if (_sold[menuNumber - 1])
            return PurchaseResult.AlreadySold;

        if (player.Gold < item.Price)
            return PurchaseResult.InsufficientGold;

        // Resolved before anything changes, so an unsupported stat throws with Gold and stats untouched.
        var stat = StatAccessor(item.AffectedStat);

        player.Gold -= item.Price;
        stat.Set(player, stat.Get(player) + item.Bonus);
        _sold[menuNumber - 1] = true;

        return PurchaseResult.Purchased;
    }

    // Drives the automatic end of the encounter (R9): false once no unsold Item is within the Player's Gold.
    public bool CanAffordAnyUnsoldItem(Player player)
    {
        for (int i = 0; i < Catalogue.Length; i++)
        {
            if (!_sold[i] && player.Gold >= Catalogue[i].Price)
                return true;
        }

        return false;
    }

    // The one place a PlayerStat is mapped to a Player property. No fallback: an unsupported stat fails explicitly.
    private static (Func<Player, int> Get, Action<Player, int> Set) StatAccessor(PlayerStat stat) => stat switch
    {
        PlayerStat.HealthPoints => (p => p.HealthPoints, (p, value) => p.HealthPoints = value),
        PlayerStat.Strength => (p => p.Strength, (p, value) => p.Strength = value),
        PlayerStat.Agility => (p => p.Agility, (p, value) => p.Agility = value),
        _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, "No Item rule supports this stat."),
    };

    private static Item ItemAt(int menuNumber)
    {
        if (menuNumber < 1 || menuNumber > Catalogue.Length)
            throw new ArgumentOutOfRangeException(nameof(menuNumber), menuNumber,
                $"Menu number must be between 1 and {Catalogue.Length}.");

        return Catalogue[menuNumber - 1];
    }
}
