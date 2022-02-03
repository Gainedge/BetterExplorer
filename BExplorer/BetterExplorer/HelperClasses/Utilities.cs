using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BExplorer.Shell;
using Microsoft.Win32;

namespace BetterExplorer {
  public static class Utilities {

    static Utilities() {
    }

    #region File Extensions

    public static string RemoveExtensionsFromFile(string file, string ext) {
      return file.EndsWith(ext) ? file.Remove(file.LastIndexOf(ext), ext.Length) : file;
    }

    /*
    [System.Diagnostics.DebuggerStepThrough()]
    public static string GetExtension(string file) {
      return file.Substring(file.LastIndexOf("."));
    }
    */

    #endregion


    /// <summary>
    /// 
    /// </summary>
    /// <param name="header"></param>
    /// <param name="tag"></param>
    /// <param name="icon"></param>
    /// <param name="name"></param>
    /// <param name="focusable"></param>
    /// <param name="checkable"></param>
    /// <param name="isChecked"></param>
    /// <param name="GroupName"></param>
    /// <param name="onClick">Test</param>
    /// <returns></returns>	
    [System.Diagnostics.DebuggerStepThrough()]
    public static MenuItem Build_MenuItem(Object header = null, Object tag = null, Object icon = null, string name = null, object ToolTip = null,
      bool focusable = true, bool checkable = false, bool isChecked = false, string GroupName = null, System.Windows.RoutedEventHandler onClick = null) {

      var Item = new MenuItem() {
        Name = name,
        Header = header,
        Tag = tag,
        Focusable = focusable,
        IsCheckable = checkable,
        IsChecked = isChecked,
        Icon = icon,
        //GroupName = GroupName,
        ToolTip = ToolTip
      };

      if (onClick != null) Item.Click += onClick;
      return Item;
    } //TODO: Convert this into an extension


    [System.Diagnostics.DebuggerStepThrough()]
    public static string GetValueOnly(string property, string value) {
      return value.Substring(property.Length + 1);
    }


    /// <summary>
    /// Move somewhere else later
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static System.Windows.ResourceDictionary Load(string filename) {
      if (System.IO.File.Exists(filename)) {
        using (var s = new System.IO.FileStream(filename, System.IO.FileMode.Open)) {
          return System.Windows.Markup.XamlReader.Load(s) as System.Windows.ResourceDictionary;
        }
      }
      else {
        return null;
      }
    }//TODO: Move somewhere else later


    public static string AppDirectoryItem(string FileName) {
      var currentexePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
      var dir = System.IO.Path.GetDirectoryName(currentexePath);
      return System.IO.Path.Combine(dir, FileName);
    }

    public static string CombinePaths(List<BExplorer.Shell._Plugin_Interfaces.IListItemEx> paths, string separatorvalue = ";", bool checkforfolders = false) {
      var ret = String.Empty;

      foreach (var item in paths) {
        if (!checkforfolders)
          ret += separatorvalue + item.ParsingName.ToShellParsingName();
        else if (item.IsFolder)
          ret += $"{separatorvalue}(f){item.ParsingName.ToShellParsingName()}";
        else
          ret += separatorvalue + item.ParsingName.ToShellParsingName();
      }

      if (ret.StartsWith(separatorvalue))
        ret = ret.Substring(1);

      return ret;
    }
  }
}
