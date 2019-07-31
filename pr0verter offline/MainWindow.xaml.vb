Imports System.Threading
Class MainWindow

    Private currentversion As String = "1.2"
    Private converter As New Converter
    Private ofd As New System.Windows.Forms.OpenFileDialog
    Private ofdresult As New System.Windows.Forms.DialogResult
    Private sfd As New System.Windows.Forms.SaveFileDialog
    Private sfdresult As New System.Windows.Forms.DialogResult
    Private fbd As New System.Windows.Forms.FolderBrowserDialog
    Private fbdresult As New System.Windows.Forms.DialogResult

    Private Function parseseconds(ByRef InputString As String) As Integer
        If IsNumeric(InputString) Then
            Return Convert.ToInt32(InputString)
        End If
        If InputString.Contains(":") Then
            Dim TimeSplit As List(Of String) = InputString.Split(":").ToList
            If TimeSplit.Count = 2 And IsNumeric(TimeSplit.Item(0)) And IsNumeric(TimeSplit.Item(1)) Then
                Return CInt(TimeSplit.Item(0)) * 60 + CInt(TimeSplit.Item(1))
            Else
                MsgBox("Mehrere Doppelpunkte in der Zeitangabe '" + InputString + "' erkannt.", MsgBoxStyle.Critical, "Parser error")
                Return -1
            End If
        End If
        Return -1
    End Function

    Private Sub ButtonOpen_Click(sender As Object, e As RoutedEventArgs) Handles ButtonOpen.Click
        ofd.Title = "Videodatei auswählen"
        ofdresult = ofd.ShowDialog
        If ofdresult <> System.Windows.Forms.DialogResult.OK Then
            Me.Title = "Keine Datei ausgewählt"
            Return
        End If
        Me.Title = ofd.SafeFileName
        converter.inputfile = ofd.FileName
    End Sub

    Private Sub SliderTon_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles SliderTon.ValueChanged
        Select Case SliderTon.Value
            Case 0
                TextBlockTon.Text = "Kein Ton"
            Case 1
                TextBlockTon.Text = "Niedrige Tonqualität"
            Case 2
                TextBlockTon.Text = "Normale Tonqualität"
            Case 3
                TextBlockTon.Text = "Hohe Tonqualität"
        End Select
    End Sub

    Private Sub ButtonUmwandeln_Click(sender As Object, e As RoutedEventArgs) Handles ButtonUmwandeln.Click
        'Show Dialog
        sfdresult = New System.Windows.Forms.DialogResult
        fbdresult = New System.Windows.Forms.DialogResult
        Select Case ComboBoxFormat.SelectedIndex
            Case 0
                sfd.Filter = "MP4|*.mp4"
                sfd.DefaultExt = "mp4"
                sfd.AddExtension = True
                sfdresult = sfd.ShowDialog
            Case 1
                sfd.Filter = "WebM|*.webm"
                sfd.DefaultExt = "webm"
                sfd.AddExtension = True
                sfdresult = sfd.ShowDialog
            Case 2
                fbdresult = fbd.ShowDialog
        End Select
        'Set Settings and convert
        If TranslateSetting() = True Then
            Dim convertthread As Thread = New Thread(AddressOf converter.convert)
            convertthread.Start()
        End If
    End Sub

    Private Function TranslateSetting() As Boolean
        'Format Textboxes
        TextBoxDateigroesse.Text = TextBoxDateigroesse.Text.Replace(" ", "")
        TextBoxStartzeit.Text = TextBoxStartzeit.Text.Replace(" ", "")
        TextBoxEndzeit.Text = TextBoxEndzeit.Text.Replace(" ", "")

        'Check Input
        If ComboBoxFormat.SelectedIndex < 2 Then
            If sfdresult <> System.Windows.Forms.DialogResult.OK Then
                Return False
            End If
        Else
            If fbdresult <> System.Windows.Forms.DialogResult.OK Then
                Return False
            End If
        End If
        If ofdresult <> System.Windows.Forms.DialogResult.OK Then
            MsgBox("Du hast noch keine Datei ausgewählt.", MsgBoxStyle.Critical, "IO Error")
            Return False
        End If
        If TextBoxDateigroesse.Text.Length = 0 And ComboBoxFormat.SelectedIndex <> 2 Then
            MsgBox("Du hast noch keine Zielgröße angegeben.", MsgBoxStyle.Critical, "IO Error")
            Return False
        End If

        'Fill Variables
        If TextBoxStartzeit.Text.Length = 0 Then
            converter.starttime = 0
        Else
            converter.starttime = parseseconds(TextBoxStartzeit.Text)
        End If
        If TextBoxEndzeit.Text = "0" Or TextBoxEndzeit.Text.Length = 0 Then
            converter.length = -1
        Else
            converter.length = parseseconds(TextBoxEndzeit.Text) - parseseconds(TextBoxStartzeit.Text)
        End If
        converter.outputformat = ComboBoxFormat.SelectedIndex
        If converter.outputformat <> 2 Then
            converter.outputfile = sfd.FileName
        Else
            converter.outputpath = fbd.SelectedPath
            'Everything OK for PNG
            Return True
        End If
        converter.size = CInt(TextBoxDateigroesse.Text)
        converter.videoquality = ComboBoxPrioritaet.Items.Count - 1 - ComboBoxPrioritaet.SelectedIndex
        converter.changeresolution = CheckBoxAufloesung.IsChecked
        converter.audiorate = SliderTon.Value * 32
        converter.mono = CheckBoxMono.IsChecked
        converter.twopass = CheckBox2pass.IsChecked
        converter.videorate = CInt((CDbl(converter.size * 8000) / CDbl(converter.length)) - CDbl(converter.audiorate))
        If CheckBox2pass.IsChecked = True Then
            converter.videorate = converter.videorate - CInt(0.05 * (converter.videorate + converter.audiorate)) '5% Overhead/Offset
        Else
            converter.videorate = converter.videorate - CInt(0.1 * (converter.videorate + converter.audiorate)) '10% Overhead/Offset
        End If

        'Check Length
        If (converter.length <= 0 And converter.length <> -1) Or (converter.length = -1 And (converter.getlength - converter.starttime) <= 0) Then
            MsgBox("Die Videolänge ist <= 0", MsgBoxStyle.Critical, "IO Error")
            Return False
        End If
        If converter.length > 300 Then
            Select Case MessageBox.Show("Videolänge", "Die Videolänge ist mit " + CStr(converter.length) + " Sekunden über dem Maximum für Uploads. Trotzdem fortfahren?", MessageBoxButton.YesNo)
                Case Windows.Forms.DialogResult.No
                    Return False
            End Select
        End If

        'Everything OK for Videoformats
        Return True
    End Function

    Private Sub IntegerInputHandler(sender As Object, e As TextCompositionEventArgs)
        If Asc(e.Text) < 48 Or 57 < Asc(e.Text) Then
            e.Handled = True
        End If
    End Sub

    Private Sub TimeInputHandler(sender As Object, e As TextCompositionEventArgs)
        If Asc(e.Text) < 48 Or 58 < Asc(e.Text) Then
            e.Handled = True
        End If
    End Sub

    Private Sub ImageDateigroesse_MouseEnter(sender As Object, e As Input.MouseEventArgs) Handles ImageDateigroesse.MouseEnter
        TextBlockHelp.Text = "Ohne Pr0mium 10mb, mit 20mb"
        PopupHelp.IsOpen = True
    End Sub

    Private Sub ImageDateigroesse_MouseLeave(sender As Object, e As Input.MouseEventArgs) Handles ImageDateigroesse.MouseLeave
        PopupHelp.IsOpen = False
    End Sub

    Private Sub ImageStartzeit_MouseEnter(sender As Object, e As Input.MouseEventArgs) Handles ImageStartzeit.MouseEnter
        TextBlockHelp.Text = "Eingabe in Sekunden oder M*:SS, leere Eingabe erlaubt"
        PopupHelp.IsOpen = True
    End Sub

    Private Sub ImageStartzeit_MouseLeave(sender As Object, e As Input.MouseEventArgs) Handles ImageStartzeit.MouseLeave
        PopupHelp.IsOpen = False
    End Sub

    Private Sub ImageEndzeit_MouseEnter(sender As Object, e As Input.MouseEventArgs) Handles ImageEndzeit.MouseEnter
        TextBlockHelp.Text = "Eingabe in Sekunden oder M*:SS, leere Eingabe erlaubt"
        PopupHelp.IsOpen = True
    End Sub

    Private Sub ImageEndzeit_MouseLeave(sender As Object, e As Input.MouseEventArgs) Handles ImageEndzeit.MouseLeave
        PopupHelp.IsOpen = False
    End Sub

    Private Sub ComboBoxFormat_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxFormat.SelectionChanged
        If Me.IsLoaded = False Then
            Return
        End If

        Dim isenabled As Boolean
        If ComboBoxFormat.SelectedIndex = 2 Then
            isenabled = False
        Else
            isenabled = True
        End If

        TextBoxDateigroesse.IsEnabled = isenabled
        SliderTon.IsEnabled = isenabled
        CheckBoxMono.IsEnabled = isenabled
        CheckBoxAufloesung.IsEnabled = isenabled
        ComboBoxPrioritaet.IsEnabled = isenabled
        CheckBox2pass.IsEnabled = isenabled
    End Sub

    Private Sub ImageAusgabeformat_MouseEnter(sender As Object, e As MouseEventArgs) Handles ImageAusgabeformat.MouseEnter
        TextBlockHelp.Text = "Empfohlenes Videoformat: MP4 (H.264/AAC)"
        PopupHelp.IsOpen = True
    End Sub

    Private Sub ImageAusgabeformat_MouseLeave(sender As Object, e As MouseEventArgs) Handles ImageAusgabeformat.MouseLeave
        PopupHelp.IsOpen = False
    End Sub

    Private Sub ImageDurchlaeufe_MouseEnter(sender As Object, e As MouseEventArgs) Handles ImageDurchlaeufe.MouseEnter
        TextBlockHelp.Text = "Kodierung in zwei Durchläufen, erhöht die Videoqualität."
        PopupHelp.IsOpen = True
    End Sub

    Private Sub ImageDurchlaeufe_MouseLeave(sender As Object, e As MouseEventArgs) Handles ImageDurchlaeufe.MouseLeave
        PopupHelp.IsOpen = False
    End Sub

    Private Sub ComboBoxPreset_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxPreset.SelectionChanged
        If Me.IsLoaded = False Then
            Return
        End If
        Select Case ComboBoxPreset.SelectedIndex
            Case 0
                ComboBoxFormat.SelectedIndex = 0
                TextBoxDateigroesse.Text = "10"
                SliderTon.Value = 2
                ComboBoxPrioritaet.SelectedIndex = 0
                CheckBoxMono.IsChecked = True
                CheckBoxAufloesung.IsChecked = True
                CheckBox2pass.IsChecked = True
            Case 1
                ComboBoxFormat.SelectedIndex = 0
                TextBoxDateigroesse.Text = "10"
                SliderTon.Value = 2
                ComboBoxPrioritaet.SelectedIndex = 1
                CheckBoxMono.IsChecked = True
                CheckBoxAufloesung.IsChecked = True
                CheckBox2pass.IsChecked = True
            Case 2
                ComboBoxFormat.SelectedIndex = 0
                TextBoxDateigroesse.Text = "10"
                SliderTon.Value = 2
                ComboBoxPrioritaet.SelectedIndex = 1
                CheckBoxMono.IsChecked = True
                CheckBoxAufloesung.IsChecked = True
                CheckBox2pass.IsChecked = False
            Case 3
                ComboBoxFormat.SelectedIndex = 0
                TextBoxDateigroesse.Text = "20"
                SliderTon.Value = 2
                ComboBoxPrioritaet.SelectedIndex = 0
                CheckBoxMono.IsChecked = True
                CheckBoxAufloesung.IsChecked = True
                CheckBox2pass.IsChecked = True
            Case 4
                ComboBoxFormat.SelectedIndex = 0
                TextBoxDateigroesse.Text = "20"
                SliderTon.Value = 2
                ComboBoxPrioritaet.SelectedIndex = 1
                CheckBoxMono.IsChecked = True
                CheckBoxAufloesung.IsChecked = True
                CheckBox2pass.IsChecked = True
            Case 5
                ComboBoxFormat.SelectedIndex = 0
                TextBoxDateigroesse.Text = "20"
                SliderTon.Value = 2
                ComboBoxPrioritaet.SelectedIndex = 1
                CheckBoxMono.IsChecked = True
                CheckBoxAufloesung.IsChecked = True
                CheckBox2pass.IsChecked = False
        End Select
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Check for Updates
        Try
            Dim version As String
            Dim request As System.Net.WebRequest = System.Net.WebRequest.Create("https://raw.githubusercontent.com/Pr0Pennypacker/Pr0verter-Offline/master/version")
            Using response As System.Net.WebResponse = request.GetResponse
                Using reader As New IO.StreamReader(response.GetResponseStream)
                    version = reader.ReadToEnd
                End Using
            End Using
            version = version.Trim
            If String.Compare(currentversion, version) <> 0 Then
                Select Case MessageBox.Show("Möchtest du Version " + CStr(version) + " installieren?", "Update verfügbar!", MessageBoxButton.YesNo)
                    Case System.Windows.Forms.DialogResult.Yes
                        Process.Start("https://github.com/Pr0Pennypacker/Pr0verter-Offline")
                        Me.Close()
                End Select
            End If
        Catch ex As Exception

        End Try

        Me.Title = "pr0verter offline V" + currentversion
    End Sub
End Class
