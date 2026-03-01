using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<Empresa?> ObterPorCNPJAsync(string cnpj, CancellationToken ct = default);
    Task<IReadOnlyList<Empresa>> ObterPorGrupoAsync(Guid grupoId, CancellationToken ct = default);
}
