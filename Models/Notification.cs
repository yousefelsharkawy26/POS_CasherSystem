using System.ComponentModel.DataAnnotations.Schema;

namespace POS_ModernUI.Models;
public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.Now; // Automatically set to current time
    public string? Payload { get; set; } // Optional payload for additional data
    public Notification(string title, string message, string? payload = null)
    {
        Title = title;
        Message = message;
        Timestamp = DateTime.Now;
        Payload = payload;
    }

}
