using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Converters
{
    public class StatusTranslatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JobStatus status)
            {
                string key = status switch
                {
                    JobStatus.Active => "status.active",
                    JobStatus.Completed => "status.completed",
                    JobStatus.Error => "status.error",
                    _ => "status.inactive"
                };
                return LanguageManager.Instance[key];
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
