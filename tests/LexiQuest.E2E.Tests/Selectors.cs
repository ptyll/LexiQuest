namespace LexiQuest.E2E.Tests;

internal static class Selectors
{
    public static class Auth
    {
        public const string LoginPage = "login-page";
        public const string LoginForm = "login-form";
        public const string LoginSubmit = "login-submit";
        public const string LoginForgotPassword = "login-forgot-password";
        public const string RegisterPage = "register-page";
        public const string RegisterForm = "register-form";
        public const string RegisterSubmit = "register-submit";
        public const string PasswordResetRequestPage = "password-reset-request-page";
        public const string PasswordResetRequestForm = "password-reset-request-form";
        public const string PasswordResetRequestSubmit = "password-reset-request-submit";
        public const string PasswordResetConfirmPage = "password-reset-confirm-page";
        public const string PasswordResetConfirmForm = "password-reset-confirm-form";
        public const string PasswordResetConfirmSubmit = "password-reset-confirm-submit";
        public const string PasswordStrength = "password-strength";
    }

    public static class Game
    {
        public const string StartScreen = "game-start-screen";
        public const string ModeTraining = "game-mode-training";
        public const string ModeTimeAttack = "game-mode-time-attack";
        public const string Arena = "game-arena";
        public const string AnswerInput = "game-answer-input";
        public const string ScrambledWord = "game-scrambled-word";
        public const string Submit = "game-submit";
        public const string Skip = "game-skip";
        public const string Feedback = "game-feedback";
        public const string ErrorState = "game-error-state";
        public const string Timer = "game-timer";
        public const string OfflineBadge = "game-offline-badge";
        public const string Lives = "game-lives";
        public const string LivesCount = "game-lives-count";
        public const string LivesRegen = "game-lives-regen";
        public const string LowLivesWarning = "game-low-lives-warning";
        public const string GameOver = "game-over";
        public const string LevelUpModal = "level-up-modal";
        public const string LevelUpContinue = "level-up-continue";
        public const string LevelComplete = "game-level-complete";
    }

    public static class Dashboard
    {
        public const string Page = "dashboard-page";
        public const string Skeleton = "dashboard-skeleton";
        public const string Error = "dashboard-error";
        public const string StatsGrid = "dashboard-stats-grid";
        public const string TotalXpStat = "dashboard-total-xp-stat";
        public const string AccuracyStat = "dashboard-accuracy-stat";
        public const string AverageTimeStat = "dashboard-average-time-stat";
        public const string XpProgress = "dashboard-xp-progress";
        public const string XpBar = "xp-bar";
        public const string XpBarLevel = "xp-bar-level";
        public const string XpBarText = "xp-bar-text";
        public const string XpBarFill = "xp-bar-fill";
        public const string StreakIndicator = "dashboard-streak-indicator";
        public const string StreakRisk = "dashboard-streak-risk";
        public const string StreakTimer = "dashboard-streak-timer";
        public const string ShieldActivate = "dashboard-shield-activate";
        public const string ShieldActive = "dashboard-shield-active";
        public const string ShieldBuy = "dashboard-shield-buy";
        public const string ShieldsRemaining = "dashboard-shields-remaining";
        public const string FreezeBadge = "dashboard-freeze-badge";
    }

    public static class Layout
    {
        public const string Sidebar = "app-sidebar";
    }

    public static class Notifications
    {
        public const string Bell = "notification-bell";
        public const string BellButton = "notification-bell-button";
        public const string UnreadBadge = "notification-unread-badge";
        public const string Dropdown = "notification-dropdown";
        public const string Empty = "notification-empty";
        public const string MarkAllRead = "notification-mark-all-read";
        public const string Group = "notification-group";
        public const string GroupLabel = "notification-group-label";
        public const string Item = "notification-item";
        public const string Title = "notification-title";
        public const string Message = "notification-message";
        public const string Time = "notification-time";
        public const string Action = "notification-action";
    }

