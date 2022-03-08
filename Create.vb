Imports System.Runtime.InteropServices

Public Class Create
#Region "   CreateIconIndirect"

    Private Structure IconInfo
        Public fIcon As Boolean
        Public xHotspot As Integer
        Public yHotspot As Integer
        Public hbmMask As IntPtr
        Public hbmColor As IntPtr
    End Structure

    <DllImport("user32.dll", EntryPoint:="CreateIconIndirect")>
    Private Shared Function CreateIconIndirect(iconInfo As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function DestroyIcon(handle As IntPtr) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Public Shared Function DeleteObject(hObject As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' CreateCursor
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <returns>custom Cursor</returns>
    ''' <remarks>creates a custom cursor from a bitmap</remarks>
    Public Shared Function CreateCursor(bmp As Bitmap, xHotspot As Integer, yHotspot As Integer) As Cursor
        'Setup the Cursors IconInfo
        Dim tmp As New IconInfo With {
            .xHotspot = xHotspot,
            .yHotspot = yHotspot,
            .fIcon = False,
            .hbmMask = bmp.GetHbitmap(),
            .hbmColor = bmp.GetHbitmap()
        }

        'Create the Pointer for the Cursor Icon
        Dim pnt As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(tmp))
        Marshal.StructureToPtr(tmp, pnt, True)
        Dim curPtr As IntPtr = CreateIconIndirect(pnt)

        'Clean Up
        DestroyIcon(pnt)
        DeleteObject(tmp.hbmMask)
        DeleteObject(tmp.hbmColor)

        Return New Cursor(curPtr)
    End Function

#End Region
End Class
