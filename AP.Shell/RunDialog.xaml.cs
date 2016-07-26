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
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Threading;

namespace AP.Shell
{
    /// <summary>
    /// Interaction logic for RunDialog.xaml
    /// </summary>
    public partial class RunDialog : Window
    {
        public RunDialog(int left, int top)
        {
            InitializeComponent();
            this.Left = left;
            this.Top = top;
        }
        #region Metro
        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
                DragMove();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDialog run = new OpenFileDialog();
            run.FileOk += run_FileOk;
            run.ShowDialog();
        }

        void run_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            runcmd.Text = ((OpenFileDialog)sender).FileName;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (runcmd.Text.Length > 0)
            {
                using (Process app = new Process())
                {
                    app.StartInfo.FileName = runcmd.Text;
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        app.StartInfo.Verb = "runas";
                    }
                    try
                    {
                        app.Start();
                    }
                    catch //(Win32Exception ex)
                    {

                        //MessageBox.Show(ex.Message);
                    }
                }
                this.Close();
            }
        }

        private void runcmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (runcmd.Text.Length > 0)
            {
                runbtn.IsEnabled = true;
            }
            else
            {
                runbtn.IsEnabled = false;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate()
                {
                    runcmd.Focus();
                }));

        }

    }
}
