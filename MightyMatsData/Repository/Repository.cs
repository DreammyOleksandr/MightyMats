using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MightyMatsData.Repository.IRepository;

namespace MightyMatsData.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        dbSet = _db.Set<T>();
    }

    public async Task<List<T>> GetAll(Expression<Func<T, bool>>? filter)
    {
        IQueryable<T> query = dbSet;
        
        if (filter != null)
            query = query.Where(filter);

        return await query.ToListAsync();
    }

    public async Task<T> Get(Expression<Func<T, bool>> filter)
    {
        IQueryable<T> query = dbSet;
        return await query.FirstOrDefaultAsync();
    }

    public async Task Add(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
         dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        dbSet.RemoveRange(entities);
    }
    
    public async Task Save()
    {
        await _db.SaveChangesAsync();
    }
}