using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MDsenfMailV4
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // // INICIO DE SERVICIO
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MDSenderMailService()
            };
            ServiceBase.Run(ServicesToRun);

            //// MODO DEBUG
            //var Send = new MDSenderMailService();
            //Send.IniciarServicio();
        }


    }
}
