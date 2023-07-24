using MightyMatsData.Models;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.Repository;

public sealed class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(ApplicationDbContext db) : base(db)
    {
    }
}