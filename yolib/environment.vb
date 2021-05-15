﻿Public Class [environment]
    Public ReadOnly Property username() As String
        Get
            Return System.Environment.UserName
        End Get
    End Property
    Public ReadOnly Property userdomainname() As String
        Get
            Return System.Environment.UserDomainName
        End Get
    End Property
    Public ReadOnly Property stacktrace() As String
        Get
            Return System.Environment.StackTrace
        End Get
    End Property
    Public ReadOnly Property osversion() As String
        Get
            Return System.Environment.OSVersion.ToString
        End Get
    End Property
    Public ReadOnly Property machinename() As String
        Get
            Return System.Environment.MachineName
        End Get
    End Property
    Public ReadOnly Property is64bitprocess() As String
        Get
            Return System.Environment.Is64BitProcess
        End Get
    End Property
    Public ReadOnly Property is64bitos() As String
        Get
            Return System.Environment.Is64BitOperatingSystem
        End Get
    End Property
    Public ReadOnly Property commandline() As String
        Get
            Return System.Environment.CommandLine
        End Get
    End Property

    Public ReadOnly Property crdir() As String
        Get
            Return System.Environment.CurrentDirectory
        End Get
    End Property

    Public ReadOnly Property appdir() As String
        Get
            Return My.Application.Info.DirectoryPath
        End Get
    End Property

    Public ReadOnly Property sysdir() As String
        Get
            Return System.Environment.SystemDirectory
        End Get
    End Property
End Class
