// BExplorer.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007-2009 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either 
// version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free 
// Software Foundation, Inc., 51 Franklin Street, Fifth Floor,  
// Boston, MA 2110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using Microsoft.Win32;

namespace BExplorer.Shell {
	/// <summary>
	/// Provides support for displaying the context menu of a shell item.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// Use this class to display a context menu for a shell item, either
	/// as a popup menu, or as a main menu. 
	/// </para>
	/// 
	/// <para>
	/// To display a popup menu, simply call <see cref="ShowContextMenu"/>
	/// with the parent control and the position at which the menu should
	/// be shown.
	/// </para>
	/// 
	/// <para>
	/// To display a shell context menu in a Form's main menu, call the
	/// <see cref="Populate"/> method to populate the menu. In addition, 
	/// you must intercept a number of special messages that will be sent 
	/// to the menu's parent form. To do this, you must override 
	/// <see cref="Form.WndProc"/> like so:
	/// </para>
	/// 
	/// <code>
	///     protected override void WndProc(ref Message m) {
	///         if ((m_ContextMenu == null) || (!m_ContextMenu.HandleMenuMessage(ref m))) {
	///             base.WndProc(ref m);
	///         }
	///     }
	/// </code>
	/// 
	/// <para>
	/// Where m_ContextMenu is the <see cref="ShellContextMenu"/> being shown.
	/// </para>
	/// 
	/// Standard menu commands can also be invoked from this class, for 
	/// example <see cref="InvokeDelete"/> and <see cref="InvokeRename"/>.
	/// </remarks>
	public class ShellContextMenu {

		const int m_CmdFirst = 0x8000;
		MessageWindow m_MessageWindow;
		IContextMenu m_ComInterface;
		IContextMenu2 m_ComInterface2;
		IContextMenu3 m_ComInterface3;

		/// <summary>The ShellView the ContextMenu is associated with</summary>
		private ShellView _ShellView { get; set; }

		/*
		/// <summary>
		/// Gets the underlying COM <see cref="IContextMenu"/> interface.
		/// </summary>
		[Obsolete("Never Used")]
		public IContextMenu ComInterface {
			get { return m_ComInterface; }
			set { m_ComInterface = value; }
		}
		*/



