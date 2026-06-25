using System.Collections.Concurrent;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class InventoryService : IInventoryService
{
    private const string DiamondAvatarAssetUrl = "/assets/shop/avatar-diamond.svg";
    private const string OwlAvatarAssetUrl = "/assets/shop/avatar-owl.svg";

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> PurchaseLocks = new();
    private static readonly IReadOnlyDictionary<string, string> AvatarAssetsByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Diamantový avatar"] = DiamondAvatarAssetUrl,
        ["Sova učence"] = OwlAvatarAssetUrl
    };

    private readonly IShopItemRepository _shopItemRepository;
    private readonly IUserInventoryRepository _inventoryRepository;
    private readonly IPremiumFeatureService _premiumFeatureService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IShopItemRepository shopItemRepository,
        IUserInventoryRepository inventoryRepository,
        IPremiumFeatureService premiumFeatureService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _shopItemRepository = shopItemRepository;
        _inventoryRepository = inventoryRepository;
        _premiumFeatureService = premiumFeatureService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ShopItem>> GetShopItemsAsync(ShopCategory? category = null, CancellationToken cancellationToken = default)
    {
        if (category.HasValue)
        {
            return await _shopItemRepository.GetByCategoryAsync(category.Value);
        }
        return await _shopItemRepository.GetAllAsync();
    }

    public async Task<ShopItem?> GetShopItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _shopItemRepository.GetByIdAsync(itemId);
    }

    public async Task<IEnumerable<UserInventoryItem>> GetUserInventoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var inventoryItems = (await _inventoryRepository.GetByUserIdAsync(userId)).ToList();
        await RepairEquippedItemEffectsAsync(userId, inventoryItems, cancellationToken);
        return inventoryItems;
    }

    public async Task<bool> HasItemAsync(Guid userId, Guid shopItemId, CancellationToken cancellationToken = default)
    {
        return await _inventoryRepository.HasItemAsync(userId, shopItemId);
    }

    public async Task<bool> IsPremiumOnlyAsync(Guid shopItemId, CancellationToken cancellationToken = default)
    {
        var item = await _shopItemRepository.GetByIdAsync(shopItemId);
        return item?.IsPremiumOnly ?? false;
    }

    public async Task<PurchaseResult> PurchaseItemAsync(Guid userId, Guid shopItemId, CancellationToken cancellationToken = default)
    {
        var lockKey = $"{userId:N}:{shopItemId:N}";
        var purchaseLock = PurchaseLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await purchaseLock.WaitAsync(cancellationToken);

        try
        {
            var shopItem = await _shopItemRepository.GetByIdAsync(shopItemId);
            if (shopItem == null)
            {
                return new PurchaseResult(false, "Položka nebyla nalezena.");
            }

            if (!shopItem.IsAvailable())
            {
                return new PurchaseResult(false, "Tato položka již není dostupná.");
            }

            if (await _inventoryRepository.HasItemAsync(userId, shopItemId))
            {
                return new PurchaseResult(false, "Tuto položku již vlastníte.");
            }

            if (shopItem.IsPremiumOnly)
            {
                var isPremium = await _premiumFeatureService.IsPremiumAsync(userId);
                if (!isPremium)
                {
                    return new PurchaseResult(false, "Tato položka je dostupná pouze pro Premium uživatele.");
                }
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new PurchaseResult(false, "Uživatel nebyl nalezen.");
            }

            if (user.CoinBalance < shopItem.Price)
            {
                return new PurchaseResult(false, "Nedostatek mincí.");
            }

            if (shopItem.Price > 0)
            {
                user.AddCoinTransaction(-shopItem.Price, CoinTransactionType.ShopPurchase.ToString(), $"Nákup položky {shopItem.Name}");
            }

            var inventoryItem = UserInventoryItem.Create(userId, shopItemId);
            await _inventoryRepository.AddAsync(inventoryItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PurchaseResult(true, "Položka byla úspěšně zakoupena!", inventoryItem.Id);
        }
        finally
        {
            purchaseLock.Release();
        }
    }

    public async Task<EquipResult> EquipItemAsync(Guid userId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var item = await _inventoryRepository.GetByIdAsync(inventoryItemId);
        if (item == null)
        {
            return new EquipResult(false, "Položka nebyla nalezena.", false);
        }

        if (item.UserId != userId)
        {
            return new EquipResult(false, "Nemáte oprávnění k této položce.", false);
        }

        var equippedItems = await _inventoryRepository.GetEquippedByUserIdAsync(userId);
        var itemDefinition = await _shopItemRepository.GetByIdAsync(item.ShopItemId);
        foreach (var equippedItem in equippedItems.Where(e => e.Id != item.Id))
        {
            var equippedDefinition = await _shopItemRepository.GetByIdAsync(equippedItem.ShopItemId);
            if (itemDefinition != null && equippedDefinition?.Category == itemDefinition.Category)
            {
                equippedItem.Unequip();
                _inventoryRepository.Update(equippedItem);
            }
        }

        item.Equip();
        _inventoryRepository.Update(item);
        if (itemDefinition != null)
        {
            await ApplyEquippedItemEffectAsync(userId, itemDefinition, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EquipResult(true, "Položka byla nasazena.", true);
    }

    public async Task<EquipResult> UnequipItemAsync(Guid userId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var item = await _inventoryRepository.GetByIdAsync(inventoryItemId);
        if (item == null)
        {
            return new EquipResult(false, "Položka nebyla nalezena.", false);
        }

        if (item.UserId != userId)
        {
            return new EquipResult(false, "Nemáte oprávnění k této položce.", false);
        }

        item.Unequip();
        _inventoryRepository.Update(item);

        var itemDefinition = await _shopItemRepository.GetByIdAsync(item.ShopItemId);
        if (itemDefinition != null)
        {
            await RemoveEquippedItemEffectAsync(userId, itemDefinition, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EquipResult(true, "Položka byla sundána.", false);
    }

    public async Task<int> GetCoinBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user?.CoinBalance ?? 0;
    }

    public async Task AddCoinsAsync(Guid userId, int amount, string reason, CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return;

        user.AddCoinTransaction(amount, "Credit", reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SpendCoinsAsync(Guid userId, int amount, string reason, CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        if (user.CoinBalance < amount) return false;

        user.AddCoinTransaction(-amount, "Debit", reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ApplyEquippedItemEffectAsync(Guid userId, ShopItem itemDefinition, CancellationToken cancellationToken)
    {
        if (itemDefinition.Category is not (ShopCategory.Avatar or ShopCategory.Theme))
        {
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return;
        }

        if (ApplyEquippedItemEffect(user, itemDefinition))
        {
            _userRepository.Update(user);
        }
    }

    private async Task RemoveEquippedItemEffectAsync(Guid userId, ShopItem itemDefinition, CancellationToken cancellationToken)
    {
        if (itemDefinition.Category is not (ShopCategory.Avatar or ShopCategory.Theme))
        {
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return;
        }

        if (RemoveEquippedItemEffect(user, itemDefinition))
        {
            _userRepository.Update(user);
        }
    }

    private async Task RepairEquippedItemEffectsAsync(Guid userId, IReadOnlyCollection<UserInventoryItem> inventoryItems, CancellationToken cancellationToken)
    {
        var equippedItems = inventoryItems.Where(item => item.IsEquipped).ToList();
        if (equippedItems.Count == 0)
        {
            return;
        }

        User? user = null;
        var userUpdated = false;

        foreach (var equippedItem in equippedItems)
        {
            var itemDefinition = await _shopItemRepository.GetByIdAsync(equippedItem.ShopItemId);
            if (itemDefinition?.Category is not (ShopCategory.Avatar or ShopCategory.Theme))
            {
                continue;
            }

            user ??= await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return;
            }

            userUpdated = ApplyEquippedItemEffect(user, itemDefinition) || userUpdated;
        }

        if (userUpdated && user != null)
        {
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool ApplyEquippedItemEffect(User user, ShopItem itemDefinition)
    {
        switch (itemDefinition.Category)
        {
            case ShopCategory.Avatar:
                var avatarUrl = ResolveAvatarUrl(itemDefinition);
                if (user.AvatarUrl == avatarUrl)
                {
                    return false;
                }

                user.UpdateAvatar(avatarUrl);
                return true;
            case ShopCategory.Theme:
                var theme = ResolveTheme(itemDefinition);
                if (user.Preferences?.Theme == theme)
                {
                    return false;
                }

                user.UpdatePreferences(ApplyTheme(user.Preferences, itemDefinition));
                return true;
            default:
                return false;
        }
    }

    private static bool RemoveEquippedItemEffect(User user, ShopItem itemDefinition)
    {
        switch (itemDefinition.Category)
        {
            case ShopCategory.Avatar when user.AvatarUrl == ResolveAvatarUrl(itemDefinition):
                user.ClearAvatar();
                return true;
            case ShopCategory.Theme when ResolveTheme(itemDefinition) == AppTheme.Dark && user.Preferences.Theme == AppTheme.Dark:
                var preferences = user.Preferences ?? UserPreferences.CreateDefault();
                preferences.Theme = AppTheme.Light;
                user.UpdatePreferences(preferences);
                return true;
            default:
                return false;
        }
    }

    private static UserPreferences ApplyTheme(UserPreferences? preferences, ShopItem itemDefinition)
    {
        preferences ??= UserPreferences.CreateDefault();
        preferences.Theme = ResolveTheme(itemDefinition);
        return preferences;
    }

    private static AppTheme ResolveTheme(ShopItem itemDefinition)
    {
        return itemDefinition.Name.Contains("Noční", StringComparison.OrdinalIgnoreCase)
            ? AppTheme.Dark
            : AppTheme.Light;
    }

    private static string ResolveAvatarUrl(ShopItem itemDefinition)
    {
        if (!IsLegacyPlaceholderImage(itemDefinition.ImageUrl))
        {
            return itemDefinition.ImageUrl;
        }

        return AvatarAssetsByName.TryGetValue(itemDefinition.Name, out var assetUrl)
            ? assetUrl
            : OwlAvatarAssetUrl;
    }

    private static bool IsLegacyPlaceholderImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return true;
        }

        var normalized = imageUrl.Trim();
        return normalized.Equals("/icon-192.png", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("icon-192.png", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("/icon-512.png", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("icon-512.png", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("/favicon.png", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("blazor", StringComparison.OrdinalIgnoreCase);
    }
}
