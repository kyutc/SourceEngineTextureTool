using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia.Data.Converters;

namespace SourceEngineTextureTool.Converters;

public abstract class DisplayAttributeConverter
{
    protected DisplayAttribute? GetDisplayAttribute(object? obj)
    {
        if (obj is null) return null;

        Type type = obj.GetType();
        MemberInfo memberInfo = type.GetMember(obj.ToString()!).First();
        return (DisplayAttribute?)memberInfo.GetCustomAttribute(typeof(DisplayAttribute));
    }
}

public class DisplayNameConverter : DisplayAttributeConverter, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return GetDisplayAttribute(value)?.Name ?? value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}