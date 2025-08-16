using POS_ModernUI.Services.Contracts;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace POS_ModernUI.Services;
public class DialogService : IDialogService
{
    ContentPresenter _contentPresenter;

    public void SetContentPresenter(ContentPresenter contentPresenter)
    {
        _contentPresenter = contentPresenter;
    }

    public async Task<bool?> ShowDialogAsync(ContentDialog dialog)
    {
        dialog.DialogHost = _contentPresenter;

        // هنا بنستخدم Dialog.ShowAsync الخاص بـ WPF UI
        return await dialog.ShowAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary;
    }
}

