using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using System.Timers;

namespace K181292_Q2
{
    public partial class Service1 : ServiceBase
    {
        private static List<SyndicationItem> MergedFeeds = new List<SyndicationItem>();
        private static List<NewsItem> NewsItems = new List<NewsItem>();
        private Timer timer = new Timer();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string logDirectory = ConfigurationManager.AppSettings["LogsDirectory"];
                string feedsDirectory = ConfigurationManager.AppSettings["FeedsDirectory"];

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                if (!Directory.Exists(feedsDirectory))
                {
                    Directory.CreateDirectory(feedsDirectory);
                }

                GenerateLogs("RSS Feed Started!");
                Program();

                timer.Interval = 300000; // 5 minutes
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

            GenerateLogs("RSS Feed Stopped!");
        }

        private static void TimerWrapper(object sender, ElapsedEventArgs e)
        {
            Program();
        }

        private static void Program()
        {
            try
            {
                string RSSFeedURL1 = ConfigurationManager.AppSettings["RSSFeedURL1"];
                string RSSFeedURL2 = ConfigurationManager.AppSettings["RSSFeedURL2"];

                foreach (SyndicationItem item in ReadRSSFeed(RSSFeedURL1).Items)
                {
                    MergedFeeds.Add(item);
                }

                foreach (SyndicationItem item in ReadRSSFeed(RSSFeedURL2).Items)
                {
                    MergedFeeds.Add(item);
                }

                MergedFeeds.Sort(CompareDates);

                foreach (SyndicationItem item in MergedFeeds)
                {
                    NewsItems.Add(new NewsItem(item.Title.Text, item.Summary.Text, item.Id.Split('/')[2], item.PublishDate.ToString()));
                }

                WriteXML();

                // After writing to XML - Clearing the lists
                NewsItems.Clear();
                MergedFeeds.Clear();

            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }

        }

        private static SyndicationFeed ReadRSSFeed(string url)
        {
            try
            {
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                GenerateLogs(DateTime.Now.ToString() + " RSS Feeds readed successully!");

                return feed;
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
                return null;
            }
        }
        private static int CompareDates(SyndicationItem x, SyndicationItem y)
        {
            return y.PublishDate.CompareTo(x.PublishDate);
        }

        private static void WriteXML()
        {
            try
            {
                string path = ConfigurationManager.AppSettings["FeedsDirectory"] + "\\" + "K181292_NewsItems.xml";

                XmlSerializer XMLWriter = new XmlSerializer(typeof(List<NewsItem>));
                FileStream file = File.Create(path);

                XMLWriter.Serialize(file, NewsItems);

                GenerateLogs(DateTime.Now.ToString() + " Data has been write to file successully!");

                file.Close();
            }
            catch (Exception e)
            {
                GenerateLogs(e.Message);
            }
        }

        public static void GenerateLogs(string log)
        {
            try
            {
                string path = ConfigurationManager.AppSettings["LogsDirectory"] + '\\' + "k181292_Q2_logs.txt";

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
