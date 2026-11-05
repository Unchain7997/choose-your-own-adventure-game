using System.Text.Json;

public class Messages
{
    public string CurrentLanguage { get; set; }
    private Dictionary<string, string> _dictionary;
    private Dictionary<string, string> _raceMap;
    private Dictionary<string, string> _occupationMap;
    private Dictionary<string, string> _displayRaceMap;
    private Dictionary<string, string> _displayOccupationMap;
    private Dictionary<string, string> _displayWeaponMap;

    public Messages()
    {
        CurrentLanguage = "English";
        _dictionary = new Dictionary<string, string>();
        _raceMap = new Dictionary<string, string>();
        _occupationMap = new Dictionary<string, string>();
        _displayRaceMap = new Dictionary<string, string>();
        _displayOccupationMap = new Dictionary<string, string>();
        _displayWeaponMap = new Dictionary<string, string>();
    }

    public void SetCurrentLanguage(string language)
    {
        CurrentLanguage = language;
    }

    public void ReadDictionary()
    {
        string jsonPath = Path.Combine(AppContext.BaseDirectory, "language_data.json");
        string jsonText = File.ReadAllText(jsonPath);
        using JsonDocument doc = JsonDocument.Parse(jsonText);

        JsonElement langSection = doc.RootElement.GetProperty(CurrentLanguage);

        _dictionary = new Dictionary<string, string>();
        foreach (JsonProperty entry in langSection.GetProperty("dictionary").EnumerateObject())
            _dictionary[entry.Name] = entry.Value.GetString() ?? string.Empty;

        _raceMap = new Dictionary<string, string>();
        foreach (JsonProperty entry in langSection.GetProperty("raceMap").EnumerateObject())
            _raceMap[entry.Name] = entry.Value.GetString() ?? string.Empty;

        _occupationMap = new Dictionary<string, string>();
        foreach (JsonProperty entry in langSection.GetProperty("occupationMap").EnumerateObject())
            _occupationMap[entry.Name] = entry.Value.GetString() ?? string.Empty;

        _displayRaceMap = new Dictionary<string, string>();
        foreach (JsonProperty entry in langSection.GetProperty("displayRaceMap").EnumerateObject())
            _displayRaceMap[entry.Name] = entry.Value.GetString() ?? string.Empty;

        _displayOccupationMap = new Dictionary<string, string>();
        foreach (JsonProperty entry in langSection.GetProperty("displayOccupationMap").EnumerateObject())
            _displayOccupationMap[entry.Name] = entry.Value.GetString() ?? string.Empty;

        _displayWeaponMap = new Dictionary<string, string>();
        foreach (JsonProperty entry in langSection.GetProperty("displayWeaponMap").EnumerateObject())
            _displayWeaponMap[entry.Name] = entry.Value.GetString() ?? string.Empty;
    }

    public string GetMessage(string key)
    {
        return _dictionary.ContainsKey(key) ? _dictionary[key] : $"[{key}]";
    }

    public bool IsValidRace(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return _raceMap.ContainsKey(input.Trim().ToLower());
    }

    public string NormalizeRace(string input)
    {
        return _raceMap.TryGetValue(input.Trim().ToLower(), out string? canonical) ? canonical : input;
    }

    public bool IsValidOccupation(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return _occupationMap.ContainsKey(input.Trim().ToLower());
    }

    public string NormalizeOccupation(string input)
    {
        return _occupationMap.TryGetValue(input.Trim().ToLower(), out string? canonical) ? canonical : input;
    }

    public string TranslateRaceForDisplay(string race)
    {
        return _displayRaceMap.TryGetValue(race, out string? translated) ? translated : race;
    }

    public string TranslateOccupationForDisplay(string occupation)
    {
        return _displayOccupationMap.TryGetValue(occupation, out string? translated) ? translated : occupation;
    }

    public string TranslateWeaponForDisplay(string weaponType)
    {
        return _displayWeaponMap.TryGetValue(weaponType, out string? translated) ? translated : weaponType;
    }


}
