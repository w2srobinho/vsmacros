
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics

' Duplicates the current line above or below without messing the clipboard
Public Module DuplicateLine

    Sub DuplicateLineDown()
        Dim selection As TextSelection = DTE.ActiveDocument.Selection
        Dim lineNumber As Integer
        Dim line As String

        If selection.IsEmpty Then
            selection.StartOfLine(0)
            selection.EndOfLine(True)
        Else
            Dim top As Integer = selection.TopLine
            Dim bottom As Integer = selection.BottomLine

            selection.MoveToDisplayColumn(top, 0)
            selection.StartOfLine(0)

            selection.MoveToDisplayColumn(bottom, 0, True)
            selection.EndOfLine(True)
        End If

        lineNumber = selection.TopLine
        line = selection.Text

        selection.MoveToDisplayColumn(selection.BottomLine, 0)
        selection.EndOfLine()
        selection.Insert(vbNewLine & line)
    End Sub

    Sub DuplicateLineUp()
        Dim selection As TextSelection = DTE.ActiveDocument.Selection
        Dim lineNumber As Integer
        Dim line As String

        If selection.IsEmpty Then
            selection.StartOfLine(0)
            selection.EndOfLine(True)
        Else
            Dim top As Integer = selection.TopLine
            Dim bottom As Integer = selection.BottomLine

            selection.MoveToDisplayColumn(top, 0)
            selection.StartOfLine(0)

            selection.MoveToDisplayColumn(bottom, 0, True)
            selection.EndOfLine(True)
        End If

        lineNumber = selection.BottomLine
        line = selection.Text

        selection.MoveToDisplayColumn(selection.BottomLine, 0)
        selection.Insert(vbNewLine & line)
        selection.MoveToDisplayColumn(lineNumber, 0)
    End Sub

End Module
