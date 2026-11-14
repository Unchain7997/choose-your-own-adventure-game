// CYOATests/MerchantTests.cs
// Seam 1: Merchant rules with plain values, no console.
[TestClass]
public class MerchantTests
{
    [TestMethod]
    public void NewMerchant_IsNamedOldBrannoc()
    {
        Merchant merchant = new Merchant();

        Assert.AreEqual("Old Brannoc", merchant.Name);
    }

    // Expected values are the spec's R5 table: Healing Potion, Strength Tonic, Agility Elixir.
    [TestMethod]
    [DataRow(1, 5, 6, "healing_potion")]
    [DataRow(2, 8, 4, "strength_tonic")]
    [DataRow(3, 8, 4, "agility_elixir")]
    public void Item_HasPriceBonusAndKeyFromCatalogue(int menuNumber, int expectedPrice, int expectedBonus, string expectedKey)
    {
        Merchant merchant = new Merchant();

        Assert.AreEqual(expectedPrice, merchant.GetPrice(menuNumber));
        Assert.AreEqual(expectedBonus, merchant.GetBonus(menuNumber));
        Assert.AreEqual(expectedKey, merchant.GetItemKey(menuNumber));
    }

    [TestMethod]
    [DataRow(1, PlayerStat.HealthPoints)]
    [DataRow(2, PlayerStat.Strength)]
    [DataRow(3, PlayerStat.Agility)]
    public void Item_RaisesTheStatFromCatalogue(int menuNumber, PlayerStat expectedStat)
    {
        Assert.AreEqual(expectedStat, new Merchant().GetAffectedStat(menuNumber));
    }

    // Distinct values per stat, so reading the wrong property is caught.
    [TestMethod]
    [DataRow(PlayerStat.HealthPoints, 15)]
    [DataRow(PlayerStat.Strength, 12)]
    [DataRow(PlayerStat.Agility, 11)]
    public void GetStatValue_ReadsTheMatchingPlayerStat(PlayerStat stat, int expected)
    {
        Player player = new Player { HealthPoints = 15, Strength = 12, Agility = 11, Gold = 20 };

        Assert.AreEqual(expected, Merchant.GetStatValue(player, stat));
    }

