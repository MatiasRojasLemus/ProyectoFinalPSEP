using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.Json;
using MsgLib;

namespace AsyncCli
{
    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 10; // 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        public int mensajesEnviados = 0;
    }

    public class AsynchronousClient
    {
        // The port number for the remote device.  
        private const int PORT = 11000;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }

        private static void StartClient()
        {
            // Connect to a remote device.  
            try
            {   
                while (true)
                {
                // Establish the remote endpoint for the socket. 
                IPAddress ipAddress = GetFirstLocalIpV4Address();
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne(); //Blocks the current thread until the current WaitHandle receives a signal.
                
                //We make the user send as much strings as they want
                Console.WriteLine("Write you message subject: ");
                string sms = Console.ReadLine();
                Console.WriteLine("Write your message: ");
                string resumen = Console.ReadLine();

                //If the "sms" and "resumen" equals null, we close connection and finish the program
                if(sms == "" && resumen == ""){
                    // Release the socket.
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    break;
                }

                Mensaje mensaje = new Mensaje(sms, resumen);
                Send(client, mensaje);
                sendDone.WaitOne();

                // Receive the response from the remote device.
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received:\n{0}", response);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static IPAddress GetFirstLocalIpV4Address()
        {
            List<IPAddress> ipAddressList = new List<IPAddress>();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            int t = ipHostInfo.AddressList.Length;
            string ip;
            for (int i = 0; i < t; i++)
            {
                ip = ipHostInfo.AddressList[i].ToString();
                if (ip.Contains(".") && !ip.Equals("127.0.0.1"))
                {
                    ipAddressList.Add(ipHostInfo.AddressList[i]);
                }
            }
            if (ipAddressList.Count > 0)
            {
                return ipAddressList[0];
            }
            else
            {
                return null;
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket handler, Mensaje data)
        {
            // Convert the message to JSON
            var jsonData = JsonSerializer.Serialize(data);
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);
            
            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
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

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Console.Write("_"); // Trace
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    Console.Write("1"); // Trace
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        Console.WriteLine("2"); // Trace
                        // Deserialization of the object
                        response = state.sb.ToString();
                        byte[] byteArray = Encoding.UTF8.GetBytes(response);
                        
                        // Deserialize the JSON string into a Mensaje object
                        Mensaje recibido = JsonSerializer.Deserialize<Mensaje>(response);
                    }
                    else
                    {
                        // If nothing has been received
                        Console.Write("3"); // Trace
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}