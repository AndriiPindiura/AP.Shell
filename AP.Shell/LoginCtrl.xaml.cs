using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;

namespace AP.Shell
{
    /// <summary>
    /// Interaction logic for LoginCtrl.xaml
    /// </summary>
    public partial class LoginCtrl : UserControl
    {
        public LoginCtrl()
        {
            InitializeComponent();
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (username.Text.Length > 0 && password.Password.Length > 0)
                {
#if (DEBUG)
                    {
                        //string uid = Guid.NewGuid().ToString();
                        //File.WriteAllText("hash.txt", Security.HashSHA512(password.Password + uid) + Environment.NewLine + uid + Environment.NewLine);
                        //((Core)Application.Current).Auth(username.Text, password.Password);
                    }
#endif
                    ((Core)Application.Current).Logged = ((Core)Application.Current).Auth(username.Text, password.Password);
                }
                password.Clear();
                username.Clear();

            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate()
                {
                    username.Focus();
                }));
            } 

        }

        private void username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                password.Focus();
            }

        }
    }
}
