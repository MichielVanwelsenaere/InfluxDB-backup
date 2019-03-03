using Quartz;

namespace InfluxdbBackup.Interfaces
{
    public interface IDatabaseJob : IJob
    {
        void ValidateEnvironmentVariables();        
    }
}
