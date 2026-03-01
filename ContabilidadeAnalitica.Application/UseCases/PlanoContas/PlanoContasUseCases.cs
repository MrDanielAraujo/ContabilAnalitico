using ContabilidadeAnalitica.Application.Common;
using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Domain.Interfaces;

namespace ContabilidadeAnalitica.Application.UseCases.PlanoContas;

public class PlanoContasUseCases
{
    private readonly IUnitOfWork _uow;

    public PlanoContasUseCases(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ResultadoOperacao<PlanoContasResumoDto>> CriarAsync(CriarPlanoContasDto dto, CancellationToken ct = default)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Nome))
            erros.Add("Nome do plano de contas é obrigatório.");

        var empresa = await _uow.Empresas.ObterPorIdAsync(dto.EmpresaId, ct);
        if (empresa is null)
            return new NaoEncontrado($"Empresa '{dto.EmpresaId}' não encontrada.");

        if (erros.Count > 0)
            return new ValidacaoInvalida(erros);

        // Encerrar plano vigente anterior
        var planoVigente = await _uow.PlanosContas.ObterVigenteAsync(dto.EmpresaId, null, ct);
        int novaVersao = 1;
        if (planoVigente is not null)
        {
            planoVigente.EncerrarVigencia(dto.VigenciaInicio.AddDays(-1));
            await _uow.PlanosContas.AtualizarAsync(planoVigente, ct);
            novaVersao = planoVigente.Versao + 1;
        }

        var plano = new Domain.Entities.PlanoContas
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            EmpresaId = dto.EmpresaId,
            VigenciaInicio = dto.VigenciaInicio,
            Versao = novaVersao,
            Vigente = true
        };

        await _uow.PlanosContas.AdicionarAsync(plano, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso<PlanoContasResumoDto>(MapToResumoDto(plano));
    }

    public async Task<ResultadoOperacao<PlanoContasDto>> ObterComContasAsync(Guid planoId, CancellationToken ct = default)
    {
        var plano = await _uow.PlanosContas.ObterComContasAsync(planoId, ct);
        if (plano is null)
            return new NaoEncontrado($"Plano de contas '{planoId}' não encontrado.");

        return new Sucesso<PlanoContasDto>(MapToDto(plano));
    }

    public async Task<ResultadoOperacao<PlanoContasDto>> ObterVigenteAsync(Guid empresaId, CancellationToken ct = default)
    {
        var plano = await _uow.PlanosContas.ObterVigenteAsync(empresaId, null, ct);
        if (plano is null)
            return new NaoEncontrado($"Nenhum plano de contas vigente encontrado para a empresa '{empresaId}'.");

        var planoCompleto = await _uow.PlanosContas.ObterComContasAsync(plano.Id, ct);
        return new Sucesso<PlanoContasDto>(MapToDto(planoCompleto!));
    }

    public async Task<ResultadoOperacao<List<PlanoContasResumoDto>>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var planos = await _uow.PlanosContas.ObterPorEmpresaAsync(empresaId, ct);
        var dtos = planos.Select(MapToResumoDto).ToList();
        return new Sucesso<List<PlanoContasResumoDto>>(dtos);
    }

    private static PlanoContasResumoDto MapToResumoDto(Domain.Entities.PlanoContas p) => new(
        p.Id, p.Nome, p.Versao, p.VigenciaInicio, p.VigenciaFim,
        p.Vigente, p.EmpresaId, p.Contas?.Count ?? 0
    );

    private static PlanoContasDto MapToDto(Domain.Entities.PlanoContas p) => new(
        p.Id, p.Nome, p.Descricao, p.Versao, p.VigenciaInicio, p.VigenciaFim,
        p.Vigente, p.EmpresaId,
        p.Contas?.Where(c => c.ContaPaiId == null)
            .OrderBy(c => c.Ordem)
            .Select(c => MapContaDto(c))
            .ToList() ?? new List<ContaContabilDto>()
    );

    private static ContaContabilDto MapContaDto(ContaContabil c) => new(
        c.Id, c.Codigo, c.Nome, c.Descricao, c.Natureza, c.Subtipo, c.Tipo,
        c.AceitaLancamento, c.ExpressaoCalculo, c.Nivel, c.Ordem,
        c.ContaPaiId, c.PlanoContasId,
        c.ContasFilhas?.OrderBy(f => f.Ordem).Select(MapContaDto).ToList() ?? new List<ContaContabilDto>()
    );
}
