Imports System.Runtime.InteropServices

Public Class AccurateTimer
    Implements IDisposable

    Private Delegate Sub TimerEventDelegate(uTimerID As UInteger, uMsg As UInteger, dwUser As UIntPtr, dw1 As UIntPtr, dw2 As UIntPtr)

    <DllImport("winmm.dll")>
    Private Shared Function timeBeginPeriod(uMilliseconds As UInteger) As UInteger
    End Function

    <DllImport("winmm.dll")>
    Private Shared Function timeEndPeriod(uMilliseconds As UInteger) As UInteger
    End Function

    <DllImport("winmm.dll")>
    Private Shared Function timeSetEvent(
        uDelay As UInteger,
        uResolution As UInteger,
        lpTimeProc As TimerEventDelegate,
        dwUser As UIntPtr,
        fuEvent As UInteger
    ) As UInteger
    End Function

    <DllImport("winmm.dll")>
    Private Shared Function timeKillEvent(uTimerID As UInteger) As UInteger
    End Function

    Private Const TIME_PERIODIC As UInteger = 1
    Private Const TIME_CALLBACK_FUNCTION As UInteger = &H0

    Private ReadOnly _interval As UInteger
    Private ReadOnly _callback As Action
    Private _timerId As UInteger
    Private _timerProc As TimerEventDelegate
    Private _running As Boolean

    Public Sub New(intervalMs As UInteger, callback As Action)
        _interval = intervalMs
        _callback = callback
        _timerProc = AddressOf TimerTick
    End Sub

    Public Sub Start()
        If _running Then Return
        timeBeginPeriod(1) ' request 1 ms system timer resolution
        _timerId = timeSetEvent(_interval, 0, _timerProc, UIntPtr.Zero, TIME_PERIODIC Or TIME_CALLBACK_FUNCTION)
        _running = True
    End Sub

    Public Sub [Stop]()
        If Not _running Then Return
        timeKillEvent(_timerId)
        timeEndPeriod(1)
        _running = False
    End Sub

    Private Sub TimerTick(uTimerID As UInteger, uMsg As UInteger, dwUser As UIntPtr, dw1 As UIntPtr, dw2 As UIntPtr)
        _callback?.Invoke()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
    End Sub
End Class

