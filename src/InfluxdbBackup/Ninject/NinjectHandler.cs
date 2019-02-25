﻿using InfluxdbBackup.BackupMedium;
using InfluxdbBackup.DatabaseJobs;
using InfluxdbBackup.Factories;
using InfluxdbBackup.Interfaces;
using Ninject;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfluxdbBackup.Ninject
{
    internal static class NinjectHandler
    {
        internal static IKernel InitializeNinjectKernel()
        {
            var kernel = new StandardKernel();
            return kernel;
        }

        internal static void CreateBindings(IKernel kernel)
        {
            // setup Quartz scheduler that uses our NinjectJobFactory
            kernel.Bind<IScheduler>().ToMethod(x =>
            {
                var sched = new StdSchedulerFactory().GetScheduler().Result;
                sched.JobFactory = new QuartzJobFactory(kernel);
                return sched;
            });

            switch (Environment.GetEnvironmentVariable("INFLUXDB_BACKUPMEDIUM").ToLower())
            {
                case "azureblob":
                    kernel.Bind<IBackupMedium>().To<AzureBlob>();
                    break;
                default:
                    throw new ArgumentException("backup medium type not found!");
            }

            switch (Environment.GetEnvironmentVariable("INFLUXDB_ACTION").ToLower())
            {
                case "fullbackup":
                    kernel.Bind<IDatabaseJob>().To<FullBackupJob>();
                    break;
                case "restorefullbackup":
                    kernel.Bind<IDatabaseJob>().To<RestoreFullBackupJob>();
                    break;
                default:
                    throw new ArgumentException("database job type not found!");
            }


            
        }
    }
}