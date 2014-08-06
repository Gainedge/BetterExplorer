using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Controls;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell {

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
	public class ContextShellMenu {

		private readonly Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser Explorer;
		/// <summary>
		/// Initialises a new instance of the <see cref="ShellContextMenu"/> 
		/// class.
		/// </summary>
		/// 
		/// <param name="item">
		/// The item to which the context menu should refer.
		/// </param>
		public ContextShellMenu(ShellObject item) {
			Initialize(new ShellObject[] { item });
		}

		/// <summary>
		/// Initialises a new instance of the <see cref="ShellContextMenu"/> 
		/// class.
		/// </summary>
		/// 
		/// <param name="items">
		/// The items to which the context menu should refer.
		/// </param>
		public ContextShellMenu(ShellObject[] items) {
			Initialize(items);
		}

		public ContextShellMenu(Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser explorer, ShellViewGetItemObject items) {
			this.Explorer = explorer;
			Initialize(this.Explorer.GetShellView(), items);
		}

		public ContextShellMenu(Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser explorer, Boolean isOpenWith) {
			this.Explorer = explorer;
			if (isOpenWith)
				Initialize(this.Explorer.SelectedItems[0], isOpenWith);
			else
				Initialize(this.Explorer.NavigationLog.CurrentLocation);
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
			if ((m.Msg == (int)WindowsAPI.WndMsg.WM_COMMAND) && ((int)m.WParam >= m_CmdFirst)) {
				InvokeCommand((int)m.WParam - m_CmdFirst);
				return true;
			}
			else {
				if (m_ComInterface3 != null) {
					IntPtr result;
					if (m_ComInterface3.HandleMenuMsg2(m.Msg, m.WParam, m.LParam,
						out result) == HResult.Ok) {
						m.Result = result;
						return true;
					}
				}
				else if (m_ComInterface2 != null) {
					if (m_ComInterface2.HandleMenuMsg(m.Msg, m.WParam, m.LParam)
							== HResult.Ok) {
						m.Result = IntPtr.Zero;
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Invokes the Delete command on the shell item.
		/// </summary>
		public void InvokeDelete() {
			CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
			invoke.cbSize = Marshal.SizeOf(invoke);
			invoke.lpVerb = "delete";

			try {
				m_ComInterface.InvokeCommand(ref invoke);
			}
			catch (COMException e) {
				// Ignore the exception raised when the user cancels
				// a delete operation.
				if (e.ErrorCode != unchecked((int)0x800704C7)) throw;
			}
		}

		public void InvokeCommand(string verb) {
			CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
			invoke.cbSize = Marshal.SizeOf(invoke);
			invoke.lpVerb = verb;
			m_ComInterface.InvokeCommand(ref invoke);
		}

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
		/// <param name="menu">
		/// The menu to populate.
		/// </param>
		public void Populate(Menu menu) {
			RemoveShellMenuItems(menu);
			m_ComInterface.QueryContextMenu(menu.Handle, 0,
				m_CmdFirst, int.MaxValue, CMF.EXPLORE | CMF.CANRENAME | ((Control.ModifierKeys & Keys.Shift) != 0 ? CMF.EXTENDEDVERBS : 0));
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
		public void ShowContextMenu(Point pos, Boolean isNew = false) {
			using (ContextMenu menu = new ContextMenu()) {

				Populate(menu);
				int count = ShellNativeMethods.GetMenuItemCount(menu.Handle);
				MENUITEMINFO itemInfo = new MENUITEMINFO();

				itemInfo.cbSize = (uint)Marshal.SizeOf(itemInfo);
				itemInfo.fMask = (uint)MIIM.MIIM_FTYPE;
				for (int i = 0; i < count; i++) {
					if (ShellNativeMethods.GetMenuItemInfo(menu.Handle, i, true, ref itemInfo)) {
						var isSep = (itemInfo.fType & 2048) != 0;
						if (i == count - 1) {
							ShellNativeMethods.GetMenuItemInfo(menu.Handle, i, true, ref itemInfo);
							if ((itemInfo.fType & 2048) != 0) {
								ShellNativeMethods.DeleteMenu(menu.Handle, i, MF.MF_BYPOSITION);
							}
						}
						else if (i < count - 1)
							ShellNativeMethods.GetMenuItemInfo(menu.Handle, i + 1, true, ref itemInfo);
						if (isSep && (itemInfo.fType & 2048) != 0) {
							ShellNativeMethods.DeleteMenu(menu.Handle, i, MF.MF_BYPOSITION);
						}
					}
				}

				int command = ShellNativeMethods.TrackPopupMenuEx(menu.Handle,
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
						info = Encoding.Unicode.GetString(bytes, 0, index); //+ 1);

					if (!isNew) {
						if (info == "rename")
							Explorer.DoRename();
						else if (info == "cut")
							Explorer.DoCut();
						else
							InvokeCommand(command - m_CmdFirst);
					}
					else {
						if (String.IsNullOrEmpty(info)) {
							var newItems = GetNewContextMenuItems();
							InvokeCommand(m_ComInterface3, newItems[command - m_CmdFirst - 3], this.Explorer.NavigationLog.CurrentLocation.ParsingName, pos);

						}
						else {
							InvokeCommand(m_ComInterface3, info, this.Explorer.NavigationLog.CurrentLocation.ParsingName, pos);
						}
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

		public void InvokeCommand(IContextMenu3 iContextMenu, string cmd, string parentDir, Point ptInvoke) {
			CMINVOKECOMMANDINFOEX invoke = new CMINVOKECOMMANDINFOEX();
			invoke.Size = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFOEX));
			invoke.Verb = Marshal.StringToHGlobalAnsi(cmd);
			invoke.Directory = parentDir;
			invoke.VerbW = Marshal.StringToHGlobalUni(cmd);
			invoke.DirectoryW = parentDir;
			invoke.Mask = CMIC.Unicode | CMIC.PtInvoke |
				((Control.ModifierKeys & Keys.Control) != 0 ? CMIC.ControlDown : 0) |
				((Control.ModifierKeys & Keys.Shift) != 0 ? CMIC.ShiftDown : 0);
			POINT pt = new POINT();
			pt.x = ptInvoke.X;
			pt.y = ptInvoke.Y;
			invoke.InvokePoint = pt;
			invoke.ShowType = SW.ShowNormal;

			var res = iContextMenu.InvokeCommand(ref invoke);
		}


		/// <summary>
		/// Gets the underlying COM <see cref="IContextMenu"/> interface.
		/// </summary>
		public IContextMenu ComInterface {
			get { return m_ComInterface; }
			set { m_ComInterface = value; }
		}

		void Initialize(IShellView view, ShellViewGetItemObject items) {
			Object result = null;
			Guid iicm = typeof(IContextMenu).GUID;
			view.GetItemObject(items, ref iicm, out result);
			m_ComInterface = (IContextMenu)result;
			m_ComInterface2 = m_ComInterface as IContextMenu2;
			m_ComInterface3 = m_ComInterface as IContextMenu3;
			m_MessageWindow = new MessageWindow(this);
		}

		void Initialize(ShellObject[] items) {
			IntPtr[] pidls = new IntPtr[items.Length];
			ShellObject parent = null;
			Object result;

			for (int n = 0; n < items.Length; ++n) {
				pidls[n] = WindowsAPI.ILFindLastID(items[n].PIDL);

				if (parent == null) {
					if (items[n] == (ShellObject)KnownFolders.Desktop) {
						parent = (ShellObject)KnownFolders.Desktop;
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
			Guid iicm = typeof(IContextMenu).GUID;
			Microsoft.WindowsAPICodePack.Controls.WindowsForms.BHID.GetIShellFolder(parent).GetUIObjectOf(IntPtr.Zero, (uint)pidls.Length, pidls, ref iicm, 0, out result);
			m_ComInterface = (IContextMenu)result;
			m_ComInterface2 = m_ComInterface as IContextMenu2;
			m_ComInterface3 = m_ComInterface as IContextMenu3;
			m_MessageWindow = new MessageWindow(this);
		}

		void Initialize(ShellObject dirItem, bool isOpenWith = false) {
			object iContextMenu = null;
			if (isOpenWith)
				GetOpenWithContextMenu(dirItem, out iContextMenu, out m_ComInterface);
			else
				GetNewContextMenu(dirItem, out iContextMenu, out m_ComInterface);

			m_ComInterface2 = m_ComInterface as IContextMenu2;
			m_ComInterface3 = m_ComInterface as IContextMenu3;
			m_MessageWindow = new MessageWindow(this);
		}

		void InvokeCommand(int index) {
			const int SW_SHOWNORMAL = 1;
			CMINVOKECOMMANDINFO_ByIndex invoke = new CMINVOKECOMMANDINFO_ByIndex();
			invoke.cbSize = Marshal.SizeOf(invoke);
			invoke.iVerb = index;
			invoke.nShow = SW_SHOWNORMAL;
			m_ComInterface2.InvokeCommand(ref invoke);
		}

		void TagManagedMenuItems(Menu menu, int tag) {
			MENUINFO info = new MENUINFO();

			info.cbSize = Marshal.SizeOf(info);
			info.fMask = MIM.MIM_MENUDATA;
			info.dwMenuData = tag;

			foreach (MenuItem item in menu.MenuItems) {
				ShellNativeMethods.SetMenuInfo(item.Handle, ref info);
			}
		}

		public bool GetNewContextMenu(ShellObject item, out object iContextMenuPtr, out IContextMenu iContextMenu) {
			Guid CLSID_NewMenu = new Guid("{D969A300-E7FF-11d0-A93B-00A0C90F2719}");
			Guid iicm = typeof(IContextMenu).GUID;
			Guid iise = typeof(IShellExtInit).GUID;
			if (WindowsAPI.CoCreateInstance(
					ref CLSID_NewMenu,
					IntPtr.Zero,
					0x1,
					ref iicm,
					out iContextMenuPtr) == (int)HResult.Ok) {
				iContextMenu = iContextMenuPtr as IContextMenu;

				IntPtr iShellExtInitPtr;
				if (Marshal.QueryInterface(
					Marshal.GetIUnknownForObject(iContextMenuPtr),
					ref iise,
					out iShellExtInitPtr) == (int)HResult.Ok) {
					IShellExtInit iShellExtInit = Marshal.GetTypedObjectForIUnknown(
						iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

					try {
						iShellExtInit.Initialize(item.PIDL, IntPtr.Zero, 0);

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

					if (iContextMenuPtr != null) {
						Marshal.ReleaseComObject(iContextMenuPtr);
						iContextMenuPtr = null;
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

		public bool GetOpenWithContextMenu(ShellObject item, out object iContextMenuPtr, out IContextMenu iContextMenu) {
			Guid CLSID_OpenWith = new Guid(0x09799AFB, 0xAD67, 0x11d1, 0xAB, 0xCD, 0x00, 0xC0, 0x4F, 0xC3, 0x09, 0x36);
			Guid iicm = typeof(IContextMenu).GUID;
			Guid iise = typeof(IShellExtInit).GUID;
			if (WindowsAPI.CoCreateInstance(
					ref CLSID_OpenWith,
					IntPtr.Zero,
					0x1,
					ref iicm,
					out iContextMenuPtr) == (int)HResult.Ok) {
				iContextMenu = iContextMenuPtr as IContextMenu;

				IntPtr iShellExtInitPtr;
				if (Marshal.QueryInterface(
					Marshal.GetIUnknownForObject(iContextMenuPtr),
					ref iise,
					out iShellExtInitPtr) == (int)HResult.Ok) {
					IShellExtInit iShellExtInit = Marshal.GetTypedObjectForIUnknown(
						iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

					try {
						iShellExtInit.Initialize(item.PIDL, IntPtr.Zero, 0);

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

					if (iContextMenuPtr != null) {
						Marshal.ReleaseComObject(iContextMenuPtr);
						iContextMenuPtr = null;
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


		void RemoveShellMenuItems(Menu menu) {
			const int tag = 0xAB;
			List<int> remove = new List<int>();
			int count = ShellNativeMethods.GetMenuItemCount(menu.Handle);
			MENUINFO menuInfo = new MENUINFO();
			MENUITEMINFO itemInfo = new MENUITEMINFO();

			menuInfo.cbSize = Marshal.SizeOf(menuInfo);
			menuInfo.fMask = MIM.MIM_MENUDATA;
			itemInfo.cbSize = (uint)Marshal.SizeOf(itemInfo);
			itemInfo.fMask = (uint)MIIM.MIIM_ID | (uint)MIIM.MIIM_SUBMENU;

			// First, tag the managed menu items with an arbitary 
			// value (0xAB).
			TagManagedMenuItems(menu, tag);

			for (int n = 0; n < count; ++n) {
				ShellNativeMethods.GetMenuItemInfo(menu.Handle, n, true, ref itemInfo);

				if (itemInfo.hSubMenu == IntPtr.Zero) {
					// If the item has no submenu we can't get the tag, so 
					// check its ID to determine if it was added by the shell.
					if (itemInfo.wID >= m_CmdFirst) remove.Add(n);
				}
				else {
					ShellNativeMethods.GetMenuInfo(itemInfo.hSubMenu, ref menuInfo);
					if (menuInfo.dwMenuData != tag) remove.Add(n);
				}
			}

			// Remove the unmanaged menu items.
			remove.Reverse();
			foreach (int position in remove) {
				ShellNativeMethods.DeleteMenu(menu.Handle, position, MF.MF_BYPOSITION);
			}
		}

		class MessageWindow : Control {
			public MessageWindow(ContextShellMenu parent) {
				m_Parent = parent;
			}

			protected override void WndProc(ref Message m) {
				if (!m_Parent.HandleMenuMessage(ref m)) {
					base.WndProc(ref m);
				}
			}

			ContextShellMenu m_Parent;
		}

		MessageWindow m_MessageWindow;
		IContextMenu m_ComInterface;
		IContextMenu2 m_ComInterface2;
		IContextMenu3 m_ComInterface3;
		const int m_CmdFirst = 0x8000;
	}

}
