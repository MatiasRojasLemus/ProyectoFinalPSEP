using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/Cliente")]
    [ApiController]
    public class ClienteController: ControllerBase
    {
        private readonly VideoclubDbContext _context;
        public ClienteController(VideoclubDbContext context){
            _context = context;
        }


        //Obtener todos los clientes
        //GET: api/Cliente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }


        //Obtener un cliente en particular
        //GET: api/Cliente/3
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(long id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if(cliente == null){
                return NotFound();
            }

            return cliente;
        }

        //Añadir un cliente
        //POST: api/Cliente
        [HttpPost]
        public async Task<ActionResult<Cliente>> AnadirCliente(Cliente cliente){
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCliente), new {id = cliente.Id}, cliente);
        }

        //Modificar un cliente por id
        [HttpPut("{id}")]
        public async Task<IActionResult> ModificarCliente(long id, Cliente cliente)
        {
            //Comprobar si el id del parametro y el id del cliente coincide
            if(id != cliente.Id){
                return BadRequest();
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!ClienteExiste(id)){
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        //ELiminar un cliente
        //DELETE: api/Cliente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(long id){
            var cliente = await _context.Clientes.FindAsync(id);
            
            if(cliente == null){
                return NotFound();
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClienteExiste(long id){
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}