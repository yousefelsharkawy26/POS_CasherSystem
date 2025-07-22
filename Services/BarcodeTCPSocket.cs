using POS_ModernUI.Helpers;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace POS_ModernUI.Services;
public class BarcodeTCPSocket: WebSocketBehavior
{
    protected override async void OnMessage(MessageEventArgs e)
    {
        await Task.Run(() =>
        {
            MessageBox.Show(e.Data);
        });
    }
}
