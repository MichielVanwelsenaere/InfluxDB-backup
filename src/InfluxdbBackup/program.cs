using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using InfluxdbBackup.Factories;
using InfluxdbBackup.Ninject;
using Ninject;

namespace InfluxdbBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ConfigurationManager.VerifyMinimalConfiguration();
            }
            catch (System.ArgumentException e)
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Critical, "Failed validating general environment variables: {0}", e.Message.ToString());
                Environment.Exit(-1);
            }

            IKernel kernel = NinjectHandler.InitializeNinjectKernel();
            NinjectHandler.CreateBindings(kernel);

            var scheduler = kernel.Get<IScheduler>();

            //Create the trigger
            ITrigger trigger = null;
            if(Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON").ToLower() == "single")
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Info, "Configuring a single execution trigger for the database job");
                trigger = TriggerBuilder.Create().StartAt(DateTime.Now.AddSeconds(3)).Build();
            }
            else
            {
                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Info, "Configuring a Cron execution trigger for the database job");
                trigger = TriggerBuilder.Create().StartNow().WithCronSchedule(Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON")).Build();
            }

            scheduler.Start().Wait();

            JobKey jobkey = new JobKey("databaseJob");
            scheduler.ScheduleJob(
                JobBuilder.Create<IDatabaseJob>().WithIdentity(jobkey).Build(), 
                trigger).GetAwaiter().GetResult();
            

            // Do tis to prevent the container from exiting
            if(Environment.GetEnvironmentVariable("INFLUXDB_ACTION_CRON").ToLower() == "single")
            {
                do
                {
                    Thread.Sleep(5000);
                } while (scheduler.CheckExists(jobkey).GetAwaiter().GetResult());

                SimpleConsoleLogger.Log(SimpleConsoleLogger.LogLevel.Info, "Single database job execution finished");
                Environment.Exit(0);
            }

            Thread.Sleep(Timeout.Infinite);                
        }
    }
}