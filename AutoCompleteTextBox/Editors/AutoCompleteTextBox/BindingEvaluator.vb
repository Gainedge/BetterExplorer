Public Class BindingEvaluator
    Inherits FrameworkElement

    #Region "Fields"

    Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register("Value",                            GetType(String), GetType(BindingEvaluator),                            New FrameworkPropertyMetadata(String.Empty))

    Private _valueBinding As Binding

    #End Region 'Fields

    #Region "Constructors"

    Public Sub New(ByVal binding As Binding)
        ValueBinding = binding
        SetBinding(ValueProperty, binding)
    End Sub

    #End Region 'Constructors

    #Region "Properties"

    Public Property Value() As String
        Get
            Return GetValue(ValueProperty)
        End Get

        Set(ByVal value As String)
            SetValue(ValueProperty, value)
        End Set
    End Property

    Public Property ValueBinding() As Binding
        Get
            Return _valueBinding
        End Get
        Set(ByVal value As Binding)
            _valueBinding = value
        End Set
    End Property

    #End Region 'Properties

    #Region "Methods"

    Public Function Evaluate(ByVal dataItem As Object) As String
        Me.DataContext = dataItem
        Return Value
    End Function

    #End Region 'Methods

End Class