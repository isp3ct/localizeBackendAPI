using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using localizeBackendAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using localizeBackendAPI.DTOs;
using System.Security.Cryptography;

namespace localizeBackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly LocalizeBackendContext _context;
        private readonly ExceptionController _exceptionController;

        private readonly IConfiguration _configuration;


        public UsuariosController(LocalizeBackendContext context, IConfiguration configuration, ExceptionController exceptionController)
        {
            _context = context;
            _configuration = configuration;
            _exceptionController = exceptionController;
        }

        // GET: api/Usuarios
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            try
            {
                return await _context.Usuarios.ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        // GET: api/Usuarios/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Usuario>> GetUsuario(Guid id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                    return NotFound();
                return usuario;
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }

        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            try
            {
                usuario.Id = Guid.NewGuid();
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }

        }

        // POST: api/Usuarios/register
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                    return BadRequest("E-mail já cadastrado.");

                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = request.Nome,
                    Email = request.Email,
                    SenhaHash = HashPassword(request.Senha),
                    Ativo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(usuario);

                return Ok(new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        // GET: api/Usuarios/perfil
        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult<Usuario>> GetPerfil()
        {
            try
            {
                // Recupera o usuário logado pelo token JWT
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guidId))
                    return Unauthorized();

                var usuario = await _context.Usuarios.FindAsync(guidId);
                if (usuario == null)
                    return NotFound();

                return usuario;
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        // PUT: api/Usuarios/perfil
        [HttpPut("perfil")]
        [Authorize]
        public async Task<IActionResult> AtualizarPerfil([FromBody] UpdateProfileRequest request)
        {
            try
            {
                // Recupera o usuário logado pelo token JWT
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guidId))
                    return Unauthorized();

                var usuario = await _context.Usuarios.FindAsync(guidId);
                if (usuario == null)
                    return NotFound();

                // Atualiza nome e email
                if (!string.IsNullOrWhiteSpace(request.Nome))
                    usuario.Nome = request.Nome;

                if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != usuario.Email)
                {
                    // Verifica se o novo e-mail já está em uso
                    if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email && u.Id != usuario.Id))
                        return BadRequest("E-mail já cadastrado.");
                    usuario.Email = request.Email;
                }

                // Atualiza senha se solicitado
                if (!string.IsNullOrWhiteSpace(request.SenhaNova) || !string.IsNullOrWhiteSpace(request.SenhaConfirmacao))
                {
                    if (string.IsNullOrWhiteSpace(request.SenhaAtual))
                        return BadRequest("Digite sua senha atual.");

                    if (HashPassword(request.SenhaAtual) != usuario.SenhaHash)
                        return BadRequest("Senha atual incorreta.");

                    if (request.SenhaNova.Length < 6)
                        return BadRequest("A nova senha deve ter pelo menos 6 caracteres.");

                    if (request.SenhaNova != request.SenhaConfirmacao)
                        return BadRequest("A confirmação da senha não corresponde à nova senha.");

                    usuario.SenhaHash = HashPassword(request.SenhaNova);
                }

                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        // DELETE: api/Usuarios/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUsuario(Guid id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                    return NotFound();

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null || usuario.SenhaHash != HashPassword(request.Senha))
                    return Unauthorized();

                if (!usuario.Ativo)
                    return Forbid();

                var token = GenerateJwtToken(usuario);

                return Ok(new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("nome", usuario.Nome)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}