// CYOATests/FixedDie.cs
// Test double for Die (Difficulty spec, Seam 5). It always rolls one chosen value, capped at the
// number of sides, and reaches the game through the Game(Die) constructor. This is dependency
// injection, not seeding: the game's own Die still uses unseeded randomness.
public class FixedDie : Die
{
    private readonly int _value;

    public FixedDie(int value)
    {
        _value = value;
    }

    public override int Roll(int sides)
    {
        return Math.Min(_value, sides);
    }
}
