using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Collections.Generic;

namespace MAS_Server
{
    class MasListener
    {
        private System.Windows.Controls.TextBox CommandLog;
        private TcpListener listener;
        private List<Client> clients;

        public MasListener(ref System.Windows.Controls.TextBox CommandLog)
        {
            this.CommandLog = CommandLog;

            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("192.168.2.101"); // @TODO: Dynamically get current network ip
            this.listener = new TcpListener(localAddr, port);
            this.clients = new List<Client>();
            listener.Start();
            listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        public void SendCommand(String command)
        {
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(command);
            foreach (Client client in clients)
            {
                client.NetworkStream.BeginWrite(buffer, 0, buffer.Length,
                    WriteCallback, client);
            }
        }

        private void Log(String line)
        {
            try
            {
                CommandLog.Dispatcher.Invoke(new Action(
                    delegate()
                    {
                        CommandLog.AppendText(line + "\r\n");
                    }
                ));
            }
            catch (System.Threading.Tasks.TaskCanceledException e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.ToString());
            }
        }

        private void AcceptTcpClientCallback(IAsyncResult result)
        {
            TcpClient tcpClient = listener.EndAcceptTcpClient(result);
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            Client client = new Client(ref tcpClient, ref buffer);

            lock (this.clients)
            {
                this.clients.Add(client);
            }

            NetworkStream networkStream = client.NetworkStream;
            networkStream.BeginRead(buffer, 0, buffer.Length,
                ReadCallback, client);
        }

        private void ReadCallback(IAsyncResult result)
        {
            Client client = result.AsyncState as Client;
            NetworkStream networkStream = client.NetworkStream;
            int read = networkStream.EndRead(result);

            if (read == 0)
            {
                lock (this.clients)
                {
                    this.clients.Remove(client);
                    return;
                }
            }

            String data = System.Text.Encoding.Unicode.GetString(client.Buffer, 0, read);
            Log(data);
            networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
        }

        private void WriteCallback(IAsyncResult result)
        {
            Client client = result.AsyncState as Client;
            NetworkStream networkStream = client.NetworkStream;
            networkStream.EndWrite(result);
        }

        internal class Client
        {
            public String Name { get; set; }
            public TcpClient TcpClient { get; set; }
            public byte[] Buffer { get; set; }
            public NetworkStream NetworkStream { get { return TcpClient.GetStream(); } }

            public Client(ref TcpClient tcpClient, ref byte[] buffer)
            {
                this.TcpClient = tcpClient;
                this.Buffer = buffer;
            }
        }
    }
}
