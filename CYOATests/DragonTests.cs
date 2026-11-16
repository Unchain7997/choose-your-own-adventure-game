using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class DragonTests
{

    private Messages GetMessages(string language = "English")
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void NewDragon_InitializesRequiredProperties()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);

        Assert.AreEqual("Smolderfang", dragon.Name);
        Assert.IsTrue(dragon.Strength >= 1 && dragon.Strength <= 20);
        Assert.IsTrue(dragon.Agility >= 1 && dragon.Agility <= 20);
        Assert.IsTrue(dragon.HealthPoints >= 1 && dragon.HealthPoints <= 20);

        Assert.IsNotNull(dragon.Weapon);
        Assert.AreEqual("claws", dragon.Weapon.Type);
        Assert.AreEqual(12, dragon.Weapon.MaxDamage);
        Assert.IsFalse(string.IsNullOrWhiteSpace(dragon.Weapon.AsciiArt));
    }

    [TestMethod]
    public void GetRandomDefenseTaunt_ReturnsExpectedTaunt()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);
        Messages messages = GetMessages();

        string taunt = dragon.GetRandomDefenseTaunt(messages);

        string[] validTaunts =
        {
        messages.GetMessage("dragon_defense_taunt_1"),
        messages.GetMessage("dragon_defense_taunt_2"),
        messages.GetMessage("dragon_defense_taunt_3"),
        messages.GetMessage("dragon_defense_taunt_4")
    };

        CollectionAssert.Contains(validTaunts, taunt);
    }

    [TestMethod]
    public void GetRandomDamageReply_ReturnsExpectedReply()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);
        Messages messages = GetMessages();

        string reply = dragon.GetRandomDamageReply(messages);

        string[] validReplies =
        {
        messages.GetMessage("dragon_damage_reply_1"),
        messages.GetMessage("dragon_damage_reply_2"),
        messages.GetMessage("dragon_damage_reply_3"),
        messages.GetMessage("dragon_damage_reply_4")
    };

        CollectionAssert.Contains(validReplies, reply);
    }

    [TestMethod]
    public void Attack_ReturnsNonEmptyResultString()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);

        Player player = new Player
        {
            Name = "TestHero",
            Agility = 10,
            HealthPoints = 30
        };

        string result = dragon.Attack(player, GetMessages());

        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
    }

    [TestMethod]
    public void Attack_DoesNotIncreasePlayerHealth()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);

        Player player = new Player
        {
            Name = "TestHero",
            Agility = 10,
            HealthPoints = 30
        };

        int startingHealth = player.HealthPoints;

        string result = dragon.Attack(player, GetMessages());

        Assert.IsTrue(player.HealthPoints <= startingHealth);
    }

    [TestMethod]
    public void Attack_KeepsPlayerHealthNonNegative()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);

        Player player = new Player
        {
            Name = "TestHero",
            Agility = 0,
            HealthPoints = 1
        };

        string result = dragon.Attack(player, GetMessages());

        Assert.IsTrue(player.HealthPoints >= 0);
    }

    // R3: the Dragon is one kind of Monster.
    [TestMethod]
    public void Dragon_IsAMonster()
    {
        Assert.IsInstanceOfType<Monster>(new Dragon(new Die()));
    }

    // The player's attack on the dragon, pinned down before combat is generalised to any Monster.
    // Extreme stats make each result certain: a d20 roll of 1-20 always hits Strength 20 and never
    // hits Strength 0; Agility 0 never blocks and Agility 20 always does; damage is always at least 1.
    private static Player AttackingPlayer(int strength) => new Player
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Strength = strength,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    private static Dragon DefendingDragon(int agility, int healthPoints)
    {
        Dragon dragon = new Dragon(new Die());
        dragon.Agility = agility;
        dragon.HealthPoints = healthPoints;
        return dragon;
    }

    private static string[] AllFourFormatted(Messages messages, string keyPrefix, string race) =>
        Enumerable.Range(1, 4)
            .Select(n => string.Format(messages.GetMessage($"{keyPrefix}_{n}"), race))
            .ToArray();

    [TestMethod]
    public void PlayerAttack_OnDragon_ShowsTheDragonAttackIntro()
    {
        Messages messages = GetMessages();

        string output = AttackingPlayer(strength: 0).Attack(DefendingDragon(agility: 0, healthPoints: 10), messages);

        StringAssert.Contains(output, string.Format(messages.GetMessage("player_attack_intro"), "Fighter", "long sword"));
    }

    [TestMethod]
    public void PlayerAttack_KillingBlowOnDragon_ShowsDragonDefeatedNarrative()
    {
        Messages messages = GetMessages();

        string output = AttackingPlayer(strength: 20).Attack(DefendingDragon(agility: 0, healthPoints: 1), messages);

        StringAssert.Contains(output, messages.GetMessage("dragon_defeated_narrative"));
    }

    [TestMethod]
    public void PlayerAttack_BlockedByDragon_ShowsOneOfTheDragonsTaunts()
    {
        Messages messages = GetMessages();

        string output = AttackingPlayer(strength: 20).Attack(DefendingDragon(agility: 20, healthPoints: 10), messages);

        string[] lines = output.Split(Environment.NewLine);
        Assert.IsTrue(AllFourFormatted(messages, "dragon_defense_taunt", "Human").Any(lines.Contains),
            $"Expected one of the dragon's taunts in:\n{output}");
    }

    [TestMethod]
    public void PlayerAttack_HitThatDoesNotKillDragon_ShowsOneOfTheDragonsDamageReplies()
    {
        Messages messages = GetMessages();

        string output = AttackingPlayer(strength: 20).Attack(DefendingDragon(agility: 0, healthPoints: 100), messages);

        string[] lines = output.Split(Environment.NewLine);
        Assert.IsTrue(AllFourFormatted(messages, "dragon_damage_reply", "Human").Any(lines.Contains),
            $"Expected one of the dragon's damage replies in:\n{output}");
    }
}

