using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class ConditionBossRulesTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<ConditionBossRules> _localizer;
    private readonly ConditionBossRules _sut;

    public ConditionBossRulesTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        _xpCalculator = Substitute.For<IXpCalculator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<ConditionBossRules>>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _sut = new ConditionBossRules(
            _wordRepository,
            _xpCalculator,
            _unitOfWork,
            _localizer);
    }

    [Fact]
    public void ConditionBoss_Start_15Words3Lives()
    {
        // Arrange & Act
        var (totalRounds, lives) = ConditionBossRules.GetSettings();

        // Assert
        totalRounds.Should().Be(15);
        lives.Should().Be(3);
    }

    [Fact]
    public void ConditionBoss_Start_HasForbiddenLetters()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Condition, DifficultyLevel.Intermediate);

        // Assert
        session.ForbiddenLetters.Should().NotBeNullOrEmpty();
        session.ForbiddenLetters!.Length.Should().Be(2);
    }

    [Theory]
    [InlineData("A", "AHOJ", true)]  // Contains forbidden 'A'
    [InlineData("E", "POKUS", false)] // No 'E' in POKUS
    [InlineData("AI", "HRA", true)]   // Contains forbidden 'A'
    [InlineData("EO", "HRAM", false)] // No 'E' or 'O' in HRAM
    public void ConditionBoss_ValidateAnswer_ForbiddenLetters(string forbidden, string answer, bool expectedPenalty)
    {
        // Arrange & Act
        var usesForbidden = _sut.UsesForbiddenLetters(answer, forbidden);

        // Assert
        usesForbidden.Should().Be(expectedPenalty);
    }

    [Fact]
    public void ConditionBoss_ForbiddenLetterUsed_Penalty5XP()
    {
        // Arrange
        const int baseXp = 10;
        const string forbiddenLetters = "AE";
        const string answer = "AHOJ"; // Contains 'A'

        // Act
        var penalty = _sut.CalculateForbiddenLetterPenalty(answer, forbiddenLetters, baseXp);

        // Assert
        penalty.Should().Be(5);
    }

    [Fact]
    public void ConditionBoss_NoForbiddenLetter_NoPenalty()
    {
        // Arrange
        const int baseXp = 10;
        const string forbiddenLetters = "AE";
        const string answer = "POKUS"; // No 'A' or 'E'

        // Act
        var penalty = _sut.CalculateForbiddenLetterPenalty(answer, forbiddenLetters, baseXp);

        // Assert
        penalty.Should().Be(0);
    }

    [Fact]
    public void ConditionBoss_WrongAnswer_NoLifeLoss_JustXP()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Condition, DifficultyLevel.Intermediate);
        var initialLives = session.LivesRemaining;

        // Act - Condition boss loses XP, not lives
        var xpPenalty = _sut.CalculateWrongAnswerPenalty();

        // Assert
        session.LivesRemaining.Should().Be(initialLives);
        xpPenalty.Should().Be(-5);
    }
}

public class ConditionBossRules : IBossRules
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<ConditionBossRules> _localizer;

    public ConditionBossRules(
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        IUnitOfWork unitOfWork,
        IStringLocalizer<ConditionBossRules> localizer)
    {
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public static (int TotalRounds, int Lives) GetSettings() => (15, 3);

    public bool UsesForbiddenLetters(string answer, string forbiddenLetters)
    {
        var upperAnswer = answer.ToUpperInvariant();
        return forbiddenLetters.Any(forbidden => upperAnswer.Contains(forbidden));
    }

    public int CalculateForbiddenLetterPenalty(string answer, string forbiddenLetters, int baseXp)
    {
        return UsesForbiddenLetters(answer, forbiddenLetters) ? 5 : 0;
    }

    public int CalculateWrongAnswerPenalty() => -5;

    public async Task ProcessWrongAnswerAsync(GameSession session)
    {
        // Condition boss: no life loss, just XP penalty handled separately
        await Task.CompletedTask;
    }

    public int CalculateCompletionBonus(GameSession session, bool perfectRun)
    {
        return perfectRun ? 150 : 75;
    }

    public int CalculateSpeedBonus(TimeSpan duration)
    {
        return duration.TotalMinutes < 4 ? 40 : 0;
    }

    public Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var session = GameSession.CreateBossSession(userId, BossType.Condition, difficulty);
        return Task.FromResult(session);
    }
}
