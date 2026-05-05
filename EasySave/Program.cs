using System;
using System.Formats.Asn1;
using System.Reflection.PortableExecutable;
using Avalonia;
using EasySave.Models;
using EasySave.Services;
using EasySave.ViewModels;

namespace EasySave;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}