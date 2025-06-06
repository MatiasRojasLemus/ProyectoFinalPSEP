using System.Text;

namespace Common
{

    public class Cliente
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public List<Pelicula> PeliculasAlquiladas { get; set; }
        public Cliente() { }
        public Cliente(string Id, string Nombre, string Apellido1, string Apellido2)
        {
            this.Id = Id;
            this.Nombre = Nombre;
            this.Apellido1 = Apellido1;
            this.Apellido2 = Apellido2;
            PeliculasAlquiladas = [];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("\tId: " + this.Id);
            sb.AppendLine("\tNombre: " + this.Nombre);
            sb.AppendLine("\tApellido1: " + this.Apellido1);
            sb.AppendLine("\tApellido2: " + this.Apellido2);


            if (this.PeliculasAlquiladas is not null && this.PeliculasAlquiladas.Count > 0)
            {
                sb.AppendLine("\tPeliculas Alquiladas:");
                foreach (Pelicula pelicula in this.PeliculasAlquiladas)
                {
                    sb.AppendLine("\t\t" + pelicula.ToString());
                }
            }
            else
            {
                sb.AppendLine("\tPeliculas Alquiladas: null");
            }

            sb.AppendLine("}");


            return sb.ToString();
        }


    }
}