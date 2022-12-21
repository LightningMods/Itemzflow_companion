using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace itemzflow_companion.ViewModels
{
    public class Sys_ControlsViewModel : BaseViewModel
    {
        public Sys_ControlsViewModel()
        {
            Title = "System Controls";
        }

        public ICommand OpenWebCommand { get; }
    }
}