﻿using SyncMe.Models;
using SyncMe.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace SyncMe.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CalendarPage : ContentPage
{
    private readonly CalendarPageViewModel _viewModel;
    private readonly ISyncEventsService _syncEventsService;
    private readonly ISyncNamespaceService _namespaceService;
    private readonly IConfigRepository _configRepository;
    private readonly IPathProvider _pathProvider;

    public CalendarPage(
        CalendarPageViewModel viewModel,
        ISyncEventsService syncEventsService,
        ISyncNamespaceService namespaceService,
        IConfigRepository configRepository,
        IPathProvider pathProvider)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _syncEventsService = syncEventsService;
        _namespaceService = namespaceService;
        _configRepository = configRepository;
        _pathProvider=pathProvider;
        SoundToggle.IsToggled = !_configRepository.Get(ConfigKey.IsMute);
        AddEvent.Clicked += AddEvent_Clicked;
        year.BindingContext = Calendar1;
    }

    protected override void OnAppearing()
    {
        _viewModel.InitEventsCollection();
        base.OnAppearing();
    }

    public async void AddEvent_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateEventPage(_syncEventsService, _namespaceService));
    }

    private void OnRemoveClicked(object sender, EventArgs e)
    {
        if (sender is MenuItem { CommandParameter: SyncEventViewModel { SyncEvent.Id: var eventId } })
        {
            _syncEventsService.TryRemoveInternalEvent(eventId);
            _viewModel.InitEventsCollection();
        }
    }

    private void OnSoundToggled(object sender, ToggledEventArgs e)
    {
        _configRepository.Set(ConfigKey.IsMute, !e.Value);
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        const string Cancel = "CANCEL";
        const string Destruction = "ERASE";

        var logFiles = Directory.Exists(_pathProvider.SyncMeLogsFolder)
            ? Directory.EnumerateFiles(_pathProvider.SyncMeLogsFolder).Select(Path.GetFileName).ToArray()
            : Array.Empty<string>();

        var actionResult = await DisplayActionSheet("Check logs", Cancel, Destruction, logFiles);

        switch (actionResult)
        {
            case Destruction:
                Directory.Delete(_pathProvider.SyncMeLogsFolder, true);
                break;

            case string x when Path.Combine(_pathProvider.SyncMeLogsFolder, x) is string selectedFile && File.Exists(selectedFile):
                await Launcher.OpenAsync(new OpenFileRequest { File = new(selectedFile) });
                break;

            default:
                break;
        }
    }
}
