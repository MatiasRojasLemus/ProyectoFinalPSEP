using System;
using System.Runtime.Serialization;


namespace Common
{
    [Serializable]
    public class Mensaje : ISerializable
    {
        public Pelicula Pelicula { get; set; }
        public Cliente Cliente{ get; set; }
        public string idCliente { get; set; }
        public string idPelicula { get; set; }
        public string opcionCliente { get; set; }
        public DateTime stamp { get; set; }

        public Mensaje() { }

        public Mensaje(string opcionCliente, string idCliente, string idPelicula, Cliente Cliente, Pelicula Pelicula)
        {
            this.Cliente = Cliente;
            this.Pelicula = Pelicula;
            this.opcionCliente = opcionCliente;
            this.idCliente = idCliente;
            this.idPelicula = idPelicula;
            this.stamp = DateTime.Now;
        }
        public Mensaje(SerializationInfo info, StreamingContext context)
        {
            this.opcionCliente = info.GetString("opcion");
            this.stamp = (DateTime)info.GetValue("fecha", typeof(DateTime));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("opcion", this.opcionCliente);
            info.AddValue("fecha", this.stamp);
        }



        public override string ToString()
        {
            return $"Opcion seleccionada: {this.opcionCliente} \n Fecha  : {this.stamp}";
        }
    }

}