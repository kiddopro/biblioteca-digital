using BibliotecaDigital.Data;
using BibliotecaDigital.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//TODO: Documentar 
namespace BibliotecaDigital.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(ApplicationDbContext context, ILogger<AuthorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Metodo para obtener todos los autores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AutorConLibrosDTO>>> GetAutores()
        {
            try
            {
                // Hacemos una consulta para traer todos los autores
                var autores = await _context.Autores
                    .Select(a => new AutorConLibrosDTO
                    {
                        Id = a.Id,
                        Nombre = a.Nombre,
                        Nacionalidad = a.Nacionalidad,
                        CantidadLibros = a.Libros.Count
                    })
                    .ToListAsync();

                // devolvemos la lista de autores
                return Ok(autores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener autores de Supabase");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}