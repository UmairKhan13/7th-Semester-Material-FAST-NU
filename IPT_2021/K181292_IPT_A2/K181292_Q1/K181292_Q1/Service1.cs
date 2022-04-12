using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Timers;

namespace K181292_Q1
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string logsDirectory = ConfigurationManager.AppSettings["LogsDirectory"];

                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }


                GenerateLogs(DateTime.Now.ToString() + " EmailSender service is Started!");

                Program();

                timer.Interval = 900000; // 15 minutes
                timer.Elapsed += new ElapsedEventHandler(TimerWrapper);
                timer.Enabled = true;
                timer.Start();
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }

        }

        protected override void OnStop()
        {
            timer.Enabled = false;
            timer.Dispose();

            GenerateLogs(DateTime.Now.ToString() + " EmailSender service is Stopped!");
        }
        private void TimerWrapper(object sender, ElapsedEventArgs e)
        {
            Program();
        }

        private void Program()
        {
            try
            {
                List<Email> emails = ReadJSON();
                foreach (Email item in emails)
                {
                    SendEmail(item);
                }

            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }
        }
        private void SendEmail(Email email)
        {
            try
            {
                string senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
                string senderPassword = ConfigurationManager.AppSettings["SenderPassword"];

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(senderEmail);
                mail.To.Add(email.To);
                mail.Subject = email.Subject;

                mail.IsBodyHtml = true;
                mail.Body = email.MessageBody;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new NetworkCredential(senderEmail, senderPassword);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                GenerateLogs(DateTime.Now.ToString() + " Email is sent to " + email.To);

            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }

        }

        private List<Email> ReadJSON()
        {
            try
            {
                string EmailsSource = ConfigurationManager.AppSettings["EmailsDirectory"];
                List<Email> Emails = new List<Email>();

                foreach (string file in Directory.EnumerateFiles(EmailsSource, "*.json"))
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        var JSONdata = reader.ReadToEnd();
                        Emails.Add(JsonConvert.DeserializeObject<Email>(JSONdata));
                    }
                }

                return Emails;
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
                return null;
            }

        }

        public static void GenerateLogs(string log)
        {
            try
            {
                string path = ConfigurationManager.AppSettings["LogsDirectory"] + '\\' + "k181292_Q1_logs.txt";

                if (!File.Exists(path))
                {
                    using (StreamWriter writer = File.CreateText(path))
                    {
                        writer.WriteLine(log);
                    }
                }
                else
                {
                    using (StreamWriter writer = File.AppendText(path))
                    {
                        writer.WriteLine(log);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
