Imports System.Net.Sockets
Imports System.IO
Imports System.Net

Public Class Form1
    Private Listen As Threading.Thread

    Sub Listener()
        If GetIPAddress() Is Nothing Then Return

        Dim l As New TcpListener(Net.IPAddress.Parse(GetIPAddress), 8765)
        l.Start()
        Do
            If l.Pending Then l.BeginAcceptTcpClient(AddressOf AcceptConnection, l)
            Threading.Thread.Sleep(1000)
        Loop
    End Sub

    Sub AcceptConnection(ByVal ar As IAsyncResult)
        Try
            
            'receives filename, filelength, filebytes
            Dim listener As TcpListener = CType(ar.AsyncState, TcpListener)
            Dim clientSocket As TcpClient = listener.EndAcceptTcpClient(ar)
            Dim filename, filepath As String, filelen As Long

            'using binaryreader (wrapped networkstream) to read a String and a Long
            Dim br As New BinaryReader(clientSocket.GetStream)
            filename = br.ReadString
            filelen = br.ReadInt64

            Dim di As New DirectoryInfo(Application.StartupPath & "\ReceivedFile")
            If Not di.Exists Then
                Directory.CreateDirectory(Application.StartupPath & "\ReceivedFile")
            End If

            filepath = Path.Combine(Application.StartupPath & "\ReceivedFile\", filename)

            UpdateStatusLabel("Receiving:" & filepath)

            Dim buffer(1024 * 20) As Byte
            Dim readstotal As Long = 0

            Dim reads As Integer = -1
            'using filestream to write read filebytes directly from networkstream
            Using fs As New FileStream(filepath, FileMode.Create, FileAccess.Write)
                Do Until readstotal = filelen
                    reads = clientSocket.GetStream.Read(buffer, 0, buffer.Length)
                    fs.Write(buffer, 0, reads)
                    readstotal += reads
                    'UpdateStatusLabel("Write:" & reads.ToString)
                Loop
            End Using

            UpdateStatusLabel("Received: " & filename)
            UpdateStatusLabel("File size: " & New FileInfo(filepath).Length)

            br.Close()
            clientSocket.Close()

        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As EventArgs) Handles Button1.Click
        Listen = New Threading.Thread(AddressOf Listener)
        Listen.Start()

        Button1.Enabled = False
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        Listen.Abort()
    End Sub

    Private Delegate Sub UpdateStatusLabelDelegate(ByVal status As String)
    Private Sub UpdateStatusLabel(ByVal status As String)
        BeginInvoke(New UpdateStatusLabelDelegate(AddressOf UpdateStatusLabelSub), status)
    End Sub
    Private Sub UpdateStatusLabelSub(ByVal status As String)
        TextBox2.Text &= status & vbCrLf
    End Sub

    Private Function GetIPAddress() As String
        Dim strHostName As String
        strHostName = Dns.GetHostName()

        Dim host As IPHostEntry
        host = Dns.GetHostEntry(strHostName)
        For Each ip As IPAddress In host.AddressList
            If ip.AddressFamily = AddressFamily.InterNetwork Then
                Return ip.ToString
            End If
        Next

        Return Nothing
    End Function
End Class
