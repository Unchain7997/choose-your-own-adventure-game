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

    public int RollAttack()
    {
        return _die.Roll(20);
    }

    public bool AttackHits(int attackRoll)
    {
        return attackRoll <= Strength;
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

        int attackRoll = RollAttack();

        if (!AttackHits(attackRoll))
        {
            output.Add(string.Format(messages.GetMessage("dragon_missed_player"), Name, player.Name));
            return string.Join(Environment.NewLine, output);
        }

        output.Add(string.Format(messages.GetMessage("dragon_hit_player"), Name, player.Name));

        int defenseRoll = _die.Roll(20);

        if (defenseRoll <= player.Agility)
        {
            output.Add(string.Format(messages.GetMessage("player_defended_dragon_attack"), player.Name));
            return string.Join(Environment.NewLine, output);
        }

        int damage = RollDamage();
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
