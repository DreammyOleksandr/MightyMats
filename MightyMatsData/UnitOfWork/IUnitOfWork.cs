using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.UnitOfWork;

public interface IUnitOfWork
{
    IProductRepository _productRepository { get; set; }
    IShoppingCartItemRepository _shoppingCartItemRepository { get; set; }
}