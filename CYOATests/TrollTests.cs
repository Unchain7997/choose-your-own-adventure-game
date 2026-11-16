// CYOATests/TrollTests.cs
// Seam 1: Grimtusk with plain values, no console.
[TestClass]
public class TrollTests
{
    // Enough fresh rolls that every value of a d12 shows up; each run only asserts "in range", so it can't flake.
    private const int ManyRolls = 200;

    [TestMethod]
    public void NewTroll_IsGrimtusk_AMonsterWithAClub()
    {
        Troll troll = new Troll(new Die());

        Assert.IsInstanceOfType<Monster>(troll);
        Assert.AreEqual("Grimtusk", troll.Name);
        Assert.AreEqual("club", troll.Weapon.Type);
        Assert.AreEqual(8, troll.Weapon.MaxDamage);
    }

    // AC3: Strength d12, Agility d10, HealthPoints d12.
    [TestMethod]
    public void NewTroll_RollsStatsWithinTheirDice()
    {
        Die die = new Die();

        for (int i = 0; i < ManyRolls; i++)
        {
            Troll troll = new Troll(die);

            Assert.IsTrue(troll.Strength is >= 1 and <= 12, $"Strength {troll.Strength}");
            Assert.IsTrue(troll.Agility is >= 1 and <= 10, $"Agility {troll.Agility}");
            Assert.IsTrue(troll.HealthPoints is >= 1 and <= 12, $"HealthPoints {troll.HealthPoints}");
        }
    }

    // AC4: a roll at or below Strength hits.
    [TestMethod]
    [DataRow(7, 7, true)]
    [DataRow(6, 7, true)]
    [DataRow(8, 7, false)]
    public void AttackHits_WhenRollIsAtOrBelowStrength(int roll, int strength, bool expected)
    {
        Troll troll = new Troll(new Die()) { Strength = strength };

        Assert.AreEqual(expected, troll.AttackHits(roll));
    }

    // AC4: the club does 1-8 damage.
    [TestMethod]
    public void RollDamage_IsBetweenOneAndEight()
    {
        Troll troll = new Troll(new Die());

        for (int i = 0; i < ManyRolls; i++)
        {
            int damage = troll.RollDamage();
            Assert.IsTrue(damage is >= 1 and <= 8, $"Damage {damage}");
        }
    }
}

