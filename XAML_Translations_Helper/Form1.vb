Public Class frmTranslations
    Public Shared Property TranslationsPath As String = "C:\Users\Aaron Campf\Documents\GitHub\BetterExplorer\BExplorer\BetterExplorer\Translation"


    Private Function GetTranslations(FileName As String) As Dictionary(Of String, String)
        Dim Results As New Dictionary(Of String, String)
        For Each Node In XElement.Load(TranslationsPath & "\" & FileName).Elements("{clr-namespace:System;assembly=mscorlib}String")
            Results.Add(Node.Attributes.First, Node.Value)
        Next

        Return Results
    End Function


    Private Sub frmTranslations_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each Item In GetTranslations("DefaultLocale.xaml")
            Dim ListItem As New ListViewItem(Item.Key)
            ListItem.SubItems.Add(Item.Value)
            lbxDefaultTranslations.Items.Add(ListItem)
        Next

        Key.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
        Value.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)

        Dim Files = Aggregate x In New IO.DirectoryInfo(TranslationsPath).GetFiles()
                        Where x.Extension.ToUpper.EndsWith(".XAML") And Not x.Name = "DefaultLocale.xaml"
                       Select x.Name Into ToArray


        cbxSelectedLanguage.Items.AddRange(Files)
    End Sub

    Private Sub cbxSelectedLanguage_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbxSelectedLanguage.SelectedIndexChanged
        lbxSecondTranslations.Items.Clear()
        lbxMissing.Items.Clear()

        For Each Item In GetTranslations(cbxSelectedLanguage.Text)
            Dim ListItem As New ListViewItem(Item.Key)
            ListItem.SubItems.Add(Item.Value)
            lbxSecondTranslations.Items.Add(ListItem)
        Next


        Dim Current_Translation = GetTranslations(cbxSelectedLanguage.Text)
        For Each Item In GetTranslations("DefaultLocale.xaml")
            If Not Current_Translation.ContainsKey(Item.Key) Then
                lbxMissing.Items.Add(Item.Key)
            End If
        Next
    End Sub
End Class
