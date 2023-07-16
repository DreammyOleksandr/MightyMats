using MightyMatsData.Models;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext db) : base(db)
    {
    }
}