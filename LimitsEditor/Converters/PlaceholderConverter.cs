using System.Globalization;
using System.Windows.Data;

namespace LimitsEditor.Converters;

/// <summary>
/// TODO: Replace with concrete converters as needed by the UI.
/// </summary>
public sealed class PlaceholderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
