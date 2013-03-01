using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WindowsHelper;

namespace BetterExplorer
{
    public partial class PreviewMedia : Form
    {
        public PreviewMedia()
        {
            InitializeComponent();
        }
        Image img;
        public void SetImage(string imagepath)
        {
            img = Image.FromFile(imagepath);
            pictureBox1.Image = img;
        }

        public void LoadPreview(Point pos)
        {
            int Xpos;
            int Ypos;
            if (Screen.PrimaryScreen.WorkingArea.Width - pos.X < this.Width + 10)
            {
                Xpos = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10;
            }
            else 
            {
                Xpos = pos.X;
            }

            if (Screen.PrimaryScreen.WorkingArea.Height - pos.Y < this.Height + 10)
            {
                Ypos = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 10;
            }
            else
            {
                Ypos = pos.Y;
            }
            try
            {
                WindowsAPI.ShowWindow(this.Handle, 4);
                WindowsAPI.SetWindowPos(this.Handle, WindowsAPI.HWND_TOPMOST, Xpos + 2, Ypos + 2, 0, 0,
                                WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_NOACTIVATE);
            }
            catch 
            {

            }
            
        }

        public void MovePreview(Point NewPos)
        {
            int Xpos;
            int Ypos;
            if (Screen.PrimaryScreen.WorkingArea.Width - NewPos.X < this.Width + 10)
            {
                Xpos = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10;
            }
            else
            {
                Xpos = NewPos.X;
            }

            if (Screen.PrimaryScreen.WorkingArea.Height - NewPos.Y < this.Height + 10)
            {
                Ypos = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 10;
            }
            else
            {
                Ypos = NewPos.Y;
            }

            try
            {
                WindowsAPI.SetWindowPos(this.Handle, WindowsAPI.HWND_TOPMOST, Xpos + 2, Ypos + 2, 0, 0,
                        WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_NOACTIVATE);
            }
            catch
            {

            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;

                baseParams.ExStyle |= (int)(
                  WindowsAPI.WS_EX_NOACTIVATE |
                  WindowsAPI.WS_EX_TOOLWINDOW);

                return baseParams;
            }
        }


        public void Disposeimg()
        {
            if (img != null)
             img.Dispose();
        }
    }
}