    public static class Paths
    {
        public const string Page = "paths-page";
        public const string Card = "path-card";
        public const string BeginnerCard = "path-card-beginner";
        public const string IntermediateCard = "path-card-intermediate";
        public const string AdvancedCard = "path-card-advanced";
        public const string ExpertCard = "path-card-expert";
        public const string LockedBadge = "path-locked";
        public const string DetailPage = "path-detail-page";
        public const string Map = "path-map";
        public const string Level = "path-level";
        public const string LevelDetail = "path-level-detail";
        public const string LevelDetailStatus = "path-level-detail-status";
        public const string LevelDetailWordCount = "path-level-detail-word-count";
        public const string LevelDetailTime = "path-level-detail-time";
        public const string LevelDetailHints = "path-level-detail-hints";
        public const string LevelDetailLives = "path-level-detail-lives";
        public const string LevelDetailReward = "path-level-detail-reward";
        public const string LevelStart = "path-level-start";
    }

    public static class Leagues
    {
        public const string Page = "leagues-page";
        public const string Tier = "league-tier";
        public const string Countdown = "league-reset-countdown";
        public const string UserPosition = "league-user-position";
        public const string Leaderboard = "league-leaderboard";
        public const string Row = "league-row";
        public const string CurrentUserRow = "league-current-user-row";
        public const string PromotionZone = "league-promotion-zone";
        public const string DemotionZone = "league-demotion-zone";
        public const string History = "league-history";
        public const string Rewards = "league-rewards";
    }

    public static class Daily
    {
        public const string Page = "daily-page";
        public const string ChallengeCard = "daily-challenge-card";
        public const string Modifier = "daily-modifier";
        public const string Start = "daily-start";
        public const string PlayPanel = "daily-play-panel";
        public const string ScrambledWord = "daily-scrambled-word";
        public const string AnswerInput = "daily-answer-input";
        public const string Submit = "daily-submit";
        public const string SubmitError = "daily-submit-error";
        public const string Completed = "daily-completed-state";
        public const string ResultTime = "daily-result-time";
        public const string ResultXp = "daily-result-xp";
        public const string ResultRank = "daily-result-rank";
        public const string Leaderboard = "daily-leaderboard";
        public const string LeaderboardEmpty = "daily-leaderboard-empty";
        public const string LeaderboardRow = "daily-leaderboard-row";
    }

    public static class Achievements
    {
        public const string Page = "achievements-page";
        public const string Progress = "achievements-progress";
        public const string UnlockedCount = "achievements-unlocked-count";
        public const string TotalProgress = "achievements-total-progress";
        public const string Tabs = "achievements-tabs";
        public const string TabAll = "achievement-tab-all";
        public const string TabPerformance = "achievement-tab-performance";
        public const string TabStreak = "achievement-tab-streak";
        public const string Grid = "achievement-grid";
        public const string Card = "achievement-card";
        public const string LockedCard = "achievement-card-locked";
        public const string InProgressCard = "achievement-card-in-progress";
        public const string UnlockedCard = "achievement-card-unlocked";
        public const string CardFirstWord = "achievement-card-first_word";
        public const string CardHundredWords = "achievement-card-100_words";
        public const string CardStreakSeven = "achievement-card-streak_7";
        public const string ProgressSection = "achievement-progress-section";
        public const string UnlockedDate = "achievement-unlocked-date";
        public const string XpReward = "achievement-xp";
        public const string UnlockModal = "achievement-unlock-modal";
        public const string UnlockModalContinue = "achievement-unlock-continue";
    }

