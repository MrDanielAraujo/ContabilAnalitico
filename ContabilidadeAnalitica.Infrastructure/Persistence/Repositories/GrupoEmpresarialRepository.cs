using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class GrupoEmpresarialRepository : RepositoryBase<GrupoEmpresarial>, IGrupoEmpresarialRepository
{
    public GrupoEmpresarialRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<GrupoEmpresarial?> ObterComEmpresasAsync(Guid grupoId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(g => g.Empresas.Where(e => e.Ativo))
            .FirstOrDefaultAsync(g => g.Id == grupoId && g.Ativo, ct);
    }
}
