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
    public void Catalogue_HasThreeItems_AndLeaveIsOptionFour()
    {
        Merchant merchant = new Merchant();

        Assert.AreEqual(3, merchant.ItemCount);
        Assert.AreEqual(4, merchant.LeaveOption);
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

    // Temporary until Ticket 03 adds purchases: Item choices behave like invalid input.
    [TestMethod]
    [DataRow("1")]
    [DataRow("2")]
    [DataRow("3")]
    public void ChoosingAnItem_ForNow_ShowsInvalidAndChangesNothing(string choice)
    {
        Messages messages = GetMessages();
        Player player = NewPlayer(gold: 20);

        string output = RunEncounter($"{choice}\n4\n", player, new Merchant(), messages);

        AssertInOrder(output,
            messages.GetMessage("merchant_leave"),
            messages.GetMessage("invalid"),
            messages.GetMessage("merchant_leave"));
        Assert.AreEqual(20, player.Gold);
        Assert.AreEqual(15, player.HealthPoints);
        Assert.AreEqual(12, player.Strength);
        Assert.AreEqual(11, player.Agility);
    }
}
