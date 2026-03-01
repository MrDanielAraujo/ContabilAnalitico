using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Application.UseCases.Demonstrativos;
using ContabilidadeAnalitica.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadeAnalitica.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DemonstrativosController : ControllerBase
{
    private readonly DemonstrativoUseCases _useCases;

    public DemonstrativosController(DemonstrativoUseCases useCases)
    {
        _useCases = useCases;
    }

    /// <summary>
    /// Gera um demonstrativo financeiro (Balanço ou Balancete) para uma empresa.
    /// Aceita anos dinâmicos informados pelo consumidor.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/demonstrativos
    ///     {
    ///         "empresaId": "b1000000-0000-0000-0000-000000000001",
    ///         "tipo": 1,
    ///         "anos": [2024, 2025, 2026],
    ///         "templateId": null
    ///     }
    /// 
    /// Tipos: 1 = Balanço, 2 = Balancete, 3 = DRE, 4 = Fluxo de Caixa
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DemonstrativoResultadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErro), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Gerar([FromBody] ConsultaDemonstrativoDto dto, CancellationToken ct)
    {
        var resultado = await _useCases.GerarAsync(dto, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Atalho GET para consulta rápida de Balanço.
    /// </summary>
    [HttpGet("balanco/{empresaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DemonstrativoResultadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Balanco(
        Guid empresaId,
        [FromQuery] string anos,
        [FromQuery] Guid? templateId,
        CancellationToken ct)
    {
        var anosLista = ParseAnos(anos);
        if (anosLista.Count == 0)
            return BadRequest(new ApiErro("Validação", new List<string> { "Informe ao menos um ano. Ex: ?anos=2024,2025,2026" }));

        var dto = new ConsultaDemonstrativoDto(empresaId, TipoDemonstrativo.Balanco, anosLista, templateId);
        var resultado = await _useCases.GerarAsync(dto, ct);
        return resultado.ToActionResult();
    }

    /// <summary>
    /// Atalho GET para consulta rápida de Balancete.
    /// </summary>
    [HttpGet("balancete/{empresaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DemonstrativoResultadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Balancete(
        Guid empresaId,
        [FromQuery] string anos,
        [FromQuery] Guid? templateId,
        CancellationToken ct)
    {
        var anosLista = ParseAnos(anos);
        if (anosLista.Count == 0)
            return BadRequest(new ApiErro("Validação", new List<string> { "Informe ao menos um ano. Ex: ?anos=2024,2025,2026" }));

        var dto = new ConsultaDemonstrativoDto(empresaId, TipoDemonstrativo.Balancete, anosLista, templateId);
        var resultado = await _useCases.GerarAsync(dto, ct);
        return resultado.ToActionResult();
    }

    private static List<int> ParseAnos(string? anos)
    {
        if (string.IsNullOrWhiteSpace(anos))
            return new List<int>();

        return anos.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(a => int.TryParse(a.Trim(), out var ano) ? ano : 0)
            .Where(a => a > 0)
            .Distinct()
            .OrderBy(a => a)
            .ToList();
    }
}
