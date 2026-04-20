namespace EasyLog_DLL
{
    public class Logger : IEasyLogger
    {
        private static Logger _instance;
        private string _logDirectory;

        private Logger()
        {
        }

        public static Logger GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }
            return _instance;
        }

        public void LogAction(LogEntry entry)
        {
        }

        private void WriteJsonEntry(LogEntry entry, string path)
        {
        }
    }
}
