using Microsoft.WindowsAPICodePack.Controls;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell.ExplorerBrowser {
	[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
	public class SubclassHWND : NativeWindow {
		public Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList jumbo = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList(ImageListSize.Jumbo);
		public Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList extra = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList(ImageListSize.ExtraLarge);
		public Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList large = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList(ImageListSize.Large);
		public Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList syssmall = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList(ImageListSize.SystemSmall);
		public Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList small = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ImageList(ImageListSize.Small);
		private Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser Browser;
		private IntPtr SysListviewhandle;
		public const int CDRF_DODEFAULT = 0x00000000;
		public const int CDRF_NEWFONT = 0x00000002;
		public const int CDRF_SKIPDEFAULT = 0x00000004;
		public const int CDRF_DOERASE = 0x00000008; // draw the background
		public const int CDRF_NOTIFYPOSTPAINT = 0x00000010;
		public const int CDRF_NOTIFYITEMDRAW = 0x00000020;
		public const int CDRF_NOTIFYSUBITEMDRAW = 0x00000020;

		public const int CDDS_PREPAINT = 0x00000001;
		public const int CDDS_POSTPAINT = 0x00000002;
		public const int CDDS_ITEM = 0x00010000;
		public const int CDDS_SUBITEM = 0x00020000;
		public const int CDDS_ITEMPREPAINT = (CDDS_ITEM | CDDS_PREPAINT);
		public const int CDDS_ITEMPOSTPAINT = (CDDS_ITEM | CDDS_POSTPAINT);

		public SubclassHWND(Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser browser, IntPtr sysListviewHandle) {
			this.Browser = browser;
			this.SysListviewhandle = sysListviewHandle;
		}
		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		protected override void WndProc(ref Message m) {
			if (m.Msg == (int)BExplorer.Shell.Interop.WM.WM_CONTEXTMENU) {
				this.Browser.IsRenameStarted = true;
				ShellObject[] dirs = this.Browser.SelectedItems.ToArray();
				ContextShellMenu cm1 = new ContextShellMenu(this.Browser, this.Browser.SelectedItems.Count > 0 ? ShellViewGetItemObject.Selection : ShellViewGetItemObject.Background);
				cm1.ShowContextMenu(Cursor.Position);//new System.Drawing.Point(GetCursorPosition().X, GetCursorPosition().Y));
				return;
			}
			if (m.Msg == (int)BExplorer.Shell.Interop.WM.WM_EXITMENULOOP) {
				//this.IsRenameStarted = false;
			}
			if (m.Msg == 78) {
				//var r = (WindowsAPI.NMHDR*)(IntPtr)lParam;
				WindowsAPI.NMHDR nmhdr = new WindowsAPI.NMHDR();
				nmhdr = (WindowsAPI.NMHDR)m.GetLParam(nmhdr.GetType());
				switch ((int)nmhdr.code) {
					case WNM.LVN_GETINFOTIP:
						//TODO: Write here the code for the tooltip flyout
						break;
					case WNM.NM_CUSTOMDRAW:
						if (!this.Browser.NavigationLog.CurrentLocation.IsSearchFolder) {
							if (nmhdr.hwndFrom == this.SysListviewhandle) {

								var nmlvcd = WindowsAPI.PtrToStructure<WindowsAPI.NMLVCUSTOMDRAW>(m.LParam);
								var index = (int)nmlvcd.nmcd.dwItemSpec;
								var hdc = nmlvcd.nmcd.hdc;

								Guid IIFV2 = typeof(IShellItem).GUID;
								IFolderView2 fv2 = this.Browser.GetFolderView2();
								IShellItem item = null;
								try {
									fv2.GetItem(index, ref IIFV2, out item);
								}
								catch (Exception) {

								}

								object ext = null;
								ShellObject itemobj = null;
								if (item != null) {
									itemobj = ShellObjectFactory.Create(item);

									ext = itemobj.Properties.System.FileExtension.Value;
								}
								Color? textColor = null;
								if (this.Browser.LVItemsColorCodes != null && this.Browser.LVItemsColorCodes.Count > 0) {
									if (ext != null) {
										var extItemsAvailable = this.Browser.LVItemsColorCodes.Where(c => c.ExtensionList.Contains(ext.ToString())).Count() > 0;
										if (extItemsAvailable) {
											var color = this.Browser.LVItemsColorCodes.Where(c => c.ExtensionList.ToLowerInvariant().Contains(ext.ToString().ToLowerInvariant())).Select(c => c.TextColor).SingleOrDefault();
											textColor = color;
										}
									}
								}

								switch (nmlvcd.nmcd.dwDrawStage) {
									case CDDS_PREPAINT:
										m.Result = (IntPtr)CDRF_NOTIFYITEMDRAW;
										break;
									case CDDS_ITEMPREPAINT:
										// call default procedure in case system might do custom drawing and set special colors

										if (textColor != null) {
											nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);

											m.Result = (IntPtr)(CDRF_NEWFONT | CDRF_NOTIFYPOSTPAINT | CDRF_NOTIFYSUBITEMDRAW);
										}
										else {
											m.Result = (IntPtr)(CDRF_NOTIFYPOSTPAINT | CDRF_NOTIFYSUBITEMDRAW);
										}
										break;
									case CDDS_ITEMPREPAINT | CDDS_SUBITEM:
										// before a subitem drawn
										if ((nmlvcd.nmcd.uItemState & (WindowsAPI.CDIS.HOT | WindowsAPI.CDIS.DROPHILITED)) != 0 || 0 != WindowsAPI.SendMessage(this.SysListviewhandle, BExplorer.Shell.Interop.LVM.GETITEMSTATE, index, (int)WindowsAPI.LVIS.LVIS_SELECTED)) {
											// hot, drophilited or selected.
											if (nmlvcd.iSubItem == 0) {
												// do default to draw hilite bar
												m.Result = (IntPtr)CDRF_DODEFAULT;
											}
											else {
												// remaining region of a hilted item need to be drawn
												m.Result = (IntPtr)CDRF_NOTIFYPOSTPAINT;
											}
										}
										else if ((nmlvcd.iSubItem == 0 && nmlvcd.nmcd.uItemState.HasFlag(WindowsAPI.CDIS.FOCUS))) {
											// if the subitem in selected column, or first item with focus
											m.Result = (IntPtr)CDRF_NOTIFYPOSTPAINT;
										}
										else {
											if (textColor != null) {
												nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
												Marshal.StructureToPtr(nmlvcd, m.LParam, false);
												m.Result = (IntPtr)CDRF_NEWFONT;
											}
											else {
												m.Result = (IntPtr)CDRF_DODEFAULT;
											}
										}
										break;
									case CDDS_ITEMPOSTPAINT:
										//base.WndProc(ref m);
										if (nmlvcd.clrTextBk != 0) {
											var iconBounds = new BExplorer.Shell.Interop.User32.RECT();

											iconBounds.Left = 1;

											WindowsAPI.SendMessage(this.SysListviewhandle, BExplorer.Shell.Interop.LVM.GETITEMRECT, index, ref iconBounds);

											//using (Graphics graphics = Graphics.FromHdc(nmlvcd.nmcd.hdc))
											//{
											//  graphics.Clip = new Region(iconBounds.ToRectangle()); ;
											//  graphics.FillRectangle(Brushes.Black, new Rectangle(iconBounds.left, iconBounds.bottom - 20, 20, 20));
											//}
											if (itemobj.IsShared) {
												if (this.Browser.ContentOptions.ViewMode == ExplorerBrowserViewMode.Details || this.Browser.ContentOptions.ViewMode == ExplorerBrowserViewMode.List || this.Browser.ContentOptions.ViewMode == ExplorerBrowserViewMode.SmallIcon)
													small.DrawOverlay(hdc, 1, new Point(iconBounds.Left, iconBounds.Bottom - 16));
												else {
													if (this.Browser.ContentOptions.ThumbnailSize > 180)
														jumbo.DrawOverlay(hdc, 1, new Point(iconBounds.Left, iconBounds.Bottom - this.Browser.ContentOptions.ThumbnailSize / 3), this.Browser.ContentOptions.ThumbnailSize / 3);
													else
														if (this.Browser.ContentOptions.ThumbnailSize > 64)
															extra.DrawOverlay(hdc, 1, new Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
														else
															large.DrawOverlay(hdc, 1, new Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
												}
											}
										}
										m.Result = (IntPtr)CDRF_SKIPDEFAULT;
										break;
								}
								if (itemobj != null)
									itemobj.Dispose();
								return;
							}
						}
						break;
				}
				//base.WndProc(ref m);
			}
			//return;
			// Perform whatever custom processing you must have for this message
			//System.Diagnostics.Debug.WriteLine(m.ToString());
			// forward message to base WndProc
			base.WndProc(ref m);
		}
	}
}
