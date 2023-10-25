using MightyMatsData.Repository;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        _productRepository = new ProductRepository(_db);
        _shoppingCartItemRepository = new ShoppingCartItemRepository(_db);
        // _orderHeaderRepository = new OrderHeaderRepository(_db);
        _orderDetailRepository = new OrderDetailRepository(_db);
        _identityUserRepository = new IdentityUserRepository(_db);
    }

    public IProductRepository _productRepository { get; set; }
    public IShoppingCartItemRepository _shoppingCartItemRepository { get; set; }
    public IOrderDetailRepository _orderDetailRepository { get; set; }
    public IOrderHeaderRepository _orderHeaderRepository { get; set; }
    public IIdentityUserRepository _identityUserRepository { get; set; }
}