Public Class FastBitmapRenderer
    <DllImport("user32.dll")>
    Private Shared Function GetDC(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ReleaseDC(hWnd As IntPtr, hDC As IntPtr) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function CreateCompatibleDC(hDC As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function SelectObject(hDC As IntPtr, hObject As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteDC(hDC As IntPtr) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function BitBlt(hdcDest As IntPtr, x As Integer, y As Integer, width As Integer, height As Integer, hdcSrc As IntPtr, xSrc As Integer, ySrc As Integer, dwRop As Integer) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(hObject As IntPtr) As Boolean
    End Function
    <DllImport("gdi32.dll", SetLastError:=True)>
    Private Shared Function StretchBlt(
    hdcDest As IntPtr,
    xDest As Integer,
    yDest As Integer,
    wDest As Integer,
    hDest As Integer,
    hdcSrc As IntPtr,
    xSrc As Integer,
    ySrc As Integer,
    wSrc As Integer,
    hSrc As Integer,
    dwRop As Integer
) As Boolean
    End Function
    Private Const SRCCOPY As Integer = &HCC0020
    Public Shared Sub RenderBitmapDirectly(bitmap As Bitmap, x As Integer, y As Integer)
        Dim screenDC As IntPtr = GetDC(IntPtr.Zero)
        If screenDC = IntPtr.Zero Then Exit Sub
        Dim memDC As IntPtr = CreateCompatibleDC(screenDC)
        If memDC = IntPtr.Zero Then
            ReleaseDC(IntPtr.Zero, screenDC)
            Exit Sub
        End If
        Dim hBitmap As IntPtr = bitmap.GetHbitmap()
        Dim oldObj As IntPtr = SelectObject(memDC, hBitmap)
        BitBlt(screenDC, x, y, bitmap.Width, bitmap.Height, memDC, 0, 0, SRCCOPY)
        SelectObject(memDC, oldObj)
        DeleteObject(hBitmap)
        DeleteDC(memDC)
        ReleaseDC(IntPtr.Zero, screenDC)
    End Sub
    Public Shared Sub RenderBitmapOnForm(handle As IntPtr, bitmap As Bitmap, x As Integer, y As Integer)
        Dim screenDC As IntPtr = GetDC(handle)
        If screenDC = IntPtr.Zero Then Exit Sub
        Dim memDC As IntPtr = CreateCompatibleDC(screenDC)
        If memDC = IntPtr.Zero Then
            ReleaseDC(IntPtr.Zero, screenDC)
            Exit Sub
        End If
        Dim hBitmap As IntPtr = bitmap.GetHbitmap()
        Dim oldObj As IntPtr = SelectObject(memDC, hBitmap)
        BitBlt(screenDC, x, y, bitmap.Width, bitmap.Height, memDC, 0, 0, SRCCOPY)
        SelectObject(memDC, oldObj)
        DeleteObject(hBitmap)
        DeleteDC(memDC)
        ReleaseDC(IntPtr.Zero, screenDC)
    End Sub
    Public Shared Sub RenderBitmapStretched(
    handle As IntPtr,
    bitmap As Bitmap,
    x As Integer,
    y As Integer,
    width As Integer,
    height As Integer)

        Dim screenDC As IntPtr = GetDC(handle)
        If screenDC = IntPtr.Zero Then Exit Sub

        Dim memDC As IntPtr = CreateCompatibleDC(screenDC)
        If memDC = IntPtr.Zero Then
            ReleaseDC(handle, screenDC)
            Exit Sub
        End If

        Dim hBitmap As IntPtr = bitmap.GetHbitmap()
        Dim oldObj As IntPtr = SelectObject(memDC, hBitmap)

        StretchBlt(
        screenDC,
        x, y, width, height,
        memDC,
        0, 0, bitmap.Width, bitmap.Height,
        SRCCOPY)

        SelectObject(memDC, oldObj)
        DeleteObject(hBitmap)
        DeleteDC(memDC)
        ReleaseDC(handle, screenDC)
    End Sub
    Public Shared Sub RenderBitmapStretched(
    handle As IntPtr,
    bitmap As Bitmap,
    x As Integer,
    y As Integer,
    size As Size)

        Dim screenDC As IntPtr = GetDC(handle)
        If screenDC = IntPtr.Zero Then Exit Sub

        Dim memDC As IntPtr = CreateCompatibleDC(screenDC)
        If memDC = IntPtr.Zero Then
            ReleaseDC(handle, screenDC)
            Exit Sub
        End If

        Dim hBitmap As IntPtr = bitmap.GetHbitmap()
        Dim oldObj As IntPtr = SelectObject(memDC, hBitmap)

        StretchBlt(
        screenDC,
        x, y, size.Width, size.Height,
        memDC,
        0, 0, bitmap.Width, bitmap.Height,
        SRCCOPY)

        SelectObject(memDC, oldObj)
        DeleteObject(hBitmap)
        DeleteDC(memDC)
        ReleaseDC(handle, screenDC)
    End Sub
    Public Shared Sub StretchBlitBitmapToBitmap(
        srcBitmap As Bitmap,
        destBitmap As Bitmap,
        destX As Integer,
        destY As Integer,
        destWidth As Integer,
        destHeight As Integer)

        Dim srcDC As IntPtr = IntPtr.Zero
        Dim srcHbm As IntPtr = IntPtr.Zero
        Dim oldSrc As IntPtr = IntPtr.Zero

        Dim g As Graphics = Nothing
        Dim destDC As IntPtr = IntPtr.Zero

        Try
            ' Create source DC
            srcDC = CreateCompatibleDC(IntPtr.Zero)
            srcHbm = srcBitmap.GetHbitmap()
            oldSrc = SelectObject(srcDC, srcHbm)

            ' Get destination HDC from bitmap
            g = Graphics.FromImage(destBitmap)
            destDC = g.GetHdc()

            'SetStretchBltMode(destDC, COLORONCOLOR)

            StretchBlt(
                destDC,
                destX, destY, destWidth, destHeight,
                srcDC,
                0, 0, srcBitmap.Width, srcBitmap.Height,
                SRCCOPY)

        Finally
            ' Cleanup
            If destDC <> IntPtr.Zero Then g.ReleaseHdc(destDC)
            If g IsNot Nothing Then g.Dispose()

            If oldSrc <> IntPtr.Zero Then SelectObject(srcDC, oldSrc)
            If srcHbm <> IntPtr.Zero Then DeleteObject(srcHbm)
            If srcDC <> IntPtr.Zero Then DeleteDC(srcDC)
        End Try

    End Sub
End Class
