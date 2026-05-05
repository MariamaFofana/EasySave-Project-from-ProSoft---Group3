using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Converters
{
    public class TypeTranslatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BackupType type)
            {
                string key = type switch
                {
                    BackupType.Full => "backup_type.full",
                    BackupType.Differential => "backup_type.differential",
                    _ => type.ToString()
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
