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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using FileOperations;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Shell;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Controls {
    static class HookLibManager {
      public static bool IsCustomDialog = false;
        private static IntPtr hHookLib;
        private static int[] hookStatus = Enumerable.Repeat(-1, Enum.GetNames(typeof(Hooks)).Length).ToArray();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void HookLibCallback(int hookId, int retcode);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool NewWindowCallback(IntPtr pIDL);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate bool CopyItemCallback(IntPtr SourceItems, IntPtr DestinationFolder);
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
            public CopyItemCallback cbMoveItem;
            public RenameItemCallback cbRenameItem;
            public DeleteItemCallback cbDeleteItem;
            
            // TODO: NewTreeView should probably also go here.
            // Using PostThreadMessage has a small chance of causing a memory leak.
        }

        private static readonly CallbackStruct callbackStruct = new CallbackStruct() {
            cbHookResult = HookResult,
            cbCopyItem = CopyItem,
            cbMoveItem = MoveItem,
            cbRenameItem = RenameItem,
            cbDeleteItem = DeleteItem,
            
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitShellBrowserHookDelegate(IntPtr shellBrowser);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitHookLibDelegate(CallbackStruct fpHookResult);

        public enum HookCheckPoint{
            Initial,
            ShellBrowser,
            NewWindow,
            Automation,
        }

        // Unmarked hooks exist only to set other hooks.
        private enum Hooks {
            CoCreateInstance = 0,           // Treeview Middle-click
            RegisterDragDrop,               // DragDrop into SubDirTips
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

      private static bool CopyItem(IntPtr SourceItems,IntPtr DestinationFolder) {
        object destinationObject = Marshal.GetObjectForIUnknown(DestinationFolder);
        object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);

        //IntPtr z = IntPtr.Zero;
        //Guid j = new Guid("0000010E-0000-0000-C000-000000000046");
        //Marshal.QueryInterface(item,ref j,out z);

        var SourceItemsCollection = WindowsAPI.ParseShellIDListArray((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject);
        var DestinationLocation = ShellObjectFactory.Create((IShellItem)destinationObject);
        return IsCustomDialog;
      }

      private static bool MoveItem(IntPtr SourceItems, IntPtr DestinationFolder) {
        object destinationObject = Marshal.GetObjectForIUnknown(DestinationFolder);
        object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);

        //IntPtr z = IntPtr.Zero;
        //Guid j = new Guid("0000010E-0000-0000-C000-000000000046");
        //Marshal.QueryInterface(item,ref j,out z);

        var SourceItemsCollection = WindowsAPI.ParseShellIDListArray((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject);
        var DestinationLocation = ShellObjectFactory.Create((IShellItem)destinationObject);
        return IsCustomDialog;
      }

      private static bool RenameItem(IntPtr SourceItems, String DestinationName) {
        object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);

        //IntPtr z = IntPtr.Zero;
        //Guid j = new Guid("0000010E-0000-0000-C000-000000000046");
        //Marshal.QueryInterface(item,ref j,out z);

        var SourceItemsCollection = WindowsAPI.ParseShellIDListArray((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject);
        return IsCustomDialog;
      }

      private static bool DeleteItem(IntPtr SourceItems) {
        object sourceObject = Marshal.GetObjectForIUnknown(SourceItems);

        //IntPtr z = IntPtr.Zero;
        //Guid j = new Guid("0000010E-0000-0000-C000-000000000046");
        //Marshal.QueryInterface(item,ref j,out z);

        var SourceItemsCollection = WindowsAPI.ParseShellIDListArray((System.Runtime.InteropServices.ComTypes.IDataObject)sourceObject);
        return IsCustomDialog;
      }
        

        public static void CheckHooks() {
            //TODO:
        }
    }
}
