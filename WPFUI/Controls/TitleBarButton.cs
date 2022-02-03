// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFUI.Controls {
  /// <summary>
  /// Inherited from the <see cref="System.Windows.Controls.Button"/>, adding <see cref="Common.Icon"/>.
  /// </summary>
  public class TitleBarButton : System.Windows.Controls.Button {

    /// <summary>
    /// Property for <see cref="IsMouseOver"/>.
    /// </summary>
    public static readonly DependencyProperty IsMouseOverProperty = DependencyProperty.Register(nameof(IsMouseOver),
        typeof(bool), typeof(TitleBarButton), new PropertyMetadata(false));


    public bool IsMouseOver {
      get => (bool)GetValue(IsMouseOverProperty);
      set => SetValue(IsMouseOverProperty, value);
    }

    public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(nameof(IsPressed),
      typeof(bool), typeof(TitleBarButton), new PropertyMetadata(false));


    public bool IsPressed {
      get => (bool)GetValue(IsPressedProperty);
      set => SetValue(IsPressedProperty, value);
    }


  }
}