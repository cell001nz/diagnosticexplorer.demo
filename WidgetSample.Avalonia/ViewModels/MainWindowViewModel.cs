using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Threading;
using Classic.CommonControls.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagnosticExplorer;
using log4net;
using log4net.Config;
using log4net.Core;
using Metalama.Patterns.Observability;
using WidgetSample.Avalonia.Models;
using WidgetSample.Avalonia.Services;
using WidgetSample.Avalonia.Views;

namespace WidgetSample.Avalonia.ViewModels;

[DiagnosticClass(AttributedPropertiesOnly = true, DeclaringTypeOnly = false)]
public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly ILog _gadgetLog = LogManager.GetLogger("Gadgets");
    private static readonly ILog _widgetLog = LogManager.GetLogger("Widgets");
    private static readonly ILog _formLog = LogManager.GetLogger(typeof(MainWindowViewModel));
    private static readonly Random _rand = new();
    private static int _evtCount;

    private readonly IDialogService _dialogService;
    private readonly Timer _counterTimer;
    private readonly Timer _evtTimer;
    private readonly Timer _listTestTimer;

    [Property(Category = "Widgets")] public decimal ItemCount { get; set; } = 123;

    [Property(Category = "General")]
    public bool SendSystemEvents
    {
        get => field;
        set => SetProperty(value, field, v => field = v);
    }

    [Property(Category = "General")]
    public bool SendWidgetEvents
    {
        get => field;
        set => SetProperty(value, field, v => field = v);
    }

    [Property(Category = "General")]
    public bool SendGadgetEvents
    {
        get => field;
        set => SetProperty(value, field, v => field = v);
    }


    public string StatusText
    {
        get => field;
        set => SetProperty(ref field, value);
    } = "Ready";

    public ObservableCollection<SiteStatus> SiteStatuses { get; } = [];

    [Property] public string InfoText { get; set; } = "Status OK";

    [Property] public int SetMePlease { get; set; }

    [Property] public int Counter2 { get; set; }

    [Property(Category = "Gadgets", Description = "Max Gadget Id")]
    public int GadgetIdCount { get; private set; }

    [Property(Category = "Widgets")] public int WidgetIdCount { get; private set; }

    [RateProperty(Category = "Widgets", ExposeRate = false, ExposeTotal = true)]
    public RateCounter WidgetEvents { get; } = new(5);

    [RateProperty(Category = "Gadgets", ExposeTotal = true, Description = "The rate of gadget events received")]
    public RateCounter GadgetEvents { get; } = new(5);
    

    [CollectionProperty(CollectionMode.List, Category = "All Gadgets")]
    public ObservableCollection<Gadget> Gadgets { get; } = [];

    public DataGridCollectionView GadgetsView => field ??= new DataGridCollectionView(Gadgets);

    [CollectionProperty(CollectionMode.Categories, CategoryProperty = nameof(Widget.FullName))]
    public ObservableCollection<Widget> Widgets { get; } = [];

    public DataGridCollectionView WidgetsView => field ??= new DataGridCollectionView(Widgets);

    private DiagnosticHostingService _diagnosticHostingService;
    
    public MainWindowViewModel(IDialogService dialogService, DiagnosticHostingService diagnosticHostingService)
    {
        _dialogService = dialogService;
        _diagnosticHostingService = diagnosticHostingService;

        var log4netPath = Path.GetFullPath("log4net.config");
        if (File.Exists(log4netPath))
            XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netPath));

        DiagnosticManager.Register(this, "Main Window", "Main");

        _diagnosticHostingService.StatusChanged += OnDiagnosticStatusChanged;
        OnDiagnosticStatusChanged(this, _diagnosticHostingService.Status);

        _evtTimer = new Timer(SendEvents, null, 1000, 1000);
        _counterTimer = new Timer(IncrementCount, null, 400, 400);
        _listTestTimer = new Timer(_ => { }, null, 100, 100);
    }

    private void StartDiagnostics()
    {
        try
        {
            StatusText = $"Connecting";
            _diagnosticHostingService.Start();
        }
        catch (Exception ex)
        {
            StatusText = $"Error starting diagnostics: {ex.Message}";
        }
    }

    private void OnDiagnosticStatusChanged(object? sender, HostingStatus status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusText = status.StatusText;

            SiteStatuses.Clear();
            foreach (var site in status.Sites)
                SiteStatuses.Add(site);
        });
    }

    private static string SettingsFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DiagnosticExplorer", "WidgetSample", "settings.json");

    [RelayCommand]
    private async Task StopDiagnosticsAsync()
    {
        await _diagnosticHostingService.StopAsync();
        StatusText = "Diagnostics stopped";
    }

    [RelayCommand]
    private Task StartDiagnosticsAsync()
    {
        StartDiagnostics();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void AddGadget()
    {
        var gadget = new Gadget(GadgetIdCount++);
        DiagnosticManager.Register(gadget, $"Gadget {gadget.Id}", "Gadgets");

        Gadgets.Add(gadget);
        _gadgetLog.InfoFormat("Added gadget {0}", gadget.Id);
        GadgetEvents.Register(1);
        OnPropertyChanged(nameof(GadgetIdCount));
    }

    [RelayCommand]
    private void RemoveGadget(Gadget? gadget = null)
    {
        gadget ??= Gadgets.ElementAtOrDefault(0);
        if (gadget != null)
        {
            //Shouldn't be necessary, but Avalon binding is not releasing the object for garbage collection
            DiagnosticManager.Unregister(gadget);
            Gadgets.Remove(gadget);
            _gadgetLog.InfoFormat("Removed gadget {0}", gadget.Id);
            GadgetEvents.Register(1);
        }

        GC.Collect();
    }

    [RelayCommand]
    private void AddWidget()
    {
        var widget = new Widget(WidgetIdCount++);
        //Shouldn't be necessary, but Avalon binding is not releasing the object for garbage collection
        DiagnosticManager.Register(widget, widget.Name, "Widgets");
        Widgets.Add(widget);
        _widgetLog.InfoFormat("Added widget {0}", widget.Id);
        WidgetEvents.Register(1);
        OnPropertyChanged(nameof(WidgetIdCount));
    }

    [RelayCommand]
    private void RemoveWidget(Widget? widget = null)
    {
        widget ??= Widgets.ElementAtOrDefault(0);
        if (widget != null)
        {
            DiagnosticManager.Unregister(widget);
            Widgets.Remove(widget);
            _widgetLog.InfoFormat("Removed widget {0}", widget.Id);
            WidgetEvents.Register(1);
        }

        GC.Collect();
    }

    [RelayCommand]
    private void LogMinorProblem()
    {
        try
        {
            _ = "Hello".Substring(6, 10);
        }
        catch (Exception ex)
        {
            _formLog.Info("Info: something went a little wrong.", ex);
        }
    }

    [RelayCommand]
    private void LogNotice()
    {
        try
        {
            _ = "Hello".Substring(6, 10);
        }
        catch (Exception ex)
        {
            _formLog.Notice("Notice: something went a little wrong.", ex);
        }
    }

    [RelayCommand]
    private void LogWarning()
    {
        try
        {
            _ = "Hello".Substring(6, 10);
        }
        catch (Exception ex)
        {
            _formLog.Warn("Warn: something went a little wrong.", ex);
        }
    }

    [RelayCommand]
    private void LogError()
    {
        try
        {
            decimal denominator = 0;
            var _ = 12.345M / denominator;
        }
        catch (Exception ex)
        {
            _formLog.Error(ex);
        }
    }

    [RelayCommand]
    private void GenerateEvents10()
    {
        GenerateEvents(10);
    }

    [RelayCommand]
    private void GenerateEvents100()
    {
        GenerateEvents(100);
    }

    [RelayCommand]
    private void GenerateEvents1000()
    {
        GenerateEvents(1000);
    }

    private void GenerateEvents(int count)
    {
        Task.Run(() =>
        {
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var data = new LoggingEventData
                {
                    Message = $"Event #{i}",
                    Level = IntToLevel(_rand.Next(1, 12) * 10000)
                };
                _formLog.Logger.Log(new LoggingEvent(data));
            }

            Debug.WriteLine($"Sent {count} messages in {watch.ElapsedMilliseconds}ms");
        });
    }

    [DiagnosticMethod]
    public int GetRandomInt1()
    {
        return _rand.Next();
    }

    [DiagnosticMethod]
    public int GetRandomInt2()
    {
        return _rand.Next();
    }

    [DiagnosticMethod]
    public int GetRandomInt3()
    {
        return _rand.Next();
    }

    [DiagnosticMethod]
    public int GetRandomInt4()
    {
        return _rand.Next();
    }

    [DiagnosticMethod]
    public string RandomText()
    {
        return string.Join(Environment.NewLine, Enumerable.Range(1, _rand.Next(5, 100)).Select(_ => RandomLine()));
    }

    [DiagnosticMethod]
    public string RandomWord()
    {
        return new string(Enumerable.Range(1, _rand.Next(1, 10)).Select(_ => (char)('A' + _rand.Next(0, 26))).ToArray());
    }

    [DiagnosticMethod]
    public string RandomLine()
    {
        return string.Join(" ", Enumerable.Range(1, _rand.Next(1, 50)).Select(_ => RandomWord()));
    }

    [DiagnosticMethod]
    public async Task SayHello(string message)
    {
        _formLog.Info($"SayHello: {message}");
        StatusText = $"Hello: {message}";

        await _dialogService.ShowInfoAsync($"Hello: {message}", "SayHello");
    }

    [DiagnosticMethod]
    public async Task<string> GetValueAsync(string message)
    {
        _formLog.Info($"GetValueAsync: {message}");
        await Task.Delay(500);

        return $"The date is {DateTime.Now:F} and you said: {message}";
    }

    [DiagnosticMethod]
    public void GargageCollect()
    {
        GC.Collect();
    }

    private void SendEvents(object? state)
    {
        if (SendSystemEvents) _formLog.Info($"System event {_evtCount++}");
        if (SendWidgetEvents) _widgetLog.Info($"Widget event {_evtCount++}");
        if (SendGadgetEvents) _gadgetLog.Info($"Gadget event {_evtCount++}");
    }

    private void IncrementCount(object? state)
    {
        SetMePlease++;
        Counter2++;
    }

    private static Level IntToLevel(int value)
    {
        if (value >= Level.Emergency.Value) return Level.Emergency;
        if (value >= Level.Fatal.Value) return Level.Fatal;
        if (value >= Level.Alert.Value) return Level.Alert;
        if (value >= Level.Critical.Value) return Level.Critical;
        if (value >= Level.Severe.Value) return Level.Severe;
        if (value >= Level.Error.Value) return Level.Error;
        if (value >= Level.Warn.Value) return Level.Warn;
        if (value >= Level.Notice.Value) return Level.Notice;
        if (value >= Level.Info.Value) return Level.Info;
        if (value >= Level.Debug.Value) return Level.Debug;
        if (value >= Level.Trace.Value) return Level.Trace;
        return Level.Verbose;
    }

    public async Task OnClosingAsync()
    {
        await _evtTimer.DisposeAsync();
        await _counterTimer.DisposeAsync();
        await _listTestTimer.DisposeAsync();
        _diagnosticHostingService.StatusChanged -= OnDiagnosticStatusChanged;
        await _diagnosticHostingService.StopAsync();
    }
}