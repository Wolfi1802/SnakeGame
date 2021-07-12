using SnakeClient.Registre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SNAKE.Registre
{
    /// <summary>
    /// Interaktionslogik für AddPlayerNameWindow.xaml
    /// </summary>
    public partial class AddPlayerNameWindow : Window
    {
        public AddPlayerNameWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            RegistreViewModel vm = new RegistreViewModel();

            vm.CloseWindow += (string userName) =>
            {
                mainWindow.PlayerName = userName;
                this.Close();
            };

            this.DataContext = vm;
        }
    }
}
