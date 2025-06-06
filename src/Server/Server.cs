

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;
using API;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;





namespace Server
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

    }

    public class AsynchronousSocketListener
    {
        //String con la peticion enviada por el cliente.
        public static string peticionCliente = "";

        private static int PORT = 11000;

        // Hilo de señal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static readonly HttpClient client = new HttpClient();


        public async static Task Main(String[] args)
        {
            await StartListening();
            return;
        }


        public static async Task StartListening()
        {
            client.BaseAddress = new Uri("http://localhost:5080/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

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

        public static async void ReadCallback(IAsyncResult ar)
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
                    byte[] byteArray = Encoding.UTF8.GetBytes(state.sb.ToString());

                    // Deserializa el string JSON y lo convierte en un objeto Mensaje
                    Mensaje recibido = JsonSerializer.Deserialize<Mensaje>(state.sb.ToString());
                    StringBuilder sbRespuesta = new();
                    string url;

                    //TODO:
                    switch (recibido.opcionCliente)
                    {
                        //Peliculas
                        case MenuConstants.OBTENER_TODAS_LAS_PELICULAS:
                            url = "http://localhost:5080/api/Pelicula/todas";
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var peliculas = await response.Content.ReadFromJsonAsync<IEnumerable<Pelicula>>();
                                    if (peliculas.Count() > 0 && peliculas != null)
                                    {
                                        foreach (Pelicula pelicula in peliculas)
                                        {
                                            sbRespuesta.AppendLine(pelicula.ToString());
                                        }
                                        Send(handler, sbRespuesta.ToString());
                                    }
                                    else
                                    {
                                        String mensaje = "No se han encontrado películas";
                                        Send(handler, mensaje);
                                    }
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.OBTENER_UNA_PELICULA:
                            url = "http://localhost:5080/api/Pelicula/" + recibido.idPelicula;
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var pelicula = await response.Content.ReadAsStringAsync();
                                    Send(handler, pelicula);
                                }
                                else
                                {
                                    String mensaje = "No se ha encontrado ninguna pelicula con dicha ID";
                                    Send(handler, mensaje);
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.OBTENER_PELICULAS_EN_ALQUILER:
                            url = "http://localhost:5080/api/Pelicula/alquiladas";
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var peliculas = await response.Content.ReadFromJsonAsync<IEnumerable<Pelicula>>();
                                    if (peliculas.Count() > 0 && peliculas != null)
                                    {
                                        foreach (Pelicula pelicula in peliculas)
                                        {
                                            sbRespuesta.AppendLine(pelicula.ToString());
                                        }
                                        Send(handler, sbRespuesta.ToString());
                                    }
                                    else
                                    {
                                        String mensaje = "No se han encontrado películas";
                                        Send(handler, mensaje);
                                    }
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.OBTENER_PELICULAR_SIN_ALQUILAR:
                            url = "http://localhost:5080/api/Pelicula/sin-alquilar";
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var peliculas = await response.Content.ReadFromJsonAsync<IEnumerable<Pelicula>>();
                                    if (peliculas.Count() > 0 && peliculas != null)
                                    {
                                        foreach (Pelicula pelicula in peliculas)
                                        {
                                            sbRespuesta.AppendLine(pelicula.ToString());
                                        }
                                        Send(handler, sbRespuesta.ToString());
                                    }
                                    else
                                    {
                                        String mensaje = "No se han encontrado películas";
                                        Send(handler, mensaje);
                                    }
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.ANADIR_UNA_PELICULA:
                            url = "http://localhost:5080/api/Pelicula/";
                            try
                            {
                                var peli = recibido.Pelicula;
                                HttpResponseMessage response = await client.PostAsJsonAsync(url, peli);
                                response.EnsureSuccessStatusCode();
                                Send(handler, peli.ToString());
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al añadir la pelicula: {e.Message}");
                            }
                            break;

                        case MenuConstants.ELIMINAR_UNA_PELICULA:
                            url = "http://localhost:5080/api/Pelicula/" + recibido.idPelicula;
                            try
                            {
                                HttpResponseMessage response = await client.DeleteAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    Send(handler, "Pelicula con la ID " + recibido.idPelicula + "ha sido eliminada correctamente");
                                }
                                else
                                {
                                    String mensaje = "No se ha encontrado ninguna pelicula con esa ID que eliminar";
                                    Send(handler, mensaje);
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;


                        //CLIENTE
                        case MenuConstants.OBTENER_TODOS_LOS_CLIENTES:
                            url = "http://localhost:5080/api/Cliente";
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var clientes = await response.Content.ReadFromJsonAsync<IEnumerable<Cliente>>();
                                    if (clientes.Count() > 0 && clientes != null)
                                    {
                                        foreach (Cliente cliente in clientes)
                                        {
                                            sbRespuesta.AppendLine(cliente.ToString());
                                        }
                                        Send(handler, sbRespuesta.ToString());
                                    }
                                    else
                                    {
                                        String mensaje = "No se han encontrado películas";
                                        Send(handler, mensaje);
                                    }
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.OBTENER_UN_CLIENTE:
                            url = "http://localhost:5080/api/Cliente/" + recibido.idCliente;
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var cliente = await response.Content.ReadAsStringAsync();
                                    Send(handler, cliente);
                                }
                                else
                                {
                                    String mensaje = "No se ha encontrado ningun cliente con dicha ID";
                                    Send(handler, mensaje);
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.OBTENER_PELICULAS_ALQUILADAS_DE_UN_CLIENTE:
                            url = "http://localhost:5080/api/Cliente/" + recibido.idCliente + "/peliculas";
                            try
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    var peliculas = await response.Content.ReadFromJsonAsync<IEnumerable<Pelicula>>();
                                    if (peliculas.Count() > 0 && peliculas != null)
                                    {
                                        foreach (Pelicula pelicula in peliculas)
                                        {
                                            sbRespuesta.AppendLine(pelicula.ToString());
                                        }
                                        Send(handler, sbRespuesta.ToString());
                                    }
                                    else
                                    {
                                        String mensaje = "No se han encontrado películas alquiladas de este cliente";
                                        Send(handler, mensaje);
                                    }
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        case MenuConstants.ANADIR_UN_CLIENTE:
                            url = "http://localhost:5080/api/Cliente/";
                            try
                            {
                                var cliente = recibido.Cliente;
                                HttpResponseMessage response = await client.PostAsJsonAsync(url, cliente);
                                response.EnsureSuccessStatusCode();
                                Send(handler, cliente.ToString());

                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al añadir la pelicula: {e.Message}");
                            }
                            break;

                        case MenuConstants.ALQUILAR_UNA_PELICULA:
                            url = "http://localhost:5080/api/Cliente/" + recibido.idCliente + "/Alquilar-pelicula/" + recibido.idPelicula;
                            try
                            {
                                var pelicula = recibido.Pelicula;
                                HttpResponseMessage response = await client.PutAsJsonAsync(url, pelicula);
                                response.EnsureSuccessStatusCode();
                                Send(handler, pelicula.ToString());
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al alquilar la pelicula: {e.Message}");
                            }
                            break;


                        /**
                            //Desalquilar una pelicula de un cliente
                            //PUT: api/Cliente
                            [HttpPut("api/Cliente/{clienteId}/Desalquilar-pelicula/{peliculaId}")]
                        **/
                        case MenuConstants.DESALQUILAR_UNA_PELICULA:
                            url = "http://localhost:5080/api/Cliente/" + recibido.idCliente + "/Desalquilar-pelicula/" + recibido.idPelicula;
                            try
                            {
                                var pelicula = recibido.Pelicula;
                                HttpResponseMessage response = await client.PutAsJsonAsync(url, pelicula);
                                response.EnsureSuccessStatusCode();
                                Send(handler, pelicula.ToString());
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al añadir la pelicula: {e.Message}");
                            }
                            break;

                        case MenuConstants.ELIMINAR_UN_CLIENTE:
                            url = "http://localhost:5080/api/Cliente/" + recibido.idCliente;
                            try
                            {
                                HttpResponseMessage response = await client.DeleteAsync(url);
                                if (response.IsSuccessStatusCode)
                                {
                                    Send(handler, "Cliente con la ID " + recibido.idCliente + "ha sido eliminada correctamente");
                                }
                                else
                                {
                                    String mensaje = "No se ha encontrado ninguna pelicula con esa ID que eliminar";
                                    Send(handler, mensaje);
                                }
                            }
                            catch (HttpRequestException e)
                            {
                                // Manejar el error de la solicitud
                                Console.WriteLine($"Error al obtener las películas: {e.Message}");
                            }
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    // Si nada a sido recibido
                    Console.Write("3"); // Rastro
                }
            }

        }

        private static void Send(Socket handler, string data)
        {
            // Convierte el mensaje
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Comienza a enviar los datos al dispositivo remoto.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
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
}

