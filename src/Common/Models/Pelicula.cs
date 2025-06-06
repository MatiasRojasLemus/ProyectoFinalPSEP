using System.Text;

namespace Common
{

    public class Pelicula
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Director { get; set; }
        public string Sinopsis { get; set; }
        public double Precio { get; set; }
        public bool Alquilado { get; set; }
        public Pelicula() { }

        public Pelicula(string Id, string Titulo, string Director, string Sinopsis, double Precio)
        {
            this.Id = Id;
            this.Titulo = Titulo;
            this.Director = Director;
            this.Sinopsis = Sinopsis;
            this.Precio = Precio;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("\tId: " + this.Id);
            sb.AppendLine("\tTitulo: " + this.Titulo);
            sb.AppendLine("\tDirector: " + this.Director);
            sb.AppendLine("\tSinopsis: " + this.Sinopsis);
            sb.AppendLine("\tPrecio: " + this.Precio);
            sb.AppendLine("\tAlquilado: " + this.Alquilado);
            sb.AppendLine("}");

            return sb.ToString();
        }



    }
}