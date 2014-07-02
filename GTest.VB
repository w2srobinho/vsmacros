Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics

Public Module GTest
    Public Sub RunCurrentFixture()
        SetCurrentFixture()
        DTE.ExecuteCommand("Debug.StartWithoutDebugging")
    End Sub

    Public Sub RunCurrentTest()
        SetCurrentTest()
        DTE.ExecuteCommand("Debug.StartWithoutDebugging")
    End Sub

    Public Sub SetCurrentFixture()
        SetCurrent(True)
    End Sub

    Public Sub SetCurrentTest()
        SetCurrent(False)
    End Sub

    Private Sub SetCurrent(ByVal Fixture As Boolean)
        ' From the active source file, find the test that the user wants to run
        ' based on the current cursor position. Set the project containing this
        ' source file as the startup project, changes the command line to run
        ' only this test, compile the project and starts it under the debugger.
        ' Doesn't change any breakpoint.
        Dim ActiveDoc As Document = DTE.ActiveDocument

        ' Try to guess the test to run.
        Dim TestGroup As String = ""
        Dim TestName As String = ""
        Dim selection As TextSelection = CType(ActiveDoc.Selection(), TextSelection)
        Dim toppoint As EditPoint = selection.TopPoint.CreateEditPoint()
        Dim bottompoint As EditPoint = selection.BottomPoint.CreateEditPoint()
        Dim ranges As TextRanges = selection.TextRanges
        Dim line As Integer = selection.TopPoint.Line
        ' selection.FindPattern() is crummy.'
        While line <> 0
            selection.GotoLine(line)
            selection.SelectLine()
            Dim match As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(selection.Text, "TEST[_FP]*\((.*),(.*)\)")
            If Not match Is System.Text.RegularExpressions.Match.Empty Then
                TestGroup = match.Groups.Item(1).Value.Trim()
                If Not Fixture Then
                    TestName = match.Groups.Item(2).Value.Trim()
                End If
                Exit While
            End If
            line = line - 1
        End While
        ' Cheap way to try to restore the old selection. Isn\'t 100% correct.'
        selection.MoveToLineAndOffset(toppoint.Line, toppoint.LineCharOffset)
        selection.MoveToLineAndOffset(bottompoint.Line, bottompoint.LineCharOffset, True)

        ' From the current active document, find the project and the active configuration.'
        Dim Proj As Project = ActiveDoc.ProjectItem.ContainingProject
        Dim config As Configuration = Proj.ConfigurationManager.ActiveConfiguration

        ' Fix the command line argument.'
        Dim CmdLine As EnvDTE.Property = config.Properties.Item("CommandArguments")
        If TestGroup <> "" Then
            CmdLine.Value = "--gtest_filter=*" & TestGroup & "." & TestName & "*"
        Else
            ' Run all'
            CmdLine.Value = ""
        End If

        ' Set it as startup project.
        Dim SoluBuild As SolutionBuild = DTE.Solution.SolutionBuild
        Dim StartupProject As String
        StartupProject = Proj.UniqueName
        SoluBuild.StartupProjects = StartupProject
    End Sub

End Module

