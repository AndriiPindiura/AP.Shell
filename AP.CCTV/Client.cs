using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace AP.CCTV
{
    [Guid("06FC3A80-F2B3-46A4-86B4-380674478086"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IClient
    {
        void Connect(string ip, int compress, long rights, int continious);
    }

    [ProgId("AP.CCTV.Control"), ClassInterface(ClassInterfaceType.None),
    Guid("CCC33404-40B2-4483-8769-380674478086"), ComVisible(true)]
    public class Client:IClient
    {
        private List<Window> activeMonitor;
        private string exportDir;

        [ComVisible(true)]
        public string exportPath
        {
            get { return exportDir; }
        }

        public Client()
        {
            activeMonitor = new List<Window>();
        }

        [ComVisible(true)]
        public void Connect(string ip, int compress, long rights, int continuous)
        {
            //MessageBox.Show("111");
            Monitor monitor = new Monitor(ip, compress, rights, continuous);
            activeMonitor.Add(monitor);
            monitor.Show();
            exportDir = monitor.ocxITV.GetExportDir();
        }

        [ComVisible(true)]
        public void CloseAll()
        {
            foreach (Window monitor in activeMonitor)
            {
                monitor.Close();
            }
            activeMonitor.Clear();
        }

    }
}