    [TestMethod]
    public void GetStatValue_UnsupportedStat_FailsExplicitly()
    {
        Player player = new Player { HealthPoints = 15, Strength = 12, Agility = 11 };

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Merchant.GetStatValue(player, (PlayerStat)99));
    }

    [TestMethod]
    public void Catalogue_HasThreeItems_AndLeaveIsOptionFour()
    {
        Merchant merchant = new Merchant();

        Assert.AreEqual(3, merchant.ItemCount);
        Assert.AreEqual(4, merchant.LeaveOption);
    }

    private static Player PlayerWith(int gold) => new Player
    {
        Strength = 12,
        Agility = 11,
        HealthPoints = 15,
        Gold = gold
    };

    // Starting stats are Strength 12, Agility 11, HealthPoints 15; expected values follow the R5 table.
    [TestMethod]
    [DataRow(1, 15, 21, 12, 11)] // Healing Potion: 5 Gold, +6 HealthPoints
    [DataRow(2, 12, 15, 16, 11)] // Strength Tonic: 8 Gold, +4 Strength
    [DataRow(3, 12, 15, 12, 15)] // Agility Elixir: 8 Gold, +4 Agility
    public void Purchase_AffordableUnsoldItem_DeductsPriceAppliesBonusAndMarksSold(
        int menuNumber, int expectedGold, int expectedHealth, int expectedStrength, int expectedAgility)
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold: 20);
        Assert.IsFalse(merchant.IsSold(menuNumber));

        PurchaseResult result = merchant.Purchase(player, menuNumber);

        Assert.AreEqual(PurchaseResult.Purchased, result);
        AssertStats(player, expectedGold, expectedHealth, expectedStrength, expectedAgility);
        Assert.IsTrue(merchant.IsSold(menuNumber));
    }

    private static void AssertStats(Player player, int gold, int health, int strength, int agility)
    {
        Assert.AreEqual(gold, player.Gold, "Gold");
        Assert.AreEqual(health, player.HealthPoints, "HealthPoints");
        Assert.AreEqual(strength, player.Strength, "Strength");
        Assert.AreEqual(agility, player.Agility, "Agility");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void Purchase_SoldItem_ReturnsAlreadySoldAndChangesNothing(int menuNumber)
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold: 20);
        merchant.Purchase(player, menuNumber);
        int gold = player.Gold, health = player.HealthPoints, strength = player.Strength, agility = player.Agility;

        PurchaseResult result = merchant.Purchase(player, menuNumber);

        Assert.AreEqual(PurchaseResult.AlreadySold, result);
        AssertStats(player, gold, health, strength, agility);
        Assert.IsTrue(merchant.IsSold(menuNumber));
    }

    // One Gold short of each price.
    [TestMethod]
    [DataRow(1, 4)]
    [DataRow(2, 7)]
    [DataRow(3, 7)]
    public void Purchase_UnaffordableItem_ReturnsInsufficientGoldChangesNothingAndStaysUnsold(int menuNumber, int gold)
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold);

        PurchaseResult result = merchant.Purchase(player, menuNumber);

        Assert.AreEqual(PurchaseResult.InsufficientGold, result);
        AssertStats(player, gold, health: 15, strength: 12, agility: 11);
        Assert.IsFalse(merchant.IsSold(menuNumber));
    }

    // AC3: buying the Tonic with exactly 8 Gold leaves 0, so it is now both Sold and unaffordable.
    [TestMethod]
    public void Purchase_ItemSoldAndUnaffordable_ReturnsAlreadySold()
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold: 8);
        merchant.Purchase(player, 2);

        PurchaseResult result = merchant.Purchase(player, 2);

        Assert.AreEqual(PurchaseResult.AlreadySold, result);
        AssertStats(player, gold: 0, health: 15, strength: 16, agility: 11);
    }

    // AC7: a mix of purchases and refusals never takes Gold below 0, and only the purchases change anything.
    [TestMethod]
    public void Purchase_AnySequenceOfAttempts_NeverMakesGoldNegative()
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold: 13);
        int[] attempts = { 2, 3, 1, 1, 2, 3, 1, 2, 3 };
        PurchaseResult[] expected =
        {
            PurchaseResult.Purchased,        // Tonic: 13 -> 5
            PurchaseResult.InsufficientGold, // Elixir costs 8
            PurchaseResult.Purchased,        // Potion: 5 -> 0
            PurchaseResult.AlreadySold,
            PurchaseResult.AlreadySold,
            PurchaseResult.InsufficientGold,
            PurchaseResult.AlreadySold,
            PurchaseResult.AlreadySold,
            PurchaseResult.InsufficientGold,
        };

        for (int i = 0; i < attempts.Length; i++)
        {
            Assert.AreEqual(expected[i], merchant.Purchase(player, attempts[i]), $"Attempt {i + 1} (Item {attempts[i]})");
            Assert.IsTrue(player.Gold >= 0, $"Gold went negative after attempt {i + 1}: {player.Gold}");
        }

        AssertStats(player, gold: 0, health: 21, strength: 16, agility: 11);
        Assert.IsFalse(merchant.IsSold(3));
    }

    [TestMethod]
    public void Purchase_TwoDifferentItems_BothSucceedInOneEncounter()
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold: 20);

        Assert.AreEqual(PurchaseResult.Purchased, merchant.Purchase(player, 2));
        Assert.AreEqual(PurchaseResult.Purchased, merchant.Purchase(player, 3));

        AssertStats(player, gold: 4, health: 15, strength: 16, agility: 15);
        Assert.IsFalse(merchant.IsSold(1));
        Assert.IsTrue(merchant.IsSold(2));
        Assert.IsTrue(merchant.IsSold(3));
    }

    // R9: the cheapest Item (Healing Potion) costs 5 Gold.
    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    public void CanAffordAnyUnsoldItem_BelowFiveGold_IsFalse(int gold)
    {
        Assert.IsFalse(new Merchant().CanAffordAnyUnsoldItem(PlayerWith(gold)));
    }

    // With 7 Gold only the Potion (5) is within reach, and it is Sold; the Elixir costs 8.
    [TestMethod]
    public void CanAffordAnyUnsoldItem_EveryAffordableItemSold_IsFalse()
    {
        Merchant merchant = new Merchant();
        Player player = PlayerWith(gold: 20);
        merchant.Purchase(player, 1);
        merchant.Purchase(player, 2);
        player.Gold = 7;

        Assert.IsFalse(merchant.CanAffordAnyUnsoldItem(player));
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(7)]
    public void CanAffordAnyUnsoldItem_OnlyThePotionAffordable_IsTrue(int gold)
    {
        Assert.IsTrue(new Merchant().CanAffordAnyUnsoldItem(PlayerWith(gold)));
    }
}

