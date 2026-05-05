using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.ViewModels
{
    /// <summary>
    /// ViewModel for the main backup list view.
    /// Acts as a thin bridge between the UI and the central Job collection in MainViewModel.
    /// </summary>
    public class BackupListViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        // UI Header text
        public string Title => "Backup Jobs List";

        // Exposes the live job collection for UI binding
        public IEnumerable<BackupJob> Jobs => _mainViewModel.Jobs;

        public BackupListViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        // Action methods triggered by UI buttons.
        // They delegate the execution control to the specific job instances or the central viewmodel.
        public void PlayJob(BackupJob job) => job.Play();
        public void PauseJob(BackupJob job) => job.Pause();
        public void StopJob(BackupJob job) => job.Stop();
        public void DeleteJob(BackupJob job) => _mainViewModel.DeleteJob(job);
    }
}
