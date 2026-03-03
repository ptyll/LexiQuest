using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Infrastructure.Persistence;

public static class SeedData
{
    public static List<Word> GetWords()
    {
        var words = new List<Word>();

        // Beginner (3-5 písmen): 50+ slov
        // Animals
        words.Add(Word.Create("pes", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("kos", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("had", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("lev", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("sova", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("ryba", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("orel", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("vlk", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("koza", DifficultyLevel.Beginner, WordCategory.Animals));
        words.Add(Word.Create("liška", DifficultyLevel.Beginner, WordCategory.Animals));

        // Food
        words.Add(Word.Create("dort", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("maso", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("syr", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("rize", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("med", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("jablko", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("hruska", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("chleb", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("mleko", DifficultyLevel.Beginner, WordCategory.Food));
        words.Add(Word.Create("vejce", DifficultyLevel.Beginner, WordCategory.Food));

        // Colors
        words.Add(Word.Create("bila", DifficultyLevel.Beginner, WordCategory.Colors));
        words.Add(Word.Create("modra", DifficultyLevel.Beginner, WordCategory.Colors));
        words.Add(Word.Create("zluta", DifficultyLevel.Beginner, WordCategory.Colors));
        words.Add(Word.Create("ruda", DifficultyLevel.Beginner, WordCategory.Colors));
        words.Add(Word.Create("seda", DifficultyLevel.Beginner, WordCategory.Colors));

        // Nature
        words.Add(Word.Create("les", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("hora", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("reka", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("more", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("pole", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("louka", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("strom", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("kvet", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("trava", DifficultyLevel.Beginner, WordCategory.Nature));
        words.Add(Word.Create("mrak", DifficultyLevel.Beginner, WordCategory.Nature));

        // Household
        words.Add(Word.Create("dum", DifficultyLevel.Beginner, WordCategory.Household));
        words.Add(Word.Create("stul", DifficultyLevel.Beginner, WordCategory.Household));
        words.Add(Word.Create("zidle", DifficultyLevel.Beginner, WordCategory.Household));
        words.Add(Word.Create("okno", DifficultyLevel.Beginner, WordCategory.Household));
        words.Add(Word.Create("dvere", DifficultyLevel.Beginner, WordCategory.Household));
        words.Add(Word.Create("lampa", DifficultyLevel.Beginner, WordCategory.Household));
        words.Add(Word.Create("postel", DifficultyLevel.Beginner, WordCategory.Household));

        // Technology
        words.Add(Word.Create("myš", DifficultyLevel.Beginner, WordCategory.Technology));
        words.Add(Word.Create("disk", DifficultyLevel.Beginner, WordCategory.Technology));
        words.Add(Word.Create("sit", DifficultyLevel.Beginner, WordCategory.Technology));
        words.Add(Word.Create("data", DifficultyLevel.Beginner, WordCategory.Technology));
        words.Add(Word.Create("kod", DifficultyLevel.Beginner, WordCategory.Technology));

        // Sports
        words.Add(Word.Create("mic", DifficultyLevel.Beginner, WordCategory.Sports));
        words.Add(Word.Create("gol", DifficultyLevel.Beginner, WordCategory.Sports));
        words.Add(Word.Create("beh", DifficultyLevel.Beginner, WordCategory.Sports));
        words.Add(Word.Create("skok", DifficultyLevel.Beginner, WordCategory.Sports));
        words.Add(Word.Create("plav", DifficultyLevel.Beginner, WordCategory.Sports));

        // Intermediate (5-7 písmen): 30+ slov
        words.Add(Word.Create("motyl", DifficultyLevel.Intermediate, WordCategory.Animals));
        words.Add(Word.Create("zajíc", DifficultyLevel.Intermediate, WordCategory.Animals));
        words.Add(Word.Create("medved", DifficultyLevel.Intermediate, WordCategory.Animals));
        words.Add(Word.Create("kocour", DifficultyLevel.Intermediate, WordCategory.Animals));
        words.Add(Word.Create("delfin", DifficultyLevel.Intermediate, WordCategory.Animals));

        words.Add(Word.Create("brambor", DifficultyLevel.Intermediate, WordCategory.Food));
        words.Add(Word.Create("rajce", DifficultyLevel.Intermediate, WordCategory.Food));
        words.Add(Word.Create("mrkev", DifficultyLevel.Intermediate, WordCategory.Food));
        words.Add(Word.Create("cibule", DifficultyLevel.Intermediate, WordCategory.Food));
        words.Add(Word.Create("cesnek", DifficultyLevel.Intermediate, WordCategory.Food));

        words.Add(Word.Create("fialova", DifficultyLevel.Intermediate, WordCategory.Colors));
        words.Add(Word.Create("oranzova", DifficultyLevel.Intermediate, WordCategory.Colors));
        words.Add(Word.Create("zelena", DifficultyLevel.Intermediate, WordCategory.Colors));

        words.Add(Word.Create("jeskyně", DifficultyLevel.Intermediate, WordCategory.Nature));
        words.Add(Word.Create("vodopad", DifficultyLevel.Intermediate, WordCategory.Nature));
        words.Add(Word.Create("ostrov", DifficultyLevel.Intermediate, WordCategory.Nature));
        words.Add(Word.Create("udoli", DifficultyLevel.Intermediate, WordCategory.Nature));
        words.Add(Word.Create("vulkan", DifficultyLevel.Intermediate, WordCategory.Nature));

        words.Add(Word.Create("klavir", DifficultyLevel.Intermediate, WordCategory.Music));
        words.Add(Word.Create("kytara", DifficultyLevel.Intermediate, WordCategory.Music));
        words.Add(Word.Create("housle", DifficultyLevel.Intermediate, WordCategory.Music));
        words.Add(Word.Create("buben", DifficultyLevel.Intermediate, WordCategory.Music));
        words.Add(Word.Create("flétna", DifficultyLevel.Intermediate, WordCategory.Music));

        words.Add(Word.Create("monitor", DifficultyLevel.Intermediate, WordCategory.Technology));
        words.Add(Word.Create("server", DifficultyLevel.Intermediate, WordCategory.Technology));
        words.Add(Word.Create("tablet", DifficultyLevel.Intermediate, WordCategory.Technology));
        words.Add(Word.Create("router", DifficultyLevel.Intermediate, WordCategory.Technology));

        words.Add(Word.Create("fotbal", DifficultyLevel.Intermediate, WordCategory.Sports));
        words.Add(Word.Create("tenis", DifficultyLevel.Intermediate, WordCategory.Sports));
        words.Add(Word.Create("hokej", DifficultyLevel.Intermediate, WordCategory.Sports));

        words.Add(Word.Create("kuchyn", DifficultyLevel.Intermediate, WordCategory.Household));
        words.Add(Word.Create("zahrada", DifficultyLevel.Intermediate, WordCategory.Household));
        words.Add(Word.Create("koberec", DifficultyLevel.Intermediate, WordCategory.Household));

        // Advanced (7-10 písmen): 15+ slov
        words.Add(Word.Create("krokodyl", DifficultyLevel.Advanced, WordCategory.Animals));
        words.Add(Word.Create("nosorozec", DifficultyLevel.Advanced, WordCategory.Animals));
        words.Add(Word.Create("chameleon", DifficultyLevel.Advanced, WordCategory.Animals));

        words.Add(Word.Create("broskev", DifficultyLevel.Advanced, WordCategory.Food));
        words.Add(Word.Create("mandarinka", DifficultyLevel.Advanced, WordCategory.Food));
        words.Add(Word.Create("pomeranc", DifficultyLevel.Advanced, WordCategory.Food));

        words.Add(Word.Create("sopka", DifficultyLevel.Advanced, WordCategory.Nature));
        words.Add(Word.Create("kontinent", DifficultyLevel.Advanced, WordCategory.Nature));
        words.Add(Word.Create("atmosfera", DifficultyLevel.Advanced, WordCategory.Nature));

        words.Add(Word.Create("algoritmus", DifficultyLevel.Advanced, WordCategory.Technology));
        words.Add(Word.Create("databaze", DifficultyLevel.Advanced, WordCategory.Technology));
        words.Add(Word.Create("procesor", DifficultyLevel.Advanced, WordCategory.Technology));

        words.Add(Word.Create("badminton", DifficultyLevel.Advanced, WordCategory.Sports));
        words.Add(Word.Create("atletika", DifficultyLevel.Advanced, WordCategory.Sports));
        words.Add(Word.Create("basketbal", DifficultyLevel.Advanced, WordCategory.Sports));

        // Expert (10+ písmen): 5+ slov
        words.Add(Word.Create("programovani", DifficultyLevel.Expert, WordCategory.Technology));
        words.Add(Word.Create("architektura", DifficultyLevel.Expert, WordCategory.Technology));
        words.Add(Word.Create("infrastruktura", DifficultyLevel.Expert, WordCategory.Technology));
        words.Add(Word.Create("komunikace", DifficultyLevel.Expert, WordCategory.Science));
        words.Add(Word.Create("matematika", DifficultyLevel.Expert, WordCategory.Science));

        return words;
    }
}
