﻿using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;

namespace CustomAlarm.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services
            .AddMaui();

        return builder.Build();
    }
}
