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
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            try
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return BadRequest("El email ya está registrado");
                }

                var usuario = new Usuario
                {
                    Nombre = registerDto.Nombre,
                    Email = registerDto.Email,
                    Password = registerDto.Password, // En un caso real, deberías hashear la contraseña
                    Estado = EstadoUsuario.Activo
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario registrado en Supabase: {Email}", registerDto.Email);

                // Opcional: Enviar email de bienvenida
                // await _emailService.SendWelcomeEmail(usuario.Email, usuario.Nombre);

                return Ok(new { message = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario en Supabase");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (usuario == null || usuario.Password != loginDto.Password)
                {
                    return Unauthorized("Credenciales inválidas");
                }

                if (usuario.Estado == EstadoUsuario.Inactivo)
                {
                    return Unauthorized("Usuario inactivo");
                }

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
                _logger.LogError(ex, "Error al autenticar usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("nombre", usuario.Nombre)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}