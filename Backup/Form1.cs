using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Windows7.DesktopIntegration.WindowsForms;

namespace Taskbar_AppId
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			AppIdTextBox.Text = WindowsFormsExtensions.GetAppId(this);
		}

		private void SetAppIdButton_Click(object sender, EventArgs e)
		{
			WindowsFormsExtensions.SetAppId(this, AppIdTextBox.Text);
		}
	}
}
