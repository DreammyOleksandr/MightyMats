using MightyMatsData.Repository;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public IProductRepository _productRepository { get; set; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        _productRepository = new ProductRepository(_db);
    }
}