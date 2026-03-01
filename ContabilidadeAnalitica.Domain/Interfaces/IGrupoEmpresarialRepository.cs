using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface IGrupoEmpresarialRepository : IRepository<GrupoEmpresarial>
{
    Task<GrupoEmpresarial?> ObterComEmpresasAsync(Guid grupoId, CancellationToken ct = default);
}
