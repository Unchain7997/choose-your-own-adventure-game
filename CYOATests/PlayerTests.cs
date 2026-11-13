[TestClass]
[DoNotParallelize] // These tests redirect the global Console.In/Out.
public class PlayerTests
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

    private static Messages CreateMessages(string language)
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    // Race, name, occupation, four "roll"s (Strength, Agility, Health, Gold), then "2" to continue.
    private const string CharacterCreationInput =
        "human\nHero\nfighter\nroll\nroll\nroll\nroll\n2\n";

    private static string RunCharacterCreation(Player player, Messages messages)
    {
        Console.SetIn(new StringReader(CharacterCreationInput));
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        player.CreateCharacter(messages);

        return output.ToString();
    }

    [TestMethod]
    public void NewPlayer_ShouldHaveDefaultValues()
    {
        Player player = new Player();

        Assert.AreEqual("", player.Name);
        Assert.AreEqual("", player.Race);
        Assert.AreEqual("", player.Occupation);

        Assert.AreEqual(0, player.Strength);
        Assert.AreEqual(0, player.Agility);
        Assert.AreEqual(0, player.HealthPoints);
        Assert.AreEqual(0, player.Gold);

        Assert.IsNull(player.Weapon);
    }

    [TestMethod]
    public void CreateCharacter_RollsGoldBetween1And20()
    {
        Messages messages = CreateMessages("English");

        for (int run = 0; run < 20; run++)
        {
            Player player = new Player();
            RunCharacterCreation(player, messages);

            Assert.IsTrue(player.Gold >= 1 && player.Gold <= 20,
                $"Run {run}: Gold was {player.Gold}, expected 1-20.");
        }
    }

    [TestMethod]
    public void CreateCharacter_PromptsForGoldAfterHealthAndBeforeNextAction()
    {
        Messages messages = CreateMessages("English");

        string output = RunCharacterCreation(new Player(), messages);

        int healthPrompt = output.IndexOf(messages.GetMessage("roll_health"));
        int goldPrompt = output.IndexOf(messages.GetMessage("roll_gold"));
        int nextAction = output.IndexOf(messages.GetMessage("next_action"));

        Assert.IsTrue(healthPrompt >= 0, "Health roll prompt was not shown.");
        Assert.IsTrue(goldPrompt > healthPrompt, "Gold roll prompt was not shown after the Health roll prompt.");
        Assert.IsTrue(nextAction > goldPrompt, "Next-action menu was not shown after the Gold roll prompt.");
    }

    [TestMethod]
    [DataRow("English", "Gold: ")]
    [DataRow("Spanish", "Oro: ")]
    [DataRow("French", "Or: ")]
    [DataRow("Italian", "Oro: ")]
    public void DisplayStats_ShowsTranslatedGoldLabelAndValue(string language, string expectedLabel)
    {
        Messages messages = CreateMessages(language);
        Player player = new Player { Name = "Hero", Race = "Human", Occupation = "Fighter", Gold = 13 };
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        player.DisplayStats(messages);

        StringAssert.Contains(output.ToString(), expectedLabel + "13");
    }
}
