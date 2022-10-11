using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ShellControls.ShellContextMenu {
  // Licensed to the .NET Foundation under one or more agreements.
  // The .NET Foundation licenses this file to you under the MIT license.
  // See the LICENSE file in the project root for more information.

  /// <remarks>
  /// To be kept outside <see cref="ObservableCollection{T}"/>, since otherwise, a new instance will be created for each generic type used.
  /// </remarks>
  internal static class EventArgsCache {
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
    internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
  }
}