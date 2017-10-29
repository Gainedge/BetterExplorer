using System;
using System.Drawing;
using System.Windows.Forms;


namespace wyDay.Controls
{
    internal partial class frmChanges : Form
    {
        public bool UpdateNow { get; private set; }

        public frmChanges(string version, string changes, bool isRTF, bool showUpdateNow, AUTranslation translation)
        {
            Font = SystemFonts.MessageBoxFont;

            InitializeComponent();

            if (isRTF)
                richChanges.Rtf = changes;
            else
                richChanges.Text = changes;

            Text = lblTitle.Text = translation.ChangesInVersion.Replace("%version%", version);
            lblTitle.Text += ":";

            btnUpdateNow.Text = translation.UpdateNowButton;
            btnOK.Text = translation.CloseButton;

            // resize the buttons to fit their contents
            btnUpdateNow.Left = btnOK.Left - btnUpdateNow.Width - 6;

            MinimumSize = new Size(richChanges.Left + Right - btnUpdateNow.Left, 250);

            // update now
            if (!showUpdateNow)
                btnUpdateNow.Visible = false;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (lblTitle != null)
            {
                lblTitle.Width = ClientRectangle.Width - 2 * lblTitle.Left;
                richChanges.Top = lblTitle.Bottom + 5;
            }

            // resize the textbox to fill the form
            if (richChanges != null)
            {
                richChanges.Width = ClientRectangle.Width - 2 * richChanges.Left;
                richChanges.Height = ClientRectangle.Height - richChanges.Top - richChanges.Left - (ClientRectangle.Height - btnOK.Top);
            }
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        void btnUpdateNow_Click(object sender, EventArgs e)
        {
            UpdateNow = true;
            Close();
        }
    }
}
