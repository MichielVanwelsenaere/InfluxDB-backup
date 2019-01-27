using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfluxdbBackup.DatabaseJobs
{
    internal class GenericDatabaseJob : IDatabaseJob
    {
        private readonly IDatabaseJob _dbjob;

        public GenericDatabaseJob(IDatabaseJob dbjob)
        {
            _dbjob = dbjob;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Info, "Cron triggered, executing database job...");

            try
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Info, "Validating specific database Job environment variables");
                ValidateEnvironmentVariables();
            }
            catch (Exception e)
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Critical, "Validating specific database Job environment variables failed, verify the logs for a root cause: {0}", e.Message.ToString());
            }
            try
            {
                await _dbjob.Execute(context);
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Info, "Database job completed successfully");
            }
            catch (Exception e)
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Critical, "Database job failed, verify the logs for a root cause: {0}", e.Message.ToString());
            }
        }

        public void ValidateEnvironmentVariables()
        {
            _dbjob.ValidateEnvironmentVariables();
        }
    }
}