    public static class Boss
    {
        public const string MarathonHeader = "marathon-header";
        public const string ConditionHeader = "condition-header";
        public const string TwistHeader = "twist-header";
        public const string ProgressText = "progress-text";
        public const string LivesDisplay = "lives-display";
        public const string LivesCount = "lives-count";
        public const string ScrambledWord = "boss-scrambled-word";
        public const string AnswerInput = "boss-answer-input";
        public const string Submit = "boss-submit";
        public const string ForbiddenWarning = "forbidden-warning-box";
        public const string ForbiddenLetters = "forbidden-letters";
        public const string RevealGrid = "reveal-grid";
        public const string LetterRevealed = "letter-revealed";
        public const string EarlyGuessBonus = "early-guess-bonus";
        public const string VictoryModal = "victory-modal";
        public const string DefeatModal = "defeat-modal";
    }

    public static class Settings
    {
        public const string Page = "settings-page";
        public const string Loading = "settings-loading";
        public const string ProfileSection = "profile-section";
        public const string UsernameInput = "username-input";
        public const string EmailInput = "email-input";
        public const string AvatarFileInput = "avatar-file-input";
        public const string AvatarPreview = "avatar-preview";
        public const string AvatarError = "avatar-error";
        public const string SaveProfile = "save-profile-btn";
        public const string PasswordSection = "password-section";
        public const string CurrentPasswordInput = "current-password-input";
        public const string NewPasswordInput = "new-password-input";
        public const string ConfirmPasswordInput = "confirm-password-input";
        public const string ChangePassword = "change-password-btn";
        public const string PasswordStatus = "password-status";
        public const string PreferencesSection = "preferences-section";
        public const string PushNotificationsToggle = "push-notifications-toggle";
        public const string EmailNotificationsToggle = "email-notifications-toggle";
        public const string LeagueUpdatesToggle = "league-updates-toggle";
        public const string AchievementNotificationsToggle = "achievement-notifications-toggle";
        public const string DailyChallengeReminderToggle = "daily-challenge-reminder-toggle";
        public const string StreakReminderTimeInput = "streak-reminder-time-input";
        public const string ThemeGroup = "theme-group";
        public const string ThemeLight = "theme-option-light";
        public const string ThemeDark = "theme-option-dark";
        public const string ThemeAuto = "theme-option-auto";
        public const string LanguageSelect = "language-select";
        public const string AnimationsToggle = "animations-toggle";
        public const string SoundsToggle = "sounds-toggle";
        public const string SavePreferences = "save-preferences-btn";
        public const string PrivacySection = "privacy-section";
        public const string ProfileVisibilityGroup = "profile-visibility-group";
        public const string VisibilityPublic = "visibility-option-public";
        public const string VisibilityFriends = "visibility-option-friends";
        public const string VisibilityPrivate = "visibility-option-private";
        public const string LeaderboardVisibilityToggle = "leaderboard-visibility-toggle";
        public const string StatsSharingToggle = "stats-sharing-toggle";
        public const string SavePrivacy = "save-privacy-btn";
        public const string DangerZone = "danger-zone";
        public const string Logout = "settings-logout-btn";
        public const string Deactivate = "settings-deactivate-btn";
        public const string Delete = "settings-delete-btn";
        public const string ConfirmModal = "settings-confirm-modal";
        public const string ConfirmInput = "settings-confirm-input";
        public const string ConfirmPrimary = "settings-confirm-primary";
        public const string ConfirmSecondary = "settings-confirm-secondary";
    }

    public static class Premium
    {
        public const string Page = "premium-page";
        public const string ActiveBadge = "premium-active-badge";
        public const string FeatureAvailability = "premium-feature-availability";
        public const string LockedFeature = "premium-feature-locked";
        public const string UnlockedFeature = "premium-feature-unlocked";
        public const string PricingGrid = "premium-pricing-grid";
        public const string MonthlyCard = "premium-plan-monthly";
        public const string YearlyCard = "premium-plan-yearly";
        public const string LifetimeCard = "premium-plan-lifetime";
        public const string BestValueBadge = "premium-best-value";
        public const string SubscribeMonthly = "premium-subscribe-monthly";
        public const string SubscribeYearly = "premium-subscribe-yearly";
        public const string SubscribeLifetime = "premium-subscribe-lifetime";
        public const string CancelSubscription = "premium-cancel-subscription";
        public const string CheckoutSuccess = "premium-checkout-success";
        public const string CheckoutCancel = "premium-checkout-cancel";
    }

