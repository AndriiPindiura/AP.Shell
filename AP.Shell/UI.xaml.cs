using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Interop;



namespace AP.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UI : Window
    {
        public UI()
        {

            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if (DEBUG)
            {
                //MG.ShowGridLines = true;
            }
#endif
            processListView.Visibility = System.Windows.Visibility.Visible;
        }


        /*private void userListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView items = (ListView)sender;
            ItemsWithIcons item = (ItemsWithIcons)items.SelectedItem;
            if (item != null)
            {
                if (item.LocalResource)
                {
                    Type core = Type.GetType("AP.Shell.Utilities");
                    MethodInfo method = core.GetMethod(item.FileName);
                    if (method != null)
                    {
                        //MessageBox.Show(method.Name);
                        ConstructorInfo coreConstructor = core.GetConstructor(Type.EmptyTypes);
                        object coreObject = coreConstructor.Invoke(new object[] { });
                        method.Invoke(coreObject, null);
                    }
                   
                }
                else
                {
                    using (Process app = new Process())
                    {
                        app.StartInfo.FileName = item.FileName;
                        try
                        {
                            app.Start();
                            ((Core)Application.Current).FillTaskBar();
                        }
                        catch //(Win32Exception ex)
                        {
                            //MessageBox.Show(ex.Message);
                        }
                    }
                }
                items.UnselectAll();
            }

        }*/

        private void adminListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView items = (ListView)sender;
            ItemsWithIcons item = (ItemsWithIcons)items.SelectedItem;
            if (item != null)
            {
                ((Core)Application.Current).AppRun(item, (Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift)));
            }
            items.UnselectAll();
        }
        
        private void processListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView items = (ListView)sender;
            ProcessItems item = (ProcessItems)items.SelectedItem;
            if (item != null)
            {
                if (Mouse.RightButton == MouseButtonState.Pressed)
                {
                    ((Core)Application.Current).CloseApp(item.Id);
                }
                else
                {
                    ((Core)Application.Current).SetFocused(item.Id);
                }
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //((Core)Application.Current).ShowAbout();
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!((Core)Application.Current).Logged)
            {
                if (loginUC.Visibility == System.Windows.Visibility.Hidden)
                {
                    loginUC.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    loginUC.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                ((Core)Application.Current).Logged = false;
            }
        }

        internal void UIForm_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void UIForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (((Core)Application.Current).IsAdmin)
            {
                bool exit = (Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl)) &
                    (Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift)) &
                    (Keyboard.IsKeyDown(Key.LeftAlt) | Keyboard.IsKeyDown(Key.RightAlt)) & Keyboard.IsKeyDown(Key.F4);
                if (exit)
                {
                    this.Topmost = false;
                    ExitConfirmDialog msgbox = new ExitConfirmDialog("Ви впевнені, що бажаєте завершити роботу програми?");
                    if ((bool)msgbox.ShowDialog())
                    {
                        this.Closing -= UIForm_Closing;
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        this.Topmost = true;
                    }

                }
            }
        }

    }
}
