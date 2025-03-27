using BibliotecaDigital.Data;
using BibliotecaDigital.DTOs;
using BibliotecaDigital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaDigital.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    // Este es el controlador que maneja las operaciones relacionadas con la autenticacion
    // La ruta base para este controlador es `api/auth`
    public class AuthController : ControllerBase
    {
        // Estos variables privados van a contener las dependencias que necesita el controlador
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        // Aca el constructor que recibe las dependencias necesarias
        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // endpoint para registrar un usuario
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            try
            {
                // validamos que el email no este registrado (ya que habiamos definido que era unico)
                if (await _context.Usuarios.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return BadRequest("El email ya está registrado");
                }
                // si llegamos hasta aca es porque no existe el usuario, por ende tomamos los datos que nos llega para luego crearlo
                var usuario = new Usuario
                {
                    Nombre = registerDto.Nombre,
                    Email = registerDto.Email,
                    Password = registerDto.Password,
                    Estado = EstadoUsuario.Activo
                };

                // aca lo agregamos a la base de datos
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // y lo registramos este evento en el log
                _logger.LogInformation("Usuario registrado en PostgreSQL: {Email}", registerDto.Email);

                // aca iria el envio de email pero no llegue a implementarlo 🥺, un ejemplo seria:
                // await _emailService.SendWelcomeEmail(usuario.Email, usuario.Nombre);

                // devolvemos respuesta exitosa
                return Ok(new { message = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                // registramos los errores en el log y retornamos el error
                _logger.LogError(ex, "Error al registrar usuario en PostgreSQL");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // enpoint donde se inicia sesion
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            try
            {
                // buscamos el mail del usuario en la base de datos  
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                // verificamos que el usuario exista o sus credenciales coincidan
                if (usuario == null || usuario.Password != loginDto.Password)
                {
                    return Unauthorized("Credenciales inválidas");
                }

                // verificamos que este activo
                if (usuario.Estado == EstadoUsuario.Inactivo)
                {
                    return Unauthorized("Usuario inactivo");
                }


                // si llegamos hasta aca es porque el usuario existe y sus credenciales son correctas por ende generamos el token y luego armamos el response
                var token = GenerateJwtToken(usuario);

                _logger.LogInformation("Usuario autenticado: {Email}", loginDto.Email);

                return Ok(new AuthResponseDTO
                {
                    Token = token,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre
                });
            }
            catch (Exception ex)
            {
                // registramos los errores en el log y retornamos el error
                _logger.LogError(ex, "Error al autenticar usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // Metodo para generar el token
        private string GenerateJwtToken(Usuario usuario)
        {

            // Creamos una clave de seguridad a partir de la clave secreta en la configuracion de nuestro proyecto
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Aca definimos una fecha de expiracion, normalmente minutos, que obtenemos desde la configuracion (60 min)
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]));


            // Definimos las claims o info (payload) que contendra el token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("nombre", usuario.Nombre)
            };

            // Creamos el token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // retornamos el token convertido a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}