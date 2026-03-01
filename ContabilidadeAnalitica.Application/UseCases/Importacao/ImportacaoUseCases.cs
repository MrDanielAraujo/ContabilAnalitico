using ContabilidadeAnalitica.Application.Common;
using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Domain.Interfaces;

namespace ContabilidadeAnalitica.Application.UseCases.Importacao;

public class ImportacaoUseCases
{
    private readonly IUnitOfWork _uow;

    public ImportacaoUseCases(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ResultadoOperacao<ImportacaoDto>> CriarEProcessarAsync(CriarImportacaoDto dto, CancellationToken ct = default)
    {
        var erros = new List<string>();

        var empresa = await _uow.Empresas.ObterPorIdAsync(dto.EmpresaId, ct);
        if (empresa is null)
            return new NaoEncontrado($"Empresa '{dto.EmpresaId}' não encontrada.");

        if (dto.Itens == null || dto.Itens.Count == 0)
            return new ValidacaoInvalida("A importação deve conter ao menos um item.");

        // Obter plano vigente para validar códigos de conta
        var plano = await _uow.PlanosContas.ObterVigenteAsync(dto.EmpresaId, null, ct);
        if (plano is null)
            return new NaoEncontrado($"Nenhum plano de contas vigente encontrado para a empresa '{dto.EmpresaId}'.");

        var contasPlano = await _uow.ContasContabeis.ObterPorPlanoAsync(plano.Id, ct);
        var contasPorCodigo = contasPlano.ToDictionary(c => c.Codigo, c => c);

        var importacao = new Domain.Entities.Importacao
        {
            EmpresaId = dto.EmpresaId,
            Descricao = dto.Descricao,
            SistemaOrigem = dto.SistemaOrigem,
            Status = StatusImportacao.Processando,
            TotalItens = dto.Itens.Count
        };

        var itens = new List<ImportacaoItem>();
        int processados = 0;
        int comErro = 0;

        foreach (var itemDto in dto.Itens)
        {
            var item = new ImportacaoItem
            {
                ImportacaoId = importacao.Id,
                CodigoConta = itemDto.CodigoConta,
                Ano = itemDto.Ano,
                Mes = itemDto.Mes,
                Valor = itemDto.Valor,
                SaldoDevedor = itemDto.SaldoDevedor,
                SaldoCredor = itemDto.SaldoCredor,
                Moeda = itemDto.Moeda
            };

            if (!contasPorCodigo.TryGetValue(itemDto.CodigoConta, out var conta))
            {
                item.Erro = true;
                item.MensagemErro = $"Conta '{itemDto.CodigoConta}' não encontrada no plano de contas vigente.";
                comErro++;
            }
            else if (!conta.AceitaLancamento)
            {
                item.Erro = true;
                item.MensagemErro = $"Conta '{itemDto.CodigoConta}' não aceita lançamentos diretos (conta sintética).";
                comErro++;
            }
            else
            {
                // Criar ou atualizar saldo
                var saldoExistente = await _uow.SaldosContas.ObterSaldoEspecificoAsync(
                    conta.Id, dto.EmpresaId, itemDto.Ano, itemDto.Mes, ct);

                if (saldoExistente is not null)
                {
                    saldoExistente.Valor = itemDto.Valor;
                    saldoExistente.SaldoDevedor = itemDto.SaldoDevedor;
                    saldoExistente.SaldoCredor = itemDto.SaldoCredor;
                    saldoExistente.Moeda = itemDto.Moeda;
                    saldoExistente.Origem = "Importacao";
                    saldoExistente.ImportacaoId = importacao.Id;
                    saldoExistente.AlteradoEm = DateTime.UtcNow;
                    await _uow.SaldosContas.AtualizarAsync(saldoExistente, ct);
                }
                else
                {
                    var saldo = new SaldoConta
                    {
                        ContaContabilId = conta.Id,
                        EmpresaId = dto.EmpresaId,
                        Ano = itemDto.Ano,
                        Mes = itemDto.Mes,
                        Valor = itemDto.Valor,
                        SaldoDevedor = itemDto.SaldoDevedor,
                        SaldoCredor = itemDto.SaldoCredor,
                        Moeda = itemDto.Moeda,
                        Origem = "Importacao",
                        ImportacaoId = importacao.Id
                    };
                    await _uow.SaldosContas.AdicionarAsync(saldo, ct);
                }

                item.Processado = true;
                processados++;
            }

            itens.Add(item);
        }

        importacao.Itens = itens;
        importacao.ItensProcessados = processados;
        importacao.ItensComErro = comErro;
        importacao.ProcessadoEm = DateTime.UtcNow;
        importacao.Status = comErro > 0 ? StatusImportacao.ConcluidaComErros : StatusImportacao.Concluida;

        await _uow.Importacoes.AdicionarAsync(importacao, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso<ImportacaoDto>(MapToDto(importacao));
    }

    public async Task<ResultadoOperacao<ImportacaoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var importacao = await _uow.Importacoes.ObterComItensAsync(id, ct);
        if (importacao is null)
            return new NaoEncontrado($"Importação '{id}' não encontrada.");

        return new Sucesso<ImportacaoDto>(MapToDto(importacao));
    }

    public async Task<ResultadoOperacao<List<ImportacaoDto>>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var importacoes = await _uow.Importacoes.ObterPorEmpresaAsync(empresaId, ct);
        var dtos = importacoes.Select(MapToDto).ToList();
        return new Sucesso<List<ImportacaoDto>>(dtos);
    }

    private static ImportacaoDto MapToDto(Domain.Entities.Importacao i) => new(
        i.Id, i.EmpresaId, i.Descricao, i.SistemaOrigem, i.Status,
        i.TotalItens, i.ItensProcessados, i.ItensComErro,
        i.CriadoEm, i.ProcessadoEm, i.MensagemErro
    );
}