    public static class Admin
    {
        public const string DashboardPage = "admin-dashboard-page";
        public const string DashboardStats = "admin-dashboard-stats";
        public const string TotalUsers = "admin-stat-total-users";
        public const string ActiveToday = "admin-stat-active-today";
        public const string TotalWords = "admin-stat-total-words";
        public const string DailyChallenges = "admin-stat-daily-challenges";
        public const string LinkWords = "admin-link-words";
        public const string LinkUsers = "admin-link-users";
    }

    public static class AdminWords
    {
        public const string Page = "admin-words-page";
        public const string Filters = "admin-word-filters";
        public const string Search = "admin-word-search";
        public const string DifficultyFilter = "admin-word-filter-difficulty";
        public const string CategoryFilter = "admin-word-filter-category";
        public const string MinLengthFilter = "admin-word-filter-min-length";
        public const string MaxLengthFilter = "admin-word-filter-max-length";
        public const string ApplyFilters = "admin-word-apply-filters";
        public const string ResetFilters = "admin-word-reset-filters";
        public const string ColumnPicker = "admin-word-column-picker";
        public const string ColumnPickerPanel = "admin-word-column-picker-panel";
        public const string ColumnToggleCategory = "admin-word-column-toggle-category";
        public const string HeadingCategory = "admin-word-heading-category";
        public const string Table = "admin-word-table";
        public const string Row = "admin-word-row";
        public const string WordCell = "admin-word-cell-word";
        public const string CategoryCell = "admin-word-cell-category";
        public const string Pagination = "admin-word-pagination";
        public const string PageInfo = "admin-word-page-info";
        public const string PaginationNext = "admin-word-pagination-next";
        public const string CreateOpen = "admin-word-create-open";
        public const string Modal = "admin-word-modal";
        public const string FormWord = "admin-word-form-word";
        public const string FormDifficulty = "admin-word-form-difficulty";
        public const string FormCategory = "admin-word-form-category";
        public const string Save = "admin-word-save";
        public const string Edit = "admin-word-edit";
        public const string Delete = "admin-word-delete";
        public const string DeleteModal = "admin-word-delete-modal";
        public const string DeleteConfirm = "admin-word-delete-confirm";
        public const string ImportOpen = "admin-word-import-open";
        public const string ImportModal = "admin-word-import-modal";
        public const string ImportCsv = "admin-word-import-csv";
        public const string ImportSave = "admin-word-import-save";
        public const string ImportCancel = "admin-word-import-cancel";
        public const string ImportResult = "admin-word-import-result";
        public const string Export = "admin-word-export";
        public const string StatsOpen = "admin-word-stats-open";
        public const string StatsDrawer = "admin-word-stats-drawer";
        public const string StatsTotal = "admin-word-stats-total";
        public const string StatsDistribution = "admin-word-stats-distribution";
    }

    public static class AdminUsers
    {
        public const string Page = "admin-users-page";
        public const string Filters = "admin-user-filters";
        public const string Search = "admin-user-search";
        public const string StatusFilter = "admin-user-filter-status";
        public const string PremiumFilter = "admin-user-filter-premium";
        public const string MinLevelFilter = "admin-user-filter-min-level";
        public const string MaxLevelFilter = "admin-user-filter-max-level";
        public const string ApplyFilters = "admin-user-apply-filters";
        public const string ResetFilters = "admin-user-reset-filters";
        public const string Table = "admin-user-table";
        public const string Row = "admin-user-row";
        public const string EmailCell = "admin-user-cell-email";
        public const string LevelCell = "admin-user-cell-level";
        public const string StatusCell = "admin-user-cell-status";
        public const string Detail = "admin-user-detail";
        public const string Suspend = "admin-user-suspend";
        public const string Unsuspend = "admin-user-unsuspend";
        public const string ResetPassword = "admin-user-reset-password";
        public const string Drawer = "admin-user-detail-drawer";
        public const string DrawerEmail = "admin-user-detail-email";
        public const string DrawerLevel = "admin-user-detail-level";
        public const string DrawerXp = "admin-user-detail-xp";
        public const string DrawerStatus = "admin-user-detail-status";
        public const string DrawerSuspend = "admin-user-detail-suspend";
        public const string DrawerUnsuspend = "admin-user-detail-unsuspend";
        public const string DrawerResetPassword = "admin-user-detail-reset-password";
        public const string ResetPasswordResult = "admin-user-reset-password-result";
        public const string PageInfo = "admin-user-page-info";
    }

