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
        void updateurllist()
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


                var l0 = App.carouselPage.Children[0];
                App.carouselPage.Children.Clear();
                App.carouselPage.Children.Add(l0);

                var ng = Path.GetInvalidFileNameChars();                
                foreach (var item in ts)
                {
                    var filename = item + ".txt";
                    foreach (var item1 in ng)
                    {
                        filename = filename.Replace(item1, ' ');
                    }
                    if (File.Exists(directory_path+"/"+filename))
                    {
                        var ss = App.getfeedxml(directory_path + "/" + filename);
                        var c = new page.FeedPage(ss);
                        App.carouselPage.Children.Add(c);
                    }
                }

                listView.ItemsSource = ts;
                btn.Text = "更新";
            }
        }

        public StartPage()
        {
            InitializeComponent();
            Task.Run(() => {
                updateurllist();
            });
        }
        private async void btn_Clicked(object sender, EventArgs e)
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
                await Task.Run(() =>{

                    var html = httpClient.GetStringAsync(item).Result;
                    var filename = Regex.Replace(item, @"http.*//|/|\?", "");
                    File.WriteAllText(directory_path + "/" + filename + ".txt", html);
                    var xDocument = XDocument.Load(directory_path + "/" + filename + ".txt");
                    XNamespace ns = xDocument.Root.GetNamespaceOfPrefix("rdf");
                    App.Feed feed=new App.Feed();
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
                                feed = App.atom(element,ns, item);
                            }
                        }

                    }
                    if (feed.content!=null)
                    {
                        feed.url = item;
                        feeds.Add(feed);
                    }
                });

            }
            var l0 = App.carouselPage.Children[0];
            App.carouselPage.Children.Clear();
            App.carouselPage.Children.Add(l0);
            foreach (var item in feeds)
            {
                var ng=Path.GetInvalidFileNameChars();
                var filename = item.url+".txt";
                foreach (var item1 in ng)
                {
                    filename = filename.Replace( item1,' ');
                }
                if(!File.Exists(directory_path + "/" + filename))
                {
                    App.setfeedxml(item, directory_path + "/" + filename);
                    var c = new page.FeedPage(item);
                    App.carouselPage.Children.Add(c);
                }
                else
                {
                    var oldfeed=App.getfeedxml(directory_path + "/" + filename);
                    DateTime oldTime = DateTime.Parse(oldfeed.updatedate);
                    DateTime newTime = DateTime.Parse(item.updatedate);
                    var x= newTime - oldTime;
                    if (x.TotalSeconds>0)
                    {
                        item.content.Reverse();
                        foreach (var content in item.content) 
                        {
                            var xz = oldfeed.content.Find((a)=>
                            {
                                return a.link==content.link;
                            }) ;

                            if (xz.link==null)
                            {
                                oldfeed.content.Insert(0,content);
                            }
                        }
                        item.content.Reverse();
                        oldfeed.updateda = item.updateda;
                        App.setfeedxml(oldfeed, directory_path + "/" + filename);
                        var c = new page.FeedPage(oldfeed);
                        App.carouselPage.Children.Add(c);
                    }
                    else
                    {
                        var c = new page.FeedPage(item);
                        App.carouselPage.Children.Add(c);
                    }
                }

            }
            btn.Text = "更新";
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (edit.Text == null)
            {
                return;
            }
            if (!edit.Text.Contains("http") | edit.Text == "")
            {
                return;
            }
            var v=(List<string>)listView.ItemsSource;
            if (v==null)
            {
                return;
            }
            if (v.Contains(edit.Text))
            {
                return;
            }
            v.Add(edit.Text);
            edit.Text = ""; 
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            var ee = "";
            foreach (var item in v)
            {
                ee += item+"\r\n";
            }
            File.WriteAllText(directory_path + "/feed_list.txt",ee);
            updateurllist();
        }
    }
}