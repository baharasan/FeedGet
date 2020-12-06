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
    public partial class FeedPage_Search : ContentPage
    {
        App.Feed feed;
        public FeedPage_Search(App.Feed feed)
        {
            InitializeComponent();
            this.feed = feed; 
            

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
            List<App.Feed_Data> nfeed=new List<App.Feed_Data>();
            int i = 1;
            foreach (var item in feed.content)
            {
                App.Feed_Data tm = item;

                if (tm.title.IndexOf(e.NewTextValue, StringComparison.OrdinalIgnoreCase) >= 0 | tm.content.IndexOf(e.NewTextValue, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tm.title = i.ToString() + "." + item.title;
                    nfeed.Add(tm);
                    i++;
                }


            }
            listView.ItemsSource = nfeed;
        }
    }
}