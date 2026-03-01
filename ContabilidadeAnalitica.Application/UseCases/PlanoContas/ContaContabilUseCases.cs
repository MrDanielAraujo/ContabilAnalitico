using ContabilidadeAnalitica.Application.Common;
using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Domain.Interfaces;

namespace ContabilidadeAnalitica.Application.UseCases.PlanoContas;

public class ContaContabilUseCases
{
    private readonly IUnitOfWork _uow;

    public ContaContabilUseCases(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ResultadoOperacao<ContaContabilDto>> CriarAsync(CriarContaContabilDto dto, CancellationToken ct = default)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Codigo))
            erros.Add("Código da conta é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Nome))
            erros.Add("Nome da conta é obrigatório.");

        var plano = await _uow.PlanosContas.ObterPorIdAsync(dto.PlanoContasId, ct);
        if (plano is null)
            return new NaoEncontrado($"Plano de contas '{dto.PlanoContasId}' não encontrado.");

        if (!string.IsNullOrWhiteSpace(dto.Codigo))
        {
            var existe = await _uow.ContasContabeis.ExisteCodigoAsync(dto.PlanoContasId, dto.Codigo, null, ct);
            if (existe)
                return new ConflitoDominio($"Já existe uma conta com o código '{dto.Codigo}' neste plano.");
        }

        if (erros.Count > 0)
            return new ValidacaoInvalida(erros);

        if (dto.ContaPaiId.HasValue)
        {
            var pai = await _uow.ContasContabeis.ObterPorIdAsync(dto.ContaPaiId.Value, ct);
            if (pai is null)
                return new NaoEncontrado($"Conta pai '{dto.ContaPaiId}' não encontrada.");
        }

        // Validar que conta analítica aceita lançamento e sintética não
        if (dto.Tipo == TipoConta.Sintetica && dto.AceitaLancamento)
            erros.Add("Conta sintética não pode aceitar lançamentos diretos.");

        if (erros.Count > 0)
            return new ValidacaoInvalida(erros);

        var nivel = dto.Codigo.Split('.').Length;

        var conta = new ContaContabil
        {
            Codigo = dto.Codigo,
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Natureza = dto.Natureza,
            Subtipo = dto.Subtipo,
            Tipo = dto.Tipo,
            AceitaLancamento = dto.AceitaLancamento,
            ExpressaoCalculo = dto.ExpressaoCalculo,
            Nivel = nivel,
            Ordem = dto.Ordem,
            ContaPaiId = dto.ContaPaiId,
            PlanoContasId = dto.PlanoContasId
        };

        await _uow.ContasContabeis.AdicionarAsync(conta, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso<ContaContabilDto>(MapToDto(conta));
    }

    public async Task<ResultadoOperacao<ContaContabilDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var conta = await _uow.ContasContabeis.ObterPorIdAsync(id, ct);
        if (conta is null)
            return new NaoEncontrado($"Conta contábil '{id}' não encontrada.");

        return new Sucesso<ContaContabilDto>(MapToDto(conta));
    }

    public async Task<ResultadoOperacao<List<ContaContabilDto>>> ListarPorPlanoAsync(Guid planoId, CancellationToken ct = default)
    {
        var contas = await _uow.ContasContabeis.ObterPorPlanoAsync(planoId, ct);
        var dtos = contas.Select(MapToDto).ToList();
        return new Sucesso<List<ContaContabilDto>>(dtos);
    }

    public async Task<ResultadoComando> AtualizarAsync(Guid id, AtualizarContaContabilDto dto, CancellationToken ct = default)
    {
        var conta = await _uow.ContasContabeis.ObterPorIdAsync(id, ct);
        if (conta is null)
            return new NaoEncontrado($"Conta contábil '{id}' não encontrada.");

        if (string.IsNullOrWhiteSpace(dto.Nome))
            return new ValidacaoInvalida("Nome da conta é obrigatório.");

        if (dto.Tipo == TipoConta.Sintetica && dto.AceitaLancamento)
            return new ValidacaoInvalida("Conta sintética não pode aceitar lançamentos diretos.");

        conta.Nome = dto.Nome;
        conta.Descricao = dto.Descricao;
        conta.Natureza = dto.Natureza;
        conta.Subtipo = dto.Subtipo;
        conta.Tipo = dto.Tipo;
        conta.AceitaLancamento = dto.AceitaLancamento;
        conta.ExpressaoCalculo = dto.ExpressaoCalculo;
        conta.Ordem = dto.Ordem;
        conta.AlteradoEm = DateTime.UtcNow;

        await _uow.ContasContabeis.AtualizarAsync(conta, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso();
    }

    public async Task<ResultadoComando> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var conta = await _uow.ContasContabeis.ObterPorIdAsync(id, ct);
        if (conta is null)
            return new NaoEncontrado($"Conta contábil '{id}' não encontrada.");

        var filhas = await _uow.ContasContabeis.ObterFilhasAsync(id, ct);
        if (filhas.Count > 0)
            return new ConflitoDominio("Não é possível remover uma conta que possui contas filhas.");

        await _uow.ContasContabeis.RemoverAsync(id, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso();
    }

    private static ContaContabilDto MapToDto(ContaContabil c) => new(
        c.Id, c.Codigo, c.Nome, c.Descricao, c.Natureza, c.Subtipo, c.Tipo,
        c.AceitaLancamento, c.ExpressaoCalculo, c.Nivel, c.Ordem,
        c.ContaPaiId, c.PlanoContasId,
        c.ContasFilhas?.OrderBy(f => f.Ordem).Select(MapToDto).ToList() ?? new List<ContaContabilDto>()
    );
}
