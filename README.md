# EasySave-Project-from-ProSoft---Group3
Group project in C#/.NET: development of EasySave, backup console application with JSON logs, real-time status, full/differential backups and FR/EN support.
# EasySave V1.0 

EasySave is a professional-grade backup solution engineered by Group 3 for the ProSoft ecosystem. Built on the .NET 8.0 framework, this system provides a robust command-line interface for high-priority data redundancy, featuring real-time state monitoring and standardized logging across multiple environments.

## Architecture and Development Strategy

To ensure long-term maintainability and prepare for the Version 2.0 MVVM transition, the solution is divided into two distinct projects. We utilize a strict feature-branching workflow to keep the main branch stable while developing specialized components in isolation.

### Solution Structure
* EasyLog: A dedicated Dynamic Link Library (DLL) that handles all logging operations to ensure cross-project compatibility within ProSoft.
* EasySave: The primary application, structured using industry-standard patterns including Factories, Models, Services, and ViewModels.

### Branch Roadmap
* main: The source of truth containing only stable, peer-reviewed code.
* feature/data-models: Definition of core structures and job types.
* feature/state-manager: Implementation of real-time status persistence.
* feature/backup-core: Development of the backup logic and the job creation factory.

## Core System Components

### Backup Execution Core
The backup engine is designed for flexibility, allowing the system to distinguish between different copy strategies to optimize storage and time.
* Backup Logic: Dedicated implementations for FullBackupJob and DifferentialBackupJob ensure efficient data handling based on specific user needs.
* Factory Pattern: We implemented a BackupJobFactory to centralize job creation. This pattern secures the instantiation process and ensures data consistency from the moment a job is initialized.

### Data Management and Services
As the project's Data Architect, the focus was on building a transparent and reliable data layer.
* Localization (LanguageManager): A Singleton service managing FR/EN translations, ensuring the tool is ready for ProSoft’s international subsidiaries.
* Real-Time Monitoring (StateManager): This service maintains a state.json file in the AppData folder. It provides a live pulse of active jobs, including file counts, size estimates, and progress percentages, all while using thread-locking to prevent data corruption.

### Standardized Logging (EasyLog)
All file transfers and system actions are captured in real-time by the EasyLogger component. It records high-precision data points including UNC paths, file sizes, and transfer durations measured in milliseconds.

## Getting Started

### Prerequisites
* Environment: Visual Studio 2022 and the .NET 8.0 SDK.
* Installation: Clone the repository and ensure the project references between EasyLog and EasySave are correctly established.
