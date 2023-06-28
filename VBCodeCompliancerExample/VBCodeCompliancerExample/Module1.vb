Imports ClassLibrary

Module Module1

    Sub Main()

        Dim t_c = New TestClass
        Dim _v1, _v2 As Integer

        Console.Write("_v1: ")
        _v1 = Console.ReadLine()

        Console.Write("_v2: ")
        _v2 = Console.ReadLine()



        Console.WriteLine($"{_v1} + {_v2} = { Convert.ToString(t_c._Sum_2_Values(4, 3)) }")
        Console.ReadKey()
    End Sub

End Module