    public static class AIChallenge
    {
        public const string Page = "ai-challenge-page";
        public const string Loading = "ai-challenge-loading";
        public const string Error = "ai-challenge-error";
        public const string Analysis = "ai-analysis";
        public const string WeakLetters = "ai-weak-letters";
        public const string WeakLettersEmpty = "ai-weak-letters-empty";
        public const string WeakLetter = "ai-weak-letter";
        public const string CategoryPerformance = "ai-category-performance";
        public const string CategoryPerformanceEmpty = "ai-category-performance-empty";
        public const string CategoryRow = "ai-category-row";
        public const string Tips = "ai-tips";
        public const string ChallengeGrid = "ai-challenge-grid";
        public const string ChallengeCard = "ai-challenge-card";
        public const string PreviewWord = "ai-preview-word";
        public const string PreviewReasonToggle = "ai-preview-reason-toggle";
        public const string PreviewReasonTooltip = "ai-preview-reason-tooltip";
        public const string StartWeaknessFocus = "ai-start-weakness-focus";
        public const string StartSpeedTraining = "ai-start-speed-training";
        public const string StartMemoryGame = "ai-start-memory-game";
        public const string StartPatternRecognition = "ai-start-pattern-recognition";
    }

    public static class Profile
    {
        public const string Page = "profile-page";
        public const string Heading = "profile-heading";
        public const string Loading = "profile-loading";
        public const string SummaryCard = "profile-summary-card";
        public const string StatsCard = "profile-stats-card";
        public const string StatLevel = "profile-stat-level";
        public const string StatXp = "profile-stat-xp";
        public const string StatWords = "profile-stat-words";
        public const string StatCurrentStreak = "profile-stat-current-streak";
        public const string StatLongestStreak = "profile-stat-longest-streak";
        public const string StatAccuracy = "profile-stat-accuracy";
        public const string AchievementsCard = "profile-achievements-card";
        public const string PremiumBadge = "profile-premium-badge";
    }

    public static class Shop
    {
        public const string Page = "shop-page";
        public const string CoinBalance = "shop-coin-balance";
        public const string Tabs = "shop-tabs";
        public const string TabAll = "shop-tab-all";
        public const string TabAvatar = "shop-tab-avatar";
        public const string TabFrame = "shop-tab-frame";
        public const string TabTheme = "shop-tab-theme";
        public const string TabBoost = "shop-tab-boost";
        public const string ItemCard = "shop-item-card";
        public const string Buy = "shop-buy";
        public const string Equip = "shop-equip";
        public const string Equipped = "shop-equipped";
        public const string OwnedBadge = "shop-owned-badge";
        public const string PremiumBadge = "shop-premium-badge";
        public const string RarityBadge = "shop-rarity-badge";
    }

