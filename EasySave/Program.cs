using System;
using System.Formats.Asn1;
using EasySave.ViewModels;
using EasySave.Services;
using EasySave.Models;


class Program
{
    // Static methods to respect the coordinated output of the mainviewmodel and the application logic
    private static void ExecuteSingleJobFromUserInput(MainViewModel viewModel)
    {
        Console.WriteLine();
        Console.WriteLine(LanguageManager.GetInstance().GetText("jobs.header"));
        if (viewModel.Jobs.Count == 0)
        {
            Console.WriteLine(LanguageManager.GetInstance().GetText("jobs.none"));
            return;
        }

        for (int i = 0; i < viewModel.Jobs.Count; i++)
        {
            var job = viewModel.Jobs[i];
            Console.WriteLine($"{i + 1} | {job.Name} | {job.Type} | {job.SourceDirectory} -> {job.TargetDirectory}");
        }
        Console.WriteLine();

        Console.Write(LanguageManager.GetInstance().GetText("cli.prompt.job_number"));
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int jobNumber))
        {
            int index = jobNumber - 1;
            viewModel.ExecuteJob(index);
        }
        else
        {
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.invalid_input"));
        }
    }

    private static void CreateJobFromUserInput(MainViewModel viewModel)
    {
        Console.Write(LanguageManager.GetInstance().GetText("prompt.name"));
        string? name = Console.ReadLine() ?? "";

        Console.Write(LanguageManager.GetInstance().GetText("prompt.source"));
        string? source = Console.ReadLine() ?? "";

        Console.Write(LanguageManager.GetInstance().GetText("prompt.target"));
        string? target = Console.ReadLine() ?? "";

        Console.Write(LanguageManager.GetInstance().GetText("prompt.type"));
        string? typeStr = Console.ReadLine();
        BackupType type = (typeStr == "2") ? BackupType.Differential : BackupType.Full;

        viewModel.CreateJob(name, source, target, type);
        Console.WriteLine(LanguageManager.GetInstance().GetText("jobs.created"));
    }

    private static void SettingsMenuFromUserInput()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.separator"));
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.4"));
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.separator"));
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.settings.1"));
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.settings.2"));
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.settings.3"));
            Console.Write(LanguageManager.GetInstance().GetText("cli.prompt.choice"));

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write(LanguageManager.GetInstance().GetText("prompt.language"));
                    string? langStr = Console.ReadLine();
                    if (langStr == "2")
                    {
                        LanguageManager.GetInstance().CurrentLanguage = "fr";
                        SettingsManager.CurrentSettings.Language = "fr";
                    }
                    else
                    {
                        LanguageManager.GetInstance().CurrentLanguage = "en";
                        SettingsManager.CurrentSettings.Language = "en";
                    }
                    SettingsManager.SaveSettings();
                    Console.WriteLine(LanguageManager.GetInstance().GetText("language.changed"));
                    break;

                case "2":
                    Console.Write(LanguageManager.GetInstance().GetText("prompt.log_format"));
                    string? formatStr = Console.ReadLine();
                    if (formatStr == "2")
                    {
                        EasyLogDLL.EasyLogger.LogFormat = "xml";
                        SettingsManager.CurrentSettings.LogFormat = "xml";
                    }
                    else
                    {
                        EasyLogDLL.EasyLogger.LogFormat = "json";
                        SettingsManager.CurrentSettings.LogFormat = "json";
                    }
                    SettingsManager.SaveSettings();
                    Console.WriteLine(LanguageManager.GetInstance().GetText("log_format.changed"));
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine(LanguageManager.GetInstance().GetText("cli.invalid_choice"));
                    break;
            }
        }
    }

    private static void ExecuteCommandLineArguments(MainViewModel viewModel, string[] args)
    {
        string command = string.Join("", args).Trim();

        if (string.Equals(command, "all", StringComparison.OrdinalIgnoreCase))
        {
            viewModel.ExecuteAllJobs();
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.all_executed"));
            return;
        }

        if (command.Contains("-"))
        {
            ExecuteRange(viewModel, command);
            return;
        }

        if (command.Contains(";"))
        {
            ExecuteList(viewModel, command);
            return;
        }

        if (int.TryParse(command, out int singleJobNumber))
        {
            int index = singleJobNumber - 1;
            viewModel.ExecuteJob(index);
            return;
        }

        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.invalid_command"));
    }

    private static void ExecuteRange(MainViewModel viewModel, string command)
    {
        string[] parts = command.Split('-');

        if (parts.Length != 2)
        {
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.invalid_range_format"));
            return;
        }

        if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
        {
            for (int i = start; i <= end; i++)
            {
                viewModel.ExecuteJob(i - 1);
            }
        }
        else
        {
            Console.WriteLine(LanguageManager.GetInstance().GetText("cli.invalid_range_values"));
        }
    }

    private static void ExecuteList(MainViewModel viewModel, string command)
    {
        string[] parts = command.Split(';');

        foreach (string part in parts)
        {
            if (int.TryParse(part.Trim(), out int jobNumber))
            {
                viewModel.ExecuteJob(jobNumber - 1);
            }
            else
            {
                Console.WriteLine($"{LanguageManager.GetInstance().GetText("cli.invalid_job_number")}{part}");
            }
        }
    }


    static void Main(string[] args)
    {
        MainViewModel viewModel = new MainViewModel();
        viewModel.LoadJobs();

        try
        {
            if (args.Length > 0)
            {
                ExecuteCommandLineArguments(viewModel, args);
            }
            else
            {
                while (RunCLI(viewModel)) { }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{LanguageManager.GetInstance().GetText("cli.error")}{ex.Message}");
        }
    }

    private static bool RunCLI(MainViewModel viewModel)
    {
        Console.WriteLine();
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.separator"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.welcome"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.separator"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.1"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.2"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.3"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.4"));
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.menu.5"));
        Console.Write(LanguageManager.GetInstance().GetText("cli.prompt.choice"));

        string? choice = Console.ReadLine();
        Console.Clear();

        switch (choice)
        {
            case "1":
                ExecuteSingleJobFromUserInput(viewModel);
                break;

            case "2":
                viewModel.ExecuteAllJobs();
                Console.WriteLine(LanguageManager.GetInstance().GetText("cli.all_executed"));
                break;

            case "3":
                CreateJobFromUserInput(viewModel);
                break;

            case "4":
                SettingsMenuFromUserInput();
                break;

            case "5":
                Console.WriteLine(LanguageManager.GetInstance().GetText("cli.app_closed"));
                return false;

            default:
                Console.WriteLine(LanguageManager.GetInstance().GetText("cli.invalid_choice"));
                break;
        }
        Console.Clear();

        return true;
    }

    private static void RunCLI()
    {
        Console.WriteLine(LanguageManager.GetInstance().GetText("cli.welcome_cli"));
    }
}