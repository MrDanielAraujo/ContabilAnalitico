using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class EmpresaRepository : RepositoryBase<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<Empresa?> ObterPorCNPJAsync(string cnpj, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.CNPJ == cnpj && e.Ativo, ct);
    }

    public async Task<IReadOnlyList<Empresa>> ObterPorGrupoAsync(Guid grupoId, CancellationToken ct = default)
    {
        return await _dbSet.Where(e => e.GrupoEmpresarialId == grupoId && e.Ativo).ToListAsync(ct);
    }
}
