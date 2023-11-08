using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MightyMatsData.DBInitializer;

public class DBInitializer : IDBInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DBInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public void Initialize()
    {
        try
        {
            if (_db.Database.GetPendingMigrations().Count() > 0) _db.Database.Migrate();
        }
        catch (Exception e)
        {
            // ignored
        }

        if (!_roleManager.RoleExistsAsync(StaticDetails.CustomerRole).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.CustomerRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.AdminRole)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new IdentityUser()
            {
                UserName = "mightymatsadmin@gmail.com", Email = "mightymatsadmin@gmail.com", EmailConfirmed = true
            }, "Admin123!").GetAwaiter().GetResult();

            IdentityUser user = _db.Users.FirstOrDefaultAsync(_ => _.Email == "mightymatsadmin@gmail.com").GetAwaiter()
                .GetResult();
            _userManager.AddToRoleAsync(user, StaticDetails.AdminRole).GetAwaiter().GetResult();
        }
    }
}