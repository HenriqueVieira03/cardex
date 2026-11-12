using System.Globalization;

namespace Cardex.Converters;

// Carta que eu tenho (true) aparece normal; carta que falta (false) fica esmaecida.
public class BoolParaOpacidadeConverter : IValueConverter
{
    public double OpacidadeFalta { get; set; } = 0.3;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? 1.0 : OpacidadeFalta;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
