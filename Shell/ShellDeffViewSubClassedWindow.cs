using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BExplorer.Shell
{
	public class ShellDeffViewSubClassedWindow : NativeWindow
	{
		public ImageList jumbo = new ImageList(ImageListSize.Jumbo);
		public ImageList extra = new ImageList(ImageListSize.ExtraLarge);
		public ImageList large = new ImageList(ImageListSize.Large);
		public ImageList syssmall = new ImageList(ImageListSize.SystemSmall);
		public ImageList small = new ImageList(ImageListSize.Small);
		private ShellView Browser;
		public IntPtr SysListviewhandle;
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
		public ShellDeffViewSubClassedWindow(IntPtr handle, IntPtr sysListViewhandle, ShellView view)
		{
			//this.AssignHandle(handle);
			SysListviewhandle = sysListViewhandle;
			Browser = view;
		}

		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == (int)WM.WM_CONTEXTMENU)
			{
				//this.Browser.IsRenameStarted = true;
				ShellItem[] dirs = this.Browser.SelectedItems;
				ShellContextMenu cm1 = new ShellContextMenu(dirs);
				cm1.ShowContextMenu(this.Browser, Cursor.Position);
				return;
			}
			//if (m.Msg == (int)WindowsAPI.WndMsg.WM_EXITMENULOOP)
			//{
			//	//this.IsRenameStarted = false;
			//}
			base.WndProc(ref m);
			if (m.Msg == 78)
			{
				//var r = (WindowsAPI.NMHDR*)(IntPtr)lParam;
				User32.NMHDR nmhdr = new User32.NMHDR();
				nmhdr = (User32.NMHDR)m.GetLParam(nmhdr.GetType());
				switch ((int)nmhdr.code)
				{
					case WNM.LVN_GETINFOTIP:
						//TODO: Write here the code for the tooltip flyout
						break;
					case WNM.LVN_ITEMCHANGED:
						//User32.SendMessage(Browser.ShellListViewHandle, 0x127, 0x10001, 0);
						break;
					case WNM.NM_CUSTOMDRAW:
						//if (!this.Browser.NavigationLog.CurrentLocation.IsSearchFolder)
						{
							if (nmhdr.hwndFrom == Browser.ShellListViewHandle)
							{
								User32.NMLVCUSTOMDRAW nmlvcd = new User32.NMLVCUSTOMDRAW();
								nmlvcd = (User32.NMLVCUSTOMDRAW)m.GetLParam(nmlvcd.GetType());
								var index = (int)nmlvcd.nmcd.dwItemSpec;
								var hdc = nmlvcd.nmcd.hdc;

								Guid IIFV2 = typeof(IShellItem).GUID;
								IFolderView2 fv2 = this.Browser.FolderView2;
								IShellItem item = null;
								try
								{
									fv2.GetItem(index, ref IIFV2, out item);
								}
								catch (Exception)
								{

								}

								object ext = null;
								ShellItem itemobj = null;
								if (item != null)
								{
									itemobj = new ShellItem(item);

									ext = Path.GetExtension(itemobj.ParsingName);
								}
								Color? textColor = null;
								if (this.Browser.LVItemsColorCodes != null && this.Browser.LVItemsColorCodes.Count > 0)
								{
									if (ext != null && ext != String.Empty)
									{
										var extItemsAvailable = this.Browser.LVItemsColorCodes.Where(c => c.ExtensionList.Contains(ext.ToString())).Count() > 0;
										if (extItemsAvailable)
										{
											var color = this.Browser.LVItemsColorCodes.Where(c => c.ExtensionList.ToLowerInvariant().Contains(ext.ToString().ToLowerInvariant())).Select(c => c.TextColor).SingleOrDefault();
											textColor = color;
										}
									}
								}

								switch (nmlvcd.nmcd.dwDrawStage)
								{
									case CDDS_PREPAINT:
										//User32.SendMessage(Browser.ShellListViewHandle, User32.WM_CHANGEUISTATE, User32.MAKELONG(1, 1), 0);
										m.Result = (IntPtr)CDRF_NOTIFYITEMDRAW;
										break;
									case CDDS_ITEMPREPAINT:
										//User32.SendMessage(Browser.ShellListViewHandle, User32.WM_CHANGEUISTATE, User32.MAKELONG(1, 1), 0);
										// call default procedure in case system might do custom drawing and set special colors
										//User32.SendMessage(this.SysListviewhandle, User32.WM_CHANGEUISTATE, User32.MakeLong(User32.UIS_SET, User32.UISF_HIDEFOCUS), 0);
										//base.WndProc(ref m);
										if (textColor != null)
										{
											nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);

											m.Result = (IntPtr)(CDRF_NEWFONT | CDRF_NOTIFYPOSTPAINT | CDRF_NOTIFYSUBITEMDRAW);
										}
										else
										{
											m.Result = (IntPtr)(CDRF_NOTIFYPOSTPAINT | CDRF_NOTIFYSUBITEMDRAW);
										}
										break;
									case CDDS_ITEMPREPAINT | CDDS_SUBITEM:
										//User32.SendMessage(Browser.ShellListViewHandle, User32.WM_CHANGEUISTATE, User32.MAKELONG(1, 1), 0);
										// before a subitem drawn
										if ((nmlvcd.nmcd.uItemState & (CDIS.HOT | CDIS.DROPHILITED)) != 0 || 0 != User32.SendMessage(this.SysListviewhandle, MSG.LVM_GETITEMSTATE, index, (int)LVIS.LVIS_SELECTED))
										{
											// hot, drophilited or selected.
											if (nmlvcd.iSubItem == 0)
											{
												// do default to draw hilite bar
												m.Result = (IntPtr)CDRF_DODEFAULT;
											}
											else
											{
												// remaining region of a hilted item need to be drawn
												m.Result = (IntPtr)CDRF_NOTIFYPOSTPAINT;
											}
										}
										else if ((nmlvcd.iSubItem == 0 && nmlvcd.nmcd.uItemState.HasFlag(CDIS.FOCUS)))
										{
											// if the subitem in selected column, or first item with focus
											m.Result = (IntPtr)CDRF_NOTIFYPOSTPAINT;
										}
										else
										{
											if (textColor != null)
											{
												nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
												Marshal.StructureToPtr(nmlvcd, m.LParam, false);
												m.Result = (IntPtr)CDRF_NEWFONT;
											}
											else
											{
												m.Result = (IntPtr)CDRF_DODEFAULT;
											}
										}
										break;
									case CDDS_ITEMPOSTPAINT:
										//base.WndProc(ref m);
										//User32.SendMessage(this.SysListviewhandle, User32.WM_CHANGEUISTATE, User32.MakeLong(User32.UIS_SET, User32.UISF_HIDEFOCUS), 0);
										if (nmlvcd.clrTextBk != 0)
										{
											var iconBounds = new User32.RECT();

											iconBounds.Left = 1;

											User32.SendMessage(this.SysListviewhandle, MSG.LVM_GETITEMRECT, index, ref iconBounds);

											if (itemobj.IsShared)
											{
												if (this.Browser.View == ShellViewStyle.Details || this.Browser.View == ShellViewStyle.List || this.Browser.View == ShellViewStyle.SmallIcon)
													small.DrawOverlay(hdc, 1, new Point(iconBounds.Left, iconBounds.Bottom - 16));
												else
												{
													if (this.Browser.ThumbnailSize > 180)
														jumbo.DrawOverlay(hdc, 1, new Point(iconBounds.Left, iconBounds.Bottom - this.Browser.ThumbnailSize / 3), this.Browser.ThumbnailSize / 3);
													else
														if (this.Browser.ThumbnailSize > 64)
															extra.DrawOverlay(hdc, 1, new Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
														else
															large.DrawOverlay(hdc, 1, new Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
												}
											}
											//
										}
										m.Result = (IntPtr)CDRF_SKIPDEFAULT;
										break;
								}
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
			
		}

	}
}
