using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class TemplateDemonstrativoRepository : RepositoryBase<TemplateDemonstrativo>, ITemplateDemonstrativoRepository
{
    public TemplateDemonstrativoRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<TemplateDemonstrativo?> ObterComLinhasAsync(Guid templateId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(t => t.Linhas.OrderBy(l => l.Ordem))
            .FirstOrDefaultAsync(t => t.Id == templateId && t.Ativo, ct);
    }

    public async Task<TemplateDemonstrativo?> ObterPadraoAsync(TipoDemonstrativo tipo, Guid? empresaId = null, CancellationToken ct = default)
    {
        // Primeiro tenta template padrão da empresa, depois global
        if (empresaId.HasValue)
        {
            var templateEmpresa = await _dbSet
                .Include(t => t.Linhas.OrderBy(l => l.Ordem))
                .FirstOrDefaultAsync(t => t.Tipo == tipo && t.Padrao && t.EmpresaId == empresaId && t.Ativo, ct);
            if (templateEmpresa is not null)
                return templateEmpresa;
        }

        return await _dbSet
            .Include(t => t.Linhas.OrderBy(l => l.Ordem))
            .FirstOrDefaultAsync(t => t.Tipo == tipo && t.Padrao && t.EmpresaId == null && t.Ativo, ct);
    }

    public async Task<IReadOnlyList<TemplateDemonstrativo>> ObterPorTipoAsync(TipoDemonstrativo tipo, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(t => t.Linhas.OrderBy(l => l.Ordem))
            .Where(t => t.Tipo == tipo && t.Ativo)
            .ToListAsync(ct);
    }
}
