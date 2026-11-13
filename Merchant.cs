// The Merchant's rules and Item catalogue. No console input or output happens here.
public class Merchant
{
    private enum Stat { HealthPoints, Strength, Agility }

    // Key identifies the Item's translations: "item_<Key>" (name) and "effect_<Key>" (effect description).
    private readonly record struct Item(string Key, int Price, int Bonus, Stat AffectedStat);

    // The single source of truth for the Items and their order. Menu number = index + 1.
    private static readonly Item[] Catalogue =
    {
        new Item("healing_potion", 5, 6, Stat.HealthPoints),
        new Item("strength_tonic", 8, 4, Stat.Strength),
        new Item("agility_elixir", 8, 4, Stat.Agility),
    };

    public string Name { get; } = "Old Brannoc";

    public int ItemCount => Catalogue.Length;

    // Leave is always the option after the last Item.
    public int LeaveOption => Catalogue.Length + 1;

    public string GetItemKey(int menuNumber) => ItemAt(menuNumber).Key;

    public int GetPrice(int menuNumber) => ItemAt(menuNumber).Price;

    public int GetBonus(int menuNumber) => ItemAt(menuNumber).Bonus;

    private static Item ItemAt(int menuNumber)
    {
        if (menuNumber < 1 || menuNumber > Catalogue.Length)
            throw new ArgumentOutOfRangeException(nameof(menuNumber), menuNumber,
                $"Menu number must be between 1 and {Catalogue.Length}.");

        return Catalogue[menuNumber - 1];
    }
}
