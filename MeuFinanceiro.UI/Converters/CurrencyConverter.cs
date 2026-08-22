using System;
using Microsoft.UI.Xaml.Data;

namespace MeuFinanceiro.UI.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            // Formata em Real brasileiro (pt-BR): R$ 1.234,56
            return decimalValue.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        }

        if (value is double doubleValue)
        {
            return ((decimal)doubleValue).ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        }

        return "R$ 0,00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}