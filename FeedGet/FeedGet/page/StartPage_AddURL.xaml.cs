﻿using System;
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
    public partial class StartPage_AddURL : ContentPage
    {
        public StartPage_AddURL()
        {
            InitializeComponent();
            updateurllist();
        }

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
                var urll = File.ReadAllLines(directory_path + "/feed_list.txt");
                List<string> ts = new List<string>();
                foreach (var item in urll)
                {
                    if (item.Contains("http"))
                    {
                        ts.Add(item);
                    }
                }

                edit.Text = ts.Count.ToString();
                var ng = Path.GetInvalidFileNameChars();
                foreach (var item in ts)
                {
                    var filename = item + ".txt";
                    foreach (var item1 in ng)
                    {
                        filename = filename.Replace(item1, ' ');
                    }
                    if (File.Exists(directory_path + "/" + filename))
                    {
                        try
                        {
                            var ss = App.getfeedxml(directory_path + "/" + filename);

                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                listView.ItemsSource = ts;
            }
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
            var v = (List<string>)listView.ItemsSource;
            if (v == null)
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
                ee += item + "\r\n";
            }
            File.WriteAllText(directory_path + "/feed_list.txt", ee);
            updateurllist();
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var retult = await DisplayActionSheet("選択してください", "キャンセル", "削除", "コピー");
            //返された文字列でボタンテキストを書き換える
            if (retult== "削除")
            {
                var v = (List<string>)listView.ItemsSource;
                if (v == null)
                {
                    return;
                }
                var z = v.IndexOf(e.Item.ToString());
                try
                {
                    v.RemoveAt(z);
                }
                catch (Exception)
                {

                }
                var directory_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + "feed";
                var ee = "";
                foreach (var item in v)
                {
                    ee += item + "\r\n";
                }
                File.WriteAllText(directory_path + "/feed_list.txt", ee);
                updateurllist();
            }
            else if (retult=="コピー")
            {
                Clipboard.SetTextAsync(e.Item.ToString());
            }
        }
    }
}