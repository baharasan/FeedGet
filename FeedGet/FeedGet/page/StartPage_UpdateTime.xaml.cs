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
    public partial class StartPage_UpdateTime : ContentPage
    {
        public StartPage_UpdateTime()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (edit.Text == null)
            {
                return;
            }
            if (edit.Text == "")
            {
                return;
            }
            if (edit.Text == "0")
            {
                return;
            }
            int s =0;
            try
            {
                s = int.Parse(edit.Text);
            }
            catch (Exception)
            {
                return;
            }

            var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(directory_path + "/" + "Configuration"))
            {
                Directory.CreateDirectory(directory_path + "/" + "Configuration");
            }
            File.WriteAllText(directory_path + "/" + "Configuration/UpdateTime.txt", s.ToString());
            ketei.Text = edit.Text+"分毎に更新するように設定";  
        }
    }
}