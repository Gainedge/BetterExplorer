Public Class IDSearchDialog

    Public Confirm As Boolean = False

    Private Sub TextBox1_PreviewKeyUp(sender As System.Object, e As System.Windows.Input.KeyEventArgs) Handles TextBox1.PreviewKeyDown
        If e.Key = Key.Enter Then
            Button1_Click(sender, New RoutedEventArgs(e.RoutedEvent))
        End If
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button1.Click
        Confirm = True
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button2.Click
        Confirm = False
        Me.Close()
    End Sub

    Private Sub Window_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        TextBox1.Focus()
    End Sub
End Class
