using System.Collections.Generic;
using EasySave.ViewModels;
using EasySave.Models;

namespace EasySave.Views
{
    public class ConsoleView
    {
        private BackupViewModel _viewModel;

        public ConsoleView(BackupViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void DisplayMenu()
        {
        }

        public void RunCliMode(List<int> jobIds)
        {
        }

        private void OnProgressReceived(JobState state)
        {
        }
    }
}
