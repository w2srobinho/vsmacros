Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics
Imports System.Collections

Public Module SmartSort

    Public Class IncludeComparer
        Implements IComparer

        Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
            Dim strx As String = DirectCast(x, String)
            Dim stry As String = DirectCast(y, String)
            Return strx.ToLower() < stry.ToLower()
        End Function

    End Class

    ' Organizes C++ includes grouping them by the corresponding header of current source file (if it
    ' applies), and then user headers, system headers and resource headers. It removes duplicated includes and orders them
    ' alfabetically.
    ' It's usefull too to order alfabetically everything other than include headers.
    Sub SmartSort()
        Dim selection As EnvDTE.TextSelection = ActiveDocument.Selection
        Dim editPoint As EnvDTE.EditPoint = selection.TopPoint.CreateEditPoint

        If (selection.TopPoint.Line = selection.BottomPoint.Line) Then
            Return
        End If

        Dim bottomLine = selection.BottomPoint.Line
        If (selection.BottomPoint.AtStartOfLine) Then
            bottomLine = bottomLine - 1
        End If

        Dim selectedText = editPoint.GetLines(selection.TopPoint.Line, bottomLine + 1)
        Dim lines = selectedText.Split(System.Environment.NewLine.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)

        Dim include_comparer As New IncludeComparer()
        System.Array.Sort(lines, include_comparer)

        Dim fileName As String = ActiveDocument.Name.ToLower()
        Dim correspondingHeaderFileName As String = ""
        If fileName.EndsWith(".c") Then
            correspondingHeaderFileName = Left(fileName, fileName.Length - 1) + "h"
        ElseIf fileName.EndsWith(".cpp") Then
            correspondingHeaderFileName = Left(fileName, fileName.Length - 3) + "h"
        End If

        Dim correspondingHeaderInclude As String = ""
        Dim systemIncludes As New ArrayList()
        Dim userIncludes As New ArrayList()
        Dim resourceIncludes As New ArrayList()
        Dim nonIncludeLines As New ArrayList()

        Try
            For i = LBound(lines) To UBound(lines)
                Dim str = lines(i).Trim()
                Dim isInclude = str.IndexOf("#include") <> -1
                If isInclude Then
                    str = str.Replace("\", "/")
                End If
                Dim minorIndex = str.IndexOf("<")
                Dim isSystemInclude = isInclude And minorIndex <> -1
                If isSystemInclude Then
                    Dim majorIndex = str.IndexOf(">")
                    Dim header = Mid(str, minorIndex + 1, majorIndex - minorIndex).ToLower()
                    systemIncludes.Add(str)
                ElseIf isInclude Then
                    If str.IndexOf(".rh") <> -1 Then
                        resourceIncludes.Add(str)
                    ElseIf correspondingHeaderFileName <> "" And str.ToLower().IndexOf("""" + correspondingHeaderFileName.ToLower()) <> -1 Then
                        correspondingHeaderInclude = str
                    Else
                        userIncludes.Add(str)
                    End If
                Else
                    nonIncludeLines.Add(lines(i)) ' keeps identation
                End If
            Next
        Catch ex As System.Exception
            Return
        End Try

        RemoveDuplicates(userIncludes)
        RemoveDuplicates(systemIncludes)
        RemoveDuplicates(resourceIncludes)

        DTE.UndoContext.Open("Smart Sort")
        Try
            ' clear selection
            selection.MoveToLineAndOffset(selection.TopPoint.Line, 1)
            selection.MoveToLineAndOffset(bottomLine, 1, True)
            selection.EndOfLine(True)
            selection.Delete()

            If correspondingHeaderInclude <> "" Then
                selection.Insert(correspondingHeaderInclude)
                selection.NewLine()
                selection.NewLine()
            End If

            For i = 0 To userIncludes.Count - 1
                selection.Insert(userIncludes(i))
                If i < userIncludes.Count - 1 Then selection.NewLine()
            Next i

            If userIncludes.Count > 0 And (systemIncludes.Count > 0) Then
                selection.NewLine()
                selection.NewLine()
            End If

            For i = 0 To systemIncludes.Count - 1
                selection.Insert(systemIncludes(i))
                If i < systemIncludes.Count - 1 Then selection.NewLine()
            Next i

            If (userIncludes.Count > 0 Or systemIncludes.Count > 0) And resourceIncludes.Count > 0 Then
                selection.NewLine()
                selection.NewLine()
            End If

            For i = 0 To resourceIncludes.Count - 1
                selection.Insert(resourceIncludes(i))
                If i < resourceIncludes.Count - 1 Then selection.NewLine()
            Next i

            If (userIncludes.Count > 0 Or systemIncludes.Count > 0 Or resourceIncludes.Count > 0) And nonIncludeLines.Count > 0 Then
                selection.NewLine()
                selection.NewLine()
            End If

            ' order alfabetically o que não for include
            nonIncludeLines.Sort(New CaseInsensitiveComparer())

            For i = 0 To nonIncludeLines.Count - 1
                selection.EndOfLine(False)
                selection.Insert(nonIncludeLines(i))
                If i < nonIncludeLines.Count - 1 Then selection.NewLine()
            Next i

        Finally
            ' If an error occurred, then make sure that the undo context is cleaned up.
            ' Otherwise, the editor can be left in a perpetual undo context.
            DTE.UndoContext.Close()
        End Try
    End Sub

    Public Sub RemoveDuplicates(ByRef includes As ArrayList)
        For i = includes.Count - 1 To 1 Step -1
            If (includes(i).ToLower() = includes(i - 1).ToLower()) Then
                includes.RemoveAt(i)
            End If
        Next
    End Sub

End Module
