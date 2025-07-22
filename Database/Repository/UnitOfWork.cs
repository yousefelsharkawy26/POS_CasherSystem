using Microsoft.EntityFrameworkCore;
using POS_ModernUI.Database.Context;
using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Models;

namespace POS_ModernUI.Database.Repository;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IRepository<Notification> _notifications;
    private readonly IRepository<Product> _products;
    private readonly IRepository<PurchaseOrder> _purchaseOrders;
    private readonly IRepository<Supplier> _suppliers;
    private readonly IRepository<PurchaseOrderDetail> _purchaseOrderDetails;
    private readonly IRepository<SalesOrder> _salesOrders;
    private readonly IRepository<SalesOrderDetail> _salesOrderDetails;
    private readonly IRepository<User> _users;
    private readonly IRepository<ImageCleaner> _imagePaths;
    public UnitOfWork()
    {
        _context = new();
        _notifications = new Repository<Notification>(_context);
        _products = new Repository<Product>(_context);
        _purchaseOrders = new Repository<PurchaseOrder>(_context);
        _suppliers = new Repository<Supplier>(_context);
        _purchaseOrderDetails = new Repository<PurchaseOrderDetail>(_context);
        _salesOrders = new Repository<SalesOrder>(_context);
        _salesOrderDetails = new Repository<SalesOrderDetail>(_context);
        _users = new Repository<User>(_context);
        _imagePaths = new Repository<ImageCleaner>(_context);
    }

    public IRepository<Notification> Notifications { get => _notifications; }
    public IRepository<Product> Products { get => _products; }
    public IRepository<PurchaseOrder> PurchaseOrders { get => _purchaseOrders; }
    public IRepository<Supplier> Suppliers { get => _suppliers; }
    public IRepository<PurchaseOrderDetail> PurchaseOrderDetails { get => _purchaseOrderDetails; }
    public IRepository<SalesOrder> SalesOrders { get => _salesOrders; }
    public IRepository<SalesOrderDetail> SalesOrderDetails { get => _salesOrderDetails; }
    public IRepository<User> Users { get => _users; }
    public IRepository<ImageCleaner> ImagePaths { get => _imagePaths; }

    public void Save()
    {
        _context.SaveChanges();
    }

    public void Save<T>(T entity, EntityState state) where T : class
    {
        _context.Entry(entity).State = state;

        _context.SaveChanges();
    }
}
