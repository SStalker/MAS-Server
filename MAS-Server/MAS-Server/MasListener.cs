using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace MAS_Server
{
    class MasListener
    {
        private System.Windows.Controls.TextBox CommandLog;
        private System.Collections.Queue CommandQueue;

        public MasListener(ref System.Windows.Controls.TextBox CommandLog)
        {
            this.CommandLog = CommandLog;
            this.CommandQueue = new System.Collections.Queue();
        }

        public void AppendCommand(String command)
        {
            Log("Appended command: " + command);
            CommandQueue.Enqueue(command);
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

        public void StartListening()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("192.168.2.101"); // @TODO: Dynamically get current network ip

                server = new TcpListener(localAddr, port);
                server.Start();

                Byte[] bytes = new Byte[256];


                while (true)
                {
                    try
                    {

                        Log("Waiting for a connection... ");
                    }
                    catch (InvalidOperationException e)
                    {
                        MessageBox.Show(e.ToString());
                    }


                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Log("Connected!");

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    // Loop to receive all the data sent by the client.
                    try
                    {
                        while (true)
                        {
                            // Pause 200 ms if command queue is empty
                            while (CommandQueue.Count == 0) System.Threading.Thread.Sleep(200);

                            // Get command from command queue
                            String command = CommandQueue.Dequeue() as String;
                            byte[] msg = System.Text.Encoding.Unicode.GetBytes(command);
                            stream.Write(msg, 0, msg.Length);
                        }
                    }
                    catch (System.IO.IOException e)
                    {
                        Log("Connection closed...");
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        // Shutdown and end connection
                        client.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Log("SocketException: " + e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}
