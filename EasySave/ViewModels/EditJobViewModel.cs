using System;
using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.ViewModels
{
    public class EditJobViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public string Title => Services.LanguageManager.Instance["jobs.create_title"];


        private string _jobName;
        public string JobName
        {
            get => _jobName;
            set { _jobName = value; OnPropertyChanged(); }
        }

        private string _sourceDirectory;
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set { _sourceDirectory = value; OnPropertyChanged(); }
        }

        private string _targetDirectory;
        public string TargetDirectory
        {
            get => _targetDirectory;
            set { _targetDirectory = value; OnPropertyChanged(); }
        }

        private BackupType _selectedType;
        public BackupType SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        public List<BackupType> BackupTypes { get; } = new List<BackupType> 
        { 
            BackupType.Full, 
            BackupType.Differential 
        };

        public EditJobViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            SelectedType = BackupType.Full;
        }

        public void CreateJob()
        {
            if (string.IsNullOrWhiteSpace(JobName) || 
                string.IsNullOrWhiteSpace(SourceDirectory) || 
                string.IsNullOrWhiteSpace(TargetDirectory))
            {
                // In a real app we'd show a message box
                return;
            }

            _mainViewModel.CreateJob(JobName, SourceDirectory, TargetDirectory, SelectedType);
            _mainViewModel.NavigateToBackupList();
        }

        public void Cancel()
        {
            _mainViewModel.NavigateToBackupList();
        }
    }
}

