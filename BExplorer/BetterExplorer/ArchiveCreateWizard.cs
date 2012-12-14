using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using SevenZip;

namespace BetterExplorer
{

    public partial class ArchiveCreateWizard : Form
    {
        OutArchiveFormat format = OutArchiveFormat.Zip;
        string ext = ".zip";
        bool cos = false;
        bool hsa = false;

        public ArchiveCreateWizard()
        {
            InitializeComponent();
            radioButton1.Checked = true;
        }

        public ArchiveCreateWizard(ShellObjectCollection list)
        {
            InitializeComponent();
            radioButton1.Checked = true;
            ListFilesToBeAdded(list);
        }

        public ArchiveCreateWizard(ShellObjectCollection list, string output_folder)
        {
            InitializeComponent();
            radioButton1.Checked = true;
            ListFilesToBeAdded(list);
            this.txtOutput.Text = output_folder;
            this.openFileDialog1.InitialDirectory = output_folder;
        }

        public void ListFilesToBeAdded(ShellObjectCollection list)
        {
            foreach (ShellObject item in list)
            {
                if (item.IsFolder == true)
                {
                    try
                    {
                        System.IO.DirectoryInfo dd = new System.IO.DirectoryInfo(item.ParsingName);
                        foreach (System.IO.FileInfo file in dd.GetFiles("*.*", System.IO.SearchOption.AllDirectories))
                        {
                            listBox1.Items.Add(file.FullName);
                        }
                    }
                    catch
                    {
                        listBox1.Items.Add(item.ParsingName);
                    }
                }
                else
                {
                    System.IO.FileInfo fd = new System.IO.FileInfo(item.ParsingName);
                    listBox1.Items.Add(item.ParsingName);
                }
            }

            UpdateFileListFromBox();

            try
            {
                this.txtName.Text = new System.IO.FileInfo((string)this.listBox1.Items[0]).Name + "_compressed";
            }
            catch
            {
                
            }
        }

        private void UpdateFileListFromBox()
        {
            int tfc = 0;
            long tfs = 0;

            foreach (object item in listBox1.Items)
            {
                System.IO.FileInfo fd = new System.IO.FileInfo((string)item);
                tfc += 1;
                tfs += fd.Length;
            }

            if (tfc > 1)
            {
                this.radioButton3.Visible = false;
                this.radioButton4.Visible = false;
                this.radioButton6.Visible = false;
                label7.Visible = true;
            }
            else
            {
                this.radioButton3.Visible = true;
                this.radioButton4.Visible = true;
                this.radioButton6.Visible = true;
                label7.Visible = false;
            }

            this.lblCount.Text = Convert.ToString(tfc);
            this.lblSize.Text = FriendlySizeConverter.GetFriendlySize(tfs);

            if (this.listBox1.SelectedItems.Count > 0)
            {
                this.button3.Enabled = true;
            }
            else
            {
                this.button3.Enabled = false;
            }

            if (tfc == 0)
            {
                if (hsa == false)
                {
                    MessageBox.Show("You have not selected any files. Please select some files and try again.", "Create Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cos = true;
                }
                else
                {
                    MessageBox.Show("There are currently no files listed to be archived. Please add some files before continuing.", "Create Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.wizardPage1.AllowNext = false;
                }
            }
            else
            {
                this.wizardPage1.AllowNext = true;
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            List<object> otr = new List<object>();

            foreach (object item in this.listBox1.SelectedItems)
            {
                otr.Add(item);
            }

            foreach (object item in otr)
            {
                this.listBox1.Items.Remove(item);
            }

            UpdateFileListFromBox();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItems.Count > 0)
            {
                this.button3.Enabled = true;
            }
            else
            {
                this.button3.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in this.openFileDialog1.FileNames)
                {
                    this.listBox1.Items.Add(file);
                }
            }

            UpdateFileListFromBox();
        }

        private CompressionLevel GetSpeedFromNumber(int lvl)
        {
            switch (lvl)
            {
                case -2:
                    return CompressionLevel.Fast;
                case -1:
                    return CompressionLevel.Low;
                case 0:
                    return CompressionLevel.Normal;
                case 1:
                    return CompressionLevel.High;
                case 2:
                    return CompressionLevel.Ultra;
                default:
                    return CompressionLevel.Normal;
            }
        }

        private void UpdateOutputLabel()
        {
            lblOutput.Text = txtOutput.Text + "\\" + txtName.Text + ext;
        }

        private void GetFormatFromString(string ft)
        {
            switch (ft)
            {
                case "ZIP (.zip)":
                    format = OutArchiveFormat.Zip;
                    ext = ".zip";
                    break;
                case "7ZIP (.7z)":
                    format = OutArchiveFormat.SevenZip;
                    ext = ".7z";
                    break;
                case "BZIP2 (.bz2)":
                    format = OutArchiveFormat.BZip2;
                    ext = ".bz2";
                    break;
                case "GZIP (.gz)":
                    format = OutArchiveFormat.GZip;
                    ext = ".gz";
                    break;
                case "TAR (.tar)":
                    format = OutArchiveFormat.Tar;
                    ext = ".tar";
                    break;
                case "XZ (.xz)":
                    format = OutArchiveFormat.XZ;
                    ext = ".xz";
                    break;
                default:
                    break;
            }
            UpdateOutputLabel();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked == true)
            {
                string ft = ((RadioButton)sender).Text;
                GetFormatFromString(ft);
            }
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            UpdateOutputLabel();
        }

        private void wizardControl1_Finished(object sender, EventArgs e)
        {
            List<string> listy = new List<string>();
            string password = null;
            foreach (object item in listBox1.Items)
            {
                if (System.IO.Directory.Exists((string)item) == true)
                {
                    try
                    {
                        System.IO.DirectoryInfo dd = new System.IO.DirectoryInfo((string)item);
                        foreach (System.IO.FileInfo file in dd.GetFiles("*.*", System.IO.SearchOption.AllDirectories))
                        {
                            listy.Add(file.FullName);
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    listy.Add((string)item);
                }
            }
            if (checkBox1.Checked == true)
            {
                password = txtPassword.Text;
            }
            var archiveProcressScreen = new ArchiveProcressScreen(listy, txtOutput.Text, ArchiveAction.Compress, txtName.Text, format, false, password, GetSpeedFromNumber(this.trackBar1.Value));
            archiveProcressScreen.Show();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.txtPassword.Enabled = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtOutput.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void wizardControl1_Cancelled(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ArchiveCreateWizard_Shown(object sender, EventArgs e)
        {
            if (cos == true)
            {
                this.Close();
            }
            else
            {
                hsa = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                wizardPage2.NextPage = wizardPage3;
            }
            else
            {
                wizardPage2.NextPage = wizardPage4;
            }
        }

    }
}
