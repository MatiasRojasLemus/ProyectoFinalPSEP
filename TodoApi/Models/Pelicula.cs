namespace TodoApi.Models;

public class Pelicula
{
    public long Id { get; set; }
    public required string Titulo { get; set; }
    public required string Director { get; set; }
    public required string Sinopsis { get; set; }
    public required double Precio {get; set;}
    public required bool Alquilado {get; set;}
}