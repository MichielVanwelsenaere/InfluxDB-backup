using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfluxdbBackup.Interfaces
{
    public interface IDatabaseJob : IJob
    {
        void ValidateEnvironmentVariables();        
    }
}
