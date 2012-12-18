Imports System.Windows.Forms

Class MainWindow

    Private translations As New List(Of TranslationEntry)

    Private Sub Button4_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button4.Click
        Dim o As New OpenFileDialog
        o.Filter = "Text File|*.txt|All Files|*.*"
        o.Title = "Open Text File"
        If o.ShowDialog = Forms.DialogResult.OK Then
            translations = TranslationManager.ReadBaseFile(o.FileName)
        End If
        LoadTranslationsIntoListBox()
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button5.Click
        Dim o As New OpenFileDialog
        o.Filter = "XML File|*.xml;*.xaml|All Files|*.*"
        o.Title = "Open XML File"
        If o.ShowDialog = Forms.DialogResult.OK Then
            TranslationManager.AddDataToEntries(translations, TranslationManager.ReadXMLFile(o.FileName))
        End If
        LoadTranslationsIntoListBox()
    End Sub

    Private Sub Button6_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button6.Click
        Dim o As New SaveFileDialog
        o.Filter = "Text File|*.txt|All Files|*.*"
        o.Title = "Save Data as Text File"
        If o.ShowDialog = Forms.DialogResult.OK Then
            TranslationManager.WriteBaseFile(o.FileName, translations)
        End If
    End Sub

    Private Sub Button7_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button7.Click
        Dim o As New SaveFileDialog
        o.Filter = "XAML File|*.xaml|XML File|*.xml|All Files|*.*"
        o.Title = "Save Data as XML File"
        If o.ShowDialog = Forms.DialogResult.OK Then
            TranslationManager.WriteXMLtoFile(translations, o.FileName)
        End If
    End Sub

    Private Sub Button8_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button8.Click
        Dim o As New OpenFileDialog
        o.Filter = "XML File|*.xml;*.xaml|All Files|*.*"
        o.Title = "Load Default XML File"
        If o.ShowDialog = Forms.DialogResult.OK Then
            translations = TranslationManager.ReadDefaultXMLFile(o.FileName)
        End If
        LoadTranslationsIntoListBox()
    End Sub

    Private Sub LoadTranslationsIntoListBox()
        Me.ListBox1.Items.Clear()
        For Each item As TranslationEntry In translations
            Me.ListBox1.Items.Add(item.ToString)
        Next
    End Sub

    Private Sub Button9_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button9.Click
        LoadTranslationsIntoListBox()
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button1.Click
        Try
            Dim o As New ItemEditor
            o.LoadTranslation(New TranslationEntry(ListBox1.SelectedValue))
            o.ShowDialog()
            If o.yep = True Then
                For Each item As TranslationEntry In translations
                    If item.ToString = o.OriginalItem Then
                        Dim g As TranslationEntry = o.GetTranslation
                        item.DefaultValue = g.DefaultValue
                        item.Definition = g.Definition
                        item.Name = g.Name
                        item.Translation = g.Translation
                    End If
                Next
            End If
            LoadTranslationsIntoListBox()
        Catch ex As Exception
            MessageBox.Show("Cannot edit." + vbCrLf + vbCrLf + ex.ToString())
        End Try
    End Sub

    Private Sub ListBox1_MouseDoubleClick(sender As System.Object, e As System.Windows.Input.MouseButtonEventArgs) Handles ListBox1.MouseDoubleClick
        Button1_Click(sender, New RoutedEventArgs(e.RoutedEvent))
    End Sub

    Private Sub Button3_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button3.Click
        Dim o As Integer = ListBox1.SelectedIndex
        Dim g As New List(Of TranslationEntry)
        Try
            For Each item As TranslationEntry In translations
                If ListBox1.SelectedItems.Contains(item.ToString) Then
                    g.Add(item)
                End If
            Next

            For Each item As TranslationEntry In g
                translations.Remove(item)
            Next
            'LoadTranslationsIntoListBox()
        Catch ex As Exception
            MessageBox.Show("Cannot delete." + vbCrLf + vbCrLf + ex.ToString())
        End Try
        ListBox1.Focus()
        ListBox1.SelectedIndex = o
        LoadTranslationsIntoListBox()
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button2.Click
        Dim o As New ItemEditor
        o.ShowDialog()
        If o.yep = True Then
            translations.Add(o.GetTranslation)
        End If
        LoadTranslationsIntoListBox()
        For Each item As String In ListBox1.Items
            If item = o.GetTranslation.ToString() Then
                ListBox1.SelectedItem = item
                ListBox1.ScrollIntoView(item)
            End If
        Next
    End Sub

    Private Sub Button10_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button10.Click
        Dim o As New SaveFileDialog
        o.Filter = "XAML File|*.xaml|XML File|*.xml|All Files|*.*"
        o.Title = "Save Data as XML File"
        If o.ShowDialog = Forms.DialogResult.OK Then
            TranslationManager.WriteDefaultXMLtoFile(translations, o.FileName)
        End If
    End Sub

    Private Sub Button11_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Button11.Click
        Dim o As New IDSearchDialog
        o.ShowDialog()
        If o.Confirm = True Then
            Dim val As String = ""
            For Each item As TranslationEntry In translations
                If item.Name = o.TextBox1.Text Then
                    val = item.ToString()
                End If
            Next
            For Each item As String In ListBox1.Items
                If item = val Then
                    ListBox1.Focus()
                    ListBox1.SelectedItem = item
                    ListBox1.ScrollIntoView(item)
                End If
            Next
        End If
    End Sub

    Private Sub ListBox1_PreviewKeyUp(sender As System.Object, e As System.Windows.Input.KeyEventArgs) Handles ListBox1.PreviewKeyDown
        If e.Key = Key.Enter Then
            Button1_Click(sender, New RoutedEventArgs(e.RoutedEvent))
        ElseIf e.Key = Key.Delete Then
            Button3_Click(sender, New RoutedEventArgs(e.RoutedEvent))
        ElseIf e.Key = Key.F5 Then
            LoadTranslationsIntoListBox()
        End If
    End Sub

    Private Sub CheckBox1_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CheckBox1.Click
        If CheckBox1.IsChecked = True Then
            ListBox1.SelectionMode = Controls.SelectionMode.Multiple
            Button1.IsEnabled = False
        Else
            ListBox1.SelectionMode = Controls.SelectionMode.Single
            Button1.IsEnabled = True
        End If
    End Sub
End Class
