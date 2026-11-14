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

    // AC17 flow, without the automatic end (Ticket 05): 7 Gold, refused the Tonic, still buys the Potion.
    [TestMethod]
    public void AfterRefusal_PlayerCanStillBuyAnAffordableItem()
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 7);

        string output = RunEncounter("2\n1\n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            "You need 8 Gold but have only 7.",
            "4. Leave",
            "You buy the Healing Potion. Health is now 21. You have 2 Gold left.",
            "1. Healing Potion (+6 Health) - 5 Gold [Sold]");
        Assert.AreEqual(2, player.Gold);
        Assert.AreEqual(21, player.HealthPoints);
        Assert.AreEqual(12, player.Strength);
    }

    [TestMethod]
    public void ChoosingUnaffordableItem_InEnglish_ShowsPriceAndCurrentGold()
    {
        string output = RunEncounter("2\n4\n", NewPlayer(gold: 5), new Merchant(), GetMessages());

        StringAssert.Contains(output, "You need 8 Gold but have only 5.");
    }

    // Prices from the R5 table; Gold is one short of each price (or far short).
    [TestMethod]
    [DataRow("English", "1", 5, 4)]
    [DataRow("Spanish", "2", 8, 7)]
    [DataRow("French", "3", 8, 5)]
    [DataRow("Italian", "2", 8, 0)]
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
}
