using System.Text;

namespace TodoApi.Models;

public class Pelicula
{
    public long Id { get; set; }
    public string Titulo { get; set; }
    public string Director { get; set; }
    public string Sinopsis { get; set; }
    public double Precio { get; set; }
    public bool Alquilado { get; set; }

    public Pelicula(string Titulo, string Director, string Sinopsis, double Precio)
    {
        this.Titulo = Titulo;
        this.Director = Director;
        this.Sinopsis = Sinopsis;
        this.Precio = Precio;
    }

    public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Id: " + this.Id);
        sb.AppendLine("Titulo: " + this.Titulo);
        sb.AppendLine("Director: " + this.Director);
        sb.AppendLine("Sinopsis: " + this.Sinopsis);
        sb.AppendLine("Precio: " + this.Precio);
        sb.AppendLine("Alquilado: " + this.Alquilado);
        return sb.ToString();
    }

    
    
}