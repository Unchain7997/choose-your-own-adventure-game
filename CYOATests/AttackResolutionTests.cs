// CYOATests/AttackResolutionTests.cs
// Attack-resolution spec, Seam 2: the shared Attack rules with plain values and no console.
// ScriptedDie fixes every roll and records its sides, so outcomes, roll counts and order are exact.
[TestClass]
public class AttackResolutionTests
{
    // AC1/AC3/AC4: a hit roll above Strength misses after one d20 roll, with no damage.
    [TestMethod]
    public void Resolve_HitRollAboveStrength_IsMiss_AfterOneRoll()
    {
        ScriptedDie die = new ScriptedDie(11);

        AttackResult result = AttackResolution.Resolve(die, attackerStrength: 10, defenderAgility: 7, weaponMaxDamage: 12);

        Assert.AreEqual(AttackOutcome.Miss, result.Outcome);
        Assert.AreEqual(0, result.Damage);
        CollectionAssert.AreEqual(new[] { 20 }, die.SidesRolled.ToArray());
        Assert.IsTrue(die.IsExhausted);
    }

    // AC1/AC2/AC3/AC4: a hit roll equal to Strength hits; a block roll equal to Agility blocks.
    [TestMethod]
    public void Resolve_EqualHitAndBlockRolls_IsBlocked_AfterTwoRolls()
    {
        ScriptedDie die = new ScriptedDie(10, 7);

        AttackResult result = AttackResolution.Resolve(die, attackerStrength: 10, defenderAgility: 7, weaponMaxDamage: 12);

        Assert.AreEqual(AttackOutcome.Blocked, result.Outcome);
        Assert.AreEqual(0, result.Damage);
        CollectionAssert.AreEqual(new[] { 20, 20 }, die.SidesRolled.ToArray());
        Assert.IsTrue(die.IsExhausted);
    }

    // AC1/AC3/AC4: an unblocked hit rolls damage on the weapon's maximum, in that order.
    [TestMethod]
    [DataRow(12, 9)]
    [DataRow(8, 8)]
    [DataRow(6, 1)]
    public void Resolve_UnblockedHit_IsHit_DamageEqualsDamageRoll_AfterThreeRolls(int weaponMaxDamage, int damageRoll)
    {
        ScriptedDie die = new ScriptedDie(5, 8, damageRoll);

        AttackResult result = AttackResolution.Resolve(die, attackerStrength: 10, defenderAgility: 7, weaponMaxDamage);

        Assert.AreEqual(AttackOutcome.Hit, result.Outcome);
        Assert.AreEqual(damageRoll, result.Damage);
        CollectionAssert.AreEqual(new[] { 20, 20, weaponMaxDamage }, die.SidesRolled.ToArray());
        Assert.IsTrue(die.IsExhausted);
    }

    // AC2: Strength 0 or less never hits, even on the lowest roll.
    [TestMethod]
    [DataRow(0)]
    [DataRow(-2)]
    public void Resolve_StrengthZeroOrLess_NeverHits(int strength)
    {
        AttackResult result = AttackResolution.Resolve(new FixedDie(1), strength, defenderAgility: 0, weaponMaxDamage: 12);

        Assert.AreEqual(AttackOutcome.Miss, result.Outcome);
        Assert.AreEqual(0, result.Damage);
    }

    // AC2: Agility 20 or more always blocks, even on the highest roll.
    [TestMethod]
    [DataRow(20)]
    [DataRow(22)]
    public void Resolve_AgilityTwentyOrMore_AlwaysBlocks(int agility)
    {
        AttackResult result = AttackResolution.Resolve(new FixedDie(20), attackerStrength: 20, agility, weaponMaxDamage: 12);

        Assert.AreEqual(AttackOutcome.Blocked, result.Outcome);
        Assert.AreEqual(0, result.Damage);
    }

    // R5: no input checks. A weapon maximum of 0 reaches the Die unchanged.
    [TestMethod]
    public void Resolve_PassesWeaponMaximumToTheDieUnchecked()
    {
        ScriptedDie die = new ScriptedDie(5, 8, 1);

        AttackResult result = AttackResolution.Resolve(die, attackerStrength: 10, defenderAgility: 7, weaponMaxDamage: 0);

        Assert.AreEqual(AttackOutcome.Hit, result.Outcome);
        Assert.AreEqual(1, result.Damage);
        CollectionAssert.AreEqual(new[] { 20, 20, 0 }, die.SidesRolled.ToArray());
    }

    // AC5: a roll hits exactly when it is at or below Strength.
    [TestMethod]
    [DataRow(10, 10, true)]
    [DataRow(9, 10, true)]
    [DataRow(11, 10, false)]
    [DataRow(1, 0, false)]
    [DataRow(20, 20, true)]
    [DataRow(20, 19, false)]
    public void Hits_IsTrueExactlyWhenRollIsAtOrBelowStrength(int roll, int strength, bool expected)
    {
        Assert.AreEqual(expected, AttackResolution.Hits(roll, strength));
    }
}
