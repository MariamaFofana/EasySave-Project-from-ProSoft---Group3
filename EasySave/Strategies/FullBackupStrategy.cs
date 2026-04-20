using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : BackupStrategy
    {
        public FullBackupStrategy(ITransferEngine engine) : base(engine)
        {
        }

        public override void Execute(BackupJob job)
        {
        }
    }
}
