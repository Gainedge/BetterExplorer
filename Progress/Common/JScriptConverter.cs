using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace RateBar
{
    /// <summary>
    /// Class used for multi-binding analysis of simple maths expressions
    /// </summary>
    public class JScriptConverter : IMultiValueConverter, IValueConverter
    {
        private delegate object Evaluator(string code, object[] values);
        private static Evaluator evaluator;

        /// <summary>
        /// Initializes the <see cref="JScriptConverter"/> class.
        /// </summary>
		static JScriptConverter()
		{
            try
            {
                string source =
                    @"import System; 

                class Eval
                {
                    public function Evaluate(code : String, values : Object[]) : Object
                    {
                        return eval(code);
                    }
                }";

                // Load all the assemblies that are currently used by the AppDomain
                // for processing.
                CompilerParameters cp = new CompilerParameters();
                cp.GenerateInMemory = true;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                  if (!assembly.IsDynamic)
                  {
                    if (System.IO.File.Exists(assembly.Location))
                      cp.ReferencedAssemblies.Add(assembly.Location);
                  }
                }

                // Compile the delegate that we can use for evaluation of the bindings
                CompilerResults results = (new Microsoft.JScript.JScriptCodeProvider()).CompileAssemblyFromSource(cp, source);
                Assembly result = results.CompiledAssembly;
                Type eval = result.GetType("Eval");

                // Return the Evaluator
                evaluator = (Delegate.CreateDelegate(typeof(Evaluator),Activator.CreateInstance(eval),"Evaluate") as Evaluator);
            }
            catch
            {
                // Ignore exceptions in design mode
                if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
                    return;

                throw;
            }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [trap exceptions].
		/// </summary>
		/// <value>
		///   <c>true</c> if [trap exceptions]; otherwise, <c>false</c>.
		/// </value>
        public bool TrapExceptions
        {
            get
            {
                if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
                    return true;

                return this.trapExceptions;
            }
            set
            {
                this.trapExceptions = value;
            }
        }
        private bool trapExceptions;

        #region Methods

        /// <inheritdoc />
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
				      object result = evaluator(parameter.ToString(), values);
				      Trace.WriteLine(result);
				      return result;
            }
            catch
            {
				      if (TrapExceptions)
                    return null;
                else
                    throw;
            }
        }

		/// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(new object[] { value }, targetType, parameter, culture);
        }

		/// <inheritdoc />
        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException();
        }

		/// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException();
        }

        #endregion
    }
}