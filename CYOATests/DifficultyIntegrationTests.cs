// CYOATests/DifficultyIntegrationTests.cs
// Difficulty spec, Seam 5: full scripted adventures through StartGame with a FixedDie, proving the
// chosen Difficulty reaches both Monsters through the real prompt, creation and stats displays.
[TestClass]
[DoNotParallelize] // These tests redirect the global Console.In/Out.
public class DifficultyIntegrationTests
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

    // Reads the scripted lines, then throws instead of returning null. The game's other prompts loop
    // forever on end of input (spec L1), so a script that runs short fails fast rather than hanging.
    private sealed class ScriptedInput : TextReader
    {
        private readonly Queue<string> _lines;

        public ScriptedInput(IEnumerable<string> lines)
        {
            _lines = new Queue<string>(lines);
        }

        public override string? ReadLine()
        {
            if (_lines.Count == 0)
                throw new InvalidOperationException("The scripted input ran out before the game ended.");
            return _lines.Dequeue();
        }
    }

    private static Messages EnglishMessages()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        return messages;
    }

    // English; Create Character; the Difficulty; a Human Fighter with four rolls; proceed; then West
    // and retreat (once per troll visit); then North and retreat, which ends the adventure.
    private static string PlayAdventure(int dieValue, string difficultyChoice, int trollVisits)
    {
        var lines = new List<string> { "1", "1", difficultyChoice, "human", "Hero", "fighter", "roll", "roll", "roll", "roll", "2" };
        for (int i = 0; i < trollVisits; i++)
            lines.AddRange(new[] { "w", "r" });
        lines.AddRange(new[] { "n", "r" });

        Console.SetIn(new ScriptedInput(lines));
        StringWriter output = new StringWriter();
        Console.SetOut(output);

        new Game(new FixedDie(dieValue)).StartGame();

        return output.ToString();
    }

    // Every stats block the Monster displayed, found by its own translated header. Each block is
    // the 7 lines Monster.DisplayStats prints: header, name, Strength, Agility, HealthPoints, weapon, damage.
    private static List<List<string>> StatsBlocks(string output, string headerKey, Messages messages)
    {
        List<string> lines = output.Split(Environment.NewLine).ToList();
        string header = messages.GetMessage(headerKey);
        var blocks = new List<List<string>>();

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] == header)
                blocks.Add(lines.Skip(i).Take(7).ToList());
        }

        return blocks;
    }

    // Asserts the Strength, Agility and HealthPoints lines inside one Monster's own block, and that
    // the block carries no Difficulty label (AC6).
    private static void AssertBlock(List<string> block, string name, int expected, Messages messages, string which)
    {
        Assert.AreEqual(7, block.Count, $"{which}: stats block is incomplete.");
        Assert.AreEqual(messages.GetMessage("stats_name") + name, block[1], $"{which}: name line");
        Assert.AreEqual(messages.GetMessage("stats_strength") + expected, block[2], $"{which}: Strength line");
        Assert.AreEqual(messages.GetMessage("stats_agility") + expected, block[3], $"{which}: Agility line");
        Assert.AreEqual(messages.GetMessage("stats_health") + expected, block[4], $"{which}: HealthPoints line");
        Assert.IsFalse(block.Any(line => line.Contains(messages.GetMessage("stats_difficulty"))),
            $"{which}: stats block must not show a Difficulty label.");
    }

    private static void AssertAdventure(string output, int expected, int trollVisits, Messages messages)
    {
        List<List<string>> troll = StatsBlocks(output, "troll_stats_header", messages);
        List<List<string>> dragon = StatsBlocks(output, "dragon_stats_header", messages);

        Assert.AreEqual(trollVisits, troll.Count, "Grimtusk's stats should be shown once per visit.");
        Assert.AreEqual(1, dragon.Count, "Smolderfang's stats should be shown once.");

        for (int visit = 0; visit < troll.Count; visit++)
            AssertBlock(troll[visit], "Grimtusk", expected, messages, $"Grimtusk, visit {visit + 1}");
        AssertBlock(dragon[0], "Smolderfang", expected, messages, "Smolderfang");
    }

    // AC1: the difficulty prompt appears after Create Character and before the race prompt.
    private static void AssertPromptBeforeCharacterCreation(string output, Messages messages)
    {
        int prompt = output.IndexOf(messages.GetMessage("difficulty_prompt"));
        int race = output.IndexOf(messages.GetMessage("race_prompt"));
        int menu = output.IndexOf(messages.GetMessage("menu"));

        Assert.IsTrue(menu >= 0 && prompt > menu, "The difficulty prompt should follow the main menu.");
        Assert.IsTrue(race > prompt, "The difficulty prompt should come before the race prompt.");
    }

    // Run 1, AC4/AC8/AC12: Hard raises both Monsters by 2, once. A second troll visit after a
    // retreat shows the same 12s, so the Difficulty isn't applied again.
    [TestMethod]
    public void Hard_WithDieTen_BothMonstersShowTwelve_AndGrimtuskStaysTwelveOnSecondVisit()
    {
        Messages messages = EnglishMessages();

        string output = PlayAdventure(dieValue: 10, difficultyChoice: "3", trollVisits: 2);

        AssertPromptBeforeCharacterCreation(output, messages);
        AssertAdventure(output, expected: 12, trollVisits: 2, messages);
    }

    // Run 2, AC10/AC12: Normal shows the rolled values.
    [TestMethod]
    public void Normal_WithDieTen_BothMonstersShowTen()
    {
        Messages messages = EnglishMessages();

        string output = PlayAdventure(dieValue: 10, difficultyChoice: "2", trollVisits: 1);

        AssertPromptBeforeCharacterCreation(output, messages);
        AssertAdventure(output, expected: 10, trollVisits: 1, messages);
    }

    // Run 3, AC4/AC12: Easy lowers both Monsters by 2.
    [TestMethod]
    public void Easy_WithDieTen_BothMonstersShowEight()
    {
        Messages messages = EnglishMessages();

        string output = PlayAdventure(dieValue: 10, difficultyChoice: "1", trollVisits: 1);

        AssertPromptBeforeCharacterCreation(output, messages);
        AssertAdventure(output, expected: 8, trollVisits: 1, messages);
    }

    // Run 4, AC4/AC12: Easy never takes a stat below the minimum of 1.
    [TestMethod]
    public void Easy_WithDieOne_BothMonstersStayAtTheMinimumOfOne()
    {
        Messages messages = EnglishMessages();

        string output = PlayAdventure(dieValue: 1, difficultyChoice: "1", trollVisits: 1);

        AssertPromptBeforeCharacterCreation(output, messages);
        AssertAdventure(output, expected: 1, trollVisits: 1, messages);
    }
}
