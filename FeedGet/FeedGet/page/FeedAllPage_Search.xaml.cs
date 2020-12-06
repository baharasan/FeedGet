using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedGet.page
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedAllPage_Search : ContentPage
    {
        public FeedAllPage_Search()
        {
            InitializeComponent();
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            var x = (App.Feed_Data)e.Item;


            var webpage = new WebPage(x);
            Navigation.PushModalAsync(webpage);
            //Application.Current.MainPage = webpage;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            List<App.Feed> feeds = new List<App.Feed>();
            List<Task<string>> taskres = new List<Task<string>>();


            List<App.Feed_Data> nfeed = new List<App.Feed_Data>();
            int i = 1;

            foreach (var item0 in File.ReadAllLines(directory_path + "/feed_list.txt"))
            {
                if (!item0.Contains("http"))
                {
                    continue;
                }
                try
                {
                    var ng = Path.GetInvalidFileNameChars();

                    var filename = item0 + ".txt";
                    foreach (var item1 in ng)
                    {
                        filename = filename.Replace(item1, ' ');
                    }
                    if (File.Exists(directory_path + "/" + filename))
                    {
                        try
                        {
                            var ss = App.getfeedxml(directory_path + "/" + filename);
                            foreach (var item in ss.content)
                            {
                                App.Feed_Data tm = item;

                                if (tm.title.IndexOf(e.NewTextValue, StringComparison.OrdinalIgnoreCase) >= 0 | tm.content.IndexOf(e.NewTextValue, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    tm.title = i.ToString() + "." + item.title;
                                    nfeed.Add(tm);
                                    i++;
                                }


                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            listView.ItemsSource = nfeed;
        }
    }
}