    public static class Dictionaries
    {
        public const string Page = "dictionaries-page";
        public const string PremiumGate = "dictionaries-premium-gate";
        public const string CreateButton = "dictionaries-create";
        public const string EmptyState = "dictionaries-empty";
        public const string PublicEmptyState = "dictionaries-public-empty";
        public const string MyTab = "dictionaries-tab-my";
        public const string PublicTab = "dictionaries-tab-public";
        public const string Card = "dictionary-card";
        public const string WordCount = "dictionary-word-count";
        public const string PublicBadge = "dictionary-public-badge";
        public const string Detail = "dictionary-detail";
        public const string AddWord = "dictionary-add-word";
        public const string Import = "dictionary-import";
        public const string Delete = "dictionary-delete";
        public const string ImportResult = "dictionary-import-result";
        public const string CreateDialog = "dictionary-create-dialog";
        public const string NameInput = "dictionary-name-input";
        public const string DescriptionInput = "dictionary-description-input";
        public const string PublicToggle = "dictionary-public-toggle";
        public const string SaveCreate = "dictionary-create-save";
        public const string NameError = "dictionary-name-error";
        public const string AddWordDialog = "dictionary-add-word-dialog";
        public const string WordInput = "dictionary-word-input";
        public const string WordDifficulty = "dictionary-word-difficulty";
        public const string SaveWord = "dictionary-word-save";
        public const string WordError = "dictionary-word-error";
        public const string ImportDialog = "dictionary-import-dialog";
        public const string ImportFile = "dictionary-import-file";
        public const string ImportError = "dictionary-import-error";
        public const string ImportSave = "dictionary-import-save";
        public const string ImportPreview = "dictionary-import-preview";
        public const string DetailDialog = "dictionary-detail-dialog";
        public const string DetailStatus = "dictionary-detail-status";
        public const string DetailWordList = "dictionary-detail-word-list";
        public const string DetailClose = "dictionary-detail-close";
    }

