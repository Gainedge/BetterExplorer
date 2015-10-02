using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BExplorer.Shell.CommonFileDialogs
{
    /// <summary>
    /// Wraps System.Windows.Forms.OpenFileDialog to make it present
    /// a vista-style dialog.
    /// </summary>
    public class FolderSelectDialog
    {
        // Wrapped dialog
        OpenFileDialog ofd = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FolderSelectDialog()
        {
            ofd = new OpenFileDialog() { Filter = "Folders|\n", AddExtension = false, CheckFileExists = false, DereferenceLinks = true, Multiselect = false };
        }

        #region Properties

        /// <summary>
        /// Gets/Sets the initial folder to be selected. A null value selects the current directory.
        /// </summary>
        public string InitialDirectory
        {
            get { return ofd.InitialDirectory; }
            set { ofd.InitialDirectory = value == null || value.Length == 0 ? Environment.CurrentDirectory : value; }
        }

        /// <summary>
        /// Gets/Sets the title to show in the dialog
        /// </summary>
        public string Title
        {
            get { return ofd.Title; }
            set { ofd.Title = value == null ? "Select a folder" : value; }
        }

        /// <summary>
        /// Gets the selected folder
        /// </summary>
        public string FileName => ofd.FileName;

        #endregion

        #region Methods

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog() => ShowDialog(IntPtr.Zero);

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <param name="hWndOwner">Handle of the control to be parent</param>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog(IntPtr hWndOwner)
        {
            bool flag = false;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                var r = new Reflector("System.Windows.Forms");

                uint num = 0;
                Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                object dialog = r.Call(ofd, "CreateVistaDialog");
                r.Call(ofd, "OnBeforeVistaDialog", dialog);

                uint options = (uint)r.CallAs(typeof(System.Windows.Forms.FileDialog), ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

                object pfde = r.New("FileDialog.VistaDialogEvents", ofd);
                object[] parameters = new object[] { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try
                {
                    int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", hWndOwner);
                    flag = 0 == num2;
                }
                finally
                {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }
            }
            else
            {
                var fbd = new FolderBrowserDialog();
                fbd.Description = this.Title;
                fbd.SelectedPath = this.InitialDirectory;
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog(new WindowWrapper(hWndOwner)) != DialogResult.OK) return false;
                ofd.FileName = fbd.SelectedPath;
                flag = true;
            }

            return flag;
        }

        #endregion
    }

    /// <summary>
    /// Creates IWin32Window around an IntPtr
    /// </summary>
    public class WindowWrapper : IWin32Window
    {

        /// <summary>
        /// Original ptr
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle">Handle to wrap</param>
        public WindowWrapper(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
