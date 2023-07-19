using MightyMatsData.Repository;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public IProductRepository _productRepository { get; set; }
    public IShoppingCartItemRepository _shoppingCartItemRepository { get; set; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        _productRepository = new ProductRepository(_db);
        _shoppingCartItemRepository = new ShoppingCartItemRepository(_db);
    }
}