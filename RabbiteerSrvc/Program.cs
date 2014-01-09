using System;
using System.ServiceProcess;

namespace Rabbiteer
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
#if (DEBUG)
            // debug run
            Log.EnableDebug = true;
            RabbiteerService srvc = new RabbiteerService();
            srvc.DoStart();
 #else
            // start as service
            Log.IsEventLog = true;
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new RabbiteerService() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
