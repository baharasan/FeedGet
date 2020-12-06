using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

            feed.content.Reverse();


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
                        listView.ScrollTo(listView.SelectedItem, ScrollToPosition.Start, false);
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
            try
            {
                this.feed = App.getfeedxml(directory_path + "/" + filename + ".txt");
            }
            catch (Exception)
            {
                return;
            }
            this.feed.lastaclink = x.link;

            for (int i = 0; i < this.feed.content.Count; i++)
            {
                if (this.feed.lastaclink == this.feed.content[i].link)
                {
                    if (i==0)
                    {
                        this.feed.newfeedco = "";
                        break;
                    }
                    this.feed.newfeedco = i.ToString();
                    break;
                }
            }

            App.setfeedxml(this.feed, directory_path + "/" + filename+".txt");

            var webpage = new WebPage(x);
            //Navigation.PushModalAsync(webpage);
            
            Browser.OpenAsync(x.link, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Default
            });

            //Application.Current.MainPage = webpage;
        }

        //Search
        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            App.navigationPage.PushAsync(new FeedPage_Search(feed));

        }
    }
}