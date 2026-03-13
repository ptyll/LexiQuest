using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

public class InventoryService : IInventoryService
{
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
        return await _inventoryRepository.GetByUserIdAsync(userId);
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

        var inventoryItem = UserInventoryItem.Create(userId, shopItemId);
        await _inventoryRepository.AddAsync(inventoryItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PurchaseResult(true, "Položka byla úspěšně zakoupena!", inventoryItem.Id);
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

        item.Equip();
        _inventoryRepository.Update(item);
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
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SpendCoinsAsync(Guid userId, int amount, string reason, CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        if (user.CoinBalance < amount) return false;

        user.AddCoinTransaction(-amount, "Debit", reason);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
