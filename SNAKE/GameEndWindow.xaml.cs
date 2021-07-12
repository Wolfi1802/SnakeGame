using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SNAKE
{
    /// <summary>
    /// Interaktionslogik für GameEndWindow.xaml
    /// </summary>
    public partial class GameEndWindow : Window
    {
        public GameEndWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.InitEvents();

        }

        public event EventHandler CloseClicked;
        public event EventHandler RestartClicked;


        #region events
        private void InitEvents()
        {
            this.CloseGameButton.Click += OnCloseButtonClick;
            this.RestartGameButton.Click += OnRestartbuttonClick;
        }

        private void OnRestartbuttonClick(object sender, RoutedEventArgs e)
        {
            this.RaiseRestartClicked();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.RaiseCloseClicked();
        }

        private void RaiseCloseClicked()
        {
            if (this.CloseClicked != null)
            {
                this.CloseClicked(this, new EventArgs());
            }
            else
                Debug.WriteLine($"Aufruf von {nameof(this.CloseClicked)} fehlgeschlagen!");
        }

        private void RaiseRestartClicked()
        {
            if (this.RestartClicked != null)
            {
                this.RestartClicked(this, new EventArgs());
            }
            else
                Debug.WriteLine($"Aufruf von {nameof(this.RestartClicked)} fehlgeschlagen!");
        }
        #endregion
    }
}
