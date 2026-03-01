using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface IPlanoContasRepository : IRepository<PlanoContas>
{
    Task<PlanoContas?> ObterVigenteAsync(Guid empresaId, DateTime? data = null, CancellationToken ct = default);
    Task<IReadOnlyList<PlanoContas>> ObterPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
    Task<PlanoContas?> ObterComContasAsync(Guid planoId, CancellationToken ct = default);
}
