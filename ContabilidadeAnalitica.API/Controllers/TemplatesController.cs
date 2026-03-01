using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Application.UseCases.Templates;
using ContabilidadeAnalitica.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadeAnalitica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TemplatesController : ControllerBase
{
    private readonly TemplateUseCases _useCases;

    public TemplatesController(TemplateUseCases useCases)
    {
        _useCases = useCases;
    }

    /// <summary>
    /// Lista templates por tipo de demonstrativo.
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(ApiResponse<List<TemplateDemonstrativoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorTipo(TipoDemonstrativo tipo, CancellationToken ct)
    {
        var resultado = await _useCases.ListarPorTipoAsync(tipo, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Obtém um template por ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = "ObterTemplate")]
    [ProducesResponseType(typeof(ApiResponse<TemplateDemonstrativoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _useCases.ObterPorIdAsync(id, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Cria um novo template de demonstrativo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TemplateDemonstrativoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarTemplateDemonstrativoDto dto, CancellationToken ct)
    {
        var resultado = await _useCases.CriarAsync(dto, ct);
        return resultado.ToCreatedResult("ObterTemplate", t => new { id = t.Id });
    }
}
