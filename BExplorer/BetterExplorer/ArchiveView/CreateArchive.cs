using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SevenZip;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;

namespace BetterExplorer
{
    public partial class CreateArchive : Form
    {
        private readonly IList<string> _fileAndDirectoryFullPaths;
        private const string COMPRESS = "Compress";
        private const string EXTRACT = "Extract";

        public CreateArchive(IList<string> fileAndDirectoryFullPaths, bool compress, string archivePath, OutArchiveFormat defaultFormat = OutArchiveFormat.SevenZip, bool only = true)
        {
            InitializeComponent();

            _fileAndDirectoryFullPaths = fileAndDirectoryFullPaths;
            txt_archivePath.Text = archivePath;
            if (compress)
            {
                rbtn_extract.Enabled = only;
            }
            else
            {
                rbtn_compress.Enabled = only;
            }
            rbtn_compress.Checked = compress;
            rbtn_extract.Checked = !compress;

            txt_archivename.Text = Path.GetFileNameWithoutExtension(fileAndDirectoryFullPaths.First().Split('\\').Last());

            CreateGroepBoxEnum(typeof(OutArchiveFormat), gb_format);
            SetEnum(gb_format, defaultFormat.ToString());
            CreateGroepBoxEnum(typeof(CompressionLevel), gb_compressionlevel);
            SetEnum(gb_compressionlevel, CompressionLevel.Normal.ToString());
        }

        private void SetEnum(GroupBox groupBox, string radiobuttontext)
        {
            foreach (var control in groupBox.Controls)
            {
                if (control.GetType() != typeof(RadioButton))
                {
                    continue;
                }

                var radioButton = control as RadioButton;
                if (radioButton == null)
                {
                    continue;
                }

                if (radioButton.Text == radiobuttontext)
                {
                    radioButton.Checked = true;
                }
            }
        }

        private void CreateGroepBoxEnum(Type EnumType, GroupBox groupbox)
        {
            //fills the groepBox with alle posible options in the enum.
            var left = 10;
            var top = 20;

            var names = Enum.GetNames(EnumType);
            foreach(var name in names)
            {
                var radioButton = new RadioButton();
                radioButton.Text = name;
                radioButton.Left = left;
                radioButton.Top = top;
                groupbox.Controls.Add(radioButton);

                top += 20;
            }
        }

        private string FindSelectedRadiobutton(GroupBox groupBox)
        {
            //gets the name from the selected radiobutton.
            foreach(var control in groupBox.Controls)
            {
                if (control.GetType() != typeof(RadioButton))
                {
                    continue;
                }

                var radioButton = control as RadioButton;
                if(radioButton == null)
                {
                    continue;
                }

                if(radioButton.Checked)
                {
                    return radioButton.Text;
                }
            }

            return null;
        }

        public void Archive(object sender, EventArgs args)
        {
            bool compress = rbtn_compress.Checked;

            if (!compress)
            {
                var archiveProcressScreen = new ArchiveProcressScreen(_fileAndDirectoryFullPaths,
                                               txt_archivePath.Text,
                                               ArchiveAction.Extract,
                                               "");
                archiveProcressScreen.Show();
            }
            else
            {
                //cast the values from the radiobuttons to enums.
                var format = OutArchiveFormat.Zip;
                var value = FindSelectedRadiobutton(gb_format);
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    format = (OutArchiveFormat)Enum.Parse(typeof(OutArchiveFormat), value);
                }

                var level = CompressionLevel.Normal;
                var value2 = FindSelectedRadiobutton(gb_compressionlevel);
                if (string.IsNullOrWhiteSpace(value2) == false)
                {
                    level = (CompressionLevel)Enum.Parse(typeof(CompressionLevel), value2);
                }

                var password = string.IsNullOrWhiteSpace(txt_password.Text) ? null : txt_password.Text;

                var archiveProcressScreen = new ArchiveProcressScreen(_fileAndDirectoryFullPaths, txt_archivePath.Text,  ArchiveAction.Compress,txt_archivename.Text, format, cb_fastCompression.Checked, password, level);
                archiveProcressScreen.Show();
            }
            Dispose(true);
        }

        private void rbtn_compress_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;
            if(radioButton.Checked == false)
            {
                return;
            }

            gb_format.Visible = true;
            gb_compressionlevel.Visible = true;
            txt_password.Visible = true;
            cb_fastCompression.Visible = true;
            lbl_password.Visible = true;
            btn_execute.Text = COMPRESS;
            lbl_archive.Visible = true;
            txt_archivename.Visible = true;
        }

        private void rbtn_extract_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton.Checked == false)
            {
                return;
            }

            gb_format.Visible = false;
            gb_compressionlevel.Visible = false;
            txt_password.Visible = false;
            cb_fastCompression.Visible = false;
            lbl_password.Visible = false;
            btn_execute.Text = EXTRACT;
            lbl_archive.Visible = false;
            txt_archivename.Visible = false;
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Dispose(true);
        }

        private void btnPickDir_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog SelectFolderDlg = new CommonOpenFileDialog();
            SelectFolderDlg.IsFolderPicker = true;
            if (SelectFolderDlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txt_archivePath.Text = SelectFolderDlg.FileName;
            }
        }

        private void CreateArchive_Load(object sender, EventArgs e)
        {
        }

        private void CreateArchive_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

    }
}
