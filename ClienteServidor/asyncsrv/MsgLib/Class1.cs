using System;
using System.Runtime.Serialization;

namespace MsgLib
{
    [Serializable]
    public class Mensaje : ISerializable
    {
        public required string opcionCliente { get; set; }
        public DateTime stamp { get; set; }
        public Mensaje(string opcionCliente)
        {
            this.opcionCliente = opcionCliente;
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
            return $"Opcion seleccionada: {this.opcionCliente}\nFecha  : {this.stamp}";
        }
    }

}