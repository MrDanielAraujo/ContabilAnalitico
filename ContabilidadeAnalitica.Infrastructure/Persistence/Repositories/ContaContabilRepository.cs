using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class ContaContabilRepository : RepositoryBase<ContaContabil>, IContaContabilRepository
{
    public ContaContabilRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<ContaContabil?> ObterPorCodigoAsync(Guid planoId, string codigo, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.PlanoContasId == planoId && c.Codigo == codigo && c.Ativo, ct);
    }

    public async Task<IReadOnlyList<ContaContabil>> ObterPorPlanoAsync(Guid planoId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.PlanoContasId == planoId && c.Ativo)
            .OrderBy(c => c.Codigo)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ContaContabil>> ObterFilhasAsync(Guid contaPaiId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.ContaPaiId == contaPaiId && c.Ativo)
            .OrderBy(c => c.Ordem)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ContaContabil>> ObterHierarquiaCompletaAsync(Guid planoId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.PlanoContasId == planoId && c.Ativo)
            .Include(c => c.ContasFilhas.Where(f => f.Ativo))
            .OrderBy(c => c.Codigo)
            .ToListAsync(ct);
    }

    public async Task<bool> ExisteCodigoAsync(Guid planoId, string codigo, Guid? excluirId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(c => c.PlanoContasId == planoId && c.Codigo == codigo && c.Ativo);
        if (excluirId.HasValue)
            query = query.Where(c => c.Id != excluirId.Value);
        return await query.AnyAsync(ct);
    }
}
