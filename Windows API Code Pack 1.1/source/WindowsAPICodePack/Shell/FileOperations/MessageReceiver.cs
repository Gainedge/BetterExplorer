using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations
{
    public partial class MessageReceiver : Form
    {
        public event EventHandler<MessageEventArgs> OnMessageReceived;
      public event EventHandler<MessageEventArgs> OnInitAdminOP;
        uint WM_FOWINC = WindowsAPI.RegisterWindowMessage("BE_FOWINC");
        public MessageReceiver(string title)
        {
            InitializeComponent();
            this.Text = title;
            this.Show();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_FOWINC)
            {

              if (OnInitAdminOP != null)
                OnInitAdminOP.Invoke(this, new MessageEventArgs(m.LParam.ToString()));
            }
            if (m.Msg == WindowsAPI.WM_COPYDATA)
            {
                byte[] b = new Byte[Marshal.ReadInt32(m.LParam, IntPtr.Size)];
                IntPtr dataPtr = Marshal.ReadIntPtr(m.LParam, IntPtr.Size * 2);
                Marshal.Copy(dataPtr, b, 0, b.Length);
                string message = System.Text.Encoding.Unicode.GetString(b);

                if (OnMessageReceived != null)
                    OnMessageReceived.Invoke(this, new MessageEventArgs(message));
            }
            base.WndProc(ref m);

        }
        

    }

    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    } 
}
