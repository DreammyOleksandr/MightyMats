using Microsoft.EntityFrameworkCore;
using MightyMatsData.Models;

namespace MightyMatsData;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
    }

    public DbSet<Product> Products { get; set; }
}