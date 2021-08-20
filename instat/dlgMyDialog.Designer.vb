<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class dlgMyDialog
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
        Me.lblXVariable = New System.Windows.Forms.Label()
        Me.lblYVariable = New System.Windows.Forms.Label()
        Me.ucrChooseGraph = New System.Windows.Forms.ComboBox()
        Me.lblGraphs = New System.Windows.Forms.Label()
        Me.ucrCheckKeys = New System.Windows.Forms.CheckBox()
        Me.ucrCheckStatisticalSummary = New System.Windows.Forms.CheckBox()
        Me.ucrPlotGraph = New System.Windows.Forms.Button()
        Me.ucrNumNumber = New System.Windows.Forms.NumericUpDown()
        Me.lblNumber = New System.Windows.Forms.Label()
        Me.ucrColourInput = New System.Windows.Forms.ComboBox()
        Me.lblColour = New System.Windows.Forms.Label()
        Me.grpChooseParametersToAdd = New System.Windows.Forms.GroupBox()
        Me.ucrSaveGraph = New instat.ucrSaveGraph()
        Me.ucrReceiverY = New instat.ucrReceiverSingle()
        Me.ucrReceiverX = New instat.ucrReceiverSingle()
        Me.ucrSelectorMyDialog = New instat.ucrSelectorByDataFrameAddRemove()
        Me.ucrBase = New instat.ucrButtons()
        CType(Me.ucrNumNumber, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpChooseParametersToAdd.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblXVariable
        '
        Me.lblXVariable.AutoSize = True
        Me.lblXVariable.Location = New System.Drawing.Point(240, 63)
        Me.lblXVariable.Name = "lblXVariable"
        Me.lblXVariable.Size = New System.Drawing.Size(54, 13)
        Me.lblXVariable.TabIndex = 4
        Me.lblXVariable.Text = "X variable"
        '
        'lblYVariable
        '
        Me.lblYVariable.AutoSize = True
        Me.lblYVariable.Location = New System.Drawing.Point(240, 115)
        Me.lblYVariable.Name = "lblYVariable"
        Me.lblYVariable.Size = New System.Drawing.Size(54, 13)
        Me.lblYVariable.TabIndex = 5
        Me.lblYVariable.Text = "Y variable"
        '
        'ucrChooseGraph
        '
        Me.ucrChooseGraph.FormattingEnabled = True
        Me.ucrChooseGraph.Location = New System.Drawing.Point(9, 244)
        Me.ucrChooseGraph.Name = "ucrChooseGraph"
        Me.ucrChooseGraph.Size = New System.Drawing.Size(121, 21)
        Me.ucrChooseGraph.TabIndex = 6
        '
        'lblGraphs
        '
        Me.lblGraphs.AutoSize = True
        Me.lblGraphs.Location = New System.Drawing.Point(9, 228)
        Me.lblGraphs.Name = "lblGraphs"
        Me.lblGraphs.Size = New System.Drawing.Size(41, 13)
        Me.lblGraphs.TabIndex = 7
        Me.lblGraphs.Text = "Graphs"
        '
        'ucrCheckKeys
        '
        Me.ucrCheckKeys.AutoSize = True
        Me.ucrCheckKeys.Location = New System.Drawing.Point(0, 19)
        Me.ucrCheckKeys.Name = "ucrCheckKeys"
        Me.ucrCheckKeys.Size = New System.Drawing.Size(49, 17)
        Me.ucrCheckKeys.TabIndex = 8
        Me.ucrCheckKeys.Text = "Keys"
        Me.ucrCheckKeys.UseVisualStyleBackColor = True
        '
        'ucrCheckStatisticalSummary
        '
        Me.ucrCheckStatisticalSummary.AutoSize = True
        Me.ucrCheckStatisticalSummary.Location = New System.Drawing.Point(0, 42)
        Me.ucrCheckStatisticalSummary.Name = "ucrCheckStatisticalSummary"
        Me.ucrCheckStatisticalSummary.Size = New System.Drawing.Size(69, 17)
        Me.ucrCheckStatisticalSummary.TabIndex = 9
        Me.ucrCheckStatisticalSummary.Text = "Summary"
        Me.ucrCheckStatisticalSummary.UseVisualStyleBackColor = True
        '
        'ucrPlotGraph
        '
        Me.ucrPlotGraph.Location = New System.Drawing.Point(144, 370)
        Me.ucrPlotGraph.Name = "ucrPlotGraph"
        Me.ucrPlotGraph.Size = New System.Drawing.Size(75, 23)
        Me.ucrPlotGraph.TabIndex = 10
        Me.ucrPlotGraph.Text = "Plot"
        Me.ucrPlotGraph.UseVisualStyleBackColor = True
        '
        'ucrNumNumber
        '
        Me.ucrNumNumber.Location = New System.Drawing.Point(369, 143)
        Me.ucrNumNumber.Name = "ucrNumNumber"
        Me.ucrNumNumber.Size = New System.Drawing.Size(46, 20)
        Me.ucrNumNumber.TabIndex = 11
        '
        'lblNumber
        '
        Me.lblNumber.AutoSize = True
        Me.lblNumber.Location = New System.Drawing.Point(366, 128)
        Me.lblNumber.Name = "lblNumber"
        Me.lblNumber.Size = New System.Drawing.Size(44, 13)
        Me.lblNumber.TabIndex = 12
        Me.lblNumber.Text = "Number"
        '
        'ucrColourInput
        '
        Me.ucrColourInput.FormattingEnabled = True
        Me.ucrColourInput.Location = New System.Drawing.Point(260, 247)
        Me.ucrColourInput.Name = "ucrColourInput"
        Me.ucrColourInput.Size = New System.Drawing.Size(75, 21)
        Me.ucrColourInput.TabIndex = 14
        '
        'lblColour
        '
        Me.lblColour.AutoSize = True
        Me.lblColour.Location = New System.Drawing.Point(257, 231)
        Me.lblColour.Name = "lblColour"
        Me.lblColour.Size = New System.Drawing.Size(37, 13)
        Me.lblColour.TabIndex = 15
        Me.lblColour.Text = "Colour"
        '
        'grpChooseParametersToAdd
        '
        Me.grpChooseParametersToAdd.Controls.Add(Me.ucrCheckKeys)
        Me.grpChooseParametersToAdd.Controls.Add(Me.ucrCheckStatisticalSummary)
        Me.grpChooseParametersToAdd.Location = New System.Drawing.Point(144, 244)
        Me.grpChooseParametersToAdd.Name = "grpChooseParametersToAdd"
        Me.grpChooseParametersToAdd.Size = New System.Drawing.Size(98, 82)
        Me.grpChooseParametersToAdd.TabIndex = 17
        Me.grpChooseParametersToAdd.TabStop = False
        Me.grpChooseParametersToAdd.Text = "Choose Box"
        '
        'ucrSaveGraph
        '
        Me.ucrSaveGraph.Location = New System.Drawing.Point(15, 361)
        Me.ucrSaveGraph.Name = "ucrSaveGraph"
        Me.ucrSaveGraph.Size = New System.Drawing.Size(115, 32)
        Me.ucrSaveGraph.TabIndex = 13
        '
        'ucrReceiverY
        '
        Me.ucrReceiverY.frmParent = Me
        Me.ucrReceiverY.Location = New System.Drawing.Point(243, 128)
        Me.ucrReceiverY.Margin = New System.Windows.Forms.Padding(0)
        Me.ucrReceiverY.Name = "ucrReceiverY"
        Me.ucrReceiverY.Selector = Nothing
        Me.ucrReceiverY.Size = New System.Drawing.Size(75, 20)
        Me.ucrReceiverY.strNcFilePath = ""
        Me.ucrReceiverY.TabIndex = 3
        Me.ucrReceiverY.ucrSelector = Nothing
        '
        'ucrReceiverX
        '
        Me.ucrReceiverX.frmParent = Me
        Me.ucrReceiverX.Location = New System.Drawing.Point(243, 76)
        Me.ucrReceiverX.Margin = New System.Windows.Forms.Padding(0)
        Me.ucrReceiverX.Name = "ucrReceiverX"
        Me.ucrReceiverX.Selector = Nothing
        Me.ucrReceiverX.Size = New System.Drawing.Size(75, 20)
        Me.ucrReceiverX.strNcFilePath = ""
        Me.ucrReceiverX.TabIndex = 2
        Me.ucrReceiverX.ucrSelector = Nothing
        '
        'ucrSelectorMyDialog
        '
        Me.ucrSelectorMyDialog.bDropUnusedFilterLevels = False
        Me.ucrSelectorMyDialog.bShowHiddenColumns = False
        Me.ucrSelectorMyDialog.bUseCurrentFilter = True
        Me.ucrSelectorMyDialog.Location = New System.Drawing.Point(9, 9)
        Me.ucrSelectorMyDialog.Margin = New System.Windows.Forms.Padding(0)
        Me.ucrSelectorMyDialog.Name = "ucrSelectorMyDialog"
        Me.ucrSelectorMyDialog.Size = New System.Drawing.Size(210, 180)
        Me.ucrSelectorMyDialog.TabIndex = 1
        '
        'ucrBase
        '
        Me.ucrBase.Location = New System.Drawing.Point(12, 399)
        Me.ucrBase.Name = "ucrBase"
        Me.ucrBase.Size = New System.Drawing.Size(410, 52)
        Me.ucrBase.TabIndex = 0
        '
        'dlgMyDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(427, 450)
        Me.Controls.Add(Me.lblColour)
        Me.Controls.Add(Me.ucrColourInput)
        Me.Controls.Add(Me.ucrSaveGraph)
        Me.Controls.Add(Me.lblNumber)
        Me.Controls.Add(Me.ucrNumNumber)
        Me.Controls.Add(Me.ucrPlotGraph)
        Me.Controls.Add(Me.lblGraphs)
        Me.Controls.Add(Me.ucrChooseGraph)
        Me.Controls.Add(Me.lblYVariable)
        Me.Controls.Add(Me.lblXVariable)
        Me.Controls.Add(Me.ucrReceiverY)
        Me.Controls.Add(Me.ucrReceiverX)
        Me.Controls.Add(Me.ucrSelectorMyDialog)
        Me.Controls.Add(Me.ucrBase)
        Me.Controls.Add(Me.grpChooseParametersToAdd)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgMyDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "My Dialog"
        CType(Me.ucrNumNumber, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpChooseParametersToAdd.ResumeLayout(False)
        Me.grpChooseParametersToAdd.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ucrBase As ucrButtons
    Friend WithEvents ucrSelectorMyDialog As ucrSelectorByDataFrameAddRemove
    Friend WithEvents ucrReceiverX As ucrReceiverSingle
    Friend WithEvents ucrCheckStatisticalSummary As CheckBox
    Friend WithEvents ucrCheckKeys As CheckBox
    Friend WithEvents lblGraphs As Label
    Friend WithEvents ucrChooseGraph As ComboBox
    Friend WithEvents lblYVariable As Label
    Friend WithEvents lblXVariable As Label
    Friend WithEvents ucrReceiverY As ucrReceiverSingle
    Friend WithEvents ucrPlotGraph As Button
    Friend WithEvents lblColour As Label
    Friend WithEvents ucrColourInput As ComboBox
    Friend WithEvents ucrSaveGraph As ucrSaveGraph
    Friend WithEvents lblNumber As Label
    Friend WithEvents ucrNumNumber As NumericUpDown
    Friend WithEvents grpChooseParametersToAdd As GroupBox
End Class
