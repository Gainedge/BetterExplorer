Public Class SuggestionProvider
    Implements ISuggestionProvider

    Private _method As Func(Of String, IEnumerable)

    Public Sub New(ByVal method As Func(Of String, IEnumerable))
        If method Is Nothing Then
            Throw New ArgumentNullException("method")
        End If
        _method = method
    End Sub

    Public Function GetSuggestions(ByVal filter As String) As System.Collections.IEnumerable Implements ISuggestionProvider.GetSuggestions
        Return _method(filter)
    End Function

End Class
