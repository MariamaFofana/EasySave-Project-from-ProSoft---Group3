using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.Views;
using EasySave.ViewModels;

namespace EasySave;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = new MainViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };

            // Handle CLI arguments (e.g., "1-3" or "1;3")
            if (desktop.Args != null && desktop.Args.Length > 0)
            {
                System.Threading.Tasks.Task.Run(() => HandleCommandLineArgs(desktop.Args[0], mainVm));
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void HandleCommandLineArgs(string input, MainViewModel mainVm)
    {
        var jobIndices = new System.Collections.Generic.List<int>();

        try
        {
            if (input.Contains("-"))
            {
                var parts = input.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                {
                    for (int i = start; i <= end; i++) jobIndices.Add(i - 1);
                }
            }
            else if (input.Contains(";"))
            {
                var parts = input.Split(';');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int index)) jobIndices.Add(index - 1);
                }
            }
            else if (int.TryParse(input, out int singleIndex))
            {
                jobIndices.Add(singleIndex - 1);
            }

            foreach (var index in jobIndices)
            {
                mainVm.ExecuteJob(index);
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error processing arguments: {ex.Message}");
        }
    }
}