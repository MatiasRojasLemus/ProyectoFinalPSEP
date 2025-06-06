using Microsoft.EntityFrameworkCore;

namespace Common
{
    //Clase contexto del videoclub
    public class VideoclubDbContext : DbContext
    {
        //Constructor
        public VideoclubDbContext(DbContextOptions<VideoclubDbContext> options) : base(options) { }
        //Atributos
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }

    }
}


