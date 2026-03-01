using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class PlanoContasRepository : RepositoryBase<PlanoContas>, IPlanoContasRepository
{
    public PlanoContasRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<PlanoContas?> ObterVigenteAsync(Guid empresaId, DateTime? data = null, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.EmpresaId == empresaId && p.Vigente && p.Ativo)
            .OrderByDescending(p => p.Versao)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PlanoContas>> ObterPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(p => p.EmpresaId == empresaId && p.Ativo)
            .OrderByDescending(p => p.Versao)
            .ToListAsync(ct);
    }

    public async Task<PlanoContas?> ObterComContasAsync(Guid planoId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Contas.Where(c => c.Ativo))
            .FirstOrDefaultAsync(p => p.Id == planoId, ct);
    }
}
