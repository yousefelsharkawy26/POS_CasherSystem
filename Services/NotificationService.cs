using POS_ModernUI.Database.Repository.IRepository;
using POS_ModernUI.Models;
using POS_ModernUI.Services.Contracts;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace POS_ModernUI.Services;
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    public ObservableCollection<Notification> Notifications { get; private set; }

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        Notifications = new(_unitOfWork.Notifications.GetAll().OrderByDescending(u=> u.Timestamp));
    }

    public void SetNotification(Notification notification)
    {
        _unitOfWork.Notifications.Add(notification);
        _unitOfWork.Save(notification, Microsoft.EntityFrameworkCore.EntityState.Added);

        // Refresh the Notifications collection
        Notifications = new(_unitOfWork.Notifications.GetAll().OrderByDescending(u=> u.Timestamp));
    }

    public void SetRead(Notification notification)
    {
        notification.IsRead = true;

        _unitOfWork.Notifications.Update(notification);
        _unitOfWork.Save(notification, Microsoft.EntityFrameworkCore.EntityState.Modified);
        Notifications = new(_unitOfWork.Notifications.GetAll().OrderByDescending(u => u.Timestamp));
    }

    public void SetReadAll()
    {
        _unitOfWork.Notifications.GetAll(u => u.IsRead == false)
                                 .ToList()
                                 .ForEach(notification =>
        {
            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
        });
        _unitOfWork.Save();
        Notifications = new(_unitOfWork.Notifications.GetAll().OrderByDescending(u => u.Timestamp));
    }

    public void RemoveNotification(Notification notification)
    {
        _unitOfWork.Notifications.Delete(notification);
        _unitOfWork.Save(notification, Microsoft.EntityFrameworkCore.EntityState.Deleted);
        Notifications = new(_unitOfWork.Notifications.GetAll().OrderByDescending(u => u.Timestamp));
    }
}
    