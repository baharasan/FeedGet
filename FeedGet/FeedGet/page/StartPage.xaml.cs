using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedGet.page
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
        }
        private void btn_Clicked(object sender, EventArgs e)
        {
            HttpClient httpClient = new HttpClient();

            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";

            if (!Directory.Exists(directory_path))
            {
                Directory.CreateDirectory(directory_path);
                App.init(directory_path);
            }
            foreach (var item in File.ReadAllLines(directory_path + "/feed_list.txt"))
            {
                if (!item.Contains("http"))
                {
                    continue;
                }
                var html = httpClient.GetStringAsync(item).Result;

                var filename = Regex.Replace(item, @"http.*//|/|\?", "");
                File.WriteAllText(directory_path + "/" + filename + ".txt", html);
                var xDocument = XDocument.Load(directory_path + "/" + filename + ".txt");
                XNamespace ns = xDocument.Root.GetNamespaceOfPrefix("rdf");
                if (ns != null && ns.NamespaceName == "http://www.w3.org/1999/02/22-rdf-syntax-ns#")//rss1.0
                {
                    var element = xDocument.Element("rss");
                    if (element == null)
                    {
                        var feed = App.rss1(xDocument, ns);
                        var c = new page.FeedPage(feed);
                        App.carouselPage.Children.Add(c);                        
                    }
                    else
                    {
                        var feed = App.rss2(element);
                        var c = new page.FeedPage(feed);
                        App.carouselPage.Children.Add(c);
                    }
                }
                else
                {
                    var element = xDocument.Element("rss");
                    if (element != null)//rss2.0
                    {
                        if (element.Attribute("version").Value == "2.0")
                        {
                            var feed = App.rss2(element);
                            var c = new page.FeedPage(feed);
                            App.carouselPage.Children.Add(c);
                        }
                    }
                    else
                    {
                        ns = xDocument.Root.GetDefaultNamespace();
                        element = xDocument.Element(ns + "feed");
                        if (element != null)//atom
                        {
                            App.atom(element);
                        }
                    }

                }
            }

        }

    }
}