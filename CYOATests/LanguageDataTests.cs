// CYOATests/LanguageDataTests.cs
// Cross-language checks on language_data.json: every language must match English
// in dictionary keys, map structure, and formatting placeholders.
using System.Text.Json;
using System.Text.RegularExpressions;

[TestClass]
public class LanguageDataTests
{
    private static readonly string[] RequiredMaps =
    {
        "dictionary", "raceMap", "occupationMap",
        "displayRaceMap", "displayOccupationMap", "displayWeaponMap"
    };

    private static JsonElement LoadSection(JsonDocument doc, string language)
    {
        Assert.IsTrue(doc.RootElement.TryGetProperty(language, out JsonElement section),
            $"Language section '{language}' is missing from language_data.json.");
        return section;
    }

    private static JsonDocument LoadLanguageData()
    {
        string jsonPath = Path.Combine(AppContext.BaseDirectory, "language_data.json");
        return JsonDocument.Parse(File.ReadAllText(jsonPath));
    }

    private static HashSet<string> KeysOf(JsonElement map) =>
        map.EnumerateObject().Select(p => p.Name).ToHashSet();

    private static string Placeholders(string text) =>
        string.Join(",", Regex.Matches(text, @"\{\d+\}").Select(m => m.Value).OrderBy(v => v));

    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_HasSameDictionaryKeysAsEnglish(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        HashSet<string> english = KeysOf(LoadSection(doc, "English").GetProperty("dictionary"));
        HashSet<string> other = KeysOf(LoadSection(doc, language).GetProperty("dictionary"));

        CollectionAssert.AreEquivalent(english.ToList(), other.ToList(),
            $"{language} dictionary keys differ from English. " +
            $"Missing: [{string.Join(", ", english.Except(other))}] " +
            $"Extra: [{string.Join(", ", other.Except(english))}]");
    }

    [TestMethod]
    [DataRow("Italian")]
    public void Language_HasAll97DictionaryKeys(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        Assert.AreEqual(97, KeysOf(LoadSection(doc, language).GetProperty("dictionary")).Count);
    }

    // Troll Encounter keys added by Ticket 02. troll_fight_not_ready is temporary: Ticket 03 removes it.
    private static readonly string[] TrollEncounterKeys =
    {
        "west_path_narrative", "troll_intro", "troll_stats_intro", "troll_stats_header", "troll_retreat",
        "troll_fight_not_ready"
    };

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_HasTrollEncounterKeys(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        HashSet<string> keys = KeysOf(LoadSection(doc, language).GetProperty("dictionary"));

        foreach (string key in TrollEncounterKeys)
            Assert.IsTrue(keys.Contains(key), $"{language} is missing dictionary key '{key}'.");
    }

    // AC23: Grimtusk's name always comes from {0}, like Smolderfang's and Old Brannoc's.
    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_TrollName_ComesFromPlaceholder_NeverTypedIntoText(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement dictionary = LoadSection(doc, language).GetProperty("dictionary");

        foreach (JsonProperty entry in dictionary.EnumerateObject())
            StringAssert.DoesNotMatch(entry.Value.GetString()!, new Regex("Grimtusk"),
                $"{language} key '{entry.Name}' types Grimtusk's name instead of using {{0}}.");

        foreach (string key in new[] { "troll_intro", "troll_fight_not_ready" })
            Assert.AreEqual("{0}", Placeholders(dictionary.GetProperty(key).GetString()!),
                $"{language} '{key}' must take Grimtusk's name as {{0}}.");
    }

