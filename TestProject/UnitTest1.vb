Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BExplorer.Shell

<TestClass()>
Public Class UnitTest1

	<TestMethod()>
	Public Sub TestMethod1()
		Dim ShellItem As New ShellItem("C:\temp\New Folder (16)")
		Dim Parent = ShellItem.Parent


		MsgBox("Hello")
	End Sub

End Class