using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Application.UseCases.Empresas;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadeAnalitica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmpresasController : ControllerBase
{
    private readonly EmpresaUseCases _useCases;

    public EmpresasController(EmpresaUseCases useCases)
    {
        _useCases = useCases;
    }

    /// <summary>
    /// Lista todas as empresas ativas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EmpresaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var resultado = await _useCases.ListarAsync(ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Obtém uma empresa por ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = "ObterEmpresa")]
    [ProducesResponseType(typeof(ApiResponse<EmpresaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _useCases.ObterPorIdAsync(id, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Cria uma nova empresa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmpresaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarEmpresaDto dto, CancellationToken ct)
    {
        var resultado = await _useCases.CriarAsync(dto, ct);
        return resultado.ToCreatedResult("ObterEmpresa", e => new { id = e.Id });
    }

    /// <summary>
    /// Atualiza uma empresa existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarEmpresaDto dto, CancellationToken ct)
    {
        var resultado = await _useCases.AtualizarAsync(id, dto, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Remove (desativa) uma empresa.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        var resultado = await _useCases.RemoverAsync(id, ct);
        return resultado.ToActionResult();
    }
}
