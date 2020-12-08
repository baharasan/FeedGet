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
https://pc.watch.impress.co.jp/data/rss/1.0/pcw/feed.rdf
https://www.4gamer.net/rss/index.xml
https://forest.watch.impress.co.jp/data/rss/1.0/wf/feed.rdf
https://tech.nikkeibp.co.jp/rss/xtech-it.rdf
http://feeds.japan.cnet.com/rss/cnet/all.rdf
https://www.watch.impress.co.jp/vr/vr.xml
https://northwood.blog.fc2.com/?xml=
http://maya.indyzone.jp/feed/
http://c-imp.sblo.jp/index.rdf
http://masafumi.cocolog-nifty.com/masafumis_diary/index.rdf
https://www.gamespark.jp/rss/index.rdf

https://ada211.blogspot.com/rss.xml

//rss2.0
https://rss.itmedia.co.jp/rss/2.0/itmedia_all.xml
https://gigazine.net/news/rss_2.0/
http://www.oshiete-kun.net/index.xml
https://dev.classmethod.jp/feed/
http://zerogram.info/?feed=rss2
https://codezine.jp/rss/new/20/index.xml
https://www.pixivision.net/ja/rss
https://framesynthesis.jp/tech/index.xml
http://h.javtorrent.re/feed/
http://www.anime-sharing.com/forum/external.php?type=RSS2&forumids=47
https://www.reddit.com/user/FitGirlLV/.rss
http://imoue.hatenablog.com/rss
http://www.rlslog.net/category/games/pc/feed/
https://gihyo.jp/feed/rss2

https://rss.itmedia.co.jp/rss/2.0/techfactory.xml
https://rss.itmedia.co.jp/rss/2.0/techtarget.xml
https://rss.itmedia.co.jp/rss/2.0/keymans.xml
https://rss.itmedia.co.jp/rss/2.0/ait.xml
https://rss.itmedia.co.jp/rss/2.0/business.xml
https://rss.itmedia.co.jp/rss/2.0/pcuser.xml
https://rss.itmedia.co.jp/rss/2.0/mobile.xml

https://jp.techcrunch.com/feed

https://haxnode.com/feed/
https://www.downloadpirate.com/feed/
https://www.iiicg.com/feed

https://www.moguravr.com/feed
https://automaton-media.com/feed/

https://3dnchu.com/feed/

https://ascii.jp/tech/rss.xml
https://ascii.jp/digital/rss.xml
https://ascii.jp/hobby/rss.xml
https://ascii.jp/pc/rss.xml

https://www.aeblender.com/feed/
http://iiidea.cn/feed
http://www.gfxcamp.com/feed/
https://godownloads.net/feed/

//atom
https://www.publickey1.jp/atom.xml
http://monsho.hatenablog.com/feed
http://wololo.net/feed/
http://feeds.feedburner.com/giveawayoftheday/xFAb
http://gamestorrent.co/feed





https://www.famitsu.com/rss/fcom_all.rdf





