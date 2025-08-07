using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MDSendMail.Helper;
using MDsenfMailV4.HelPer;

namespace MDsenfMailV4
{

     partial class MDSenderMailService : ServiceBase
    {

        int IntervaloTimer = int.Parse(ConfigurationManager.AppSettings["intervalo"]);


        bool IsRunning = false;       
        Timer DispatcherTimer = null;
        public MDSenderMailService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WinServiceLog.WriteLog("Iniciando Servicio");
            IniciarServicio();
        }

        public void IniciarServicio()
        {
            IsRunning = false;
            EjecutarColaDeProcesos();

            DispatcherTimer = new Timer();
            DispatcherTimer.Interval = IntervaloTimer;
            DispatcherTimer.Elapsed += DispatcherTimer_Elapsed;
            DispatcherTimer.Enabled = true;
            DispatcherTimer.Start();

        }

        private void DispatcherTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsRunning)
            {
                EjecutarColaDeProcesos();
            }

        }
         void EjecutarColaDeProcesos()
        {
            WinServiceLog.WriteLog("iniciando cola de procesos...");
            EnviarCorreo();
        }

        // protected async Task ExecuteAsync(CancellationToken stoppingToken)
        // protected async Task ExecuteAsync()
             void EnviarCorreo()
        {



            //while (!stoppingToken.IsCancellationRequested)
            //{
                try
                {

                    WinServiceLog.WriteLog($"Iniciando el envio de correos: {DateTimeOffset.Now}");
                    var plantillas = new List<(int Id, string StoredProc, string Asunto, string Destinatarios, string DiasEnvio, TimeSpan HoraEnvio, DateTime? UltimoEnvio)>();
              
                    using (var connection = new SqlConnection(ConfigurationManager.AppSettings["SqlConnection"]))
                    {
                    //await connection.OpenAsync(stoppingToken);
                    connection.Open(); // 👈 Esto es necesario

                    Console.WriteLine("Conexión abierta.");
                    using (var cmd = new SqlCommand("sp_ObtenerCorreo", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            using (var reader =  cmd.ExecuteReader())
                            {
                                while ( reader.Read())
                                {
                                //var id = reader.GetInt32(int.Parse("Id"));
                                    string id = reader["Id"].ToString(); 
                                    string storedProc = reader["StoredProcedure"].ToString();
                                    string asunto = reader["Asunto"].ToString();
                                    string destinatarios = reader["Destinatarios"].ToString();
                                    string diasEnvio = reader["DiasEnvio"].ToString(); // Ej: "L,M,X"
                                    TimeSpan horaEnvio = reader["HoraEnvio"] != DBNull.Value ? (TimeSpan)reader["HoraEnvio"] : TimeSpan.Zero;
                                    DateTime? ultimoEnvio = reader["UltimoEnvio"] != DBNull.Value ? (DateTime?)reader["UltimoEnvio"] : null;



                                    plantillas.Add((int.Parse(id), storedProc, asunto, destinatarios, diasEnvio, horaEnvio, ultimoEnvio));

                                    ////Cuerpo del Correo 
                                    //string cuerpoHtml = "";

                                    ////using (var cmdBody = new SqlCommand(storedProc, connection))
                                    //{
                                    //    // cmdBody.CommandType = CommandType.StoredProcedure;
                                    //    cmd.CommandType = CommandType.StoredProcedure;

                                    //    using (var result = cmd.ExecuteReader())
                                    //    {
                                    //        if (result.Read())
                                    //        {
                                    //            cuerpoHtml = result[0].ToString();
                                    //        }
                                    //    }
                                    //}

                                    //// Lógica de envío con MailKit o SmtpClient
                                    //_sendCorreo.EnviarCorreo(destinatarios, asunto, cuerpoHtml);
                                }

                            }

                            foreach (var plantilla in plantillas)
                            {
                                // Día y hora actual
                                var ahora = DateTime.Now;
                                string diaActual = ahora.ToString("ddd", new System.Globalization.CultureInfo("es-ES")).Substring(0, 1).ToUpper(); // L, M, X, J, V, S, D

                                TimeSpan horaActual = ahora.TimeOfDay;

                                // Leer campos de SQL
                                string diasEnvio = plantilla.DiasEnvio; // Ej: "L,M,X"
                                TimeSpan horaEnvio = plantilla.HoraEnvio;
                                DateTime? ultimoEnvio = plantilla.UltimoEnvio;

                                // Verifica si es el día correcto
                                if (!diasEnvio.Split(',').Contains(diaActual))
                                    continue; // No es el día correcto

                                // Verifica si ya pasó la hora de envío
                                if (horaActual < horaEnvio)
                                    continue; // Aún no es hora

                                // Verifica si ya se envió hoy
                                if (ultimoEnvio.HasValue && ultimoEnvio.Value.Date == ahora.Date)
                                    continue; // Ya se envió hoy

                                // 👉 Si llegó aquí: se puede enviar


                                string cuerpoHtml = "";

                                using (var cmdBody = new SqlCommand(plantilla.StoredProc, connection))
                                {
                                    cmdBody.CommandType = CommandType.StoredProcedure;

                                    using (var result = cmdBody.ExecuteReader())
                                    {
                                        if (result.Read())
                                        {
                                            cuerpoHtml = result[0].ToString();
                                        }
                                    }

                                }

                                SendCorreo.EnviarCorreo(plantilla.Destinatarios, plantilla.Asunto, cuerpoHtml);

                                WinServiceLog.WriteLog($"AsunCorreo enviado: {plantilla.Asunto}");
                            }

                        }

                        //await connection.OpenAsync(stoppingToken);
                        using (var cmd = new SqlCommand("sp_ActualizarPlantillaCorreo", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.ExecuteReader();
                        }
                    }
                    //TODO
                    //_logger.LogInformation("Correos enviados: {time}", DateTimeOffset.Now);
                    WinServiceLog.WriteLog($"Correos enviados: {DateTimeOffset.Now}");




                }
                catch (Exception ex)
                {
                //TODO
                //_logger.LogError(ex, "Error al enviar correos");
                WinServiceLog.WriteLog("Error al enviar correos: " + ex.Message);
                }
                //var parametros = _config.GetSection("Parametros");
                // Espera antes de volver a revisar
                //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                // Task.Delay(TimeSpan.FromMinutes(IntervaloTimer));
               //}
             }



        protected override void OnStop()
        {
            WinServiceLog.WriteLog("Finalizando Servicio...");
        }


    }
}
