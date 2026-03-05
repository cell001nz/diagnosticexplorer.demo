using Avalonia;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WidgetSample.Avalonia.Services;
using WidgetSample.Avalonia.ViewModels;
using DiagnosticExplorer;

namespace WidgetSample.Avalonia;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        IHost host = BuildHost(args);
        App.Services = host.Services;

        await host.StartAsync();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        await host.StopAsync();
    }

    private static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration(config => config
                .AddJsonFile("config.json", optional: true, reloadOnChange: false))
            .ConfigureServices(services =>
            {
                // Diagnostics — reads "DiagnosticExplorer" section from IConfiguration automatically
                services.AddDiagnosticExplorer();
                services.AddMetrics();

                // Dialog service
                services.AddSingleton<IDialogService, DialogService>();

                // View models
                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
