using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedGet.page
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        void updateurllist_navigationPage()
        {
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";

            if (!Directory.Exists(directory_path))
            {
                Directory.CreateDirectory(directory_path);
                App.init(directory_path);
            }

            if (File.Exists(directory_path + "/feed_list.txt"))
            {
                btn.Text = "更新中";
                var urll = File.ReadAllLines(directory_path + "/feed_list.txt");
                List<string> ts = new List<string>();
                foreach (var item in urll)
                {
                    if (item.Contains("http"))
                    {
                        ts.Add(item);
                    }
                }


                var ng = Path.GetInvalidFileNameChars();
                foreach (var item in ts)
                {
                    var filename = item + ".txt";
                    foreach (var item1 in ng)
                    {
                        filename = filename.Replace(item1, ' ');
                    }
                    if (File.Exists(directory_path + "/" + filename))
                    {
                        var ss = App.getfeedxml(directory_path + "/" + filename);
                    }
                }

                btn.Text = "更新";
            }
        }
        async Task btnAsync()
        {

            if (btn.Text == "更新中")
            {
                return;
            }
            btn.Text = "更新中";

            HttpClient httpClient = new HttpClient();

            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            List<App.Feed> feeds = new List<App.Feed>();
            foreach (var item in File.ReadAllLines(directory_path + "/feed_list.txt"))
            {
                if (!item.Contains("http"))
                {
                    continue;
                }
                await Task.Run(() => {

                    var html = httpClient.GetStringAsync(item).Result;
                    var filename = Regex.Replace(item, @"http.*//|/|\?", "");
                    File.WriteAllText(directory_path + "/" + filename + ".txt", html);
                    var xDocument = XDocument.Load(directory_path + "/" + filename + ".txt");
                    XNamespace ns = xDocument.Root.GetNamespaceOfPrefix("rdf");
                    App.Feed feed = new App.Feed();
                    if (ns != null && ns.NamespaceName == "http://www.w3.org/1999/02/22-rdf-syntax-ns#")//rss1.0
                    {
                        var element = xDocument.Element("rss");
                        if (element == null)
                        {
                            feed = App.rss1(xDocument, ns, item);
                        }
                        else
                        {
                            feed = App.rss2(element, item);
                        }
                    }
                    else
                    {
                        var element = xDocument.Element("rss");
                        if (element != null)//rss2.0
                        {
                            if (element.Attribute("version").Value == "2.0")
                            {
                                feed = App.rss2(element, item);
                            }
                        }
                        else
                        {
                            ns = xDocument.Root.GetDefaultNamespace();
                            element = xDocument.Element(ns + "feed");
                            if (element != null)//atom
                            {
                                feed = App.atom(element, ns, item);
                            }
                        }

                    }
                    if (feed.content != null)
                    {
                        feed.url = item;
                        feeds.Add(feed);
                    }
                });

            }
            //var l0 = App.carouselPage.Children[0];
            //App.carouselPage.Children.Clear();
            //App.carouselPage.Children.Add(l0);
            foreach (var item in feeds)
            {
                var ng = Path.GetInvalidFileNameChars();
                var filename = item.url + ".txt";
                foreach (var item1 in ng)
                {
                    filename = filename.Replace(item1, ' ');
                }
                if (!File.Exists(directory_path + "/" + filename))
                {
                    App.setfeedxml(item, directory_path + "/" + filename);
                    var c = new page.FeedPage(item);
                    //App.carouselPage.Children.Add(c);
                }
                else
                {
                    var oldfeed = App.getfeedxml(directory_path + "/" + filename);
                    DateTime oldTime = DateTime.Parse(oldfeed.updatedate);
                    DateTime newTime = DateTime.Parse(item.updatedate);
                    var x = newTime - oldTime;
                    if (x.TotalSeconds > 0)
                    {
                        item.content.Reverse();
                        foreach (var content in item.content)
                        {
                            var xz = oldfeed.content.Find((a) =>
                            {
                                return a.link == content.link;
                            });

                            if (xz.link == null)
                            {
                                oldfeed.content.Insert(0, content);
                            }
                        }
                        item.content.Reverse();
                        oldfeed.updateda = item.updateda;
                        App.setfeedxml(oldfeed, directory_path + "/" + filename);
                        var c = new page.FeedPage(oldfeed);
                        //App.carouselPage.Children.Add(c);
                    }
                    else
                    {
                        var c = new page.FeedPage(item);
                        //App.carouselPage.Children.Add(c);
                    }
                }

            }
            btn.Text = "更新";

        }


        public void resumelist(List<App.Feed> lfeed)
        {
            listView.ItemsSource = lfeed;
        }

        public StartPage()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                updateurllist_navigationPage();
                Thread.Sleep(20000);
                Device.StartTimer(TimeSpan.FromSeconds(60 * 30), () =>
                {
                    btn_Clicked(null, null);
                    return true;
                });
            });

            var ng = Path.GetInvalidFileNameChars();
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            List<App.Feed> lfeeds = new List<App.Feed>();
            while (!File.Exists(directory_path + "/feed_list.txt"))
                continue;
            foreach (var item in File.ReadAllLines(directory_path + "/feed_list.txt"))
            {
                var filename = item + ".txt";
                foreach (var item1 in ng)
                {
                    filename = filename.Replace(item1, ' ');
                }
                if (File.Exists(directory_path + "/" + filename))
                {
                    var oldfeed = App.getfeedxml(directory_path + "/" + filename);
                    lfeeds.Add(oldfeed);

                }
                
            }
            lfeeds.Sort((a, b) =>
            {
                var t = DateTime.Parse(a.updateda) - DateTime.Parse(b.updateda);
                if (t.TotalSeconds < 0)
                {
                    return 1;
                }
                else if (t.TotalSeconds > 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });

            listView.ItemsSource = lfeeds;
            App.startpage_list = lfeeds;

        }
        private async void btn_Clicked(object sender, EventArgs e)
        {
            if (btn.Text != "更新")
            {
                return;
            }
            btn.Text = "更新中";

            HttpClient httpClient = new HttpClient();

            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            List<App.Feed> feeds = new List<App.Feed>();
            List<Task<string>> taskres = new List<Task<string>>();
            foreach (var item in File.ReadAllLines(directory_path + "/feed_list.txt"))
            {
                if (!item.Contains("http"))
                {
                    continue;
                }
                var tsr= Task.Run(() => {

                    var html = "";
                    try
                    {
                        html = httpClient.GetStringAsync(item).Result;
                    }
                    catch (Exception)
                    {
                        return "error"+" "+item;
                    }
                    var filename = Regex.Replace(item, @"http.*//|/|\?", "");
                    File.WriteAllText(directory_path + "/" + filename + ".txt", html);
                    var xDocument = XDocument.Load(directory_path + "/" + filename + ".txt");
                    XNamespace ns = xDocument.Root.GetNamespaceOfPrefix("rdf");
                    App.Feed feed = new App.Feed();
                    if (ns != null && ns.NamespaceName == "http://www.w3.org/1999/02/22-rdf-syntax-ns#")//rss1.0
                    {
                        var element = xDocument.Element("rss");
                        if (element == null)
                        {
                            feed = App.rss1(xDocument, ns, item);
                        }
                        else
                        {
                            feed = App.rss2(element, item);
                        }
                    }
                    else
                    {
                        var element = xDocument.Element("rss");
                        if (element != null)//rss2.0
                        {
                            if (element.Attribute("version").Value == "2.0")
                            {
                                feed = App.rss2(element, item);
                            }
                        }
                        else
                        {
                            ns = xDocument.Root.GetDefaultNamespace();
                            element = xDocument.Element(ns + "feed");
                            if (element != null)//atom
                            {
                                feed = App.atom(element, ns, item);
                            }
                        }

                    }
                    if (feed.content != null)
                    {
                        feed.url = item;
                        feeds.Add(feed);
                    }
                    return item;
                });
                taskres.Add(tsr);
            }
            foreach (var item in taskres)
            {
                btn.Text = await item;

            }
            var lfeed = new List<App.Feed>();
            foreach (var item in feeds)
            {
                var ng = Path.GetInvalidFileNameChars();
                var filename = item.url + ".txt";
                foreach (var item1 in ng)
                {
                    filename = filename.Replace(item1, ' ');
                }
                if (!File.Exists(directory_path + "/" + filename))
                {
                    App.setfeedxml(item, directory_path + "/" + filename);
                    var c = new page.FeedPage(item);
                    lfeed.Add(item);
                }
                else
                {
                    var oldfeed = App.getfeedxml(directory_path + "/" + filename);
                    DateTime oldTime = DateTime.Parse(oldfeed.updatedate);
                    DateTime newTime = DateTime.Parse(item.updatedate);
                    var x = newTime - oldTime;
                    if (x.TotalSeconds > 0)
                    {
                        item.content.Reverse();
                        foreach (var content in item.content)
                        {
                            var xz = oldfeed.content.Find((a) =>
                            {
                                return a.link == content.link;
                            });

                            if (xz.link == null)
                            {
                                oldfeed.content.Insert(0, content);
                            }
                        }
                        item.content.Reverse();
                        oldfeed.updateda = item.updateda;
                        App.setfeedxml(oldfeed, directory_path + "/" + filename);
                        var c = new page.FeedPage(oldfeed);
                        lfeed.Add(item);
                    }
                    else
                    {
                        var c = new page.FeedPage(item);
                        lfeed.Add(item);
                    }
                }

            }
            btn.Text = "更新";
            lfeed.Sort((a, b) => {
                var t = DateTime.Parse(a.updateda)- DateTime.Parse(b.updateda);
                if (t.TotalSeconds<0)
                {
                    return 1;
                }
                else if(t.TotalSeconds > 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }                
            });
            listView.ItemsSource = lfeed;
            App.startpage_list = lfeed;
        }



        private void ToolbarItem_AddURL_Clicked(object sender, EventArgs e)
        {
            App.navigationPage.PushAsync(new StartPage_AddURL());
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            var ng = Path.GetInvalidFileNameChars();
            var t=(App.Feed)e.Item;
            var filename = t.url + ".txt";
            foreach (var item1 in ng)
            {
                filename = filename.Replace(item1, ' ');
            }

            var oldfeed = App.getfeedxml(directory_path + "/" + filename);
            App.navigationPage.PushAsync(new page.FeedPage(oldfeed));

        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Left:
                    // Handle the swipe
                    break;
                case SwipeDirection.Right:
                    // Handle the swipe
                    break;
                case SwipeDirection.Up:
                    // Handle the swipe
                    break;
                case SwipeDirection.Down:
                    btn_Clicked(null, null); 
                    break;
            }
        }

        private void listView_Refreshing(object sender, EventArgs e)
        {
            btn_Clicked(null, null);
            listView.IsRefreshing = false;
        }
    }
}