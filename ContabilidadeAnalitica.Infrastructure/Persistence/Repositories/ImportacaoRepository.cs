using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class ImportacaoRepository : RepositoryBase<Importacao>, IImportacaoRepository
{
    public ImportacaoRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<Importacao?> ObterComItensAsync(Guid importacaoId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(i => i.Itens)
            .FirstOrDefaultAsync(i => i.Id == importacaoId, ct);
    }

    public async Task<IReadOnlyList<Importacao>> ObterPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(i => i.EmpresaId == empresaId && i.Ativo)
            .OrderByDescending(i => i.CriadoEm)
            .ToListAsync(ct);
    }
}