    public static class Multiplayer
    {
        public const string Page = "multiplayer-page";
        public const string QuickMatchCard = "multiplayer-quick-match-card";
        public const string PrivateRoomCard = "multiplayer-private-room-card";
        public const string QuickMatchStart = "multiplayer-quick-match-start";
        public const string CreateRoom = "multiplayer-create-room";
        public const string JoinRoom = "multiplayer-join-room";
        public const string MatchHistory = "multiplayer-match-history";
        public const string QuickMatchPage = "quick-match-page";
        public const string Searching = "quick-match-searching";
        public const string SearchingStatus = "quick-match-searching-status";
        public const string CancelSearch = "quick-match-cancel";
        public const string Timeout = "quick-match-timeout";
        public const string TimeoutTitle = "quick-match-timeout-title";
        public const string TimeoutRetry = "quick-match-timeout-retry";
        public const string TimeoutPlayVsAi = "quick-match-timeout-play-ai";
        public const string TimeoutBack = "quick-match-timeout-back";
        public const string MatchFound = "quick-match-found";
        public const string Countdown = "quick-match-countdown";
        public const string RealtimeGame = "realtime-game";
        public const string RealtimeScrambledWord = "realtime-scrambled-word";
        public const string RealtimeTimer = "realtime-timer";
        public const string RealtimeAnswerInput = "realtime-answer-input";
        public const string RealtimeSubmit = "realtime-submit";
        public const string RealtimeForfeit = "realtime-forfeit";
        public const string RealtimeFeedback = "realtime-feedback";
        public const string RealtimePlayerScore = "realtime-player-score";
        public const string RealtimeOpponentScore = "realtime-opponent-score";
        public const string ResultPage = "multiplayer-result-page";
        public const string ResultTitle = "multiplayer-result-title";
        public const string ResultXp = "multiplayer-result-xp";
        public const string ResultLeagueXp = "multiplayer-result-league-xp";
        public const string ResultNoLeagueInfo = "multiplayer-result-no-league-info";
        public const string ResultSeriesScore = "multiplayer-result-series-score";
        public const string ResultRematchPending = "multiplayer-result-rematch-pending";
        public const string ResultRematchRequest = "multiplayer-result-rematch-request";
        public const string ResultRematchAccept = "multiplayer-result-rematch-accept";
        public const string ResultRematchDecline = "multiplayer-result-rematch-decline";
        public const string ResultRematchDeclined = "multiplayer-result-rematch-declined";
        public const string ResultNext = "multiplayer-result-next";
        public const string ResultHome = "multiplayer-result-home";
        public const string HistoryPage = "multiplayer-history-page";
        public const string HistoryStatsPlayed = "multiplayer-history-stats-played";
        public const string HistoryStatsWins = "multiplayer-history-stats-wins";
        public const string HistoryMatchRow = "multiplayer-history-match-row";
        public const string PrivateCreateModal = "private-room-create-modal";
        public const string PrivateWordCount10 = "private-room-word-count-10";
        public const string PrivateWordCount15 = "private-room-word-count-15";
        public const string PrivateWordCount20 = "private-room-word-count-20";
        public const string PrivateTimeLimit2 = "private-room-time-limit-2";
        public const string PrivateTimeLimit3 = "private-room-time-limit-3";
        public const string PrivateTimeLimit5 = "private-room-time-limit-5";
        public const string PrivateDifficulty = "private-room-difficulty";
        public const string PrivateBestOf1 = "private-room-best-of-1";
        public const string PrivateBestOf3 = "private-room-best-of-3";
        public const string PrivateBestOf5 = "private-room-best-of-5";
        public const string PrivateCreateSubmit = "private-room-create-submit";
        public const string PrivateJoinModal = "private-room-join-modal";
        public const string PrivateJoinInput = "private-room-join-input";
        public const string PrivateJoinSubmit = "private-room-join-submit";
        public const string PrivateJoinValidation = "private-room-join-validation";
        public const string PrivateRoomError = "private-room-error";
        public const string PrivateLobby = "private-room-lobby";
        public const string PrivateRoomCode = "private-room-code";
        public const string PrivateCopyCode = "private-room-copy-code";
        public const string PrivatePlayer = "private-room-player";
        public const string PrivatePlayerName = "private-room-player-name";
        public const string PrivateLeave = "private-room-leave";
        public const string PrivateReady = "private-room-ready";
        public const string PrivateCancelReady = "private-room-cancel-ready";
        public const string PrivateCountdown = "private-room-countdown";
        public const string PrivateChatSection = "private-room-chat-section";
        public const string PrivateChatInput = "private-room-chat-input";
        public const string PrivateChatSend = "private-room-chat-send";
        public const string PrivateChatError = "private-room-chat-error";
        public const string PrivateChatMessage = "private-room-chat-message";
        public const string PrivateChatMessageText = "private-room-chat-message-text";
        public const string PrivateSettingsWordCount = "private-room-settings-word-count";
        public const string PrivateSettingsTimeLimit = "private-room-settings-time-limit";
        public const string PrivateSettingsDifficulty = "private-room-settings-difficulty";
        public const string PrivateSettingsBestOf = "private-room-settings-best-of";
    }

