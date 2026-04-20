using EasySave.Models;

namespace EasySave.Core
{
    public class StateManager
    {
        private string _stateFilePath;

        public StateManager(string path)
        {
            _stateFilePath = path;
        }

        public void UpdateState(JobState state)
        {
        }
    }
}
