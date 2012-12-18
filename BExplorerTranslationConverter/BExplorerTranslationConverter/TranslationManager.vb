Public Class TranslationManager

    Public Class TranslationData
        Public name As String
        Public value As String
    End Class

    Public Shared Function ReadXMLFile(ByVal file As String) As List(Of TranslationData)
        Dim fdata As String = My.Computer.FileSystem.ReadAllText(file)
        Dim items As List(Of String) = Split(fdata, vbCrLf).ToList
        MessageBox.Show("Successfully loaded " + items.Count.ToString + " lines.")
        Dim trans As New List(Of TranslationData)
        For Each item As String In items
            If item.StartsWith("    <system:String") Then
                Try
                    Dim entry As New TranslationData
                    entry.name = item.Substring(item.IndexOf("""") + 1, item.LastIndexOf("""") - (item.IndexOf("""") + 1))
                    entry.value = item.Substring(item.IndexOf(">") + 1, item.LastIndexOf("</") - (item.IndexOf(">") + 1))
                    trans.Add(entry)
                Catch ex As Exception
                    MessageBox.Show("An error occurred while trying to parse this line:" + vbCrLf + vbCrLf + item, "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        Next
        Return trans
    End Function

    Public Shared Function ReadDefaultXMLFile(ByVal file As String) As List(Of TranslationEntry)
        Dim fdata As String = My.Computer.FileSystem.ReadAllText(file)
        Dim items As List(Of String) = Split(fdata, vbCrLf).ToList
        MessageBox.Show("Successfully loaded " + items.Count.ToString + " lines.")
        Dim trans As New List(Of TranslationEntry)
        For Each item As String In items
            If item.StartsWith("    <system:String") Then
                Try
                    Dim entry As New TranslationEntry
                    entry.Name = item.Substring(item.IndexOf("""") + 1, item.LastIndexOf("""") - (item.IndexOf("""") + 1))
                    entry.DefaultValue = item.Substring(item.IndexOf(">") + 1, item.LastIndexOf("</") - (item.IndexOf(">") + 1))
                    entry.Definition = ""
                    entry.Translation = ""
                    trans.Add(entry)
                Catch ex As Exception
                    MessageBox.Show("An error occurred while trying to parse this line:" + vbCrLf + vbCrLf + item, "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        Next
        Return trans
    End Function

    Public Shared Sub AddDataToEntries(ByVal translations As List(Of TranslationEntry), ByVal data As List(Of TranslationData))
        For Each item As TranslationData In data
            Dim found As Boolean = False
            For Each thing As TranslationEntry In translations
                If found = False Then
                    If thing.Name = item.name Then
                        thing.Translation = item.value
                        found = True
                    End If
                End If
            Next

            If found = False Then
                translations.Add(New TranslationEntry(item.name, "", item.value, ""))
            End If
        Next
    End Sub

    Public Shared Function ReadBaseFile(ByVal file As String) As List(Of TranslationEntry)
        Dim fdata As String = My.Computer.FileSystem.ReadAllText(file)
        Dim items As List(Of String) = Split(fdata, vbCrLf).ToList
        MessageBox.Show(items.Count)
        Dim trans As New List(Of TranslationEntry)
        For Each item As String In items
            If item.StartsWith("+") = False Then
                trans.Add(New TranslationEntry(item))
            End If
        Next
        Return trans
    End Function

    Public Shared Sub WriteBaseFile(ByVal file As String, ByVal translations As List(Of TranslationEntry))
        Dim o As String = ""
        Dim first As Boolean = True
        For Each item As TranslationEntry In translations
            If first = True Then
                o += item.ToString()
                first = False
            Else
                o += vbCrLf + item.ToString()
            End If
        Next
        My.Computer.FileSystem.WriteAllText(file, o, False)
    End Sub

    Public Shared Function CreateXMLFile(ByVal translations As List(Of TranslationEntry)) As String
        Dim o As String = "<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""" + vbCrLf +
                          "                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""" + vbCrLf +
                          "                    xmlns:system=""clr-namespace:System;assembly=mscorlib"">" + vbCrLf +
                          vbCrLf +
                          "   <!-- String resource that can be localized -->" + vbCrLf

        For Each item As TranslationEntry In translations
            o += "    " + item.ToXMLString() + vbCrLf
        Next

        o += vbCrLf + "</ResourceDictionary>"

        Return o
    End Function

    Public Shared Sub WriteXMLtoFile(ByVal translations As List(Of TranslationEntry), ByVal file As String)
        My.Computer.FileSystem.WriteAllText(file, CreateXMLFile(translations), False)
    End Sub

    Public Shared Function CreateDefaultXMLFile(ByVal translations As List(Of TranslationEntry)) As String
        Dim o As String = "<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""" + vbCrLf +
                          "                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""" + vbCrLf +
                          "                    xmlns:system=""clr-namespace:System;assembly=mscorlib"">" + vbCrLf +
                          vbCrLf +
                          "   <!-- String resource that can be localized -->" + vbCrLf

        For Each item As TranslationEntry In translations
            o += "    " + item.ToXMLString(True) + vbCrLf
        Next

        o += vbCrLf + "</ResourceDictionary>"

        Return o
    End Function

    Public Shared Sub WriteDefaultXMLtoFile(ByVal translations As List(Of TranslationEntry), ByVal file As String)
        My.Computer.FileSystem.WriteAllText(file, CreateDefaultXMLFile(translations), False)
    End Sub

End Class
