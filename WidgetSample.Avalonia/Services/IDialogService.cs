using System.Threading.Tasks;

namespace WidgetSample.Avalonia.Services;

/// <summary>
/// Provides methods for displaying common dialogs to the user.
/// </summary>
public interface IDialogService
{
    /// <summary>Shows an informational message to the user.</summary>
    Task ShowInfoAsync(string message, string title = "Information");

    /// <summary>Shows a warning message to the user.</summary>
    Task ShowWarningAsync(string message, string title = "Warning");

    /// <summary>Shows an error message to the user.</summary>
    Task ShowErrorAsync(string message, string title = "Error");

    /// <summary>Asks the user a yes/no question. Returns <c>true</c> if the user confirmed.</summary>
    Task<bool> ShowConfirmAsync(string message, string title = "Confirm");

    /// <summary>
    /// Shows an open-file dialog. Returns the selected file path, or <c>null</c> if cancelled.
    /// </summary>
    Task<string?> ShowOpenFileAsync(string title = "Open File", string? filter = null);

    /// <summary>
    /// Shows a save-file dialog. Returns the chosen file path, or <c>null</c> if cancelled.
    /// </summary>
    Task<string?> ShowSaveFileAsync(string title = "Save File", string? defaultFileName = null, string? filter = null);
}

