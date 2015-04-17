Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BExplorer.Shell

<TestClass()>
Public Class UnitTest1

	<TestMethod()>
	Public Sub TestMethod1()
		Dim ShellItem As New ShellItem("C:\temp\New Folder (16)\New folder (17)")
		MsgBox(ShellItem.IsFileSystem)
		MsgBox(ShellItem.IsLink)


		MsgBox("Hello")
	End Sub

End Class