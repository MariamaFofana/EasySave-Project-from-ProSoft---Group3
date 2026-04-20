using EasySave.Models;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : BackupStrategy
    {
        public DifferentialBackupStrategy(ITransferEngine engine) : base(engine)
        {
        }

        public override void Execute(BackupJob job)
        {
        }
    }
}
