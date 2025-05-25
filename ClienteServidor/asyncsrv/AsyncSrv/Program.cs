using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.Json;
using MsgLib;
using Constants.MenuConstants;
using TodoApi.Controllers;
using TodoApi.Models

namespace AsyncSrv
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Socket cliente.  
        public Socket workSocket = null;
        // Tamaño del buffer receptor.   !!!!
        public const int BufferSize = 10; // 1024;
        // Buffer receptor.  
        public byte[] buffer = new byte[BufferSize];
        // Datos string recibidos.  
        public StringBuilder sb = new StringBuilder();
        //String con la peticion enviada por el cliente.
        public string peticionCliente = "";

    }

    public class AsynchronousSocketListener
    {
        //Controlador de los Clientes
        private static PeliculaController peliculaController = new PeliculaController(new VideoclubDbContext(new DbContextOptions<VideoclubDbContext>()));
        //Controlador de las peliculas
        private static ClienteController clienteController = new ClienteController(new VideoclubDbContext(new DbContextOptions<VideoclubDbContext>()));

        private static int PORT = 11000;

        // Hilo de señal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }

        public static void StartListening()
        {
            // Estable un endpoint local para el socket
            IPAddress ipAddress = GetLocalIpAddress();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            // Crea un socket TCP/IP  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);


            // Vincula el socket al endpoint local y escucha a la espera de futuras conexiones
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Establece el evento a un estado de "sin señal"
                    allDone.Reset();

                    //Inicia un socket asincrono a la espera de recibir conexiones  
                    Console.WriteLine("Waiting for a connection at {0}...", localEndPoint);
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    switch (this.peticionCliente)
                    {
                        //Peliculas
                        case MenuContants.OBTENER_TODAS_LAS_PELICULAS: break;
                        case MenuContants.OBTENER_UNA_PELICULA: break;
                        case MenuContants.OBTENER_PELICULAS_EN_ALQUILER: break;
                        case MenuContants.OBTENER_PELICULAR_SIN_ALQUILAR: break;
                        case MenuContants.ANADIR_UNA_PELICULA: break;
                        case MenuContants.ELIMINAR_UNA_PELICULA: break;

                        //Clientes
                        case MenuContants.OBTENER_TODOS_LOS_CLIENTES: break;
                        case MenuContants.OBTENER_UN_CLIENTE: break;
                        case MenuContants.OBTENER_PELICULAS_ALQUILADAS_DE_UN_CLIENTE: break;
                        case MenuContants.ANADIR_UN_CLIENTE: break;
                        case MenuContants.ALQUILAR_UNA_PELICULA: break;
                        case MenuContants.DESALQUILAR_UNA_PELICULA: break;
                        case MenuContants.ELIMINAR_UN_CLIENTE: break;

                    }


                    // Espera hasta que se realice una conexion antes de continuar
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Señal del hilo principal para continuar
            allDone.Set();

            // Obtiente el socket que maneja la peticion del cliente
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Crea un Objeto de Estado
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            Console.Write("_"); // Rastro
            // Recupera el Objeto de Estado y el socket que maneja la peticion del cliente
            // del Objeto de Estado Asincrono
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Lee los datos del socket cliente 
            int bytesRead = handler.EndReceive(ar);

            // Recibe la cantidad de datos que han sido recibidos de la red y
            // estan disponibles para ser leidos
            if (handler.Available > 0)
            {
                Console.Write("0"); // Rastro
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                // No se recibieron todos los datos. Obtener mas.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            else
            {
                if (bytesRead > 0)
                {
                    Console.Write("1"); // Rastro
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                }
                if (state.sb.Length > 1)
                {
                    Console.WriteLine("2"); // Rastro
                    Console.WriteLine(state.sb.ToString());

                    byte[] byteArray = Encoding.UTF8.GetBytes(state.sb.ToString());

                    // Deserializa el string JSON y lo convierte en un objeto Mensaje
                    this.peticionCliente = state.sb.ToString();
                    Mensaje recibido = JsonSerializer.Deserialize<Mensaje>(state.sb.ToString());

                    // Todos los datos que han sido leidos desde el cliente. Reproduciendolos en consola. 
                    Console.WriteLine("Read {0} bytes from socket.\n{1}",
                        byteArray.Length, recibido);

                    // Manda de vuelta los datos al cliente  
                    Send(handler, recibido);
                }
                else
                {
                    // Si nada a sido recibido
                    Console.Write("3"); // Rastro
                }
            }

        }

        private static void Send(Socket handler, Mensaje data)
        {
            // Convierte el mensaje a JSON
            string jsonData = JsonSerializer.Serialize(data);
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

            // Comienza a enviar los datos al dispositivo remoto.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Recupera el socket del Objeto de Estado
                Socket handler = (Socket)ar.AsyncState;

                // Termina de enviar los datos al dispositivo remoto.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static IPAddress GetLocalIpAddress()
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
            if (ipAddressList.Count == 1)
            {
                return ipAddressList[0];
            }
            else
            {
                int i = 0;
                foreach (IPAddress ipa in ipAddressList)
                {
                    Console.WriteLine($"[{i++}]: {ipa}");
                }
                t = ipAddressList.Count - 1;
                System.Console.Write($"Opción [0-{t}]: ");
                string s = Console.ReadLine();
                if (Int32.TryParse(s, out int j))
                {
                    if ((j >= 0) && (j <= t))
                    {
                        return ipAddressList[j];
                    }
                }
                return null;
            }
        }

    }
    
    private async void MostrarPeliculas() {
        
    }


}