using BibliotecaDigital.Data;
using BibliotecaDigital.DTOs;
using BibliotecaDigital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaDigital.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ApplicationDbContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // endpoint para obtener un listado de libros con opciones de filtro
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LibroResumenDTO>>> GetLibros(
            [FromQuery] string title = null,
            [FromQuery] string isbn = null,
            [FromQuery] int? author = null,
            [FromQuery] int? genre = null)
        {
            try
            {
                // hacemos una consulta para traer todos los libros
                var query = _context.Libros.AsQueryable();

                // se aplican los filtros segun lo que se reciba por parametro

                // este es un filtro para buscar libros por titulo
                if (!string.IsNullOrEmpty(title))
                {
                    query = query.Where(l => l.Titulo.Contains(title));
                }

                // este es un filtro para buscar libros por ISBN
                if (!string.IsNullOrEmpty(isbn))
                {
                    query = query.Where(l => l.ISBN == isbn);
                }


                // este es un filtro para buscar libros por autor
                if (author.HasValue)
                {
                    query = query.Where(l => l.AutorId == author.Value);
                }
                // filtro por genero
                if (genre.HasValue)
                {
                    query = query.Where(l => l.GeneroId == genre.Value);
                }

                // Mandamos los resultados al DTO de resumen
                var libros = await query
                    .Select(l => new LibroResumenDTO
                    {
                        Id = l.Id,
                        Titulo = l.Titulo,
                        AnioPublicacion = l.AnioPublicacion,
                        Imagen = l.Imagen,
                        Disponible = l.Stock > 0
                    })
                    .ToListAsync();

                return Ok(libros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener libros de Supabase");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Endpoint para obtener los detalles de un libro especificado en su ID
        [HttpGet("{id}")]
        public async Task<ActionResult<LibroDetalleDTO>> GetLibro(int id)
        {
            try
            {
                // hacemos una consulta para traer todos los libros y buscamos el libro por ID
                var libro = await _context.Libros
                    .Include(l => l.Autor)
                    .Include(l => l.Genero)
                    .FirstOrDefaultAsync(l => l.Id == id);

                // validamos que se encuentre el libro
                if (libro == null)
                {
                    return NotFound();
                }

                // Si encontramos el libro mandamos los resultados al DTO
                var libroDetalle = new LibroDetalleDTO
                {
                    Id = libro.Id,
                    Titulo = libro.Titulo,
                    Resumen = libro.Resumen,
                    AnioPublicacion = libro.AnioPublicacion,
                    Imagen = libro.Imagen,
                    ISBN = libro.ISBN,
                    Stock = libro.Stock,
                    Autor = new AutorDTO
                    {
                        Id = libro.Autor.Id,
                        Nombre = libro.Autor.Nombre,
                        Nacionalidad = libro.Autor.Nacionalidad,
                        FechaNacimiento = libro.Autor.FechaNacimiento
                    },
                    Genero = new GeneroDTO
                    {
                        Id = libro.Genero.Id,
                        Nombre = libro.Genero.Nombre,
                        Descripcion = libro.Genero.Descripcion
                    }
                };

                return Ok(libroDetalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener libro con ID: {Id} de Supabase", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Endpoitn para crear un nuevo libro, aca se requiere que el usuario este autenticado
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Libro>> CreateLibro(LibroDTO libroDto)
        {
            try
            {
                // Validamos que autor y genero existan
                var autorExists = await _context.Autores.AnyAsync(a => a.Id == libroDto.AutorId);
                var generoExists = await _context.Generos.AnyAsync(g => g.Id == libroDto.GeneroId);

                if (!autorExists || !generoExists)
                {
                    return BadRequest("El autor o género especificado no existe");
                }

                // Chequeamos que el ISBN no exista
                if (await _context.Libros.AnyAsync(l => l.ISBN == libroDto.ISBN))
                {
                    return BadRequest("El ISBN ya existe");
                }

                // Cremos el libro con los datos que nos proporcionaron
                var libro = new Libro
                {
                    Titulo = libroDto.Titulo,
                    Resumen = libroDto.Resumen,
                    AnioPublicacion = libroDto.AnioPublicacion,
                    Imagen = libroDto.Imagen,
                    ISBN = libroDto.ISBN,
                    Stock = libroDto.Stock,
                    AutorId = libroDto.AutorId,
                    GeneroId = libroDto.GeneroId
                };

                // Agregamos el libro a la base de datos
                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();

                // Registramos este evento en el log
                _logger.LogInformation("Libro creado en Supabase: {Titulo}", libro.Titulo);


                // Damos una respuesta con el nuevo libro
                return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, libro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear libro en Supabase");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Metodo para actualizar un libro
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateLibro(int id, LibroDTO libroDto)
        {
            try
            {
                // Buscamos el libro por ID
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    return NotFound();
                }

                // Validamos que genero y autor existan
                var autorExists = await _context.Autores.AnyAsync(a => a.Id == libroDto.AutorId);
                var generoExists = await _context.Generos.AnyAsync(g => g.Id == libroDto.GeneroId);

                if (!autorExists || !generoExists)
                {
                    return BadRequest("El autor o género especificado no existe");
                }

                // Verificamos si el ISBN es unico o se modifico
                if (libro.ISBN != libroDto.ISBN && await _context.Libros.AnyAsync(l => l.ISBN == libroDto.ISBN))
                {
                    return BadRequest("El ISBN ya existe");
                }

                // Actualizamos los datos del libro

                libro.Titulo = libroDto.Titulo;
                libro.Resumen = libroDto.Resumen;
                libro.AnioPublicacion = libroDto.AnioPublicacion;
                libro.Imagen = libroDto.Imagen;
                libro.ISBN = libroDto.ISBN;
                libro.Stock = libroDto.Stock;
                libro.AutorId = libroDto.AutorId;
                libro.GeneroId = libroDto.GeneroId;

                // Guardamos los cambios
                await _context.SaveChangesAsync();


                // Registramos este cambio en el log
                _logger.LogInformation("Libro actualizado en Supabase: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar libro con ID: {Id} en Supabase", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // empoint para eliminar un libro
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteLibro(int id)
        {
            try
            {
                // Buscamos el libro por ID
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    return NotFound();
                }

                // Eliminamos el libro
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();

                // Registramos este cambio en el log
                _logger.LogInformation("Libro eliminado de Supabase: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar libro con ID: {Id} de Supabase", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}