using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Application.UseCases.PlanoContas;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadeAnalitica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlanoContasController : ControllerBase
{
    private readonly PlanoContasUseCases _planoUseCases;
    private readonly ContaContabilUseCases _contaUseCases;

    public PlanoContasController(PlanoContasUseCases planoUseCases, ContaContabilUseCases contaUseCases)
    {
        _planoUseCases = planoUseCases;
        _contaUseCases = contaUseCases;
    }

    // ── Plano de Contas ──

    /// <summary>
    /// Lista planos de contas de uma empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PlanoContasResumoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorEmpresa(Guid empresaId, CancellationToken ct)
    {
        var resultado = await _planoUseCases.ListarPorEmpresaAsync(empresaId, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Obtém o plano de contas vigente de uma empresa com todas as contas.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/vigente")]
    [ProducesResponseType(typeof(ApiResponse<PlanoContasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterVigente(Guid empresaId, CancellationToken ct)
    {
        var resultado = await _planoUseCases.ObterVigenteAsync(empresaId, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Obtém um plano de contas por ID com todas as contas.
    /// </summary>
    [HttpGet("{planoId:guid}", Name = "ObterPlanoContas")]
    [ProducesResponseType(typeof(ApiResponse<PlanoContasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid planoId, CancellationToken ct)
    {
        var resultado = await _planoUseCases.ObterComContasAsync(planoId, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Cria um novo plano de contas (versiona automaticamente).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PlanoContasResumoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarPlanoContasDto dto, CancellationToken ct)
    {
        var resultado = await _planoUseCases.CriarAsync(dto, ct);
        return resultado.ToCreatedResult("ObterPlanoContas", p => new { planoId = p.Id });
    }

    // ── Contas Contábeis ──

    /// <summary>
    /// Lista contas de um plano.
    /// </summary>
    [HttpGet("{planoId:guid}/contas")]
    [ProducesResponseType(typeof(ApiResponse<List<ContaContabilDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarContas(Guid planoId, CancellationToken ct)
    {
        var resultado = await _contaUseCases.ListarPorPlanoAsync(planoId, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Obtém uma conta contábil por ID.
    /// </summary>
    [HttpGet("contas/{contaId:guid}", Name = "ObterConta")]
    [ProducesResponseType(typeof(ApiResponse<ContaContabilDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterConta(Guid contaId, CancellationToken ct)
    {
        var resultado = await _contaUseCases.ObterPorIdAsync(contaId, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Cria uma nova conta contábil.
    /// </summary>
    [HttpPost("contas")]
    [ProducesResponseType(typeof(ApiResponse<ContaContabilDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaContabilDto dto, CancellationToken ct)
    {
        var resultado = await _contaUseCases.CriarAsync(dto, ct);
        return resultado.ToCreatedResult("ObterConta", c => new { contaId = c.Id });
    }

    /// <summary>
    /// Atualiza uma conta contábil.
    /// </summary>
    [HttpPut("contas/{contaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarConta(Guid contaId, [FromBody] AtualizarContaContabilDto dto, CancellationToken ct)
    {
        var resultado = await _contaUseCases.AtualizarAsync(contaId, dto, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Remove uma conta contábil.
    /// </summary>
    [HttpDelete("contas/{contaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoverConta(Guid contaId, CancellationToken ct)
    {
        var resultado = await _contaUseCases.RemoverAsync(contaId, ct);
        return resultado.ToActionResult();
    }
}
