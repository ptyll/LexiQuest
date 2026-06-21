using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiQuest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase9DomainTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRounds_GameSessions_GameSessionId",
                table: "GameRounds");

            migrationBuilder.DropIndex(
                name: "IX_Users_LockoutEnd",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "TotalXP",
                table: "Users",
                newName: "Stats_TotalXP");

            migrationBuilder.RenameColumn(
                name: "TotalWordsSolved",
                table: "Users",
                newName: "Stats_TotalWordsSolved");

            migrationBuilder.RenameColumn(
                name: "Theme",
                table: "Users",
                newName: "Preferences_Theme");

            migrationBuilder.RenameColumn(
                name: "StreakLongestDays",
                table: "Users",
                newName: "Streak_LongestDays");

            migrationBuilder.RenameColumn(
                name: "StreakLastActivityDate",
                table: "Users",
                newName: "Streak_LastActivityDate");

            migrationBuilder.RenameColumn(
                name: "StreakCurrentDays",
                table: "Users",
                newName: "Streak_CurrentDays");

            migrationBuilder.RenameColumn(
                name: "SoundsEnabled",
                table: "Users",
                newName: "Preferences_SoundsEnabled");

            migrationBuilder.RenameColumn(
                name: "PremiumPlan",
                table: "Users",
                newName: "Premium_Plan");

            migrationBuilder.RenameColumn(
                name: "PremiumExpiresAt",
                table: "Users",
                newName: "Premium_ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Users",
                newName: "Stats_Level");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Users",
                newName: "Preferences_Language");

            migrationBuilder.RenameColumn(
                name: "IsPremium",
                table: "Users",
                newName: "Premium_IsPremium");

            migrationBuilder.RenameColumn(
                name: "AverageResponseTime",
                table: "Users",
                newName: "Stats_AverageResponseTime");

            migrationBuilder.RenameColumn(
                name: "AnimationsEnabled",
                table: "Users",
                newName: "Preferences_AnimationsEnabled");

            migrationBuilder.RenameColumn(
                name: "Accuracy",
                table: "Users",
                newName: "Stats_Accuracy");

            migrationBuilder.RenameColumn(
                name: "CurrentLevel",
                table: "GameSessions",
                newName: "TotalRounds");

            migrationBuilder.RenameColumn(
                name: "Scrambled",
                table: "GameRounds",
                newName: "ScrambledWord");

            migrationBuilder.RenameColumn(
                name: "GameSessionId",
                table: "GameRounds",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_GameRounds_GameSessionId",
                table: "GameRounds",
                newName: "IX_GameRounds_SessionId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Preferences_Theme",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Premium_Plan",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Preferences_Language",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoinBalance",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLifeLostAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LivesRemaining",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxLives",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextLifeRegenAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Preferences_AchievementNotificationsEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Preferences_DailyChallengeReminderEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Preferences_EmailNotificationsEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Preferences_LeagueUpdatesEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Preferences_PushNotificationsEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Preferences_StreakReminderTime",
                table: "Users",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Privacy_LeaderboardVisible",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Privacy_ProfileVisibility",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Privacy_StatsSharingEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BossType",
                table: "GameSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComboCount",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswers",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRound",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ForbiddenLetters",
                table: "GameSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LevelNumber",
                table: "GameSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevealedLettersCount",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "GameRounds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ForbiddenLetters",
                table: "GameRounds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "GameRounds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RevealedLettersCount",
                table: "GameRounds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RevealedPositions",
                table: "GameRounds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "GameRounds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitSeconds",
                table: "GameRounds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentMs",
                table: "GameRounds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    XPReward = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequiredValue = table.Column<int>(type: "int", nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRoleAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoinTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BalanceAfter = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinTransaction_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomDictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WordCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomDictionaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyChallengeCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeTaken = table.Column<TimeSpan>(type: "time", nullable: false),
                    XPEarned = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallengeCompletions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Modifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DictionaryWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DictionaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    TotalLevels = table.Column<int>(type: "int", nullable: false),
                    WordLengthMin = table.Column<int>(type: "int", nullable: false),
                    WordLengthMax = table.Column<int>(type: "int", nullable: false),
                    TimePerWord = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player1Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Player2Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Player1Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Player2Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Player1Score = table.Column<int>(type: "int", nullable: false),
                    Player2Score = table.Column<int>(type: "int", nullable: false),
                    Player1Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Player2Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Player1MaxCombo = table.Column<int>(type: "int", nullable: false),
                    Player2MaxCombo = table.Column<int>(type: "int", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDraw = table.Column<bool>(type: "bit", nullable: false),
                    Player1XPEarned = table.Column<int>(type: "int", nullable: false),
                    Player2XPEarned = table.Column<int>(type: "int", nullable: false),
                    Player1LeagueXPEarned = table.Column<int>(type: "int", nullable: false),
                    Player2LeagueXPEarned = table.Column<int>(type: "int", nullable: false),
                    IsPrivateRoom = table.Column<bool>(type: "bit", nullable: false),
                    RoomCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SeriesPlayer1Wins = table.Column<int>(type: "int", nullable: true),
                    SeriesPlayer2Wins = table.Column<int>(type: "int", nullable: true),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StreakReminder = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StreakReminderTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    LeagueUpdates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AchievementNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DailyChallengeReminder = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPremiumOnly = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsLimited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AvailableUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreakProtections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShieldsRemaining = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FreezeUsedThisWeek = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastShieldActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsShieldActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreakProtections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInvites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamJoinRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamJoinRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LeaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsUnlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEquipped = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeagueParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyXP = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Rank = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsPromoted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDemoted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueParticipants_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PathLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsBoss = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPerfect = table.Column<bool>(type: "bit", nullable: false),
                    LearningPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PathLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PathLevels_LearningPaths_LearningPathId",
                        column: x => x.LearningPathId,
                        principalTable: "LearningPaths",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeeklyXP = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AllTimeXP = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Wins = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_UserId_StartedAt",
                table: "GameSessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GameRounds_RoundNumber",
                table: "GameRounds",
                column: "RoundNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Category",
                table: "Achievements",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Key",
                table: "Achievements",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoleAssignments_UserId",
                table: "AdminRoleAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoleAssignments_UserId_Role",
                table: "AdminRoleAssignments",
                columns: new[] { "UserId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_UserId",
                table: "CoinTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDictionaries_IsPublic",
                table: "CustomDictionaries",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDictionaries_UserId",
                table: "CustomDictionaries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallengeCompletions_ChallengeDate",
                table: "DailyChallengeCompletions",
                column: "ChallengeDate");

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallengeCompletions_UserId_ChallengeDate",
                table: "DailyChallengeCompletions",
                columns: new[] { "UserId", "ChallengeDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_Date",
                table: "DailyChallenges",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryWords_DictionaryId",
                table: "DictionaryWords",
                column: "DictionaryId");

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryWords_DictionaryId_Word",
                table: "DictionaryWords",
                columns: new[] { "DictionaryId", "Word" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipants_LeagueId",
                table: "LeagueParticipants",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipants_UserId",
                table: "LeagueParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipants_UserId_LeagueId",
                table: "LeagueParticipants",
                columns: new[] { "UserId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipants_WeeklyXP",
                table: "LeagueParticipants",
                column: "WeeklyXP");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_Tier_IsActive",
                table: "Leagues",
                columns: new[] { "Tier", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_WeekStart",
                table: "Leagues",
                column: "WeekStart");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_CompletedAt",
                table: "MatchResults",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_IsPrivateRoom",
                table: "MatchResults",
                column: "IsPrivateRoom");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_MatchId",
                table: "MatchResults",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_Player1Id",
                table: "MatchResults",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_Player2Id",
                table: "MatchResults",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PathLevels_LearningPathId",
                table: "PathLevels",
                column: "LearningPathId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_Endpoint",
                table: "PushSubscriptions",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_UserId",
                table: "PushSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopItems_Category",
                table: "ShopItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ShopItems_IsLimited",
                table: "ShopItems",
                column: "IsLimited");

            migrationBuilder.CreateIndex(
                name: "IX_ShopItems_IsPremiumOnly",
                table: "ShopItems",
                column: "IsPremiumOnly");

            migrationBuilder.CreateIndex(
                name: "IX_StreakProtections_UserId",
                table: "StreakProtections",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ExpiresAt",
                table: "Subscriptions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_StripeSubscriptionId",
                table: "Subscriptions",
                column: "StripeSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvites_InvitedUserId",
                table: "TeamInvites",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvites_Status",
                table: "TeamInvites",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvites_TeamId_InvitedUserId",
                table: "TeamInvites",
                columns: new[] { "TeamId", "InvitedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamJoinRequests_Status",
                table: "TeamJoinRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TeamJoinRequests_TeamId_UserId",
                table: "TeamJoinRequests",
                columns: new[] { "TeamId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId",
                table: "TeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId",
                table: "TeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId_TeamId",
                table: "TeamMembers",
                columns: new[] { "UserId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeaderId",
                table: "Teams",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Tag",
                table: "Teams",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId",
                table: "UserAchievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInventoryItems_UserId",
                table: "UserInventoryItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventoryItems_UserId_IsEquipped",
                table: "UserInventoryItems",
                columns: new[] { "UserId", "IsEquipped" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInventoryItems_UserId_ShopItemId",
                table: "UserInventoryItems",
                columns: new[] { "UserId", "ShopItemId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRounds_GameSessions_SessionId",
                table: "GameRounds",
                column: "SessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRounds_GameSessions_SessionId",
                table: "GameRounds");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "AdminRoleAssignments");

            migrationBuilder.DropTable(
                name: "CoinTransaction");

            migrationBuilder.DropTable(
                name: "CustomDictionaries");

            migrationBuilder.DropTable(
                name: "DailyChallengeCompletions");

            migrationBuilder.DropTable(
                name: "DailyChallenges");

            migrationBuilder.DropTable(
                name: "DictionaryWords");

            migrationBuilder.DropTable(
                name: "LeagueParticipants");

            migrationBuilder.DropTable(
                name: "MatchResults");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PathLevels");

            migrationBuilder.DropTable(
                name: "PushSubscriptions");

            migrationBuilder.DropTable(
                name: "ShopItems");

            migrationBuilder.DropTable(
                name: "StreakProtections");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "TeamInvites");

            migrationBuilder.DropTable(
                name: "TeamJoinRequests");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserInventoryItems");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "LearningPaths");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_UserId_StartedAt",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameRounds_RoundNumber",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CoinBalance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLifeLostAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LivesRemaining",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaxLives",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NextLifeRegenAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_AchievementNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_DailyChallengeReminderEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_EmailNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_LeagueUpdatesEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_PushNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_StreakReminderTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Privacy_LeaderboardVisible",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Privacy_ProfileVisibility",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Privacy_StatsSharingEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BossType",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "ComboCount",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CorrectAnswers",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CurrentRound",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "ForbiddenLetters",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "LevelNumber",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "RevealedLettersCount",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "ForbiddenLetters",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "RevealedLettersCount",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "RevealedPositions",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "TimeLimitSeconds",
                table: "GameRounds");

            migrationBuilder.DropColumn(
                name: "TimeSpentMs",
                table: "GameRounds");

            migrationBuilder.RenameColumn(
                name: "Streak_LongestDays",
                table: "Users",
                newName: "StreakLongestDays");

            migrationBuilder.RenameColumn(
                name: "Streak_LastActivityDate",
                table: "Users",
                newName: "StreakLastActivityDate");

            migrationBuilder.RenameColumn(
                name: "Streak_CurrentDays",
                table: "Users",
                newName: "StreakCurrentDays");

            migrationBuilder.RenameColumn(
                name: "Stats_TotalXP",
                table: "Users",
                newName: "TotalXP");

            migrationBuilder.RenameColumn(
                name: "Stats_TotalWordsSolved",
                table: "Users",
                newName: "TotalWordsSolved");

            migrationBuilder.RenameColumn(
                name: "Stats_Level",
                table: "Users",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "Stats_AverageResponseTime",
                table: "Users",
                newName: "AverageResponseTime");

            migrationBuilder.RenameColumn(
                name: "Stats_Accuracy",
                table: "Users",
                newName: "Accuracy");

            migrationBuilder.RenameColumn(
                name: "Premium_Plan",
                table: "Users",
                newName: "PremiumPlan");

            migrationBuilder.RenameColumn(
                name: "Premium_IsPremium",
                table: "Users",
                newName: "IsPremium");

            migrationBuilder.RenameColumn(
                name: "Premium_ExpiresAt",
                table: "Users",
                newName: "PremiumExpiresAt");

            migrationBuilder.RenameColumn(
                name: "Preferences_Theme",
                table: "Users",
                newName: "Theme");

            migrationBuilder.RenameColumn(
                name: "Preferences_SoundsEnabled",
                table: "Users",
                newName: "SoundsEnabled");

            migrationBuilder.RenameColumn(
                name: "Preferences_Language",
                table: "Users",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "Preferences_AnimationsEnabled",
                table: "Users",
                newName: "AnimationsEnabled");

            migrationBuilder.RenameColumn(
                name: "TotalRounds",
                table: "GameSessions",
                newName: "CurrentLevel");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "GameRounds",
                newName: "GameSessionId");

            migrationBuilder.RenameColumn(
                name: "ScrambledWord",
                table: "GameRounds",
                newName: "Scrambled");

            migrationBuilder.RenameIndex(
                name: "IX_GameRounds_SessionId",
                table: "GameRounds",
                newName: "IX_GameRounds_GameSessionId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PremiumPlan",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LockoutEnd",
                table: "Users",
                column: "LockoutEnd");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRounds_GameSessions_GameSessionId",
                table: "GameRounds",
                column: "GameSessionId",
                principalTable: "GameSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
