using EasySave.UnitTests.Helpers;
using System;

namespace EasySave.UnitTests.Fixtures
{
    public class TemporaryDirectoryFixture : IDisposable
    {
        public TemporaryDirectoryFixture()
        {
            TestPaths.CleanBaseDirectory();
            TestPaths.EnsureDirectoriesExist();
        }

        public void Dispose()
        {
            TestPaths.CleanBaseDirectory();
        }
    }
}