public class Player
{
    private readonly Die _die = new Die();

    public string Name { get; set; } = "";
    public string Race { get; set; } = "";
    public string Occupation { get; set; } = "";

    public int Strength { get; set; }
    public int Agility { get; set; }
    public int HealthPoints { get; set; }
    public int Gold { get; set; }

    public Weapon? Weapon { get; set; }

    public void CreateCharacter(Messages messages)
    {
        Race = PromptForRace(messages);
        Name = PromptForName(messages);
        Occupation = PromptForOccupation(messages);

        AssignWeaponByOccupation();

        int strengthModifier = RollStrength(messages);
        RollAgility(messages);
        RollHealthPoints(messages, strengthModifier);
        RollGold(messages);

        PromptForNextAction(messages);
    }

    private string PromptForRace(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("race_prompt"));
            string? input = Console.ReadLine()?.Trim();

            if (messages.IsValidRace(input))
                return messages.NormalizeRace(input!);

            Console.WriteLine(messages.GetMessage("race_invalid"));
        }
    }

    private string PromptForName(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("name_prompt"));
            string? input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
                return input;

            Console.WriteLine(messages.GetMessage("name_invalid"));
        }
    }

    private string PromptForOccupation(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("occupation_prompt"));
            string? input = Console.ReadLine()?.Trim();

            if (messages.IsValidOccupation(input))
                return messages.NormalizeOccupation(input!);

            Console.WriteLine(messages.GetMessage("occupation_invalid"));
        }
    }

    private void AssignWeaponByOccupation()
    {
        switch (Occupation.ToLower())
        {
            case "fighter":
                Weapon = new Weapon("long sword", 12, "-)=====>");
                break;
            case "magician":
                Weapon = new Weapon("lightning bolt spell", 12, "zap~~~~~~");
                break;
            case "thief":
                Weapon = new Weapon("dagger", 6, "-)==>");
                break;
            case "archer":
                Weapon = new Weapon("long bow", 8, "}    -->");
                break;
        }
    }

    private int RollStrength(Messages messages)
    {
        PromptRoll(messages.GetMessage("roll_strength"), messages);
        Strength = _die.Roll(20);

        int modifier = _die.Roll(4);

        if (Race == "Halfling")
        {
            Strength -= modifier;
            return -modifier;
        }

        Strength += modifier;
        return modifier;
    }

    private void RollAgility(Messages messages)
    {
        PromptRoll(messages.GetMessage("roll_agility"), messages);
        Agility = _die.Roll(20);

        if (Race == "Halfling" || Race == "Elf")
            Agility += _die.Roll(4);
    }

    private void RollHealthPoints(Messages messages, int strengthModifier)
    {
        PromptRoll(messages.GetMessage("roll_health"), messages);
        HealthPoints = _die.Roll(20) + strengthModifier;
    }

    private void RollGold(Messages messages)
    {
        PromptRoll(messages.GetMessage("roll_gold"), messages);
        Gold = _die.Roll(20);
    }

    private void PromptForNextAction(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("next_action"));
            string? input = Console.ReadLine()?.Trim();

            if (input == "1")
                DisplayStats(messages);
            else if (input == "2")
                return;
            else
                Console.WriteLine(messages.GetMessage("invalid"));
        }
    }

    public void DisplayStats(Messages messages)
    {
        Console.WriteLine(messages.GetMessage("stats_header"));
        Console.WriteLine(messages.GetMessage("stats_name") + Name);
        Console.WriteLine(messages.GetMessage("stats_race") + messages.TranslateRaceForDisplay(Race));
        Console.WriteLine(messages.GetMessage("stats_occupation") + messages.TranslateOccupationForDisplay(Occupation));
        Console.WriteLine(messages.GetMessage("stats_strength") + Strength);
        Console.WriteLine(messages.GetMessage("stats_agility") + Agility);
        Console.WriteLine(messages.GetMessage("stats_health") + HealthPoints);
        Console.WriteLine(messages.GetMessage("stats_gold") + Gold);

        if (Weapon != null)
        {
            Console.WriteLine(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(Weapon.Type));
            Console.WriteLine(messages.GetMessage("stats_damage") + Weapon.MaxDamage);
        }
    }

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
        if (Weapon == null)
            return 0;

        return _die.Roll(Weapon.MaxDamage);
    }

    public string Attack(Monster monster, Messages messages)
    {
        if (Weapon == null)
            return "You have no weapon to attack with.";

        var output = new List<string>();

        output.Add(monster.GetPlayerAttackIntro(
            messages,
            messages.TranslateOccupationForDisplay(Occupation),
            messages.TranslateWeaponForDisplay(Weapon.Type)
        ));

        // If this fails to compile, change AsciiArt to whatever exists in Weapon.cs
        output.Add(Weapon.AsciiArt);

        int attackRoll = RollAttack();

        // MISS
        if (!AttackHits(attackRoll))
        {
            output.Add(string.Format(messages.GetMessage("player_missed_dragon"), monster.Name));
            return string.Join(Environment.NewLine, output);
        }

        output.Add(string.Format(messages.GetMessage("player_hit_dragon"), monster.Name));

        int defenseRoll = _die.Roll(20);

        // DEFENSE
        if (defenseRoll <= monster.Agility)
        {
            output.Add(string.Format(messages.GetMessage("dragon_defended_player_attack"), monster.Name));
            output.Add(string.Format(
                monster.GetRandomDefenseTaunt(messages),
                messages.TranslateRaceForDisplay(Race)
            ));
            return string.Join(Environment.NewLine, output);
        }

        // DAMAGE
        int damage = RollDamage();
        monster.HealthPoints -= damage;

        output.Add(string.Format(messages.GetMessage("player_damage_dealt"), damage));
        output.Add(string.Format(messages.GetMessage("dragon_health_now"), monster.Name, monster.HealthPoints));

        // END
        if (monster.HealthPoints <= 0)
            output.Add(monster.GetDefeatedNarrative(messages));
        else
            output.Add(monster.GetRandomDamageReply(messages));

        return string.Join(Environment.NewLine, output);
    }

    private void PromptRoll(string prompt, Messages messages)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "roll")
                return;

            Console.WriteLine(messages.GetMessage("roll_invalid"));
        }
    }
}