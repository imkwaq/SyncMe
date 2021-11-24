﻿using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using dotMorten.Xamarin.Forms;
using SyncMe.Models;
using SyncMe.Repos;

namespace SyncMe.Views;

public sealed partial class CreateEventPage : ContentPage, IDisposable
{
    private readonly ISyncNamespaceRepository _namespaceRepository;
    private readonly ISyncEventsRepository _eventsRepository;
    private readonly Dictionary<string, IReadOnlyCollection<Namespace>> _namespaces;
    private readonly IDisposable _addEventConnection;
    private readonly IDisposable _addEventSubscription;
    private readonly SyncEventViewModel _eventModel;

    public ToolbarItem AddEvent { get; init; }
    public IObservable<Guid> ScheduledEvents { get; }

    public CreateEventPage(ISyncEventsRepository eventsRepository, ISyncNamespaceRepository namespaceRepository)
    {
        _eventModel = new SyncEventViewModel();
        InitializeComponent();
        _namespaceRepository = namespaceRepository;
        _eventsRepository = eventsRepository;
        _namespaces = _namespaceRepository.GetAllSyncNamespaces();
        BindingContext = _eventModel;

        AddEvent = new ToolbarItem { Text = "Add event", };

        var scheduledEvents = Observable
            .FromEventPattern(AddEvent, nameof(Button.Clicked))
            .Select(x => AddNewSyncEvent())
            .Do(x => _namespaceRepository.AddSyncNamespace(_eventModel.Namespace))
            .Publish();

        _addEventConnection = scheduledEvents.Connect();
        ScheduledEvents = scheduledEvents;

        _addEventSubscription = ScheduledEvents
            .SelectMany(x => NavigateToCalendar())
            .Subscribe(x => CleanUpElements());
    }

    private void OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (e.ChosenSuggestion != null)
        {
            // User selected an item from the suggestion list, take an action on it here.
        }
        else
        {
            // User hit Enter from the search box. Use args.QueryText to determine what to do.
        }
    }

    private void OnSuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
    {
        if (sender is AutoSuggestBox autoSuggest)
        {
            var particles = autoSuggest.Text.Split('.');
            //if (particles.Length > 1)

            autoSuggest.Text = $"{e.SelectedItem}.";
        }
    }

    private void OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender is AutoSuggestBox autoSuggest)
        {
            autoSuggest.ItemsSource = _namespaces
                .Select(x => x.Key)
                .Where(x => x.StartsWith(autoSuggest.Text, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToArray();
        }
    }

    private async void OnConfigureScheduleClicked(object sender, EventArgs e) => await Navigation.PushAsync(new EventSchedulePage(_eventModel));

    private async void OnAlertButtonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new EventAlertPage(_eventModel));

    public Guid AddNewSyncEvent() => _eventsRepository.AddSyncEvent(_eventModel.SyncEvent);

    private void AddNewSyncEvent(object sender, EventArgs e) => AddNewSyncEvent();

    private async void CancelEvent(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_eventModel.Namespace))
        {
            if (!await DisplayAlert(null, "Discard this event?", "Keep editing", "Discard"))
            {
                CleanUpElements();
            }
        }
        await NavigateToCalendar();
    }

    private static async Task<Unit> NavigateToCalendar()
    {
        await Shell.Current.GoToAsync("//calendar");
        return Unit.Default;
    }

    private void ValidateButtonState(object sender, PropertyChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_eventModel.Namespace) && !string.IsNullOrEmpty(_eventModel.Title))
        {
            _eventModel.IsAddEvenEnabled = true;
        }
        else
        {
            _eventModel.IsAddEvenEnabled = false;
        }
    }

    private void OnSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            _eventModel.StartDate = DateTime.Today.Date;
            _eventModel.EndDate = DateTime.Today.Date.AddDays(1).AddTicks(-1);
        }
    }

    private void CleanUpElements()
    {
        _eventModel.Namespace = string.Empty;
        _eventModel.Title = string.Empty;
    }

    public void Dispose()
    {
        _addEventSubscription.Dispose();
        _addEventConnection.Dispose();
    }
}
