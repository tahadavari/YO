﻿Imports YOCA

Public Class condproc

    Dim sncond(0) As singlecondition
    Dim branchinfo As branchtargetinfo
    Structure singlecondition
        Dim lvalue As xmlunpkd.linecodestruc
        Dim optcond As tokenhared.token
        Dim rvalue As xmlunpkd.linecodestruc
        Dim sepopt As tokenhared.token
    End Structure

    Structure branchtargetinfo
        Dim truebranch, falsebranch As String
    End Structure
    Enum sepstate
        lval
        opt
        rval
        sepopt
    End Enum
    Friend Shared Function get_condition(clinecodestruc As xmlunpkd.linecodestruc(), Optional stindex As Integer = 0) As xmlunpkd.linecodestruc()
        Dim conditioncodestruc(0) As xmlunpkd.linecodestruc
        If clinecodestruc.Length < 3 Then
            dserr.new_error(conserr.errortype.SYNTAXERROR, clinecodestruc(0).line, ilbodybulider.path, authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(clinecodestruc(0)), clinecodestruc(0).value))
        End If

        If clinecodestruc(stindex).tokenid <> tokenhared.token.PRSTART Then
            dserr.new_error(conserr.errortype.SYNTAXERROR, clinecodestruc(0).line, ilbodybulider.path, authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(clinecodestruc(0)), clinecodestruc(0).value))
        End If

        Dim cid As Integer = 0
        For index = stindex + 1 To clinecodestruc.Length - 1
            If clinecodestruc(index).tokenid <> tokenhared.token.PREND Then
                conditioncodestruc(cid) = clinecodestruc(index)
                cid += 1
                Array.Resize(conditioncodestruc, cid + 1)
            Else
                Exit For
            End If
        Next
        Array.Resize(conditioncodestruc, cid)
        Return conditioncodestruc
    End Function

    Friend Shared Function get_block_body(clinecodestruc() As xmlunpkd.linecodestruc) As xmlunpkd.linecodestruc
        For index = 0 To clinecodestruc.Length - 1
            If clinecodestruc(index).tokenid = tokenhared.token.BLOCKOP Then
                Return clinecodestruc(index)
            End If
        Next

        dserr.new_error(conserr.errortype.SYNTAXERROR, clinecodestruc(0).line, ilbodybulider.path, authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(clinecodestruc(0)), clinecodestruc(0).value) & vbCrLf & "Block body not found.")
        Return Nothing
    End Function

    Public Sub New(branchinfo As branchtargetinfo)
        sncond(0) = New singlecondition
        Me.branchinfo = branchinfo
    End Sub

    Public Sub set_condition(ByRef _ilmethod As ilformat._ilmethodcollection, condlinecodestruc As xmlunpkd.linecodestruc(), Optional deftruebr As Boolean = True)
        'coutputdata.print_token(condlinecodestruc)
        sep_condition(condlinecodestruc)
        load_condition_ref(_ilmethod, deftruebr)
    End Sub

    Private Sub load_condition_ref(ByRef _ilmethod As ilformat._ilmethodcollection, deftruebr As Boolean)
        For index = 0 To sncond.Length - 1
            Dim ltype As String = String.Empty
            Dim rtype As String = String.Empty
            servinterface.get_dt_proc(_ilmethod, sncond(index).lvalue, ltype)
            servinterface.get_dt_proc(_ilmethod, sncond(index).rvalue, rtype)
            Dim illdloc As New illdloc(_ilmethod)
            'TODO : check data types and warning
            _ilmethod = illdloc.load_single_in_stack(ltype, sncond(index).lvalue)
            _ilmethod = illdloc.load_single_in_stack(rtype, sncond(index).rvalue)
            load_expression(_ilmethod, sncond(index), ltype, rtype, deftruebr)
        Next
    End Sub

    Private Sub load_expression(ByRef _ilmethod As ilformat._ilmethodcollection, sncond As singlecondition, ltype As String, rtype As String, deftruebr As Boolean)
        Select Case sncond.optcond
            Case tokenhared.token.CONDEQ    '==
                Select Case sncond.sepopt
                    Case tokenhared.token.ANDLOGIC
                        If ltype = conrex.STRING AndAlso rtype = conrex.STRING Then
                            Dim param As New ArrayList
                            param.Add(conrex.STRING)
                            param.Add(conrex.STRING)
                            cil.call_method(_ilmethod.codes, "bool", "mscorlib", "System.String", "op_Inequality", param)
                            cil.branch_if_true(_ilmethod.codes, branchinfo.falsebranch)
                        Else
                            cil.bne(_ilmethod.codes, branchinfo.falsebranch)
                        End If
                    Case tokenhared.token.ORLOGIC
                        cil.beq(_ilmethod.codes, branchinfo.truebranch)
                    Case tokenhared.token.UNDEFINED
                        If deftruebr Then
                            cil.beq(_ilmethod.codes, branchinfo.truebranch)
                        ElseIf ltype = conrex.STRING AndAlso rtype = conrex.STRING Then
                            Dim param As New ArrayList
                                param.Add(conrex.STRING)
                                param.Add(conrex.STRING)
                                cil.call_method(_ilmethod.codes, "bool", "mscorlib", "System.String", "op_Inequality", param)
                                cil.branch_if_true(_ilmethod.codes, branchinfo.falsebranch)
                            Else
                                cil.bne(_ilmethod.codes, branchinfo.falsebranch)
                            End If
                End Select
            Case tokenhared.token.LKOEQ     '>=
                Select Case sncond.sepopt
                    Case tokenhared.token.ANDLOGIC
                        cil.blt(_ilmethod.codes, branchinfo.falsebranch)
                    Case tokenhared.token.ORLOGIC
                        cil.bge(_ilmethod.codes, branchinfo.truebranch)
                    Case tokenhared.token.UNDEFINED
                        If deftruebr Then
                            cil.bge(_ilmethod.codes, branchinfo.truebranch)
                        Else
                            cil.blt(_ilmethod.codes, branchinfo.falsebranch)
                        End If
                End Select
            Case tokenhared.token.L2KO      '>>
                Select Case sncond.sepopt
                    Case tokenhared.token.ANDLOGIC
                        cil.ble(_ilmethod.codes, branchinfo.falsebranch)
                    Case tokenhared.token.ORLOGIC
                        cil.bgt(_ilmethod.codes, branchinfo.truebranch)
                    Case tokenhared.token.UNDEFINED
                        If deftruebr Then
                            cil.bgt(_ilmethod.codes, branchinfo.truebranch)
                        Else
                            cil.ble(_ilmethod.codes, branchinfo.falsebranch)
                        End If
                End Select
            Case tokenhared.token.RKOEQ     '<=
                Select Case sncond.sepopt
                    Case tokenhared.token.ANDLOGIC
                        cil.bgt(_ilmethod.codes, branchinfo.falsebranch)
                    Case tokenhared.token.ORLOGIC
                        cil.ble(_ilmethod.codes, branchinfo.truebranch)
                    Case tokenhared.token.UNDEFINED
                        If deftruebr Then
                            cil.ble(_ilmethod.codes, branchinfo.truebranch)
                        Else
                            cil.bgt(_ilmethod.codes, branchinfo.falsebranch)
                        End If
                End Select
            Case tokenhared.token.R2KO      '<<
                Select Case sncond.sepopt
                    Case tokenhared.token.ANDLOGIC
                        cil.bge(_ilmethod.codes, branchinfo.falsebranch)
                    Case tokenhared.token.ORLOGIC
                        cil.blt(_ilmethod.codes, branchinfo.truebranch)
                    Case tokenhared.token.UNDEFINED
                        If deftruebr Then
                            cil.blt(_ilmethod.codes, branchinfo.truebranch)
                        Else
                            cil.bge(_ilmethod.codes, branchinfo.falsebranch)
                        End If
                End Select
            Case tokenhared.token.LRNA      '<>
                If ltype = conrex.STRING AndAlso rtype = conrex.STRING Then
                    Dim param As New ArrayList
                    param.Add(conrex.STRING)
                    param.Add(conrex.STRING)
                    If sncond.sepopt = tokenhared.token.ANDLOGIC Then
                        cil.call_method(_ilmethod.codes, "bool", "mscorlib", "System.String", "op_Equality", param)
                        cil.branch_if_true(_ilmethod.codes, branchinfo.falsebranch)
                    Else
                        cil.call_method(_ilmethod.codes, "bool", "mscorlib", "System.String", "op_Inequality", param)
                        cil.branch_if_true(_ilmethod.codes, branchinfo.truebranch)
                    End If

                Else
                    Select Case sncond.sepopt
                        Case tokenhared.token.ANDLOGIC
                            cil.beq(_ilmethod.codes, branchinfo.falsebranch)
                        Case tokenhared.token.ORLOGIC
                            cil.bne(_ilmethod.codes, branchinfo.truebranch)
                        Case tokenhared.token.UNDEFINED
                            If deftruebr Then
                                cil.bne(_ilmethod.codes, branchinfo.truebranch)
                            Else
                                cil.beq(_ilmethod.codes, branchinfo.falsebranch)
                            End If
                    End Select
                End If
        End Select
    End Sub
    Private Sub sep_condition(condlinecodestruc As xmlunpkd.linecodestruc())
        Dim ibar As Integer = 0
        Dim opt, sepopt As String
        opt = conrex.NULL
        sepopt = conrex.NULL
        Dim spstate As New sepstate
        spstate = sepstate.lval
        For index = 0 To condlinecodestruc.Length - 1
            Select Case spstate
                Case sepstate.lval
                    sncond(ibar).lvalue = condlinecodestruc(index)
                    sncond(ibar).sepopt = tokenhared.token.UNDEFINED
                    spstate = sepstate.opt
                Case sepstate.opt
                    opt &= condlinecodestruc(index).value
                    If opt.Length = 2 Then
                        get_condition_opt(sncond(ibar).optcond, opt, condlinecodestruc(index))
                        spstate = sepstate.rval
                        opt = conrex.NULL
                    ElseIf opt.Length > 2 Then
                        dserr.new_error(conserr.errortype.SYNTAXERROR, condlinecodestruc(index).line, ilbodybulider.path, "Operator expected , '" & opt & "' The operator could not be identified(conditional operators)." & vbCrLf & authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(condlinecodestruc(index - 1)), condlinecodestruc(index - 1).value), "Use '>>' , '>=' , '==' , '<>' , ...")
                    End If
                Case sepstate.rval
                    sncond(ibar).rvalue = condlinecodestruc(index)
                    spstate = sepstate.sepopt
                Case sepstate.sepopt
                    sepopt &= condlinecodestruc(index).value
                    If sepopt.Length >= 2 Then
                        Select Case sepopt
                            Case "&&"
                                sncond(ibar).sepopt = tokenhared.token.ANDLOGIC
                            Case "||"
                                sncond(ibar).sepopt = tokenhared.token.ORLOGIC
                            Case Else
                                dserr.new_error(conserr.errortype.SYNTAXERROR, condlinecodestruc(index).line, ilbodybulider.path, "Expression expected , '" & sepopt & "' The operator could not be identified." & vbCrLf & authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(condlinecodestruc(index)), condlinecodestruc(index).value), "Use '||' or '&&'")
                        End Select
                        Array.Resize(sncond, sncond.Length + 1)
                        ibar += 1
                        sepopt = conrex.NULL
                        spstate = sepstate.lval
                    End If
            End Select
        Next

        If sepopt <> conrex.NULL Then
            dserr.new_error(conserr.errortype.SYNTAXERROR, condlinecodestruc(condlinecodestruc.Length - 1).line, ilbodybulider.path, "Expression expected , '" & sepopt & "' The operator could not be identified." & vbCrLf & authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(condlinecodestruc(condlinecodestruc.Length - 1)), sepopt), "Use '||' or '&&'")
        End If
    End Sub

    Private Sub get_condition_opt(ByRef optcond As tokenhared.token, optval As String, linecodestruc As xmlunpkd.linecodestruc)
        optcond = tokenhared.check_sym(optval)
        For index = 0 To tokenhared.conditiontoken.Length - 1
            If optval = tokenhared.conditiontoken(index) Then
                Return
            End If
        Next

        dserr.new_error(conserr.errortype.SYNTAXERROR, linecodestruc.line, ilbodybulider.path, "Expression expected , '" & optval & "' The operator could not be identified." & vbCrLf & authfunc.get_line_error(ilbodybulider.path, servinterface.get_target_info(linecodestruc), linecodestruc.value))
    End Sub
End Class
