using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using Microsoft.WindowsAPICodePack.Dialogs;
using SevenZip;
using SystemImageList;
using BExplorer.Shell.Interop;

namespace BetterExplorer
{
    public partial class ArchiveDetailView : Form
    {
        //private readonly IconReader _iconReader;
        private readonly string _pathArchive;
        private const string CHECK_ERROR = "The Archive is valid and contains no errors";
        private const string CHECK_OKE = "The Archive is not valid and contains errors!";
        SysImageList lst = new SysImageList(SysImageListSize.extraLargeIcons);
        public ArchiveDetailView(string pathIconLibrary, string pathArchive)
        {
            InitializeComponent();


            //_iconReader = new IconReader();
            //archiveTree.ImageList = new ImageList();
            //_iconReader.ReadIcons(pathIconLibrary).ForEach(o => archiveTree.ImageList.Images.Add(o.Icon));
            UxTheme.SetWindowTheme(lvArchiveDetails.Handle, "Explorer", 0);
            SysImageListHelper.SetListViewImageList(lvArchiveDetails, lst, false);
            SevenZipExtractor.SetLibraryPath(IntPtr.Size == 8 ? "7z64.dll" : "7z32.dll");
            _pathArchive = pathArchive;
            
            Shown += ShowArchiveContent;
        }
        
        public void ShowArchiveContent(object sender, EventArgs args)
        {
            archiveTree.Nodes.Clear();
            var defrag = new SevenZipExtractor(_pathArchive);
            defrag.PreserveDirectoryStructure = true;

            var parentNode = new TreeNode(defrag.FileName);

            ReadDirectory(defrag.ArchiveFileData, parentNode, 0);
            //archiveTree.Nodes.Add(parentNode);
            //archiveTree.Update();

        }

        private void ReadDirectory(IList<ArchiveFileInfo> archiveFileInfos, TreeNode parentNode, int level)
        {

            lvArchiveDetails.Tag = archiveFileInfos;
            lvArchiveDetails.BeginUpdate();
            for (int i = 0; i < archiveFileInfos.Count; i++)
            {
                var Splittedname = archiveFileInfos[i].FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (Splittedname.Length == 1)
                {
                    
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = Path.GetFileNameWithoutExtension(Splittedname[0]);
                    lvi.Tag = archiveFileInfos[i].FileName;
                    if (archiveFileInfos[i].IsDirectory)
                    {
                        lvi.ImageIndex = lst.IconIndex(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),true);
                        lvi.SubItems.Add("Folder");
                    }
                    else
                    {
                        lvi.ImageIndex = lst.IconIndex(Path.GetExtension(archiveFileInfos[i].FileName));
                        lvi.SubItems.Add("File");
                    }

                    
                    lvArchiveDetails.Items.Add(lvi);
                }
            }
            lvArchiveDetails.EndUpdate();

            //for (int j = 0; j < archiveFileInfos.Count;j++)
            //{
            //    var childNode = new TreeNode();
            //    var names = archiveFileInfos[j].FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            //    childNode.Text = names[level];
            //    childNode.Tag = archiveFileInfos[j];


                

            //    if (archiveFileInfos[j].IsDirectory)
            //    {
            //        var fileInfos = new List<ArchiveFileInfo>();
            //        foreach(var fileInfo in archiveFileInfos)
            //        {
            //            var testnames = fileInfo.FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            //            if (testnames.Length - 1 < level + 1)
            //            {
            //                continue;
            //            }
            //            for (int i = 0; i <= level; i++)
            //            {
            //                if (testnames[i] != names[i])
            //                {
            //                    continue;
            //                }
            //            }
            //            fileInfos.Add(fileInfo);
            //        }
            //        ReadDirectory(fileInfos, childNode, level + 1);

            //        //childNode.ImageIndex = 3;
            //        j += fileInfos.Count;
            //    }
            //    //else
            //    //{
            //    //    childNode.ImageIndex = SelectImageIndex(names[names.Length -1]);
            //    //}

                
            //    parentNode.Nodes.Add(childNode);
            //}
        }

