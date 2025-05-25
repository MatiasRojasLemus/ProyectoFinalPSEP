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



namespace AsyncCli
{

    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Socket cliente.  
        public Socket workSocket = null;

        // Tamaño del buffer recibido.  
        public const int BufferSize = 10; // 1024;

        // Buffer recibido.  
        public byte[] buffer = new byte[BufferSize];

        // Dato de objeto string recibido.  
        public StringBuilder sb = new StringBuilder();
        public int mensajesEnviados = 0;
    }

    public class AsynchronousClient
    {
        // El numero de puerto para el dispositivo remoto 
        private const int PORT = 11000;

        //ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // La respuesta desde el dispositivo remoto.  
        private static String response = String.Empty;

        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }

        private static void StartClient()
        {
            // Se conecta a un dispositivo remoto.  
            try
            {
                while (true)
                {
                    // Establece el endpoint remoto para el socket. 
                    IPAddress ipAddress = GetFirstLocalIpV4Address();
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                    // Crea un socket TCP/IP.  
                    Socket cliente = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);


                    // Se conecta al endpoint remoto.  
                    cliente.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), cliente);
                    connectDone.WaitOne(); //Bloquea el hilo reciente hasta que la instancia WaitHandle actual reciba una señal

                    string opcionMenu = MenuPrincipal();

                    if (opcionMenu == MenuConstants.SALIR_DEL_PROGRAMA)
                    {
                        cliente.Shutdown(SocketShutdown.Both);
                        cliente.Close();
                        break;
                    }

                    Mensaje mensaje = new Mensaje(opcionMenu);

                    //Mandamos un mensaje
                    Send(cliente, mensaje);

                    //Bloqueamos el hilo actual hasta recibir una respuesta
                    sendDone.WaitOne();

                    // Recibe la respuesta del dispositivo remoto.
                    Receive(cliente);
                    receiveDone.WaitOne();

                    // Escribe la respuesta en la consola.  
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
                //Recupera el socket del objecto de estado
                Socket cliente = (Socket)ar.AsyncState;

                // Completa la conexion  
                cliente.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    cliente.RemoteEndPoint.ToString());

                // Señal de que la conexion ha sido realizada 
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket handler, Mensaje data)
        {
            // Convierte el mensaje a JSON
            var jsonData = JsonSerializer.Serialize(data);
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

            // Comienza a enviar la informacion al dispositivo remoto  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Recuera el socket del objeto de estado.  
                Socket cliente = (Socket)ar.AsyncState;

                // Termina de enviar la informacion al dispositivo remoto
                int bytesSent = cliente.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                //Muestra una señal de que todos los bytes han sido enviados
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket cliente)
        {
            try
            {
                // Crea el StateObject (Objeto de Estado)  
                StateObject state = new StateObject();
                state.workSocket = cliente;

                //Comienza a recibir los datos del dispositivo remoto 
                cliente.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
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
                Console.Write("_"); // Rastro
                                    // Recupera el Objeto de Estado y el socket cliente
                                    // del Objeto de Estado asincrono
                StateObject state = (StateObject)ar.AsyncState;
                Socket cliente = state.workSocket;

                // Leer datos del dispositivo remoto 
                int bytesRead = cliente.EndReceive(ar);

                if (bytesRead > 0)
                {
                    Console.Write("1"); // Rastro
                                        // Pueden haber mas datos, asi que se almacena los recibidos hasta ahora
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Obtener el resto de los datos
                    cliente.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // Todos los datos han llegado; poner los en respuesta
                    if (state.sb.Length > 1)
                    {
                        Console.WriteLine("2"); // Rastro
                                                // Deserializacion del objeto
                        response = state.sb.ToString();
                        byte[] byteArray = Encoding.UTF8.GetBytes(response);

                        // Deserializar el JSON string en un objeto Mensaje
                        Mensaje recibido = JsonSerializer.Deserialize<Mensaje>(response);
                    }
                    else
                    {
                        // Si nada ha sido recibido
                        Console.Write("3"); // Rastro
                    }
                    // Señal de que todos los datos han sido recibidos
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private static string MenuPrincipal()
        {
            Console.WriteLine("Menu principal:");
            Console.WriteLine("1.- Peliculas");
            Console.WriteLine("2.- Clientes");
            Console.WriteLine("Cualquier otra opcion - Salir del programa");
            Console.WriteLine("Introduce una opcion:");
            string menuSeleccionado = Console.ReadLine();


            switch (menuSeleccionado)
            {
                case MenuConstants.MENU_PELICULA: return SubmenuPeliculas();
                case MenuConstants.MENU_CLIENTE: return SubmenuClientes();
                default: return MenuConstants.SALIR_DEL_PROGRAMA;
            }

        }

        private static string SubmenuPeliculas()
        {
            Console.WriteLine("1.- Obtener todas las películas");
            Console.WriteLine("2.- Obtener una película");
            Console.WriteLine("3.- Obtener solo las peliculas alquiladas");
            Console.WriteLine("4.- Obtener solo las peliculas sin alquilar");
            Console.WriteLine("5.- Añadir una pelicula");
            Console.WriteLine("6.- Eliminar una pelicula");
            Console.WriteLine("Cualquier otra opcion - Volver al menu principal");
            string opcionSeleccionada = Console.ReadLine();

            switch (opcionSeleccionada)
            {
                case "1": return MenuConstants.OBTENER_TODAS_LAS_PELICULAS;
                case "2": return MenuConstants.OBTENER_UNA_PELICULA;
                case "3": return MenuConstants.OBTENER_PELICULAS_EN_ALQUILER;
                case "4": return MenuConstants.OBTENER_PELICULAR_SIN_ALQUILAR;
                case "5": return MenuConstants.ANADIR_UNA_PELICULA;
                case "6": return MenuConstants.ELIMINAR_UNA_PELICULA;
                default: return MenuPrincipal();
            }

        }

        private static string SubmenuClientes()
        {
            Console.WriteLine("1.- Obtener todos los clientes");
            Console.WriteLine("2.- Obtener un cliente en particular");
            Console.WriteLine("3.- Obtener peliculas alquiladas de un cliente en particular");
            Console.WriteLine("4.- Añadir un cliente");
            Console.WriteLine("5.- Alquilar una Pelicula");
            Console.WriteLine("6.- Desalquilar una Pelicula");
            Console.WriteLine("7.- Eliminar un cliente");
            Console.WriteLine("Cualquier otra opcion - Volver al menu principal");
            string opcionSeleccionada = Console.ReadLine();
            switch (opcionSeleccionada)
            {
                case "1": return MenuConstants.OBTENER_TODOS_LOS_CLIENTES;
                case "2": return MenuConstants.OBTENER_UN_CLIENTE;
                case "3": return MenuConstants.OBTENER_PELICULAS_ALQUILADAS_DE_UN_CLIENTE;
                case "4": return MenuConstants.ANADIR_UN_CLIENTE;
                case "5": return MenuConstants.ALQUILAR_UNA_PELICULA;
                case "6": return MenuConstants.DESALQUILAR_UNA_PELICULA;
                case "7": return MenuConstants.ELIMINAR_UN_CLIENTE;
                default: return MenuPrincipal();
            }
        }
    }
}
}