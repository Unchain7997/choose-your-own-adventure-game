// CYOATests/ScriptedDie.cs
// Test double for Die (attack-resolution spec). It returns a queued list of values in order, records
// the number of sides each roll asked for, and throws if the code rolls more often than scripted.
// Like FixedDie, it is dependency injection, not seeding: the game's own Die stays unseeded.
public class ScriptedDie : Die
{
    private readonly Queue<int> _values;
    private readonly List<int> _sidesRolled = new List<int>();

    public ScriptedDie(params int[] values)
    {
        _values = new Queue<int>(values);
    }

    // The number of sides of every roll made so far, in order.
    public IReadOnlyList<int> SidesRolled => _sidesRolled;

    // True once every scripted value has been used.
    public bool IsExhausted => _values.Count == 0;

    public override int Roll(int sides)
    {
        _sidesRolled.Add(sides);

        if (_values.Count == 0)
            throw new InvalidOperationException(
                $"Unscripted roll {_sidesRolled.Count} (d{sides}): the code rolled more often than the test scripted.");

        return _values.Dequeue();
    }
}
