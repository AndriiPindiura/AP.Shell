using Caliburn.Micro;
using FileExplorer;
using FileExplorer.Models;
using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AP.Explorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window
    {
        public ExplorerWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            IProfile _exProfile = new FileSystemInfoExProfile(explorer.Events, explorer.WindowManager);
            IProfile[] _profiles = new IProfile[] { _exProfile };
            IEntryModel[] _rootDirs = new IEntryModel[] { AsyncUtils.RunSync(() => _exProfile.ParseAsync("")) };
            explorer.ViewModel.Initializer =
                new ScriptCommandInitializer()
                {
                    OnModelCreated = ScriptCommands.Run("{OnModelCreated}"),
                    OnViewAttached = ScriptCommands.Run("{OnViewAttached}"),
                    RootModels = _rootDirs,
                    StartupParameters = new ParameterDic()
                    {
                         { "Profiles", _profiles },
                         { "RootDirectories", _rootDirs },	 
                         { "StartupPath", "" },                         
                         { "ViewMode", "List" }, 
                         { "ItemSize", 8 },
                         { "EnableDrag", true }, 
                         { "EnableDrop", true }, 
                         { "FileListNewWindowCommand", NullScriptCommand.Instance }, //Disable NewWindow Command.
                         { "EnableMultiSelect", true},
                         { "ShowToolbar", true }, 
                         { "ShowGridHeader", true }, 
                         { "OnModelCreated", IOInitializeHelpers.Explorer_Initialize_Default }, 
                         { "OnViewAttached", UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot() }
                    }
                };

        }

    }
}
