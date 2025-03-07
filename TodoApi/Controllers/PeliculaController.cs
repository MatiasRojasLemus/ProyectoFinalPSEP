using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/Pelicula")]
    [ApiController]
    public class PeliculaController: ControllerBase
    {
        private readonly VideoclubDbContext _context;
        public PeliculaController(VideoclubDbContext context)
        {
            _context = context;
        }

        //Obtener todas las peliculas
        //GET: api/Pelicula
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pelicula>>> ObtenerPeliculas()
        {
            return await _context.Peliculas.ToListAsync();
        }

        //Obtener una pelicula en particular
        //GET: api/Pelicula/3
        [HttpGet("{id}")]
        public async Task<ActionResult<Pelicula>> ObtenerPelicula(long id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);

            if(pelicula == null){
                return NotFound();
            }

            return pelicula;
        }

        //Añadir una Pelicula
        //POST: api/Pelicula
        [HttpPost]
        public async Task<ActionResult<Pelicula>> AnadirPelicula(Pelicula pelicula){
            _context.Peliculas.Add(pelicula);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(ObtenerPelicula), new {id = pelicula.Id}, pelicula);
        }


        //Modificar una Pelicula por id
        //PUT: api/Pelicula/3
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarPelicula(long id, Pelicula pelicula){
            if(id != pelicula.Id){
                return BadRequest();
            }

            _context.Entry(pelicula).State = EntityState.Modified;

            try
            {
               await _context.SaveChangesAsync(); 
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!PeliculaExiste(id)){
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        //Eliminar una pelicula
        //DELETE: api/Pelicula/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPelicula(long id){
            var pelicula = await _context.Peliculas.FindAsync(id);

            if(pelicula == null){
                return NotFound();
            } 

            _context.Peliculas.Remove(pelicula);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PeliculaExiste(long id){
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}