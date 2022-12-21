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

namespace itemzflow_companion.Views
{
    public partial class InstallerPage : ContentPage
    {
        bool file_selected = false;
        private string GetIPAddress()
        {
            string iniFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IP.txt");
            // Set the path to the INI file'
            if (!File.Exists(iniFilePath))
                return "IP Address here";

            // Open the stream and read it back.    
            using (StreamReader sr = File.OpenText(iniFilePath))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    return s;
                }
            }

            return "IP Address here";
        }
        public InstallerPage()
        {
            InitializeComponent();
            tes.Text = "Select a Homebrew PKG to continue";
            IP_Addr.Text = GetIPAddress();
        }
        static String BytesToString(long byteCount)
        {
            string[] suf = { " Bs", " KBs", " MBs", " GBs", " TBs", " PBs", " EBs" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
        public string ftp_file_path;
        async void OpenFileSelector(System.Object sender, System.EventArgs e)
        {

            var file = await FilePicker.PickAsync(PickOptions.Default);
            if (file != null)
            {
                // Text = $"File Name: {file.FileName}";
                if (file.FileName.EndsWith("pkg", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("PKG: " + $"{file.FullPath}");
                    FileInfo fi = new FileInfo(file.FullPath);
                    ftp_file_path = file.FullPath;
                    tes.Text = "Selected PKG: " + file.FileName.Substring(0, Math.Min(15, file.FileName.Length)).ToString()  + "\n" + "Size: " + BytesToString(fi.Length);
                    UploadButton.IsEnabled = file_selected = true;
                    ActivityIndicator activityIndicator = new ActivityIndicator { IsRunning = true };

                }
            }
        }
        private void SaveIPAddress(string ipAddress)
        {
            // Set the path to the INI file
            string iniFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IP.txt");
            // Create a new file     
            using (FileStream fs = File.Create(iniFilePath))
            {
                // Add some text to file    
                Byte[] settt = new UTF8Encoding(true).GetBytes(ipAddress);
                fs.Write(settt, 0, settt.Length);
                fs.Close();
            }
            
        }
        async void OpenFTPUpload(System.Object sender, System.EventArgs e)
        {
            // Text = $"File Name: {file.FileName}";
          IPAddress ip;
          if (!IPAddress.TryParse(IP_Addr.Text, out ip))
          {
                await DisplayAlert("ERROR", "IP Address is invaild!", "OK");
                return;
          }
          SaveIPAddress(IP_Addr.Text);
          if (file_selected && ftp_file_path.EndsWith("pkg", StringComparison.OrdinalIgnoreCase))
          {

                try
                {
                    UploadButton.IsEnabled = false;
                    ai.IsRunning = true;
                    UpText.Text = "Connecting ...";
                    _ = Task.Run(() =>
                        {
                            using (var ftp = new FtpClient(IP_Addr.Text, "ftp", "ftp"))
                            {
                                if (ftp == null)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        UploadButton.IsEnabled = true;
                                        DisplayAlert("ERROR", "Failed to connect to the PS4! Is Itemzflow running?", "OK");

                                    });
                                    return;
                                }
                                ftp.Config.ConnectTimeout = 6000;
                                ftp.Config.ReadTimeout = 6000;
                                try
                                {
                                    ftp.Connect();
                                }
                                catch (TimeoutException)
                                {
                                    Console.WriteLine("timed out");
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        UploadButton.IsEnabled = true;
                                        ai.IsRunning = false;
                                        UpText.Text = "Failed to connect!";
                                        DisplayAlert("ERROR", "Failed to connect to the PS4! Is Itemzflow running?", "OK");

                                    });
                                    return;
                                }

                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    progressBar.IsVisible = true;
                                    ai.IsRunning = false;
                                    UpText.Text = "Uploading PKG ...";
                                });
                                

                                // define the progress tracking callback
                                Action<FtpProgress> progress = delegate (FtpProgress p)
                                {
                                    if (p.Progress != 100)
                                    {
                                        // percent done = (p.Progress * 100)
                                        Device.BeginInvokeOnMainThread(() =>
                                      {
                                          progressBar.Progress = p.Progress * 0.01;
                                      });
                                        Console.WriteLine("Prog: " + p.Progress);
                                    }
                                };

                                // upload a file with progress tracking
                                FtpStatus err = ftp.UploadFile(ftp_file_path, "/user/app/temp.pkg", FtpRemoteExists.Overwrite, false, FtpVerify.None, progress);
                                // all done!
                                if (err == FtpStatus.Failed)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DisplayAlert("Failed!", "The PKG has failed to Upload", "OK");
                                    });
                                }
                                else if (err == FtpStatus.Success)
                                {
                                    Console.WriteLine("Installing PKG on PS4");
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        UpText.Text = "Installing PKG ...";
                                        UploadButton.IsEnabled = ai.IsVisible = ai.IsRunning = true;
                                        progressBar.IsVisible = false;
                                    });
                                    FtpReply er = ftp.Execute("installpkg");
                                    ftp.Disconnect();
                                    if (er.Code == "200")
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            ai.IsRunning = ai.IsVisible = false;
                                            UploadButton.IsEnabled = true;
                                            UpText.Text = "PKG Installed Successfully!";
                                            DisplayAlert("Success!", "PKG has been Installed Successfully", "OK");
                                        });
                                    }
                                    else
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            ai.IsRunning = ai.IsVisible = false;
                                            UploadButton.IsEnabled = true;
                                            UpText.Text = "Install Failed!";
                                            DisplayAlert("Failed!", "The PKG has failed to Install", "OK");
                                        });
                                    }
                                }
                                else
                                    Console.WriteLine("skipped ig");

                            }
                        });
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        UploadButton.IsEnabled = true;
                        DisplayAlert("ERROR", "Failed to connect to the PS4! Is Itemzflow running?", "OK");
                    });
                }

          }
          else
          {
                UploadButton.IsEnabled = true;
                await DisplayAlert("ERROR", "Selected file isnt a PKG????", "OK");

          }

        }
    }
}