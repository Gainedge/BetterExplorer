using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BetterExplorerControls
{
	public class PathExistsValidationRule : ValidationRule
	{
		#region Constructor

		public PathExistsValidationRule(IHierarchyHelper hierarchyHelper, object root)
		{
			_hierarchyHelper = hierarchyHelper;
			_root = root;
		}

		public PathExistsValidationRule()
		{

		}

		#endregion

		#region Methods

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			try
			{
				if (!(value is string))
					return new ValidationResult(false, "Invalid Path");

				if (_hierarchyHelper.GetItem(_root, (string)value) == null)
					return new ValidationResult(false, "Path Not Found");
			}
			catch (Exception ex)
			{
				return new ValidationResult(false, "Invalid Path");
			}
			return new ValidationResult(true, null);
		}

		#endregion

		#region Data

		IHierarchyHelper _hierarchyHelper;
		object _root;

		#endregion

		#region Public Properties

		#endregion
	}
}
