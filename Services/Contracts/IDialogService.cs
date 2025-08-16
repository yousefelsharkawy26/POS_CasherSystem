using System.Windows.Controls;

namespace POS_ModernUI.Services.Contracts;
public interface IDialogService
{
    Task<bool?> ShowDialogAsync(Wpf.Ui.Controls.ContentDialog dialog);
    void SetContentPresenter(ContentPresenter contentPresenter);
}
