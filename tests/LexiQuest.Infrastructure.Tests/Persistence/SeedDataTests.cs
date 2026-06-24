using FluentAssertions;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Infrastructure.Tests.Persistence;

public class SeedDataTests
{
    [Fact]
    public void WordSeedData_Contains_MinimumWords()
    {
        var words = SeedData.GetWords();

        words.Should().HaveCountGreaterThanOrEqualTo(100);
    }

    [Fact]
    public void WordSeedData_Contains_AllDifficultyLevels()
    {
        var words = SeedData.GetWords();

        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Beginner);
        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Intermediate);
        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Advanced);
        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Expert);
    }

    [Fact]
    public void WordSeedData_Beginner_HasAtLeast50Words()
    {
        var words = SeedData.GetWords();

        words.Count(w => w.Difficulty == DifficultyLevel.Beginner).Should().BeGreaterThanOrEqualTo(50);
    }

    [Fact]
    public void WordSeedData_UsesCorrectCzechWords()
    {
        var expectedReplacements = new Dictionary<string, string>
        {
            ["rize"] = "rýže",
            ["syr"] = "sýr",
            ["hruska"] = "hruška",
            ["mleko"] = "mléko",
            ["bila"] = "bílá",
            ["modra"] = "modrá",
            ["zluta"] = "žlutá",
            ["ruda"] = "rudá",
            ["seda"] = "šedá",
            ["reka"] = "řeka",
            ["more"] = "moře",
            ["kvet"] = "květ",
            ["trava"] = "tráva",
            ["dum"] = "dům",
            ["stul"] = "stůl",
            ["zidle"] = "židle",
            ["dvere"] = "dveře",
            ["sit"] = "síť",
            ["kod"] = "kód",
            ["mic"] = "míč",
            ["gol"] = "gól",
            ["beh"] = "běh",
            ["plav"] = "jóga",
            ["motyl"] = "motýl",
            ["medved"] = "medvěd",
            ["delfin"] = "delfín",
            ["rajce"] = "rajče",
            ["cesnek"] = "česnek",
            ["fialova"] = "fialová",
            ["oranzova"] = "oranžová",
            ["zelena"] = "zelená",
            ["udoli"] = "údolí",
            ["vulkan"] = "vulkán",
            ["klavir"] = "klavír",
            ["kuchyn"] = "kuchyň",
            ["nosorozec"] = "nosorožec",
            ["pomeranc"] = "pomeranč",
            ["atmosfera"] = "atmosféra",
            ["databaze"] = "databáze",
            ["programovani"] = "programování"
        };
        var words = SeedData.GetWords().Select(w => w.Original).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (incorrectWord, correctWord) in expectedReplacements)
        {
            words.Should().NotContain(incorrectWord, $"seed data must use the Czech word '{correctWord}' instead of '{incorrectWord}'");
            words.Should().Contain(correctWord, $"seed data must include the corrected Czech word '{correctWord}'");
        }
    }
}
