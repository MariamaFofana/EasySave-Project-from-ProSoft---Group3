using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using EasySave.Models;
using EasySave.Services;
using EasyLogDLL;

namespace EasySave.ViewModels
{
    /// <summary>
    /// ViewModel for the main backup list view.
    /// Acts as a thin bridge between the UI and the central Job collection in MainViewModel.
    /// </summary>
    public class BackupListViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public string Title => "Your backup jobs";

        // Exposes the live job collection for UI binding
        public ObservableCollection<BackupJob> Jobs => _mainViewModel.Jobs;

        // Quick settings available directly from the dashboard
        public List<string> AvailableLanguages { get; } = new List<string> { "en", "fr" };
        public List<string> AvailableLogFormats { get; } = new List<string> { "json", "xml" };
        public List<string> AvailableLogModes { get; } = new List<string> { "local", "central", "mixed" };

        public string SelectedLanguage
        {
            get => SettingsManager.CurrentSettings.Language;
            set
            {
                if (SettingsManager.CurrentSettings.Language != value)
                {
                    SettingsManager.CurrentSettings.Language = value;
                    LanguageManager.Instance.CurrentLanguage = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalJobsText));
                    OnPropertyChanged(nameof(SelectedJobsText));
                }
            }
        }

        public string SelectedLogFormat
        {
            get => SettingsManager.CurrentSettings.LogFormat;
            set
            {
                if (SettingsManager.CurrentSettings.LogFormat != value)
                {
                    SettingsManager.CurrentSettings.LogFormat = value;
                    EasyLogger.LogFormat = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedLogMode
        {
            get => SettingsManager.CurrentSettings.LogMode;
            set
            {
                if (SettingsManager.CurrentSettings.LogMode != value)
                {
                    SettingsManager.CurrentSettings.LogMode = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Summary info for the right panel
        public int TotalJobsCount => Jobs.Count;
        public int SelectedJobsCount => Jobs.Count(j => j.IsSelected);

        public string TotalJobsText => string.Format(LanguageManager.Instance["jobs.total_jobs"], TotalJobsCount);
        public string SelectedJobsText => string.Format(LanguageManager.Instance["jobs.selected_jobs"], SelectedJobsCount);

        public BackupListViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Jobs.CollectionChanged += OnJobsCollectionChanged;

            foreach (var job in Jobs)
            {
                job.PropertyChanged += OnJobPropertyChanged;
            }
        }

        private void OnJobsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BackupJob job in e.NewItems)
                {
                    job.PropertyChanged += OnJobPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (BackupJob job in e.OldItems)
                {
                    job.PropertyChanged -= OnJobPropertyChanged;
                }
            }

            OnPropertyChanged(nameof(TotalJobsCount));
            OnPropertyChanged(nameof(SelectedJobsCount));
            OnPropertyChanged(nameof(TotalJobsText));
            OnPropertyChanged(nameof(SelectedJobsText));
        }

        private void OnJobPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BackupJob.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedJobsCount));
                OnPropertyChanged(nameof(SelectedJobsText));
            }
        }

        // Per-job actions
        public void PlayJob(BackupJob job) => System.Threading.Tasks.Task.Run(() => job.Play());
        public void PauseJob(BackupJob job) => job.Pause();
        public void StopJob(BackupJob job) => job.Stop();
        public void DeleteJob(BackupJob job) => _mainViewModel.DeleteJob(job);

        // Global actions
        public void NewJob() => _mainViewModel.NavigateToEditJob();
        public void OpenSettings() => _mainViewModel.NavigateToSettings();
        public void RunAllJobs() => _mainViewModel.ExecuteAllJobs();
        public void RunSelectedJobs() => _mainViewModel.ExecuteSelectedJobs();

        public void PauseAllJobs()
        {
            foreach (var job in Jobs)
            {
                job.Pause();
            }
        }

        public void ResumeAllJobs()
        {
            foreach (var job in Jobs)
            {
                System.Threading.Tasks.Task.Run(() => job.Play());
            }
        }

        public void StopAllJobs()
        {
            foreach (var job in Jobs)
            {
                job.Stop();
            }
        }
    }
}