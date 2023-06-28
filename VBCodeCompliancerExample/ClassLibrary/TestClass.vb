Public Class TestClass

    Private Const Dont_Touch_Th1s_Var1able As String = "Dont_Touch_Th1s_Var1able"

    Public Sub New()

    End Sub

    Public Function _Sum_2_Values(ByVal value1 As Integer, ByVal value2 As Integer)

        Dim result1231_to_return As Integer

        Su_m(value1, value2, result1231_to_return)

        Return result1231_to_return

    End Function

    Private Sub Su_m(ByVal _v1 As Integer, ByVal _v_2 As Integer, ByRef _result As Integer)
        _result = _v1 + _v_2
    End Sub

End Class
