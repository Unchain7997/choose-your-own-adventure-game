// CYOATests/MonsterAttackCharacterizationTests.cs
// Attack-resolution spec, Seam 1: pins Monster.Attack's exact output, the Player's HealthPoints and the
// complete roll sequence. Written and green on the code before the refactor, and left unchanged after it.
[TestClass]
public class MonsterAttackCharacterizationTests
{
    // A Monster's constructor rolls its three stats on the Die it is given, so every script starts
    // with these three throwaway values; the stats are then set explicitly.
    private static readonly int[] ConstructorRolls = { 1, 1, 1 };

    private static Messages EnglishMessages()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        return messages;
    }

    private static (Dragon dragon, ScriptedDie die) DragonWithStrength(int strength, params int[] attackRolls)
    {
        ScriptedDie die = new ScriptedDie(ConstructorRolls.Concat(attackRolls).ToArray());
        Dragon dragon = new Dragon(die) { Strength = strength, Agility = 10, HealthPoints = 10 };
        return (dragon, die);
    }

    private static Player TargetPlayer(int agility, int healthPoints)
    {
        return new Player { Name = "Hero", Race = "Human", Occupation = "Fighter", Agility = agility, HealthPoints = healthPoints };
    }

    // The rolls made by Attack itself, after the constructor's three stat rolls.
    private static int[] AttackRolls(ScriptedDie die) => die.SidesRolled.Skip(ConstructorRolls.Length).ToArray();

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);

    private static string Intro(Monster monster, Player player, Messages messages) =>
        string.Format(messages.GetMessage("dragon_attack_intro"), monster.Name, player.Name,
            messages.TranslateWeaponForDisplay(monster.Weapon.Type));

    [TestMethod]
    public void Miss_HitRollAboveStrength_OneRoll_HealthUnchanged()
    {
        Messages messages = EnglishMessages();
        (Dragon dragon, ScriptedDie die) = DragonWithStrength(10, 11);
        Player player = TargetPlayer(agility: 7, healthPoints: 25);

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(Lines(
            Intro(dragon, player, messages),
            "<<< claws >>>",
            string.Format(messages.GetMessage("dragon_missed_player"), "Smolderfang", "Hero")), output);
        Assert.AreEqual(25, player.HealthPoints);
        CollectionAssert.AreEqual(new[] { 20 }, AttackRolls(die));
        Assert.IsTrue(die.IsExhausted);
    }

    // A hit roll equal to Strength hits; a block roll equal to Agility blocks.
    [TestMethod]
    public void Block_EqualRolls_TwoRolls_HealthUnchanged()
    {
        Messages messages = EnglishMessages();
        (Dragon dragon, ScriptedDie die) = DragonWithStrength(10, 10, 7);
        Player player = TargetPlayer(agility: 7, healthPoints: 25);

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(Lines(
            Intro(dragon, player, messages),
            "<<< claws >>>",
            string.Format(messages.GetMessage("dragon_hit_player"), "Smolderfang", "Hero"),
            string.Format(messages.GetMessage("player_defended_dragon_attack"), "Hero")), output);
        Assert.AreEqual(25, player.HealthPoints);
        CollectionAssert.AreEqual(new[] { 20, 20 }, AttackRolls(die));
        Assert.IsTrue(die.IsExhausted);
    }

    [TestMethod]
    public void Hit_PlayerSurvives_ThreeRolls_DamageSubtracted()
    {
        Messages messages = EnglishMessages();
        (Dragon dragon, ScriptedDie die) = DragonWithStrength(10, 5, 8, 9);
        Player player = TargetPlayer(agility: 7, healthPoints: 25);

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(Lines(
            Intro(dragon, player, messages),
            "<<< claws >>>",
            string.Format(messages.GetMessage("dragon_hit_player"), "Smolderfang", "Hero"),
            string.Format(messages.GetMessage("dragon_damage_dealt"), "Smolderfang", 9),
            string.Format(messages.GetMessage("player_health_now"), "Hero", 16)), output);
        Assert.AreEqual(16, player.HealthPoints);
        CollectionAssert.AreEqual(new[] { 20, 20, 12 }, AttackRolls(die));
        Assert.IsTrue(die.IsExhausted);
    }

    // The Player's HealthPoints are clamped at 0, never shown negative.
    [TestMethod]
    public void Hit_DropsPlayerToZero_ClampedAtZero_DefeatedNarrative()
    {
        Messages messages = EnglishMessages();
        (Dragon dragon, ScriptedDie die) = DragonWithStrength(10, 5, 8, 12);
        Player player = TargetPlayer(agility: 7, healthPoints: 4);

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(Lines(
            Intro(dragon, player, messages),
            "<<< claws >>>",
            string.Format(messages.GetMessage("dragon_hit_player"), "Smolderfang", "Hero"),
            string.Format(messages.GetMessage("dragon_damage_dealt"), "Smolderfang", 12),
            string.Format(messages.GetMessage("player_health_now"), "Hero", 0),
            string.Format(messages.GetMessage("player_defeated_narrative"), "Hero")), output);
        Assert.AreEqual(0, player.HealthPoints);
        CollectionAssert.AreEqual(new[] { 20, 20, 12 }, AttackRolls(die));
        Assert.IsTrue(die.IsExhausted);
    }

    // Grimtusk: same rules through the shared Monster code, his club's 8 sides for the damage roll.
    [TestMethod]
    public void Hit_ByGrimtusk_RollsHisClubsEightSides()
    {
        Messages messages = EnglishMessages();
        ScriptedDie die = new ScriptedDie(ConstructorRolls.Concat(new[] { 3, 9, 8 }).ToArray());
        Troll troll = new Troll(die) { Strength = 6, Agility = 5, HealthPoints = 10 };
        Player player = TargetPlayer(agility: 7, healthPoints: 20);

        string output = troll.Attack(player, messages);

        Assert.AreEqual(Lines(
            Intro(troll, player, messages),
            "(@)======",
            string.Format(messages.GetMessage("dragon_hit_player"), "Grimtusk", "Hero"),
            string.Format(messages.GetMessage("dragon_damage_dealt"), "Grimtusk", 8),
            string.Format(messages.GetMessage("player_health_now"), "Hero", 12)), output);
        Assert.AreEqual(12, player.HealthPoints);
        CollectionAssert.AreEqual(new[] { 20, 20, 8 }, AttackRolls(die));
        Assert.IsTrue(die.IsExhausted);
    }
}
