namespace EasySave.Models
{
    public class BackupJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SourceDir { get; set; }
        public string TargetDir { get; set; }
        public BackupType Type { get; set; }
        public bool IsActive { get; set; }

        public bool ValidatePaths()
        {
            return false;
        }
    }
}
