using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {
	public partial class MessageHandler : Form {
		public ShellNotifications Notifications = new ShellNotifications();
		public ShellNotifications NotificationsGlobal = new ShellNotifications();
		public ShellNotifications NotificationsRB = new ShellNotifications();
		private ShellView _ParentShellView;
		public MessageHandler(ShellView parentShellView) {
			this._ParentShellView = parentShellView;
			InitializeComponent();
		}
		public void ReinitNotify(IListItemEx item) {
			if (Notifications != null) {
				Notifications.UnregisterChangeNotify();
				Notifications.RegisterChangeNotify(this.Handle, item, false);
			}
		}
		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			this.NotificationsRB.RegisterChangeNotify(this.Handle, ShellNotifications.CSIDL.CSIDL_BITBUCKET, true);
			this.NotificationsGlobal.RegisterChangeNotify(this.Handle, ShellNotifications.CSIDL.CSIDL_DESKTOP, false);
		}
		protected override void OnHandleDestroyed(EventArgs e) {
			NotificationsRB.UnregisterChangeNotify();
			Notifications.UnregisterChangeNotify();
			NotificationsGlobal.UnregisterChangeNotify();
			base.OnHandleDestroyed(e);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			if (m.Msg == ShellNotifications.WM_SHNOTIFY) {
				if (NotificationsRB.NotificationReceipt(m.WParam, m.LParam)) {
          this._ParentShellView.RaiseRecycleBinUpdated();
					foreach (NotifyInfos info in NotificationsRB.NotificationsReceived.ToArray()) {
						NotificationsRB.NotificationsReceived.Remove(info);
					}
				}
				if (NotificationsGlobal.NotificationReceipt(m.WParam, m.LParam)) {
					foreach (NotifyInfos info in NotificationsGlobal.NotificationsReceived.ToArray()) {
						switch (info.Notification) {
							case ShellNotifications.SHCNE.SHCNE_RENAMEITEM:
								break;
							case ShellNotifications.SHCNE.SHCNE_CREATE:
								break;
							case ShellNotifications.SHCNE.SHCNE_DELETE:
								break;
							case ShellNotifications.SHCNE.SHCNE_MKDIR:
								break;
							case ShellNotifications.SHCNE.SHCNE_RMDIR:
								break;
							case ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED:
							case ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED:
								if (this._ParentShellView.CurrentFolder.ParsingName == KnownFolders.Computer.ParsingName) {
									var objMedia = new ShellItem(info.Item1);
									var exisitingItem = this._ParentShellView.Items.Where(w => w.Equals(objMedia)).SingleOrDefault();
									if (exisitingItem != null)
										this._ParentShellView.UpdateItem(this._ParentShellView.Items.IndexOf(exisitingItem));

									objMedia.Dispose();
								}
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED:
								//var obj = new ShellItem(info.Item1);
								//if (this._ParentShellView.CurrentFolder.Equals(KnownFolders.Computer)) {
								//	this._ParentShellView.Items.Remove(obj);
								//	this._ParentShellView.ItemsHashed.Remove(obj);
								//	if (this._ParentShellView.IsGroupsEnabled) this._ParentShellView.SetGroupOrder(false);

								//	User32.SendMessage(this._ParentShellView.LVHandle, MSG.LVM_SETITEMCOUNT, this._ParentShellView.Items.Count, 0);
								//}
								//this._ParentShellView.RaiseItemUpdated(ItemUpdateType.DriveRemoved, null, obj, -1);
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEADD:
								if (this._ParentShellView.CurrentFolder.Equals(KnownFolders.Computer)) {
									//this._ParentShellView.InsertNewItem(new ShellItem(info.Item1));
								}
								break;
							case ShellNotifications.SHCNE.SHCNE_NETSHARE:
								break;
							case ShellNotifications.SHCNE.SHCNE_NETUNSHARE:
								break;
							case ShellNotifications.SHCNE.SHCNE_ATTRIBUTES:
								break;
							case ShellNotifications.SHCNE.SHCNE_UPDATEDIR:
								break;
							case ShellNotifications.SHCNE.SHCNE_UPDATEITEM:
								break;
							case ShellNotifications.SHCNE.SHCNE_SERVERDISCONNECT:
								break;
							case ShellNotifications.SHCNE.SHCNE_UPDATEIMAGE:
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEADDGUI:
								break;
							case ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER:
								break;
							case ShellNotifications.SHCNE.SHCNE_FREESPACE:
								break;
							case ShellNotifications.SHCNE.SHCNE_EXTENDED_EVENT:
								break;
							case ShellNotifications.SHCNE.SHCNE_ASSOCCHANGED:
								break;
							case ShellNotifications.SHCNE.SHCNE_DISKEVENTS:
								break;
							case ShellNotifications.SHCNE.SHCNE_GLOBALEVENTS:
								break;
							case ShellNotifications.SHCNE.SHCNE_ALLEVENTS:
								break;
							case ShellNotifications.SHCNE.SHCNE_INTERRUPT:
								break;
							default:
								break;
						}
						this.NotificationsGlobal.NotificationsReceived.Remove(info);
					}
				}
				if (Notifications.NotificationReceipt(m.WParam, m.LParam)) {
					foreach (NotifyInfos info in Notifications.NotificationsReceived.ToArray()) {
						switch (info.Notification) {
							case ShellNotifications.SHCNE.SHCNE_RENAMEITEM:
								break;
							case ShellNotifications.SHCNE.SHCNE_CREATE:
							case ShellNotifications.SHCNE.SHCNE_MKDIR:
                var obj = FileSystemListItem.ToFileSystemItem(this._ParentShellView.LVHandle, info.Item1);
								var existingItem = this._ParentShellView.Items.SingleOrDefault(s => s.Equals(obj));
								if (existingItem == null && (obj.Parent != null && obj.Parent.Equals(this._ParentShellView.CurrentFolder))) {
									if (obj.Extension.ToLowerInvariant() != ".tmp") {
										var itemIndex = this._ParentShellView.InsertNewItem(obj);
										this._ParentShellView.RaiseItemUpdated(ItemUpdateType.Created, null, obj, itemIndex);
									}
									else {
										var affectedItem = this._ParentShellView.Items.SingleOrDefault(s => s.Equals(obj.Parent));
										if (affectedItem != null) {
											var index = this._ParentShellView.Items.IndexOf(affectedItem);
											this._ParentShellView.RefreshItem(index, true);
										}
									}
								}
								break;
							case ShellNotifications.SHCNE.SHCNE_RMDIR:
							case ShellNotifications.SHCNE.SHCNE_DELETE:
                var objDeleteF = FileSystemListItem.ToFileSystemItem(this._ParentShellView.LVHandle, info.Item1);
								if (!String.IsNullOrEmpty(objDeleteF.ParsingName)) {
									if (objDeleteF.ParsingName.EndsWith(".library-ms") && this._ParentShellView.IsLibraryInModify) {
										this._ParentShellView.IsLibraryInModify = false;
										break;
									}
									var theItem = this._ParentShellView.Items.ToArray().SingleOrDefault(s => s.Equals(objDeleteF));
									if (theItem != null) {
										this._ParentShellView.Items.Remove(theItem);
										if (this._ParentShellView.IsGroupsEnabled) this._ParentShellView.SetGroupOrder(false);
                    var col = this._ParentShellView.AllAvailableColumns.Where(w => w.ID == this._ParentShellView.LastSortedColumnId).SingleOrDefault();
										this._ParentShellView.SetSortCollumn(col, this._ParentShellView.LastSortOrder, false);
									}
								}
								break;
							case ShellNotifications.SHCNE.SHCNE_UPDATEDIR:
							case ShellNotifications.SHCNE.SHCNE_UPDATEITEM:
								var objUpdate = FileSystemListItem.ToFileSystemItem(this._ParentShellView.LVHandle, info.Item1);
								var exisitingItem = this._ParentShellView.Items.Where(w => w.Equals(objUpdate)).SingleOrDefault();
								if (exisitingItem != null)
									this._ParentShellView.RefreshItem(this._ParentShellView.Items.IndexOf(exisitingItem), true);

                if (objUpdate != null && this._ParentShellView.CurrentFolder != null && objUpdate.Equals(this._ParentShellView.CurrentFolder))
									this._ParentShellView.UnvalidateDirectory();

								objUpdate.Dispose();
								break;
							case ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED:
								break;
							case ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED:
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED:
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEADD:
								break;
							case ShellNotifications.SHCNE.SHCNE_NETSHARE:
							case ShellNotifications.SHCNE.SHCNE_NETUNSHARE:
							case ShellNotifications.SHCNE.SHCNE_ATTRIBUTES:
                var objNetA = FileSystemListItem.ToFileSystemItem(this._ParentShellView.LVHandle, info.Item1);
								var exisitingItemNetA = this._ParentShellView.ItemsHashed.Where(w => w.Key.Equals(objNetA)).SingleOrDefault();
								if (exisitingItemNetA.Key != null) {
									this._ParentShellView.RefreshItem(exisitingItemNetA.Value, true);
									//this._ParentShellView.RaiseItemUpdated(ItemUpdateType.Updated, null, objNetA, exisitingItemNetA.Value);
								}
								break;
							case ShellNotifications.SHCNE.SHCNE_SERVERDISCONNECT:
								break;
							case ShellNotifications.SHCNE.SHCNE_UPDATEIMAGE:
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEADDGUI:
								break;
							case ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER:
								break;
							case ShellNotifications.SHCNE.SHCNE_FREESPACE:
								//this._ParentShellView.UnvalidateDirectory();
								break;
							case ShellNotifications.SHCNE.SHCNE_EXTENDED_EVENT:
								break;
							case ShellNotifications.SHCNE.SHCNE_ASSOCCHANGED:
								break;
							case ShellNotifications.SHCNE.SHCNE_DISKEVENTS:
								break;
							case ShellNotifications.SHCNE.SHCNE_GLOBALEVENTS:
								break;
							case ShellNotifications.SHCNE.SHCNE_ALLEVENTS:
								break;
							case ShellNotifications.SHCNE.SHCNE_INTERRUPT:
								break;
							default:
								break;
						}

						//TODO: Should this be and Else If(...)?
						if (info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER || info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEITEM) {
              var obj1 = FileSystemListItem.ToFileSystemItem(this._ParentShellView.LVHandle, info.Item1); 
              var obj2 = FileSystemListItem.ToFileSystemItem(this._ParentShellView.LVHandle, info.Item2);
              if (!String.IsNullOrEmpty(obj1.ParsingName) && !String.IsNullOrEmpty(obj2.ParsingName))
                this._ParentShellView.UpdateItem(obj1, obj2);
              this._ParentShellView.IsRenameInProgress = false;
						}

						if (Notifications.NotificationsReceived.Contains(info))
							Notifications.NotificationsReceived.Remove(info);
					}
				}
				this._ParentShellView.Focus();
			}
		}
	}
}
