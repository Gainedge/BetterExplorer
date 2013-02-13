using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace WPFPieChart
{
    public class FolderSizeInfoClass : INotifyPropertyChanged
    {
        private String myFolderSizeLoc;

        public String FolderSizeLoc
        {
            get { return myFolderSizeLoc; }
            set {
                myFolderSizeLoc = value;
                RaisePropertyChangeEvent("FolderSizeLoc");
            }
        }

        private double fSize;

        public double FSize
        {
            get { return fSize; }
            set {
                fSize = value;
                RaisePropertyChangeEvent("FSize");
            }
        }

        private static long GetFolderSize(string dir, bool includesubdirs)
        {

            long retsize = 0;
            try
            {
                DirectoryInfo data = new DirectoryInfo(dir);
                if (includesubdirs == true)
                {
                    foreach (FileInfo item in data.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        retsize += item.Length;
                    }
                }
                else
                {
                    foreach (FileInfo item in data.GetFiles("*.*", SearchOption.TopDirectoryOnly))
                    {
                        retsize += item.Length;
                    }
                }
            }
            catch (Exception)
            {


            }

            return retsize;
        }

        public static List<FolderSizeInfoClass> ConstructData(string Dir)
        {

            List<FolderSizeInfoClass> FolderInfoSize = new List<FolderSizeInfoClass>();
            DirectoryInfo data = new DirectoryInfo(Dir);
            foreach (DirectoryInfo item in data.GetDirectories())
            {
                FolderSizeInfoClass fsi = new FolderSizeInfoClass();
                fsi.FolderSizeLoc = item.Name;
                fsi.FSize = GetFolderSize(item.FullName,true);
                FolderInfoSize.Add(fsi);
            }

            return FolderInfoSize;
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangeEvent(String propertyName)
        {
            if (PropertyChanged!=null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            
        }

        #endregion
    }
}
