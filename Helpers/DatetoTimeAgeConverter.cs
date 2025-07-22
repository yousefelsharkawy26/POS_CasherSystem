using System.Globalization;
using System.Windows.Data;

namespace POS_ModernUI.Helpers;
public class DatetoTimeAgeConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
            return GetTimeAgo(dateTime);

        if (value is DateOnly dateOnly)
            return DateOnlyToTimeAge(dateOnly.ToDateTime(new TimeOnly(0, 0)));

        return "غير معروف";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "منذ ثوانٍ";
        if (timeSpan.TotalMinutes < 2)
            return "منذ دقيقة";
        if (timeSpan.TotalMinutes < 60)
            return $"منذ {(int)timeSpan.TotalMinutes} دقائق";
        if (timeSpan.TotalHours < 2)
            return "منذ ساعة";
        if (timeSpan.TotalHours < 24)
            return $"منذ {(int)timeSpan.TotalHours} ساعات";
        if (timeSpan.TotalDays < 2)
            return "أمس";
        if (timeSpan.TotalDays < 7)
            return $"منذ {(int)timeSpan.TotalDays} أيام";
        if (timeSpan.TotalDays < 30)
            return $"منذ {(int)(timeSpan.TotalDays / 7)} أسبوع";

        return dateTime.ToString("dd MMM yyyy");
    }

    public static string DateOnlyToTimeAge(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalHours < 24)
            return $"اليوم";
        if (timeSpan.TotalDays < 2)
            return "أمس";
        if (timeSpan.TotalDays < 7)
            return $"منذ {(int)timeSpan.TotalDays} أيام";
        if (timeSpan.TotalDays < 30)
            return $"منذ {(int)(timeSpan.TotalDays / 7)} أسبوع";

        return dateTime.ToString("dd MMM yyyy");
    }
}
