using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using localizeBackendAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using localizeBackendAPI.DTOs;
using Newtonsoft.Json;

namespace localizeBackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly LocalizeBackendContext _context;
        private readonly ExceptionController _exceptionController;

        public EmpresasController(LocalizeBackendContext context, ExceptionController exceptionController)
        {
            _context = context;
            _exceptionController = exceptionController;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        {
            return await _context.Empresas.Include(e => e.Usuario).ToListAsync();
        }

        [HttpGet("consultar-cnpj/{cnpj}")]
        [Authorize]
        public async Task<IActionResult> ConsultarCnpj(string cnpj)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"https://www.receitaws.com.br/v1/cnpj/{cnpj}");

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Erro ao consultar CNPJ");

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonConvert.DeserializeObject<ReceitaWsResponse>(json);

                if (dados == null || dados.Status == "ERROR")
                    return BadRequest(dados?.Message ?? "Dados inválidos da ReceitaWS");

                return Ok(new
                {
                    nome = string.IsNullOrWhiteSpace(dados.Nome) ? "Não informado" : dados.Nome,
                    fantasia = string.IsNullOrWhiteSpace(dados.Fantasia) ? "Não informado" : dados.Fantasia,
                    cnpj = string.IsNullOrWhiteSpace(dados.Cnpj) ? "Não informado" : dados.Cnpj,
                    situacao = string.IsNullOrWhiteSpace(dados.Situacao) ? "Não informado" : dados.Situacao,
                    abertura = string.IsNullOrWhiteSpace(dados.Abertura) ? "Não informado" : dados.Abertura,
                    tipo = string.IsNullOrWhiteSpace(dados.Tipo) ? "Não informado" : dados.Tipo,
                    natureza_juridica = string.IsNullOrWhiteSpace(dados.NaturezaJuridica) ? "Não informado" : dados.NaturezaJuridica,
                    atividade_principal = dados.AtividadesPrincipais?.Any() == true
                        ? dados.AtividadesPrincipais
                        : new List<AtividadeDto> { new AtividadeDto { Code = "", Text = "Não informado" } },
                    logradouro = string.IsNullOrWhiteSpace(dados.Logradouro) ? "Não informado" : dados.Logradouro,
                    numero = string.IsNullOrWhiteSpace(dados.Numero) ? "Não informado" : dados.Numero,
                    complemento = string.IsNullOrWhiteSpace(dados.Complemento) ? "Não informado" : dados.Complemento,
                    bairro = string.IsNullOrWhiteSpace(dados.Bairro) ? "Não informado" : dados.Bairro,
                    municipio = string.IsNullOrWhiteSpace(dados.Municipio) ? "Não informado" : dados.Municipio,
                    uf = string.IsNullOrWhiteSpace(dados.Uf) ? "Não informado" : dados.Uf,
                    cep = string.IsNullOrWhiteSpace(dados.Cep) ? "Não informado" : dados.Cep,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CadastrarEmpresa([FromBody] EmpresaCadastroDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Erro ao obter informações da empresa.");

                var empresaExistente = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.Cnpj == dto.Cnpj && e.UsuarioId == dto.UsuarioId);

                if (empresaExistente != null)
                {
                    if (empresaExistente.Ativo)
                        return BadRequest("Você já cadastrou uma empresa com este CNPJ.");

                    empresaExistente.NomeEmpresarial = dto.NomeEmpresarial ?? "Não informado";
                    empresaExistente.NomeFantasia = string.IsNullOrWhiteSpace(dto.NomeFantasia) ? "Não informado" : dto.NomeFantasia;
                    empresaExistente.Situacao = dto.Situacao ?? "Não informado";
                    empresaExistente.Abertura = dto.Abertura ?? "Não informado";
                    empresaExistente.Tipo = dto.Tipo ?? "Não informado";
                    empresaExistente.NaturezaJuridica = dto.NaturezaJuridica ?? "Não informado";
                    empresaExistente.AtividadePrincipal = dto.AtividadesPrincipais?.FirstOrDefault()?.Text ?? "Não informado";
                    empresaExistente.Logradouro = dto.Logradouro ?? "Não informado";
                    empresaExistente.Numero = dto.Numero ?? "Não informado";
                    empresaExistente.Complemento = dto.Complemento ?? "Não informado";
                    empresaExistente.Bairro = dto.Bairro ?? "Não informado";
                    empresaExistente.Municipio = dto.Municipio ?? "Não informado";
                    empresaExistente.Uf = dto.Uf?.Length > 2 ? dto.Uf.Substring(0, 2) : dto.Uf;
                    empresaExistente.Cep = dto.Cep ?? "Não informado";
                    empresaExistente.Ativo = true;

                    _context.Entry(empresaExistente).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return Ok(empresaExistente);
                }

                var empresa = new Empresa
                {
                    Id = Guid.NewGuid(),
                    NomeEmpresarial = dto.NomeEmpresarial ?? "Não informado",
                    NomeFantasia = string.IsNullOrWhiteSpace(dto.NomeFantasia) ? "Não informado" : dto.NomeFantasia,
                    Cnpj = dto.Cnpj ?? "Não informado",
                    Situacao = dto.Situacao ?? "Não informado",
                    Abertura = dto.Abertura ?? "Não informado",
                    Tipo = dto.Tipo ?? "Não informado",
                    NaturezaJuridica = dto.NaturezaJuridica ?? "Não informado",
                    AtividadePrincipal = dto.AtividadesPrincipais?.FirstOrDefault()?.Text ?? "Não informado",
                    Logradouro = dto.Logradouro ?? "Não informado",
                    Numero = dto.Numero ?? "Não informado",
                    Complemento = dto.Complemento ?? "Não informado",
                    Bairro = dto.Bairro ?? "Não informado",
                    Municipio = dto.Municipio ?? "Não informado",
                    Uf = dto.Uf?.Length > 2 ? dto.Uf.Substring(0, 2) : dto.Uf,
                    Cep = dto.Cep ?? "Não informado",
                    UsuarioId = dto.UsuarioId,
                    Ativo = true
                };

                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.Id }, empresa);

            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEmpresa(Guid id)
        {
            try
            {
                var empresa = await _context.Empresas.FindAsync(id);
                return empresa == null ? NotFound() : Ok(empresa);
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }


        [HttpGet("minhas")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetMinhasEmpresas()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guidId))
                    return Unauthorized();

                var empresas = await _context.Empresas
                    .Where(e => e.UsuarioId == guidId)
                    .ToListAsync();

                if (empresas.Count == 0)
                    return Ok(new List<Empresa>());

                return empresas;
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutEmpresa(Guid id, Empresa empresa)
        {
            try
            {
                if (id != empresa.Id)
                    return BadRequest();

                _context.Entry(empresa).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpresaExists(id))
                        return NotFound();
                    else
                        throw;
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmpresa(Guid id)
        {
            try
            {
                var empresa = await _context.Empresas.FindAsync(id);
                if (empresa == null)
                    return NotFound();

                _context.Empresas.Remove(empresa);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }

        [HttpPut("InactiveEmpresas")]
        [Authorize]
        public async Task<IActionResult>InactiveEmpresa(Guid id)
        {
            try
            {
                var empresa = await _context.Empresas.FindAsync(id);
                if (empresa == null)
                    return NotFound();
                empresa.Ativo = false;
                _context.Entry(empresa).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(_exceptionController.GetFullExceptionMessage(ex));
            }
        }
        private bool EmpresaExists(Guid id)
        {
            try
            {
                return _context.Empresas.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception(_exceptionController.GetFullExceptionMessage(ex));
            }
        }
    }
}