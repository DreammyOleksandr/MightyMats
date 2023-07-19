using MightyMatsData.Models;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.Repository;

public class ShoppingCartItemRepository : Repository<ShoppingCartItem>, IShoppingCartItemRepository 
{
    public ShoppingCartItemRepository(ApplicationDbContext db) : base(db)
    {
    }
}