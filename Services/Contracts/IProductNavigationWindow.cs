using POS_ModernUI.Models;
using Wpf.Ui;

namespace POS_ModernUI.Services.Contracts;
public interface IProductNavigationWindow: INavigationWindow
{
    void ShowWindow(Product product);
}
