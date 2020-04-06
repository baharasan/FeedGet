using System;
using System.Collections.Generic;
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
        public FeedPage(App.Feed feed)
        {
            InitializeComponent();
            label_title.Text = feed.title;
            label_updated_time.Text = feed.updatedate;
            listView.ItemsSource = feed.content;
        }

        private void OnItemTapped(object sender, EventArgs e)
        {
            var f = (Xamarin.Forms.ItemTappedEventArgs)e;
            var x = (App.Feed_Data)f.Item;
            var webpage = new WebPage(x);
            Navigation.PushModalAsync(webpage);
            //Application.Current.MainPage = webpage;
        }

    }
}