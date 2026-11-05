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
    public void Language_HasAll69DictionaryKeys(string language)
    {
        using JsonDocument doc = LoadLanguageData();
        Assert.AreEqual(69, KeysOf(LoadSection(doc, language).GetProperty("dictionary")).Count);
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
            try
            {
                string.Format(entry.Value.GetString()!, "arg0", "arg1", "arg2");
            }
            catch (FormatException)
            {
                Assert.Fail($"{language} key '{entry.Name}' is not a valid format string.");
            }
        }
    }
}
