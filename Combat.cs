public class Combat
{
    public Player player { get; set; }
    public Monster monster { get; set; }
    public Messages messages { get; set; }

    public bool PlayerRetreated { get; private set; }

    public Combat(Player player, Monster monster, Messages messages)
    {
        this.player = player;
        this.monster = monster;
        this.messages = messages;
        PlayerRetreated = false;
    }

    public bool StartCombat()
    {
        Console.WriteLine(GetCombatStatsDisplay());

        while (true)
        {
            PlayerAttacksDragonSequence();

            if (monster.HealthPoints <= 0)
                return true;

            DragonAttacksPlayerSequence();

            if (player.HealthPoints <= 0)
                return false;

            Console.WriteLine(messages.GetMessage("attack_prompt"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "r" || input == "retreat")
            {
                PlayerRetreated = true;
                Console.WriteLine(messages.GetMessage("retreat_combat"));
                return false;
            }

            if (input == "a" || input == "attack")
                continue;

            Console.WriteLine(messages.GetMessage("invalid"));
        }
    }

    public string GetCombatStatsDisplay()
    {
        var output = new List<string>();

        output.Add(messages.GetMessage("combat_stats_header"));
        output.Add(messages.GetMessage("combat_player_header"));
        output.Add(messages.GetMessage("stats_name") + player.Name);
        output.Add(messages.GetMessage("stats_strength") + player.Strength);
        output.Add(messages.GetMessage("stats_agility") + player.Agility);
        output.Add(messages.GetMessage("stats_health") + player.HealthPoints);

        if (player.Weapon != null)
        {
            output.Add(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(player.Weapon.Type));
            output.Add(messages.GetMessage("stats_damage") + player.Weapon.MaxDamage);
        }

        output.Add("");
        output.Add(monster.GetCombatHeader(messages));
        output.Add(messages.GetMessage("stats_name") + monster.Name);
        output.Add(messages.GetMessage("stats_strength") + monster.Strength);
        output.Add(messages.GetMessage("stats_agility") + monster.Agility);
        output.Add(messages.GetMessage("stats_health") + monster.HealthPoints);
        output.Add(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(monster.Weapon.Type));
        output.Add(messages.GetMessage("stats_damage") + monster.Weapon.MaxDamage);

        return string.Join(Environment.NewLine, output);
    }

    public void PlayerAttacksDragonSequence()
    {
        Console.WriteLine(player.Attack(monster, messages));
    }

    public void DragonAttacksPlayerSequence()
    {
        Console.WriteLine(monster.Attack(player, messages));
    }
}