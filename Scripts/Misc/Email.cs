using System;
using System.Text.RegularExpressions;
using System.Threading;
using MimeKit;

namespace Server.Misc
{
    public class Email
    {
        /* In order to support emailing, fill in EmailServer and FromAddress:
        * Example:
        *  public static readonly string EmailServer = "mail.domain.com";
        *  public static readonly string FromAddress = "servuo@domain.com";
        * 
        * If you want to add crash reporting emailing, fill in CrashAddresses:
        * Example:
        *  public static readonly string CrashAddresses = "first@email.here,second@email.here,third@email.here";
        * 
        * If you want to add speech log page emailing, fill in SpeechLogPageAddresses:
        * Example:
        *  public static readonly string SpeechLogPageAddresses = "first@email.here,second@email.here,third@email.here";
        */
        public static readonly string EmailServer = Config.Get("Email.EmailServer", default(string));
        public static readonly int EmailPort = Config.Get("Email.EmailPort", 25);
        public static readonly bool EmailSsl = Config.Get("Email.EmailSsl", false);

        public static readonly string FromAddress = Config.Get("Email.FromAddress", default(string));
        public static readonly string CrashAddresses = Config.Get("Email.CrashAddresses", default(string));
        public static readonly string SpeechLogPageAddresses = Config.Get("Email.SpeechLogPageAddresses", default(string));

        public static readonly string EmailUsername = Config.Get("Email.EmailUsername", default(string));
        public static readonly string EmailPassword = Config.Get("Email.EmailPassword", default(string));

        private static readonly Regex _pattern = new Regex(@"^[a-z0-9.+_-]+@([a-z0-9-]+\.)+[a-z]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static MailKit.Net.Smtp.SmtpClient _Client;

        public static bool IsValid(string address)
        {
            if (address == null || address.Length > 320)
                return false;

            return _pattern.IsMatch(address);
        }

        public static void Configure()
        {
            if (EmailServer != null && EmailUsername != null)
            {
                _Client = new MailKit.Net.Smtp.SmtpClient();
            }
        }

        public static bool Send(MimeMessage message)
        {
            try
            {
                lock (_Client)
                {
                    if (EmailServer != null && EmailUsername != null)
                    {
                        /* Optional Use
                         * using MailKit.Security;
                         * _Client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                         * _Client.Connect(EmailServer, EmailPort, SecureSocketOptions.StartTls);
                        */

                        _Client.Connect(EmailServer, EmailPort, EmailSsl);
                        _Client.Authenticate(EmailUsername, EmailPassword);
                        _Client.Send(message);
                        _Client.Disconnect(true);
                    }
                }
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
                return false;
            }

            return true;
        }

        public static void AsyncSend(MimeMessage message)
        {
            ThreadPool.QueueUserWorkItem(SendCallback, message);
        }

        private static void SendCallback(object state)
        {
            MimeMessage message = (MimeMessage)state;

            if (Send(message))
                Console.WriteLine("Sent e-mail '{0}' to '{1}'.", message.Subject, message.To);
            else
                Console.WriteLine("Failure sending e-mail '{0}' to '{1}'.", message.Subject, message.To);
        }
    }
}