        private void PopulateListView()
        {
            foreach (TreeNode item in archiveTree.Nodes[0].Nodes)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = item.Text;
                lvArchiveDetails.Items.Add(lvi);
            }
        }
        
        private void btn_extract_Click(object sender, EventArgs e)
        {
            var files = new List<string>() {_pathArchive };
            var directoryName = Path.GetDirectoryName(_pathArchive);

            var createArchive = new CreateArchive(files, false, directoryName, only: false);
            createArchive.Show();
        }

        private void btn_view_Click(object sender, EventArgs e)
        {
            var selectedNode = archiveTree.SelectedNode;
            if(selectedNode == null || selectedNode.Nodes.Count != 0)
                return;

            var tag = selectedNode.Tag;
            var archiveFileInfo = (ArchiveFileInfo)tag;
            var sevenZipExtractor = new SevenZipExtractor(_pathArchive);
            var filePath = String.Format("{0}\\{1}", Path.GetTempPath(), archiveFileInfo.FileName.Split('\\').Last());
            var fileStream = File.Create(filePath);
            sevenZipExtractor.ExtractFile(archiveFileInfo.FileName, fileStream);
            fileStream.Flush();
            fileStream.Close();
            Process.Start(filePath);
        }

        private void btn_removefile_Click(object sender, EventArgs e)
        {
            var selectedNode = archiveTree.SelectedNode;
            var fileAndDirectoryNames = new List<string>();
            if (selectedNode.Nodes.Count > 0)
            {
                foreach(TreeNode node in selectedNode.Nodes)
                {
                    var childTag = node.Tag;
                    var childArchiveFileInfo = (ArchiveFileInfo)childTag;

                    fileAndDirectoryNames.Add(childArchiveFileInfo.FileName);
                }
            }

            var tag = selectedNode.Tag;
            var archiveFileInfo = (ArchiveFileInfo)tag;
            fileAndDirectoryNames.Add(archiveFileInfo.FileName);

            var archiveProcressScreen = new ArchiveProcressScreen(fileAndDirectoryNames, _pathArchive, ArchiveAction.RemoveFile);
            archiveProcressScreen.Show();

            ShowArchiveContent(null,null);
        }

        private void btn_checkarchive_Click(object sender, EventArgs e)
        {
            var sevenZipExtractor = new SevenZipExtractor(_pathArchive);
            var check = sevenZipExtractor.Check();
            /*
            var dialog = new TaskDialog();
            dialog.Text = check ? CHECK_OKE : CHECK_ERROR;
            dialog.StandardButtons = TaskDialogStandardButtons.Ok;
            
            dialog.Show();
			*/ 
        }

        private void lvArchiveDetails_ItemActivate(object sender, EventArgs e)
        {
            IList<ArchiveFileInfo> archiveinfos = lvArchiveDetails.Tag as IList<ArchiveFileInfo>;
            List<ArchiveFileInfo> arhiveinfoList = new List<ArchiveFileInfo>();
            if (SelectedItem.SubItems[1].Text == "Folder")
            {
                foreach (ArchiveFileInfo item in archiveinfos)
                {
                    if (item.FileName.Contains(SelectedItem.Tag.ToString()))
                    {
                        arhiveinfoList.Add(item);
                    }
                }

                lvArchiveDetails.BeginUpdate();
                lvArchiveDetails.Items.Clear();
                ListViewItem lvir = new ListViewItem();
                lvir.SubItems.Add("UP");
                lvir.Text = "...";
                lvir.Tag = SelectedItem.Tag;
                lvArchiveDetails.Items.Add(lvir);
                foreach (ArchiveFileInfo afinfo in arhiveinfoList)
                {
                    var splittdname = afinfo.FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    var Itemsplitname = SelectedItem.Tag.ToString().Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Itemsplitname.Length == splittdname.Length - 1)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = Path.GetFileNameWithoutExtension(splittdname[splittdname.Length - 1]);
                        lvi.Tag = afinfo.FileName;
                        if (afinfo.IsDirectory)
                        {
                            lvi.ImageIndex = lst.IconIndex(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), true);
                            lvi.SubItems.Add("Folder");
                        }
                        else
                        {
                            lvi.ImageIndex = lst.IconIndex(Path.GetExtension(afinfo.FileName));
                            lvi.SubItems.Add("File");
                        }


                        lvArchiveDetails.Items.Add(lvi);
                    }
                }
                lvArchiveDetails.EndUpdate();
            }
            else if (SelectedItem.SubItems[1].Text == "UP")
            {
                foreach (ArchiveFileInfo item in archiveinfos)
                {
                    var splititem = item.FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (item.FileName.Contains(SelectedItem.Tag.ToString()) || splititem.Length == 1)
                    {
                        arhiveinfoList.Add(item);
                    }
                }

                lvArchiveDetails.BeginUpdate();
                lvArchiveDetails.Items.Clear();
                var splitSelected = SelectedItem.Tag.ToString().Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitSelected.Length >= 2)
                {
                    ListViewItem lvir = new ListViewItem();
                    lvir.SubItems.Add("UP");
                    lvir.Text = "...";
                    string UpPath = "";
                    string[] str = new string[splitSelected.Length - 1];
                    for (int k = 0; k < splitSelected.Length - 1; k++)
                    {
                        str[k] = splitSelected[k];

                    }
                    UpPath = Path.Combine(str);
                    lvir.Tag = UpPath;
                    lvArchiveDetails.Items.Add(lvir);
                }
                
                foreach (ArchiveFileInfo afinfo in arhiveinfoList)
                {
                    var splittdname = afinfo.FileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    var Itemsplitname = SelectedItem.Tag.ToString().Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Itemsplitname.Length == splittdname.Length)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = Path.GetFileNameWithoutExtension(splittdname[splittdname.Length - 1]);
                        lvi.Tag = afinfo.FileName;
                        if (afinfo.IsDirectory)
                        {
                            lvi.ImageIndex = lst.IconIndex(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), true);
                            lvi.SubItems.Add("Folder");
                        }
                        else
                        {
                            lvi.ImageIndex = lst.IconIndex(Path.GetExtension(afinfo.FileName));
                            lvi.SubItems.Add("File");
                        }


                        lvArchiveDetails.Items.Add(lvi);
                    }
                }
                lvArchiveDetails.EndUpdate();
            }
            arhiveinfoList = null;
        }

        ListViewItem SelectedItem;
        private void lvArchiveDetails_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            SelectedItem = e.Item;
        }
    }

    public class ArchiveInfo
    {
        private ArchiveFileInfo afile;
        ArchiveInfo(ArchiveFileInfo archfileinfo)
        {
            afile = archfileinfo;
        }

        public string FilePath
        {
            get 
            {
                return afile.FileName;
            }
        }
        public List<ArchiveInfo> ChildItems;
    }
}
