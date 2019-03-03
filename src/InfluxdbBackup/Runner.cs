using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using NLog;
using Quartz;
using System;
using System.Threading;

namespace InfluxdbBackup
{
    class Runner
    {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;

        public Runner(ILogger logger, IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
        }

        public void Start()
        {
            try
            {
                _logger.Info("Validating general environment variables...");
                ConfigurationHelper.VerifyMinimalConfiguration();
                _logger.Info("General environment variables validated succesfully!");
            }
            catch (System.ArgumentException e)
            {
                _logger.Fatal("Failed validating general environment variables: {0}", e.Message.ToString());
                Environment.Exit(-1);
            }

            //Create the trigger
            ITrigger trigger = null;
            if (Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON").ToLower() == "single")
            {
                _logger.Info("Configuring a single execution trigger for the database job");
                trigger = TriggerBuilder.Create().StartAt(DateTime.Now.AddSeconds(3)).Build();

            }
            else
            {
                _logger.Info("Configuring a Cron execution trigger for the database job");
                _logger.Info("Configuring Cron expression: {0}", CronExpressionDescriptor.ExpressionDescriptor.GetDescription(Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON")));
                trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON")).Build();
            }

            _scheduler.Start().Wait();

            JobKey jobkey = new JobKey("databaseJob");
            _scheduler.ScheduleJob(
                JobBuilder.Create<IDatabaseJob>().WithIdentity(jobkey).Build(),
                trigger).GetAwaiter().GetResult();


            // Do tis to prevent the container from exiting
            if (Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON").ToLower() == "single")
            {
                do
                {
                    Thread.Sleep(5000);
                } while (_scheduler.CheckExists(jobkey).GetAwaiter().GetResult());

                _logger.Info("Single database job execution finished");
                Environment.Exit(0);
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}