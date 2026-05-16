using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using EasySave.Models;

namespace EasySave.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is JobStatus status)
            {
                return status switch
                {
                    JobStatus.Active => Brush.Parse("#2563EB"),             // Blue
                    //JobStatus.Paused => Brush.Parse("#F97316"),             // Orange
                    //JobStatus.WaitingForPriority => Brush.Parse("#EAB308"), // Yellow
                    //JobStatus.Stopped => Brush.Parse("#6B7280"),            // Dark grey
                    JobStatus.Completed => Brush.Parse("#16A34A"),          // Green
                    JobStatus.Error => Brush.Parse("#DC2626"),              // Red
                    JobStatus.Inactive => Brush.Parse("#9CA3AF"),           // Grey
                    _ => Brush.Parse("#9CA3AF")
                };
            }

            return Brush.Parse("#9CA3AF");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}