Public Class ItemEditor

    Public yep As Boolean = False

    Public OriginalItem As String = ""

    Public Sub LoadTranslation(ByVal entry As TranslationEntry)
        TextBox1.Text = entry.DefaultValue
        TextBox2.Text = entry.Translation
        TextBox3.Text = entry.Definition
        TextBox4.Text = entry.Name
        OriginalItem = entry.ToString()
    End Sub

    Public Function GetTranslation() As TranslationEntry
        Dim o As New TranslationEntry
        o.DefaultValue = TextBox1.Text
        o.Translation = TextBox2.Text
        o.Definition = TextBox3.Text
        o.Name = TextBox4.Text
        Return o
    End Function

    Private Sub Button2_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button2.Click
        yep = True
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button1.Click
        yep = False
        Me.Close()
    End Sub

    Private Sub TextBox1_PreviewKeyUp(sender As System.Object, e As System.Windows.Input.KeyEventArgs) Handles TextBox1.PreviewKeyDown, TextBox2.PreviewKeyDown, TextBox3.PreviewKeyDown, TextBox4.PreviewKeyDown
        If e.Key = Key.Enter Then
            e.Handled = True
            yep = True
            Me.Close()
        End If
    End Sub

    Private Sub Window_Loaded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        TextBox1.Focus()
    End Sub
End Class
