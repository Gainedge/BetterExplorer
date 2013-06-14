Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading

<TemplatePart(Name:=AutoCompleteTextBox.PartEditor, Type:=GetType(TextBox))> _
<TemplatePart(Name:=AutoCompleteTextBox.PartPopup, Type:=GetType(Popup))> _
<TemplatePart(Name:=AutoCompleteTextBox.PartSelector, Type:=GetType(Selector))> _
Public Class AutoCompleteTextBox
    Inherits Control

#Region "Fields"

    Public Const PartEditor As String = "PART_Editor"
    Public Const PartPopup As String = "PART_Popup"
    Public Const PartSelector As String = "PART_Selector"

    Public Shared ReadOnly DelayProperty As DependencyProperty = DependencyProperty.Register("Delay", GetType(Integer), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(200))
    Public Shared ReadOnly DisplayMemberProperty As DependencyProperty = DependencyProperty.Register("DisplayMember", GetType(String), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(String.Empty))
    Public Shared ReadOnly IsDropDownOpenProperty As DependencyProperty = DependencyProperty.Register("IsDropDownOpen", GetType(Boolean), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(False))
    Public Shared ReadOnly IsPopulatingProperty As DependencyProperty = DependencyProperty.Register("IsPopulating", GetType(Boolean), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(False))
    Public Shared ReadOnly IsReadOnlyProperty As DependencyProperty = DependencyProperty.Register("IsReadOnly", GetType(Boolean), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(False))
    Public Shared ReadOnly ItemTemplateProperty As DependencyProperty = DependencyProperty.Register("ItemTemplate", GetType(DataTemplate), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(Nothing))
    Public Shared ReadOnly ProviderProperty As DependencyProperty = DependencyProperty.Register("Provider", GetType(ISuggestionProvider), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(Nothing))
    Public Shared ReadOnly SelectedItemProperty As DependencyProperty = DependencyProperty.Register("SelectedItem", GetType(Object), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(Nothing))
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(String.Empty))

    Private _bindingEvaluator As BindingEvaluator
    Private _editor As TextBox
    Private _fetchTimer As DispatcherTimer
    Private _filter As String
    Private _isUpdatingText As Boolean
    Private _itemsSelector As Selector
    Private _popup As Popup
    Private _selectionAdapter As SelectionAdapter

#End Region 'Fields

#Region "Constructors"

    Shared Sub New()
        DefaultStyleKeyProperty.OverrideMetadata(GetType(AutoCompleteTextBox), New FrameworkPropertyMetadata(GetType(AutoCompleteTextBox)))
    End Sub

#End Region 'Constructors

