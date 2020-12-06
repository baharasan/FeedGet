using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        int update_time = 30;

        void listsort(List<App.Feed> lfeed)
        {
            lfeed.Sort((c, d) => {
                var tt = DateTime.Parse(c.updateda) - DateTime.Parse(d.updateda);
                if (tt.TotalSeconds < 0)
                {
                    return 1;
                }
                else if (tt.TotalSeconds > 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });
            var li = new List<int>();
            for (int i = 0; i < lfeed.Count; i++)
            {
                if (lfeed[i].newfeedco == "")
                {
                    li.Add(i);
                }
            }
            foreach (var item in li)
            {
                lfeed.Add(lfeed[item]);
            }
            for (int i = li.Count - 1; i >= 0; i--)
            {
                lfeed.RemoveAt(li[i]);
            }

            listView.ItemsSource = null;
            listView.ItemsSource = lfeed;
            App.startpage_list = lfeed;


        }

        void updateurllist_navigationPage()
        {
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";

            if (!Directory.Exists(directory_path))
            {
                Directory.CreateDirectory(directory_path);
                App.init(directory_path);
            }


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


                var directory_path0 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                if (File.Exists(directory_path0 + "/" + "Configuration/UpdateTime.txt"))
                {
                    var stime=File.ReadAllLines(directory_path0 + "/" + "Configuration/UpdateTime.txt");
                    update_time = int.Parse(stime[0]);
                }

                Device.StartTimer(TimeSpan.FromSeconds(60 * update_time), () =>
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
                    //
                    try
                    {
                        var oldfeed = App.getfeedxml(directory_path + "/" + filename);
                        lfeeds.Add(oldfeed);
                    }
                    catch (Exception)
                    {
                    }

                }
                
            }
            listsort(lfeeds);

        }
        private async void btn_Clicked(object sender, EventArgs e)
        {
            if (label.Text != "")
            {
                return;
            }
            label.Text = "更新中";

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
                try
                {

                    var tsr = Task.Run(() => {

                        var html = "";
                        try
                        {
                            html = httpClient.GetStringAsync(item).Result;
                        }
                        catch (Exception)
                        {
                            return "error" + " " + item;
                        }
                        var filename = Regex.Replace(item, @"http.*//|/|\?", "");

                        html = html.Replace("\v", "%0B");

                        File.WriteAllText(directory_path + "/" + filename + ".txt", html);
                        //
                        XDocument xDocument;
                        try
                        {
                            xDocument = XDocument.Load(directory_path + "/" + filename + ".txt");
                        }
                        catch (Exception)
                        {
                            return "XDocument.Load err " + filename;
                        }
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
                catch (Exception)
                {

                }
            }
            foreach (var item in taskres)
            {
                label.Text = await item;

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
                    var tttf = item;
                    if (item.content.Count!=0)
                    {
                        tttf.lastaclink = item.content[item.content.Count-1].link;
                        tttf.newfeedco = item.content.Count.ToString();
                    }
                    App.setfeedxml(tttf, directory_path + "/" + filename);
                    var c = new page.FeedPage(tttf);
                    lfeed.Add(tttf);
                }
                else
                {
                    App.Feed oldfeed;
                      
                    try
                    {
                        oldfeed = App.getfeedxml(directory_path + "/" + filename);
                    }
                    catch (Exception)
                    {
                        var result = await DisplayAlert("Error", item.title + " のFeed data が壊れています。Feed dataを初期化しますか", "OK", "キャンセル");
                        if (result)
                        {
                            var tttf = item;
                            if (item.content.Count != 0)
                            {
                                tttf.lastaclink = item.content[item.content.Count - 1].link;
                                tttf.newfeedco = item.content.Count.ToString();
                            }
                            App.setfeedxml(tttf, directory_path + "/" + filename);
                            oldfeed = App.getfeedxml(directory_path + "/" + filename);
                        }
                        else
                        {
                            continue;
                        }
                    }
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
                        for (int i = 0; i < oldfeed.content.Count; i++)
                        {
                            if (oldfeed.lastaclink == oldfeed.content[i].link)
                            {
                                oldfeed.newfeedco = i.ToString();
                                break;
                            }
                        }
                        oldfeed.updateda = item.updateda;
                        App.setfeedxml(oldfeed, directory_path + "/" + filename);
                        var c = new page.FeedPage(oldfeed);
                        lfeed.Add(oldfeed);
                        //lfeed.Add(item);
                    }
                    else
                    {
                        var c = new page.FeedPage(oldfeed);
                        lfeed.Add(oldfeed);
                    }
                }

            }

            listsort(lfeed);
            label.Text = "";
        }



        private void ToolbarItem_AddURL_Clicked(object sender, EventArgs e)
        {
            App.navigationPage.PushAsync(new StartPage_AddURL());
        }

        private async void ToolbarItem_Init_Clicked(object sender, EventArgs e)
        {
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";

            if (!Directory.Exists(directory_path))
            {
                return;
            }

            var result = await DisplayAlert("Init", "初期化しますか？", "OK", "キャンセル");
            if (result)
            {

                await Task.Run(() =>
                {
                    while (label.Text != "")
                    {
                        Thread.Sleep(1);
                    }
                });
                string[] filePaths = Directory.GetFiles(directory_path);
                foreach (string filePath in filePaths)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
                string[] directoryPaths = Directory.GetDirectories(directory_path);
                foreach (string directoryPath in directoryPaths)
                {
                    Directory.Delete(directoryPath);
                }
                Directory.Delete(directory_path, false);



                updateurllist_navigationPage();
                listView.ItemsSource = null;
            }

            await DisplayAlert("Init", "初期化しました。", "OK");
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


            EventHandler<NavigationEventArgs> handler = null;
            handler = (a,b) =>
            {
                oldfeed = App.getfeedxml(directory_path + "/" + filename);
                var lfeed = (List<App.Feed>)listView.ItemsSource;
                for (int i = 0; i < lfeed.Count; i++)
                {
                    if (lfeed[i].link == oldfeed.link)
                    {
                        lfeed[i] = oldfeed;
                        break;
                    }
                }

                listsort(lfeed);


                App.navigationPage.Popped -= handler;
            };

            App.navigationPage.Popped += handler;


            App.navigationPage.PushAsync(new page.FeedPage(oldfeed));


        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            App.navigationPage.PushAsync(new FeedAllPage_Search());
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

        private void ToolbarItem_Update_Time_Change(object sender, EventArgs e)
        {
            App.navigationPage.PushAsync(new StartPage_UpdateTime());

        }
    }
}