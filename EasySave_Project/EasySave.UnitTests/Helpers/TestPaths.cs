using System.IO;

namespace EasySave.UnitTests.Helpers
{
    public static class TestPaths
    {
        public static string BaseTestDirectory =>
            Path.Combine(Path.GetTempPath(), "EasySaveTests");

        public static string SourceDirectory =>
            Path.Combine(BaseTestDirectory, "Source");

        public static string TargetDirectory =>
            Path.Combine(BaseTestDirectory, "Target");

        public static string LogsDirectory =>
            Path.Combine(BaseTestDirectory, "Logs");

        public static string StateDirectory =>
            Path.Combine(BaseTestDirectory, "State");

        public static string SettingsDirectory =>
            Path.Combine(BaseTestDirectory, "Settings");

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(BaseTestDirectory);
            Directory.CreateDirectory(SourceDirectory);
            Directory.CreateDirectory(TargetDirectory);
            Directory.CreateDirectory(LogsDirectory);
            Directory.CreateDirectory(StateDirectory);
            Directory.CreateDirectory(SettingsDirectory);
        }

        public static void CleanBaseDirectory()
        {
            if (Directory.Exists(BaseTestDirectory))
            {
                Directory.Delete(BaseTestDirectory, true);
            }
        }
    }
}