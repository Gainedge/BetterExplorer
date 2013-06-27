using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AlexPilotti.FTPS.Common
{
    /// <summary>
    /// Based on Adarsh's code: http://blogs.msdn.com/adarshk/archive/2004/09/15/230177.aspx
    /// </summary>
    class DirectoryListParser
    {
        enum EDirectoryListingStyle { UnixStyle, WindowsStyle, Unknown }

        const string unixSymLinkPathSeparator = " -> ";

        public static IList<DirectoryListItem> GetDirectoryList(string datastring)
        {
            try
            {
                List<DirectoryListItem> myListArray = new List<DirectoryListItem>();
                string[] dataRecords = datastring.Split('\n');
                EDirectoryListingStyle _directoryListStyle = GuessDirectoryListingStyle(dataRecords);
                foreach (string s in dataRecords)
                {
                    if (_directoryListStyle != EDirectoryListingStyle.Unknown && s != "")
                    {
                        DirectoryListItem f = new DirectoryListItem();
                        f.Name = "..";
                        switch (_directoryListStyle)
                        {
                            case EDirectoryListingStyle.UnixStyle:
                                f = ParseDirectoryListItemFromUnixStyleRecord(s);
                                break;
                            case EDirectoryListingStyle.WindowsStyle:
                                f = ParseDirectoryListItemFromWindowsStyleRecord(s);
                                break;
                        }
                        if (!(f == null || f.Name == "." || f.Name == ".."))
                        {
                            myListArray.Add(f);
                        }
                    }
                }
                return myListArray; ;
            }
            catch (Exception ex)
            {
                throw new FTPException("Unable to parse the directory list", ex);
            }
        }

        private static DirectoryListItem ParseDirectoryListItemFromWindowsStyleRecord(string record)
        {
            ///Assuming the record style as
            /// 02-03-04  07:46PM       <DIR>          Append
            DirectoryListItem f = new DirectoryListItem();
            string processstr = record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            f.CreationTime = DateTime.Parse(dateStr + " " + timeStr, CultureInfo.GetCultureInfo("en-US"));
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                f.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                f.IsDirectory = false;

                int i = processstr.IndexOf(' ');
                f.Size = ulong.Parse(processstr.Substring(0, i));

                processstr = processstr.Substring(i + 1);
            }
            f.Name = processstr;  //Rest is name   
            return f;
        }

        private static EDirectoryListingStyle GuessDirectoryListingStyle(string[] recordList)
        {
            foreach (string s in recordList)
            {
                if (s.Length > 10
                 && Regex.IsMatch(s.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return EDirectoryListingStyle.UnixStyle;
                }
                else if (s.Length > 8
                 && Regex.IsMatch(s.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return EDirectoryListingStyle.WindowsStyle;
                }
            }
            return EDirectoryListingStyle.Unknown;
        }

        private static DirectoryListItem ParseDirectoryListItemFromUnixStyleRecord(string record)
        {
            ///Assuming record style as
            /// dr-xr-xr-x   1 owner    group               0 Nov 25  2002 bussys

            // Mac OS X - tnftpd returns the total on the first line
            if (record.ToLower().StartsWith("total "))
                return null;

            DirectoryListItem f = new DirectoryListItem();
            string processstr = record.Trim();
            f.Flags = processstr.Substring(0, 9);
            f.IsDirectory = (f.Flags[0] == 'd');
            // Note: there is no way to determine here if the symlink refers to a dir or a file
            f.IsSymLink = (f.Flags[0] == 'l');                
            processstr = (processstr.Substring(11)).Trim();
            CutSubstringFromStringWithTrim(ref processstr, " ", 0);   //skip one part
            f.Owner = CutSubstringFromStringWithTrim(ref processstr, " ", 0);
            f.Group = CutSubstringFromStringWithTrim(ref processstr, " ", 0);
            f.Size = ulong.Parse(CutSubstringFromStringWithTrim(ref processstr, " ", 0));  
            
            string creationTimeStr = CutSubstringFromStringWithTrim(ref processstr, " ", 8);
            string dateFormat;
            if(creationTimeStr.IndexOf(':') < 0)
                dateFormat = "MMM dd yyyy";
            else
                dateFormat = "MMM dd H:mm";

            // Some servers (e.g.: Mac OS X 10.5 - tnftpd) return days < 10 without a leading 0 
            if (creationTimeStr[4] == ' ')
                creationTimeStr = creationTimeStr.Substring(0, 4) + "0" + creationTimeStr.Substring(5);

            f.CreationTime = DateTime.ParseExact(creationTimeStr, dateFormat, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces);

            if (f.IsSymLink && processstr.IndexOf(unixSymLinkPathSeparator) > 0)
            {
                f.Name = CutSubstringFromStringWithTrim(ref processstr, unixSymLinkPathSeparator, 0);
                f.SymLinkTargetPath = processstr;
            }
            else
                f.Name = processstr;   //Rest of the part is name
            return f;
        }

        private static string CutSubstringFromStringWithTrim(ref string s, string str, int startIndex)
        {
            int pos1 = s.IndexOf(str, startIndex);
            string retString = s.Substring(0, pos1);
            s = (s.Substring(pos1 + str.Length)).Trim();
            return retString;
        }
    }
}
