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
    public partial class WebPage : ContentPage
    {
        public WebPage(App.Feed_Data feed)
        {
            InitializeComponent();
            webview.Source = feed.link;
        }

        private void webview_Navigated(object sender, WebNavigatedEventArgs e)
        {
            edit.Text = e.Url;
        }
    }
}