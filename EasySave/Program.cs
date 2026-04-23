using System;
using System.Formats.Asn1;
using EasySave.ViewModels;

class Program
{
    //methodes static pour respecter la sortie coordonée du mainviewmodel et la logique de l'application
    private static void ExecuteSingleJobFromUserInput(MainViewModel viewModel)
    {
        Console.Write("Enter the job number (1 to 5): ");
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int jobNumber))
        {
            int index = jobNumber - 1;
            viewModel.ExecuteJob(index);
            DisplayProgress(jobNumber, 100);
        }
        else
        {
            Console.WriteLine("Invalid input.");
        }
    }

    private static void ExecuteCommandLineArguments(MainViewModel viewModel, string[] args)
    {
        string command = string.Join("", args).Trim();

        if (string.Equals(command, "all", StringComparison.OrdinalIgnoreCase))
        {
            viewModel.ExecuteAllJobs();
            Console.WriteLine("All backup jobs executed.");
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
            DisplayProgress(singleJobNumber, 100);
            return;
        }

        Console.WriteLine("Invalid command line argument.");
    }

    private static void ExecuteRange(MainViewModel viewModel, string command)
    {
        string[] parts = command.Split('-');

        if (parts.Length != 2)
        {
            Console.WriteLine("Invalid range format.");
            return;
        }

        if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
        {
            for (int i = start; i <= end; i++)
            {
                viewModel.ExecuteJob(i - 1);
                DisplayProgress(i, 100);
            }
        }
        else
        {
            Console.WriteLine("Invalid range values.");
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
                DisplayProgress(jobNumber, 100);
            }
            else
            {
                Console.WriteLine($"Invalid job number: {part}");
            }
        }
    }


    static void Main(string[] args)
    {
        Console.WriteLine("On est dans le Main.");

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
                    RunCLI(viewModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void RunCLI(MainViewModel viewModel)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("        Welcome to EasySave      ");
        Console.WriteLine("=================================");
        Console.WriteLine("1 - Execute one backup job");
        Console.WriteLine("2 - Execute all backup jobs");
        Console.WriteLine("3 - Exit");
        Console.Write("Your choice: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                ExecuteSingleJobFromUserInput(viewModel);
                break;

            case "2":
                viewModel.ExecuteAllJobs();
                Console.WriteLine("All backup jobs executed.");
                break;

            case "3":
                Console.WriteLine("Application closed.");
                break;

            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
private static void RunCLI()
    {
        Console.WriteLine("Welcome to EasySave CLI!");
    }

    private static void DisplayProgress(int jobNumber, int progress)
    {
        Console.WriteLine($"Progress: {progress}%");
    }
}