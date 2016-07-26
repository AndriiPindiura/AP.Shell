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

namespace AP.Shell
{
    /// <summary>
    /// Interaction logic for AdminApps.xaml
    /// </summary>
    public partial class AdminApps : Window
    {
        public AdminApps()
        {
            InitializeComponent();
        }

        #region Metro
        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == System.Windows.WindowState.Normal)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                }
            }
            else
            {
                DragMove();
            }
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
        #endregion


        private void folderView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView items = (ListView)sender;
            ItemsWithIcons item = (ItemsWithIcons)items.SelectedItem;
            items.UnselectAll();
            if (item != null)
            {
                ((Core)Application.Current).AppRun(item, (Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift)));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.Hide();
            //e.Cancel = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (((Core)Application.Current).IsAdmin)
            {
                bool exit = (Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl)) &
                    (Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift)) &
                    (Keyboard.IsKeyDown(Key.LeftAlt) | Keyboard.IsKeyDown(Key.RightAlt)) & Keyboard.IsKeyDown(Key.F4);
                if (exit)
                {
                    ExitConfirmDialog msgbox = new ExitConfirmDialog("Ви впевнені, що бажаєте завершити роботу програми?");
                    if ((bool)msgbox.ShowDialog())
                    {
                        try
                        {
                            Application.Current.Shutdown();
                        }
                        catch
                        {

                        }
                    }
                }
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((Core)Application.Current).Logged = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TransparancyConverter transparancyConverter = new TransparancyConverter(this);
            //transparancyConverter.MakeTransparent();
        }

    }


}