");
        }


        public struct Feed_Data
        {
            public string title { get; set; }
            public string link { get; set; }
            public string updateda;
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

                    var im = Regex.Match(value, @"<img.*?src=.*?>");
                    if (im.Length == 0)
                    {
                        im = Regex.Match(value, @"(i|I)mage: ?http.*?(\.png|\.gif|\.jpg|\.jpeg)");
                        if (im.Length != 0)
                        {
                            image = Regex.Replace(im.Value, @"(i|I)mage: ?", "");
                        }
                    }
                    else
                    {
                        im=Regex.Match(im.Value, "src=('|\").*?('|\")");
                        image = Regex.Replace(im.Value, "src=|('|\")", "");
                    }
                    con = value;
                }
            }
            public HtmlWebViewSource htmlSource { get; set; }
            public string image { get; set; }
        }
        public struct Feed
        {
            public string url { get; set; }
            public string title { get; set; }
            public string link { get; set; }
            public string lastaclink { get; set; }
            public string newfeedco { get; set; }
            public string updateda;
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


        static public Feed rss1(XDocument xDocument, XNamespace ns,string url)
        {
            var element = xDocument.Element(ns + "RDF");

            XNamespace nsdef = xDocument.Root.GetDefaultNamespace();
            var channel = element.Element(nsdef + "channel");
            Feed feed=new Feed();
            feed.url = url;
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
            feed.updatedate = element.Element(nsdef + "item").Element(nsdc + "date").Value;
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

        static public Feed rss2(XElement element, string url)
        {
            var channel = element.Element("channel");
            Feed feed = new Feed();
            feed.url = url;
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
                var lastBuildDate = channel.Element("lastBuildDate");
                if (lastBuildDate != null)
                {
                    feed.updatedate = lastBuildDate.Value;
                }
                else
                {
                    feed.updatedate = "";
                }
            }
            if (feed.updatedate == "")
            {
                feed.updatedate = channel.Element("item").Element("pubDate").Value;
            }
            feed.updatedate = channel.Element("item").Element("pubDate").Value;
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

        static public Feed atom(XElement element, XNamespace ns,string url)
        {
            Feed feed = new Feed();
            feed.url = url;
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
            feed.updatedate = element.Element(ns + "entry").Element(ns + "updated").Value;
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

        static public Feed getfeedxml(string filename)
        {
            Feed feed = new Feed();
            XDocument xDocument;
            try
            {
                xDocument = XDocument.Load(filename);
            }
            catch (Exception)
            {
                throw;
            }
            var element = xDocument.Element("Feed");
            feed.updatedate = element.Element("updatedate").Value;
            feed.url = element.Element("url").Value;
            feed.title = element.Element("title").Value;
            feed.link = element.Element("link").Value;
            feed.lastaclink = element.Element("lastaclink").Value;
            feed.newfeedco = element.Element("newfeedco").Value;
            var zzz = element.Elements("Feed_Data");
            List<Feed_Data> feed_s = new List<Feed_Data>();
            foreach (var item in zzz)
            {
                Feed_Data feed_data = new Feed_Data();
                feed_data.updatedate = item.Element("updatedate").Value;
                feed_data.title = item.Element("title").Value;
                feed_data.link = item.Element("link").Value;
                feed_data.content = item.Element("content").Value;
                feed_s.Add(feed_data);
            }
            feed.content = feed_s;
            return feed;
        }

        static public void setfeedxml(Feed feed, string filename)
        {
            var xDocument0 = new XDocument();
            XDeclaration xmlDeclaration = new XDeclaration("1.0", "utf-8", "yes");
            xDocument0.Declaration = xmlDeclaration;
            XElement xeFeed = new XElement("Feed");
            xeFeed.Add(new XElement("url", feed.url));
            xeFeed.Add(new XElement("title", feed.title));
            xeFeed.Add(new XElement("link", feed.link));
            xeFeed.Add(new XElement("lastaclink", feed.lastaclink));
            xeFeed.Add(new XElement("newfeedco", feed.newfeedco));
            xeFeed.Add(new XElement("updatedate", feed.updateda));
            foreach (var item in feed.content)
            {
                XElement xeFeedDate = new XElement("Feed_Data");
                xeFeedDate.Add(new XElement("title", item.title));
                xeFeedDate.Add(new XElement("link", item.link));
                xeFeedDate.Add(new XElement("updatedate", item.updateda));
                xeFeedDate.Add(new XElement("content", item.content));
                xeFeed.Add(xeFeedDate);
            }
            xDocument0.Add(xeFeed);
            xDocument0.Save(filename);
        }


        /*
        static public CarouselPage carouselPage;
        static public IList<ContentPage> lc;
        */

        static public NavigationPage navigationPage;


        static public List<Feed> startpage_list;
        static public page.StartPage startpage;

        public App()
        {
            InitializeComponent();
            /*
            var carouselPage0 = new CarouselPage()
            {
                Children =
                {
                    new page.StartPage()
                }
            };
            carouselPage = carouselPage0;
            MainPage = carouselPage0;
            */
            navigationPage = new NavigationPage();
            startpage = new page.StartPage();
            navigationPage.PushAsync(startpage);
            MainPage = navigationPage;
        }

        protected override void OnStart()
        {
            /*
            if (lc!=null)
            {
                carouselPage.Children.Clear();
                foreach (var item in lc)
                {
                    carouselPage.Children.Add(item);
                }
            }
            */
            if (startpage != null)
            {
                if (startpage_list != null)
                {
                    startpage.resumelist(startpage_list);
                }
            }
            return;
        }

        protected override void OnSleep()
        {
            //lc = carouselPage.Children;
        }

        protected override void OnResume()
        {
        }
    }
}
