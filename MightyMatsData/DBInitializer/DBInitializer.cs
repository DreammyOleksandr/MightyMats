using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MightyMatsData.DBInitializer;

public class DBInitializer : IDBInitializer
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _db;

    public DBInitializer(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager,
        ApplicationDbContext db)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public void Initialize()
    {
        try
        {
            if (_db.Database.GetPendingMigrations().Count() > 0)
            {
                _db.Database.Migrate();
            }
        }
        catch (Exception e)
        {
            // ignored
        }

        if (!_roleManager.RoleExistsAsync(StaticDetails.CustomerRole).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.CustomerRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(StaticDetails.AdminRole)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new IdentityUser
            {
                UserName = "mightymatsadmin@gmail.com",
                Email = "mightymatsadmin@gmail.com",
            }, password: "RxcL2rGtcpym!").GetAwaiter().GetResult();

            IdentityUser user = _db.Users.FirstOrDefault(_ => _.Email == "mightymatsadmin@gmail.com");
            if (user is not null) _userManager.AddToRoleAsync(user, StaticDetails.AdminRole).GetAwaiter().GetResult();
        }

        return;
    }
}