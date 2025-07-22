using POS_ModernUI.Models;
using System.Collections.ObjectModel;

namespace POS_ModernUI.Services.Contracts;
public interface INotificationService
{
    ObservableCollection<Notification> Notifications { get; }
    void SetNotification(Notification notification);
    void SetRead(Notification notification);
    void SetReadAll();
    void RemoveNotification(Notification notification);

}
