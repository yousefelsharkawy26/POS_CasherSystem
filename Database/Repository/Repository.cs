using System.Linq.Expressions;
using POS_ModernUI.Database.Context;
using Microsoft.EntityFrameworkCore;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Helpers;

namespace POS_ModernUI.Database.Repository;
public class Repository<T> : IRepository<T> where T : class
{
    AppDbContext _context;
    public Repository(AppDbContext context)
    {
        _context = context;
    }
    public void Add(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
        
        try { _context.Set<T>().Add(entity); }

        catch (Exception ex)
        {
            var msg = new Wpf.Ui.Controls.MessageBox();

            msg.ShowMessage(ex.Message, "تحذير");
        }
    }

    public void Delete(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
        _context.ChangeTracker.Clear();
        try { _context.Set<T>().Remove(entity); }

        catch (Exception ex)
        {
            var msg = new Wpf.Ui.Controls.MessageBox();

            msg.ShowMessage(ex.Message, "تحذير");
        }
    }

    public T Get(Expression<Func<T, bool>> filter, string? includeProp = null)
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        if (includeProp != null)
        {
            foreach (var prop in includeProp
                .Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }
        }
        var entity = query.FirstOrDefault(filter)!;

        return entity;
    }

    public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProp = null)
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        if (filter != null)
            query = query.Where(filter);

        if (includeProp != null)
        {
            foreach (var prop in includeProp
                .Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }
        }

        return query;
    }

    public void Update(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
        
        _context.ChangeTracker.Clear();
        try { _context.Set<T>().Update(entity); }

        catch (Exception ex)
        {
            var msg = new Wpf.Ui.Controls.MessageBox();

            msg.ShowMessage(ex.Message, "تحذير");
        }
    }
}
