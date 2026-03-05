using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace WidgetSample.Avalonia.Services;

/// <summary>
/// Avalonia implementation of <see cref="IDialogService"/>.
/// </summary>
public class DialogService : IDialogService
{
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }

    public Task ShowInfoAsync(string message, string title = "Information") =>
        ShowMessageAsync(title, message, MessageBoxIcon.Info);

    public Task ShowWarningAsync(string message, string title = "Warning") =>
        ShowMessageAsync(title, message, MessageBoxIcon.Warning);

    public Task ShowErrorAsync(string message, string title = "Error") =>
        ShowMessageAsync(title, message, MessageBoxIcon.Error);

    public async Task<bool> ShowConfirmAsync(string message, string title = "Confirm")
    {
        var owner = GetMainWindow();
        if (owner == null) return false;

        var dialog = new Window
        {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        bool result = false;

        var yesButton = new Button { Content = "Yes", Width = 80, Margin = new Thickness(4) };
        var noButton  = new Button { Content = "No",  Width = 80, Margin = new Thickness(4) };

        yesButton.Click += (_, _) => { result = true;  dialog.Close(); };
        noButton.Click  += (_, _) => { result = false; dialog.Close(); };

        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 16,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, MaxWidth = 400 },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { yesButton, noButton }
                }
            }
        };

        await dialog.ShowDialog(owner);
        return result;
    }

    public async Task<string?> ShowOpenFileAsync(string title = "Open File", string? filter = null)
    {
        var owner = GetMainWindow();
        if (owner == null) return null;

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = BuildFileTypeFilter(filter)
        };

        var files = await owner.StorageProvider.OpenFilePickerAsync(options);
        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> ShowSaveFileAsync(string title = "Save File", string? defaultFileName = null, string? filter = null)
    {
        var owner = GetMainWindow();
        if (owner == null) return null;

        var options = new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = BuildFileTypeFilter(filter)
        };

        var file = await owner.StorageProvider.SaveFilePickerAsync(options);
        return file?.Path.LocalPath;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static IReadOnlyList<FilePickerFileType>? BuildFileTypeFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return null;

        // Accept simple patterns like "*.txt" or "*.txt;*.csv"
        var patterns = filter.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new[] { new FilePickerFileType(filter) { Patterns = patterns } };
    }

    private static async Task ShowMessageAsync(string title, string message, MessageBoxIcon icon)
    {
        var app = Application.Current;
        if (app?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is not { } owner)
            return;

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {

            var dialog = new Window
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var okButton = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Right };
            okButton.Click += (_, _) => dialog.Close();

            string iconText = icon switch
            {
                MessageBoxIcon.Warning => "⚠",
                MessageBoxIcon.Error => "✖",
                _ => "ℹ"
            };

            dialog.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 16,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 12,
                        Children =
                        {
                            new TextBlock { Text = iconText, FontSize = 24, VerticalAlignment = VerticalAlignment.Top },
                            new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, MaxWidth = 380, VerticalAlignment = VerticalAlignment.Center }
                        }
                    },
                    okButton
                }
            };

            await dialog.ShowDialog(owner);
        });
    }

    private enum MessageBoxIcon { Info, Warning, Error }
}

