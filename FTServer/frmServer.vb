Imports System.IO
Imports System.Net.Sockets

Public Class frmServer

    Const IPADDRESS As String = "192.168.1.69"
    Const PORT As Integer = 7832
    Const BUFFERSIZE As Integer = 1024 * 20

    Private Sub Button1_Click(sender As System.Object, e As EventArgs) Handles Button1.Click
        Dim opf As New OpenFileDialog
        opf.Multiselect = False
        If opf.ShowDialog = DialogResult.OK Then
            TextBox1.Text = opf.FileName
        End If
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As EventArgs) Handles Button2.Click
        If New FileInfo(TextBox1.Text).Exists Then
            Dim s As New Sending(AddressOf Send)
            s.BeginInvoke(TextBox1.Text, Nothing, Nothing)
        End If
    End Sub

    Private Delegate Sub Sending(ByVal filename As String)
    Private Sub Send(ByVal fullfilename As String)
        Try
            'sends filename, filelength, filebytes
            Dim info As New FileInfo(fullfilename)
            Dim tcp As New TcpClient
            tcp.Connect(Net.IPAddress.Parse(IPADDRESS), PORT) 'send the file to client IP address

            UpdateStatusLabel(String.Format("Connected to: {0}:{1}", IPADDRESS, PORT))

            'writes a String and a Long with binarywriter (wrapping networkstream)
            Dim bw As New BinaryWriter(tcp.GetStream)
            bw.Write(info.Name)
            bw.Write(info.Length)

            UpdateStatusLabel("File name: " & info.Name)
            UpdateStatusLabel("File size: " & info.Length)

            'using filestream to read file, writes this directly to networkstream
            Using fs As New FileStream(fullfilename, FileMode.Open, FileAccess.Read)
                Dim buffer(BUFFERSIZE) As Byte
                Dim reads As Integer = -1

                Dim totalRead As Integer = 0
                Do Until reads = 0
                    reads = fs.Read(buffer, 0, buffer.Length)
                    tcp.GetStream.Write(buffer, 0, reads)

                    totalRead += reads
                    'UpdateStatusLabel("Read: " & totalRead.ToString & " of " & info.Length)
                Loop
            End Using

            UpdateStatusLabel("Sent successfull: " & info.Name)

            bw.Close()
            tcp.Close()
            Button2.Enabled = True

        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub

    Private Delegate Sub UpdateStatusLabelDelegate(ByVal status As String)
    Private Sub UpdateStatusLabel(ByVal status As String)
        BeginInvoke(New UpdateStatusLabelDelegate(AddressOf UpdateStatusLabelSub), status)
    End Sub
    Private Sub UpdateStatusLabelSub(ByVal status As String)
        TextBox2.Text &= status & vbCrLf
    End Sub
End Class
