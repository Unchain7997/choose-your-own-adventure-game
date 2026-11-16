// CYOATests/DifficultyTests.cs
// Difficulty spec, Seam 1: a Difficulty applied to a Monster, with plain values and no console.
// Stats are set through the Monsters' public setters, so no test depends on a dice roll.
[TestClass]
public class DifficultyTests
{
    private static Dragon DragonWithStats(int strength, int agility, int healthPoints)
    {
        return new Dragon(new Die()) { Strength = strength, Agility = agility, HealthPoints = healthPoints };
    }

    private static Troll TrollWithStats(int strength, int agility, int healthPoints)
    {
        return new Troll(new Die()) { Strength = strength, Agility = agility, HealthPoints = healthPoints };
    }

    private static void AssertStats(Monster monster, int strength, int agility, int healthPoints)
    {
        Assert.AreEqual(strength, monster.Strength, "Strength");
        Assert.AreEqual(agility, monster.Agility, "Agility");
        Assert.AreEqual(healthPoints, monster.HealthPoints, "HealthPoints");
    }

    // AC4: Easy -2, Normal 0, Hard +2 on Strength, Agility and HealthPoints.
    [TestMethod]
    [DataRow(Difficulty.Easy, 8)]
    [DataRow(Difficulty.Normal, 10)]
    [DataRow(Difficulty.Hard, 12)]
    public void ApplyDifficulty_OnDragon_AdjustsAllThreeStats(Difficulty difficulty, int expected)
    {
        Dragon dragon = DragonWithStats(10, 10, 10);

        dragon.ApplyDifficulty(difficulty);

        AssertStats(dragon, expected, expected, expected);
    }

    [TestMethod]
    [DataRow(Difficulty.Easy, 8)]
    [DataRow(Difficulty.Normal, 10)]
    [DataRow(Difficulty.Hard, 12)]
    public void ApplyDifficulty_OnTroll_AdjustsAllThreeStats(Difficulty difficulty, int expected)
    {
        Troll troll = TrollWithStats(10, 10, 10);

        troll.ApplyDifficulty(difficulty);

        AssertStats(troll, expected, expected, expected);
    }

    // Each stat moves by itself: different starting values stay different.
    [TestMethod]
    public void ApplyDifficulty_Hard_AdjustsEachStatFromItsOwnValue()
    {
        Troll troll = TrollWithStats(5, 7, 9);

        troll.ApplyDifficulty(Difficulty.Hard);

        AssertStats(troll, 7, 9, 11);
    }

    // R12: Normal leaves the stats untouched, even values below 1 that only test fixtures use.
    [TestMethod]
    public void ApplyDifficulty_Normal_LeavesStatsBelowOneUntouched()
    {
        Dragon dragon = DragonWithStats(0, 0, 0);

        dragon.ApplyDifficulty(Difficulty.Normal);

        AssertStats(dragon, 0, 0, 0);
    }

    // R9: a minimum of 1 after the adjustment.
    [TestMethod]
    public void ApplyDifficulty_Easy_NeverTakesAStatBelowOne()
    {
        Troll troll = TrollWithStats(2, 1, 1);

        troll.ApplyDifficulty(Difficulty.Easy);

        AssertStats(troll, 1, 1, 1);
    }

    [TestMethod]
    public void ApplyDifficulty_Easy_StatOfThreeBecomesOne()
    {
        Dragon dragon = DragonWithStats(3, 3, 3);

        dragon.ApplyDifficulty(Difficulty.Easy);

        AssertStats(dragon, 1, 1, 1);
    }

    // R9: no maximum on Hard (spec Known Limitation L2).
    [TestMethod]
    public void ApplyDifficulty_Hard_HasNoMaximum()
    {
        Dragon dragon = DragonWithStats(20, 20, 20);

        dragon.ApplyDifficulty(Difficulty.Hard);

        AssertStats(dragon, 22, 22, 22);
    }

    // An unknown Difficulty is rejected, never silently treated as Normal.
    [TestMethod]
    public void ApplyDifficulty_UnknownValue_IsRejected()
    {
        Dragon dragon = DragonWithStats(10, 10, 10);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => dragon.ApplyDifficulty((Difficulty)99));
        AssertStats(dragon, 10, 10, 10);
    }

    // AC5 (weapon part): Difficulty never touches the Monster's weapon.
    [TestMethod]
    [DataRow(Difficulty.Easy)]
    [DataRow(Difficulty.Normal)]
    [DataRow(Difficulty.Hard)]
    public void ApplyDifficulty_LeavesWeaponUnchanged(Difficulty difficulty)
    {
        Dragon dragon = DragonWithStats(10, 10, 10);
        Troll troll = TrollWithStats(10, 10, 10);

        dragon.ApplyDifficulty(difficulty);
        troll.ApplyDifficulty(difficulty);

        Assert.AreEqual("claws", dragon.Weapon.Type);
        Assert.AreEqual(12, dragon.Weapon.MaxDamage);
        Assert.AreEqual("club", troll.Weapon.Type);
        Assert.AreEqual(8, troll.Weapon.MaxDamage);
    }

    // AC6 (adjusted numbers): the combat stats display shows the Monster's adjusted stats.
    // Ticket 02 adds the "no stats_difficulty label" assertion here, once that key exists.
    [TestMethod]
    public void CombatStatsDisplay_AfterHard_ShowsAdjustedMonsterStats()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();

        Player player = new Player
        {
            Name = "TestHero",
            Race = "Human",
            Occupation = "Fighter",
            Strength = 15,
            Agility = 12,
            HealthPoints = 25,
            Weapon = new Weapon("long sword", 12, "-)=====>")
        };
        Troll troll = TrollWithStats(10, 10, 10);
        troll.ApplyDifficulty(Difficulty.Hard);

        string display = new Combat(player, troll, messages).GetCombatStatsDisplay();

        // Read the Monster's own section, so the player's stats can't satisfy the assertions.
        List<string> lines = display.Split(Environment.NewLine).ToList();
        int header = lines.IndexOf(messages.GetMessage("combat_troll_header"));
        Assert.IsTrue(header >= 0, "Troll combat header not found");

        CollectionAssert.AreEqual(
            new[]
            {
                messages.GetMessage("stats_name") + "Grimtusk",
                messages.GetMessage("stats_strength") + 12,
                messages.GetMessage("stats_agility") + 12,
                messages.GetMessage("stats_health") + 12
            },
            lines.Skip(header + 1).Take(4).ToArray());
    }
}
