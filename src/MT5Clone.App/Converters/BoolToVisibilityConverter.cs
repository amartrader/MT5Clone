using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MT5Clone.App.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = parameter?.ToString() == "Invert";
            return boolValue != invert;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue;
        return false;
    }
}

public class ProfitColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            var color = doubleValue >= 0 ? Color.Parse("#00FF00") : Color.Parse("#FF0000");
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Color.Parse("#FFFFFF"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PriceDirectionColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUp)
        {
            var color = isUp ? Color.Parse("#00FF00") : Color.Parse("#FF0000");
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Color.Parse("#FFFFFF"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ImpactToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Core.Models.EconomicEventImpact impact)
        {
            var colorStr = impact switch
            {
                Core.Models.EconomicEventImpact.High => "#FF0000",
                Core.Models.EconomicEventImpact.Medium => "#FFA500",
                Core.Models.EconomicEventImpact.Low => "#FFFF00",
                _ => "#808080"
            };
            return new SolidColorBrush(Color.Parse(colorStr));
        }
        return new SolidColorBrush(Color.Parse("#808080"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
