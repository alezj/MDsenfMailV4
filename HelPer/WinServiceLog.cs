using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDsenfMailV4.HelPer
{
    public static class WinServiceLog
    {

        public static void WriteLog(string mensaje = "")
        {
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string archivo = "Logs\\MDsSendMail-" + DateTime.Now.Date.ToString("yyyyMMdd") + ".log";
                var ruta = AppDomain.CurrentDomain.BaseDirectory + archivo;
                string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(log))
                {
                    Directory.CreateDirectory(log);
                }
                using (StreamWriter writer = new StreamWriter(ruta, true))
                {
                    if (!string.IsNullOrWhiteSpace(mensaje))
                        writer.WriteLine("Version: " + version + Environment.NewLine + mensaje + " en fecha: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
                    else
                        writer.WriteLine(new string('-', 130) + Environment.NewLine);
                }
            }
            catch
            {

            }
        }
    }
}
