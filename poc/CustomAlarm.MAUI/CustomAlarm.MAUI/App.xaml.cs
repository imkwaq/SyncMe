﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using MaUIApplication = Microsoft.Maui.Controls.Application;

namespace CustomAlarm.MAUI;

public partial class App : MaUIApplication
{
    private static IServiceProvider _serviceProvider { get; set; }

    public static T GetRequiredService<T>() => _serviceProvider.GetRequiredService<T>();

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();

        MainPage = serviceProvider.GetService<MainPage>();
    }
}
