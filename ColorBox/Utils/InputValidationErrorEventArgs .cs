/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorBox
{
    internal delegate void InputValidationErrorEventHandler(object sender, InputValidationErrorEventArgs e);

    internal class InputValidationErrorEventArgs : EventArgs
    {
        Exception _exception;
        bool _throwException;

        public InputValidationErrorEventArgs(Exception e)
        {
            Exception = e;
        }
    
 
        public Exception Exception
        {
            get
            {
                return _exception;
            }
            private set
            {
                _exception = value;
            }
        }
            
        public bool ThrowException
        {
            get
            {
                return _throwException;
            }
            set
            {
                _throwException = value;
            }
        }        
    }

    internal interface IValidateInput
    {
        event InputValidationErrorEventHandler InputValidationError;
        bool CommitInput();
    }
}