namespace TodoApi.Models;

public class Cliente
{
    public long Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido1 { get; set; }
    public required string Apellido2 { get; set; }
    public List<Pelicula> PeliculasAlquiladas{get; set;} = new List<Pelicula>();
}