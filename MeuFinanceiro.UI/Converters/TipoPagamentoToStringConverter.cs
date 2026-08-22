using System;
using Microsoft.UI.Xaml.Data;
using MeuFinanceiro.Core.Enums;

namespace MeuFinanceiro.UI.Converters;

public class TipoPagamentoToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TipoPagamento tipo)
        {
            return tipo switch
            {
                TipoPagamento.PixDebito => "Pix/Débito",
                TipoPagamento.Credito => "Crédito",
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}