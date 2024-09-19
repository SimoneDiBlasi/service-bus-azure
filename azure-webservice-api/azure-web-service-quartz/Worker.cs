using Microsoft.Extensions.Hosting;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azure_web_service_quartz_worker
{
    public class Worker : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Job eseguito: " + DateTime.Now);
            // Logica del job
            return Task.CompletedTask;
        }

    }
}
