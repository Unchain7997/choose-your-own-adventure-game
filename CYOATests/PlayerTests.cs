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

    // Difficulty spec R18: a new Player plays on Normal until a Difficulty is chosen.
    [TestMethod]
    public void NewPlayer_DifficultyIsNormal()
    {
        Assert.AreEqual(Difficulty.Normal, new Player().Difficulty);
    }

    // Difficulty spec AC7: the translated Difficulty line comes immediately after Gold and
    // immediately before Weapon when a weapon is shown.
    [TestMethod]
    [DataRow("English", Difficulty.Easy, "difficulty_easy")]
    [DataRow("English", Difficulty.Normal, "difficulty_normal")]
    [DataRow("English", Difficulty.Hard, "difficulty_hard")]
    [DataRow("Spanish", Difficulty.Hard, "difficulty_hard")]
    [DataRow("French", Difficulty.Easy, "difficulty_easy")]
    [DataRow("Italian", Difficulty.Normal, "difficulty_normal")]
    public void DisplayStats_ShowsDifficultyRightAfterGold_BeforeWeapon(string language, Difficulty difficulty, string nameKey)
    {
        Messages messages = CreateMessages(language);
        Player player = new Player
        {
            Name = "Hero", Race = "Human", Occupation = "Fighter", Gold = 13,
            Weapon = new Weapon("long sword", 12, "-)=====>"),
            Difficulty = difficulty
        };
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        player.DisplayStats(messages);

        List<string> lines = output.ToString().Split(Environment.NewLine).ToList();
        int gold = lines.IndexOf(messages.GetMessage("stats_gold") + 13);
        Assert.IsTrue(gold >= 0, "Gold line not found.");
        Assert.AreEqual(messages.GetMessage("stats_difficulty") + messages.GetMessage(nameKey), lines[gold + 1]);
        Assert.AreEqual(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay("long sword"), lines[gold + 2]);
    }

    // An unknown Difficulty is rejected, never shown as Normal or as a "[key]" placeholder.
    [TestMethod]
    public void DisplayStats_UnknownDifficulty_IsRejected()
    {
        Messages messages = CreateMessages("English");
        Player player = new Player { Name = "Hero", Race = "Human", Occupation = "Fighter", Difficulty = (Difficulty)99 };
        Console.SetOut(new StringWriter());

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => player.DisplayStats(messages));
    }

    // AC14: the Strength Tonic's +4 reaches combat through AttackHits, with no combat code change.
    [TestMethod]
    public void AfterBuyingStrengthTonic_RollEqualToNewStrength_NowHits()
    {
        Player player = new Player { Strength = 12, Gold = 8 };
        Assert.IsFalse(player.AttackHits(16), "A roll of 16 should miss with Strength 12.");

        Assert.AreEqual(PurchaseResult.Purchased, new Merchant().Purchase(player, 2));

        Assert.AreEqual(16, player.Strength);
        Assert.IsTrue(player.AttackHits(16));
    }
}
