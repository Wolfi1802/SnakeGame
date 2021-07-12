using SnakeClient.Registre.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SnakeClient.Registre
{
    public class RegistreViewModel : ViewModelBase
    {
        public Action<string> CloseWindow;

        private string _userName;

        public string UserName
        {
            set
            {
                _userName = value;
                base.OnPropertyChanged(nameof(this.UserName));
            }
            get
            {
                return this._userName;
            }
        }

        public ICommand Registre => new RelayCommand(o =>
        {
            if (!string.IsNullOrEmpty(this.UserName) && !this.UserName.Contains(" "))
            {
                this.CloseWindow.Invoke(this.UserName);
            }
            else
            {
                MessageBox.Show("Der Name muss vorhanden sein und er darf keine Leertasten enthalten.");
            }
        });
    }
}
