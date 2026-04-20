using System;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class StreamedTransferEngine : ITransferEngine
    {
        public event Action<JobState> OnProgress;

        public void CopyFile(string source, string target, JobState state)
        {
        }

        private void CalculateProgress()
        {
        }
    }
}
