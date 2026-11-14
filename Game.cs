public class Game
{
    public void StartGame()
    {
        // Title screen
        Console.WriteLine("=====================================");
        Console.WriteLine("Welcome to Choose Your Adventure Game");
        Console.WriteLine("=====================================\n");

        // Instantiate Die first (per sequence diagram)
        Die die = new Die();

        Dragon dragon = new Dragon(die);

        // Language selection shown before Messages dictionary is loaded
        Console.WriteLine("Select language / Selecciona idioma / Choisissez la langue / Seleziona la lingua:");
        Console.WriteLine("1. English  2. Espanol  3. Francais  4. Italiano");
        Console.Write("Enter choice: ");
        string selectedLanguage = GetLanguageFromChoice(Console.ReadLine());

        // Initialize Messages with selected language (per sequence diagram)

        Messages messages = new Messages();
        messages.SetCurrentLanguage(selectedLanguage);
        messages.ReadDictionary();
        // Display welcome message
        Console.WriteLine();
        Console.WriteLine(messages.GetMessage("welcome"));
        Console.WriteLine();

        // Main menu loop
        bool running = true;
        while (running)
        {
            Console.WriteLine(messages.GetMessage("menu"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? choice = Console.ReadLine()?.Trim();

            if (choice == "1")
            {
                Player player = new Player();
                player.CreateCharacter(messages);
                Merchant merchant = new Merchant();

                bool inAdventureMenu = true;
                while (inAdventureMenu)
                {
                    string selectedPath = PromptForPath(messages);

                    if (selectedPath == "exit")
                    {
                        Console.WriteLine(messages.GetMessage("adventure_ends"));
                        running = false;
                        inAdventureMenu = false;
                    }
                    else if (selectedPath == "south")
                    {
                        HandleMerchantEncounter(player, merchant, messages);
                    }
                    else if (selectedPath == "north")
                    {
                        HandleDragonEncounter(player, dragon, messages);

                        running = false;
                        inAdventureMenu = false;
                    }
                }
            }
            else if (choice == "2")
            {
                Console.WriteLine(messages.GetMessage("goodbye"));
                running = false;
            }
            else
            {
                Console.WriteLine(messages.GetMessage("invalid"));
            }
        }
    }

    public void EndGame(bool playerWon, bool playerRetreated, Messages messages)
    {
        if (playerRetreated)
            Console.WriteLine(messages.GetMessage("retreat"));
        else if (playerWon)
            Console.WriteLine(messages.GetMessage("victory"));
        else
            Console.WriteLine(messages.GetMessage("defeat"));
    }

    public string GetLanguageFromChoice(string? input)
    {
        return input?.Trim() switch
        {
            "2" => "Spanish",
            "3" => "French",
            "4" => "Italian",
            _ => "English"
        };
    }

    public bool IsValidMenuChoice(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string trimmed = input.Trim();
        return trimmed == "1" || trimmed == "2";
    }

    public bool IsValidPathChoice(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string trimmed = input.Trim().ToLower();
        return trimmed == "n" || trimmed == "north" ||
               trimmed == "s" || trimmed == "south" ||
               trimmed == "e" || trimmed == "exit";
    }

    public bool IsValidCombatChoice(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string trimmed = input.Trim().ToLower();
        return trimmed == "a" || trimmed == "attack" ||
               trimmed == "r" || trimmed == "retreat";
    }

    private string PromptForPath(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("path_prompt_full"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "north" || input == "n")
                return "north";

            if (input == "south" || input == "s")
                return "south";

            if (input == "exit" || input == "e")
                return "exit";

            Console.WriteLine(messages.GetMessage("path_invalid"));
        }
    }

    public void HandleMerchantEncounter(Player player, Merchant merchant, Messages messages)
    {
        Console.WriteLine(messages.GetMessage("south_path_narrative"));
        Console.WriteLine(string.Format(messages.GetMessage("merchant_greeting"), merchant.Name));
        Console.WriteLine(string.Format(messages.GetMessage("merchant_gold_display"), player.Gold));

        while (true)
        {
            DisplayMerchantMenu(merchant, messages);
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim();

            // End of input (null) also leaves, so the loop can never spin forever.
            if (input == merchant.LeaveOption.ToString() || input == null)
                break;

            int itemNumber = ItemNumberFromInput(merchant, input);
            if (itemNumber == 0)
            {
                Console.WriteLine(messages.GetMessage("invalid"));
                continue;
            }

            string itemName = messages.GetMessage("item_" + merchant.GetItemKey(itemNumber));
            PurchaseResult result = merchant.Purchase(player, itemNumber);
            switch (result)
            {
                case PurchaseResult.Purchased:
                    PlayerStat stat = merchant.GetAffectedStat(itemNumber);
                    Console.WriteLine(string.Format(messages.GetMessage("merchant_purchased"),
                        itemName,
                        messages.GetMessage(StatNameKey(stat)),
                        Merchant.GetStatValue(player, stat),
                        player.Gold));
                    break;
                case PurchaseResult.AlreadySold:
                    Console.WriteLine(string.Format(messages.GetMessage("merchant_already_sold"), itemName));
                    break;
                case PurchaseResult.InsufficientGold:
                    Console.WriteLine(string.Format(messages.GetMessage("merchant_not_enough_gold"),
                        merchant.GetPrice(itemNumber),
                        player.Gold));
                    break;
                default:
                    // No silent fallback: a new PurchaseResult must be handled here explicitly.
                    throw new InvalidOperationException($"Unhandled purchase result: {result}.");
            }
        }

        Console.WriteLine(string.Format(messages.GetMessage("merchant_farewell"), merchant.Name));
    }

    // Translation key for a stat's name in purchase confirmations. No fallback: GetMessage would
    // otherwise print "[key]" for an unknown stat, so an unsupported stat fails explicitly.
    private static string StatNameKey(PlayerStat stat) => stat switch
    {
        PlayerStat.HealthPoints => "stat_name_health",
        PlayerStat.Strength => "stat_name_strength",
        PlayerStat.Agility => "stat_name_agility",
        _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, "No stat name message for this stat."),
    };

    // Returns the Item's menu number when the input exactly matches one (1 to ItemCount); 0 means "not an Item".
    private static int ItemNumberFromInput(Merchant merchant, string input)
    {
        for (int number = 1; number <= merchant.ItemCount; number++)
        {
            if (input == number.ToString())
                return number;
        }

        return 0;
    }

    // Menu numbers are generated here; the translated texts never carry them.
    private void DisplayMerchantMenu(Merchant merchant, Messages messages)
    {
        for (int number = 1; number <= merchant.ItemCount; number++)
        {
            string itemKey = merchant.GetItemKey(number);
            string soldMark = merchant.IsSold(number) ? " " + messages.GetMessage("merchant_sold_mark") : "";
            Console.WriteLine($"{number}. " + string.Format(
                messages.GetMessage("merchant_menu_line"),
                messages.GetMessage("item_" + itemKey),
                string.Format(messages.GetMessage("effect_" + itemKey), merchant.GetBonus(number)),
                merchant.GetPrice(number)) + soldMark);
        }

        Console.WriteLine($"{merchant.LeaveOption}. " + messages.GetMessage("merchant_leave"));
    }

    private void HandleDragonEncounter(Player player, Dragon dragon, Messages messages)
    {
        Console.WriteLine(messages.GetMessage("north_path_narrative"));

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(string.Format(messages.GetMessage("dragon_intro"), dragon.Name));
        Console.ResetColor();

        Console.WriteLine(messages.GetMessage("dragon_stats_intro"));
        dragon.DisplayStats(messages);
        while (true)
        {
            Console.WriteLine(messages.GetMessage("dragon_encounter_prompt"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "a" || input == "attack")
            {
                Combat combat = new Combat(player, dragon, messages);
                bool playerWon = combat.StartCombat();

                if (combat.PlayerRetreated)
                {
                    return; //combat already handled retreat message
                }
                else
                {
                   EndGame(playerWon, false, messages); 
                }
                
                return;
            }

        if (input == "r" || input == "retreat")
        {
            Console.WriteLine(messages.GetMessage("retreat"));
            return;
        }

        Console.WriteLine(messages.GetMessage("invalid"));
    }
}
}
