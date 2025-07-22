using Microsoft.EntityFrameworkCore;
using POS_ModernUI.Models;

namespace POS_ModernUI.Database.Repository.IRepository;
public interface IUnitOfWork
{
    IRepository<Notification> Notifications { get; }
    IRepository<Product>  Products { get; } 
    IRepository<PurchaseOrder> PurchaseOrders { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<PurchaseOrderDetail> PurchaseOrderDetails { get; }
    IRepository<SalesOrder> SalesOrders { get; }
    IRepository<SalesOrderDetail> SalesOrderDetails { get; }
    IRepository<User> Users { get; }
    IRepository<ImageCleaner> ImagePaths { get; }
    void Save();

    void Save<T>(T entity, EntityState state) where T : class;
}
