Public Class TranslationEntry

    Private eng As String
    Private tra As String
    Private tit As String
    Private exp As String

    Public Sub New(ByVal name As String, ByVal defname As String, ByVal translation As String, ByVal definition As String)
        eng = defname
        tra = translation
        tit = name
        exp = definition
    End Sub

    Public Sub New(ByVal value As String)
        Dim item As String = value
        tra = item.Substring(1, item.IndexOf(";") - 2)
        'tra = item.Substring(item.IndexOf("{") + 1, item.LastIndexOf("}") - item.IndexOf("{") - 1)
        eng = item.Substring(item.IndexOf(";") + 3, item.LastIndexOf(";") - item.IndexOf(";") - 4)
        'tra = item.Substring(item.IndexOf("{") + 1, item.LastIndexOf("}") - item.IndexOf("{") - 1)
        tit = item.Substring(item.IndexOf("<") + 1, item.LastIndexOf(">") - item.IndexOf("<") - 1)
        exp = item.Substring(item.IndexOf("[") + 1, item.LastIndexOf("]") - item.IndexOf("[") - 1)
    End Sub

    Public Sub New()

    End Sub

    Public Property DefaultValue As String
        Get
            Return eng
        End Get
        Set(value As String)
            eng = value
        End Set
    End Property

    Public Property Translation As String
        Get
            Return tra
        End Get
        Set(value As String)
            tra = value
        End Set
    End Property

    Public Property Name As String
        Get
            Return tit
        End Get
        Set(value As String)
            tit = value
        End Set
    End Property

    Public Property Definition As String
        Get
            Return exp
        End Get
        Set(value As String)
            exp = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return """" + tra + """; """ + eng + """;(Location:[" + exp + "] Name:<" + tit + ">)"
    End Function

    Public Function ToXMLString() As String
        Return "<system:String x:Key=""" + tit + """>" + tra + "</system:String>"
    End Function

    Public Function ToXMLString(ByVal usedefault As Boolean) As String
        If usedefault = False Then
            Return "<system:String x:Key=""" + tit + """>" + tra + "</system:String>"
        Else
            Return "<system:String x:Key=""" + tit + """>" + eng + "</system:String>"
        End If
    End Function

End Class
