using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BetterExplorerControls
{
	public class PathHierarchyHelper : IHierarchyHelper
	{
		#region Constructor

		public PathHierarchyHelper(string parentPath, string valuePath, string subEntriesPath)
		{
			ParentPath = parentPath;
			ValuePath = valuePath;
			SubentriesPath = subEntriesPath;
			Separator = '\\';
			StringComparisonOption = StringComparison.CurrentCultureIgnoreCase;
		}

		#endregion

		#region Methods

		#region Utils Func - extractPath/Name
		public virtual string ExtractPath(string pathName)
		{
			if (String.IsNullOrEmpty(pathName))
				return "";
			if (pathName.IndexOf(Separator) == -1)
				return "";
			else return pathName.Substring(0, pathName.LastIndexOf(Separator));
		}

		public virtual string ExtractName(string pathName)
		{
			if (String.IsNullOrEmpty(pathName))
				return "";
			if (pathName.IndexOf(Separator) == -1)
				return pathName;
			else return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
		}
		#endregion

		#region Overridable to improve speed.

		protected virtual object getParent(object item)
		{
			return PropertyPathHelper.GetValueFromPropertyInfo(item, ParentPath);
		}

		protected virtual string getValuePath(object item)
		{
			return PropertyPathHelper.GetValueFromPropertyInfo(item, ValuePath) as string;
		}

		protected virtual IEnumerable getSubEntries(object item)
		{
			return PropertyPathHelper.GetValueFromPropertyInfo(item, SubentriesPath) as IEnumerable;
		}

		#endregion

		#region Implements

		public IEnumerable<object> GetHierarchy(object item, bool includeCurrent)
		{
			if (includeCurrent)
				yield return item;

			var current = getParent(item);
			while (current != null)
			{
				yield return current;
				current = getParent(current);
			}
		}

		public string GetPath(object item)
		{
			return item == null ? "" : getValuePath(item);
		}

		public IEnumerable List(object item)
		{
			return item is IEnumerable ? item as IEnumerable : getSubEntries(item);
		}

		public object GetItem(object rootItem, string path)
		{
			var queue = new Queue<string>(path.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries));
			object current = rootItem;
			while (current != null && queue.Any())
			{
				var nextSegment = queue.Dequeue();
				object found = null;
				foreach (var item in List(current))
				{
					string valuePathName = getValuePath(item);
					string value = ExtractName(valuePathName); //Value may be full path, or just current value.
					if (value.Equals(nextSegment, StringComparisonOption))
					{
						found = item;
						break;
					}
				}
				current = found;
			}
			return current;
		}

		#endregion

		#endregion

		#region Data

		#endregion

		#region Public Properties

		public char Separator { get; set; }
		public StringComparison StringComparisonOption { get; set; }
		public string ParentPath { get; set; }
		public string ValuePath { get; set; }
		public string SubentriesPath { get; set; }

		#endregion
	}

	/// <summary>
	/// Generic version of AutoHierarchyHelper, which use Path to query for hierarchy of ViewModels
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PathHierarchyHelper<T> : PathHierarchyHelper
	{
		#region Constructor

		public PathHierarchyHelper(string parentPath, string valuePath, string subEntriesPath)
			: base(parentPath, valuePath, subEntriesPath)
		{
			propInfoSubEntries = typeof(T).GetProperty(subEntriesPath);
			propInfoValue = typeof(T).GetProperty(valuePath);
			propInfoParent = typeof(T).GetProperty(parentPath);
		}

		#endregion

		#region Methods

		protected override object getParent(object item)
		{
			return propInfoParent.GetValue(item);
		}

		protected override IEnumerable getSubEntries(object item)
		{
			return propInfoSubEntries.GetValue(item) as IEnumerable;
		}

		protected override string getValuePath(object item)
		{
			return propInfoValue.GetValue(item) as string;
		}

		#endregion

		#region Data

		PropertyInfo propInfoValue, propInfoSubEntries, propInfoParent;

		#endregion

		#region Public Properties

		#endregion
	}
}
