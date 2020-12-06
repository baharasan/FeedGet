using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

            Device.BeginInvokeOnMainThread(async () => {
                await Browser.OpenAsync(feed.link, new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Default
                });

                await Navigation.PopAsync();
            });


            //webview.Source = feed.link;
        }

        private void webview_Navigated(object sender, WebNavigatedEventArgs e)
        {
            edit.Text = e.Url;
        }

        private void edit_Focused(object sender, FocusEventArgs e)
        {
            Clipboard.SetTextAsync(edit.Text);
        }
    }
}