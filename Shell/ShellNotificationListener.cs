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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell {
	/// <summary>
	/// Listens for notifications of changes in the Windows Shell Namespace.
	/// </summary>
	public class ShellNotificationListener : Component {

		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="ShellNotificationListener"/> class.
		/// </summary>
		public ShellNotificationListener() {
			m_Window = new NotificationWindow(this);
		}

		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="ShellNotificationListener"/> class.
		/// </summary>
		public ShellNotificationListener(IContainer container) {
			container.Add(this);
			m_Window = new NotificationWindow(this);
		}

		/// <summary>
		/// Occurs when a drive is added.
		/// </summary>
		public event ShellItemEventHandler DriveAdded;

		/// <summary>
		/// Occurs when a drive is removed.
		/// </summary>
		public event ShellItemEventHandler DriveRemoved;

		/// <summary>
		/// Occurs when a folder is created.
		/// </summary>
		public event ShellItemEventHandler FolderCreated;

		/// <summary>
		/// Occurs when a folder is deleted.
		/// </summary>
		public event ShellItemEventHandler FolderDeleted;

		/// <summary>
		/// Occurs when a folder is renamed.
		/// </summary>
		public event ShellItemChangeEventHandler FolderRenamed;

		/// <summary>
		/// Occurs when a folder's contents are updated.
		/// </summary>
		public event ShellItemEventHandler FolderUpdated;

		/// <summary>
		/// Occurs when a non-folder item is created.
		/// </summary>
		public event ShellItemEventHandler ItemCreated;

		/// <summary>
		/// Occurs when a non-folder item is deleted.
		/// </summary>
		public event ShellItemEventHandler ItemDeleted;

		/// <summary>
		/// Occurs when a non-folder item is renamed.
		/// </summary>
		public event ShellItemChangeEventHandler ItemRenamed;

		/// <summary>
		/// Occurs when a non-folder item is updated.
		/// </summary>
		public event ShellItemEventHandler ItemUpdated;

		/// <summary>
		/// Occurs when the shared state for a folder changes.
		/// </summary>
		public event ShellItemEventHandler SharingChanged;

		/// <summary>
		/// Overrides the <see cref="Component.Dispose(bool)"/> method.
		/// </summary>
		/// <param name="disposing"/>
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			m_Window.Dispose();
		}

		class NotificationWindow : Control {
			public NotificationWindow(ShellNotificationListener parent) {
				SHChangeNotifyEntry notify = new SHChangeNotifyEntry();
				notify.pidl = ShellItem.Desktop.Pidl;
				notify.fRecursive = true;
				m_NotifyId = Shell32.SHChangeNotifyRegister(this.Handle,
					SHCNRF.InterruptLevel | SHCNRF.ShellLevel,
					SHCNE.ALLEVENTS, WM_SHNOTIFY, 1, ref notify);
				m_Parent = parent;
			}

			protected override void Dispose(bool disposing) {
				base.Dispose(disposing);
				Shell32.SHChangeNotifyUnregister(m_NotifyId);
			}

			protected override void WndProc(ref Message m) {
				if (m.Msg == WM_SHNOTIFY) {
					SHNOTIFYSTRUCT notify = (SHNOTIFYSTRUCT)
						Marshal.PtrToStructure(m.WParam,
							typeof(SHNOTIFYSTRUCT));

					switch ((SHCNE)m.LParam) {
						case SHCNE.CREATE:
							if (m_Parent.ItemCreated != null) {
								ShellItem item = new ShellItem(notify.dwItem1);
								m_Parent.ItemCreated(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));
							}
							break;

						case SHCNE.DELETE:
							if (m_Parent.ItemDeleted != null)
								m_Parent.ItemDeleted(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.DRIVEADD:
							if (m_Parent.DriveAdded != null)
								m_Parent.DriveAdded(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.DRIVEREMOVED:
							if (m_Parent.DriveRemoved != null)
								m_Parent.DriveRemoved(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.MKDIR:
							if (m_Parent.FolderCreated != null)
								m_Parent.FolderCreated(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.RMDIR:
							if (m_Parent.FolderDeleted != null)
								m_Parent.FolderDeleted(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.UPDATEDIR:
							if (m_Parent.FolderUpdated != null)
								m_Parent.FolderUpdated(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.UPDATEITEM:
							if (m_Parent.ItemUpdated != null)
								m_Parent.ItemUpdated(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
						case SHCNE.RENAMEFOLDER:
							if (m_Parent.FolderRenamed != null)
								m_Parent.FolderRenamed(m_Parent, new ShellItemChangeEventArgs(new ShellItem(notify.dwItem1), new ShellItem(notify.dwItem2)));

							break;
						case SHCNE.RENAMEITEM:
							if (m_Parent.ItemRenamed != null)
								m_Parent.ItemRenamed(m_Parent, new ShellItemChangeEventArgs(new ShellItem(notify.dwItem1), new ShellItem(notify.dwItem2)));


							break;
						case SHCNE.NETSHARE:
						case SHCNE.NETUNSHARE:
							if (m_Parent.SharingChanged != null)
								m_Parent.SharingChanged(m_Parent, new ShellItemEventArgs(new ShellItem(notify.dwItem1)));

							break;
					}
				}
				else {
					base.WndProc(ref m);
				}
			}

			uint m_NotifyId;
			ShellNotificationListener m_Parent;
			const int WM_SHNOTIFY = 0x401;
		}

		NotificationWindow m_Window;
	}

	/// <summary>
	/// Provides information of changes in the Windows Shell Namespace.
	/// </summary>
	public class ShellItemEventArgs : EventArgs {
		/// <summary>
		/// The ShellItem that has changed.
		/// </summary>
		public ShellItem Item { get; private set; }

		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="ShellItemEventArgs"/> class.
		/// </summary>
		/// 
		/// <param name="item">
		/// The ShellItem that has changed.
		/// </param>
		public ShellItemEventArgs(ShellItem item) { Item = item; }
	}

	/// <summary>
	/// Provides information of changes in the Windows Shell Namespace.
	/// </summary>
	public class ShellItemChangeEventArgs : EventArgs {

		/// <summary>
		/// The ShellItem before the change.
		/// </summary>
		public ShellItem OldItem { get; private set; }

		/// <summary>
		/// The ShellItem after the change.
		/// </summary>
		public ShellItem NewItem { get; private set; }


		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="ShellItemChangeEventArgs"/> class.
		/// </summary>
		/// 
		/// <param name="oldItem">
		/// The ShellItem before the change
		/// </param>
		/// 
		/// <param name="newItem">
		/// The ShellItem after the change
		/// </param>
		public ShellItemChangeEventArgs(ShellItem oldItem, ShellItem newItem) {
			OldItem = oldItem;
			NewItem = newItem;
		}
	}

	/// <summary>
	/// Represents the method that handles change notifications from
	/// <see cref="ShellNotificationListener"/>
	/// </summary>
	/// 
	/// <param name="sender">
	/// The source of the event.
	/// </param>
	/// 
	/// <param name="e">
	/// A <see cref="ShellItemEventArgs"/> that contains the data
	/// for the event.
	/// </param>
	public delegate void ShellItemEventHandler(object sender, ShellItemEventArgs e);

	/// <summary>
	/// Represents the method that handles change notifications from
	/// <see cref="ShellNotificationListener"/>
	/// </summary>
	/// 
	/// <param name="sender">
	/// The source of the event.
	/// </param>
	/// 
	/// <param name="e">
	/// A <see cref="ShellItemChangeEventArgs"/> that contains the data
	/// for the event.
	/// </param>
	public delegate void ShellItemChangeEventHandler(object sender, ShellItemChangeEventArgs e);
}
