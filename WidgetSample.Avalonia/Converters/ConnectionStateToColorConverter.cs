using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DiagnosticExplorer;

namespace WidgetSample.Avalonia.Converters;

/// <summary>Maps a <see cref="SiteConnectionState"/> to a status indicator colour.</summary>
public class ConnectionStateToColorConverter : IValueConverter
{
    public static readonly ConnectionStateToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            SiteConnectionState.Connected    => Brushes.LimeGreen,
            SiteConnectionState.Connecting   => Brushes.Orange,
            SiteConnectionState.Disconnected => Brushes.Crimson,
            _                                => Brushes.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}


