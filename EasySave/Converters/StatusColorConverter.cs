using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using EasySave.Models;

namespace EasySave.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JobStatus status)
            {
                return status switch
                {
                    JobStatus.Active => Brush.Parse("#3498db"),    // Blue
                    JobStatus.Completed => Brush.Parse("#2ecc71"), // Green
                    JobStatus.Error => Brush.Parse("#e74c3c"),     // Red
                    _ => Brush.Parse("#95a5a6")                    // Grey for Inactive
                };
            }
            return Brush.Parse("#95a5a6");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
