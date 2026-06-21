using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Infrastructure.Persistence;

public static class SeedData
{
    public static List<ShopItem> GetShopItems()
    {
        const string imageUrl = "/icon-192.png";

        return
        [
            ShopItem.Create(
                name: "Sova učence",
                description: "Avatar pro hráče, kteří rádi přemýšlejí o krok dopředu.",
                category: ShopCategory.Avatar,
                price: 120,
                rarity: ItemRarity.Common,
                imageUrl: imageUrl),
            ShopItem.CreatePremiumOnly(
                name: "Diamantový avatar",
                description: "Prémiový avatar s legendární aurou.",
                category: ShopCategory.Avatar,
                price: 0,
                rarity: ItemRarity.Legendary,
                imageUrl: imageUrl),
            ShopItem.Create(
                name: "Dřevěný rámeček",
                description: "Jednoduchý rámeček pro čistý profil.",
                category: ShopCategory.Frame,
                price: 200,
                rarity: ItemRarity.Common,
                imageUrl: imageUrl),
            ShopItem.Create(
                name: "Stříbrný rámeček",
                description: "Vzácnější rámeček pro výraznější profil.",
                category: ShopCategory.Frame,
                price: 250,
                rarity: ItemRarity.Rare,
                imageUrl: imageUrl),
            ShopItem.Create(
                name: "Noční téma",
                description: "Klidné tmavé téma pro večerní hraní.",
                category: ShopCategory.Theme,
                price: 900,
                rarity: ItemRarity.Epic,
                imageUrl: imageUrl),
            ShopItem.Create(
                name: "XP boost malý",
                description: "Krátké posílení pro rychlejší postup.",
                category: ShopCategory.Boost,
                price: 150,
                rarity: ItemRarity.Rare,
                imageUrl: imageUrl)
        ];
    }

    public static List<LearningPath> GetLearningPaths()
    {
        var paths = new List<LearningPath>
        {
            CreatePath(
                name: "Začátečník",
                description: "Krátká slova pro rozehřátí a první jisté výhry.",
                difficulty: DifficultyLevel.Beginner,
                totalLevels: 20,
                wordLengthMin: 3,
                wordLengthMax: 5,
                timePerWord: 30),
            CreatePath(
                name: "Cesta pro pokročilé",
                description: "Delší slova a svižnější tempo pro hráče na úrovni 5+.",
                difficulty: DifficultyLevel.Intermediate,
                totalLevels: 25,
                wordLengthMin: 5,
                wordLengthMax: 7,
                timePerWord: 25),
            CreatePath(
                name: "Pokročilá cesta",
                description: "Náročnější skládání slov s důrazem na přesnost.",
                difficulty: DifficultyLevel.Advanced,
                totalLevels: 30,
                wordLengthMin: 7,
                wordLengthMax: 10,
                timePerWord: 20),
            CreatePath(
                name: "Expertní cesta",
                description: "Nejtěžší slova a minimální prostor pro zaváhání.",
                difficulty: DifficultyLevel.Expert,
                totalLevels: 40,
                wordLengthMin: 10,
                wordLengthMax: 14,
                timePerWord: 18)
        };

        return paths;
    }

    public static List<Achievement> GetAchievements()
    {
        return
        [
            Achievement.Create(
                key: "first_word",
                category: AchievementCategory.Performance,
                xpReward: 10,
                name: "První slovo",
                description: "Vyřešte své první slovo.",
                requiredValue: 1,
                iconName: "trophy"),
            Achievement.Create(
                key: "100_words",
                category: AchievementCategory.Performance,
                xpReward: 100,
                name: "Sto slov",
                description: "Vyřešte celkem 100 slov.",
                requiredValue: 100,
                iconName: "target"),
            Achievement.Create(
                key: "1000_words",
                category: AchievementCategory.Performance,
                xpReward: 500,
                name: "Mistr slov",
                description: "Vyřešte celkem 1000 slov.",
                requiredValue: 1000,
                iconName: "sparkles"),
            Achievement.Create(
                key: "streak_3",
                category: AchievementCategory.Streak,
                xpReward: 25,
                name: "Třídenní série",
                description: "Udržte sérii 3 dny po sobě.",
                requiredValue: 3,
                iconName: "flame"),
            Achievement.Create(
                key: "streak_7",
                category: AchievementCategory.Streak,
                xpReward: 50,
                name: "Týdenní série",
                description: "Udržte sérii 7 dní po sobě.",
                requiredValue: 7,
                iconName: "flame"),
            Achievement.Create(
                key: "streak_14",
                category: AchievementCategory.Streak,
                xpReward: 100,
                name: "Čtrnáctidenní série",
                description: "Udržte sérii 14 dní po sobě.",
                requiredValue: 14,
                iconName: "flame"),
            Achievement.Create(
                key: "streak_30",
                category: AchievementCategory.Streak,
                xpReward: 200,
                name: "Měsíční série",
                description: "Udržte sérii 30 dní po sobě.",
                requiredValue: 30,
                iconName: "flame"),
            Achievement.Create(
                key: "streak_365",
                category: AchievementCategory.Streak,
                xpReward: 1000,
                name: "Rok bez pauzy",
                description: "Udržte sérii 365 dní po sobě.",
                requiredValue: 365,
                iconName: "calendar"),
            Achievement.Create(
                key: "beginner_master",
                category: AchievementCategory.Difficulty,
                xpReward: 75,
                name: "Mistr začátečník",
                description: "Dokončete 25 začátečnických slov.",
                requiredValue: 25,
                iconName: "medal"),
            Achievement.Create(
                key: "expert_master",
                category: AchievementCategory.Difficulty,
                xpReward: 250,
                name: "Expert na slova",
                description: "Dokončete 50 expertních slov.",
                requiredValue: 50,
                iconName: "shield"),
            Achievement.Create(
                key: "path_complete",
                category: AchievementCategory.Special,
                xpReward: 150,
                name: "Dokončená cesta",
                description: "Dokončete libovolnou učební cestu.",
                requiredValue: 1,
                iconName: "map"),
            Achievement.Create(
                key: "boss_defeated",
                category: AchievementCategory.Special,
                xpReward: 200,
                name: "Poražený boss",
                description: "Porazte první boss level.",
                requiredValue: 1,
                iconName: "swords"),
            Achievement.Create(
                key: "perfect_boss",
                category: AchievementCategory.Special,
                xpReward: 350,
                name: "Perfektní boss",
                description: "Porazte boss level bez chyby.",
                requiredValue: 1,
                iconName: "crown")
        ];
    }

    private static LearningPath CreatePath(
        string name,
        string description,
        DifficultyLevel difficulty,
        int totalLevels,
        int wordLengthMin,
        int wordLengthMax,
        int timePerWord)
    {
        var path = LearningPath.Create(
            name,
            description,
            difficulty,
            totalLevels,
            wordLengthMin,
            wordLengthMax,
            timePerWord);

        for (var level = 1; level <= totalLevels; level++)
        {
            path.AddLevel(level, isBoss: level % 5 == 0);
        }

        if (path.Levels.Count > 0)
        {
            path.Levels[0].Unlock();
        }

        return path;
    }

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
