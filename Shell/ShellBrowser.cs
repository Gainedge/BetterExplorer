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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using System.Threading.Tasks;
using System.Threading;

namespace BExplorer.Shell
{
    class ShellBrowser : IShellBrowser,
                         IOleCommandTarget,
                         Interop.IServiceProvider
    {
        public ShellBrowser(ShellView shellView)
        {
            m_ShellView = shellView;
        }

        public StatusBar StatusBar
        {
            get { return m_StatusBar; }
            set
            {
                m_StatusBar = value;
                if (m_StatusBar != null)
                {
                    m_StatusBar.ShowPanels = true;
                }
            }
        }

        #region IShellBrowser Members

        HResult IShellBrowser.GetWindow(out IntPtr phwnd)
        {
            phwnd = m_ShellView.Handle;
            return HResult.S_OK;
        }

        HResult IShellBrowser.ContextSensitiveHelp(bool fEnterMode)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.InsertMenusSB(IntPtr IntPtrShared, IntPtr lpMenuWidths)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.SetMenuSB(IntPtr IntPtrShared, IntPtr holemenuRes, IntPtr IntPtrActiveObject)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.RemoveMenusSB(IntPtr IntPtrShared)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.SetStatusTextSB(IntPtr pszStatusText)
        {
            if (m_StatusBar != null)
            {
                m_StatusBar.Panels.Clear();
                m_StatusBar.Panels.Add(Marshal.PtrToStringUni(pszStatusText));
            }
            return HResult.S_OK;
        }

        HResult IShellBrowser.EnableModelessSB(bool fEnable)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.TranslateAcceleratorSB(IntPtr pmsg, ushort wID)
        {
					//var hr = ((IInputObject)this).TranslateAcceleratorIO(pmsg);
					return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.BrowseObject(IntPtr pidl, SBSP wFlags)
        {
            IntPtr result = IntPtr.Zero;

            if ((wFlags & SBSP.SBSP_RELATIVE) != 0)
            {
                ShellItem child = new ShellItem(m_ShellView.CurrentFolder, pidl);
            }
            else if ((wFlags & SBSP.SBSP_PARENT) != 0)
            {
                m_ShellView.NavigateParent();
            }
            else if ((wFlags & SBSP.SBSP_NAVIGATEBACK) != 0)
            {
                m_ShellView.NavigateBack();
            }
            else if ((wFlags & SBSP.SBSP_NAVIGATEFORWARD) != 0)
            {
                m_ShellView.NavigateForward();
            }
						else if ((wFlags & SBSP.SBSP_SAMEBROWSER) != 0)
						{
							ShellItem child = new ShellItem(pidl);
							m_ShellView.Navigate(child);
							//child.Dispose();
						} else
            {
                m_ShellView.Navigate(new ShellItem(ShellItem.Desktop, Shell32.ILFindLastID(pidl)));
            }
            return HResult.S_OK;
        }

        HResult IShellBrowser.GetViewStateStream(uint grfMode, IntPtr ppStrm)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.GetControlWindow(FCW id, out IntPtr lpIntPtr)
        {
            if ((id == FCW.FCW_STATUS) && (m_StatusBar != null))
            {
                lpIntPtr = m_StatusBar.Handle;
                return HResult.S_OK;
            }
            else
            {
                lpIntPtr = IntPtr.Zero;
                return HResult.E_NOTIMPL;
            }
        }

        HResult IShellBrowser.SendControlMsg(FCW id, MSG uMsg, uint wParam,
                                         uint lParam, IntPtr pret)
        {
            IntPtr result = IntPtr.Zero;

            if ((id == FCW.FCW_STATUS) && (m_StatusBar != null))
            {
                result = User32.SendMessage(m_StatusBar.Handle,
                    uMsg, (int)wParam, (int)lParam);
            }

            if (pret != IntPtr.Zero)
            {
                Marshal.WriteInt32(pret, result.ToInt32());
            }

            return HResult.S_OK;
        }

        HResult IShellBrowser.QueryActiveShellView(out IShellView ppshv)
        {
            ppshv = null;
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.OnViewWindowActive(IShellView ppshv)
        {
            return HResult.E_NOTIMPL;
        }

        HResult IShellBrowser.SetToolbarItems(IntPtr lpButtons, uint nButtons, uint uFlags)
        {
            return HResult.E_NOTIMPL;
        }

        #endregion

        #region IOleCommandTarget Members

        void IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, ref OLECMDTEXT CmdText)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        void IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdExecOpt, ref object pvaIn, ref object pvaOut)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion

        #region IServiceProvider Members

        HResult Interop.IServiceProvider.QueryService(ref Guid guidService,
                                                      ref Guid riid,
                                                      out IntPtr ppvObject)
        {
            if (riid == typeof(IOleCommandTarget).GUID)
            {
                ppvObject = Marshal.GetComInterfaceForObject(this,
                    typeof(IOleCommandTarget));
            }
            else if (riid == typeof(IShellBrowser).GUID)
            {
                ppvObject = Marshal.GetComInterfaceForObject(this,
                    typeof(IShellBrowser));
            }
            else
            {
                ppvObject = IntPtr.Zero;
                return HResult.E_NOINTERFACE;
            }

            return HResult.S_OK;
        }

        #endregion

        protected ShellView m_ShellView;
        StatusBar m_StatusBar;
    }

