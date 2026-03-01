using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface IContaContabilRepository : IRepository<ContaContabil>
{
    Task<ContaContabil?> ObterPorCodigoAsync(Guid planoId, string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<ContaContabil>> ObterPorPlanoAsync(Guid planoId, CancellationToken ct = default);
    Task<IReadOnlyList<ContaContabil>> ObterFilhasAsync(Guid contaPaiId, CancellationToken ct = default);
    Task<IReadOnlyList<ContaContabil>> ObterHierarquiaCompletaAsync(Guid planoId, CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(Guid planoId, string codigo, Guid? excluirId = null, CancellationToken ct = default);
}
