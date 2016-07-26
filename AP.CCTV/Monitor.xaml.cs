using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AP.CCTV
{
    /// <summary>
    /// Interaction logic for Monitor.xaml
    /// </summary>
    public partial class Monitor : Window
    {
        private string coreIP;
        private int cameraID;
        private int compressionMode;
        private long userRights;
        private int fps;
        private string title;
        private int continuousArch;
        private DispatcherTimer fpsTimer;
        private List<int> camList;
        public Monitor(string ip, int compress, long rights, int continiuous)
        {
            try
            {
                InitializeComponent();
            }
            catch
            {
                this.Closing -= window_Closing;
                this.Close();
            }
            coreIP = ip;
            compressionMode = compress;
            userRights = rights;
            continuousArch = continiuous;
            camList = new List<int>();
            camList.Clear();
            this.Title = "Підключення до " + coreIP + "...";
            fpsTimer = new DispatcherTimer();
            fpsTimer.Tick += fpsTimer_Tick;
            fpsTimer.Interval = new TimeSpan(0, 0, 1);
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


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ocxITV.CamMenuOptions = (int)userRights;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            ocxITV.Connect(coreIP, "", "", "", continuousArch);
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
        }
        
        private void ocxITV_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            camList.Clear();
            if (e.state == 1)
            {
                this.Title = "Підключено до " + coreIP;
            }
            else
            {
                this.Title = "Неможливо підключитись до " + coreIP;
            }
        }

        private void ocxITV_OnCamListChange(object sender, AxACTIVEXLib._DCamMonitorEvents_OnCamListChangeEvent e)
        {
            if (e.cam_id == -1)
            {
                string command;
                ocxITV.DoReactMonitor("MONITOR||KEY_PRESSED|key<SCREEN.*>");
                title = "Всі камери";

                if (continuousArch == 1)
                {
                    command = "MONITOR||ARCH_FRAME_TIME|cam<{@camera}>,date<" + DateTime.Now.ToString("dd-MM-yy") + ">,time<" + DateTime.Now.ToString("HH:mm:ss") + ">"; 
                }
                else
                {
                    command = "MONITOR||ACTIVATE_CAM|cam<{@camera}>";
                }
                foreach (int camera in camList)
                {
                    ocxITV.DoReactMonitor(command.Replace("{@camera}", camera.ToString()));
                }
                camList.Clear();
                /*ocxITV.ShowCam(cameraID, compressionMode, 1);
                title = "1 камера";
                
                if (showAllCams)
                {
                    ocxITV.DoReactMonitor("MONITOR||KEY_PRESSED|key<SCREEN.*>");
                    title = "Всі камери";
                }*/
                this.Title = title;
                fpsTimer.Start();

            }
            else
            {
                camList.Add(e.cam_id);
                cameraID = e.cam_id;
            }
        }
        
        private void ocxITV_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            ++fps;
        }

        private void fpsTimer_Tick(object sender, EventArgs e)
        {
            this.Title = title + ": (" + fps.ToString() + " к/с)";
            this.Tag = DateTime.Now.ToLongTimeString();
            fps = 0;
            
        }

        private void Layout_Click(object sender, RoutedEventArgs e)
        {
            ocxITV.DoReactMonitor("MONITOR||KEY_PRESSED|key<SCREEN." + ((Button)sender).Tag + ">");
            switch (((Button)sender).Tag.ToString())
            {
                case "1": title = "1 камера";
                    break;
                case "4": title = "(2x2) 4 камери";
                    break;
                case "9": title = "(3x3) 9 камер";
                    break;
                case "16": title = "(4x4) 16 камер";
                    break;
                case "*": title = "Всі камери";
                    break;
            }
            this.Title = title;
        }
        private void Cycle_Click(object sender, RoutedEventArgs e)
        {
            ocxITV.DoReactMonitor("MONITOR||KEY_PRESSED|key<" + ((Button)sender).Tag.ToString() + ">");
        }


        private void window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (ocxITV.IsConnected() == 1)
                {
                    ocxITV.DoReactMonitor("MONITOR||REMOVE_ALL");
                }
                ocxITV.OnConnectStateChanged -= ocxITV_OnConnectStateChanged;
                ocxITV.OnVideoFrame -= ocxITV_OnVideoFrame;
                ocxITV.OnCamListChange -= ocxITV_OnCamListChange;
                fpsTimer.Stop();
                ocxITV.Disconnect();
                ocxITV.Dispose();
                GC.SuppressFinalize(this);
            }
            catch
            {

            }
        }

    }

}
