﻿Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports EnvDTE90a
Imports EnvDTE100
Imports System.Diagnostics

Public Module CleanCode

    ' Cleans the code applying the steps below:
    '
    '   * Replaces TAB's by 3 write spaces;
    '   * Trims trailing white spaces;
    '   * Removes comments after #endif's;
    '   * Removes visual pollution by comments session separator;
    '   * Removes #ifndef ... #endif between includes (old style optimization for Borland C++);
    '   * Transforms 2 blank lines in 1;
    '   * Removes void from C-style function "(void)";
    '   * Removes "Test" from GTest test names;
    '   * Includes a blank line at EOF;
    '   * Removes blank lines from EOF;
    '   * Removes the guard (#ifdef...#endif) from "#pragma once" (newer compilers are compliant or ingnores it).
    '   * Replaces occurences of NULL by nullptr
    '   * Formats assignments with a space before to be search friendly on cursor
    Sub CleanCode()
        DoCleanCode()
    End Sub

    Public Function TrimTrailingWhiteSpaces(ByVal doc As TextDocument) As Boolean
        Return doc.ReplacePattern("{:b}+$", "", vsFindOptions.vsFindOptionsRegularExpression)
    End Function

    Public Function TranslateTabsToSpaces(ByVal doc As TextDocument) As Boolean
        Return doc.ReplacePattern("\t", "   ", vsFindOptions.vsFindOptionsRegularExpression)
    End Function

    Public Function TranslateCHeadersToCppHeaders(ByVal doc As TextDocument) As Boolean
        Dim headers() As String = { _
            "assert.h", "cassert", _
            "ctype.h", "cctype", _
            "errno.h", "cerrno", _
            "float.h", "cfloat", _
            "iso646.h", "ciso646", _
            "limits.h", "climits", _
            "locale.h", "clocale", _
            "math.h", "cmath", _
            "setjmp.h", "csetjmp", _
            "signal.h", "csignal", _
            "stdarg.h", "cstdarg", _
            "stddef.h", "cstddef", _
            "stdio.h", "cstdio", _
            "stdlib.h", "cstdlib", _
            "string.h", "cstring", _
            "time.h", "ctime", _
            "wchar.h", "cwchar", _
            "wctype.h", "cwctype"}

        Dim result As Boolean = False
        For i As Integer = LBound(headers) To UBound(headers) Step 2
            Dim pattern As String = "\#include:b*[""\<]" + headers(i) + "[\>""]"
            Dim replace As String = "\#include <" + headers(i + 1) + ">"
            result = doc.ReplacePattern(pattern, replace, vsFindOptions.vsFindOptionsRegularExpression) Or result
        Next
        Return result
    End Function

    Public Function DoCleanCode() As Boolean

        Dim doc As TextDocument
        doc = DTE.ActiveDocument.Object
        Dim result As Boolean = False

        DTE.UndoContext.Open("CleanCode")
        Try
            With doc
                result = TranslateTabsToSpaces(doc) Or result
                result = TrimTrailingWhiteSpaces(doc) Or result
                result = .ReplacePattern("\#endif.*\/\/.*$", "\#endif", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("^:b*\/\/\/\/\/\/\/\/\/\/*\n\n", "", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("^:b*\/\/\/\/\/.*\/\/\/\/\/*$", "", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("^:b*\/\/----------*\n", "", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("\#ifndef Q_MOC_RUN.*\n{\#include.*\n}\#endif.*\n", "#if !defined(Q_MOC_RUN)" + vbNewLine + "\1#endif" + vbNewLine, vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("\#ifndef.*\n{\#include.*\n}\#endif.*\n", "\1", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("^\n\n*", vbNewLine, vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("(void)", "()") Or result
                result = .ReplacePattern("TEST{(_F|_P)*}\({.*},:b*[tT]est_*{.*}\)", "TEST\1(\2, \3)", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("\n\#if defined\(_MSC_VER\) \&\& \(_MSC_VER \>\= [0-9]*\)\n\#:b*pragma once\n\#endif", "#pragma once", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("<NULL>", "nullptr", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = .ReplacePattern("{:a}= ", "\1 = ", vsFindOptions.vsFindOptionsRegularExpression) Or result
                result = TranslateCHeadersToCppHeaders(doc) Or result
            End With

            ' Includes a blank line at end of file
            Dim editPoint As EditPoint = doc.StartPoint.CreateEditPoint
            editPoint.EndOfDocument()
            If editPoint.LineLength <> 0 Then
                editPoint.Insert(vbNewLine)
                result = True
            Else
                editPoint.LineUp(1)
                While editPoint.LineLength = 0
                    editPoint.Delete(1)
                    editPoint.EndOfDocument()
                    If editPoint.LineLength <> 0 Or editPoint.AtStartOfDocument Then
                        Exit While
                    End If
                    editPoint.LineUp(1)
                End While
            End If
        Finally
            DTE.UndoContext.Close()
        End Try

        Return result
    End Function

End Module
