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

        // GET: api/books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LibroResumenDTO>>> GetLibros(
            [FromQuery] string title = null,
            [FromQuery] string isbn = null,
            [FromQuery] int? author = null,
            [FromQuery] int? genre = null)
        {
            try
            {
                var query = _context.Libros.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(title))
                {
                    query = query.Where(l => l.Titulo.Contains(title));
                }

                if (!string.IsNullOrEmpty(isbn))
                {
                    query = query.Where(l => l.ISBN == isbn);
                }

                if (author.HasValue)
                {
                    query = query.Where(l => l.AutorId == author.Value);
                }

                if (genre.HasValue)
                {
                    query = query.Where(l => l.GeneroId == genre.Value);
                }

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

        // GET: api/books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LibroDetalleDTO>> GetLibro(int id)
        {
            try
            {
                var libro = await _context.Libros
                    .Include(l => l.Autor)
                    .Include(l => l.Genero)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (libro == null)
                {
                    return NotFound();
                }

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

        // POST: api/books
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Libro>> CreateLibro(LibroDTO libroDto)
        {
            try
            {
                // Validate if autor and genero exist
                var autorExists = await _context.Autores.AnyAsync(a => a.Id == libroDto.AutorId);
                var generoExists = await _context.Generos.AnyAsync(g => g.Id == libroDto.GeneroId);

                if (!autorExists || !generoExists)
                {
                    return BadRequest("El autor o género especificado no existe");
                }

                // Check if ISBN is unique
                if (await _context.Libros.AnyAsync(l => l.ISBN == libroDto.ISBN))
                {
                    return BadRequest("El ISBN ya existe");
                }

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

                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Libro creado en Supabase: {Titulo}", libro.Titulo);

                return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, libro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear libro en Supabase");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/books/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateLibro(int id, LibroDTO libroDto)
        {
            try
            {
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    return NotFound();
                }

                // Validate if autor and genero exist
                var autorExists = await _context.Autores.AnyAsync(a => a.Id == libroDto.AutorId);
                var generoExists = await _context.Generos.AnyAsync(g => g.Id == libroDto.GeneroId);

                if (!autorExists || !generoExists)
                {
                    return BadRequest("El autor o género especificado no existe");
                }

                // Check if ISBN is unique (if changed)
                if (libro.ISBN != libroDto.ISBN && await _context.Libros.AnyAsync(l => l.ISBN == libroDto.ISBN))
                {
                    return BadRequest("El ISBN ya existe");
                }

                libro.Titulo = libroDto.Titulo;
                libro.Resumen = libroDto.Resumen;
                libro.AnioPublicacion = libroDto.AnioPublicacion;
                libro.Imagen = libroDto.Imagen;
                libro.ISBN = libroDto.ISBN;
                libro.Stock = libroDto.Stock;
                libro.AutorId = libroDto.AutorId;
                libro.GeneroId = libroDto.GeneroId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Libro actualizado en Supabase: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar libro con ID: {Id} en Supabase", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/books/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteLibro(int id)
        {
            try
            {
                var libro = await _context.Libros.FindAsync(id);
                if (libro == null)
                {
                    return NotFound();
                }

                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();

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