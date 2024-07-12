using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWGachaExport.ViewModels.Dialogs
{
    public class InputUrlDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => SetProperty(ref _dialogResult, value);
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public RelayCommand ConfirmCommand => new RelayCommand(() =>
        {
            if (!string.IsNullOrEmpty(Url))
                DialogResult = true;
        });
    }
}
