using System.Text;

namespace TodoApi.Models;

public class Cliente
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido1 { get; set; }
    public string Apellido2 { get; set; }
    public List<Pelicula> PeliculasAlquiladas { get; set; }

    public Cliente(string Nombre, string Apellido1, string Apellido2)
    {
        this.Nombre = Nombre;
        this.Apellido1 = Apellido1;
        this.Apellido2 = Apellido2;
        PeliculasAlquiladas = [];
    }

    public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Id: " + this.Id);
        sb.AppendLine("Nombre: " + this.Nombre);
        sb.AppendLine("Apellido1: " + this.Apellido1);
        sb.AppendLine("Apellido2: " + this.Apellido2);
        sb.AppendLine("Peliculas Alquiladas:");
        foreach (Pelicula pelicula in this.PeliculasAlquiladas)
        {
            sb.AppendLine(pelicula.ToString());
        }
        return sb.ToString();
    }
    

}