    public static class Teams
    {
        public const string Page = "team-page";
        public const string Loading = "team-loading";
        public const string EmptyState = "team-empty-state";
        public const string EmptyTitle = "team-empty-title";
        public const string EmptyDescription = "team-empty-description";
        public const string CreateTeam = "team-create";
        public const string SearchTeam = "team-search";
        public const string CreateModal = "team-create-modal";
        public const string CreateName = "team-create-name";
        public const string CreateNameError = "team-create-name-error";
        public const string CreateTag = "team-create-tag";
        public const string CreateTagError = "team-create-tag-error";
        public const string CreateDescription = "team-create-description";
        public const string CreateDescriptionError = "team-create-description-error";
        public const string CreateCost = "team-create-cost";
        public const string CreateSubmit = "team-create-submit";
        public const string CreateCancel = "team-create-cancel";
        public const string CreateError = "team-create-error";
        public const string InvitesSection = "team-invites-section";
        public const string InviteRow = "team-invite-row";
        public const string InviteAccept = "team-invite-accept";
        public const string InviteReject = "team-invite-reject";
        public const string Dashboard = "team-dashboard";
        public const string DashboardName = "team-dashboard-name";
        public const string DashboardTag = "team-dashboard-tag";
        public const string DashboardDescription = "team-dashboard-description";
        public const string StatsWeeklyXp = "team-stats-weekly-xp";
        public const string StatsAllTimeXp = "team-stats-alltime-xp";
        public const string StatsRank = "team-stats-rank";
        public const string StatsWins = "team-stats-wins";
        public const string InviteOpen = "team-invite-open";
        public const string InviteModal = "team-invite-modal";
        public const string InviteUsername = "team-invite-username";
        public const string InviteUsernameError = "team-invite-username-error";
        public const string InviteSubmit = "team-invite-submit";
        public const string InviteCancel = "team-invite-cancel";
        public const string InviteError = "team-invite-error";
        public const string InviteSuccess = "team-invite-success";
        public const string MembersTable = "team-members-table";
        public const string MembersHeading = "team-members-heading";
        public const string MemberRow = "team-member-row";
        public const string MemberKick = "team-member-kick";
        public const string LeaveTeam = "team-leave";
        public const string DisbandTeam = "team-disband";
        public const string TransferOpen = "team-transfer-open";
        public const string TransferModal = "team-transfer-modal";
        public const string TransferMember = "team-transfer-member";
        public const string TransferSubmit = "team-transfer-submit";
        public const string TransferCancel = "team-transfer-cancel";
        public const string TransferError = "team-transfer-error";
        public const string RankingTable = "team-ranking-table";
        public const string RankingRow = "team-ranking-row";
        public const string JoinRequestOpen = "team-join-request-open";
        public const string JoinModal = "team-join-modal";
        public const string JoinMessage = "team-join-message";
        public const string JoinSubmit = "team-join-submit";
        public const string JoinCancel = "team-join-cancel";
        public const string JoinError = "team-join-error";
        public const string JoinSuccess = "team-join-success";
        public const string JoinRequestsSection = "team-join-requests-section";
        public const string JoinRequestRow = "team-join-request-row";
        public const string JoinRequestAccept = "team-join-request-accept";
        public const string JoinRequestReject = "team-join-request-reject";
    }

    public static class Guest
    {
        public const string Page = "guest-game-page";
        public const string Welcome = "guest-welcome";
        public const string StartButton = "btn-start-guest";
        public const string Arena = "game-arena";
        public const string RemainingGames = "remaining-games";
        public const string ScrambledWord = "guest-scrambled-word";
        public const string AnswerInput = "answer-input";
        public const string Submit = "btn-submit";
        public const string Feedback = "answer-feedback";
        public const string CtaModal = "guest-cta-modal";
        public const string ConvertModal = "guest-convert-modal";
        public const string ModalOverlay = "guest-modal-overlay";
        public const string ProgressBanner = "guest-progress-banner";
        public const string LimitReached = "guest-limit-reached";
    }

    public static class Landing
    {
        public const string Page = "landing-page";
        public const string Hero = "hero-section";
        public const string HowItWorks = "how-it-works-section";
        public const string Features = "features-section";
        public const string FeatureTabRpg = "tab-panel-rpg";
        public const string FeatureTabBattles = "tab-panel-battles";
        public const string FeatureTabCompetitions = "tab-panel-competitions";
        public const string Paths = "paths-section";
        public const string Testimonials = "testimonials-section";
        public const string Cta = "cta-section";
        public const string Footer = "footer";
        public const string RegisterCta = "hero-cta-register";
        public const string GuestCta = "hero-cta-guest";
    }

    public static class Pwa
    {
        public const string InstallPrompt = "install-prompt";
        public const string InstallPromptInstall = "install-prompt-install";
        public const string InstallPromptDismiss = "install-prompt-dismiss";
        public const string OfflineBanner = "offline-banner";
    }

    public static class Errors
    {
        public const string NotFoundPage = "not-found-page";
    }
}