    // AC22: Grimtusk's club is translated through the existing weapon display map.
    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_DisplayWeaponMap_HasTheClub(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement weapons = LoadSection(doc, language).GetProperty("displayWeaponMap");

        Assert.IsTrue(weapons.TryGetProperty("club", out JsonElement club), $"{language} is missing 'club'.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(club.GetString()), $"{language} 'club' is empty.");
    }

    // AC2: the Crossroads offers West, and the invalid-path message lists every letter.
    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_CrossroadsText_OffersWest(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement dictionary = LoadSection(doc, language).GetProperty("dictionary");
        string prompt = dictionary.GetProperty("path_prompt_full").GetString()!;
        string invalid = dictionary.GetProperty("path_invalid").GetString()!;

        StringAssert.Contains(prompt, "'w'", $"{language} path_prompt_full doesn't offer 'w'.");
        foreach (string letter in new[] { "'n'", "'s'", "'w'", "'e'" })
            StringAssert.Contains(invalid, letter, $"{language} path_invalid doesn't list {letter}.");
    }

    private static readonly string[] MerchantKeys =
    {
        "merchant_greeting", "merchant_gold_display",
        "item_healing_potion", "item_strength_tonic", "item_agility_elixir",
        "effect_healing_potion", "effect_strength_tonic", "effect_agility_elixir",
        "merchant_menu_line", "merchant_leave", "merchant_farewell",
        "stat_name_health", "stat_name_strength", "stat_name_agility",
        "merchant_purchased", "merchant_sold_mark", "merchant_already_sold",
        "merchant_not_enough_gold", "merchant_nothing_affordable", "merchant_moved_on"
    };

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_HasMerchantEncounterKeys(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        HashSet<string> keys = KeysOf(LoadSection(doc, language).GetProperty("dictionary"));

        foreach (string key in MerchantKeys)
            Assert.IsTrue(keys.Contains(key), $"{language} is missing dictionary key '{key}'.");
    }

    // Menu numbers are generated in code, so no menu text may carry its own number.
    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_MerchantMenuText_HasNoTypedMenuNumber(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement dictionary = LoadSection(doc, language).GetProperty("dictionary");

        foreach (string key in new[] { "merchant_leave", "merchant_menu_line" })
            StringAssert.DoesNotMatch(dictionary.GetProperty(key).GetString()!, new Regex(@"^\s*\d"),
                $"{language} '{key}' starts with a typed menu number.");
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_SouthPathNarrative_IsRewrittenWithoutNamingTheMerchant(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement dictionary = LoadSection(doc, language).GetProperty("dictionary");

        Assert.IsTrue(dictionary.TryGetProperty("south_path_narrative", out JsonElement narrative),
            $"{language} is missing 'south_path_narrative'.");
        string text = narrative.GetString()!;

        Assert.AreEqual("", Placeholders(text), $"{language} south_path_narrative must have no placeholders.");
        StringAssert.DoesNotMatch(text, new Regex("Brannoc"), $"{language} south_path_narrative names the Merchant.");
        StringAssert.DoesNotMatch(text, new Regex("nothing adventurous|no pasa nada aventurero|rien d'aventureux|non accade nulla"),
            $"{language} south_path_narrative still has the old 'nothing happens' text.");
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_MerchantName_ComesFromPlaceholder_NeverTypedIntoText(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement dictionary = LoadSection(doc, language).GetProperty("dictionary");

        foreach (JsonProperty entry in dictionary.EnumerateObject())
            StringAssert.DoesNotMatch(entry.Value.GetString()!, new Regex("Brannoc"),
                $"{language} key '{entry.Name}' types the Merchant's name instead of using {{0}}.");

        foreach (string key in new[] { "merchant_greeting", "merchant_farewell" })
            Assert.AreEqual("{0}", Placeholders(dictionary.GetProperty(key).GetString()!),
                $"{language} '{key}' must take the Merchant's name as {{0}}.");
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_HasAllSixRequiredMaps(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement section = LoadSection(doc, language);

        foreach (string map in RequiredMaps)
            Assert.IsTrue(section.TryGetProperty(map, out _), $"{language} is missing map '{map}'.");
    }

    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_InputMapsCoverSameCanonicalValuesAsEnglish(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement english = LoadSection(doc, "English");
        JsonElement other = LoadSection(doc, language);

        foreach (string map in new[] { "raceMap", "occupationMap" })
        {
            var englishValues = english.GetProperty(map).EnumerateObject().Select(p => p.Value.GetString()).ToHashSet();
            var otherValues = other.GetProperty(map).EnumerateObject().Select(p => p.Value.GetString()).ToHashSet();
            Assert.IsTrue(englishValues.SetEquals(otherValues),
                $"{language} {map} does not map to the same canonical values as English.");
        }
    }

    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_DisplayMapsHaveSameKeysAsEnglish(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement english = LoadSection(doc, "English");
        JsonElement other = LoadSection(doc, language);

        foreach (string map in new[] { "displayRaceMap", "displayOccupationMap", "displayWeaponMap" })
            CollectionAssert.AreEquivalent(KeysOf(english.GetProperty(map)).ToList(),
                KeysOf(other.GetProperty(map)).ToList(), $"{language} {map} keys differ from English.");
    }

    [TestMethod]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_PlaceholdersMatchEnglish(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        JsonElement english = LoadSection(doc, "English").GetProperty("dictionary");
        JsonElement other = LoadSection(doc, language).GetProperty("dictionary");

        foreach (JsonProperty entry in english.EnumerateObject())
        {
            if (!other.TryGetProperty(entry.Name, out JsonElement translated))
                continue; // missing keys are reported by Language_HasSameDictionaryKeysAsEnglish

            Assert.AreEqual(Placeholders(entry.Value.GetString()!), Placeholders(translated.GetString()!),
                $"{language} key '{entry.Name}' has different placeholders than English.");
        }
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_AllMessagesAreValidFormatStrings(string language)
    {
        using JsonDocument doc = LoadLanguageData();

        foreach (JsonProperty entry in LoadSection(doc, language).GetProperty("dictionary").EnumerateObject())
        {
            string text = entry.Value.GetString()!;

            // One argument per placeholder index the message uses, so messages with any number of
            // placeholders (e.g. merchant_purchased's {0}-{3}) are validated in full.
            int argumentCount = Regex.Matches(text, @"\{(\d+)")
                .Select(m => int.Parse(m.Groups[1].Value) + 1)
                .DefaultIfEmpty(0)
                .Max();
            object[] arguments = Enumerable.Range(0, argumentCount).Select(i => (object)$"<arg{i}>").ToArray();

            string formatted = "";
            try
            {
                formatted = string.Format(text, arguments);
            }
            catch (FormatException)
            {
                Assert.Fail($"{language} key '{entry.Name}' is not a valid format string.");
            }

            foreach (string placeholder in Regex.Matches(text, @"\{(\d+)\}").Select(m => m.Groups[1].Value).Distinct())
                StringAssert.Contains(formatted, $"<arg{placeholder}>",
                    $"{language} key '{entry.Name}' did not substitute {{{placeholder}}}.");
        }
    }

    // Item name, stat name, new stat value, Gold left.
    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void Language_PurchaseConfirmation_HasFourPlaceholders(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        string text = LoadSection(doc, language).GetProperty("dictionary").GetProperty("merchant_purchased").GetString()!;

        Assert.AreEqual("{0},{1},{2},{3}", Placeholders(text));
    }
}
