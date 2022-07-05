using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace IDS.EBSTCRM.ServerService
{
    [RunInstaller(true)]
    public class InstallerForServices : Installer
    {
        private ServiceProcessInstaller processInst;
        private ServiceInstaller serviceInstNne;

        public InstallerForServices()
        {
            processInst = new ServiceProcessInstaller();
            processInst.Account = ServiceAccount.LocalSystem;
            Installers.Add(processInst);

            serviceInstNne = new ServiceInstaller();
            serviceInstNne.StartType = ServiceStartMode.Automatic;
            serviceInstNne.DelayedAutoStart = true;
            serviceInstNne.Description = "Update EBST CRM company info from NNE";
            serviceInstNne.ServiceName = "NneUpdater";
            serviceInstNne.ServicesDependedOn = new string[] {"Netman"}; //,"Netlogon"};
            Installers.Add(serviceInstNne);
        }
    }
}