    class DialogShellBrowser : ShellBrowser, ICommDlgBrowser, IMessageFilter
    {
        public DialogShellBrowser(ShellView shellView)
            : base(shellView) {
							t.Interval = 100;
							t.Tick += t_Tick;
				}

				void t_Tick(object sender, EventArgs e)
				{
					//m_ShellView.OnSelectionChanged();
					(sender as System.Windows.Forms.Timer).Stop();
				}

        #region ICommDlgBrowser Members

        HResult ICommDlgBrowser.OnDefaultCommand(IShellView ppshv)
        {
            //ShellItem[] selected = m_ShellView.SelectedItems;

            //if ((selected.Length > 0) && (selected[0].IsFolder))
            //{
            //    try
            //    {
            //        m_ShellView.Navigate(selected[0]);
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
            //else
            //{
            //    //m_ShellView.OnDoubleClick(EventArgs.Empty);
            //}

            return HResult.S_OK;
        }

				System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        HResult ICommDlgBrowser.OnStateChange(IShellView ppshv, CDBOSC uChange)
        {
            if (uChange == CDBOSC.CDBOSC_SELCHANGE)
            {
							if (t.Enabled)
							{
								t.Stop();
								t.Start();
							}
							else
							{
								t.Start();
							}
                
            }
            else if (uChange == CDBOSC.CDBOSC_SETFOCUS)
            {
                //m_ShellView.OnGotFocus();
            }
            else if (uChange == CDBOSC.CDBOSC_KILLFOCUS)
            {
                //m_ShellView.OnLostFocus();
            }
						
            return HResult.S_OK;
        }

				public bool PreFilterMessage(ref Message m)
				{
					if (m.Msg == (int)WM.WM_PAINT || m.Msg == (int)WM.WM_CREATE || m.Msg == (int)WM.WM_LBUTTONUP || m.Msg == (int)WM.WM_LBUTTONDOWN || m.Msg == (int)WM.WM_KEYUP || m.Msg == (int)WM.WM_KEYDOWN)
					{
						//User32.SendMessage(m_ShellView.ShellListViewHandle, User32.WM_CHANGEUISTATE, User32.MAKELONG(1, 1), 0);
					}
					return true;
				}
        HResult ICommDlgBrowser.IncludeObject(IShellView ppshv, IntPtr pidl)
        {
            return HResult.S_OK ;
        }

        #endregion
    }
}
