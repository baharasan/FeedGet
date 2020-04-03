using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedGet
{
    public partial class App : Application
    {

        static public void init(string directory_path)
        {
            File.WriteAllText(directory_path + "/feed_list.txt", @"//rss1.0
https://www.4gamer.net/rss/index.xml

//rss2.0
https://rss.itmedia.co.jp/rss/2.0/itmedia_all.xml
https://gigazine.net/news/rss_2.0/");
        }


        public struct Feed_Data
        {
            public string title { get; set; }
            public string link { get; set; }
            public string updatedate { get; set; }
            public string content { get; set; }
        }
        public struct Feed
        {
            public string title;
            public string link;
            public string updatedate;
            public List<Feed_Data> content;
        }


        static public Feed rss1(XDocument xDocument, XNamespace ns)
        {
            var element = xDocument.Element(ns + "RDF");

            XNamespace nsdef = xDocument.Root.GetDefaultNamespace();
            var channel = element.Element(nsdef + "channel");
            Feed feed;
            feed.title = channel.Element(nsdef + "title").Value;
            feed.link = channel.Element(nsdef + "link").Value;

            XNamespace nsdc = xDocument.Root.GetNamespaceOfPrefix("dc");
            var date = channel.Element(nsdc + "date");
            if (date == null)
            {
                feed.updatedate = element.Element(nsdef + "item").Element(nsdc + "date").Value;
            }
            else
            {
                feed.updatedate = date.Value;
                if (feed.updatedate == "")
                {
                    feed.updatedate = element.Element(nsdef + "item").Element(nsdc + "date").Value;
                }
            }
            List<Feed_Data> list = new List<Feed_Data>();
            foreach (var item in element.Elements(nsdef + "item"))
            {
                Feed_Data feed_Data = new Feed_Data();
                feed_Data.title = item.Element(nsdef + "title").Value;
                feed_Data.link = item.Element(nsdef + "link").Value;
                feed_Data.updatedate = item.Element(nsdc + "date").Value;
                feed_Data.content = item.Element(nsdef + "description").Value;
                if (feed_Data.content == "")
                {
                    feed_Data.content = "-";
                }
                list.Add(feed_Data);
            }
            feed.content = list;
            return feed;
        }

        static public Feed rss2(XElement element)
        {
            var channel = element.Element("channel");
            Feed feed = new Feed();
            feed.title = channel.Element("title").Value;
            feed.link = channel.Element("link").Value;
            if (feed.title == "")
            {
                feed.title = channel.Element("description").Value;
            }
            var updatedate = channel.Element("pubDate");
            if (updatedate != null)
            {
                feed.updatedate = updatedate.Value;
            }
            else
            {
                feed.updatedate = "";
            }
            if (feed.updatedate == "")
            {
                feed.updatedate = channel.Element("item").Element("pubDate").Value;
            }
            List<Feed_Data> list = new List<Feed_Data>();
            foreach (var item in channel.Elements("item"))
            {
                Feed_Data feed_Data = new Feed_Data();
                feed_Data.title = item.Element("title").Value;
                feed_Data.link = item.Element("link").Value;
                feed_Data.updatedate = item.Element("pubDate").Value;
                var desc = item.Element("description");
                if (desc != null)
                {
                    feed_Data.content = desc.Value;
                }
                else
                {
                    feed_Data.content = "";
                }
                if (feed_Data.content == "")
                {
                    feed_Data.content = "-";
                }
                list.Add(feed_Data);
            }
            feed.content = list;
            return feed;
        }

        static public void atom(XElement element)
        {
            //
            return;
        }

        static public CarouselPage carouselPage;


        public App()
        {
            InitializeComponent();
            var carouselPage0 = new CarouselPage()
            {
                Children =
                {
                    new page.StartPage()
                }
            };
            carouselPage = carouselPage0;
            MainPage = carouselPage0;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
