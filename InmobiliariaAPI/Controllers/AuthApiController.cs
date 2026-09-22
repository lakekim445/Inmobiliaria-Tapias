using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.DTOs;
using InmobiliariaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly InmobiliariaContext _context;
        private readonly IConfiguration _configuration;

        public AuthApiController(InmobiliariaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo);

            if (usuario == null || usuario.PasswordHash != dto.Password)
                return Unauthorized(new { mensaje = "Credenciales incorrectas" });

            var token = GenerarToken(usuario);

            return Ok(new LoginResponseDTO
            {
                Token = token,
                NombreCompleto = usuario.NombreCompleto ?? "",
                Email = usuario.Email ?? "",
                Rol = usuario.Rol?.NombreRol ?? ""
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (emailExiste)
                return BadRequest(new { mensaje = "El email ya está registrado" });

            var nuevoUsuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Telefono = dto.Telefono,
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IdRol = 3
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente" });
        }

        private string GenerarToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? ""),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}