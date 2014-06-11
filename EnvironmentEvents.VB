Option Strict Off
Option Explicit Off
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics

Public Module EnvironmentEvents

    Private Sub DocumentEvents_DocumentSaved(ByVal document As EnvDTE.Document) _
            Handles DocumentEvents.DocumentSaved

        Dim textDocument As TextDocument = document.Object

        Try
            Dim fileName = document.Name.ToLower()
            If fileName.EndsWith(".cs") Or _
                fileName.EndsWith(".c") Or _
                fileName.EndsWith(".cc") Or _
                fileName.EndsWith(".cpp") Or _
                fileName.EndsWith(".h") Or _
                fileName.EndsWith(".hpp") Then

                Dim result As Boolean = _
                    TrimTrailingWhiteSpaces(textDocument) Or _
                    TranslateTabsToSpaces(textDocument)

                If result Then
                    document.Save() ' Triggers DocumentEvents_DocumentSaved event again
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.OkOnly, "Trim trailing white space exception")
        End Try
    End Sub

#Region "Automatically generated code, do not modify"
    'Automatically generated code, do not modify
    'Event Sources Begin
	<System.ContextStaticAttribute()> Public WithEvents DTEEvents As EnvDTE.DTEEvents
	<System.ContextStaticAttribute()> Public WithEvents DocumentEvents As EnvDTE.DocumentEvents
	<System.ContextStaticAttribute()> Public WithEvents WindowEvents As EnvDTE.WindowEvents
	<System.ContextStaticAttribute()> Public WithEvents TaskListEvents As EnvDTE.TaskListEvents
	<System.ContextStaticAttribute()> Public WithEvents FindEvents As EnvDTE.FindEvents
	<System.ContextStaticAttribute()> Public WithEvents OutputWindowEvents As EnvDTE.OutputWindowEvents
	<System.ContextStaticAttribute()> Public WithEvents SelectionEvents As EnvDTE.SelectionEvents
	<System.ContextStaticAttribute()> Public WithEvents BuildEvents As EnvDTE.BuildEvents
	<System.ContextStaticAttribute()> Public WithEvents SolutionEvents As EnvDTE.SolutionEvents
	<System.ContextStaticAttribute()> Public WithEvents SolutionItemsEvents As EnvDTE.ProjectItemsEvents
	<System.ContextStaticAttribute()> Public WithEvents MiscFilesEvents As EnvDTE.ProjectItemsEvents
	<System.ContextStaticAttribute()> Public WithEvents DebuggerEvents As EnvDTE.DebuggerEvents
	<System.ContextStaticAttribute()> Public WithEvents ProjectsEvents As EnvDTE.ProjectsEvents
	<System.ContextStaticAttribute()> Public WithEvents TextDocumentKeyPressEvents As EnvDTE80.TextDocumentKeyPressEvents
	<System.ContextStaticAttribute()> Public WithEvents CodeModelEvents As EnvDTE80.CodeModelEvents
	<System.ContextStaticAttribute()> Public WithEvents DebuggerProcessEvents As EnvDTE80.DebuggerProcessEvents
	<System.ContextStaticAttribute()> Public WithEvents DebuggerExpressionEvaluationEvents As EnvDTE80.DebuggerExpressionEvaluationEvents
    'Event Sources End
    'End of automatically generated code
#End Region

End Module

