using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WindowsAPICodePack.Controls.WindowsForms {
  /// <summary>
  /// BHID class
  /// </summary>
  public class BHID {
    public static Guid SFObject {
      get { return m_SFObject; }
    }

    public static Guid SFUIObject {
      get { return m_SFUIObject; }
    }

    static Guid m_SFObject = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");
    static Guid m_SFUIObject = new Guid("3981e225-f559-11d3-8e3a-00c04f6837d5");
  }
}
