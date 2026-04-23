# EasySave | Technical Specification

**Version:** 1.0.0  
**Status:** Production Ready  
**Architecture:** Decoupled MVVM Pattern  

## Project Overview

EasySave is a professional-grade backup utility developed for the ProSoft ecosystem. Engineered using C# and .NET 8.0, the application orchestrates high-priority directory redundancy through a command-line interface. It is designed for modularity, featuring real-time telemetry, thread-safe state persistence, and a native bilingual translation engine.

## Solution Architecture (MVVM)

The software is built upon a strict Model-View-ViewModel (MVVM) foundation, ensuring that business logic, data presentation, and user interaction remain entirely decoupled.

### 1. View (Program.cs)
The View serves as the exclusive entry point for user interaction. It is responsible for rendering the console interface and localized menus using the LanguageManager. It remains passive, subscribing to notifications from the ViewModel to display real-time progress while containing no internal business logic.

### 2. ViewModel (MainViewModel.cs)
Acting as the logic mediator, the MainViewModel orchestrates operations between the user interface and the underlying data. It abstracts the Model's complexity, exposing high-level methods for job creation and execution without revealing internal data structures.

### 3. Model & Core Logic
The Model layer represents the application’s core data and backup algorithms, remaining entirely UI-agnostic.
* **Backup Core:** Utilizing the BackupJobFactory, the system dynamically instantiates FullBackupJob or DifferentialBackupJob objects based on user requirements.
* **Copy Engine:** A high-performance module designed for efficient file transfers, utilizing sequential scan hints to minimize memory footprint during massive data migrations.


## Technical Ecosystem

### Standardized Logging (EasyLog DLL)
To comply with ProSoft's cross-project integration standards, all telemetry is handled by a standalone EasyLog library.
* **Daily Telemetry:** Generates a unique log file every 24 hours, capturing file transfer durations (ms), UNC paths, and sizes.
* **Format Flexibility:** Supports JSON and XML exports for seamless ingestion by enterprise monitoring tools.

### Persistence & Localization Services
* **StateManager:** Operates in a dedicated service layer to maintain a state.json file in %AppData%. This tracks the "live pulse" of active jobs—including remaining file counts and percentage completion—using thread-safe locking.
* **LanguageManager:** A Singleton-based service that provides dynamic, dictionary-driven translation. It allows the application to swap between English and French without requiring a re-compilation.


## Development Standards

### Coding Conventions
To ensure maintainability across ProSoft teams, the following C# standards are mandatory:
* **Public Members:** PascalCase for all classes, methods, and properties.
* **Local Scope:** camelCase for variables and parameters.
* **Private Fields:** Prefix with an underscore (e.g., _stateLock).


## Deployment

### Build Instructions
1. Ensure .NET 8.0 SDK is installed.
2. Clone the repository: `git clone <repo-url>`.
3. Restore dependencies and build: `dotnet build`.
4. Run the application: `dotnet run --project EasySave`.

## Authors
**ProSoft Development Group 3**
STEPHEN Jessica
GAYTE Virgil
JUANICO Maximilien
FOFANA Mariama
