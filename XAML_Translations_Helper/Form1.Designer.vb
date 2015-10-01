<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTranslations
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lbxDefaultTranslations = New System.Windows.Forms.ListView()
        Me.Key = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Value = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lbxSecondTranslations = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.cbxSelectedLanguage = New System.Windows.Forms.ComboBox()
        Me.lbxMissing = New System.Windows.Forms.ListBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lbxDefaultTranslations
        '
        Me.lbxDefaultTranslations.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Key, Me.Value})
        Me.lbxDefaultTranslations.Location = New System.Drawing.Point(12, 25)
        Me.lbxDefaultTranslations.Name = "lbxDefaultTranslations"
        Me.lbxDefaultTranslations.Size = New System.Drawing.Size(783, 251)
        Me.lbxDefaultTranslations.TabIndex = 2
        Me.lbxDefaultTranslations.UseCompatibleStateImageBehavior = False
        Me.lbxDefaultTranslations.View = System.Windows.Forms.View.Details
        '
        'Key
        '
        Me.Key.Text = "Key"
        Me.Key.Width = 206
        '
        'Value
        '
        Me.Value.Text = "Value"
        Me.Value.Width = 233
        '
        'lbxSecondTranslations
        '
        Me.lbxSecondTranslations.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.lbxSecondTranslations.Location = New System.Drawing.Point(12, 308)
        Me.lbxSecondTranslations.Name = "lbxSecondTranslations"
        Me.lbxSecondTranslations.Size = New System.Drawing.Size(783, 251)
        Me.lbxSecondTranslations.TabIndex = 3
        Me.lbxSecondTranslations.UseCompatibleStateImageBehavior = False
        Me.lbxSecondTranslations.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Key"
        Me.ColumnHeader1.Width = 206
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Value"
        Me.ColumnHeader2.Width = 233
        '
        'cbxSelectedLanguage
        '
        Me.cbxSelectedLanguage.FormattingEnabled = True
        Me.cbxSelectedLanguage.Location = New System.Drawing.Point(801, 25)
        Me.cbxSelectedLanguage.Name = "cbxSelectedLanguage"
        Me.cbxSelectedLanguage.Size = New System.Drawing.Size(202, 21)
        Me.cbxSelectedLanguage.TabIndex = 4
        '
        'lbxMissing
        '
        Me.lbxMissing.FormattingEnabled = True
        Me.lbxMissing.Location = New System.Drawing.Point(801, 308)
        Me.lbxMissing.Name = "lbxMissing"
        Me.lbxMissing.Size = New System.Drawing.Size(212, 251)
        Me.lbxMissing.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(78, 13)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "All Translations"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 292)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(165, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Selected Languages Translations"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(798, 9)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(109, 13)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "Select File/Language"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(798, 292)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(102, 13)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "Missing Translations"
        '
        'frmTranslations
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1053, 642)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lbxMissing)
        Me.Controls.Add(Me.cbxSelectedLanguage)
        Me.Controls.Add(Me.lbxSecondTranslations)
        Me.Controls.Add(Me.lbxDefaultTranslations)
        Me.Name = "frmTranslations"
        Me.Text = "Translations"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lbxDefaultTranslations As ListView
    Friend WithEvents Key As ColumnHeader
    Friend WithEvents Value As ColumnHeader
    Friend WithEvents lbxSecondTranslations As ListView
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents cbxSelectedLanguage As ComboBox
    Friend WithEvents lbxMissing As ListBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
End Class
