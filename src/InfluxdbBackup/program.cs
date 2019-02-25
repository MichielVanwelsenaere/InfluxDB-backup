using InfluxdbBackup.Helpers;
using InfluxdbBackup.Interfaces;
using InfluxdbBackup.Ninject;
using Ninject;
using NLog;
using NLog.Extensions.Logging;
using Quartz;
using System;
using System.Threading;

namespace InfluxdbBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = NinjectHandler.InitializeNinjectKernel();
            NinjectHandler.CreateBindings(kernel);

            ILogger logger = kernel.Get<ILogger>();
            var _scheduler = kernel.Get<IScheduler>();
            var runner = new Runner(logger, _scheduler);
            runner.Start();
        }
    }
}