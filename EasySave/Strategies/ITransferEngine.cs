using System;
using EasySave.Models;

namespace EasySave.Strategies
{
    public interface ITransferEngine
    {
        event Action<JobState> OnProgress;

        void CopyFile(string source, string target, JobState state);
    }
}
