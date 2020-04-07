using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
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
            private string updateda;
            public string updatedate
            {
                get
                {
                    if (updateda == "")
                    {
                        return "";
                    }
                    DateTime dateTime = DateTime.Parse(updateda);
                    var localTime = dateTime.ToLocalTime();
                    return localTime.ToString();
                }
                set
                {
                    updateda = value;
                }
            }
            private string con;
            public string content
            {
                get
                {
                    //return con;
                    return HttpUtility.HtmlDecode(con);
                }
                set
                {
                    var html = "<html><head><title>desc</title></head><body>" + HttpUtility.HtmlDecode(value) + "</body></html>";

                    htmlSource = new HtmlWebViewSource();
                    htmlSource.Html = html;
                    con = value;
                }
            }
            public HtmlWebViewSource htmlSource { get; set; }
        }
        public struct Feed
        {
            public string title;
            public string link;
            private string updateda;
            public string updatedate
            {
                get
                {
                    if (updateda=="")
                    {
                        return "";
                    }
                    DateTime dateTime = DateTime.Parse(updateda);
                    var localTime = dateTime.ToLocalTime();
                    return localTime.ToString();
                }
                set
                {
                    updateda = value;
                }
            }
            public List<Feed_Data> content;
        }


        static public Feed rss1(XDocument xDocument, XNamespace ns)
        {
            var element = xDocument.Element(ns + "RDF");

            XNamespace nsdef = xDocument.Root.GetDefaultNamespace();
            var channel = element.Element(nsdef + "channel");
            Feed feed=new Feed();
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
                var desc = item.Element(nsdef + "description");
                if (desc==null)
                {
                    feed_Data.content = "-";
                }
                else
                {
                    feed_Data.content = desc.Value;
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

        static public Feed atom(XElement element, XNamespace ns)
        {
            Feed feed = new Feed();
            feed.title = element.Element(ns + "title").Value;
            feed.link = element.Element(ns + "link").Attribute(XName.Get("href")).Value;
            if (feed.title == "")
            {
                feed.title = feed.link;
            }
            var updatedate = element.Element(ns + "updated");
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
                feed.updatedate = element.Element(ns + "entry").Element(ns + "updated").Value;
            }
            List<Feed_Data> list = new List<Feed_Data>();
            foreach (var item in element.Elements(ns + "entry"))
            {
                Feed_Data feed_Data = new Feed_Data();
                feed_Data.title = item.Element(ns + "title").Value;
                feed_Data.link = item.Element(ns + "link").Attribute(XName.Get("href")).Value;
                feed_Data.updatedate = item.Element(ns + "updated").Value;
                var desc = item.Element(ns + "content");
                if (desc != null)
                {
                    feed_Data.content = desc.Value;
                    //desc = item.Element(ns + "summary");
                    //feed_Data.content = desc.Value;
                }
                else
                {
                    desc = item.Element(ns + "summary");
                    if (desc != null)
                    {
                        feed_Data.content = desc.Value;
                    }
                    else
                    {
                        feed_Data.content = "";
                    }
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
