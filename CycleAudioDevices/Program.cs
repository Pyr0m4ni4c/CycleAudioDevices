#region Send Dunes

using System.Net;
using System.Net.Sockets;
using System.Text;

int port = 11000;
ManualResetEvent connectDone = new ManualResetEvent(false);
ManualResetEvent sendDone = new ManualResetEvent(false);
IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
IPAddress ipAddress = ipHostInfo.AddressList[0];
IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
connectDone.WaitOne(TimeSpan.FromMilliseconds(200));
if (!client.Connected)
    return;

// "empty" message
Send(client, $"<EOF>");
sendDone.WaitOne();

client.Shutdown(SocketShutdown.Both);
client.Close();

#region Send Dunes Methods

void ConnectCallback(IAsyncResult ar)
{
    try
    {
        // Retrieve the socket from the state object.
        Socket client = (Socket) ar.AsyncState;

        // Complete the connection.
        client.EndConnect(ar);

        // Signal that the connection has been made.
        connectDone.Set();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
}

void Send(Socket client, String data)
{
    // Convert the string data to byte data using ASCII encoding.
    byte[] byteData = Encoding.ASCII.GetBytes(data);

    // Begin sending the data to the remote device.
    client.BeginSend(byteData, 0, byteData.Length, 0,
        new AsyncCallback(SendCallback), client);
}

void SendCallback(IAsyncResult ar)
{
    try
    {
        // Retrieve the socket from the state object.
        Socket client = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        int bytesSent = client.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to server.", bytesSent);

        // Signal that all bytes have been sent.
        sendDone.Set();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
}

#endregion
#endregion
