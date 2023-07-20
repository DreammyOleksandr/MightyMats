using Microsoft.AspNetCore.Identity;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.Repository;

public class IdentityUserRepository : Repository<IdentityUser>, IIdentityUserRepository
{
    public IdentityUserRepository(ApplicationDbContext db) : base(db)
    {
    }
}