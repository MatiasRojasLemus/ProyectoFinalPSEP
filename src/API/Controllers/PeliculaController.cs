

using Microsoft.AspNetCore.Mvc;
using Common;
using Microsoft.EntityFrameworkCore;

namespace API
{
    [Route("api/Pelicula")]
    [ApiController]
    public class PeliculaController : ControllerBase
    {
        public readonly VideoclubDbContext _context;

        public PeliculaController(VideoclubDbContext context)
        {
            _context = context;
        }

        //Obtener todas las peliculas
        //GET: api/Pelicula
        [HttpGet("todas")]
        public async Task<ActionResult<IEnumerable<Pelicula>>> ObtenerPeliculas()
        {
            return await _context.Peliculas.ToListAsync();
        }

        //Obtener una pelicula en particular
        //GET: api/Pelicula/3
        [HttpGet("{id}")]
        public async Task<ActionResult<Pelicula>> ObtenerPelicula(string id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);

            if (pelicula == null)
            {
                return NotFound();
            }

            return pelicula;
        }

        //Obtener solo las peliculas alquiladas
        //GET: api/Pelicula
        [HttpGet("alquiladas")]
        public async Task<ActionResult<IEnumerable<Pelicula>>> ObtenerPeliculasAlquiladas()
        {
            var peliculasAlquiladas = await _context.Peliculas.Where(pelicula => pelicula.Alquilado == true).ToListAsync();

            if (peliculasAlquiladas.Count == 0)
            {
                return NotFound();
            }

            return peliculasAlquiladas;
        }

        //Obtener solo las peliculas sin alquilar
        //GET: api/Pelicula
        [HttpGet("sin-alquilar")]
        public async Task<ActionResult<IEnumerable<Pelicula>>> ObtenerPeliculasSinAlquilar()
        {
            var peliculasAlquiladas = await _context.Peliculas.Where(pelicula => pelicula.Alquilado == false).ToListAsync();

            if (peliculasAlquiladas.Count == 0)
            {
                return NotFound();
            }

            return peliculasAlquiladas;
        }

        //Añadir una Pelicula
        //POST: api/Pelicula
        [HttpPost]
        public async Task<ActionResult<Pelicula>> AnadirPelicula(Pelicula pelicula)
        {
            _context.Peliculas.Add(pelicula);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(ObtenerPelicula), new { id = pelicula.Id }, pelicula);
        }


        //Eliminar una pelicula
        //DELETE: api/Pelicula/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPelicula(string id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);

            if (pelicula == null)
            {
                return NotFound();
            }

            _context.Peliculas.Remove(pelicula);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        private bool PeliculaExiste(string id){
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}


