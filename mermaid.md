---
config:
  theme: dark
  layout: elk
---

classDiagram

    namespace EasyLog_DLL {

        class IEasyLogger {

            <<interface>>

            +LogAction(entry: LogEntry) : void

        }



        class Logger {

            <<Singleton>>

            -static Logger _instance

            -string _logDirectory

            +GetInstance() : Logger$

            +LogAction(entry: LogEntry) : void

            -WriteJsonEntry(entry: LogEntry, path: string) : void

        }



        class LogEntry {

            +DateTime Timestamp

            +string BackupName

            +string SourceFilePath

            +string TargetFilePath

            +long FileSize

            +long TransferTimeMs

        }

    }

    IEasyLogger <|.. Logger : implements

    class Program {

        +Main(args: string[]) : void

    }



    class CliArgumentProcessor {

        +ParseArguments(args: string[]) : List~int~

    }

    class LanguageManager {

        <<Singleton>>

        -static LanguageManager _instance

        -string _currentCulture

        +GetInstance() : LanguageManager$

        +GetString(key: string) : string

        +SetLanguage(culture: string) : void

    }



    class ConfigManager {

        -string _configPath

        +ConfigManager(path: string)

        +LoadJobs() : List~BackupJob~

        +SaveJobs(jobs: List~BackupJob~) : void

    }



    class StateManager {

        -string _stateFilePath

        +StateManager(path: string)

        +UpdateState(state: JobState) : void

    }

    class ConsoleView {

        -BackupViewModel _viewModel

        +ConsoleView(viewModel: BackupViewModel)

        +DisplayMenu() : void

        +RunCliMode(jobIds: List~int~) : void

        -OnProgressReceived(state: JobState) : void

    }



    class BackupViewModel {

        -List~BackupJob~ _jobs

        -ConfigManager _config

        -StateManager _state

        -IEasyLogger _logger

        -int MAX_JOBS = 5

        +event Action~JobState~ OnJobProgressUpdated



        +BackupViewModel(config: ConfigManager, state: StateManager, logger: IEasyLogger)

        +ExecuteJobs(jobIds: List~int~) : void

        +CreateJob(name: string, source: string, target: string, type: BackupType) : void

        -HandleJobProgress(state: JobState) : void

        -HandleFileCopied(log: LogEntry) : void

    }

    class BackupJob {

        +int Id

        +string Name

        +string SourceDir

        +string TargetDir

        +BackupType Type

        +bool IsActive

        +ValidatePaths() : bool

    }



    class BackupType {

        <<enumeration>>

        Full

        Differential

    }



    class JobStatus {

        <<enumeration>>

        Inactive

        Active

        Completed

        Error

    }



    class JobState {

        +string JobName

        +DateTime LastActionTimestamp

        +JobStatus Status

        +int TotalEligibleFiles

        +long TotalFileSize

        +int FilesRemaining

        +long SizeRemaining

        +double Progression

        +string CurrentSourceFile

        +string CurrentTargetFile

    }

    class BackupStrategyFactory {

        <<Factory>>

        +CreateStrategy(type: BackupType) : BackupStrategy

    }



    class ITransferEngine {

        <<interface - Implementor>>

        +event Action~JobState~ OnProgress

        +CopyFile(source: string, target: string, state: JobState) : void

    }



    class StandardTransferEngine {

        +CopyFile(source: string, target: string, state: JobState) : void

    }



    class StreamedTransferEngine {

        +CopyFile(source: string, target: string, state: JobState) : void

        -CalculateProgress() : void

    }



    class BackupStrategy {

        <<abstract - Abstraction>>

        #ITransferEngine _transferEngine

        +event Action~JobState~ OnStrategyProgress

        +event Action~LogEntry~ OnFileCopied



        +BackupStrategy(engine: ITransferEngine)

        +Execute(job: BackupJob)* : void

    }



    class FullBackupStrategy {

        +Execute(job: BackupJob) : void

    }



    class DifferentialBackupStrategy {

        +Execute(job: BackupJob) : void

    }

    Program --> ConfigManager : creates

    Program --> StateManager : creates

    Program --> ConsoleView : creates

    Program --> BackupViewModel : creates & injects dependencies

    Program --> CliArgumentProcessor : uses

    ConsoleView --> BackupViewModel : triggers actions

    ConsoleView ..> JobState : listens to (Observer)

    BackupViewModel "1" *-- "0..5" BackupJob : manages

    BackupViewModel --> ConfigManager : uses

    BackupViewModel --> StateManager : uses

    BackupViewModel --> IEasyLogger : uses

    BackupViewModel ..> BackupStrategyFactory : requests strategy

    BackupStrategyFactory ..> BackupStrategy : creates

    BackupStrategy o-- ITransferEngine : Bridge (uses)

    BackupStrategy <|-- FullBackupStrategy

    BackupStrategy <|-- DifferentialBackupStrategy



    ITransferEngine <|.. StandardTransferEngine

    ITransferEngine <|.. StreamedTransferEngine

    BackupStrategy ..> BackupViewModel : sends events to

    ITransferEngine ..> BackupStrategy : sends progress to

    BackupViewModel ..> ConsoleView : fires OnJobProgressUpdated---
