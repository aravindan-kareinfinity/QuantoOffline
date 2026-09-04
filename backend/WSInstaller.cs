using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace PrinterServer
{
    [RunInstaller(true)]
    public class WSInstaller : System.Configuration.Install.Installer
    {
        public WSInstaller()
        {
            ServiceProcessInstaller process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            ServiceInstaller serviceAdmin = new ServiceInstaller();
            serviceAdmin.StartType = ServiceStartMode.Automatic;
            serviceAdmin.ServiceName = "Quanto-Client-Service";
            serviceAdmin.DisplayName = "Quanto-Client-Service";
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            //this.Context.Parameters["assemblypath"]
            base.OnAfterInstall(savedState);
        }
    }


}
