using System;
using Microsoft.UI.Xaml.Data;

namespace MeuFinanceiro.UI.Converters;

public class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime date)
            return date.ToString("dd/MM/yyyy");
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}