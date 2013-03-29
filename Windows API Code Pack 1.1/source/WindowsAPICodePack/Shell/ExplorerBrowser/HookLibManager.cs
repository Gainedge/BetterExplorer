//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using FileOperations;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.FileOperations;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Controls {
  class HookLibManager {
      public static bool IsCustomDialog = false;
      //public static ExplorerBrowser Browser;
        private static IntPtr hHookLib;
        public static SynchronizationContext SyncContext;
        private static int[] hookStatus = Enumerable.Repeat(-1, Enum.GetNames(typeof(Hooks)).Length).ToArray();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void HookLibCallback(int hookId, int retcode);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool NewWindowCallback(IntPtr pIDL);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool CopyItemCallback(IFileOperation pThis, IntPtr SourceItems, IntPtr DestinationFolder);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool MoveItemCallback(IntPtr SourceItems, IntPtr DestinationFolder);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool RenameItemCallback(IntPtr SourceItems, String DestinationName);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool DeleteItemCallback(IntPtr SourceItems);

        [StructLayout(LayoutKind.Sequential)]
        private struct CallbackStruct {
            public HookLibCallback cbHookResult;
            public NewWindowCallback cbNewWindow;
            public CopyItemCallback cbCopyItem;
            public MoveItemCallback cbMoveItem;
            public RenameItemCallback cbRenameItem;
            public DeleteItemCallback cbDeleteItem;
        }

        private static readonly CallbackStruct callbackStruct = new CallbackStruct() {
            cbHookResult = HookResult,
            cbCopyItem = CopyItem,
            cbMoveItem = MoveItem,
            cbRenameItem = RenameItem,
            cbDeleteItem = DeleteItem,
            
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitHookLibDelegate(CallbackStruct fpHookResult);

        public enum HookCheckPoint{
            Initial,
        }

        // Unmarked hooks exist only to set other hooks.
        private enum Hooks {
            CoCreateInstance = 0,           // Treeview Middle-click; FileCopyOperations
            RegisterDragDrop,               // DragDrop
            SHCreateShellFolderView,
            BrowseObject,                   // Control Panel dialog OK/Cancel buttons
            CreateViewWindow3,              // Header in all Views
            MessageSFVCB,                   // Refresh = clear text
            UiaReturnRawElementProvider,
            QueryInterface,                 // Scrolling Lag workaround
            TravelToEntry,                  // Clear Search bar = back
            OnActivateSelection,            // Recently activated files
            SetNavigationState,             // Breadcrumb Bar Middle-click
            ShowWindow,                     // New Explorer window capturing
            UpdateWindowList,               // Compatibility with SHOpenFolderAndSelectItems
            DeleteItem,
            RenameItem,
            MoveItem,
            CopyItem,
        }

        public static void Initialize() {
            if(hHookLib != IntPtr.Zero) return;
            string installPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filename = IntPtr.Size == 8 ? "BEHookLib64.dll" : "BEHookLib32.dll";
            hHookLib = WindowsAPI.LoadLibrary(Path.Combine(installPath, filename));
            int retcode = -1;
            if(hHookLib == IntPtr.Zero) {
                int error = Marshal.GetLastWin32Error();
                //TODO: error should be logged;
            }
            else {
                IntPtr pFunc = WindowsAPI.GetProcAddress(hHookLib, "Initialize");
                if(pFunc != IntPtr.Zero) {
                    InitHookLibDelegate initialize = (InitHookLibDelegate)
                            Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitHookLibDelegate));
                    try {
                        retcode = initialize(callbackStruct);
                    }
                    catch(Exception e) {
                        //TODO: Erors should be logged
                    }

                }
            }

            if(retcode == 0) return;
            //TODO: here Errors should be logged
        }

        private static void HookResult(int hookId, int retcode) {
            lock(callbackStruct.cbHookResult) {
                hookStatus[hookId] = retcode;
            }
        }


        private static void DoCopy(object Data) {
          FileOperationsData FileData = (FileOperationsData)Data;
          if (FileData.ItemsForDrop != null) {
            using (FileOperations.FileOperation fileOp = new FileOperations.FileOperation()) {
              foreach (ShellObject item in FileData.ItemsForDrop) {

                string New_Name = "";
                if (Path.GetExtension(item.ParsingName) == "") {

                  New_Name = item.GetDisplayName(DisplayNameType.Default);
                } else {
                  New_Name = Path.GetFileName(item.ParsingName);
                }
                fileOp.CopyItem(item.ParsingName, FileData.PathForDrop, New_Name);
              }

              fileOp.PerformOperations();
            }
          }
        }

        private static void DoMove(object Data) {
          FileOperationsData FileData = (FileOperationsData)Data;
          if (FileData.ItemsForDrop != null) {
            using (FileOperations.FileOperation fileOp = new FileOperations.FileOperation()) {
              foreach (ShellObject item in FileData.ItemsForDrop) {

                string New_Name = "";
                if (Path.GetExtension(item.ParsingName) == "") {

                  New_Name = item.GetDisplayName(DisplayNameType.Default);
                } else {
                  New_Name = Path.GetFileName(item.ParsingName);
                }
                fileOp.MoveItem(item.ParsingName, FileData.PathForDrop, New_Name);
              }

              fileOp.PerformOperations();
            }
          }
        }

      private static bool CopyItem(IFileOperation pthis, IntPtr SourceItems,IntPtr DestinationFolder) {
          if (!IsCustomDialog)
          {
              return false;
          }

        object destinationObject = Marshal.GetObjectForIUnknown(DestinationFolder);
        object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);
        var u = SyncContext;
        var shellobj = ShellObjectFactory.Create((IShellItem)destinationObject);
        var DestinationLocation = shellobj.ParsingName;
        shellobj.Dispose();
        u.Post((o) => {
          var SourceItemsCollection = ShellObjectCollection.FromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject).Select(c => c.ParsingName).ToArray();
          var win = System.Windows.Application.Current.MainWindow;

          Microsoft.WindowsAPICodePack.Shell.FileOperations.FileOperation tempWindow = new Microsoft.WindowsAPICodePack.Shell.FileOperations.FileOperation(SourceItemsCollection, DestinationLocation);
          FileOperationDialog currentDialog = win.OwnedWindows.OfType<FileOperationDialog>().SingleOrDefault();

          if (currentDialog == null) {
            currentDialog = new FileOperationDialog();
            tempWindow.ParentContents = currentDialog;
            currentDialog.Owner = win;

            tempWindow.Visibility = Visibility.Collapsed;
            currentDialog.Contents.Add(tempWindow);
          } else {
            tempWindow.ParentContents = currentDialog;
            tempWindow.Visibility = Visibility.Collapsed;
            currentDialog.Contents.Add(tempWindow);
          }
          
        }, null);
        return true;
      }

      private static void ThreadStartingPoint(object parameter) {
        //var parameters = (Tuple<ShellObject[], ShellObject>)parameter;
        //FileOperationDialog tempWindow = new FileOperationDialog();
        //tempWindow.SourceItemsCollection = parameters.Item1;
        //tempWindow.DestinationLocation = parameters.Item2;
        //tempWindow.Show();
        //System.Windows.Threading.Dispatcher.Run();
      }

      private static bool MoveItem(IntPtr SourceItems, IntPtr DestinationFolder) {
        if (!IsCustomDialog)
          return false;

        object destinationObject = Marshal.GetObjectForIUnknown(DestinationFolder);
        object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);
        var SourceItemsCollection = ShellObjectCollection.FromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject).Select(c => c.ParsingName).ToArray();
        var DestinationLocation = ShellObjectFactory.Create((IShellItem)destinationObject).ParsingName;
        SyncContext.Post((o) => {
          Microsoft.WindowsAPICodePack.Shell.FileOperations.FileOperation tempWindow = new Microsoft.WindowsAPICodePack.Shell.FileOperations.FileOperation(SourceItemsCollection, DestinationLocation, OperationType.Move);
          var currentDialog = System.Windows.Application.Current.MainWindow.OwnedWindows.OfType<FileOperationDialog>().SingleOrDefault();
          if (currentDialog == null) {
            currentDialog = new FileOperationDialog();
            tempWindow.ParentContents = currentDialog;
            currentDialog.Owner = System.Windows.Application.Current.MainWindow;
            tempWindow.Visibility = System.Windows.Visibility.Collapsed;
            currentDialog.Contents.Add(tempWindow);
          } else {
            tempWindow.ParentContents = currentDialog;
            tempWindow.Visibility = System.Windows.Visibility.Collapsed;
            currentDialog.Contents.Add(tempWindow);
          }
        }, null);
        return true;
      }

      private static bool RenameItem(IntPtr SourceItems, String DestinationName) {
        //object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);

        //IntPtr z = IntPtr.Zero;
        //Guid j = new Guid("0000010E-0000-0000-C000-000000000046");
        //Marshal.QueryInterface(item,ref j,out z);

        //var SourceItem = ShellObjectFactory.Create((IShellItem)sourceObject);

        //if (SourceItem.IsFolder && SourceItem.Properties.System.FileExtension.Value == null) {
        //  Directory.Move(SourceItem.ParsingName, Path.Combine(SourceItem.Parent.ParsingName, DestinationName));
        //} else {
        //  File.Move(SourceItem.ParsingName, Path.Combine(SourceItem.Parent.ParsingName, DestinationName));
        //}
        //
        return false;
      }

      private static bool DeleteItem(IntPtr SourceItems) {
        if (!IsCustomDialog)
          return false;
        //TaskDialog dlg = new TaskDialog();
        //dlg.Icon = TaskDialogStandardIcon.;
        //dlg.Text = "Are you sure you want to move ";
        //dlg.Show();
          object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);

          //IntPtr z = IntPtr.Zero;
          //Guid j = new Guid("0000010E-0000-0000-C000-000000000046");
          //Marshal.QueryInterface(item,ref j,out z);
          FODeleteDialog confirmationDialog = new FODeleteDialog();
          confirmationDialog.MessageCaption = "Removal confirmation";
          var SourceItemsCollection = ShellObjectCollection.FromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject).Select(c => c.ParsingName).ToArray();
          var win = System.Windows.Application.Current.MainWindow;
          if (SourceItemsCollection.Count() == 1)
          {
            ShellObject item = ShellObject.FromParsingName(SourceItemsCollection[0]);
            item.Thumbnail.CurrentSize = new Size(96, 96);
            confirmationDialog.MessageIcon = item.Thumbnail.BitmapSource;
            confirmationDialog.MessageText = Control.ModifierKeys != Keys.Shift
                                               ? "Are you sure you want to move " +
                                                 item.GetDisplayName(DisplayNameType.Default) + " to Recycle Bin?"
                                               : "Are you sure you want to remove " +
                                                 item.GetDisplayName(DisplayNameType.Default) + " permanently?";
          }
          else
          {
            confirmationDialog.MessageText = Control.ModifierKeys != Keys.Shift
                                               ? "Are you sure you want to move selected " +
                                                 SourceItemsCollection.Count() + " items to Recycle Bin?"
                                               : "Are you sure you want to remove selected " +
                                                 SourceItemsCollection.Count() + " items permanently?";
          }

        confirmationDialog.Owner = win;
        if (confirmationDialog.ShowDialog() == true){
          Microsoft.WindowsAPICodePack.Shell.FileOperations.FileOperation tempWindow =
            new Microsoft.WindowsAPICodePack.Shell.FileOperations.FileOperation(SourceItemsCollection, String.Empty,
                                                                                OperationType.Delete,
                                                                                Control.ModifierKeys != Keys.Shift);
          FileOperationDialog currentDialog = win.OwnedWindows.OfType<FileOperationDialog>().SingleOrDefault();

          if (currentDialog == null)
          {
            currentDialog = new FileOperationDialog();
            tempWindow.ParentContents = currentDialog;
            currentDialog.Owner = win;

            tempWindow.Visibility = Visibility.Collapsed;
            currentDialog.Contents.Add(tempWindow);
          }
          else
          {
            tempWindow.ParentContents = currentDialog;
            tempWindow.Visibility = Visibility.Collapsed;
            currentDialog.Contents.Add(tempWindow);
          }
        }

        //if (Control.ModifierKeys == Keys.Shift) {
        //  Thread MoveThread = new Thread(new ParameterizedThreadStart(DoDelete));
        //  MoveThread.SetApartmentState(ApartmentState.STA);
        //  MoveThread.Start(SourceItemsCollection);
        //} else {
        //  DeleteToRecycleBin(SourceItemsCollection);
        //}
        return true;//IsCustomDialog;
      }

      

      public static void DeleteToRecycleBin(ShellObject[] SelectedItems) {
        string Files = "";
        foreach (ShellObject selectedItem in SelectedItems) {
          if (Files == "") {
            Files = selectedItem.ParsingName;
          } else
            Files = String.Format("{0}\0{1}", Files, selectedItem.ParsingName);
        }
        RecybleBin.Send(Files);
      }

      public static void DoDelete(object Data) {
        ShellObject[] DataDelete = (ShellObject[])Data;

        using (FileOperations.FileOperation fileOp = new FileOperations.FileOperation()) {
          foreach (ShellObject item in DataDelete) {
            fileOp.DeleteItem(item.ParsingName);
          }

          fileOp.PerformOperations();
        }

      }
        

        public static void CheckHooks() {
            //TODO:
        }
    }
}
