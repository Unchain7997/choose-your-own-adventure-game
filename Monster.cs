// Any creature the player fights (see GLOSSARY.md). Holds the stats, attack rules and messages that
// every Monster shares; each subclass supplies its own name, stats, weapon and message keys.
public abstract class Monster
{
    protected readonly Die _die;

    public string Name { get; set; } = "";
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int HealthPoints { get; set; }

    public Weapon Weapon { get; set; }

    protected Monster(Die die, Weapon weapon)
    {
        _die = die;
        Weapon = weapon;
    }

    // The messages that make each Monster sound like itself. Kept protected: other classes ask the
    // Monster for finished text rather than reading its keys.
    protected abstract string StatsHeaderKey { get; }
    protected abstract string CombatHeaderKey { get; }
    protected abstract string DefeatedNarrativeKey { get; }
    protected abstract string PlayerAttackIntroKey { get; }
    protected abstract IReadOnlyList<string> DefenseTauntKeys { get; }
    protected abstract IReadOnlyList<string> DamageReplyKeys { get; }

    // Applies the adventure's Difficulty once, right after the Monster is created: Easy -2, Hard +2 on
    // Strength, Agility and HealthPoints, each kept at 1 or more; Hard has no maximum. Normal leaves the
    // stats exactly as they are. Weapon, attack rules and messages are never changed.
    public void ApplyDifficulty(Difficulty difficulty)
    {
        int adjustment = difficulty switch
        {
            Difficulty.Easy => -2,
            Difficulty.Normal => 0,
            Difficulty.Hard => 2,
            // No silent fallback: a new Difficulty must be given its adjustment here explicitly.
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "No adjustment for this Difficulty.")
        };

        if (adjustment == 0)
            return;

        Strength = Math.Max(1, Strength + adjustment);
        Agility = Math.Max(1, Agility + adjustment);
        HealthPoints = Math.Max(1, HealthPoints + adjustment);
    }

    public int RollAttack()
    {
        return _die.Roll(20);
    }

    public bool AttackHits(int attackRoll)
    {
        return AttackResolution.Hits(attackRoll, Strength);
    }

    public int RollDamage()
    {
        return _die.Roll(Weapon.MaxDamage);
    }

    public string Attack(Player player, Messages messages)
    {
        var output = new List<string>();

        output.Add(string.Format(
            messages.GetMessage("dragon_attack_intro"),
            Name,
            player.Name,
            messages.TranslateWeaponForDisplay(Weapon.Type)
        ));

        if (!string.IsNullOrWhiteSpace(Weapon.AsciiArt))
            output.Add(Weapon.AsciiArt);

        // The Attack rules (hit, block, damage rolls) are shared with the player's Attack.
        AttackResult result = AttackResolution.Resolve(_die, Strength, player.Agility, Weapon.MaxDamage);

        if (result.Outcome == AttackOutcome.Miss)
        {
            output.Add(string.Format(messages.GetMessage("dragon_missed_player"), Name, player.Name));
            return string.Join(Environment.NewLine, output);
        }

        output.Add(string.Format(messages.GetMessage("dragon_hit_player"), Name, player.Name));

        if (result.Outcome == AttackOutcome.Blocked)
        {
            output.Add(string.Format(messages.GetMessage("player_defended_dragon_attack"), player.Name));
            return string.Join(Environment.NewLine, output);
        }

        int damage = result.Damage;
        player.HealthPoints -= damage;

        if (player.HealthPoints < 0)
            player.HealthPoints = 0;

        output.Add(string.Format(messages.GetMessage("dragon_damage_dealt"), Name, damage));
        output.Add(string.Format(messages.GetMessage("player_health_now"), player.Name, player.HealthPoints));

        if (player.HealthPoints <= 0)
            output.Add(string.Format(messages.GetMessage("player_defeated_narrative"), player.Name));

        return string.Join(Environment.NewLine, output);
    }

    public string GetRandomDefenseTaunt(Messages messages)
    {
        int index = _die.Roll(DefenseTauntKeys.Count) - 1;
        return messages.GetMessage(DefenseTauntKeys[index]);
    }

    public string GetRandomDamageReply(Messages messages)
    {
        int index = _die.Roll(DamageReplyKeys.Count) - 1;
        return messages.GetMessage(DamageReplyKeys[index]);
    }

    public string GetCombatHeader(Messages messages)
    {
        return messages.GetMessage(CombatHeaderKey);
    }

    public string GetDefeatedNarrative(Messages messages)
    {
        return messages.GetMessage(DefeatedNarrativeKey);
    }

    // The line shown when the player attacks this Monster, e.g. "As a Fighter, you withdraw your long sword and attack the dragon."
    public string GetPlayerAttackIntro(Messages messages, string occupation, string weapon)
    {
        return string.Format(messages.GetMessage(PlayerAttackIntroKey), occupation, weapon);
    }

    public void DisplayStats(Messages messages)
    {
        Console.WriteLine(messages.GetMessage(StatsHeaderKey));
        Console.WriteLine(messages.GetMessage("stats_name") + Name);
        Console.WriteLine(messages.GetMessage("stats_strength") + Strength);
        Console.WriteLine(messages.GetMessage("stats_agility") + Agility);
        Console.WriteLine(messages.GetMessage("stats_health") + HealthPoints);
        Console.WriteLine(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(Weapon.Type));
        Console.WriteLine(messages.GetMessage("stats_damage") + Weapon.MaxDamage);
    }
}
