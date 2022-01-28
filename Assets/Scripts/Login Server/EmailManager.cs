using System;
using System.Net;
using System.Net.Mail;

using DarkRift;

namespace LoginServer
{
    public class EmailManager
    {
        public static void SendEmail(string subject, string content, string mailTo)
        {
            if (!Config.Enable_Emailing_System) return;

            try
            {
                if (!mailTo.Contains("@") || !mailTo.Contains(".") || string.IsNullOrEmpty(mailTo)) return;
                if (string.IsNullOrEmpty(content)) return;
                if (string.IsNullOrEmpty(subject)) return;

                MailMessage mail = new MailMessage();
                SmtpClient client = new SmtpClient(Config.SMTP_Client);

                mail.From = new MailAddress(Config.Mail_From);
                mail.To.Add(mailTo);
                mail.Subject = subject;
                mail.Body = content;

                client.Port = Config.SMTP_Port;
                client.Credentials = new NetworkCredential(Config.Mail_From, Config.Mail_From_Password);
                client.EnableSsl = Config.Email_Enable_SSL;

                client.Send(mail);
            }
            catch (Exception ex) { Server.getInstance.Log("Error on sending an email to: " + mailTo + " with error: " + ex.Message, LogType.Error); }
        }
    }
}