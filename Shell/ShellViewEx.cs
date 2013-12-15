using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Microsoft.Test.Tools.WicCop.InteropServices.ComTypes;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Interop;

namespace BExplorer.Shell
{
	public partial class ShellView : UserControl
	{

		public ShellItem[] Items { get; set; }

		int _iconSize;
		public int IconSize {
			get
			{
				return _iconSize;
			}
		}
		CancellationToken token;
		public IntPtr LVHandle { get; set; }

		Thread thumb;
		public ShellView()
		{
			InitializeComponent();
      //thumb = new Thread(LoadIcon);
      //thumb.IsBackground = true;
      //thumb.Start();
			m_History = new ShellHistory();
			token = tokenSource.Token;
			//SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		}

		/// <summary>
		/// Occurs when the control gains focus
		/// </summary>
		public event EventHandler GotFocus;

		/// <summary>
		/// Occurs when the control loses focus
		/// </summary>
		public event EventHandler LostFocus;

		/// <summary>
		/// Occurs when the <see cref="ShellView"/> control navigates to a 
		/// new folder.
		/// </summary>
		public event EventHandler<NavigatedEventArgs> Navigated;


		/// <summary>
		/// Occurs when the <see cref="ShellView"/>'s current selection 
		/// changes.
		/// </summary>
		/// 
		/// <remarks>
		/// <b>Important:</b> When <see cref="ShowWebView"/> is set to 
		/// <see langref="true"/>, this event will not occur. This is due to 
		/// a limitation in the underlying windows control.
		/// </remarks>
		public event EventHandler SelectionChanged;

		public event EventHandler<ViewChangedEventArgs> ViewStyleChanged;

		public List<Collumns> Collumns = new List<Collumns>();
       
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			System.Windows.Forms.ImageList il = new System.Windows.Forms.ImageList();
			il.ImageSize = new System.Drawing.Size(48, 48);
			System.Windows.Forms.ImageList ils = new System.Windows.Forms.ImageList();
			ils.ImageSize = new System.Drawing.Size(16, 16);
			
			ComCtl32.INITCOMMONCONTROLSEX icc = new ComCtl32.INITCOMMONCONTROLSEX();
			icc.dwSize = Marshal.SizeOf(typeof(ComCtl32.INITCOMMONCONTROLSEX));
			icc.dwICC = 1;
			var res = ComCtl32.InitCommonControlsEx(ref icc);
			this.LVHandle = User32.CreateWindowEx(0, "SysListView32", "", User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_CLIPCHILDREN | User32.WindowStyles.WS_CLIPSIBLINGS | 
																																		(User32.WindowStyles)User32.LVS_EDITLABELS | (User32.WindowStyles)User32.LVS_OWNERDATA | (User32.WindowStyles)User32.LVS_SHOWSELALWAYS, 
																																		0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, this.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			User32.ShowWindow(this.LVHandle, User32.ShowWindowCommands.Show);


			this.AddDefaultColumns();

			
			IntPtr headerhandle = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETHEADER, 0, 0);

			for (int i = 0; i < this.Collumns.Count; i++)
			{
				this.Collumns[i].SetSplitButton(headerhandle, i);
			}
			
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
			UxTheme.SetWindowTheme(this.LVHandle, "Explorer", 0);

			this.View = ShellViewStyle.Medium;

			//Navigate((ShellItem)KnownFolders.Desktop);
			CurrentFolder = (ShellItem)KnownFolders.Desktop;

			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.HeaderInAllViews, (int)ListViewExtendedStyles.HeaderInAllViews);
			//WinAPI.SendMessage(handle, WinAPI.LVM.LVM_SetExtendedStyle, (int)WinAPI.ListViewExtendedStyles.LVS_EX_AUTOAUTOARRANGE, (int)WinAPI.ListViewExtendedStyles.LVS_EX_AUTOAUTOARRANGE);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER, (int)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.FullRowSelect, (int)ListViewExtendedStyles.FullRowSelect);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.HeaderDragDrop, (int)ListViewExtendedStyles.HeaderDragDrop);

		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.Invalidate();
			this.Refresh();
			User32.MoveWindow(this.LVHandle, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, true);
		}

		//protected override void OnResize(EventArgs e)
		//{
		//	base.OnResize(e);
		//	//User32.SetWindowPos(this.LVHandle, IntPtr.Zero, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, 0);
		//	
		//}

		internal void OnSelectionChanged()
		{

			if (SelectionChanged != null)
			{
				SelectionChanged(this, EventArgs.Empty);
			}

		}

		internal void OnGotFocus()
		{
			if (GotFocus != null)
			{
				GotFocus(this, EventArgs.Empty);
			}
		}

		internal void OnLostFocus()
		{
			if (LostFocus != null)
			{
				LostFocus(this, EventArgs.Empty);
			}
		}

		async void OnNavigated(NavigatedEventArgs e)
		{
			if (Navigated != null)
			{
				Navigated(this, e);
				
			}
			//this.FolderSizes.Clear();
			//LPCSHCOLUMNINIT lpi = new LPCSHCOLUMNINIT();
			//lpi.wszFolder = e.Folder.ParsingName + "\0";
			//if (ICP != null)
			//{
			//	await Task.Run(() =>
			//	{
			//		this.BeginInvoke(new MethodInvoker(() =>
			//		{
			//			this.FolderSizes.Clear();
			//			ICP.Initialize(lpi);
			//			foreach (var item in this.CurrentFolder)
			//			{
			//				var pn = item.ParsingName;
			//				LPCSHCOLUMNID lid = new LPCSHCOLUMNID();
			//				lid.fmtid = Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748");
			//				lid.pid = 0;
			//				LPCSHCOLUMNDATA ldata = new LPCSHCOLUMNDATA();
			//				ldata.dwFileAttributes = item.IsFolder ? (uint)FileAttributes.Directory : 0;
			//				//ldata.dwFileAttributes = (uint)FileAttributes.Directory;
			//				if (!item.IsFolder)
			//				{
			//					ldata.pwszExt = Path.GetExtension(item.ParsingName);
			//				}
			//				ldata.wszFile = pn + "\0";
			//				object o = 0;
			//				this.ICP.GetItemData(lid, ldata, out o);
			//				if (o != null)
			//				{
			//					if (this.FolderSizes.ContainsKey(pn))
			//						this.FolderSizes[pn] = o.ToString();
			//					else
			//						this.FolderSizes.TryAdd(pn, o.ToString());
			//				}

			//				Thread.Sleep(1);
			//				Application.DoEvents();

			//			}
			//		}));
			//	});
			//}

		}

