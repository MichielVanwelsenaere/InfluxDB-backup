using InfluxdbBackup.Ninject;
using Ninject;
using NLog;
using Quartz;

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