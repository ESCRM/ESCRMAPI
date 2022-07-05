using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace IDS.EBSTCRM.ServerService
{
    static class Program
    {
        static void Main()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Start the service as a function
                // Normally the service is started at:
                // protected override void OnStart(string[] args)
                // But, as it is being protected it cannot be invoked from outside the class
                // So a new function is added call Start - doing what OnStart did.
                NneUpdater nneservice = new NneUpdater();
                nneservice.Start();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new NneUpdater() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
