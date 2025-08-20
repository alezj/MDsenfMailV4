
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;

namespace MDSendMail.Helper
{
    public static class SendCorreo
    {
        //private readonly IConfiguration _config;

        //public SendCorreo(IConfiguration config)
        //{
        //    _config = config;
        //}

        public static void EnviarCorreo(string destinatarios, string asunto, string cuerpoHtml)
        {

            var smtpConfig = (NameValueCollection)ConfigurationManager.GetSection("Smtp");


             var smtp = new SmtpClient(smtpConfig["Host"], int.Parse(smtpConfig["Port"]))
            {
                Credentials = new NetworkCredential(smtpConfig["User"], smtpConfig["Password"]),
                EnableSsl = bool.Parse(smtpConfig["EnableSsl"])
            };

            var correo = new MailMessage
            {
                From = new MailAddress(smtpConfig["User"]),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            foreach (var destinatario in destinatarios.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(destinatario))
                    correo.To.Add(destinatario.Trim());
            }
            smtp.Send(correo);
        }

        public static bool DebeEnviar(DateTime? ultimoEnvio, DateTime ahora, int intervaloMinutos)
        {
            // Si nunca se ha enviado, debe enviar
            if (!ultimoEnvio.HasValue)
                return true;

            // Calcula diferencia en minutos
            double minutosTranscurridos = (ahora - ultimoEnvio.Value).TotalMinutes;

            // Retorna true si ya pasó el intervalo
            return minutosTranscurridos >= intervaloMinutos;
        }

    }
}
