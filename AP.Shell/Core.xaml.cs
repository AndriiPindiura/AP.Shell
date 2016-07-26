using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.Xml.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Reflection;
using System.ComponentModel;

namespace AP.Shell
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class Core : Application
    {
        #region Variables
        private UI mainUI;
        private AdminApps adminUI;
        private RunDialog rd;
        private ObservableCollection<ItemsWithIcons> UserAppList { get; set; }
        private ObservableCollection<ItemsWithIcons> AdminAppList { get; set; }
        private ObservableCollection<ItemsWithIcons> FolderAppList { get; set; }
        private bool watchDogActive = true;
        private bool logged;
        private bool shelladmin;
        private string[] adminapps;
        private IntPtr mainUIHandle;
        private IntPtr adminUIHandle;
        private Thread processbar;
        private int taskbarHandle;

        /*private IntPtr usertoken;
        private WindowsImpersonationContext userprofile;
        private string currentUser;
        private string currentPassword;*/
        #endregion

        #region DLLImport
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
       
        [DllImport("user32.dll")]
        static extern int PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static bool CloseHandle(IntPtr handle);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SHGetFileInfo(string pszPath, 
            uint dwFileAttributes, 
            out SHFILEINFO psfi, 
            uint cbFileInfo,
            uint uFlags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        #endregion

        #region Const
        const uint WM_CLOSE = 0x10;
        const uint WM_GETICON = 0x007f;
        IntPtr ICON_SMALL2 = new IntPtr(2);
        IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
        const int GCL_HICON = -14;

        const uint SHGFI_ICON = 0x000000100;
        const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        const uint SHGFI_OPENICON = 0x000000002;
        const uint SHGFI_SMALLICON = 0x000000001;
        const uint SHGFI_LARGEICON = 0x000000000;
        const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SHFILEINFO
        {
            internal IntPtr hIcon;
            internal int iIcon;
            internal uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            internal string szTypeName;
        };

        private enum FolderType
        {
            Closed,
            Open
        }

        private enum IconSize
        {
            Large,
            Small
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };


        #endregion
        #region Public Methods

        public Core()
        {
            mainUI = new UI();
            taskbarHandle = FindWindow("Shell_TrayWnd", "");
            ShowWindow(taskbarHandle, 0);
            this.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            UserAppList = new ObservableCollection<ItemsWithIcons>();
            AdminAppList = new ObservableCollection<ItemsWithIcons>();
            FolderAppList = new ObservableCollection<ItemsWithIcons>();
            CtrAltDelEnable(false);
            adminapps = new string[0];
            FillUserApps();
            mainUI.Show();
            mainUIHandle = new WindowInteropHelper(mainUI).Handle;
            AppsStartup();
            processbar = new Thread(fillprocess);
            processbar.IsBackground = true;
            processbar.Start();
            
        }

        public void FillTaskBar()
        {
            if (mainUI == null)
            {
                return;
            }
            ObservableCollection<ProcessItems> currentapps = new ObservableCollection<ProcessItems>();
            Dispatcher.BeginInvoke(
            DispatcherPriority.ContextIdle,
            new Action(delegate()
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if ((IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) != 0) && (hWnd != mainUIHandle))
                    {
                        uint process;
                        GetWindowThreadProcessId(hWnd, out process);
                        if (process == Process.GetCurrentProcess().Id)
                        {

                            /*Window wnd = Application.Current.Windows.Cast<Window>().Where(w => w.Title.Contains(GetWindowText(hWnd))).First();
                            if (wnd != null)
                            {
                                currentapps.Add(new ProcessItems(wnd.Icon, wnd.Title, new WindowInteropHelper(wnd).Handle, true));
                            }*/
                            foreach (Window wnd in Application.Current.Windows)
                            {
                                if (new WindowInteropHelper(wnd).Handle == hWnd)
                                    currentapps.Add(new ProcessItems(wnd.Icon, wnd.Title, new WindowInteropHelper(wnd).Handle, true));
                            }
                            /*if (hWnd == adminUIHandle)
                            {
                                currentapps.Add(new ProcessItems(adminUI.Icon, GetWindowText(hWnd), hWnd, true));
                                
                            }*/
                        }
                        else
                        {
                            IntPtr handle = GetWindowIcon(hWnd);
                            if (handle != IntPtr.Zero)
                            {
                                Icon ico = Icon.FromHandle(handle);
                                ImageSource img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                currentapps.Add(new ProcessItems(img, GetWindowText(hWnd), hWnd, true));
                                DestroyIcon(handle);
                                DestroyIcon(ico.Handle);
                                ico.Dispose();
                           }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
                this.mainUI.processListView.ItemsSource = currentapps;
            }));
        }

        public void SetFocused(IntPtr wnd)
        {
            SwitchToThisWindow(wnd, true);
            //SetForegroundWindow(wnd);
            foreach(Window lWnd in Application.Current.Windows)
            {
                if (wnd == new WindowInteropHelper(lWnd).Handle)
                    lWnd.WindowState = WindowState.Normal;
            }
            //SetForegroundWindow(wnd);
        }

        public bool Logged
        {
            get { return logged; }
            set
            {
                logged = value;
                if (logged)
                {
                    mainUI.loginUC.Visibility = Visibility.Hidden;
                }
                else
                {
                    shelladmin = false;
                    AdminAppList.Clear();
                    FolderAppList.Clear();
                    FillUserApps();
                    if (adminUI != null)
                    {
                        adminUI.Close();
                    }
                }
                CtrAltDelEnable(shelladmin);

            }
        }
        
        public bool IsAdmin
        {
            get { return shelladmin; }
            private set { shelladmin = value; }
        }

        public void BrowseFolder(string folder)
        {
            //adminUI = new AdminApps();
            if (adminUI == null)
            {
                adminUI = new AdminApps();
            }
            else
            {
                
            }
            if (!adminUI.IsVisible)
            {
                adminUI.Show();
            }
            else
            {
                adminUI.Focus();
            }
            adminUIHandle = IntPtr.Zero;
            adminUIHandle = new WindowInteropHelper(adminUI).Handle;
            FillFolderView(folder);
            //mainUI.MG.Children.Add(adminUI);
            //adminUI.ShowDialog();
        }

        public void CloseApp(IntPtr handle)
        {
            //SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            PostMessage(handle, 0x0010, 0, 0);
            FillTaskBar();
        }

        public bool Auth(string username, string password)
        {
            #region WinAPI Auth
            IntPtr usertoken = IntPtr.Zero;
            string name;
            bool result = LogonUser(username, Environment.MachineName, password, 2, 0, out usertoken);
            if (result)
            {
                using (WindowsImpersonationContext userprofile = WindowsIdentity.Impersonate(usertoken))
                {
                    WindowsPrincipal isadmin = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    shelladmin = isadmin.IsInRole("AP");
                    name = Environment.UserName;
                    userprofile.Undo();
                }
                if (!Directory.Exists(GetUserAppDataPath() + "\\apps\\" + name))
                {
                    try
                    {
                        Directory.CreateDirectory(GetUserAppDataPath() + "\\apps\\" + name);
                    }
                    catch
                    {

                    }
                }
                if (adminUI != null)
                {
                    adminUI.Close();
                }
                adminUI = new AdminApps();
                FillAdminApps(name);
                if (AdminAppList.Count > 0)
                {
                    adminUI.Show();
                }
                else
                {
                    adminUI.Close();
                    Logged = false;
                }
                //mainUI.adminListView.Visibility = Visibility.Visible;
            }
            return result;
            #endregion

            #region XML Auth
            /*bool result = false;
            using (DataSet apdata = new DataSet())
            {
                apdata.ReadXml("APData.xml", XmlReadMode.Auto);
                using (DataTable users = apdata.Tables["UsersTable"])
                {
                    try
                    {
                        DataRow row = users.Select("username='" + username + "'").First();
                        if (String.Equals(row[1].ToString(), Security.HashSHA512(password + row[2].ToString())))
                        {
                            shelladmin = Convert.ToBoolean(row[3].ToString());
                            if (String.IsNullOrEmpty(row[4].ToString()) | String.IsNullOrWhiteSpace(row[4].ToString()))
                            {
                                adminapps = new string[0];
                            }
                            else
                            {
                                adminapps = row[4].ToString().Split(';');
                            }
                            result = true;
                        }
                    }
                    catch 
                    {
                    }
                }
            }
            return result;*/
            #endregion

            #region .NET Auth
            /*bool result = false;
            using (PrincipalContext pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
            {
                //MessageBox.Show(pc.ConnectedServer);
                result = pc.ValidateCredentials(username, password, ContextOptions.Negotiate);
                if (result)
                {
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(pc, "AP");
                    UserPrincipal user = UserPrincipal.FindByIdentity(pc, username);
                    shelladmin = user.IsMemberOf(group);
                }
                else
                {
                    //MessageBox.Show(username.Text + Environment.NewLine + password.Password);
                }
                return result;
            }*/
            #endregion
        }

        public void AppRun(ItemsWithIcons item, bool runas)
        {
            if (item.LocalResource)
            {
                MethodInfo method = (this.GetType()).GetMethod(item.FileName);
                if (method != null)
                {
                    method.Invoke(this, null);
                }
            }
            else
            {
                FileAttributes attr = File.GetAttributes(item.FileName);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    BrowseFolder(item.FileName);
                }
                else
                {
                    using (Process app = new Process())
                    {
                        app.StartInfo.FileName = item.FileName;
                        if (runas)
                        {
                            app.StartInfo.Verb = "runas";
                        }
                        try
                        {
                            app.Start();
                            FillTaskBar();
                        }
                        catch (Win32Exception ex)
                        {
                            if (ex.NativeErrorCode != 1223)
                            {
                                app.StartInfo.FileName = item.FileName;
                                app.StartInfo.Verb = "runasuser";
                                try
                                {
                                    app.Start();
                                }
                                catch (Exception aex)
                                {
                                    MessageBox.Show(aex.Message + Environment.NewLine + app.StartInfo.FileName + Environment.NewLine + app.StartInfo.Verb);
                                } // cath aex
                            } //if not 1223
                        } // catch run error
                    } // using process
                } // else (if directory)
            }

        }

        public void LiveView()
        {
            try
            {
#if DEBUG
            {
            //new CCTV.Client("10.1.11.101", 1, 0x00000001 ^ 0x00000008 ^ 0x00000010 ^ 0x00000020 ^ 0x00000040, true);
            }
#endif
               CCTV.Client monitor = new CCTV.Client();
               monitor.Connect("127.0.0.1", 1, 0, 0);
            }
            catch
            {

            }
        }

        public void RunCmd()
        {
            if (shelladmin)
            {
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                if (rd != null)
                {
                    rd.Close();
                }
                rd = new RunDialog(w32Mouse.X, w32Mouse.Y);
                rd.Show();
            }
        }

        public void AdminFolder()
        {
            this.adminUI.folderView.ItemsSource = AdminAppList;
            if (shelladmin)
            {
                this.adminUI.Title = "Режим адміністратора";
                this.adminUI.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/shield.ico"));
            }
            else
            {
                this.adminUI.Title = "Режим розширеного функціоналу";
                this.adminUI.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/users.ico"));
            }
        }

        #endregion

        #region Private Methods

        protected override void OnExit(ExitEventArgs e)
        {
            CtrAltDelEnable(true);
            ShowWindow(taskbarHandle, 1);
        }

        private static Icon GetFolderIcon(IconSize size, FolderType folderType, string Path, uint dwFileAttributes)
        {
            // Need to add size check, although errors generated at present!    
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (FolderType.Open == folderType)
            {
                flags += SHGFI_OPENICON;
            }
            if (IconSize.Small == size)
            {
                flags += SHGFI_SMALLICON;
            }
            else
            {
                flags += SHGFI_LARGEICON;
            }
            // Get the folder icon    
            var shfi = new SHFILEINFO();

            var res = SHGetFileInfo(Path,
                dwFileAttributes,
                out shfi,
                (uint)Marshal.SizeOf(shfi),
                flags);

            if (res == IntPtr.Zero)
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());

            // Load the icon from an HICON handle  
            Icon.FromHandle(shfi.hIcon);

            // Now clone the icon, so that it can be successfully stored in an ImageList
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            DestroyIcon(shfi.hIcon);        // Cleanup    

            return icon;
        }

        private string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);
            return sb.ToString(0, len);
        }

        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr((long)GetClassLong32(hWnd, nIndex));
            else
                return GetClassLong64(hWnd, nIndex);
        }

        private static IntPtr GetWindowIcon(IntPtr hWnd)
        {
            try
            {
                IntPtr hIcon = default(IntPtr);

                hIcon = SendMessage(hWnd, WM_GETICON, new IntPtr(1), IntPtr.Zero);

                if (hIcon == IntPtr.Zero)
                    hIcon = GetClassLongPtr(hWnd, GCL_HICON);

                if (hIcon == IntPtr.Zero)
                    hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);

                return hIcon;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private void CtrAltDelEnable(bool yes)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser;
                key = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies", true);
                key.CreateSubKey("System");
                key = key.OpenSubKey("System", true);
                key.SetValue("DisableTaskMgr", yes ? 0 : 1, RegistryValueKind.DWord);
            }
            catch
            {
            }

        }

        private void AppsStartup()
        {
            string watchDogPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(watchDogPath))
            {
                DirectoryInfo watchDogDir = new DirectoryInfo(watchDogPath);
                foreach (FileInfo file in watchDogDir.GetFiles())
                {
                    if (!file.Name.Contains(".ini"))
                    {
                        Process watchDog = new Process();
                        watchDog.StartInfo.FileName = file.FullName;
                        try
                        {
                            watchDog.Start();
                        }
                        catch
                        { }
                    }
                }
            }
        }

        private void watchDog_Exited(object sender, EventArgs e)
        {
            if (watchDogActive)
            {
                MessageBox.Show("watchdog");
                ((Process)sender).Start();
            }
        }

        private void FillItems(string path, object items)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo file in dir.GetFiles())
            {
                if (!file.Name.Contains("desktop.ini"))
                {
                    ImageSource img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(GetFolderIcon(IconSize.Large, FolderType.Open, file.FullName, 0).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    //ImageSource img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(Icon.ExtractAssociatedIcon(file.FullName).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    ((ObservableCollection<ItemsWithIcons>)items).Add(new ItemsWithIcons(img, file.Name.Replace(file.Extension, ""), file.FullName, false));
                }
            }
            foreach (DirectoryInfo ddir in dir.GetDirectories())
            {
                ImageSource img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(GetFolderIcon(IconSize.Large, FolderType.Open, ddir.FullName, FILE_ATTRIBUTE_DIRECTORY).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                ((ObservableCollection<ItemsWithIcons>)items).Add(new ItemsWithIcons(img, ddir.Name, ddir.FullName, false));
            }
        }

        private void FillUserApps()
        {
            UserAppList.Clear();
            UserAppList.Add(new ItemsWithIcons(new BitmapImage(new Uri("pack://application:,,,/Resources/intellect.png")), "Video", "LiveView", true));
            FillItems(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), UserAppList);
            mainUI.userListView.ItemsSource = UserAppList;
        }

        private void FillAdminApps(string username)
        {
            AdminAppList.Clear();
            if (this.shelladmin)
            {
                AdminAppList.Add(new ItemsWithIcons(new BitmapImage(new Uri("pack://application:,,,/Resources/run.ico")), "Виконати", "RunCmd", true));
                //FillItems(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AdminAppList);
            }
            //MessageBox.Show(GetUserAppDataPath() + "\r\n" + System.Windows.Forms.Application.LocalUserAppDataPath);
            FillItems(GetUserAppDataPath() + "\\apps\\" + username, AdminAppList);
            if (this.shelladmin)
            {
                string adminpath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                if (!String.IsNullOrWhiteSpace(adminpath) && !String.IsNullOrEmpty(adminpath))
                {
                    FillItems(adminpath, AdminAppList);
                }
                adminpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
                if (!String.IsNullOrWhiteSpace(adminpath) && !String.IsNullOrEmpty(adminpath))
                {
                    FillItems(adminpath, AdminAppList);
                }
                adminpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonAdminTools);
                if (!String.IsNullOrWhiteSpace(adminpath) && !String.IsNullOrEmpty(adminpath))
                {
                    FillItems(adminpath, AdminAppList);
                }

            }
            //mainUI.adminListView.ItemsSource = AdminAppList;
            adminUI.folderView.ItemsSource = AdminAppList;
            if (shelladmin)
            {
                this.adminUI.Title = "Режим адміністратора";
                this.adminUI.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/shield.ico"));
            }
            else
            {
                this.adminUI.Title = "Режим розширеного функціоналу";
                this.adminUI.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/users.ico"));
            }

        }

        private void FillFolderView(string folder)
        {
            FolderAppList.Clear();
            if (this.shelladmin)
            {
                FolderAppList.Add(new ItemsWithIcons(new BitmapImage(new Uri("pack://application:,,,/Resources/start.png")), "Додому", "AdminFolder", true));
                //FillItems(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AdminAppList);
            }
            FillItems(folder, FolderAppList);
            adminUI.folderView.ItemsSource = FolderAppList;
            adminUI.Title = folder.Replace(Path.GetDirectoryName(folder) + "\\", String.Empty);
        }

        private void Restart()
        {
            this.mainUI.Closing -= this.mainUI.UIForm_Closing;
            Application.Current.Shutdown();
            System.Windows.Forms.Application.Restart();
        }

        private string GetUserAppDataPath()
        {
            string path = string.Empty;
            Assembly assm;
            Type at;
            object[] r;

            // Get the .EXE assembly
            assm = Assembly.GetEntryAssembly();
            // Get a 'Type' of the AssemblyCompanyAttribute
            at = typeof(AssemblyCompanyAttribute);
            // Get a collection of custom attributes from the .EXE assembly
            r = assm.GetCustomAttributes(at, false);
            // Get the Company Attribute
            AssemblyCompanyAttribute ct =
                          ((AssemblyCompanyAttribute)(r[0]));
            // Build the User App Data Path
            path = Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData);
            path += @"\" + ct.Company;
            path += @"\" + assm.GetName().Name;

            return path;
        }

        private void fillprocess()
        {
            while (true)
            {
                if (mainUI != null && mainUI.IsVisible)
                {
                    FillTaskBar();
                }
                Thread.Sleep(500);
            }
        }

        #endregion

    }

}