[TestClass]
[DoNotParallelize]
public class GamePbi18Tests
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

    [TestMethod]
    public void DragonDisplayStats_UsesLocalizedLabels()
    {
        Die die = new Die();
        Dragon dragon = new Dragon(die);

        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();

        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        dragon.DisplayStats(messages);

        string output = writer.ToString();

        StringAssert.Contains(output, messages.GetMessage("dragon_stats_header"));
        StringAssert.Contains(output, messages.GetMessage("stats_name"));
        StringAssert.Contains(output, messages.GetMessage("stats_strength"));
        StringAssert.Contains(output, messages.GetMessage("stats_agility"));
        StringAssert.Contains(output, messages.GetMessage("stats_health"));
        StringAssert.Contains(output, messages.GetMessage("stats_weapon"));
        StringAssert.Contains(output, messages.GetMessage("stats_damage"));
    }

    [TestMethod]
    public void HandleDragonEncounter_AttackPath_DisplaysEncounterFlowText()
    {
        Game game = new Game();
        Die die = new Die();
        Dragon dragon = new Dragon(die);

        Player player = new Player
        {
            Name = "TestHero",
            Race = "Human",
            Occupation = "Fighter",
            Strength = 20,
            Agility = 20,
            HealthPoints = 25,
            Weapon = new Weapon("long sword", 12, "-)=====>")
        };

        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();

        // "a" enters combat, then "r" retreats from Combat immediately
        Console.SetIn(new StringReader("a\nr\n"));

        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        MethodInfo? method = typeof(Game).GetMethod(
            "HandleDragonEncounter",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method, "HandleDragonEncounter method was not found.");

        method!.Invoke(game, new object[] { player, dragon, messages });

        string output = writer.ToString();

        StringAssert.Contains(output, messages.GetMessage("north_path_narrative"));
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_intro"), dragon.Name));
        StringAssert.Contains(output, messages.GetMessage("dragon_stats_intro"));
        StringAssert.Contains(output, messages.GetMessage("dragon_encounter_prompt"));
    }

    [TestMethod]
    public void DragonEncounter_MessageKeys_AreLoaded()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();

        Assert.AreNotEqual("[north_path_narrative]", messages.GetMessage("north_path_narrative"));
        Assert.AreNotEqual("[dragon_intro]", messages.GetMessage("dragon_intro"));
        Assert.AreNotEqual("[dragon_stats_intro]", messages.GetMessage("dragon_stats_intro"));
        Assert.AreNotEqual("[dragon_encounter_prompt]", messages.GetMessage("dragon_encounter_prompt"));
        Assert.AreNotEqual("[dragon_stats_header]", messages.GetMessage("dragon_stats_header"));
    }
}
