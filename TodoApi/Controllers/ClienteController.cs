using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/clientes")]
    [ApiController]
    public class ClienteController: ControllerBase
    {
        private readonly VideoclubDbContext _context;
        public ClienteController(VideoclubDbContext context){
            _context = context;
        }


        //Obtener todos los clientes
        //GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }


        //Obtener un cliente en particular
        //GET: api/clientes/3
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
        //PUT: api/
        
    }
}