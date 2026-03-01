using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Application.UseCases.Importacao;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadeAnalitica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ImportacoesController : ControllerBase
{
    private readonly ImportacaoUseCases _useCases;

    public ImportacoesController(ImportacaoUseCases useCases)
    {
        _useCases = useCases;
    }

    /// <summary>
    /// Lista importações de uma empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ImportacaoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorEmpresa(Guid empresaId, CancellationToken ct)
    {
        var resultado = await _useCases.ListarPorEmpresaAsync(empresaId, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Obtém uma importação por ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = "ObterImportacao")]
    [ProducesResponseType(typeof(ApiResponse<ImportacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _useCases.ObterPorIdAsync(id, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Cria e processa uma importação de dados contábeis.
    /// Simula dados vindos de sistema externo via JSON.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ImportacaoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] CriarImportacaoDto dto, CancellationToken ct)
    {
        var resultado = await _useCases.CriarEProcessarAsync(dto, ct);
        return resultado.ToCreatedResult("ObterImportacao", i => new { id = i.Id });
    }
}