// Seam 2: Game.HandleTrollEncounter with scripted Console input and captured output.
[TestClass]
[DoNotParallelize] // These tests redirect the global Console.In/Out.
public class TrollEncounterTests
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

    private static Player NewPlayer() => new Player
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Strength = 12,
        Agility = 11,
        HealthPoints = 15,
        Gold = 10,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    private static (string Output, TrollEncounterOutcome Outcome) RunEncounter(
        string input, Player player, Troll troll, Messages messages)
    {
        Console.SetIn(new StringReader(input));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        TrollEncounterOutcome outcome = new Game().HandleTrollEncounter(player, troll, messages);

        return (writer.ToString(), outcome);
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

    // GetMessage returns "[key]" for a missing key, which would let an order test pass with no text at all.
    private static void AssertNoMissingMessages(string output)
    {
        var missing = System.Text.RegularExpressions.Regex.Matches(output, @"\[[a-z]+(_[a-z0-9]+)+\]");
        Assert.AreEqual(0, missing.Count, $"Missing messages: {string.Join(", ", missing)}\n{output}");
    }

    // AC5: narrative, intro with his name, stats intro, stats under the troll header, then the prompt.
    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void FirstVisit_ShowsNarrativeIntroStatsThenPrompt_InOrder(string language)
    {
        Messages messages = GetMessages(language);
        Troll troll = new Troll(new Die());

        (string output, _) = RunEncounter("r\n", NewPlayer(), troll, messages);

        AssertInOrder(output,
            messages.GetMessage("west_path_narrative"),
            string.Format(messages.GetMessage("troll_intro"), troll.Name),
            messages.GetMessage("troll_stats_intro"),
            messages.GetMessage("troll_stats_header"),
            messages.GetMessage("stats_name") + troll.Name,
            messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay("club"),
            messages.GetMessage("dragon_encounter_prompt"));
        AssertNoMissingMessages(output);
    }

    private static void AssertStats(Player player, Troll troll, (int, int, int, int) playerBefore, (int, int, int) trollBefore)
    {
        Assert.AreEqual(playerBefore, (player.Strength, player.Agility, player.HealthPoints, player.Gold), "Player stats changed");
        Assert.AreEqual(trollBefore, (troll.Strength, troll.Agility, troll.HealthPoints), "Grimtusk's stats changed");
    }

    private static int Count(string output, string text) => output.Split(text).Length - 1;

    // AC10: invalid input shows the existing invalid message and the prompt again; padded input is accepted.
    [TestMethod]
    [DataRow("x")]
    [DataRow("")]
    [DataRow("north")]
    [DataRow("fight")]
    public void InvalidEntry_ShowsInvalidThenPromptAgain_AndPaddedRRetreats(string invalidEntry)
    {
        Messages messages = GetMessages();
        Troll troll = new Troll(new Die());

        (string output, TrollEncounterOutcome outcome) = RunEncounter($"{invalidEntry}\n r \n", NewPlayer(), troll, messages);

        AssertInOrder(output,
            messages.GetMessage("dragon_encounter_prompt"),
            messages.GetMessage("invalid"),
            messages.GetMessage("dragon_encounter_prompt"),
            messages.GetMessage("troll_retreat"));
        Assert.AreEqual(1, Count(output, messages.GetMessage("invalid")));
        Assert.AreEqual(TrollEncounterOutcome.Retreat, outcome);
    }

    // AC11: running out of input (ReadLine returns null) counts as a retreat instead of looping forever.
    // The timeout turns a regression into a fast failure rather than a hung test run.
    [TestMethod]
    [Timeout(5000)]
    [DataRow("", 0)]
    [DataRow("x\n", 1)]
    public void EndOfInput_RetreatsToTheCrossroads(string input, int expectedInvalidCount)
    {
        Messages messages = GetMessages();
        Troll troll = new Troll(new Die());

        (string output, TrollEncounterOutcome outcome) = RunEncounter(input, NewPlayer(), troll, messages);

        Assert.AreEqual(TrollEncounterOutcome.Retreat, outcome);
        Assert.IsTrue(output.TrimEnd().EndsWith(messages.GetMessage("troll_retreat")),
            $"troll_retreat should be the last line:\n{output}");
        Assert.AreEqual(expectedInvalidCount, Count(output, messages.GetMessage("invalid")));
    }

    // Ticket 02 only (removed by Ticket 03): attack is a valid choice, so it isn't answered as invalid.
    // It shows the temporary "fight not ready" notice and the prompt again, and starts no combat.
    [TestMethod]
    [DataRow("a")]
    [DataRow("attack")]
    [DataRow(" A ")]
    public void Attack_ShowsTemporaryFightNotReadyNotice_AndStartsNoCombat(string attackEntry)
    {
        Messages messages = GetMessages();
        Player player = NewPlayer();
        Troll troll = new Troll(new Die());
        var playerBefore = (player.Strength, player.Agility, player.HealthPoints, player.Gold);
        var trollBefore = (troll.Strength, troll.Agility, troll.HealthPoints);

        (string output, TrollEncounterOutcome outcome) = RunEncounter($"{attackEntry}\nr\n", player, troll, messages);

        AssertInOrder(output,
            messages.GetMessage("dragon_encounter_prompt"),
            string.Format(messages.GetMessage("troll_fight_not_ready"), troll.Name),
            messages.GetMessage("dragon_encounter_prompt"),
            messages.GetMessage("troll_retreat"));
        Assert.AreEqual(0, Count(output, messages.GetMessage("invalid")), "Attack must not be answered as invalid");
        Assert.AreEqual(0, Count(output, messages.GetMessage("combat_stats_header")), "No combat may start");
        Assert.AreEqual(TrollEncounterOutcome.Retreat, outcome);
        AssertStats(player, troll, playerBefore, trollBefore);
        AssertNoMissingMessages(output);
    }

    // AC8: retreating before combat prints troll_retreat, reports a retreat and changes nothing.
    [TestMethod]
    [DataRow("r")]
    [DataRow("retreat")]
    [DataRow("R")]
    [DataRow(" RETREAT ")]
    public void RetreatBeforeCombat_ReturnsToTheCrossroads_AndChangesNothing(string retreatEntry)
    {
        Messages messages = GetMessages();
        Player player = NewPlayer();
        Troll troll = new Troll(new Die());
        var playerBefore = (player.Strength, player.Agility, player.HealthPoints, player.Gold);
        var trollBefore = (troll.Strength, troll.Agility, troll.HealthPoints);

        (string output, TrollEncounterOutcome outcome) = RunEncounter($"{retreatEntry}\n", player, troll, messages);

        Assert.AreEqual(TrollEncounterOutcome.Retreat, outcome);
        Assert.IsTrue(output.TrimEnd().EndsWith(messages.GetMessage("troll_retreat")),
            $"troll_retreat should be the last line:\n{output}");
        Assert.AreEqual(0, Count(output, messages.GetMessage("combat_stats_header")), "No combat may start");
        Assert.AreEqual(0, Count(output, messages.GetMessage("retreat")), "The dragon's retreat ending must not appear");
        AssertStats(player, troll, playerBefore, trollBefore);
    }

    // AC15: the same Grimtusk is still at the pass, and the next visit plays the full encounter again.
    [TestMethod]
    public void SecondVisitAfterRetreat_PlaysTheFullEncounterAgain_WithTheSameStats()
    {
        Messages messages = GetMessages();
        Player player = NewPlayer();
        Troll troll = new Troll(new Die());
        var playerBefore = (player.Strength, player.Agility, player.HealthPoints, player.Gold);
        var trollBefore = (troll.Strength, troll.Agility, troll.HealthPoints);
        RunEncounter("r\n", player, troll, messages);

        (string output, TrollEncounterOutcome outcome) = RunEncounter("r\n", player, troll, messages);

        AssertInOrder(output,
            messages.GetMessage("west_path_narrative"),
            string.Format(messages.GetMessage("troll_intro"), troll.Name),
            messages.GetMessage("troll_stats_header"),
            messages.GetMessage("stats_strength") + trollBefore.Strength,
            messages.GetMessage("stats_agility") + trollBefore.Agility,
            messages.GetMessage("stats_health") + trollBefore.HealthPoints,
            messages.GetMessage("dragon_encounter_prompt"),
            messages.GetMessage("troll_retreat"));
        Assert.AreEqual(TrollEncounterOutcome.Retreat, outcome);
        AssertStats(player, troll, playerBefore, trollBefore);
    }

    // AC17: each adventure creates its own Grimtusk (Game does this alongside the Player and Merchant).
    // A whole adventure can't be run in a test (spec Known Limitation L4), so this checks a new Troll directly.
    [TestMethod]
    public void NewAdventure_FreshGrimtusk_IsUnhurtAndPlaysTheFullEncounter()
    {
        Messages messages = GetMessages();
        Troll firstAdventure = new Troll(new Die());
        firstAdventure.HealthPoints = 1; // fixture: an injured Grimtusk from an earlier adventure
        RunEncounter("r\n", NewPlayer(), firstAdventure, messages);

        Troll secondAdventure = new Troll(new Die());
        (string output, _) = RunEncounter("r\n", NewPlayer(), secondAdventure, messages);

        Assert.AreNotSame(firstAdventure, secondAdventure);
        Assert.IsTrue(secondAdventure.HealthPoints is >= 1 and <= 12, $"HealthPoints {secondAdventure.HealthPoints}");
        AssertInOrder(output,
            messages.GetMessage("west_path_narrative"),
            messages.GetMessage("stats_health") + secondAdventure.HealthPoints,
            messages.GetMessage("troll_retreat"));
    }
}
