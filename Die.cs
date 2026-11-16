using System;

public class Die
{
    private readonly Random _random = new Random();

    // Virtual so tests can pass a fixed-value test double through Game(Die); behaviour is unchanged.
    public virtual int Roll(int sides)
    {
        return _random.Next(1, sides + 1);
    }
}