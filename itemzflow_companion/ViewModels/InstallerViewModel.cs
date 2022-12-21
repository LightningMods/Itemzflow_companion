using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace itemzflow_companion.ViewModels
{
    public class InstallerViewModel : BaseViewModel
    {
        public InstallerViewModel()
        {
            Title = "PKG Installer";
        }

        public ICommand OpenWebCommand { get; }
    }
}