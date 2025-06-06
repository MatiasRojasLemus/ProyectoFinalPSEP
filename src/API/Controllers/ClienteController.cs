

using Microsoft.AspNetCore.Mvc;
using Common;
using Microsoft.EntityFrameworkCore;

namespace API
{
    [Route("api/Cliente")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly VideoclubDbContext _context;

        public ClienteController(VideoclubDbContext context)
        {
            _context = context;
        }


        //Obtener todos los clientes
        //GET: api/Cliente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> ObtenerClientes()
        {
            return await _context.Clientes.ToListAsync();
        }


        //Obtener un cliente en particular
        //GET: api/Cliente/3
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> ObtenerCliente(string id)
        {
            var cliente = await _context.Clientes
                            .Include(c => c.PeliculasAlquiladas)
                            .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente;
        }

        //Obtener peliculas alquiladas de un cliente en particular
        //GET: api/Cliente/3/peliculas
        [HttpGet("{id}/peliculas")]
        public async Task<ActionResult<IEnumerable<Pelicula>>> ObtenerPeliculasCliente(string id)
        {
            var cliente = await _context.Clientes
                            .Include(c => c.PeliculasAlquiladas)
                            .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente.PeliculasAlquiladas;
        }


        //Añadir un cliente
        //POST: api/Cliente
        [HttpPost]
        public async Task<ActionResult<Cliente>> AnadirCliente(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(ObtenerCliente), new { id = cliente.Id }, cliente);
        }


        //Alquilar una pelicula y añadir la lista de peliculas alquiladas de un Cliente
        //PUT: api/Cliente
        [HttpPut("{clienteId}/Alquilar-pelicula/{peliculaId}")]
        public async Task<IActionResult> AlquilarPelicula(string clienteId, string peliculaId)
        {
            Console.WriteLine("Principio del codigo.");

            var cliente = await _context.Clientes
                            .Include(c => c.PeliculasAlquiladas)
                            .FirstOrDefaultAsync(c => c.Id == clienteId);

            Console.WriteLine("Definicion de variable cliente.");

            var pelicula = await _context.Peliculas.FindAsync(peliculaId);

            Console.WriteLine("Definicion de variable pelicula.");

            if (cliente == null)
            {
                return BadRequest("El cliente no existe.");
            }

            if (pelicula == null)
            {
                return BadRequest("La película no existe.");
            }

            if (pelicula.Alquilado)
            {
                return BadRequest("La película ya está alquilada.");
            }

            Console.WriteLine("Filtros de Peliculas");

            // Alquilar la película
            cliente.PeliculasAlquiladas.Add(pelicula);
            pelicula.Alquilado = true;

            Console.WriteLine("Accion de alquilar pelicula");

            // Marcar como modificados
            _context.Entry(cliente).State = EntityState.Modified;
            _context.Entry(pelicula).State = EntityState.Modified;

            Console.WriteLine("Marcar datos como modificados.");

            // Guardar cambios
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExiste(clienteId))
                {
                    return NotFound("El cliente no existe.");
                }
                throw;
            }

            Console.WriteLine("Datos 'Guardados'");

            return NoContent();


        }

        //Desalquilar una pelicula de un cliente
        //PUT: api/Cliente
        [HttpPut("{clienteId}/Desalquilar-pelicula/{peliculaId}")]
        public async Task<IActionResult> DesalquilarPelicula(string clienteId, string peliculaId)
        {
            var cliente = await _context.Clientes
                            .Include(c => c.PeliculasAlquiladas)
                            .FirstOrDefaultAsync(c => c.Id == clienteId);

            var pelicula = await _context.Peliculas.FindAsync(peliculaId);

            if (cliente == null)
            {
                return BadRequest("El cliente no existe.");
            }

            if (pelicula == null)
            {
                return BadRequest("La pelicula no existe");
            }

            if (!cliente.PeliculasAlquiladas.Contains(pelicula))
            {
                return BadRequest("El cliente no tiene alquilada esta pelicula");
            }

            // Desalquilar la película
            cliente.PeliculasAlquiladas.Remove(pelicula);
            pelicula.Alquilado = false;

            // Marcar como modificados
            _context.Entry(cliente).State = EntityState.Modified;
            _context.Entry(pelicula).State = EntityState.Modified;

            // Guardar cambios
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExiste(clienteId))
                {
                    return NotFound("El cliente no existe.");
                }
                throw;
            }

            return NoContent();
        }

        //ELiminar un cliente
        //DELETE: api/Cliente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(string id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.PeliculasAlquiladas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();        }

        private bool ClienteExiste(string id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}


