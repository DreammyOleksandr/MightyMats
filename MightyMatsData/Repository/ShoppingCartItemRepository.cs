using MightyMatsData.Models;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.Repository;

public sealed class ShoppingCartItemRepository : Repository<ShoppingCartItem>, IShoppingCartItemRepository
{
    public ShoppingCartItemRepository(ApplicationDbContext db) : base(db)
    {
    }
}