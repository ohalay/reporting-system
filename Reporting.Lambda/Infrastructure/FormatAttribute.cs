using System.Reflection;

internal class FormatAttribute : Attribute
{
    public string Format { get; }

    public FormatAttribute(string format)
        => Format = format;
}

public static class FormatExtensions
{
    public static object? GetFormattedValue(this PropertyInfo prop, object obj)
    {
        var attribute = prop.GetCustomAttribute<FormatAttribute>();
        var value = prop.GetValue(obj);

        if (value is DateTimeOffset offset)
            value = offset.DateTime;

        return attribute is null || value is null
            ? value
            : string.Format(attribute.Format, value);
    }
}
