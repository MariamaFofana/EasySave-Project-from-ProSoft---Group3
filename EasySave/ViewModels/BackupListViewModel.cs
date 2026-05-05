using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.ViewModels
{
    public class BackupListViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public string Title => "Liste de vos sauvegardes";
        public IEnumerable<BackupJob> Jobs => _mainViewModel.Jobs;

        public BackupListViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public void PlayJob(BackupJob job) => job.Play();
        public void PauseJob(BackupJob job) => job.Pause();
        public void StopJob(BackupJob job) => job.Stop();
        public void DeleteJob(BackupJob job) => _mainViewModel.DeleteJob(job);
    }
}


