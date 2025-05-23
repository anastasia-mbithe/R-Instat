﻿' R- Instat
' Copyright (C) 2015-2017
'
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports instat.Translations

Public Class ucrDataView
    Private _clsDataBook As clsDataBook
    Private _grid As IDataViewGrid
    Private bOnlyUpdateOneCell As Boolean = False
    Private _hasChanged As Boolean

    Public WriteOnly Property DataBook() As clsDataBook
        Set(value As clsDataBook)
            _clsDataBook = value
            _grid.DataBook = value
        End Set
    End Property

    Public ReadOnly Property CellContextMenu As ContextMenuStrip
        Get
            Return cellContextMenuStrip
        End Get
    End Property

    Public ReadOnly Property ColumnContextMenu As ContextMenuStrip
        Get
            Return columnContextMenuStrip
        End Get
    End Property

    Public ReadOnly Property RowContextMenu As ContextMenuStrip
        Get
            Return rowContextMenuStrip
        End Get
    End Property

    Public ReadOnly Property SheetTabContextMenu As ContextMenuStrip
        Get
            Return statusColumnMenu
        End Get
    End Property

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub ucrDataView_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        mnuInsertColsBefore.Visible = False
        mnuInsertColsAfter.Visible = False
        autoTranslate(Me)

        If RuntimeInformation.IsOSPlatform(OSPlatform.Linux) Then
            _grid = ucrLinuxGrid
        Else
            _grid = ucrReoGrid
        End If
        'Debug 
        '_grid = ucrLinuxGrid

        _grid.SetContextmenuStrips(columnContextMenuStrip, cellContextMenuStrip, rowContextMenuStrip, statusColumnMenu)
        AttachEventsToGrid()
        RefreshDisplayInformation()
    End Sub

    Private Sub AttachEventsToGrid()
        AddHandler _grid.WorksheetChanged, AddressOf CurrentWorksheetChanged
        AddHandler _grid.WorksheetInserted, AddressOf WorksheetInserted
        AddHandler _grid.WorksheetRemoved, AddressOf WorksheetRemoved
        AddHandler _grid.ReplaceValueInData, AddressOf ReplaceValueInData
        AddHandler _grid.PasteValuesToDataframe, AddressOf PasteValuesToDataFrame
        AddHandler _grid.CellDataChanged, AddressOf CellDataChanged
        AddHandler _grid.DeleteValuesToDataframe, AddressOf DeleteCell_Click
        AddHandler _grid.EditCell, AddressOf EditCell
        AddHandler _grid.FindRow, AddressOf FindRow
    End Sub

    Private Sub RefreshWorksheet(fillWorkSheet As clsWorksheetAdapter, dataFrame As clsDataFrame)
        If Not dataFrame.clsVisibleDataFramePage.HasChanged Then
            Exit Sub
        End If
        _grid.CurrentWorksheet = fillWorkSheet
        _grid.AddColumns(dataFrame.clsVisibleDataFramePage)
        _grid.AddRowData(dataFrame)
        _grid.UpdateWorksheetStyle(fillWorkSheet)
        dataFrame.clsVisibleDataFramePage.HasChanged = False

        RefreshDisplayInformation()
    End Sub

    Public Sub UpdateAllWorksheetStyles()
        _grid.UpdateAllWorksheetStyles()
    End Sub

    Private Sub UpdateNavigationButtons()
        lblColBack.Enabled = If(GetCurrentDataFrameFocus()?.clsVisibleDataFramePage?.CanLoadPreviousColumnPage(), False)
        lblColNext.Enabled = If(GetCurrentDataFrameFocus()?.clsVisibleDataFramePage?.CanLoadNextColumnPage(), False)
        lblColFirst.Enabled = lblColBack.Enabled
        lblColLast.Enabled = lblColNext.Enabled

        lblRowBack.Enabled = If(GetCurrentDataFrameFocus()?.clsVisibleDataFramePage?.CanLoadPreviousRowPage(), False)
        lblRowNext.Enabled = If(GetCurrentDataFrameFocus()?.clsVisibleDataFramePage?.CanLoadNextRowPage(), False)
        lblRowFirst.Enabled = lblRowBack.Enabled
        lblRowLast.Enabled = lblRowNext.Enabled
    End Sub

    Private Sub AddAndUpdateWorksheets()
        Dim firstAddedWorksheet As clsWorksheetAdapter = Nothing
        Dim strCurrWorksheet As String = GetCurrentDataFrameNameFocus()
        For Each clsDataFrame In _clsDataBook.DataFrames
            Dim worksheet As clsWorksheetAdapter = _grid.GetWorksheet(clsDataFrame.strName)
            If worksheet Is Nothing Then
                worksheet = _grid.AddNewWorksheet(clsDataFrame.strName)
                If firstAddedWorksheet Is Nothing Then
                    firstAddedWorksheet = worksheet
                End If
            End If
            RefreshWorksheet(worksheet, clsDataFrame)

        Next
        If strCurrWorksheet IsNot Nothing Then
            _grid.ReOrderWorksheets(strCurrWorksheet)
        End If
        If firstAddedWorksheet IsNot Nothing Then
            _grid.CurrentWorksheet = firstAddedWorksheet
        End If
    End Sub

    Public Sub RefreshGridData()
        If _clsDataBook Is Nothing Then
            Exit Sub
        End If
        _clsDataBook.RefreshData()
        'If we are updating one cell we do not need to refresh the grid and the 
        'refresh of that cell will be done manually 
        If Not bOnlyUpdateOneCell Then
            AddAndUpdateWorksheets()
            _grid.RemoveOldWorksheets()
            If _clsDataBook.DataFrames.Count = 0 Then
                RefreshDisplayInformation()
            End If
        End If
        _hasChanged = True
        EnableDisableUndoMenu()
        _grid.Focus()
        frmMain.EnableDisbaleViewSwapMenu(_clsDataBook.DataFrames.Count > 0)
    End Sub

    ''' <summary>
    ''' <para>Gets current selected data frame</para>
    ''' todo. rename this to GetSelectedDataFrame?
    ''' </summary>
    ''' <returns>
    ''' <para>Nothing if no data frame is currently focused.</para>
    ''' <para>This can happen when all data frames have been deleted</para>
    ''' </returns>
    Public Function GetCurrentDataFrameFocus() As clsDataFrame
        Return If(_grid.CurrentWorksheet Is Nothing, Nothing, _clsDataBook.GetDataFrame(_grid.CurrentWorksheet.Name))
    End Function

    ''' <summary>
    ''' <para>Gets current selected data frame name</para> 
    ''' todo. rename this to GetSelectedDataFrameName?
    ''' </summary>
    ''' <returns>
    ''' <para>Nothing if no data frame is currently focused.</para>
    ''' <para>This can happen when all data frames have been deleted</para>
    ''' </returns>
    Public Function GetCurrentDataFrameNameFocus() As String
        Return If(_grid.CurrentWorksheet Is Nothing, Nothing, _grid.CurrentWorksheet.Name)
    End Function

    Public Property HasDataChanged() As Boolean
        Get
            Dim currentDataFrame = GetCurrentDataFrameFocus()
            If currentDataFrame IsNot Nothing AndAlso currentDataFrame.clsVisibleDataFramePage IsNot Nothing Then
                Return currentDataFrame.clsVisibleDataFramePage.HasDataChangedForAutoSave
            End If
            Return False ' Or a default value
        End Get
        Set(ByVal value As Boolean)
            Dim currentDataFrame = GetCurrentDataFrameFocus()
            If currentDataFrame IsNot Nothing AndAlso currentDataFrame.clsVisibleDataFramePage IsNot Nothing Then
                currentDataFrame.clsVisibleDataFramePage.HasDataChangedForAutoSave = value
            End If
            ' Optionally handle the case where currentDataFrame is Nothing
        End Set
    End Property

    Private Sub mnuDeleteCol_Click(sender As Object, e As EventArgs) Handles mnuDeleteCol.Click
        If GetSelectedColumns.Count = GetCurrentDataFrameFocus()?.iTotalColumnCount Then
            MsgBox("Cannot delete all visible columns." & Environment.NewLine & "Use Prepare > Data Object > Delete Data Frame if you wish to delete the data.", MsgBoxStyle.Information, "Cannot Delete All Columns")
        Else
            Dim deleteCol = MsgBox("Are you sure you want to delete these column(s)?", MessageBoxButtons.YesNo, "Delete Column")
            If deleteCol = DialogResult.Yes Then
                StartWait()
                GetCurrentDataFrameFocus().clsPrepareFunctions.DeleteColumn(GetSelectedColumnNames())
                EndWait()
                _grid.Focus()
            End If
        End If
    End Sub

    Private Sub mnuInsertRowsAfter_Click(sender As Object, e As EventArgs) Handles mnuInsertRowsAfter.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.InsertRows(GetSelectedRows.Count, GetLastSelectedRow(), False)
        EndWait()
        _grid.Focus()
    End Sub

    Private Sub mnuInsertRowsBefore_Click(sender As Object, e As EventArgs) Handles mnuInsertRowsBefore.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.InsertRows(GetSelectedRows.Count, GetFirstSelectedRow, True)
        EndWait()
        _grid.Focus()
    End Sub

    Private Sub mnuDeleteRows_Click(sender As Object, e As EventArgs) Handles mnuDeleteRows.Click
        Dim Delete = MsgBox("Are you sure you want to delete these row(s)?" & Environment.NewLine & "This action cannot be undone.", MessageBoxButtons.YesNo, "Delete Row(s)")
        If Delete = DialogResult.Yes Then
            StartWait()
            GetCurrentDataFrameFocus().clsPrepareFunctions.DeleteRows(GetSelectedRows())
            EndWait()
            _grid.Focus()
        End If
    End Sub

    Public Sub CopyRange()
        Try
            _grid.CopyRange()
        Catch
            MessageBox.Show("Cannot copy the current selection.")
        End Try
    End Sub

    Public Sub SelectAllText()
        _grid.SelectAll()
    End Sub

    Private Sub EnableDisableUndoMenu()
        If GetWorkSheetCount() <> 0 AndAlso _clsDataBook IsNot Nothing AndAlso GetCurrentDataFrameFocus() IsNot Nothing Then
            frmMain.mnuUndo.Enabled = GetCurrentDataFrameFocus.clsVisibleDataFramePage.HasUndoHistory
        End If
    End Sub

    Private Sub deleteSheet_Click(sender As Object, e As EventArgs) Handles deleteDataFrame.Click
        dlgDeleteDataFrames.SetDataFrameToAdd(_grid.CurrentWorksheet.Name)
        dlgDeleteDataFrames.ShowDialog()
    End Sub

    Public Sub WorksheetRemoved(worksheet As clsWorksheetAdapter)
        SetGridVisibility(_clsDataBook.DataFrames.Count > 0)
    End Sub

    Private Sub mnuColumnRename_Click(sender As Object, e As EventArgs) Handles mnuColumnRename.Click
        dlgName.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        dlgName.ShowDialog()
    End Sub

    Public Sub WorksheetInserted()
        DisableEnableUndo(frmMain.clsInstatOptions.bSwitchOffUndo)
    End Sub

    Public Sub CurrentWorksheetChanged()
        frmMain.ucrColumnMeta.SetCurrentDataFrame(GetCurrentDataFrameNameFocus())
        RefreshDisplayInformation()
        IsUndo()
    End Sub

    Public Function GetFirstRowHeader() As String
        Return _grid.GetFirstRowHeader
    End Function

    Public Function GetLastRowHeader() As String
        Return _grid.GetLastRowHeader
    End Function

    Public Function GetWorkSheetCount() As Integer
        Return _grid.GetWorksheetCount
    End Function

    Public Sub RemoveAllBackgroundColors()
        _grid.RemoveAllBackgroundColors()
    End Sub

    Public Sub AdjustColumnWidthAfterWrapping(strColumn As String, Optional bApplyWrap As Boolean = False)
        _grid.AdjustColumnWidthAfterWrapping(strColumn, bApplyWrap)
    End Sub

    Public Sub IsUndo()
        If GetWorkSheetCount() <> 0 AndAlso _clsDataBook IsNot Nothing AndAlso GetCurrentDataFrameFocus() IsNot Nothing Then
            frmMain.clsInstatOptions.SetOffUndo(GetCurrentDataFrameFocus.clsVisibleDataFramePage.IsUndo(GetCurrentDataFrameNameFocus))
        End If
    End Sub

    Private Sub RefreshDisplayInformation()
        If GetWorkSheetCount() <> 0 AndAlso _clsDataBook IsNot Nothing AndAlso GetCurrentDataFrameFocus() IsNot Nothing Then
            frmMain.tstatus.Text = _grid.CurrentWorksheet.Name
            SetDisplayLabels()
            UpdateNavigationButtons()
            SetGridVisibility(True)
            EnableDisableUndoMenu()
        Else
            frmMain.tstatus.Text = GetTranslation("No data loaded")
            SetGridVisibility(False)
        End If
    End Sub

    Public Sub DisableEnableUndo(bDisable As Boolean)
        If GetWorkSheetCount() <> 0 AndAlso _clsDataBook IsNot Nothing AndAlso GetCurrentDataFrameFocus() IsNot Nothing Then
            GetCurrentDataFrameFocus.clsVisibleDataFramePage.DisableEnableUndo(bDisable, GetCurrentDataFrameNameFocus)
        End If
    End Sub

    Private Sub ResizeLabels()
        Const iMinSize As Single = 4.5
        TblPanPageDisplay.Font = New Font(TblPanPageDisplay.Font.FontFamily, 12, TblPanPageDisplay.Font.Style)

        While lblRowDisplay.Width + lblColDisplay.Width + 50 +
                    lblColBack.Width + lblColFirst.Width + lblColLast.Width + lblColNext.Width +
                    lblRowBack.Width + lblRowFirst.Width + lblRowNext.Width + lblRowLast.Width > TblPanPageDisplay.Width AndAlso
                    TblPanPageDisplay.Font.Size > iMinSize
            TblPanPageDisplay.Font = New Font(TblPanPageDisplay.Font.FontFamily, TblPanPageDisplay.Font.Size - 0.5F, TblPanPageDisplay.Font.Style)
        End While
    End Sub

    Private Sub SetGridVisibility(bIsVisible As Boolean)
        If bIsVisible Then
            tlpTableContainer.ColumnStyles(0).SizeType = SizeType.Absolute
            tlpTableContainer.ColumnStyles(0).Width = 0
            If _grid.GetType() Is GetType(ucrDataViewReoGrid) Then
                tlpTableContainer.ColumnStyles(1).SizeType = SizeType.Absolute
                tlpTableContainer.ColumnStyles(1).Width = 0
                tlpTableContainer.ColumnStyles(2).SizeType = SizeType.Percent
                tlpTableContainer.ColumnStyles(2).Width = 100
                'when the TableLayoutPanel column for the reogrid gets set to 0,
                'the SheetTabWidth gets set to 60 pixels
                'this makes the sheet names invisible when data is loaded into the grid.
                'this check is meant to be a quick fix to this.
                'Other possible solutions can implemented later
                If ucrReoGrid.grdData.SheetTabWidth < 450 Then
                    '450 pixels is the ideal width for displaying mutliple sheet names loaded to the grid
                    ucrReoGrid.grdData.SheetTabWidth = 450
                End If
            Else
                tlpTableContainer.ColumnStyles(1).SizeType = SizeType.Percent
                tlpTableContainer.ColumnStyles(1).Width = 100
                tlpTableContainer.ColumnStyles(2).SizeType = SizeType.Absolute
                tlpTableContainer.ColumnStyles(2).Width = 0
            End If
        Else
            tlpTableContainer.ColumnStyles(0).SizeType = SizeType.Percent
            tlpTableContainer.ColumnStyles(0).Width = 100
            tlpTableContainer.ColumnStyles(1).SizeType = SizeType.Absolute
            tlpTableContainer.ColumnStyles(1).Width = 0
            tlpTableContainer.ColumnStyles(2).SizeType = SizeType.Absolute
            tlpTableContainer.ColumnStyles(2).Width = 0
        End If
    End Sub

    ''' <summary>
    ''' Set the text at the bottom of the status bar. For example:
    ''' <para>Rows 1:1000 (42063) Columns 1:10 (10)</para>
    ''' <para>Rows 11000 (10672/42063) Columns 1:10 (10)</para>
    ''' <para>Rows 1:1000 (10672/42063) Columns 1:7 (7/641)</para>
    ''' </summary>
    Public Sub SetDisplayLabels()
        If IsNothing(GetCurrentDataFrameFocus()) Then
            Exit Sub
        End If

        Dim startRow As Integer = GetCurrentDataFrameFocus().clsVisibleDataFramePage.intStartRow
        Dim endRow As Integer = GetCurrentDataFrameFocus().clsVisibleDataFramePage.intEndRow
        Dim filteredRows As Integer = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.iFilteredRowCount
        Dim totalRows As Integer = GetCurrentDataFrameFocus().iTotalRowCount

        Dim strRowLabel As String
        If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied AndAlso filteredRows = 0 Then
            strRowLabel = " 0:0 ("
        Else
            strRowLabel = " " & startRow & ":" & endRow & " ("
        End If

        Dim startCol As Integer = GetCurrentDataFrameFocus().clsVisibleDataFramePage.intStartColumn
        Dim endCol As Integer = GetCurrentDataFrameFocus().clsVisibleDataFramePage.intEndColumn
        Dim totalCols As Integer = GetCurrentDataFrameFocus().iTotalColumnCount

        Dim strColLabel As String = " " & startCol & ":" & endCol & " ("

        ' Set Row Display Text
        lblRowDisplay.Text = GetTranslation("Rows") ' Required by translation engine
        lblRowDisplay.Text &= strRowLabel

        If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied Then
            lblRowDisplay.Text &= filteredRows & "/" & totalRows & ") | " &
                              GetCurrentDataFrameFocus().clsFilterOrColumnSelection.strName
        Else
            lblRowDisplay.Text &= totalRows & ")"
        End If

        ' Set Column Display Text
        lblColDisplay.Text = GetTranslation("Columns") ' Required by translation engine
        lblColDisplay.Text &= strColLabel

        If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied AndAlso
       GetCurrentDataFrameFocus.clsVisibleDataFramePage.UseColumnSelectionInDataView Then
            lblColDisplay.Text &= GetCurrentDataFrameFocus().clsFilterOrColumnSelection.iSelectedColumnCount &
                              "/" & totalCols & ") | " &
                              GetCurrentDataFrameFocus().clsFilterOrColumnSelection.strSelectionName
        Else
            lblColDisplay.Text &= totalCols & ")"
        End If

        ResizeLabels()
    End Sub

    Private Sub ReplaceValueInData(strNewValue As String, strColumnName As String, strRowText As String)
        Dim dblValue As Double
        Dim iValue As Integer
        Dim bWithQuotes As Boolean
        Dim bListOfVector = False

        If strNewValue = "NA" Then
            bWithQuotes = False
        Else
            Select Case GetCurrentDataFrameFocus().clsPrepareFunctions.GetDataTypeLabel(strColumnName)
                Case "factor"
                    If Not GetCurrentDataFrameFocus().clsPrepareFunctions.GetColumnFactorLevels(strColumnName).Contains(strNewValue) Then
                        MsgBox("Invalid value: '" & strNewValue & "'" & Environment.NewLine & "This column is: factor. Values must be an existing level of this factor column.", MsgBoxStyle.Exclamation, "Invalid Value")
                        Exit Sub
                    Else
                        bWithQuotes = True
                    End If
                Case "numeric"
                    If Double.TryParse(strNewValue, dblValue) Then
                        bWithQuotes = False
                    Else
                        MsgBox("Invalid value: '" & strNewValue & "'" & Environment.NewLine & "This column is: numeric. Values must be numeric.", MsgBoxStyle.Exclamation, "Invalid Value")
                        Exit Sub
                    End If
                Case "integer"
                    If Integer.TryParse(strNewValue, iValue) Then
                        bWithQuotes = False
                    Else
                        MsgBox("Invalid value: '" & strNewValue & "'" & Environment.NewLine & "This column is: integer. Values must be integer.", MsgBoxStyle.Exclamation, "Invalid Value")
                        Exit Sub
                    End If
                Case "list"
                    If strNewValue.Split(",").All(Function(x) Integer.TryParse(x, iValue) Or Double.TryParse(x, dblValue) Or Trim(x) = "NA") Then
                        bWithQuotes = False
                        bListOfVector = strNewValue.Contains(",")
                    Else
                        MsgBox("Invalid value: '" & strNewValue & "'" & Environment.NewLine & "This column is: a list of numeric and numeric vector. Values must be numeric.", MsgBoxStyle.Exclamation, "Invalid Value")
                        Exit Sub
                    End If
                Case Else
                    If Double.TryParse(strNewValue, dblValue) OrElse strNewValue = "TRUE" OrElse strNewValue = "FALSE" Then
                        bWithQuotes = False
                    Else
                        bWithQuotes = True
                    End If
            End Select
        End If
        StartWait()
        bOnlyUpdateOneCell = True
        GetCurrentDataFrameFocus().clsPrepareFunctions.ReplaceValueInData(strNewValue, strColumnName, strRowText, bWithQuotes, bListOfVector)
        bOnlyUpdateOneCell = False
        EndWait()
    End Sub

    Private Sub renameSheet_Click(sender As Object, e As EventArgs) Handles renameSheet.Click
        dlgRenameDataFrame.SetCurrentDataframe(_grid.CurrentWorksheet.Name)
        dlgRenameDataFrame.ShowDialog()
    End Sub

    Private Sub MoveOrCopySheet_Click(sender As Object, e As EventArgs) Handles CopySheet.Click
        dlgCopyDataFrame.SetCurrentDataframe(_grid.CurrentWorksheet.Name)
        dlgCopyDataFrame.ShowDialog()
    End Sub

    Private Sub mnuLevelsLabels_Click(sender As Object, e As EventArgs) Handles mnuLevelsLabels.Click
        If IsFirstSelectedColumnAFactor() Then
            dlgLabelsLevels.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        End If
        dlgLabelsLevels.ShowDialog()
    End Sub

    Public Sub UseColumnSelectionInDataView(bUseColumnSelecion As Boolean)
        If GetCurrentDataFrameFocus() IsNot Nothing Then
            GetCurrentDataFrameFocus().clsVisibleDataFramePage.UseColumnSelectionInDataView = bUseColumnSelecion
        End If
    End Sub

    Public Function IsColumnSelectionApplied() As Boolean
        Return GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied
    End Function

    Private Function GetSelectedColumns() As List(Of clsColumnHeaderDisplay)
        Return _grid.GetSelectedColumns()
    End Function

    Private Function GetSelectedColumnIndexes() As List(Of String)
        Return _grid.GetSelectedColumnIndexes()
    End Function

    Private Function GetSelectedColumnNames() As List(Of String)
        Return GetSelectedColumns().Select(Function(x) x.strName).ToList()
    End Function

    Public Function GetFirstSelectedColumnName() As String
        Return GetSelectedColumns().FirstOrDefault().strName
    End Function

    Private Function GetLastSelectedColumnName() As String
        Return GetSelectedColumns().LastOrDefault().strName
    End Function

    Private Function IsOnlyOneColumnSelected() As Boolean
        Return GetSelectedColumnNames.Count = 1
    End Function

    Private Function IsFirstSelectedColumnAFactor() As Boolean
        Return GetSelectedColumns().FirstOrDefault().bIsFactor
    End Function

    Private Function GetFirstSelectedRow() As String
        Return GetSelectedRows().FirstOrDefault()
    End Function

    Private Function GetSelectedRows() As List(Of String)
        Return _grid.GetSelectedRows()
    End Function

    Private Function GetLastSelectedRow() As String
        Return GetSelectedRows.LastOrDefault()
    End Function

    Public Sub StartWait()
        Cursor = Cursors.WaitCursor
        _grid.bEnabled = False
    End Sub

    Public Sub EndWait()
        _grid.bEnabled = True
        Cursor = Cursors.Default
    End Sub

    Private Sub mnuConvertText_Click(sender As Object, e As EventArgs) Handles mnuConvertText.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToText(GetSelectedColumnNames())
        EndWait()
    End Sub

    Private Sub mnuConvertToFactor_Click(sender As Object, e As EventArgs) Handles mnuConvertToFactor.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToFactor(GetSelectedColumnNames())
        EndWait()
    End Sub

    Private Sub mnuColumnFilter_Click(sender As Object, e As EventArgs) Handles mnuColumnFilterRows.Click
        dlgRestrict.bIsSubsetDialog = False
        dlgRestrict.strDefaultDataframe = _grid.CurrentWorksheet.Name
        dlgRestrict.ShowDialog()
    End Sub

    Private Sub reorderSheet_Click(sender As Object, e As EventArgs) Handles reorderSheet.Click
        dlgReorderDataFrame.ShowDialog()
    End Sub

    Private Sub mnuFilter_Click(sender As Object, e As EventArgs) Handles mnuFilter.Click
        dlgRestrict.bIsSubsetDialog = False
        dlgRestrict.strDefaultDataframe = _grid.CurrentWorksheet.Name
        dlgRestrict.ShowDialog()
    End Sub

    Private Sub mnuRemoveCurrentFilter_Click(sender As Object, e As EventArgs) Handles mnuRemoveCurrentFilter.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.RemoveCurrentFilter()
        EndWait()
    End Sub

    Private Sub mnuClearColumnFilter_Click(sender As Object, e As EventArgs) Handles mnuClearColumnFilter.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.RemoveCurrentFilter()
        EndWait()
    End Sub

    Private Sub mnuConvertToDate_Click(sender As Object, e As EventArgs) Handles mnuConvertToDate.Click, mnuConvertToColumnDate.Click
        dlgMakeDate.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        dlgMakeDate.enumMakedateMode = dlgMakeDate.MakedateMode.Column
        dlgMakeDate.ShowDialog()
    End Sub

    Private Sub mnuSort_Click(sender As Object, e As EventArgs) Handles mnuSort.Click
        dlgSort.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        dlgSort.ShowDialog()
    End Sub

    Private Sub mnuFreezeToHere_Click(sender As Object, e As EventArgs)
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.FreezeColumns(GetLastSelectedColumnName)
        EndWait()
    End Sub

    Private Sub mnuUnfreeze_Click(sender As Object, e As EventArgs)
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.UnFreezeColumns()
        EndWait()
    End Sub

    Private Sub mnuCovertToOrderedFactors_Click(sender As Object, e As EventArgs) Handles mnuCovertToOrderedFactors.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToOrderedFactor(GetSelectedColumnNames())
        EndWait()
    End Sub

    Private Sub mnuDuplicateColumn_Click(sender As Object, e As EventArgs) Handles mnuDuplicateColumn.Click
        dlgDuplicateColumns.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        dlgDuplicateColumns.ShowDialog()
    End Sub

    Private Sub mnuAddComment_Click(sender As Object, e As EventArgs) Handles mnuAddComment.Click
        dlgAddComment.SetPosition(_grid.CurrentWorksheet.Name, GetFirstSelectedRow())
        dlgAddComment.ShowDialog()
    End Sub

    Private Sub mnuComment_Click(sender As Object, e As EventArgs) Handles mnuComment.Click
        dlgAddComment.SetPosition(_grid.CurrentWorksheet.Name, GetFirstSelectedRow(), GetFirstSelectedColumnName())
        dlgAddComment.ShowDialog()
    End Sub

    Public Sub SetCurrentDataFrame(strDataName As String)
        _grid.SetCurrentDataFrame(strDataName)
    End Sub

    Public Sub SetCurrentDataFrame(iIndex As Integer)
        _grid.SetCurrentDataFrame(iIndex)
    End Sub

    Private Sub columnContextMenuStrip_Opening(sender As Object, e As CancelEventArgs) Handles columnContextMenuStrip.Opening
        If IsOnlyOneColumnSelected() Then
            mnuLevelsLabels.Enabled = IsFirstSelectedColumnAFactor()
            mnuDeleteCol.Text = GetTranslation("Delete Column(s)")
            mnuInsertColsBefore.Text = GetTranslation("Insert 1 Column Before")
            mnuInsertColsAfter.Text = GetTranslation("Insert 1 Column After")
        Else
            mnuLevelsLabels.Enabled = False
            mnuDeleteCol.Text = GetTranslation("Delete Column(s)")
            mnuInsertColsBefore.Text = "Insert " & GetSelectedColumns.Count & " Columns Before"
            mnuInsertColsAfter.Text = "Insert " & GetSelectedColumns.Count & " Columns After"
        End If
        mnuClearColumnFilter.Enabled = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied
        mnuColumnContextRemoveCurrentColumnSelection.Enabled = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied
    End Sub

    Private Sub HideSheet_Click(sender As Object, e As EventArgs) Handles HideSheet.Click
        StartWait()
        _clsDataBook.HideDataFrame(_grid.CurrentWorksheet.Name)
        EndWait()
    End Sub

    Private Sub unhideSheet_Click(sender As Object, e As EventArgs) Handles unhideSheet.Click
        dlgHideDataframes.ShowDialog()
    End Sub

    Private Sub statusColumnMenu_Opening(sender As Object, e As CancelEventArgs) Handles statusColumnMenu.Opening
        HideSheet.Enabled = (GetWorkSheetCount() > 1)
    End Sub

    Private Sub mnuReorderColumns_Click(sender As Object, e As EventArgs) Handles mnuReorderColumns.Click
        dlgReorderColumns.ShowDialog()
    End Sub

    Private Sub mnuRenameColumn_Click(sender As Object, e As EventArgs) Handles mnuRenameColumn.Click
        dlgName.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        dlgName.ShowDialog()
    End Sub

    Private Sub mnuDuplColumn_Click(sender As Object, e As EventArgs) Handles mnuDuplColumn.Click
        dlgDuplicateColumns.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.CurrentWorksheet.Name)
        dlgDuplicateColumns.ShowDialog()
    End Sub

    Private Sub mnuReorderColumn_Click(sender As Object, e As EventArgs) Handles mnuReorderColumn.Click
        dlgReorderColumns.ShowDialog()
    End Sub

    Private Sub mnuConvertToFact_Click(sender As Object, e As EventArgs) Handles mnuConvertToFact.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToFactor(GetSelectedColumnNames())
        EndWait()
    End Sub

    Private Sub mnuConvertToOrderedFactor_Click(sender As Object, e As EventArgs) Handles mnuConvertToOrderedFactor.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToOrderedFactor(GetSelectedColumnNames())
        EndWait()
    End Sub

    Private Sub mnuConvertToCharacter_Click(sender As Object, e As EventArgs) Handles mnuConvertToCharacter.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToCharacter(GetSelectedColumnNames())
        EndWait()
    End Sub

    Private Sub mnuConvertToNumeric_Click(sender As Object, e As EventArgs) Handles mnuConvertToNumeric.Click, mnuConvertVariate.Click
        For Each strColumn In GetSelectedColumnNames()
            Dim iNonNumericValues As Integer = GetCurrentDataFrameFocus().clsPrepareFunctions.GetAmountOfNonNumericValuesInColumn(strColumn)
            If iNonNumericValues = 0 Then
                GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToNumeric(strColumn, True)
            Else
                Dim bCheckLabels As Boolean = GetCurrentDataFrameFocus().clsPrepareFunctions.CheckHasLabels(strColumn)
                If bCheckLabels Then
                    frmConvertToNumeric.SetDataFrameName(GetCurrentDataFrameFocus().strName)
                    frmConvertToNumeric.SetColumnName(strColumn)
                    frmConvertToNumeric.CheckLabels(bCheckLabels)
                    frmConvertToNumeric.SetNonNumeric(iNonNumericValues)
                    frmConvertToNumeric.ShowDialog()
                    ' Yes for "normal" convert and No for "labelled" convert
                    Select Case frmConvertToNumeric.DialogResult
                        Case DialogResult.Yes
                            GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToNumeric(strColumn, True)
                        Case DialogResult.No
                            GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToNumeric(strColumn, False)
                        Case DialogResult.Cancel
                            Continue For
                    End Select
                Else
                    frmConvertToNumeric.SetDataFrameName(GetCurrentDataFrameFocus().strName)
                    frmConvertToNumeric.SetColumnName(strColumn)
                    frmConvertToNumeric.CheckLabels(bCheckLabels)
                    frmConvertToNumeric.SetNonNumeric(iNonNumericValues)
                    frmConvertToNumeric.ShowDialog()
                    ' Yes for "normal" convert and No for "ordinal" convert
                    Select Case frmConvertToNumeric.DialogResult
                        Case DialogResult.Yes
                            GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToNumeric(strColumn, True)
                        Case DialogResult.No
                            GetCurrentDataFrameFocus().clsPrepareFunctions.ConvertToNumeric(strColumn, False)
                        Case DialogResult.Cancel
                            Continue For
                    End Select
                End If
                frmConvertToNumeric.Close()
            End If
        Next
    End Sub

    Private Sub mnuLebelsLevel_Click(sender As Object, e As EventArgs) Handles mnuLabelsLevel.Click
        If IsFirstSelectedColumnAFactor() Then
            dlgLabelsLevels.SetCurrentColumn(GetFirstSelectedColumnName, _grid.CurrentWorksheet.Name)
        End If
        dlgLabelsLevels.ShowDialog()
    End Sub

    Private Sub mnuSorts_Click(sender As Object, e As EventArgs) Handles mnuSorts.Click
        dlgSort.SetCurrentColumn(GetFirstSelectedColumnName, _grid.CurrentWorksheet.Name)
        dlgSort.ShowDialog()
    End Sub

    Private Sub mnuFilters_Click(sender As Object, e As EventArgs) Handles mnuFilterRows.Click
        dlgRestrict.bIsSubsetDialog = False
        dlgRestrict.strDefaultDataframe = _grid.CurrentWorksheet.Name
        dlgRestrict.ShowDialog()
    End Sub

    Private Sub mnuRemoveCurrentFilters_Click(sender As Object, e As EventArgs) Handles mnuRemoveCurrentFilters.Click
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.RemoveCurrentFilter()
        EndWait()
    End Sub

    Private Sub CellDataChanged()
        frmMain.bDataSaved = False
    End Sub

    Private Sub linkStartNewDataFrame_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartNewDataFrame.LinkClicked
        dlgNewDataFrame.ShowDialog()
    End Sub

    Private Sub linkStartOpenFile_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartOpenFile.LinkClicked
        dlgImportDataset.ShowDialog()
    End Sub

    Private Sub linkStartOpenLibrary_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartOpenLibrary.LinkClicked
        dlgFromLibrary.ShowDialog()
    End Sub

    ''' <summary>
    ''' clears all the added links label menu items from the recents panel of the data view
    ''' </summary>
    Public Sub ClearRecentFileMenuItems()
        panelRecentMenuItems.Controls.Clear()
        HideOrShowRecentSection()
    End Sub

    ''' <summary>
    ''' adds the link label as a menu item to the recents panel of the data view
    ''' </summary>
    ''' <param name="linkMenuItem">link label with file path set as its tag</param>
    Public Sub InsertRecentFileMenuItems(linkMenuItem As LinkLabel)
        'label used to display the path to the user
        Dim lblMenuItemPath As New Label
        Dim position As Integer = 1

        'add subsequent links after each other, separating them by 19 pixels on the Y axis
        If panelRecentMenuItems.Controls.Count > 0 Then
            'get Y axis position of last control then add 19 pixels to be used as the new Y axis position.
            position = panelRecentMenuItems.Controls.Item(panelRecentMenuItems.Controls.Count - 1).Location.Y
            position = position + 19
        End If

        linkMenuItem.Location = New Point(0, position)
        linkMenuItem.Height = 13
        linkMenuItem.LinkBehavior = LinkBehavior.NeverUnderline
        linkMenuItem.AutoSize = True

        'add the link control.
        panelRecentMenuItems.Controls.Add(linkMenuItem)

        'add the label control. will be besides each other on the same Y axis
        lblMenuItemPath.Text = If(String.IsNullOrEmpty(linkMenuItem.Tag), "", Path.GetDirectoryName(linkMenuItem.Tag).Replace("\", "/"))
        lblMenuItemPath.Location = New Point(linkMenuItem.Width + 10, position)
        lblMenuItemPath.Height = 13
        lblMenuItemPath.AutoSize = True
        panelRecentMenuItems.Controls.Add(lblMenuItemPath)
        HideOrShowRecentSection()
    End Sub

    Private Sub HideOrShowRecentSection()
        panelSectionRecent.Visible = panelRecentMenuItems.Controls.Count > 0
    End Sub

    Private Sub linkHelpIntroduction_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        Help.ShowHelp(frmMain, frmMain.strStaticPath & "/" & frmMain.strHelpFilePath, HelpNavigator.TopicId, "0")
    End Sub

    Private Sub linkHelpRpackages_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        dlgHelpVignettes.ShowDialog()
    End Sub

    Private Sub linkHelpRInstatWebsite_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkHelpRInstatResourcesSite.LinkClicked
        Process.Start("https://ecampus.r-instat.org/course/view.php?id=14")
    End Sub

    Private Sub rowContextMenuStrip_Opening(sender As Object, e As CancelEventArgs) Handles rowContextMenuStrip.Opening
        mnuRemoveCurrentFilter.Enabled = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied
        mnuRowContextRemoveCurrentColumnSelection.Enabled = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied
    End Sub

    Private Sub cellContextMenuStrip_Opening(sender As Object, e As CancelEventArgs) Handles cellContextMenuStrip.Opening
        mnuLabelsLevel.Enabled = IsOnlyOneColumnSelected() AndAlso IsOnlyOneColumnSelected() AndAlso IsFirstSelectedColumnAFactor()
        mnuRemoveCurrentFilters.Enabled = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied
        mnuCellContextRemoveCurrentColumnSelection.Enabled = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied
    End Sub

    Private Sub mnuColumnAddComment_Click(sender As Object, e As EventArgs) Handles mnuColumnAddComment.Click
        dlgAddComment.SetPosition(strDataFrame:=_grid.CurrentWorksheet.Name, strColumn:=GetFirstSelectedColumnName)
        dlgAddComment.ShowDialog()
    End Sub

    Private Sub mnuBottomAddComment_Click(sender As Object, e As EventArgs) Handles mnuBottomAddComment.Click
        dlgAddComment.SetPosition(strDataFrame:=_grid.CurrentWorksheet.Name)
        dlgAddComment.ShowDialog()
    End Sub

    ''' <summary>
    ''' pastes data from clipboard to data view
    ''' </summary>
    Public Sub PasteValuesToDataFrame()
        Dim strClipBoardText As String = My.Computer.Clipboard.GetText
        If String.IsNullOrEmpty(strClipBoardText) Then
            MsgBox("No data available for pasting.", MsgBoxStyle.Information, "No Data")
            Exit Sub
        End If
        'warn user action cannot be undone
        If DialogResult.No = MsgBox("Are you sure you want to paste to these column(s)?" & Environment.NewLine &
                            "This action cannot be undone.", MessageBoxButtons.YesNo, "Paste Data") Then
            Exit Sub
        End If
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.PasteValues(strClipBoardText, GetSelectedColumnNames(), GetFirstSelectedRow)
        EndWait()
    End Sub

    Private Sub lblRowFirst_Click(sender As Object, e As EventArgs) Handles lblRowFirst.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadFirstRowPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblRowBack_Click(sender As Object, e As EventArgs) Handles lblRowBack.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadPreviousRowPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblRowNext_Click(sender As Object, e As EventArgs) Handles lblRowNext.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadNextRowPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblRowLast_Click(sender As Object, e As EventArgs) Handles lblRowLast.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadLastRowPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblColFirst_Click(sender As Object, e As EventArgs) Handles lblColFirst.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadFirstColumnPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblColBack_Click(sender As Object, e As EventArgs) Handles lblColBack.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadPreviousColumnPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblColNext_Click(sender As Object, e As EventArgs) Handles lblColNext.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadNextColumnPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblColLast_Click(sender As Object, e As EventArgs) Handles lblColLast.Click
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.LoadLastColumnPage()
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub mnuColumnContextColumnSelection_Click(sender As Object, e As EventArgs) Handles mnuColumnContextColumnSelection.Click
        LoadColumnSelectionDialog()
    End Sub

    Private Sub mnuColumnContextRemoveCurrentColumnSelection_Click(sender As Object, e As EventArgs) Handles mnuColumnContextRemoveCurrentColumnSelection.Click
        RemoveCurrentColumnSelection()
    End Sub

    Private Sub mnuRowContextColumnSelection_Click(sender As Object, e As EventArgs) Handles mnuRowContextColumnSelection.Click
        LoadColumnSelectionDialog()
    End Sub

    Private Sub mnuRowContextRemoveCurrentColumnSelection_Click(sender As Object, e As EventArgs) Handles mnuRowContextRemoveCurrentColumnSelection.Click
        RemoveCurrentColumnSelection()
    End Sub

    Private Sub mnuCellContextColumnSelection_Click(sender As Object, e As EventArgs) Handles mnuCellContextColumnSelection.Click
        LoadColumnSelectionDialog()
    End Sub

    Private Sub mnuCellContextRemoveCurrentColumnSelection_Click(sender As Object, e As EventArgs) Handles mnuCellContextRemoveCurrentColumnSelection.Click
        RemoveCurrentColumnSelection()
    End Sub

    Private Sub LoadColumnSelectionDialog()
        dlgSelect.SetDefaultDataFrame(_grid.CurrentWorksheet.Name)
        dlgSelect.ShowDialog()
    End Sub

    Private Sub RemoveCurrentColumnSelection()
        StartWait()
        GetCurrentDataFrameFocus().clsPrepareFunctions.RemoveCurrentColumnSelection()
        EndWait()
    End Sub

    Private Sub ucrDataView_Resize(sender As Object, e As EventArgs) Handles TblPanPageDisplay.Resize
        ResizeLabels()
    End Sub

    Private Sub DeleteCell_Click()
        Dim deleteCell = MsgBox("This will replace the selected cells with missing values (NA)." &
                                Environment.NewLine & "Continue?",
                                MessageBoxButtons.YesNo, "Delete Cells")
        If deleteCell = DialogResult.Yes Then
            StartWait()
            GetCurrentDataFrameFocus().clsPrepareFunctions.DeleteCells(GetSelectedRows(), GetSelectedColumnIndexes())
            EndWait()
            _grid.Focus()
        End If
    End Sub

    Private Sub mnuDeleteCells_Click(sender As Object, e As EventArgs) Handles mnuDeleteCells.Click
        DeleteCell_Click()
    End Sub

    Private Sub mnuHelp_Click(sender As Object, e As EventArgs) Handles mnuHelp.Click, mnuHelp1.Click, mnuHelp2.Click, mnuHelp3.Click
        Help.ShowHelp(frmMain, frmMain.strStaticPath & "/" & frmMain.strHelpFilePath, HelpNavigator.TopicId, "697")
    End Sub

    Public Sub GoToSpecificRowPage(iPage As Integer)
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.GoToSpecificRowPage(iPage)
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblRowDisplay_Click(sender As Object, e As EventArgs) Handles lblRowDisplay.Click
        If lblRowNext.Enabled OrElse lblRowBack.Enabled Then
            sdgWindowNumber.enumWINNUMBERMode = sdgWindowNumber.WINNUMBERMode.Row
            Dim iTotalRow As Integer
            If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied Then
                iTotalRow = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.iFilteredRowCount
            Else
                iTotalRow = GetCurrentDataFrameFocus().iTotalRowCount
            End If
            sdgWindowNumber.iTotalRowOrColumn = iTotalRow
            sdgWindowNumber.iEndRowOrColumn = GetCurrentDataFrameFocus().clsVisibleDataFramePage.intEndRow
            sdgWindowNumber.ShowDialog()

            GoToSpecificRowPage(sdgWindowNumber.iPage)
        End If
    End Sub

    Public Sub GoToSpecificColumnPage(iPage As Integer)
        GetCurrentDataFrameFocus().clsVisibleDataFramePage.GoToSpecificColumnPage(iPage)
        RefreshWorksheet(_grid.CurrentWorksheet, GetCurrentDataFrameFocus())
    End Sub

    Private Sub lblColDisplay_Click(sender As Object, e As EventArgs) Handles lblColDisplay.Click
        If lblColNext.Enabled OrElse lblColBack.Enabled Then
            sdgWindowNumber.enumWINNUMBERMode = sdgWindowNumber.WINNUMBERMode.Col
            Dim iTotalCol As Integer
            If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied Then
                iTotalCol = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.iSelectedColumnCount
            Else
                iTotalCol = GetCurrentDataFrameFocus().iTotalColumnCount
            End If
            sdgWindowNumber.iTotalRowOrColumn = iTotalCol
            sdgWindowNumber.iEndRowOrColumn = GetCurrentDataFrameFocus().clsVisibleDataFramePage.intEndColumn
            sdgWindowNumber.ShowDialog()
            GoToSpecificColumnPage(sdgWindowNumber.iPage)
        End If
    End Sub

    Private Sub lblRowDisplay_MouseHover(sender As Object, e As EventArgs) Handles lblRowDisplay.MouseHover
        If lblRowNext.Enabled OrElse lblRowBack.Enabled Then
            Dim iTotalRow As Integer
            If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bFilterApplied Then
                iTotalRow = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.iFilteredRowCount
            Else
                iTotalRow = GetCurrentDataFrameFocus().iTotalRowCount
            End If
            ttGoToRowOrColPage.SetToolTip(lblRowDisplay, GetTranslation("Click to go to a specific window 1-") &
                    Math.Ceiling(CDbl(iTotalRow / frmMain.clsInstatOptions.iMaxRows)))
        Else
            ttGoToRowOrColPage.RemoveAll()
        End If
    End Sub

    Private Sub lblColDisplay_MouseHover(sender As Object, e As EventArgs) Handles lblColDisplay.MouseHover
        If lblColNext.Enabled OrElse lblColBack.Enabled Then
            Dim iTotalCol As Integer
            If GetCurrentDataFrameFocus().clsFilterOrColumnSelection.bColumnSelectionApplied Then
                iTotalCol = GetCurrentDataFrameFocus().clsFilterOrColumnSelection.iSelectedColumnCount
            Else
                iTotalCol = GetCurrentDataFrameFocus().iTotalColumnCount
            End If
            ttGoToRowOrColPage.SetToolTip(lblColDisplay, GetTranslation("Click to go to a specific window 1-") &
                    Math.Ceiling(CDbl(iTotalCol / frmMain.clsInstatOptions.iMaxCols)))
        Else
            ttGoToRowOrColPage.RemoveAll()
        End If
    End Sub

    Private Sub EditCell()
        dlgEdit.SetCurrentDataframe(GetCurrentDataFrameNameFocus)
        dlgEdit.SetCurrentColumn(GetFirstSelectedColumnName(), _grid.GetCellValue(GetFirstSelectedRow() - 1, GetFirstSelectedColumnName), GetFirstSelectedRow())
        dlgEdit.ShowDialog()
    End Sub

    Private Sub mnuEditCell_Click(sender As Object, e As EventArgs) Handles mnuEditCell.Click
        EditCell()
    End Sub

    Public Sub FindRow()
        dlgFindInVariableOrFilter.ShowDialog()
    End Sub

    Public Sub Undo()
        If frmMain.clsInstatOptions.bSwitchOffUndo Then
            ' Show a message box indicating that undo is turned off
            MsgBox("Undo is turned off, go to Tools > Options to turn it on.", vbInformation, "Undo Disabled")
            Exit Sub
        End If

        If _clsDataBook.DataFrames.Count > 0 Then
            If (GetCurrentDataFrameFocus().iTotalColumnCount >= frmMain.clsInstatOptions.iUndoColLimit) OrElse
   (GetCurrentDataFrameFocus().iTotalRowCount >= frmMain.clsInstatOptions.iUndoRowLimit) Then

                ' Retrieve the default limits for rows and columns
                Dim colLimit As Integer = frmMain.clsInstatOptions.iUndoColLimit
                Dim rowLimit As Integer = frmMain.clsInstatOptions.iUndoRowLimit

                ' Construct the concise message
                Dim msg As String = "The current data frame exceeds the undo limit (Columns: " & colLimit & ", Rows: " & rowLimit & ")."

                ' Append information on whether it's the rows, columns, or both
                If GetCurrentDataFrameFocus().iTotalColumnCount >= colLimit AndAlso
           GetCurrentDataFrameFocus().iTotalRowCount >= rowLimit Then
                    msg &= " Both columns and rows exceed the limit."
                ElseIf GetCurrentDataFrameFocus().iTotalColumnCount >= colLimit Then
                    msg &= " Columns exceed the limit."
                ElseIf GetCurrentDataFrameFocus().iTotalRowCount >= rowLimit Then
                    msg &= " Rows exceed the limit."
                End If

                msg &= " Please go to Tools > Options to adjust the limits."

                ' Display the message box
                MsgBox(msg, vbExclamation, "Undo Limit Exceeded")

                Exit Sub
            End If


            If GetCurrentDataFrameFocus.clsVisibleDataFramePage.HasUndoHistory Then
                GetCurrentDataFrameFocus.clsVisibleDataFramePage.Undo()
            End If
        End If
    End Sub

    Public Sub SearchRowInGrid(rowNumbers As List(Of Integer), strColumn As String, Optional iRow As Integer = 0,
                           Optional bApplyToRows As Boolean = False)
        _grid.SearchRowInGrid(rowNumbers, strColumn, iRow, bApplyToRows)
    End Sub

    Public Sub SelectColumnInGrid(strColumn As String)
        _grid.SelectColumnInGrid(strColumn)
    End Sub

    Private Sub linkHelpGettingStarted_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkHelpGettingStarted.LinkClicked
        Help.ShowHelp(frmMain, frmMain.strStaticPath & "/" & frmMain.strHelpFilePath, HelpNavigator.TopicId, "3")
    End Sub

    Private Sub linkHelpData_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkHelpData.LinkClicked
        Help.ShowHelp(frmMain, frmMain.strStaticPath & "/" & frmMain.strHelpFilePath, HelpNavigator.TopicId, "71")
    End Sub

    Private Sub linkHelpPrepareMenu_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        Help.ShowHelp(frmMain, frmMain.strStaticPath & "/" & frmMain.strHelpFilePath, HelpNavigator.TopicId, "9")
    End Sub

    Private Sub linkHelpClimaticMenu_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        Help.ShowHelp(frmMain, frmMain.strStaticPath & "/" & frmMain.strHelpFilePath, HelpNavigator.TopicId, "19")
    End Sub

    Private Sub linkStartPasteScriptfromClipboard_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartPasteScriptfromClipboard.LinkClicked
        frmMain.mnuViewSwapDataAndMetadata.Enabled = frmMain.mnuViewSwapDataAndScript.Checked
        frmMain.mnuViewSwapDataAndScript.Checked = Not frmMain.mnuViewSwapDataAndScript.Checked
        frmMain.UpdateSwapDataAndScript()
        frmMain.UpdateLayout()
    End Sub

    Private Sub linkStartRestoreBackup_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartRestoreBackup.LinkClicked
        dlgRestoreBackup.ShowDialog()
    End Sub

    Private Sub linkStartAddRPackage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartAddRPackage.LinkClicked
        dlgInstallRPackage.ShowDialog()
    End Sub

    Private Sub linkStartPasteData_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkStartPasteData.LinkClicked
        dlgPasteNewColumns.ShowDialog()
    End Sub

End Class