// Seam 2: Game.HandleMerchantEncounter with scripted Console input and captured output.
[TestClass]
[DoNotParallelize] // These tests redirect the global Console.In/Out.
public class MerchantEncounterTests
{
    private TextReader? _originalIn;
    private TextWriter? _originalOut;

    [TestInitialize]
    public void Setup()
    {
        _originalIn = Console.In;
        _originalOut = Console.Out;
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_originalIn != null)
            Console.SetIn(_originalIn);

        if (_originalOut != null)
            Console.SetOut(_originalOut);
    }

    private static Messages GetMessages(string language = "English")
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    private static Player NewPlayer(int gold = 10) => new Player
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Strength = 12,
        Agility = 11,
        HealthPoints = 15,
        Gold = gold
    };

    private static string RunEncounter(string input, Player player, Merchant merchant, Messages messages)
    {
        Console.SetIn(new StringReader(input));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        new Game().HandleMerchantEncounter(player, merchant, messages);

        return writer.ToString();
    }

    private static void AssertInOrder(string output, params string[] expected)
    {
        int position = 0;
        foreach (string text in expected)
        {
            int found = output.IndexOf(text, position, StringComparison.Ordinal);
            Assert.IsTrue(found >= 0, $"Expected \"{text}\" after position {position} in output:\n{output}");
            position = found + text.Length;
        }
    }

    [TestMethod]
    public void Encounter_ShowsNarrativeGreetingGoldThenMenu_InOrder()
    {
        Messages messages = GetMessages();
        Merchant merchant = new Merchant();

        string output = RunEncounter("4\n", NewPlayer(gold: 13), merchant, messages);

        AssertInOrder(output,
            messages.GetMessage("south_path_narrative"),
            string.Format(messages.GetMessage("merchant_greeting"), merchant.Name),
            string.Format(messages.GetMessage("merchant_gold_display"), 13),
            messages.GetMessage("merchant_leave"));
    }

    [TestMethod]
    public void Menu_InEnglish_ShowsEachItemWithEffectAndPrice()
    {
        string output = RunEncounter("4\n", NewPlayer(), new Merchant(), GetMessages());

        AssertInOrder(output,
            "1. Healing Potion (+6 Health) - 5 Gold",
            "2. Strength Tonic (+4 Strength) - 8 Gold",
            "3. Agility Elixir (+4 Agility) - 8 Gold",
            "4. Leave");
    }

    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Menu_InOtherLanguages_ShowsTranslatedItemNamesAndEffects(string language)
    {
        Messages messages = GetMessages(language);
        Messages english = GetMessages();

        string output = RunEncounter("4\n", NewPlayer(), new Merchant(), messages);

        string line(int number, string item, int bonus, int price) => $"{number}. " + string.Format(
            messages.GetMessage("merchant_menu_line"),
            messages.GetMessage($"item_{item}"),
            string.Format(messages.GetMessage($"effect_{item}"), bonus), price);

        AssertInOrder(output,
            line(1, "healing_potion", 6, 5),
            line(2, "strength_tonic", 4, 8),
            line(3, "agility_elixir", 4, 8),
            "4. " + messages.GetMessage("merchant_leave"));

        foreach (string item in new[] { "healing_potion", "strength_tonic", "agility_elixir" })
            Assert.AreNotEqual(english.GetMessage($"item_{item}"), messages.GetMessage($"item_{item}"),
                $"{language} item_{item} is not translated.");
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void ChoosingLeave_ShowsFarewellAfterMenu(string language)
    {
        Messages messages = GetMessages(language);
        Merchant merchant = new Merchant();

        string output = RunEncounter("4\n", NewPlayer(), merchant, messages);

        AssertInOrder(output,
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_farewell"), merchant.Name));
        StringAssert.Contains(output, "Old Brannoc");
    }

    [TestMethod]
    [DataRow("x")]
    [DataRow("")]
    [DataRow("5")]
    [DataRow("leave")]
    public void InvalidEntry_ShowsInvalidThenMenuAgain_AndPaddedFourLeaves(string invalidEntry)
    {
        Messages messages = GetMessages();
        Merchant merchant = new Merchant();

        string output = RunEncounter($"{invalidEntry}\n 4 \n", NewPlayer(), merchant, messages);

        AssertInOrder(output,
            messages.GetMessage("merchant_leave"),
            messages.GetMessage("invalid"),
            "1. Healing Potion (+6 Health) - 5 Gold",
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_farewell"), merchant.Name));
    }

    // When input runs out (Console.ReadLine() returns null) the encounter leaves instead of looping forever.
    [TestMethod]
    [DataRow("", 0)]
    [DataRow("x\n", 1)]
    public void EndOfInput_LeavesWithFarewell(string input, int expectedInvalidCount)
    {
        Messages messages = GetMessages();
        Merchant merchant = new Merchant();
        Player player = NewPlayer(gold: 20);

        string output = RunEncounter(input, player, merchant, messages);

        string farewell = string.Format(messages.GetMessage("merchant_farewell"), merchant.Name);
        AssertInOrder(output, "4. Leave", farewell);
        Assert.IsTrue(output.TrimEnd().EndsWith(farewell), $"Farewell should be the last line:\n{output}");
        Assert.AreEqual(expectedInvalidCount,
            output.Split(messages.GetMessage("invalid")).Length - 1);
        Assert.AreEqual(20, player.Gold);
    }

    [TestMethod]
    public void BuyingAnItem_InEnglish_ShowsNewStatAndGoldLeft_ThenMenuAgain()
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 20);

        string output = RunEncounter("2\n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            "4. Leave",
            "You buy the Strength Tonic. Strength is now 16. You have 12 Gold left.",
            "1. Healing Potion (+6 Health) - 5 Gold",
            "4. Leave");
        Assert.AreEqual(12, player.Gold);
        Assert.AreEqual(16, player.Strength);
    }

    // Starting stats are HealthPoints 15, Strength 12, Agility 11 with 20 Gold.
    [TestMethod]
    [DataRow("English", "1", "healing_potion", "health", 21, 15)]
    [DataRow("Spanish", "2", "strength_tonic", "strength", 16, 12)]
    [DataRow("French", "3", "agility_elixir", "agility", 15, 12)]
    [DataRow("Italian", "1", "healing_potion", "health", 21, 15)]
    public void BuyingAnItem_ShowsTranslatedConfirmation_ThenMenuAgain(
        string language, string choice, string itemKey, string statKey, int newStat, int goldLeft)
    {
        Messages messages = GetMessages(language);

        string output = RunEncounter($"{choice}\n4\n", NewPlayer(gold: 20), new Merchant(), messages);

        AssertInOrder(output,
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_purchased"),
                messages.GetMessage($"item_{itemKey}"), messages.GetMessage($"stat_name_{statKey}"), newStat, goldLeft),
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_farewell"), "Old Brannoc"));
    }

    // AC8 and AC18: a Sold Item keeps its number and price and gains the Sold mark; other lines are unchanged.
    [TestMethod]
    public void AfterPurchase_SoldItemStaysListedWithMark_AndNumbersDoNotChange()
    {
        Messages messages = GetMessages();

        string output = RunEncounter("3\n4\n", NewPlayer(gold: 20), new Merchant(), messages);

        string afterPurchase = output[output.IndexOf("You buy the Agility Elixir. Agility is now 15.", StringComparison.Ordinal)..];
        AssertInOrder(afterPurchase,
            "1. Healing Potion (+6 Health) - 5 Gold" + Environment.NewLine,
            "2. Strength Tonic (+4 Strength) - 8 Gold" + Environment.NewLine,
            "3. Agility Elixir (+4 Agility) - 8 Gold [Sold]" + Environment.NewLine,
            "4. Leave");
        Assert.IsFalse(output[..output.IndexOf("You buy", StringComparison.Ordinal)].Contains("[Sold]"),
            "Nothing should be marked Sold before the purchase.");
    }

    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void AfterPurchase_SoldMarkIsTranslated(string language)
    {
        Messages messages = GetMessages(language);

        string output = RunEncounter("2\n4\n", NewPlayer(gold: 20), new Merchant(), messages);

        string soldLine = "2. " + string.Format(messages.GetMessage("merchant_menu_line"),
            messages.GetMessage("item_strength_tonic"),
            string.Format(messages.GetMessage("effect_strength_tonic"), 4), 8)
            + " " + messages.GetMessage("merchant_sold_mark");
        StringAssert.Contains(output, soldLine);
        Assert.AreNotEqual("[Sold]", messages.GetMessage("merchant_sold_mark"));
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void ChoosingSoldItem_ShowsAlreadyBought_ThenMenuAgain_AndChangesNothing(string language)
    {
        Messages messages = GetMessages(language);
        Player player = NewPlayer(gold: 20);

        string output = RunEncounter("1\n 1 \n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            string.Format(messages.GetMessage("merchant_already_sold"), messages.GetMessage("item_healing_potion")),
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_farewell"), "Old Brannoc"));
        Assert.AreEqual(15, player.Gold);
        Assert.AreEqual(21, player.HealthPoints);
    }

    [TestMethod]
    public void ChoosingSoldItem_InEnglish_ShowsAlreadyBoughtMessage()
    {
        string output = RunEncounter("1\n1\n4\n", NewPlayer(gold: 20), new Merchant(), GetMessages());

        StringAssert.Contains(output, "You have already bought the Healing Potion. Choose something else.");
    }

    // AC17: 7 Gold. The Tonic is refused with nothing changed and the menu shown again; the Potion
    // can still be bought, which leaves 2 Gold and ends the encounter automatically.
    [TestMethod]
    public void AC17_SevenGold_RefusedTonic_ThenPotion_EndsAutomatically()
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 7);

        string output = RunEncounter("2\n1\n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            NotEnoughGold(messages, 8, 7),
            PotionMenuLine(messages),
            LeaveLine(messages),
            Purchased(messages, "healing_potion", "health", 21, 2),
            Reason(messages),
            Farewell(messages));
        Assert.IsFalse(output.Contains(messages.GetMessage("merchant_sold_mark")),
            "The menu should not be shown again after the automatic end.");
        Assert.AreEqual(2, CountOf(output, LeaveLine(messages)), "Menu shown at the start and once after the refusal only.");
        Assert.AreEqual(2, player.Gold);
        Assert.AreEqual(21, player.HealthPoints);
        Assert.AreEqual(12, player.Strength);
        Assert.AreEqual(11, player.Agility);
    }

    // AC15: 13 Gold. Potion (8 left, +6 HealthPoints), then Tonic (0 left, +4 Strength), then the automatic end.
    [TestMethod]
    public void AC15_ThirteenGold_PotionThenTonic_EndsAutomatically()
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 13);

        string output = RunEncounter("1\n2\n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            Purchased(messages, "healing_potion", "health", 21, 8),
            LeaveLine(messages),
            Purchased(messages, "strength_tonic", "strength", 16, 0),
            Reason(messages),
            Farewell(messages));
        Assert.AreEqual(2, CountOf(output, LeaveLine(messages)));
        Assert.AreEqual(0, player.Gold);
        Assert.AreEqual(21, player.HealthPoints);
        Assert.AreEqual(16, player.Strength);
        Assert.AreEqual(11, player.Agility);
    }

    // AC16: 20 Gold. Tonic (12 left), then Elixir (4 left), then the automatic end; the Potion can't be bought too.
    [TestMethod]
    public void AC16_TwentyGold_TonicThenElixir_EndsAutomatically_PotionNotBought()
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 20);
        Merchant merchant = new Merchant();

        string output = RunEncounter("2\n3\n1\n4\n", player, merchant, messages);

        AssertInOrder(output,
            Purchased(messages, "strength_tonic", "strength", 16, 12),
            Purchased(messages, "agility_elixir", "agility", 15, 4),
            Reason(messages),
            Farewell(messages));
        Assert.IsFalse(merchant.IsSold(1), "Not all three Items can be bought.");
        Assert.AreEqual(4, player.Gold);
        Assert.AreEqual(15, player.HealthPoints);
        Assert.AreEqual(16, player.Strength);
        Assert.AreEqual(15, player.Agility);
    }

    [TestMethod]
    public void ChoosingUnaffordableItem_InEnglish_ShowsPriceAndCurrentGold()
    {
        string output = RunEncounter("2\n4\n", NewPlayer(gold: 5), new Merchant(), GetMessages());

        StringAssert.Contains(output, "You need 8 Gold but have only 5.");
    }

    // Prices from the R5 table. Gold is at least 5, because below 5 the menu never opens (AC1);
    // for the same reason the Healing Potion (5 Gold) can never be refused for lack of Gold.
    [TestMethod]
    [DataRow("English", "2", 8, 5)]
    [DataRow("Spanish", "2", 8, 7)]
    [DataRow("French", "3", 8, 5)]
    [DataRow("Italian", "3", 8, 6)]
    public void ChoosingUnaffordableItem_ShowsNotEnoughGold_ThenMenuAgain_AndChangesNothing(
        string language, string choice, int price, int gold)
    {
        Messages messages = GetMessages(language);
        Player player = NewPlayer(gold);

        string output = RunEncounter($"{choice}\n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_not_enough_gold"), price, gold),
            messages.GetMessage("merchant_leave"),
            string.Format(messages.GetMessage("merchant_farewell"), "Old Brannoc"));
        Assert.IsFalse(output.Contains(messages.GetMessage("invalid")), "A refused purchase is not an invalid entry.");
        Assert.IsFalse(output.Contains(messages.GetMessage("merchant_sold_mark")), "An unaffordable Item must not be marked Sold.");
        Assert.AreEqual(gold, player.Gold);
        Assert.AreEqual(15, player.HealthPoints);
        Assert.AreEqual(12, player.Strength);
        Assert.AreEqual(11, player.Agility);
    }

    private static string Reason(Messages messages) => messages.GetMessage("merchant_nothing_affordable");

    private static string Farewell(Messages messages) =>
        string.Format(messages.GetMessage("merchant_farewell"), "Old Brannoc");

    private static int CountOf(string output, string text) => output.Split(text).Length - 1;

    private static string LeaveLine(Messages messages) => "4. " + messages.GetMessage("merchant_leave");

    private static string PotionMenuLine(Messages messages) => "1. " + string.Format(
        messages.GetMessage("merchant_menu_line"),
        messages.GetMessage("item_healing_potion"),
        string.Format(messages.GetMessage("effect_healing_potion"), 6), 5);

    private static string Purchased(Messages messages, string item, string stat, int newValue, int goldLeft) =>
        string.Format(messages.GetMessage("merchant_purchased"),
            messages.GetMessage($"item_{item}"), messages.GetMessage($"stat_name_{stat}"), newValue, goldLeft);

    private static string NotEnoughGold(Messages messages, int price, int gold) =>
        string.Format(messages.GetMessage("merchant_not_enough_gold"), price, gold);

    // AC1: below 5 Gold the menu is never shown; the reason line and farewell follow the Gold line.
    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    public void StartingBelowFiveGold_ShowsNoMenu_ThenReasonThenFarewell(int gold)
    {
        Messages messages = GetMessages();
        Merchant merchant = new Merchant();
        Player player = NewPlayer(gold);

        string output = RunEncounter("1\n", player, merchant, messages);

        AssertInOrder(output,
            messages.GetMessage("south_path_narrative"),
            string.Format(messages.GetMessage("merchant_greeting"), merchant.Name),
            string.Format(messages.GetMessage("merchant_gold_display"), gold),
            Reason(messages),
            Farewell(messages));
        Assert.IsFalse(output.Contains(messages.GetMessage("merchant_leave")), "No menu should be shown.");
        Assert.IsFalse(output.Contains(messages.GetMessage("item_healing_potion")), "No menu should be shown.");
        Assert.IsFalse(output.Contains(messages.GetMessage("enter_choice")), "No choice should be asked for.");
        Assert.AreEqual(gold, player.Gold);
        Assert.AreEqual(15, player.HealthPoints);
    }

    // AC2: after buying the Tonic, 5 Gold is left and only the Potion is affordable. Each refused
    // choice shows the menu again instead of ending the encounter; Leave then ends it with no reason line.
    [TestMethod]
    [DataRow("2")] // Sold
    [DataRow("3")] // Unaffordable: the Elixir costs 8
    [DataRow("x")] // Invalid
    public void RefusedChoice_NeverEndsTheEncounterAutomatically(string refusedChoice)
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 13);

        string output = RunEncounter($"2\n{refusedChoice}\n{refusedChoice}\n4\n", player, new Merchant(), messages);

        string refusal = refusedChoice switch
        {
            "2" => string.Format(messages.GetMessage("merchant_already_sold"), messages.GetMessage("item_strength_tonic")),
            "3" => NotEnoughGold(messages, 8, 5),
            _ => messages.GetMessage("invalid"),
        };
        AssertInOrder(output,
            Purchased(messages, "strength_tonic", "strength", 16, 5),
            refusal, LeaveLine(messages),
            refusal, LeaveLine(messages),
            Farewell(messages));
        Assert.IsFalse(output.Contains(Reason(messages)), "A refused choice must not end the encounter.");
        Assert.AreEqual(5, player.Gold);
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void ChoosingLeave_ShowsOnlyTheFarewell_NoReasonLine(string language)
    {
        Messages messages = GetMessages(language);

        string output = RunEncounter("1\n4\n", NewPlayer(gold: 20), new Merchant(), messages);

        Assert.AreEqual(1, CountOf(output, Farewell(messages)));
        Assert.IsFalse(output.Contains(Reason(messages)), "Leave must not show the reason line.");
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void AutomaticEnd_ReasonLineThenFarewell_EndTheEncounter(string language)
    {
        Messages messages = GetMessages(language);

        string output = RunEncounter("1\n", NewPlayer(gold: 5), new Merchant(), messages);

        string ending = Reason(messages) + Environment.NewLine + Farewell(messages) + Environment.NewLine;
        Assert.IsTrue(output.EndsWith(ending), $"Expected the reason line then the farewell at the end:\n{output}");
    }

    // A full run in each other language: greeting, menu, two purchases, then the automatic end (AC15 numbers).
    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void FullEncounterRun_InOtherLanguages_PurchasesThenEndsAutomatically(string language)
    {
        Messages messages = GetMessages(language);
        Merchant merchant = new Merchant();
        Player player = NewPlayer(gold: 13);

        string output = RunEncounter("1\n2\n", player, merchant, messages);

        AssertInOrder(output,
            messages.GetMessage("south_path_narrative"),
            string.Format(messages.GetMessage("merchant_greeting"), merchant.Name),
            string.Format(messages.GetMessage("merchant_gold_display"), 13),
            LeaveLine(messages),
            messages.GetMessage("enter_choice"),
            Purchased(messages, "healing_potion", "health", 21, 8),
            messages.GetMessage("merchant_sold_mark"),
            Purchased(messages, "strength_tonic", "strength", 16, 0),
            Reason(messages),
            Farewell(messages));
        Assert.AreNotEqual(GetMessages().GetMessage("merchant_nothing_affordable"), Reason(messages),
            $"{language} reason line is not translated.");
        Assert.AreEqual(0, player.Gold);
        Assert.AreEqual(21, player.HealthPoints);
        Assert.AreEqual(16, player.Strength);
    }
}
