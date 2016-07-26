using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AP.Shell
{
    public class ItemsWithIcons
    {
        public ItemsWithIcons(ImageSource image, string name, string filename, bool localresource)  
        {  
            this.Name = name;  
            this.Image = image;
            this.FileName = filename;
            this.LocalResource = localresource;
        }  

        public string Name { get; set; }  
        public ImageSource Image { get; set; }
        public string FileName { get; set; }
        public bool LocalResource { get; set; }
  
    }

    public class ProcessItems
    {
        public ProcessItems(ImageSource image, string name, IntPtr id, bool localresource)  
        {  
            this.Name = name;  
            this.Image = image;
            this.Id = id;
            this.LocalResource = localresource;
        }  

        public string Name { get; set; }  
        public ImageSource Image { get; set; }
        public IntPtr Id { get; set; }
        public bool LocalResource { get; set; }
  
    }
}
