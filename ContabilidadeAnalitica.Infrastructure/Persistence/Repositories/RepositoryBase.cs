using ContabilidadeAnalitica.Domain.Common;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : EntidadeBase
{
    protected readonly ContabilidadeDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(ContabilidadeDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public virtual async Task<IReadOnlyList<T>> ObterTodosAsync(CancellationToken ct = default)
    {
        return await _dbSet.Where(e => e.Ativo).ToListAsync(ct);
    }

    public virtual async Task<T> AdicionarAsync(T entidade, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entidade, ct);
        return entidade;
    }

    public virtual Task AtualizarAsync(T entidade, CancellationToken ct = default)
    {
        _context.Entry(entidade).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var entidade = await ObterPorIdAsync(id, ct);
        if (entidade is not null)
        {
            entidade.Ativo = false;
            _context.Entry(entidade).State = EntityState.Modified;
        }
    }
}
