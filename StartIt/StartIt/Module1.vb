Imports System.Windows.Forms
Module Module1

    Sub Main()
    Console.WriteLine("Waiting 2 seconds")
        Console.WriteLine("2")
        Wait.Wait(1)
        Console.WriteLine("1")
        Wait.Wait(1)
        Dim a As System.Collections.ObjectModel.Collection(Of String) = Nothing
        Dim o As String = ""
        For i As Integer = 1 To My.Application.CommandLineArgs.Count - 1
            a.Add(My.Application.CommandLineArgs.Item(i))
        Next
        Dim e As Integer = 0
        Try
            For Each item In a
                o += a.Item(e)
                e += 1

            Next
        Catch ex As Exception
            Console.WriteLine("No command line arguments found. Continuing.")
        End Try
        Try
            Console.WriteLine("Starting file " + My.Application.CommandLineArgs.Item(0))
            Console.WriteLine("CL Arguments: " + o)
            System.Diagnostics.Process.Start(My.Application.CommandLineArgs.Item(0), o)
        Catch ex As Exception
            Console.WriteLine("file not found. Exiting.")
        End Try
    End Sub

End Module

Public Class Wait

    ''' <summary>
    ''' Instructs the program to wait a specified amount before continuing.
    ''' </summary>
    ''' <param name="Seconds">Number of seconds to wait.</param>
    ''' <remarks></remarks>
    Public Shared Sub Wait(ByVal Seconds As Double)

        'Can use a timer control instead
        'of this function to implement intervals
        'between flashses

        Dim dStart As Double
        Dim bPastMidnight As Boolean
        Dim dTimeToQuit As Double


        dStart = Microsoft.VisualBasic.DateAndTime.Timer
        'Deal with timeout being reset at Midnight
        If Seconds > 0 Then
            If dStart + Seconds < 86400 Then
                dTimeToQuit = dStart + Seconds
            Else
                dTimeToQuit = (dStart - 86400) + Seconds
                bPastMidnight = True
            End If
        End If

        Do Until Microsoft.VisualBasic.DateAndTime.Timer >= dTimeToQuit
            Application.DoEvents()
        Loop

    End Sub

End Class
