Public Structure Resolution
    Public Width As Integer
    Public Height As Integer
End Structure

Public Class Converter
    Public inputfile As String
    Public outputfile As String
    Public outputpath As String
    Public outputformat As Integer

    Public starttime As Integer
    Public length As Integer
    Public size As Double
    Public twopass As Boolean

    Public changeresolution As Boolean
    Public videoresolution As Resolution
    Public videorate As Integer
    Public videoquality As Integer

    Public audiorate As Integer = 32
    Public mono As Boolean = True

    Private Function getresolution() As Resolution
        Dim ffprobe As New Process
        Dim ffprobeoptions As New ProcessStartInfo
        ffprobeoptions.FileName = "ffprobe"
        ffprobeoptions.Arguments = "-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 " + ControlChars.Quote + inputfile + ControlChars.Quote
        ffprobeoptions.UseShellExecute = False
        ffprobeoptions.WindowStyle = ProcessWindowStyle.Hidden
        ffprobeoptions.CreateNoWindow = True
        ffprobeoptions.RedirectStandardError = True
        ffprobeoptions.RedirectStandardOutput = True
        ffprobe.StartInfo = ffprobeoptions
        ffprobe.Start()
        Dim ffprobeoutput As String
        Using streamreader As System.IO.StreamReader = ffprobe.StandardOutput
            ffprobeoutput = streamreader.ReadToEnd.ToString
        End Using
        Dim resstr As List(Of String) = ffprobeoutput.Split("x").ToList
        Dim res As New Resolution
        If resstr.Count < 2 Then
            MsgBox("Die Videoauflösung konnte nicht bestimmt werden. Stell sicher, dass ffprobe korrekt installiert wurde.")
            res.Width = 640
            res.Height = 480
            Return res
        End If
        res.Width = CInt(resstr.Item(0))
        res.Height = CInt(resstr.Item(1))
        Return res
    End Function

    Private Function calculateresolution() As Resolution
        Dim heightfactor As Double = videoresolution.Height / videoresolution.Width
        Dim newresolution As Resolution
        newresolution.Width = 1052
        While (CDbl(videorate) / (CDbl(newresolution.Width) * CDbl(newresolution.Width) * heightfactor)) < CDbl(1 / 1024)
            newresolution.Width -= 1
        End While
        newresolution.Width = newresolution.Width - (CInt(newresolution.Width Mod 2))
        newresolution.Height = CInt(newresolution.Width * heightfactor) - (CInt(newresolution.Width * heightfactor) Mod 2)

        If newresolution.Width > videoresolution.Width Then
            Return videoresolution
        Else
            Return newresolution
        End If
    End Function

    Private Function getcrfbitrate() As Integer
        Dim ffmpeg As New Process
        Dim ffmpegoptions As New ProcessStartInfo
        ffmpegoptions.FileName = "ffmpeg"
        'Inputsettings
        ffmpegoptions.Arguments = "-i " + ControlChars.Quote + inputfile + ControlChars.Quote + " "
        ffmpegoptions.Arguments += "-ss " + CStr(starttime) + " "
        If length <> -1 Then
            ffmpegoptions.Arguments += "-t " + CStr(length - 1) + ".9 "
        End If
        'Videosettings
        ffmpegoptions.Arguments += "-c:v libx264 "
        ffmpegoptions.Arguments += "-profile:v main -level 4.0 "
        ffmpegoptions.Arguments += "-movflags faststart "
        ffmpegoptions.Arguments += "-pix_fmt yuv420p "
        ffmpegoptions.Arguments += "-crf 28 "
        'Filterqueue
        ffmpegoptions.Arguments += "-vf "
        If changeresolution = True Then
            ffmpegoptions.Arguments += "scale=" + CStr(calculateresolution.Width) + ":" + CStr(calculateresolution.Height) + ","
        End If
        If getfps() > 30 Then
            ffmpegoptions.Arguments += "fps=fps=30,"
        End If
        ffmpegoptions.Arguments += "null "
        'Audiosettings
        ffmpegoptions.Arguments += "-c:a aac "
        ffmpegoptions.Arguments += "-b:a 96k "
        'Outputsettings
        ffmpegoptions.Arguments += ".temp.mp4 "
        ffmpegoptions.Arguments += "-y "
        ffmpegoptions.UseShellExecute = True
        ffmpegoptions.WindowStyle = ProcessWindowStyle.Normal
        ffmpegoptions.CreateNoWindow = False
        ffmpeg.StartInfo = ffmpegoptions
        ffmpeg.Start()
        ffmpeg.WaitForExit()

        Dim ffprobe As New Process
        Dim ffprobeoptions As New ProcessStartInfo
        ffprobeoptions.FileName = "ffprobe"
        ffprobeoptions.Arguments = "-v error -select_streams v:0 -show_entries stream=bit_rate -of csv=p=0 .temp.mp4"
        ffprobeoptions.UseShellExecute = False
        ffprobeoptions.WindowStyle = ProcessWindowStyle.Hidden
        ffprobeoptions.CreateNoWindow = True
        ffprobeoptions.RedirectStandardError = True
        ffprobeoptions.RedirectStandardOutput = True
        ffprobe.StartInfo = ffprobeoptions
        ffprobe.Start()
        Dim ffprobeoutput As String
        Using streamreader As System.IO.StreamReader = ffprobe.StandardOutput
            ffprobeoutput = streamreader.ReadToEnd.ToString
        End Using

        IO.File.Delete(Windows.Forms.Application.StartupPath + "\.temp.mp4")
        If IsNumeric(ffprobeoutput) Then
            Return CInt(Val(ffprobeoutput) / 1000)
        Else
            MsgBox("Die CRF Bitrate konnte nicht bestimmt werden. Stell sicher, dass ffprobe korrekt installiert wurde.", MsgBoxStyle.Critical)
            Return 10000
        End If
    End Function

    Private Function getfps() As Integer
        Dim ffprobe As New Process
        Dim ffprobeoptions As New ProcessStartInfo
        ffprobeoptions.FileName = "ffprobe"
        ffprobeoptions.Arguments = "-v error -select_streams v:0 -show_entries stream=r_frame_rate -of csv=p=0 " + ControlChars.Quote + inputfile + ControlChars.Quote
        ffprobeoptions.UseShellExecute = False
        ffprobeoptions.WindowStyle = ProcessWindowStyle.Hidden
        ffprobeoptions.CreateNoWindow = True
        ffprobeoptions.RedirectStandardError = True
        ffprobeoptions.RedirectStandardOutput = True
        ffprobe.StartInfo = ffprobeoptions
        ffprobe.Start()
        Dim ffprobeoutput As String
        Using streamreader As System.IO.StreamReader = ffprobe.StandardOutput
            ffprobeoutput = streamreader.ReadToEnd.ToString
        End Using
        If ffprobeoutput.Contains("/") Then
            Dim dividend As Integer = ffprobeoutput.Split("/")(0)
            Dim divisor As Integer = ffprobeoutput.Split("/")(1)
            Dim result As Integer = CInt(Math.Round(dividend / divisor))
            Return result
        Else
            MsgBox("Die Framerate konnte nicht bestimmt werden. Stell sicher, dass ffprobe korrekt installiert wurde.", MsgBoxStyle.Critical)
            Return 30
        End If
    End Function

    Public Function getlength() As Integer
        Dim ffprobe As New Process
        Dim ffprobeoptions As New ProcessStartInfo
        ffprobeoptions.FileName = "ffprobe"
        ffprobeoptions.Arguments = "-v error -select_streams v:0 -show_entries stream=duration -of csv=p=0 " + ControlChars.Quote + inputfile + ControlChars.Quote
        ffprobeoptions.UseShellExecute = False
        ffprobeoptions.WindowStyle = ProcessWindowStyle.Hidden
        ffprobeoptions.CreateNoWindow = True
        ffprobeoptions.RedirectStandardError = True
        ffprobeoptions.RedirectStandardOutput = True
        ffprobe.StartInfo = ffprobeoptions
        ffprobe.Start()
        Dim ffprobeoutput As String
        Using streamreader As System.IO.StreamReader = ffprobe.StandardOutput
            ffprobeoutput = streamreader.ReadToEnd.ToString
        End Using
        Return CInt(Val(ffprobeoutput))
    End Function

    Public Sub convert()
        Select Case outputformat
            Case 0
                createmp4()
            Case 1
                createwebm()
            Case 2
                createpng()
        End Select
    End Sub

    Private Sub createwebm()
        'Calculate videorate
        If length = -1 Then
            If twopass = True Then
                videorate = CInt(0.96 * ((size * 8000) / (getlength() - starttime) - CDbl(audiorate))) '4% Overhead
            Else
                videorate = CInt(0.92 * ((size * 8000) / (getlength() - starttime) - CDbl(audiorate))) '8% Overhead/Offset
            End If
        Else
            If twopass = True Then
                videorate = CInt(0.96 * ((size * 8000) / CDbl(length) - CDbl(audiorate))) '4% Overhead
            Else
                videorate = CInt(0.92 * ((size * 8000) / CDbl(length) - CDbl(audiorate))) '8% Overhead/Offset
            End If
        End If

        'Get resolution
        videoresolution = getresolution()

        'Check if videorate is good enought
        If videorate < 150 Then
            MsgBox("Die Videobitrate ist mit " + CStr(videorate) + "kbit/s zu klein. Das kannst du tun:" + vbNewLine + "- Die Zielgröße vergrößern" + vbNewLine + "- Den Ausschnitt verkürzen" + vbNewLine + "- Die Audioqualität heruntersetzen")
            Return
        End If

        'Check if changeresolution is false but changeresolution is needed
        If changeresolution = False And (CDbl(videorate) / (CDbl(videoresolution.Width) * CDbl(videoresolution.Height))) < CDbl(1 / 1024) Then
            MsgBox("Bitte aktiviere das automatische Anpassen des Auflösung.", MsgBoxStyle.Information)
            Return
        End If

        Dim ffmpeg As New Process
        Dim ffmpegoptions As New ProcessStartInfo

        '-------- PASS 1 --------
        'Inputsettings
        ffmpegoptions.FileName = "ffmpeg"
        ffmpegoptions.Arguments = "-i " + ControlChars.Quote + inputfile + ControlChars.Quote + " "
        ffmpegoptions.Arguments += "-ss " + CStr(starttime) + " "
        If length > 0 Then
            ffmpegoptions.Arguments += "-t " + CStr(length - 1) + ".9 "
        End If
        'Videosettings
        ffmpegoptions.Arguments += "-c:v libvpx "
        ffmpegoptions.Arguments += "-b:v " + CStr(videorate) + "k "
        Select Case videoquality
            Case 0
                ffmpegoptions.Arguments += "-deadline good "
            Case 1
                ffmpegoptions.Arguments += "-deadline best "
        End Select
        'Filterqueue
        ffmpegoptions.Arguments += "-vf "
        If changeresolution = True Then
            ffmpegoptions.Arguments += "scale=" + CStr(calculateresolution.Width) + ":" + CStr(calculateresolution.Height) + ","
        End If
        If getfps() > 30 Then
            ffmpegoptions.Arguments += "fps=fps=30,"
        End If
        ffmpegoptions.Arguments += "null "
        'Audiosettings
        If audiorate = 0 Then
            ffmpegoptions.Arguments += "-an "
        Else
            ffmpegoptions.Arguments += "-c:a libopus "
            ffmpegoptions.Arguments += "-b:a " + CStr(audiorate) + "k "
            If mono = True Then
                ffmpegoptions.Arguments += "-ac 1 "
            End If
        End If
        'Outputsettings
        If twopass = True Then
            ffmpegoptions.Arguments += "-pass 1 -f webm NUL -y"
        Else
            ffmpegoptions.Arguments += ControlChars.Quote + outputfile + ControlChars.Quote + " -y"
        End If
        ffmpegoptions.UseShellExecute = True
        ffmpegoptions.WindowStyle = ProcessWindowStyle.Normal
        ffmpegoptions.CreateNoWindow = False
        ffmpeg.StartInfo = ffmpegoptions
        ffmpeg.Start()
        ffmpeg.WaitForExit()

        If twopass = False Then
            Return
        End If

        '-------- PASS 2 --------
        'Inputsettings
        ffmpegoptions.Arguments = "-i " + ControlChars.Quote + inputfile + ControlChars.Quote + " "
        ffmpegoptions.Arguments += "-ss " + CStr(starttime) + " "
        If length > 0 Then
            ffmpegoptions.Arguments += "-t " + CStr(length - 1) + ".9 "
        End If
        'Videosettings
        ffmpegoptions.Arguments += "-c:v libvpx "
        ffmpegoptions.Arguments += "-b:v " + CStr(videorate) + "k "
        Select Case videoquality
            Case 0
                ffmpegoptions.Arguments += "-deadline good "
            Case 1
                ffmpegoptions.Arguments += "-deadline best "
        End Select
        'Filterqueue
        ffmpegoptions.Arguments += "-vf "
        If changeresolution = True Then
            ffmpegoptions.Arguments += "scale=" + CStr(calculateresolution.Width) + ":" + CStr(calculateresolution.Height) + ","
        End If
        If getfps() > 30 Then
            ffmpegoptions.Arguments += "fps=fps=30,"
        End If
        ffmpegoptions.Arguments += "null "
        'Audiosettings
        If audiorate = 0 Then
            ffmpegoptions.Arguments += "-an "
        Else
            ffmpegoptions.Arguments += "-c:a libopus "
            ffmpegoptions.Arguments += "-b:a " + CStr(audiorate) + "k "
            If mono = True Then
                ffmpegoptions.Arguments += "-ac 1 "
            End If
        End If
        'Outputsettings
        ffmpegoptions.Arguments += "-pass 2 " + ControlChars.Quote + outputfile + ControlChars.Quote + " -y"
        ffmpegoptions.UseShellExecute = True
        ffmpegoptions.WindowStyle = ProcessWindowStyle.Normal
        ffmpegoptions.CreateNoWindow = False
        ffmpeg.StartInfo = ffmpegoptions
        ffmpeg.Start()
        ffmpeg.WaitForExit()

        IO.File.Delete(Windows.Forms.Application.StartupPath + "\ffmpeg2pass-0.log")
    End Sub

    Private Sub createmp4()
        'Calculate videorate
        If length = -1 Then
            videorate = CInt(0.97 * ((size * 8000) / (getlength() - starttime) - CDbl(audiorate)))
        Else
            videorate = CInt(0.97 * ((size * 8000) / CDbl(length) - CDbl(audiorate)))
        End If

        'Get Resolution
        videoresolution = getresolution()

        'Check if videorate is good enought
        If videorate < 150 Then
            MsgBox("Die Videobitrate ist mit " + CStr(videorate) + "kbit/s zu klein. Das kannst du tun:" + vbNewLine + "- Die Zielgröße vergrößern" + vbNewLine + "- Den Ausschnitt verkürzen" + vbNewLine + "- Die Audioqualität heruntersetzen")
            Return
        End If

        'Check if changeresolution is false but changeresolution is needed
        If changeresolution = False And (CDbl(videorate) / (CDbl(videoresolution.Width) * CDbl(videoresolution.Height))) < CDbl(1 / 1024) Then
            MsgBox("Bitte aktiviere das automatische Anpassen des Auflösung.", MsgBoxStyle.Information)
            Return
        End If

        'Take the minimum bitrate of the calculated bitrate and the bitrate with crf=28, to prevent reconverting
        Dim crfrate As Integer = getcrfbitrate()
        If crfrate < videorate Then
            videorate = crfrate
        End If

        Dim ffmpeg As New Process
        Dim ffmpegoptions As New ProcessStartInfo

        '-------- PASS 1 --------
        'Inputsetting
        ffmpegoptions.FileName = "ffmpeg"
        ffmpegoptions.Arguments = "-i " + ControlChars.Quote + inputfile + ControlChars.Quote + " "
        ffmpegoptions.Arguments += "-ss " + CStr(starttime) + " "
        If length > 0 Then
            ffmpegoptions.Arguments += "-t " + CStr(length - 1) + ".9 "
        End If
        'Videosetting
        ffmpegoptions.Arguments += "-c:v libx264 "
        ffmpegoptions.Arguments += "-profile:v main -level 4.0 "
        ffmpegoptions.Arguments += "-pix_fmt yuv420p "
        ffmpegoptions.Arguments += "-b:v " + CStr(videorate) + "k "
        Select Case videoquality
            Case 0
                ffmpegoptions.Arguments += "-preset fast "
            Case 1
                ffmpegoptions.Arguments += "-preset slower "
        End Select
        'Filterqueue
        ffmpegoptions.Arguments += "-vf "
        If changeresolution = True Then
            ffmpegoptions.Arguments += "scale=" + CStr(calculateresolution.Width) + ":" + CStr(calculateresolution.Height) + ","
        End If
        If getfps() > 30 Then
            ffmpegoptions.Arguments += "fps=fps=30,"
        End If
        ffmpegoptions.Arguments += "null "
        'Audiosetting
        If audiorate = 0 Then
            ffmpegoptions.Arguments += "-an "
        Else
            ffmpegoptions.Arguments += "-c:a aac "
            ffmpegoptions.Arguments += "-b:a " + CStr(audiorate) + "k "
            If mono = True Then
                ffmpegoptions.Arguments += "-ac 1 "
            End If
        End If
        'Outputsetting
        If twopass = True Then
            ffmpegoptions.Arguments += "-pass 1 -f mp4 NUL -y"
        Else
            ffmpegoptions.Arguments += ControlChars.Quote + outputfile + ControlChars.Quote + " -y"
        End If
        ffmpegoptions.UseShellExecute = True
        ffmpegoptions.WindowStyle = ProcessWindowStyle.Normal
        ffmpegoptions.CreateNoWindow = False
        ffmpeg.StartInfo = ffmpegoptions
        ffmpeg.Start()
        ffmpeg.WaitForExit()

        If twopass = False Then
            Return
        End If

        '-------- PASS 2 --------
        'Inputsetting
        ffmpegoptions.Arguments = "-i " + ControlChars.Quote + inputfile + ControlChars.Quote + " "
        ffmpegoptions.Arguments += "-ss " + CStr(starttime) + " "
        If length > 0 Then
            ffmpegoptions.Arguments += "-t " + CStr(length - 1) + ".9 "
        End If
        'Videosetting
        ffmpegoptions.Arguments += "-c:v libx264 "
        ffmpegoptions.Arguments += "-profile:v main -level 4.0 "
        ffmpegoptions.Arguments += "-pix_fmt yuv420p "
        ffmpegoptions.Arguments += "-b:v " + CStr(videorate) + "k "
        Select Case videoquality
            Case 0
                ffmpegoptions.Arguments += "-preset fast "
            Case 1
                ffmpegoptions.Arguments += "-preset slower "
        End Select
        'Filterqueue
        ffmpegoptions.Arguments += "-vf "
        If changeresolution = True Then
            ffmpegoptions.Arguments += "scale=" + CStr(calculateresolution.Width) + ":" + CStr(calculateresolution.Height) + ","
        End If
        If getfps() > 30 Then
            ffmpegoptions.Arguments += "fps=fps=30,"
        End If
        ffmpegoptions.Arguments += "null "
        'Audiosetting
        If audiorate = 0 Then
            ffmpegoptions.Arguments += "-an "
        Else
            ffmpegoptions.Arguments += "-c:a aac "
            ffmpegoptions.Arguments += "-b:a " + CStr(audiorate) + "k "
            If mono = True Then
                ffmpegoptions.Arguments += "-ac 1 "
            End If
        End If
        'Outputsetting
        ffmpegoptions.Arguments += "-pass 2 " + ControlChars.Quote + outputfile + ControlChars.Quote + " -y"
        ffmpegoptions.UseShellExecute = True
        ffmpegoptions.WindowStyle = ProcessWindowStyle.Normal
        ffmpegoptions.CreateNoWindow = False
        ffmpeg.StartInfo = ffmpegoptions
        ffmpeg.Start()
        ffmpeg.WaitForExit()

        IO.File.Delete(Windows.Forms.Application.StartupPath + "\ffmpeg2pass-0.log")
    End Sub

    Private Sub createpng()
        Dim ffmpeg As New Process
        Dim ffmpegoptions As New ProcessStartInfo
        'first pass
        ffmpegoptions.FileName = "ffmpeg"
        ffmpegoptions.Arguments = "-i " + ControlChars.Quote + inputfile + ControlChars.Quote + " "
        ffmpegoptions.Arguments += "-ss " + CStr(starttime) + " "
        ffmpegoptions.Arguments += "-t " + CStr(length) + " "
        ffmpegoptions.Arguments += outputpath + "\Frame%05d.png -y"
        ffmpegoptions.UseShellExecute = True
        ffmpegoptions.WindowStyle = ProcessWindowStyle.Normal
        ffmpegoptions.CreateNoWindow = False
        ffmpeg.StartInfo = ffmpegoptions
        ffmpeg.Start()
    End Sub

End Class
