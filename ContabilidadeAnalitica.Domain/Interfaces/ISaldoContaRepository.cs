using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface ISaldoContaRepository : IRepository<SaldoConta>
{
    Task<IReadOnlyList<SaldoConta>> ObterPorEmpresaEAnosAsync(
        Guid empresaId, IEnumerable<int> anos, CancellationToken ct = default);

    Task<IReadOnlyList<SaldoConta>> ObterPorContaEPeriodoAsync(
        Guid contaId, int ano, int? mes = null, CancellationToken ct = default);

    Task<SaldoConta?> ObterSaldoEspecificoAsync(
        Guid contaId, Guid empresaId, int ano, int mes, CancellationToken ct = default);

    Task AdicionarOuAtualizarAsync(SaldoConta saldo, CancellationToken ct = default);
}
