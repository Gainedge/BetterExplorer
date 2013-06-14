Imports System.Windows.Controls.Primitives

Public Class SelectionAdapter

    #Region "Fields"

    Private _selectorControl As Selector

    #End Region 'Fields

    #Region "Constructors"

    Public Sub New(ByVal selector As Selector)
        SelectorControl = selector
        AddHandler SelectorControl.PreviewMouseUp, AddressOf OnSelectorMouseDown
    End Sub

    #End Region 'Constructors

    #Region "Events"

    Public Event Cancel()

    Public Event Commit()

    Public Event SelectionChanged()

    #End Region 'Events

    #Region "Properties"

    Public Property SelectorControl() As Selector
        Get
            Return _selectorControl
        End Get
        Set(ByVal value As Selector)
            _selectorControl = value
        End Set
    End Property

    #End Region 'Properties

    #Region "Methods"

    Public Sub HandleKeyDown(ByVal key As KeyEventArgs)
        Debug.WriteLine(key.Key)
        Select Case key.Key
            Case Input.Key.Down
                IncrementSelection()
            Case Input.Key.Up
                DecrementSelection()
            Case Input.Key.Enter
                RaiseEvent Commit()
            Case Input.Key.Escape
                RaiseEvent Cancel()
        End Select
    End Sub

    Private Sub DecrementSelection()
        If SelectorControl.SelectedIndex = -1 Then
            SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1
        Else
            SelectorControl.SelectedIndex -= 1
        End If
        RaiseEvent SelectionChanged()
    End Sub

    Private Sub IncrementSelection()
        If SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1 Then
            SelectorControl.SelectedIndex = -1
        Else
            SelectorControl.SelectedIndex += 1
        End If
        RaiseEvent SelectionChanged()
    End Sub

    Private Sub OnSelectorMouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        RaiseEvent Commit()
    End Sub

    #End Region 'Methods

End Class