// How one Attack ended (see GLOSSARY.md: Attack).
public enum AttackOutcome { Miss, Blocked, Hit }

// The result of one Attack: its outcome, and the damage rolled on a Hit (0 on a Miss or a Block).
public readonly record struct AttackResult(AttackOutcome Outcome, int Damage);

// The rules of an Attack, shared by the player's Attack and every Monster's. Rolls with the attacker's
// own Die, in a fixed order, stopping as soon as the Attack misses or is blocked:
//   1. a d20 hit roll, which hits at or below the attacker's Strength;
//   2. a d20 block roll, which blocks at or below the defender's Agility;
//   3. a damage roll with the attacker's weapon maximum as its sides.
// It changes no HealthPoints and builds no messages; callers do both. Inputs are not checked, so
// every value reaches the Die and the comparisons exactly as the attack code always passed them.
public static class AttackResolution
{
    public static AttackResult Resolve(Die die, int attackerStrength, int defenderAgility, int weaponMaxDamage)
    {
        if (!Hits(die.Roll(20), attackerStrength))
            return new AttackResult(AttackOutcome.Miss, 0);

        if (Blocks(die.Roll(20), defenderAgility))
            return new AttackResult(AttackOutcome.Blocked, 0);

        return new AttackResult(AttackOutcome.Hit, die.Roll(weaponMaxDamage));
    }

    // The hit rule: a roll at or below the attacker's Strength hits.
    public static bool Hits(int roll, int strength)
    {
        return roll <= strength;
    }

    // The block rule: a roll at or below the defender's Agility blocks.
    private static bool Blocks(int roll, int agility)
    {
        return roll <= agility;
    }
}
