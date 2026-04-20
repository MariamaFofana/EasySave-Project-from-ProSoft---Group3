using System;

namespace EasySave.Models
{
    public class JobState
    {
        public string JobName { get; set; }
        public DateTime LastActionTimestamp { get; set; }
        public JobStatus Status { get; set; }
        public int TotalEligibleFiles { get; set; }
        public long TotalFileSize { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }
        public double Progression { get; set; }
        public string CurrentSourceFile { get; set; }
        public string CurrentTargetFile { get; set; }
    }
}