		void OnViewChanged(ViewChangedEventArgs e)
		{
			if (ViewStyleChanged != null)
			{
				ViewStyleChanged(this, e);
			}
		}

		public void ResizeIcons(int value)
		{
			_iconSize = value;
			this.Cancel = true;
			waitingThumbnails.Clear();
			this.cache.Clear();
			System.Windows.Forms.ImageList il = new System.Windows.Forms.ImageList();
			il.ImageSize = new System.Drawing.Size(value, value);
			System.Windows.Forms.ImageList ils = new System.Windows.Forms.ImageList();
			ils.ImageSize = new System.Drawing.Size(16, 16);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
			this.Cancel = false;

		}

		private byte[] ImageToByte(Bitmap image, ImageFormat format)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				// Convert Image to byte[]
				image.Save(ms, format);
				var imageBytes = ms.ToArray();
				//bmp.Dispose();
				return imageBytes;
			}
		}

		private Bitmap ByteToImage(byte[] imageBytes)
		{
			// Convert byte[] to Image
			MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
			ms.Write(imageBytes, 0, imageBytes.Length);
			Bitmap image = new Bitmap(ms);
			return image;
		}

		public int GetItemsCount()
		{
			return this.Items.Length;
		}

		public void DeSelectAllItems()
		{
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = 0;
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
		}

        //private async void LoadImage(int id, int iconSize, int index)
        //{
        //    if (!cache.ContainsKey(id))
        //    {
        //        string query = "SELECT thumbnail FROM Thumbnails WHERE id=@0;";
        //        SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();
        //        if (iconSize == 16)
        //            connBuilder.DataSource = "cache16.s3db";
        //        else if (iconSize > 16 && iconSize <= 32)
        //            connBuilder.DataSource = "cache32.s3db";
        //        else if (iconSize > 32 && iconSize <= 48)
        //            connBuilder.DataSource = "cache48.s3db";
        //        else if (iconSize > 48 && iconSize <= 96)
        //            connBuilder.DataSource = "cache96.s3db";
        //        else if (iconSize > 96 && iconSize <= 256)
        //            connBuilder.DataSource = "cache256.s3db";
        //        connBuilder.Version = 3;
        //        //Set page size to NTFS cluster size = 4096 bytes
        //        connBuilder.PageSize = 4096;
        //        connBuilder.CacheSize = 10000;
        //        connBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
        //        connBuilder.Pooling = true;
        //        connBuilder.LegacyFormat = false;
        //        connBuilder.DefaultTimeout = 500;
        //        connBuilder.SyncMode = SynchronizationModes.Normal;
        //        SQLiteConnection con = new SQLiteConnection(connBuilder.ToString());
        //        SQLiteCommand cmd = new SQLiteCommand(query, con);
        //        SQLiteParameter param = new SQLiteParameter("@0", System.Data.DbType.Int64);
        //        param.Value = id;
        //        cmd.Parameters.Add(param);
        //        if (con.State != ConnectionState.Open)
        //            con.Open();
        //        Bitmap b = null;
        //        try
        //        {
        //            IDataReader rdr = await cmd.ExecuteReaderAsync();
        //            try
        //            {
        //                while (rdr.Read())
        //                {
        //                    byte[] a = (System.Byte[])rdr[0];
        //                    b = ByteToImage(a);

        //                }
        //            }
        //            catch (Exception exc) { MessageBox.Show(exc.Message); }
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message); }

        //        con.Close();
        //        if (!cache.ContainsKey(id))
        //            cache.Add(id, b);
        //        User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
        //    }
        //}
		public void SetSortCollumn(int colIndex, SortOrder Order)
		{
			if (colIndex == this.LastSortedColumnIndex)
			{
				// Reverse the current sort direction for this column.
				if (this.LastSortOrder == SortOrder.Ascending)
				{
					this.LastSortOrder = SortOrder.Descending;
				}
				else
				{
					this.LastSortOrder = SortOrder.Ascending;
				}
			}
			else
			{
				// Set the column number that is to be sorted; default to ascending.
				this.LastSortedColumnIndex = colIndex;
				this.LastSortOrder = Order;
			}

			if (Order == SortOrder.Ascending)
			{
				this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenBy(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToArray();
			}
			else
			{
				this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenByDescending(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToArray();
			}
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Length, 0);
			this.SetSortIcon(colIndex, Order);
		}

		public void RefreshContents()
		{
			Navigate(this.CurrentFolder);
		}
		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the previous folder 
		/// in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a <b>Back</b> button similar to the one in 
		/// Windows Explorer, which will allow your users to return to a 
		/// previous folder in the navigation history. 
		/// </para>
		/// 
		/// <para>
		/// Use the <see cref="CanNavigateBack"/> property to determine whether 
		/// the navigation history is available and contains a previous page. 
		/// This property is useful, for example, to change the enabled state 
		/// of a Back button when the ShellView control navigates to or leaves 
		/// the beginning of the navigation history.
		/// </para>
		/// </remarks>
		/// 
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate backwards through.
		/// </exception>
		public void NavigateBack()
		{
			m_CurrentFolder = m_History.MoveBack();
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control backwards to the 
		/// requested folder in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a drop-down menu on a <b>Back</b> button similar 
		/// to the one in Windows Explorer, which will allow your users to return 
		/// to a previous folder in the navigation history. 
		/// </remarks>
		/// 
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		/// 
		/// <exception cref="Exception">
		/// The requested folder is not present in the 
		/// <see cref="ShellView"/>'s 'back' history.
		/// </exception>
		public void NavigateBack(ShellItem folder)
		{
			m_History.MoveBack(folder);
			m_CurrentFolder = folder;
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the next folder 
		/// in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateForward"/> 
		/// method to implement a <b>Forward</b> button similar to the one 
		/// in Windows Explorer, allowing your users to return to the next 
		/// folder in the navigation history after navigating backward.
		/// </para>
		/// 
		/// <para>
		/// Use the <see cref="CanNavigateForward"/> property to determine 
		/// whether the navigation history is available and contains a folder 
		/// located after the current one.  This property is useful, for 
		/// example, to change the enabled state of a <b>Forward</b> button 
		/// when the ShellView control navigates to or leaves the end of the 
		/// navigation history.
		/// </para>
		/// </remarks>
		/// 
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate forwards through.
		/// </exception>
		public void NavigateForward()
		{
			m_CurrentFolder = m_History.MoveForward();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control forwards to the 
		/// requested folder in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the 
		/// <see cref="NavigateForward"/> method to implement a drop-down menu 
		/// on a <b>Forward</b> button similar to the one in Windows Explorer, 
		/// which will allow your users to return to a folder in the 'forward'
		/// navigation history. 
		/// </remarks>
		/// 
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		/// 
		/// <exception cref="Exception">
		/// The requested folder is not present in the 
		/// <see cref="ShellView"/>'s 'forward' history.
		/// </exception>
		public void NavigateForward(ShellItem folder)
		{
			m_History.MoveForward(folder);
			m_CurrentFolder = folder;

			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates to the parent of the currently displayed folder.
		/// </summary>
		public void NavigateParent()
		{
			Navigate(m_CurrentFolder.Parent);
		}

		public List<Collumns> AllAvailableColumns = new List<Collumns>();

        //private void RefreshThumbnail()
        //{
        //    while (true)
        //    {
        //        while (refreshedImages.Count == 0)
        //            Thread.Sleep(1);
        //        var iconSize = 256;
        //        SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();
        //        if (this.IconSize == 16)
        //        {
        //            connBuilder.DataSource = "cache16.s3db";
        //            iconSize = 16;
        //        }
        //        else if (IconSize > 16 && IconSize <= 32)
        //        {
        //            connBuilder.DataSource = "cache32.s3db";
        //            iconSize = 32;
        //        }
        //        else if (IconSize > 32 && IconSize <= 48)
        //        {
        //            connBuilder.DataSource = "cache48.s3db";
        //            iconSize = 48;
        //        }
        //        else if (IconSize > 48 && IconSize <= 96)
        //        {
        //            connBuilder.DataSource = "cache96.s3db";
        //            iconSize = 96;
        //        }
        //        else if (IconSize > 96 && IconSize <= 256)
        //        {
        //            connBuilder.DataSource = "cache256.s3db";
        //            iconSize = 256;
        //        }

        //        connBuilder.Version = 3;
        //        //Set page size to NTFS cluster size = 4096 bytes
        //        connBuilder.PageSize = 4096;
        //        connBuilder.CacheSize = 10000;
        //        connBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
        //        connBuilder.Pooling = true;
        //        connBuilder.LegacyFormat = false;
        //        connBuilder.DefaultTimeout = 500;

        //        using (SQLiteConnection con1 = new SQLiteConnection(connBuilder.ToString()))
        //        {
        //            var refreshedImagesArray = refreshedImages.ToArray();
        //            con1.Open();
        //            //SQLiteTransaction transaction = con1.BeginTransaction();
        //            SQLiteCommand cmd = con1.CreateCommand();
        //            //cmd.Transaction = transaction;
        //            cmd.CommandText = "INSERT INTO Thumbnails (id, thumbnail) VALUES (@0, @1);";
        //            foreach (var itemf in refreshedImagesArray)
        //            {
        //                if (Cancel)
        //                {
        //                    Cancel = false;
        //                    break;
        //                }
        //                if (User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_ISITEMVISIBLE, itemf, 0) == 0)
        //                {
        //                    refreshedImages.Remove(itemf);
        //                    continue;
        //                }
        //                if (itemf < Items.Length)
        //                {
        //                    Bitmap image = null;
        //                    ShellItem item = null;
        //                    int hash = -1;
        //                    //this.Invoke(new MethodInvoker(() =>
        //                    //{
        //                        try
        //                        {
        //                            item = Items[itemf];
        //                            hash = item.GetHashCode();
        //                        }
        //                        catch { }

        //                    //}));

        //                    if (item != null && image == null)
        //                    {
        //                        image = new Bitmap(item.GetShellThumbnail(iconSize, iconSize < 32 ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.Default, ShellThumbnailRetrievalOption.Default));

        //                        if (image != null)
        //                        {

        //                            try
        //                            {
        //                                var buffer = image;
        //                                byte[] imageToByte;
        //                                lock (buffer)
        //                                {
        //                                    imageToByte = ImageToByte(buffer, ImageFormat.Png);


        //                                    if (!cache.ContainsKey(hash))
        //                                        cache.Add(hash, new Bitmap(buffer));
        //                                }
        //                                buffer.Dispose();
        //                                if (imageToByte != null)
        //                                {
        //                                    SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Int64);
        //                                    param0.Value = hash;
        //                                    SQLiteParameter param = new SQLiteParameter("@1", System.Data.DbType.Binary);
        //                                    param.Value = imageToByte;
        //                                    cmd.Parameters.Add(param0);
        //                                    cmd.Parameters.Add(param);

        //                                    cmd.ExecuteNonQueryAsync();
        //                                    imageToByte = null;
        //                                }
        //                                Thread.Sleep(1);
        //                                Application.DoEvents();

        //                                User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, itemf, itemf);

										

        //                            }
        //                            catch (Exception exc1)
        //                            {
        //                                User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, itemf, itemf);
        //                                //transaction.Rollback();
        //                                //MessageBox.Show(exc1.Message);
        //                            }

        //                            refreshedImages.Remove(itemf);
        //                        }
        //                    }
        //                    else
        //                    {

        //                    }
        //                }
        //                refreshedImages.Remove(itemf);
        //            }
        //            //transaction.Commit();

        //            con1.Close();
        //        }
        //    }
        //}
		public List<ShellItem> SelectedItems
		{
			get
			{
				List<ShellItem> selItems = new List<ShellItem>();
				int index = -2;
				int iStart = -1;

				while (index != -1)
				{
					index = User32.SendMessage(this.LVHandle, LVM.GETNEXTITEM, iStart, LVNI.LVNI_SELECTED);
					iStart = index;
					if (index != -1)
					{
						selItems.Add(this.Items[index]);
					}
				}

				return selItems;

			}
		}

		public int GetSelectedCount()
		{
			return (int)User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETSELECTEDCOUNT, 0, 0);
		}
        ImageList jumbo = new ImageList(ImageListSize.Jumbo);
        ImageList extra = new ImageList(ImageListSize.ExtraLarge);
				ImageList small = new ImageList(ImageListSize.SystemSmall);
				ImageList large = new ImageList(ImageListSize.Large);
		//public async void LoadIcon()
		//{
		//	while (true)
		//	{

		//		Thread.Sleep(1);
		//		while (waitingThumbnails.Count == 0)
		//			Thread.Sleep(1);

		//		try
		//		{
		//			//while (waitingThumbnails.Count > 0)
		//			{


		//				var index = waitingThumbnails.Dequeue();
		//				//if (User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_ISITEMVISIBLE, index, 0) == IntPtr.Zero || this.Cancel)
		//				//	continue;

		//				ShellItem sho = null;
		//				int hash = -1;
		//				Bitmap bitmap = null;
		//				var shoTemp = Items[index];
		//				if (shoTemp.ParsingName.StartsWith("::"))
		//				{
		//					sho = shoTemp;
		//				}
		//				else
		//				{
		//					sho = new ShellItem(shoTemp.ParsingName);
		//				}
		//				var pidl = sho.Pidl;
		//				hash = shoTemp.GetHashCode();

		//				if (hash != -1)
		//				{
		//					bitmap = sho.GetShellThumbnail(IconSize, (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details) ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.ThumbnailOnly);
		//					if (bitmap == null)
		//					{
		//						IExtractIconpwFlags fl = 0;
		//						fl = sho.GetIconType();
		//						if ((fl & IExtractIconpwFlags.GIL_PERCLASS) == 0)
		//						{
		//							if (!sho.IsFolder)
		//							{
		//								bitmap = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
		//								if (!cache.ContainsKey(hash))
		//								{
		//									cache.Add(hash, new Bitmap(bitmap));

		//									User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
		//								}
		//							}
		//							else
		//							{
		//								if (!cache.ContainsKey(hash))
		//								{
		//									cache.Add(hash, null);
		//									User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
		//								}
		//							}
		//						}
		//						else
		//						{
		//							if (!cache.ContainsKey(hash))
		//							{
		//								cache.Add(hash, null);
		//								User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
		//							}
		//						}

		//					}
		//					else
		//					{
		//						if (!cache.ContainsKey(hash))
		//						{
		//							cache.Add(hash, null);
		//							User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
		//						}
		//					}
		//					if (bitmap != null)
		//					{
		//						bitmap.Dispose();
		//					}
		//					//Thread.Sleep(1);
		//					//Application.DoEvents();
		//					//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);

		//				}
		//			}
		//		}
		//		catch (Exception)
		//		{
		//			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
		//		}
		//	}
		//}
				Dictionary<int, int> overlays = new Dictionary<int, int>();
				public void LoadOverlay(int index)
				{
					if (this.Cancel)
						return;
					ShellItem sho = null;
					var shoTemp = Items[index];
					if (shoTemp.ParsingName.StartsWith("::"))
					{
						sho = shoTemp;
					}
					else
					{
						sho = new ShellItem(shoTemp.ParsingName);
					}
					int hash = shoTemp.GetHashCode();

					int overlayIndex = 0;
					small.GetIconIndexWithOverlay(sho.Pidl, out overlayIndex);
					if (!overlays.ContainsKey(hash))
					{
						overlays.Add(hash, overlayIndex);
					}
					if (overlayIndex > 0)
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
				}

				Dictionary<int, int> shieldedIcons = new Dictionary<int, int>();
				public void LoadShield(int index)
				{
					if (this.Cancel)
						return;
					ShellItem sho = null;
					var shoTemp = Items[index];
					if (shoTemp.ParsingName.StartsWith("::"))
					{
						sho = shoTemp;
					}
					else
					{
						sho = new ShellItem(shoTemp.ParsingName);
					}
					int hash = shoTemp.GetHashCode();

					var shieldOverlay = 0;
					if ((sho.GetShield() & IExtractIconpwFlags.GIL_SHIELD) != 0)
					{
						Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
						defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));
						Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_SHIELD, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
						shieldOverlay = defIconInfo.iSysIconIndex;
					}
					if (shieldOverlay > 0)
					{
						if (!shieldedIcons.ContainsKey(hash))
							shieldedIcons.Add(hash, shieldOverlay);

						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
					}
				}
				public async void LoadIcon(int index)
				{

					try
					{
						if (this.Cancel)
							return;

						ShellItem sho = null;
						int hash = -1;
						Bitmap bitmap = null;
						var shoTemp = Items[index];
						if (shoTemp.ParsingName.StartsWith("::"))
						{
							sho = shoTemp;
						}
						else
						{
							sho = new ShellItem(shoTemp.ParsingName);
						}

						hash = shoTemp.GetHashCode();

						if (hash != -1)
						{

							if ((sho.GetIconType() & IExtractIconpwFlags.GIL_PERINSTANCE) != 0)
							{
								if ((sho.GetIconType() & IExtractIconpwFlags.GIL_PERCLASS) == 0)
								{
									bitmap = sho.GetShellThumbnail(IconSize, (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details) ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.IconOnly);
									if (!cache.ContainsKey(hash))
										cache.Add(hash, new Bitmap(bitmap));
									if (!this.Cancel)
										User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
								}
							}
							
							if (sho.IsFolder || sho.Extension == ".jpg" || sho.Extension == ".png")
							//Task.Run(() =>
							{
								bitmap = sho.GetShellThumbnail(IconSize, (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details) ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.ThumbnailOnly);
								if (bitmap != null && !this.Cancel)
								{

									User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
									bitmap.Dispose();
								}
							}//);
							//if ((sho.GetIconType() & IExtractIconpwFlags.GIL_PERCLASS) != 0 && (sho.GetIconType() & IExtractIconpwFlags.GIL_PERINSTANCE) == 0)
							//{
							//	bitmap = sho.GetShellThumbnail(IconSize, (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details) ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.Default);
							//	if (bitmap != null)
							//	{
							//		if (!cache.ContainsKey(hash))
							//			cache.Add(hash, new Bitmap(bitmap));
							//		if (!this.Cancel)
							//			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
							//	}
							//}
							if (bitmap != null)
							{
								bitmap.Dispose();
							}
							//Thread.Sleep(1);
							//Application.DoEvents();
						}
					}
					catch (Exception)
					{
						if (!this.Cancel)
							User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
					}
				}

		public void RenameSelectedItem()
		{
			//User32.EnumChildWindows(this.LVHandle, RenameCallback, IntPtr.Zero);
			RenameCallback(this.LVHandle, IntPtr.Zero);
		}

		public void InvertSelection()
		{
			int itemCount = (int)User32.SendMessage(this.LVHandle,
								BExplorer.Shell.Interop.MSG.LVM_GETITEMCOUNT, 0, 0);

			for (int n = 0; n < itemCount; ++n)
			{

				var state = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE,
						n, LVIS.LVIS_SELECTED);

				LVITEM item_new = new LVITEM();
				item_new.mask = LVIF.LVIF_STATE;
				item_new.stateMask = LVIS.LVIS_SELECTED;
				item_new.state = (state & LVIS.LVIS_SELECTED) == LVIS.LVIS_SELECTED ? 0 : LVIS.LVIS_SELECTED;
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, n, ref item_new);
			}
		}

		bool RenameCallback(IntPtr hwnd, IntPtr lParam)
		{
			var index = User32.SendMessage(this.LVHandle, LVM.GETNEXTITEM, -1, LVNI.LVNI_SELECTED);
			var res = User32.SendMessage(hwnd, BExplorer.Shell.Interop.MSG.LVM_EDITLABELW, index, 0);
			return false;

			//return true;
		}
		private static BitmapFrame CreateResizedImage(IntPtr hBitmap, int width, int height, int margin)
		{
			var source = Imaging.CreateBitmapSourceFromHBitmap(
						hBitmap,
						IntPtr.Zero,
						System.Windows.Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions()).Clone();
			Gdi32.DeleteObject(hBitmap);



				var group = new DrawingGroup();
				RenderOptions.SetBitmapScalingMode(
						group, BitmapScalingMode.Fant);
				group.Children.Add(
						new ImageDrawing(source,
								new Rect(0, 0, width, height)));
				var targetVisual = new DrawingVisual();
				var targetContext = targetVisual.RenderOpen();
				targetContext.DrawDrawing(group);
				var target = new RenderTargetBitmap(
						width, height, 96, 96, PixelFormats.Default);
				targetContext.Close();
				target.Render(targetVisual);
				return BitmapFrame.Create(target);
		}

		private Bitmap BitmapImage2Bitmap(BitmapFrame bitmapImage)
		{
			byte[] targetBytes = null;
			using (var memoryStream = new MemoryStream())
			{
				var targetEncoder = new PngBitmapEncoder();
				targetEncoder.Frames.Add(bitmapImage);
				targetEncoder.Save(memoryStream);
				return new Bitmap(memoryStream);
			}
			
		}

		Boolean Cancel = false;
		Dictionary<int, Bitmap> cache = new Dictionary<int, Bitmap>();
		List<int> refreshedImages = new List<int>();
		Queue<int> waitingThumbnails = new Queue<int>();
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		System.Windows.Forms.Timer selectionTimer = new System.Windows.Forms.Timer();
        Bitmap icon = null;
		protected override void WndProc(ref Message m)
		{
			bool isSmallIcons = (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details);
			base.WndProc(ref m);
			if (m.Msg == 78)
			{
				NMHDR nmhdr = new NMHDR();
				nmhdr = (NMHDR)m.GetLParam(nmhdr.GetType());
				switch ((int)nmhdr.code)
				{
					case WNM.LVN_GETDISPINFOW:
						var nmlv = new NMLVDISPINFO();
						nmlv = (NMLVDISPINFO)m.GetLParam(nmlv.GetType());
						//if ((nmlv.item.mask & LVIF.LVIF_COLUMNS) == LVIF.LVIF_COLUMNS)
						//{
						//	int[] varArray = {0,1,2,3};
						//	IntPtr ptr = Marshal.AllocHGlobal(varArray.Length*Marshal.SizeOf(varArray[0]));
						//	Marshal.Copy(varArray,0,ptr, varArray.Length);
						//	nmlv.item.cColumns = varArray.Length;
						//	nmlv.item.puColumns = (uint)ptr;
						//	Marshal.StructureToPtr(nmlv, m.LParam, false);
						//}
						if ((nmlv.item.mask & LVIF.LVIF_TEXT) == LVIF.LVIF_TEXT)
						{
							if (nmlv.item.iSubItem == 0){
								var currentItem = Items[nmlv.item.iItem];
								nmlv.item.pszText =  this.View == ShellViewStyle.Tile ? String.Empty : currentItem.DisplayName;
								Marshal.StructureToPtr(nmlv, m.LParam, false);
							} else {
								if (isSmallIcons)
								{
									var currentItem = Items[nmlv.item.iItem];
									IShellItem2 isi2 = (IShellItem2)currentItem.m_ComInterface;
									Collumns currentCollumn = this.Collumns[nmlv.item.iSubItem];
									PROPERTYKEY pk = currentCollumn.pkey;
									PropVariant pvar = new PropVariant();
									if (isi2.GetProperty(ref pk, pvar) == HResult.S_OK)
									{
										String value = String.Empty;
										if (pvar.Value != null)
										{
											if (currentCollumn.CollumnType == typeof(DateTime))
											{

												value = ((DateTime)pvar.Value).ToString(Thread.CurrentThread.CurrentCulture);
											}
											else if (currentCollumn.CollumnType == typeof(long))
											{
												value = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(pvar.Value.ToString()) / 1024).ToString("# ### ### ##0"))); //ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
											}
											else
											{
												value = pvar.Value.ToString();
											}
										}
										nmlv.item.pszText = value;
										Marshal.StructureToPtr(nmlv, m.LParam, false);
									}
									pvar.Dispose();
								}
							}
						}

						break;
					case WNM.LVN_COLUMNCLICK:
						NMLISTVIEW nlcv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
						SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
						break;
					case WNM.LVN_ITEMACTIVATE:
						var iac = new NMITEMACTIVATE();
						iac = (NMITEMACTIVATE)m.GetLParam(iac.GetType());
            ShellItem selectedItem = Items[iac.iItem];
						if (selectedItem.IsFolder)
							Navigate(selectedItem);
						else
							Process.Start(selectedItem.ParsingName);
						break;
					case WNM.LVN_BEGINSCROLL:
						this.Cancel = true;
						waitingThumbnails.Clear();
						cache.Clear();
						break;
					case WNM.LVN_ENDSCROLL:
						Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
						this.Cancel = false;
						break;
					case WNM.LVN_ITEMCHANGED:
						NMLISTVIEW nlv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
						if ((nlv.uChanged & LVIF.LVIF_STATE) == LVIF.LVIF_STATE)
						{
							selectionTimer.Interval = 100;
							selectionTimer.Tick += selectionTimer_Tick;
							if (selectionTimer.Enabled)
							{
								selectionTimer.Stop();
								selectionTimer.Start();
							}
							else
							{
								selectionTimer.Start();
							}
						}

						break;
					case WNM.LVN_ODSTATECHANGED:
						OnSelectionChanged();
						break;
					case WNM.LVN_KEYDOWN:
						NMLVKEYDOWN nkd = (NMLVKEYDOWN)m.GetLParam(typeof(NMLVKEYDOWN));
						switch (nkd.wVKey)
						{
							case (short)Keys.F2:
								RenameSelectedItem();
								break;
							case (short)Keys.Enter:
								if (SelectedItems[0].IsFolder)
									Navigate(SelectedItems[0]);
								else
									Process.Start(SelectedItems[0].ParsingName);
								break;
							default:
								break;
						}
						break;
					case WNM.NM_RCLICK:
						var selitems = this.SelectedItems;
						NMITEMACTIVATE itemActivate = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
						ShellContextMenu cm = new ShellContextMenu(selitems.ToArray());
						cm.ShowContextMenu(this, itemActivate.ptAction, CMF.CANRENAME);
						break;
					case WNM.NM_SETFOCUS:
						OnGotFocus();
						break;
					case WNM.NM_KILLFOCUS:
						OnLostFocus();
						break;
					case CustomDraw.NM_CUSTOMDRAW:
						{
							if (nmhdr.hwndFrom == this.LVHandle)
							{
								User32.SendMessage(this.LVHandle, 296, User32.MAKELONG(1, 1), 0);
								var nmlvcd = new NMLVCUSTOMDRAW();
								nmlvcd = (NMLVCUSTOMDRAW)m.GetLParam(nmlvcd.GetType());
								var index = (int)nmlvcd.nmcd.dwItemSpec;
								var hdc = nmlvcd.nmcd.hdc;
								switch (nmlvcd.nmcd.dwDrawStage)
								{
									case CustomDraw.CDDS_PREPAINT:
										m.Result = (IntPtr)CustomDraw.CDRF_NOTIFYITEMDRAW;
										break;
									case CustomDraw.CDDS_ITEMPREPAINT:
										m.Result = (IntPtr)CustomDraw.CDRF_NOTIFYPOSTPAINT;
										break;
									case CustomDraw.CDDS_ITEMPOSTPAINT:
										if (nmlvcd.clrFace != 0 && nmlvcd.clrTextBk != 0 && nmlvcd.clrText != 0 )
										{
											var itemBounds = new User32.RECT();
											User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMRECT, index, ref itemBounds);

											var iconBounds = new User32.RECT();

											iconBounds.Left = 1;

											User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMRECT, index, ref iconBounds);
											ShellItem sho = null;
											var hash = -1;

											sho = Items[index];


                      
										try
										{
											hash = sho.GetHashCode();


											if (sho != null)
											{
												Icon real_icon = null;
												
												int shieldOverlay = 0;
												IExtractIconpwFlags iconType = sho.GetIconType();
												var thumbnail = sho.GetShellThumbnail(IconSize, isSmallIcons ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.CacheOnly);
												if (thumbnail == null)
												{
													if ((iconType & IExtractIconpwFlags.GIL_PERINSTANCE) != 0)
													{
														if ((iconType & IExtractIconpwFlags.GIL_PERCLASS) == 0)
														{
															int iconindex = 0;
															if (Path.GetExtension(sho.ParsingName) == ".exe" || Path.GetExtension(sho.ParsingName) == ".com" || Path.GetExtension(sho.ParsingName) == ".bat")
															{
																Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
																defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));
																Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_APPLICATION, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
																iconindex = defIconInfo.iSysIconIndex;
															}
															else if (sho.IsFolder)
															{
																Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
																defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));
																Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_FOLDER, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
																iconindex = defIconInfo.iSysIconIndex;
															}
															real_icon = isSmallIcons ? small.GetIcon(iconindex) : IconSize <= 48 ? extra.GetIcon(iconindex) : jumbo.GetIcon(iconindex);
															if (real_icon != null)
															{
																icon = real_icon.ToBitmap();
																//real_icon.Dispose();
																User32.DestroyIcon(real_icon.Handle);

															}
														}
													}
													if ((iconType & IExtractIconpwFlags.GIL_PERCLASS) != 0 && (iconType & IExtractIconpwFlags.GIL_PERINSTANCE) == 0)
													{
														icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly);

													}

													if (!shieldedIcons.TryGetValue(hash, out shieldOverlay))
													{
														Task.Run(() =>
														{
															LoadShield(index);
														});
													}
													Bitmap tempBitmap = null;
													if (!cache.TryGetValue(hash, out tempBitmap))
													{
														//waitingThumbnails.Enqueue(index);
														//if (icon != null)
														//{
														//	using (Graphics g = Graphics.FromHdc(hdc))
														//	{
														//		if (icon.Width > IconSize)
														//		{
														//			g.DrawImage(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
														//		}
														//		else
														//		{
														//			g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
														//		}

														//	}
														//}
														Task.Run(() =>
														{
															LoadIcon(index);
														});

													}
													else
													{
														icon = tempBitmap;
													}
												}
												else
												{
													icon = thumbnail;
												}
												//Bitmap tempicon2;
												

                          if (icon != null)
                          {
                              using (Graphics g = Graphics.FromHdc(hdc))
                              {
																if (icon.Width != IconSize)
																{
																	Bitmap tempBitmap = null;
																	if (!cache.TryGetValue(hash, out tempBitmap))
																	{
																		//waitingThumbnails.Enqueue(index);
																		//if (icon != null)
																		//{
																		//	using (Graphics g = Graphics.FromHdc(hdc))
																		//	{
																		//		if (icon.Width > IconSize)
																		//		{
																		//			g.DrawImage(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		//		}
																		//		else
																		//		{
																		//			g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
																		//		}

																		//	}
																		//}
																		Task.Run(() =>
																		{
																			LoadIcon(index);
																		});

																	}
																	else
																	{
																		icon = tempBitmap;
																	}
																	g.DrawImage(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
																	//var factory = (IWICComponentFactory)new WICImagingFactory();
																	//var bmp = factory.CreateBitmapFromHBITMAP(icon.GetHbitmap(), IntPtr.Zero, WICBitmapAlphaChannelOption.WICBitmapUsePremultipliedAlpha);
																	//var scaler = factory.CreateBitmapScaler();
																	//scaler.Initialize(bmp, (uint)IconSize, (uint)IconSize, WICBitmapInterpolationMode.WICBitmapInterpolationModeFant);
																	//scaler.
																	//var hicon = jumbo.GetHIcon(3);
																	//User32.DestroyIcon(hicon);
																	//var factory = (IWICComponentFactory)new WICImagingFactory();
																	//var bmp = factory.CreateBitmapFromHICON(hicon);
																	//var scaler = factory.CreateBitmapScaler();
																	//scaler.Initialize(bmp, (uint)IconSize, (uint)IconSize, WICBitmapInterpolationMode.WICBitmapInterpolationModeFant);
																	//var image = CreateResizedImage(hicon,IconSize,IconSize,0);
																	//var mp = BitmapImage2Bitmap(image);
																	//g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
																}
																else
																{
																	//var hicon = jumbo.GetHIcon(3);
																	//Bitmap bb = new Bitmap(256,256);
																	//Graphics gg = Graphics.FromImage(bb);
																	////User32.DestroyIcon(hicon);
																	//jumbo.DrawIcon(gg.GetHdc(),3,new System.Drawing.Point(0,0));
																	//gg.Dispose();
																	////var factory = (IWICComponentFactory)new WICImagingFactory();
																	////var bmp = factory.CreateBitmapFromHICON(hicon);
																	////var scaler = factory.CreateBitmapScaler();
																	////scaler.Initialize(bmp, (uint)IconSize, (uint)IconSize, WICBitmapInterpolationMode.WICBitmapInterpolationModeFant);
																	//var image = CreateResizedImage(bb.GetHbitmap(), IconSize, IconSize, 0);
																	//var mp = BitmapImage2Bitmap(image);
																	g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
																}

                              }
														int overlayIndex = 0;
														if (overlays.TryGetValue(hash, out overlayIndex))
														{
															if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon)
															{
																if (overlayIndex > 0)
																	small.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - 16));
															}
															else
															{
																if (this.IconSize > 180)
																{
																	if (overlayIndex > 0)
																		jumbo.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
																}
																else
																	if (this.IconSize > 64)
																	{
																		if (overlayIndex > 0)
																			extra.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
																	}
																	else
																	{
																		if (overlayIndex > 0)
																			large.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
																	}
															}
														}
														else
														{
															Task.Run(() =>
															{
																LoadOverlay(index);
															});
														}

														if (sho.IsShared)
														{
															if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon)
																small.DrawOverlay(hdc, 1, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - 16));
															else
															{
																if (this.IconSize > 180)
																	jumbo.DrawOverlay(hdc, 1, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
																else
																	if (this.IconSize > 64)
																		extra.DrawOverlay(hdc, 1, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
																	else
																		large.DrawOverlay(hdc, 1, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
															}
														}

														if (shieldOverlay != 0)
														{
															if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon)
																small.DrawIcon(hdc, shieldOverlay, new System.Drawing.Point(iconBounds.Right - 16, iconBounds.Bottom - 16));
															else
															{
																if (this.IconSize > 180)
																	jumbo.DrawIcon(hdc, shieldOverlay, new System.Drawing.Point(iconBounds.Right - this.IconSize / 3, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
																else
																	if (this.IconSize > 64)
																		extra.DrawIcon(hdc, shieldOverlay, new System.Drawing.Point(iconBounds.Right - 50, iconBounds.Bottom - 50));
																	else
																		large.DrawIcon(hdc, shieldOverlay, new System.Drawing.Point(iconBounds.Right - 32, iconBounds.Bottom - 32));
															}
														}
                          }

												}
											}
											catch (Exception ex)
											{
												//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
												//throw;
											}
										}
										m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
										break;
								}
							}
						}
						break;
				}
			}

		}

		void selectionTimer_Tick(object sender, EventArgs e)
		{
			OnSelectionChanged();
			(sender as System.Windows.Forms.Timer).Stop();
		}

		bool IsDoubleNavFinished = false;
		BackgroundWorker bw = new BackgroundWorker();
		public async void Navigate(ShellItem destination)
		{
			Application.DoEvents();
			bw.DoWork += bw_DoWork;
			bw.RunWorkerCompleted += bw_RunWorkerCompleted;
			overlays.Clear();
			shieldedIcons.Clear();
			Items = new ShellItem[0];
			waitingThumbnails.Clear();
			this.Cancel = true;
			this.cache.Clear();

			Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, 0, 0);
			
			//Task.Run(() =>
			//{
			//	var array = destination.ToArray();
			//	IsDoubleNavFinished = true;
			//});
			//if (!bw.IsBusy)
			//{
			//	bw.RunWorkerAsync(destination);
			//}

			var pc = new ProgressContext<ShellItem>(destination);
			pc.UpdateProgress += pc_UpdateProgress;
			this.Items = pc.OrderByDescending(o => o.IsFolder).ToArray();
			//this.Items = destination.OrderByDescending(o => o.IsFolder).ToArray();

			this.Cancel = false;
			this.LastSortedColumnIndex = 0;
			this.LastSortOrder = SortOrder.Ascending;
			this.SetSortIcon(this.LastSortedColumnIndex, this.LastSortOrder);
			this.m_CurrentFolder = destination;
			try
			{
				m_History.Add(destination);
			}
			catch { }

			this.OnNavigated(new NavigatedEventArgs(destination));
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Length, 0);


			//this.Items = items.Select(s => new ShellItem(s.ToShellParsingName())).OrderByDescending(o => o.IsFolder).ToArray();
			
			IsDoubleNavFinished = false;
			
		}

		void pc_UpdateProgress(object sender, ProgressArgs<ShellItem> e)
		{
			var aaa = e.Item;
		}

		void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var destination = e.Result as ShellItem;
			//this.Items = destination.OrderByDescending(o => o.IsFolder).ToArray();
			////this.Items = destination.OrderByDescending(o => o.IsFolder).ToArray();

			//this.Cancel = false;
			//this.LastSortedColumnIndex = 0;
			//this.LastSortOrder = SortOrder.Ascending;
			//this.SetSortIcon(this.LastSortedColumnIndex, this.LastSortOrder);
			//this.m_CurrentFolder = destination;
			//try
			//{
			//	m_History.Add(destination);
			//}
			//catch { }

			//this.OnNavigated(new NavigatedEventArgs(destination));
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Length, 0);
		}

		void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			var dest = e.Argument as ShellItem;
			var array = dest.OrderByDescending(o => o.IsFolder).ToArray();
			array = null;
			GC.Collect();
			e.Result = dest;
		}

		public int LastSortedColumnIndex { get; set; }
		public SortOrder LastSortOrder { get; set; }

		public void SelectAll()
		{
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = LVIS.LVIS_SELECTED;
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
		}
		ShellHistory m_History;
		ShellItem m_CurrentFolder;
		ShellViewStyle m_View;
		/// <summary>
		/// Gets a value indicating whether a previous page in navigation 
		/// history is available, which allows the <see cref="NavigateBack"/> 
		/// method to succeed. 
		/// </summary>
		[Browsable(false)]
		public bool CanNavigateBack
		{
			get { return m_History.CanNavigateBack; }
		}

		/// <summary>
		/// Gets a value indicating whether a subsequent page in navigation 
		/// history is available, which allows the <see cref="NavigateForward"/> 
		/// method to succeed. 
		/// </summary>
		[Browsable(false)]
		public bool CanNavigateForward
		{
			get { return m_History.CanNavigateForward; }
		}

		/// <summary>
		/// Gets a value indicating whether the folder currently being browsed
		/// by the <see cref="ShellView"/> has parent folder which can be
		/// navigated to by calling <see cref="NavigateParent"/>.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateParent
		{
			get
			{
				return m_CurrentFolder != ShellItem.Desktop;
			}
		}

		/// <summary>
		/// Gets/sets a <see cref="ShellItem"/> describing the folder 
		/// currently being browsed by the <see cref="ShellView"/>.
		/// </summary>
		[Browsable(false)]
		public ShellItem CurrentFolder
		{
			get { return m_CurrentFolder; }
			set
			{
				//if (value != m_CurrentFolder)
				//{
				//	Navigate(value);
				//}
				m_CurrentFolder = value;
			}
		}

		// <summary>
		/// Gets the <see cref="ShellView"/>'s navigation history.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ShellHistory History
		{
			get { return m_History; }
		}

		/// <summary>
		/// Gets or sets how items are displayed in the control. 
		/// </summary>
		[DefaultValue(ShellViewStyle.Medium), Category("Appearance")]
		public ShellViewStyle View
		{
			get { return m_View; }
			set
			{
				m_View = value;
				var iconsize = 16;
				switch (value)
				{
					case ShellViewStyle.ExtraLargeIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(256);
						iconsize = 256;
						break;
					case ShellViewStyle.LargeIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(96);
						iconsize = 96;
						break;
					case ShellViewStyle.Medium:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(48);
						iconsize = 48;
						break;
					case ShellViewStyle.SmallIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_SMALLICON, 0);
						ResizeIcons(16);
						iconsize = 16;
						break;
					case ShellViewStyle.List:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_LIST, 0);
						ResizeIcons(16);
						iconsize = 16;
						break;
					case ShellViewStyle.Details:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_DETAILS, 0);
						ResizeIcons(16);
						iconsize = 16;
						break;
					case ShellViewStyle.Thumbnail:
						break;
					case ShellViewStyle.Tile:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_TILE, 0);
						ResizeIcons(48);
						iconsize = 48;
						//LVTILEVIEWINFO tvi = new LVTILEVIEWINFO();
						//tvi.cbSize = Marshal.SizeOf(typeof(LVTILEVIEWINFO));
						//tvi.dwMask = (int)LVTVIM.LVTVIM_COLUMNS | (int)LVTVIM.LVTVIM_TILESIZE;
						//tvi.dwFlags = (int)LVTVIF.LVTVIF_AUTOSIZE;
						//tvi.cLines = 4;
						//var a = User32.SendMessage(this.LVHandle, (int)BExplorer.Shell.Interop.MSG.LVM_SETTILEVIEWINFO, 0, tvi);
						break;
					case ShellViewStyle.Thumbstrip:
						break;
					case ShellViewStyle.Content:
						break;
					default:
						break;
				}
				OnViewChanged(new ViewChangedEventArgs(value, iconsize));
				//OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
			}
		}
	}

	/// <summary>
	/// Provides information for the <see cref="ShellView.Navigating"/>
	/// event.
	/// </summary>
	public class NavigatingEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NavigatingEventArgs"/>
		/// class.
		/// </summary>
		/// 
		/// <param name="folder">
		/// The folder being navigated to.
		/// </param>
		public NavigatingEventArgs(ShellItem folder)
		{
			m_Folder = folder;
		}

		/// <summary>
		/// Gets/sets a value indicating whether the navigation should be
		/// cancelled.
		/// </summary>
		public bool Cancel
		{
			get { return m_Cancel; }
			set { m_Cancel = value; }
		}

		/// <summary>
		/// The folder being navigated to.
		/// </summary>
		public ShellItem Folder
		{
			get { return m_Folder; }
			set { m_Folder = value; }
		}

		bool m_Cancel;
		ShellItem m_Folder;
	}

	public class NavigatedEventArgs : EventArgs
	{
		ShellItem m_Folder;
		public NavigatedEventArgs(ShellItem folder)
		{
			m_Folder = folder;
		}
		/// <summary>
		/// The folder that is navigated to.
		/// </summary>
		public ShellItem Folder
		{
			get { return m_Folder; }
			set { m_Folder = value; }
		}
	}

	public class ViewChangedEventArgs : EventArgs
	{
		ShellViewStyle m_View;
		Int32 m_ThumbnailSize;
		public ViewChangedEventArgs(ShellViewStyle view, Int32? thumbnailSize)
		{
			m_View = view;
			if (thumbnailSize != null)
				m_ThumbnailSize = thumbnailSize.Value;
		}
		/// <summary>
		/// The current ViewStyle
		/// </summary>
		public ShellViewStyle CurrentView
		{
			get { return m_View; }
			set { m_View = value; }
		}

		public Int32 ThumbnailSize
		{
			get { return m_ThumbnailSize; }
			set { m_ThumbnailSize = value; }
		}
	}

	/// <summary>
	/// Specifies how list items are displayed in a <see cref="ShellView"/> 
	/// control. 
	/// </summary>
	public enum ShellViewStyle
	{
		ExtraLargeIcon,
		LargeIcon,
		/// <summary>
		/// Each item appears as a full-sized icon with a label below it. 
		/// </summary>
		Medium,

		/// <summary>
		/// Each item appears as a small icon with a label to its right. 
		/// </summary>
		SmallIcon,

		/// <summary>
		/// Each item appears as a small icon with a label to its right. 
		/// Items are arranged in columns with no column headers. 
		/// </summary>
		List,

		/// <summary>
		/// Each item appears on a separate line with further information 
		/// about each item arranged in columns. The left-most column 
		/// contains a small icon and label. 
		/// </summary>
		Details,

		/// <summary>
		/// Each item appears with a thumbnail picture of the file's content.
		/// </summary>
		Thumbnail,

		/// <summary>
		/// Each item appears as a full-sized icon with the item label and 
		/// file information to the right of it. 
		/// </summary>
		Tile,

		/// <summary>
		/// Each item appears in a thumbstrip at the bottom of the control,
		/// with a large preview of the seleted item appearing above.
		/// </summary>
		Thumbstrip,
		/// <summary>
		/// Each item appears in a item that occupy the whole view width
		/// </summary>
		Content,


	}
}