		//[Obsolete("Never Used")]
		//private ShellTreeViewEx _ShellTreeView { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellContextMenu"/> class.
		/// </summary>
		/// <param name="shellView">The ShellView the ContextMenu is associated with</param>
		/// <param name="menuType"></param>
		public ShellContextMenu(ShellView shellView, int menuType) {
			this._ShellView = shellView;

			IntPtr iContextMenu = IntPtr.Zero;

			if (menuType == 0)
				this.GetNewContextMenu(_ShellView.CurrentFolder, out iContextMenu, out m_ComInterface);
			else
				this.GetOpenWithContextMenu(_ShellView.CurrentFolder, out iContextMenu, out m_ComInterface);

			m_ComInterface2 = m_ComInterface as IContextMenu2;
			m_ComInterface3 = m_ComInterface as IContextMenu3;
			m_MessageWindow = new MessageWindow(this);
		}
		/// <summary>
		/// Initialises a new instance of the <see cref="ShellContextMenu"/> 
		/// class.
		/// </summary>
		/// 
		/// <param name="item">
		/// The item to which the context menu should refer.
		/// </param>
		public ShellContextMenu(ShellItem item) {
			Initialize(new ShellItem[] { item });
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellContextMenu"/> class.
		/// </summary>
		/// 
		/// <param name="items">
		/// The items to which the context menu should refer.
		/// </param>
		/// <param name="svgio"></param>
		public ShellContextMenu(ShellItem[] items, SVGIO svgio = SVGIO.SVGIO_SELECTION) {
			//this._ShellView = view;
			//this._ShellTreeView = tree;
			if (svgio == SVGIO.SVGIO_BACKGROUND) {
				Initialize(items[0]);
			}
			else {
				Initialize(items);
			}

		}

		/// <summary>
		/// Handles context menu messages when the <see cref="ShellContextMenu"/>
		/// is displayed on a Form's main menu bar.
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// To display a shell context menu in a Form's main menu, call the
		/// <see cref="Populate"/> method to populate the menu with the shell
		/// item's menu items. In addition, you must intercept a number of
		/// special messages that will be sent to the menu's parent form. To
		/// do this, you must override <see cref="Form.WndProc"/> like so:
		/// </para>
		/// 
		/// <code>
		///     protected override void WndProc(ref Message m) {
		///         if ((m_ContextMenu == null) || (!m_ContextMenu.HandleMenuMessage(ref m))) {
		///             base.WndProc(ref m);
		///         }
		///     }
		/// </code>
		/// 
		/// <para>
		/// Where m_ContextMenu is the <see cref="ShellContextMenu"/> being shown.
		/// </para>
		/// </remarks>
		/// 
		/// <param name="m">
		/// The message to handle.
		/// </param>
		/// 
		/// <returns>
		/// <see langword="true"/> if the message was a Shell Context Menu
		/// message, <see langword="false"/> if not. If the method returns false,
		/// then the message should be passed down to the base class's
		/// <see cref="Form.WndProc"/> method.
		/// </returns>
		public bool HandleMenuMessage(ref Message m) {
			//For send to menu in the ListView context menu
			int hr = 0;
			if (m.Msg == (int)WM.WM_INITMENUPOPUP | m.Msg == (int)WM.WM_MEASUREITEM | m.Msg == (int)WM.WM_DRAWITEM) {
				if ((m_ComInterface2 != null)) {
					hr = (int)m_ComInterface2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
					if (hr == 0) {
						return true;
					}
				}
				//else if ((m.Msg == (int)WM.WM_INITMENUPOPUP & m.WParam == m_WindowsContextMenu.newMenuPtr) | m.Msg == (int)ShellAPI.WM.MEASUREITEM | m.Msg == (int)ShellAPI.WM.DRAWITEM) {
				//	if ((m_WindowsContextMenu.newMenu2 != null)) {
				//		hr = m_WindowsContextMenu.newMenu2.HandleMenuMsg(m.Msg, m.WParam, m.LParam);
				//		if (hr == 0) {
				//			return true;
				//		}
				//	}
				//}
			}
			else if (m.Msg == (int)WM.WM_MENUCHAR) {
				if ((m_ComInterface3 != null)) {
					var ptr = IntPtr.Zero;
					hr = (int)m_ComInterface3.HandleMenuMsg2(m.Msg, m.WParam, m.LParam, out ptr);
					if (hr == 0) {
						return true;
					}
				}
			}
			//if ((m.Msg == (int)BExplorer.Shell.Interop.MSG.WM_COMMAND) && ((int)m.WParam >= m_CmdFirst)) {
			//	InvokeCommand((int)m.WParam, new Point() { X = 0, Y = 0 });
			//	return true;
			//} else {
			//	if (m_ComInterface3 != null) {
			//		IntPtr result;
			//		if (m_ComInterface3.HandleMenuMsg2(m.Msg, m.WParam, m.LParam,
			//				out result) == HResult.S_OK) {
			//			m.Result = result;
			//			return true;
			//		}
			//	} else if (m_ComInterface2 != null) {
			//		if (m_ComInterface2.HandleMenuMsg(m.Msg, m.WParam, m.LParam)
			//						== HResult.S_OK) {
			//			m.Result = IntPtr.Zero;
			//			return true;
			//		}
			//	}
			//}
			return false;
		}

		/*
		/// <summary>
		/// Invokes the Delete command on the shell item.
		/// </summary>
		[Obsolete("Not Used", true)]
		public void InvokeDelete() {
			//CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
			//invoke.cbSize = Marshal.SizeOf(invoke);
			//invoke.lpVerb = "delete";

			//try {
			//	m_ComInterface.InvokeCommand(ref invoke);
			//} catch (COMException e) {
			//	// Ignore the exception raised when the user cancels
			//	// a delete operation.
			//	if (e.ErrorCode != unchecked((int)0x800704C7)) throw;
			//}
		}
		*/

		/*
		/// <summary>
		/// Invokes the Rename command on the shell item.
		/// </summary>
		public void InvokeRename() {
			//CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
			//invoke.cbSize = Marshal.SizeOf(invoke);
			//invoke.lpVerb = "rename";
			//m_ComInterface.InvokeCommand(ref invoke);
			if (this._ShellView != null)
				this._ShellView.RenameSelectedItem();
		}
		*/

		/// <summary>
		/// Populates a <see cref="Menu"/> with the context menu items for
		/// a shell item.
		/// </summary>
		/// 
		/// <remarks>
		/// If this method is being used to populate a Form's main menu
		/// then you need to call <see cref="HandleMenuMessage"/> in the
		/// Form's message handler.
		/// </remarks>
		/// 
		/// <param name="menu">The menu to populate.</param>
		/// <param name="additionalFlags"></param>
		public void Populate(Menu menu, CMF additionalFlags) {
			RemoveShellMenuItems(menu);
			m_ComInterface.QueryContextMenu(menu.Handle, 0, m_CmdFirst, int.MaxValue, CMF.EXPLORE | additionalFlags);
		}

		/// <summary>
		/// Shows a context menu for a shell item.
		/// </summary>
		/// 
		/// <param name="control">
		/// The parent control.
		/// </param>
		/// 
		/// <param name="pos">
		/// The position on <paramref name="control"/> that the menu
		/// should be displayed at.
		/// </param>
		/// <param name="aditionalFlags"></param>
		/// <param name="IsOnEmpty"></param>
		public void ShowContextMenu(Control control, Point pos, CMF aditionalFlags = 0, bool IsOnEmpty = false) {
			using (ContextMenu menu = new ContextMenu()) {
				pos = control.PointToScreen(pos);
				Populate(menu, aditionalFlags);
				int count = User32.GetMenuItemCount(menu.Handle);
				MENUITEMINFO itemInfo = new MENUITEMINFO();
				itemInfo.cbSize = (uint)Marshal.SizeOf(itemInfo);
				itemInfo.fMask = MIIM.MIIM_FTYPE | MIIM.MIIM_DATA;
				if (User32.GetMenuItemInfo(menu.Handle, count - 1, true, ref itemInfo)) {
					var isSep = (itemInfo.fType & 2048) != 0;
					if (isSep) {
						User32.DeleteMenu(menu.Handle, count - 1, MF.MF_BYPOSITION);
					}
				}

				if (IsOnEmpty) {
					User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
					User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
					User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
					User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
					User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
				}
				if (User32.GetMenuItemInfo(menu.Handle, 1, true, ref itemInfo)) {
					if ((itemInfo.fType & 2048) != 0) {
						User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
						User32.DeleteMenu(menu.Handle, 0, MF.MF_BYPOSITION);
					}
				}
				int command = User32.TrackPopupMenuEx(menu.Handle,
						TPM.TPM_RETURNCMD, pos.X, pos.Y, m_MessageWindow.Handle,
						IntPtr.Zero);
				if (command > 0) {
					string info = string.Empty;
					byte[] bytes = new byte[256];
					int index;

					m_ComInterface.GetCommandString(command - m_CmdFirst, 4, 0, bytes, 260);

					index = 0;
					while (index < bytes.Length - 1 && (bytes[index] != 0 || bytes[index + 1] != 0)) { index += 2; }

					if (index < bytes.Length - 1)
						info = Encoding.Unicode.GetString(bytes, 0, index);

					switch (info) {
						case "rename":
							if (control is ShellView) {
								(control as ShellView).RenameSelectedItem();
							}
							break;
						case "cut":
							if (control is ShellView) {
								(control as ShellView).CutSelectedFiles();
							}
							break;
						case "copy":
							if (control is ShellView) {
								(control as ShellView).CopySelectedFiles();
							}
							break;
						default:
							InvokeCommand(command - m_CmdFirst, pos);
							break;
					}
				}
			}
		}

		private List<string> GetNewContextMenuItems() {
			List<string> newEntrieslist = new List<string>();
			RegistryKey reg = Registry.CurrentUser;
			RegistryKey classesrk = reg.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Discardable\PostSetup\ShellNew");
			string[] classes = (string[])classesrk.GetValue("Classes");
			newEntrieslist.AddRange(classes);
			classesrk.Close();
			reg.Close();
			return newEntrieslist;
		}

		public void ShowContextMenu(Point pos, int type = 0) {
			if (type == 0) {
				var newItems = this.GetNewContextMenuItems();
				using (ContextMenu menu = new ContextMenu()) {
					Populate(menu, CMF.NORMAL);
					int command = User32.TrackPopupMenuEx(menu.Handle,
							TPM.TPM_RETURNCMD, pos.X, pos.Y, m_MessageWindow.Handle,
							IntPtr.Zero);
					if (command > 0) {
						int cmdID = command - m_CmdFirst;
						var verb = cmdID == 1 ? "newFolder" : cmdID == 2 ? ".lnk" : newItems[cmdID - 3];
						var item = Marshal.StringToHGlobalAuto(verb);
						//this._ShellView.IsRenameNeeded = true;
						InvokeCommand((int)item, pos);
					}
				}
			}
			else {
				using (ContextMenu menu = new ContextMenu()) {
					Populate(menu, CMF.NORMAL);
					int command = User32.TrackPopupMenuEx(menu.Handle,
							TPM.TPM_RETURNCMD, pos.X, pos.Y, m_MessageWindow.Handle,
							IntPtr.Zero);
					if (command > 0) {
						InvokeCommand(command - m_CmdFirst, pos);
					}
				}
			}
		}

		void Initialize(ShellItem[] items) {
			IntPtr[] pidls = new IntPtr[items.Length];
			ShellItem parent = null;
			IntPtr result;

			for (int n = 0; n < items.Length; ++n) {
				pidls[n] = Shell32.ILFindLastID(items[n].Pidl);

				if (parent == null) {
					if (items[n] == ShellItem.Desktop) {
						parent = ShellItem.Desktop;
					}
					else {
						parent = items[n].Parent;

					}
				}
				else {
					if (items[n].Parent != parent) {
						throw new Exception("All shell items must have the same parent");
					}
				}
			}

			if (items.Length == 0) {
				var desktop = KnownFolders.Desktop as ShellItem;
				var ishellViewPtr = desktop.GetIShellFolder().CreateViewObject(IntPtr.Zero, typeof(IShellView).GUID);
				var view = Marshal.GetObjectForIUnknown(ishellViewPtr) as IShellView;
				view.GetItemObject(SVGIO.SVGIO_BACKGROUND, typeof(IContextMenu).GUID, out result);
				Marshal.ReleaseComObject(view);
			}
			else {
				parent.GetIShellFolder().GetUIObjectOf(IntPtr.Zero,
						(uint)pidls.Length, pidls,
						typeof(IContextMenu).GUID, 0, out result);
			}
			m_ComInterface = (IContextMenu)
					Marshal.GetTypedObjectForIUnknown(result,
							typeof(IContextMenu));
			m_ComInterface2 = m_ComInterface as IContextMenu2;
			m_ComInterface3 = m_ComInterface as IContextMenu3;
			m_MessageWindow = new MessageWindow(this);
		}

		void Initialize(ShellItem item) {
			IntPtr result = IntPtr.Zero;
			var ishellViewPtr = item.GetIShellFolder().CreateViewObject(IntPtr.Zero, typeof(IShellView).GUID);
			var view = Marshal.GetObjectForIUnknown(ishellViewPtr) as IShellView;
			view.GetItemObject(SVGIO.SVGIO_BACKGROUND, typeof(IContextMenu).GUID, out result);
			Marshal.ReleaseComObject(view);
			m_ComInterface = (IContextMenu)
					Marshal.GetTypedObjectForIUnknown(result,
							typeof(IContextMenu));
			m_ComInterface2 = m_ComInterface as IContextMenu2;
			m_ComInterface3 = m_ComInterface as IContextMenu3;
			m_MessageWindow = new MessageWindow(this);
		}

		void InvokeCommand(int command, Point pt) {
			const int SW_SHOWNORMAL = 1;
			CMINVOKECOMMANDINFOEX invoke = new CMINVOKECOMMANDINFOEX();
			invoke.cbSize = Marshal.SizeOf(invoke);
			invoke.nShow = SW_SHOWNORMAL;
			invoke.fMask = (int)(CMIC.Unicode | CMIC.PtInvoke);
			invoke.lpVerb = (IntPtr)(command);
			invoke.lpVerbW = (IntPtr)(command);
			invoke.ptInvoke = pt;
			m_ComInterface.InvokeCommand(ref invoke);
		}

		void TagManagedMenuItems(Menu menu, int tag) {
			MENUINFO info = new MENUINFO();

			info.cbSize = Marshal.SizeOf(info);
			info.fMask = MIM.MIM_MENUDATA;
			info.dwMenuData = tag;

			foreach (MenuItem item in menu.MenuItems) {
				User32.SetMenuInfo(item.Handle, ref info);
			}
		}

		void RemoveShellMenuItems(Menu menu) {
			const int tag = 0xAB;
			List<int> remove = new List<int>();
			int count = User32.GetMenuItemCount(menu.Handle);
			MENUINFO menuInfo = new MENUINFO();
			MENUITEMINFO itemInfo = new MENUITEMINFO();

			menuInfo.cbSize = Marshal.SizeOf(menuInfo);
			menuInfo.fMask = MIM.MIM_MENUDATA;
			itemInfo.cbSize = (uint)Marshal.SizeOf(itemInfo);
			itemInfo.fMask = MIIM.MIIM_ID | MIIM.MIIM_SUBMENU;

			// First, tag the managed menu items with an arbitary 
			// value (0xAB).
			TagManagedMenuItems(menu, tag);

			for (int n = 0; n < count; ++n) {
				User32.GetMenuItemInfo(menu.Handle, n, true, ref itemInfo);

				if (itemInfo.hSubMenu == IntPtr.Zero) {
					// If the item has no submenu we can't get the tag, so 
					// check its ID to determine if it was added by the shell.
					if (itemInfo.wID >= m_CmdFirst) remove.Add(n);
				}
				else {
					User32.GetMenuInfo(itemInfo.hSubMenu, ref menuInfo);
					if (menuInfo.dwMenuData != tag) remove.Add(n);
				}
			}

			// Remove the unmanaged menu items.
			remove.Reverse();
			foreach (int position in remove) {
				User32.DeleteMenu(menu.Handle, position, MF.MF_BYPOSITION);
			}
		}

		public bool GetNewContextMenu(ShellItem item, out IntPtr iContextMenuPtr, out IContextMenu iContextMenu) {
			Guid CLSID_NewMenu = new Guid("{D969A300-E7FF-11d0-A93B-00A0C90F2719}");
			Guid iicm = typeof(IContextMenu).GUID;
			Guid iise = typeof(IShellExtInit).GUID;
			if (Ole32.CoCreateInstance(
							ref CLSID_NewMenu,
							IntPtr.Zero,
							Ole32.CLSCTX.INPROC_SERVER,
							ref iicm,
							out iContextMenuPtr) == (int)HResult.S_OK) {
				iContextMenu = Marshal.GetObjectForIUnknown(iContextMenuPtr) as IContextMenu;

				IntPtr iShellExtInitPtr;
				if (Marshal.QueryInterface(
						iContextMenuPtr,
						ref iise,
						out iShellExtInitPtr) == (int)HResult.S_OK) {
					IShellExtInit iShellExtInit = Marshal.GetTypedObjectForIUnknown(
							iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

					try {
						iShellExtInit.Initialize(item.Pidl, IntPtr.Zero, 0);

						Marshal.ReleaseComObject(iShellExtInit);
						Marshal.Release(iShellExtInitPtr);
						return true;
					}
					finally {

					}
				}
				else {
					if (iContextMenu != null) {
						Marshal.ReleaseComObject(iContextMenu);
						iContextMenu = null;
					}

					if (iContextMenuPtr != IntPtr.Zero) {
						Marshal.ReleaseComObject(iContextMenuPtr);
						iContextMenuPtr = IntPtr.Zero;
					}

					return false;
				}
			}
			else {
				iContextMenuPtr = IntPtr.Zero;
				iContextMenu = null;
				return false;
			}
		}

		public bool GetOpenWithContextMenu(ShellItem item, out IntPtr iContextMenuPtr, out IContextMenu iContextMenu) {
			Guid CLSID_OpenWith = new Guid(0x09799AFB, 0xAD67, 0x11d1, 0xAB, 0xCD, 0x00, 0xC0, 0x4F, 0xC3, 0x09, 0x36);
			Guid iicm = typeof(IContextMenu).GUID;
			Guid iise = typeof(IShellExtInit).GUID;
			if (Ole32.CoCreateInstance(
							ref CLSID_OpenWith,
							IntPtr.Zero,
							Ole32.CLSCTX.INPROC_SERVER,
							ref iicm,
							out iContextMenuPtr) == (int)HResult.S_OK) {
				iContextMenu = Marshal.GetObjectForIUnknown(iContextMenuPtr) as IContextMenu;

				IntPtr iShellExtInitPtr;
				if (Marshal.QueryInterface(
						iContextMenuPtr,
						ref iise,
						out iShellExtInitPtr) == (int)HResult.S_OK) {
					IShellExtInit iShellExtInit = Marshal.GetTypedObjectForIUnknown(iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

					try {
						iShellExtInit.Initialize(item.Pidl, IntPtr.Zero, 0);

						Marshal.ReleaseComObject(iShellExtInit);
						Marshal.Release(iShellExtInitPtr);
						return true;
					}
					finally {

					}
				}
				else {
					if (iContextMenu != null) {
						Marshal.ReleaseComObject(iContextMenu);
						iContextMenu = null;
					}

					return false;
				}
			}
			else {
				iContextMenuPtr = IntPtr.Zero;
				iContextMenu = null;
				return false;
			}
		}

		class MessageWindow : Control {
			public MessageWindow(ShellContextMenu parent) {
				m_Parent = parent;
			}

			protected override void WndProc(ref Message m) {
				if (!m_Parent.HandleMenuMessage(ref m)) {
					base.WndProc(ref m);
				}
			}

			ShellContextMenu m_Parent;
		}
	}
}
