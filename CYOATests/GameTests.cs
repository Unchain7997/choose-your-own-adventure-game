// CYOATests/GameTests.cs
[TestClass]
public class GameTests
{
    private Messages GetMessages(string language)
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerWinsInEnglish()
    {
        Game game = new Game();
        game.EndGame(true, false, GetMessages("English"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerLosesInEnglish()
    {
        Game game = new Game();
        game.EndGame(false, false, GetMessages("English"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerWinsInSpanish()
    {
        Game game = new Game();
        game.EndGame(true, false, GetMessages("Spanish"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerWinsInFrench()
    {
        Game game = new Game();
        game.EndGame(true, false, GetMessages("French"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerLosesInSpanish()
    {
        Game game = new Game();
        game.EndGame(false, false, GetMessages("Spanish"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerLosesInFrench()
    {
        Game game = new Game();
        game.EndGame(false, false, GetMessages("French"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerRetreatsInEnglish()
    {
        Game game = new Game();
        game.EndGame(false, true, GetMessages("English"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerRetreatsInSpanish()
    {
        Game game = new Game();
        game.EndGame(false, true, GetMessages("Spanish"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerRetreatsInFrench()
    {
        Game game = new Game();
        game.EndGame(false, true, GetMessages("French"));
    }

    // --- IsValidMenuChoice ---

    [TestMethod]
    public void IsValidMenuChoice_ReturnsTrue_ForOne()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidMenuChoice("1"));
    }

    [TestMethod]
    public void IsValidMenuChoice_ReturnsTrue_ForTwo()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidMenuChoice("2"));
    }

    [TestMethod]
    public void IsValidMenuChoice_ReturnsFalse_ForThree()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidMenuChoice("3"));
    }

    [TestMethod]
    public void IsValidMenuChoice_ReturnsFalse_ForNull()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidMenuChoice(null));
    }

    [TestMethod]
    public void IsValidMenuChoice_ReturnsFalse_ForEmpty()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidMenuChoice(""));
    }

    [TestMethod]
    public void IsValidMenuChoice_ReturnsFalse_ForLetters()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidMenuChoice("abc"));
    }

    // --- IsValidPathChoice ---

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForN()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("n"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForNorth()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("north"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForS()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("s"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForSouth()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("south"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForE()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("e"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForExit()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("exit"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForInvalidInput()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice("west"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForNull()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice(null));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForEmpty()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice(""));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForWhitespace()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice("   "));
    }

    // --- IsValidMenuChoice: padded input ---

    [TestMethod]
    public void IsValidMenuChoice_ReturnsTrue_ForPaddedOne()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidMenuChoice("  1  "));
    }

    [TestMethod]
    public void IsValidMenuChoice_ReturnsTrue_ForPaddedTwo()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidMenuChoice("  2  "));
    }

    // --- IsValidPathChoice: case insensitivity ---

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForUppercaseN()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("N"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForUppercaseNorth()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("NORTH"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForUppercaseS()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("S"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForUppercaseE()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("E"));
    }

    // --- IsValidPathChoice: padded input ---

    [TestMethod]
    public void IsValidPathChoice_ReturnsTrue_ForPaddedNorth()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidPathChoice("  north  "));
    }

    // --- IsValidPathChoice: numbers should be invalid ---

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForOne()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice("1"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForTwo()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice("2"));
    }

    [TestMethod]
    public void IsValidPathChoice_ReturnsFalse_ForThree()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidPathChoice("3"));
    }

    // --- IsValidCombatChoice ---

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForA()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("a"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForAttack()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("attack"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForR()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("r"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForRetreat()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("retreat"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsFalse_ForInvalidInput()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidCombatChoice("fight"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsFalse_ForNull()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidCombatChoice(null));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsFalse_ForEmpty()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidCombatChoice(""));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsFalse_ForWhitespace()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidCombatChoice("   "));
    }

    // --- IsValidCombatChoice: case insensitivity ---

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForUppercaseA()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("A"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForUppercaseAttack()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("ATTACK"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForUppercaseR()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("R"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForUppercaseRetreat()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("RETREAT"));
    }

    // --- IsValidCombatChoice: numbers should be invalid (old prompt said "1. Attack 2. Retreat") ---

    [TestMethod]
    public void IsValidCombatChoice_ReturnsFalse_ForOne()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidCombatChoice("1"));
    }

    [TestMethod]
    public void IsValidCombatChoice_ReturnsFalse_ForTwo()
    {
        Game game = new Game();
        Assert.IsFalse(game.IsValidCombatChoice("2"));
    }

    // --- IsValidCombatChoice: padded input ---

    [TestMethod]
    public void IsValidCombatChoice_ReturnsTrue_ForPaddedA()
    {
        Game game = new Game();
        Assert.IsTrue(game.IsValidCombatChoice("  a  "));
    }

    // --- EndGame: Italian ---

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerWinsInItalian()
    {
        Game game = new Game();
        game.EndGame(true, false, GetMessages("Italian"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerLosesInItalian()
    {
        Game game = new Game();
        game.EndGame(false, false, GetMessages("Italian"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_WhenPlayerRetreatsInItalian()
    {
        Game game = new Game();
        game.EndGame(false, true, GetMessages("Italian"));
    }

    // --- GetLanguageFromChoice ---

    [TestMethod]
    public void GetLanguageFromChoice_ReturnsItalian_ForFour()
    {
        Game game = new Game();
        Assert.AreEqual("Italian", game.GetLanguageFromChoice("4"));
    }

    [TestMethod]
    public void GetLanguageFromChoice_ReturnsItalian_ForPaddedFour()
    {
        Game game = new Game();
        Assert.AreEqual("Italian", game.GetLanguageFromChoice("  4  "));
    }

    [TestMethod]
    [DataRow("1", "English")]
    [DataRow("2", "Spanish")]
    [DataRow("3", "French")]
    public void GetLanguageFromChoice_PreservesExistingLanguages(string input, string expected)
    {
        Game game = new Game();
        Assert.AreEqual(expected, game.GetLanguageFromChoice(input));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("5")]
    [DataRow("Italian")]
    public void GetLanguageFromChoice_DefaultsToEnglish_ForInvalidInput(string? input)
    {
        Game game = new Game();
        Assert.AreEqual("English", game.GetLanguageFromChoice(input));
    }
}
