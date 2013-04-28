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
        public event EventHandler<EventArgs> OnMessageReceived;
        private ShellNotifications.ShellNotifications Notifications = new ShellNotifications.ShellNotifications();

        public MessageReceiver()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
          if (m.Msg == ShellNotifications.ShellNotifications.WM_SHNOTIFY)
          {
            if (Notifications.NotificationReceipt(m.WParam, m.LParam))
            {
              OnMessageReceived.Invoke(this, new EventArgs());
            }

          }
            base.WndProc(ref m);

        }

        private void MessageReceiver_Load(object sender, EventArgs e)
        {
          Notifications.RegisterChangeNotify(
          this.Handle,
          ShellNotifications.ShellNotifications.CSIDL.CSIDL_DESKTOP,
          true);

        }

        private void MessageReceiver_FormClosing(object sender, FormClosingEventArgs e)
        {
          Notifications.UnregisterChangeNotify();
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