#Region "Properties"

    Public Property BindingEvaluator() As BindingEvaluator
        Get
            Return _bindingEvaluator
        End Get
        Set(ByVal value As BindingEvaluator)
            _bindingEvaluator = value
        End Set
    End Property

    Public Property Delay() As Integer
        Get
            Return GetValue(DelayProperty)
        End Get

        Set(ByVal value As Integer)
            SetValue(DelayProperty, value)
        End Set
    End Property

    Public Property DisplayMember() As String
        Get
            Return GetValue(DisplayMemberProperty)
        End Get

        Set(ByVal value As String)
            SetValue(DisplayMemberProperty, value)
        End Set
    End Property

    Public Property Editor() As TextBox
        Get
            Return _editor
        End Get
        Set(ByVal value As TextBox)
            _editor = value
        End Set
    End Property

    Public Property FetchTimer() As DispatcherTimer
        Get
            Return _fetchTimer
        End Get
        Set(ByVal value As DispatcherTimer)
            _fetchTimer = value
        End Set
    End Property

    Public Property Filter() As String
        Get
            Return _filter
        End Get
        Set(ByVal value As String)
            _filter = value
        End Set
    End Property

    Public Property IsDropDownOpen() As Boolean
        Get
            Return GetValue(IsDropDownOpenProperty)
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsDropDownOpenProperty, value)
        End Set
    End Property

    Public Property IsPopulating() As Boolean
        Get
            Return GetValue(IsPopulatingProperty)
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsPopulatingProperty, value)
        End Set
    End Property

    Public Property IsReadOnly() As Boolean
        Get
            Return GetValue(IsReadOnlyProperty)
        End Get

        Set(ByVal value As Boolean)
            SetValue(IsReadOnlyProperty, value)
        End Set
    End Property

    Public Property ItemsSelector() As Selector
        Get
            Return _itemsSelector
        End Get
        Set(ByVal value As Selector)
            _itemsSelector = value
        End Set
    End Property

    Public Property ItemTemplate() As DataTemplate
        Get
            Return GetValue(ItemTemplateProperty)
        End Get

        Set(ByVal value As DataTemplate)
            SetValue(ItemTemplateProperty, value)
        End Set
    End Property

    Public Property Popup() As Popup
        Get
            Return _popup
        End Get
        Set(ByVal value As Popup)
            _popup = value
        End Set
    End Property

    Public Property Provider() As ISuggestionProvider
        Get
            Return GetValue(ProviderProperty)
        End Get

        Set(ByVal value As ISuggestionProvider)
            SetValue(ProviderProperty, value)
        End Set
    End Property

    Public Property SelectedItem() As Object
        Get
            Return GetValue(SelectedItemProperty)
        End Get

        Set(ByVal value As Object)
            SetValue(SelectedItemProperty, value)
        End Set
    End Property

    Public Property SelectionAdapter() As SelectionAdapter
        Get
            Return _selectionAdapter
        End Get
        Set(ByVal value As SelectionAdapter)
            _selectionAdapter = value
        End Set
    End Property

    Public Property Text() As String
        Get
            Return GetValue(TextProperty)
        End Get

        Set(ByVal value As String)
            SetValue(TextProperty, value)
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()
        Editor = Template.FindName(PartEditor, Me)
        Popup = Template.FindName(PartPopup, Me)
        ItemsSelector = Template.FindName(PartSelector, Me)
        BindingEvaluator = New BindingEvaluator(New Binding(DisplayMember))

        If Editor IsNot Nothing Then
            AddHandler Editor.TextChanged, AddressOf OnEditroTextChanged
            AddHandler Editor.PreviewKeyDown, AddressOf OnEditorKeyDown
            AddHandler Editor.LostFocus, AddressOf OnEditorLostFocus
        End If
        If Popup IsNot Nothing Then
            Popup.StaysOpen = False
            AddHandler Popup.Opened, AddressOf OnPopupOpened
            AddHandler Popup.Closed, AddressOf OnPopupClosed
        End If
        If ItemsSelector IsNot Nothing Then
            SelectionAdapter = New SelectionAdapter(ItemsSelector)
            AddHandler SelectionAdapter.Commit, AddressOf OnSelectionAdapterCommit
            AddHandler SelectionAdapter.Cancel, AddressOf OnSelectionAdapterCancel
            AddHandler SelectionAdapter.SelectionChanged, AddressOf OnSelectionAdapterSelectionChanged
        End If
    End Sub

    Private Function GetDisplayText(ByVal dataItem As Object) As String
        If BindingEvaluator Is Nothing Then
            BindingEvaluator = New BindingEvaluator(New Binding(DisplayMember))
        End If
        If dataItem Is Nothing Then
            Return String.Empty
        End If
        If String.IsNullOrEmpty(DisplayMember) Then
            Return dataItem.ToString()
        End If
        Return BindingEvaluator.Evaluate(dataItem)
    End Function

    Private Sub OnEditorKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If SelectionAdapter IsNot Nothing Then
            SelectionAdapter.HandleKeyDown(e)
        End If
    End Sub

    Private Sub OnEditorLostFocus(ByVal sender As Object, ByVal e As RoutedEventArgs)
        IsDropDownOpen = False
    End Sub

    Private Sub OnEditroTextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
        If _isUpdatingText Then Return
        If FetchTimer Is Nothing Then
            FetchTimer = New DispatcherTimer
            FetchTimer.Interval = TimeSpan.FromMilliseconds(Delay)
            AddHandler FetchTimer.Tick, AddressOf OnFetchTimerTick
        End If
        FetchTimer.IsEnabled = False
        FetchTimer.Stop()
        If Editor.Text.Length > 0 Then
            FetchTimer.IsEnabled = True
            FetchTimer.Start()
        Else
            IsDropDownOpen = False
        End If
    End Sub

    Private Sub OnFetchTimerTick(ByVal sender As Object, ByVal e As EventArgs)
        FetchTimer.IsEnabled = False
        FetchTimer.Stop()
        If Provider IsNot Nothing AndAlso ItemsSelector IsNot Nothing Then
            Filter = Editor.Text
            ItemsSelector.ItemsSource = Provider.GetSuggestions(Editor.Text)
            ItemsSelector.SelectedIndex = -1
            If ItemsSelector.HasItems AndAlso IsKeyboardFocusWithin Then
                IsDropDownOpen = True
            Else
                IsDropDownOpen = False
            End If
        End If
    End Sub

    Private Sub OnPopupClosed(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Private Sub OnPopupOpened(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Private Sub OnSelectionAdapterCancel()
        IsDropDownOpen = False
    End Sub

    Private Sub OnSelectionAdapterCommit()
        SelectedItem = ItemsSelector.SelectedItem
        _isUpdatingText = True
        Editor.Text = GetDisplayText(ItemsSelector.SelectedItem)
        _isUpdatingText = False
        IsDropDownOpen = False
    End Sub

    Private Sub OnSelectionAdapterSelectionChanged()
        _isUpdatingText = True
        If ItemsSelector.SelectedItem Is Nothing Then
            Editor.Text = Filter
        Else
            Editor.Text = GetDisplayText(ItemsSelector.SelectedItem)
        End If
        Editor.SelectionStart = Editor.Text.Length
        Editor.SelectionLength = 0
        _isUpdatingText = False
    End Sub

#End Region 'Methods

End Class