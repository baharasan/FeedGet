using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedGet.page
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedPage : ContentPage
    {
        App.Feed feed;
        public FeedPage(App.Feed feed)
        {
            InitializeComponent();
            this.feed = feed;

            label_title.Text = feed.title;
            label_updated_time.Text = feed.updatedate;

            
            int i = 1;
            List<App.Feed_Data> n = new List<App.Feed_Data>();
            foreach (var item in feed.content)
            {
                App.Feed_Data tm = item;
                tm.title = i.ToString() + "." + item.title;
                n.Add(tm);
                i++;
            }
            feed.content = n;

            listView.ItemsSource = feed.content;

            if (feed.lastaclink != "")
            {
                foreach (var item in feed.content)
                {
                    if (item.link == feed.lastaclink)
                    {
                        listView.SelectedItem = item;
                        listView.ScrollTo(listView.SelectedItem, ScrollToPosition.End, false);
                        break;
                    }
                }
            }
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            var x = (App.Feed_Data)e.Item;

            var ng = Path.GetInvalidFileNameChars();
            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
            var filename = this.feed.url;
            foreach (var item1 in ng)
            {
                filename = filename.Replace(item1, ' ');
            }

            this.feed = App.getfeedxml(directory_path + "/" + filename+".txt");
            this.feed.lastaclink = x.link;
            App.setfeedxml(this.feed, directory_path + "/" + filename+".txt");

            var webpage = new WebPage(x);
            Navigation.PushModalAsync(webpage);
            //Application.Current.MainPage = webpage;
        }

    }
}