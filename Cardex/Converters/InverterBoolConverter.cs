using System.Globalization;

namespace Cardex.Converters;

// true → false e false → true. Usado para mostrar o "estado vazio" quando algo NÃO existe.
public class InverterBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : false;
    }
}
