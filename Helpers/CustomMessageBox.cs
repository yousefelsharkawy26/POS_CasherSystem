using System.Threading.Tasks;

namespace POS_ModernUI.Helpers
{
    public static class CustomMessageBox
    {
        public static Wpf.Ui.Controls.MessageBoxResult ShowMessage(this Wpf.Ui.Controls.MessageBox msg,
                                                            string message,
                                                            string title = "Information",
                                                            MessageBoxButton buttons = MessageBoxButton.OK)
        {
            msg.Title = title;
            msg.Content = message;

            if (buttons == MessageBoxButton.OK)
            {
                msg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
                msg.CloseButtonText = "موافق";
                msg.ShowDialogAsync();
                return Wpf.Ui.Controls.MessageBoxResult.Primary;
            }
            else if (buttons == MessageBoxButton.YesNo)
            {
                msg.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
                msg.PrimaryButtonText = "نعم";
                msg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger;
                msg.CloseButtonText = "لا";
            }
            else if (buttons == MessageBoxButton.OKCancel)
            {
                msg.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
                msg.PrimaryButtonText = "موافق";
                msg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger;
                msg.CloseButtonText = "إلغاء";
            }

            msg.FlowDirection = FlowDirection.RightToLeft;

            return msg.ShowDialogAsync().Result;
        }

        public static async Task<Wpf.Ui.Controls.MessageBoxResult> ShowMessageAsync(this Wpf.Ui.Controls.MessageBox msg,
                                                            string message,
                                                            string title = "Information",
                                                            MessageBoxButton buttons = MessageBoxButton.OK)
        {
            msg.Title = title;
            msg.Content = message;

            if (buttons == MessageBoxButton.OK)
            {
                msg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
                msg.CloseButtonText = "موافق";
                await msg.ShowDialogAsync();
                return Wpf.Ui.Controls.MessageBoxResult.Primary;
            }
            else if (buttons == MessageBoxButton.YesNo)
            {
                msg.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
                msg.PrimaryButtonText = "نعم";
                msg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger;
                msg.CloseButtonText = "لا";
            }
            else if (buttons == MessageBoxButton.OKCancel)
            {
                msg.PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Primary;
                msg.PrimaryButtonText = "موافق";
                msg.CloseButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger;
                msg.CloseButtonText = "إلغاء";
            }

            msg.FlowDirection = FlowDirection.RightToLeft;

            return await msg.ShowDialogAsync();
        }
    }
}
