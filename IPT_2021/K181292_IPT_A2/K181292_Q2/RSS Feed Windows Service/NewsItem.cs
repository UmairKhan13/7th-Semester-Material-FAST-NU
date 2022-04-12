using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K181292_Q2
{
    public class NewsItem

    {
        public string title { get; set; }
        public string description { get; set; }
        public string newsChannel { get; set; }
        public string publishedDate { get; set; }

        public NewsItem()
        {

        }
        public NewsItem(string title, string description, string newsChannel, string publishedDate)
        {
            this.title = title;
            this.description = description;
            this.newsChannel = newsChannel;
            this.publishedDate = publishedDate;
        }
    }
}
