using System;
using System.ComponentModel;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FluentFTP;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Input;

namespace itemzflow_companion.Views
{
    public partial class aboutPage : ContentPage
    {
        public aboutPage()
        {
            InitializeComponent();

        }
        void OnPKGZone(object sender, EventArgs args)
        {
           Browser.OpenAsync("https://pkg-zone.com");
        }

        void OpenDiscord(object sender, EventArgs args)
        {
          Browser.OpenAsync("https://discord.gg/fcUg6yP5Gy");
        }

        void OpenKofi(object sender, EventArgs args)
        {
           Browser.OpenAsync("https://ko-fi.com/lightningmods");
        }

    }
}