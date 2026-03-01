using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

public class SaldoContaRepository : RepositoryBase<SaldoConta>, ISaldoContaRepository
{
    public SaldoContaRepository(ContabilidadeDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SaldoConta>> ObterPorEmpresaEAnosAsync(
        Guid empresaId, IEnumerable<int> anos, CancellationToken ct = default)
    {
        var anosLista = anos.ToList();
        return await _dbSet
            .Where(s => s.EmpresaId == empresaId && anosLista.Contains(s.Ano) && s.Ativo)
            .Include(s => s.ContaContabil)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SaldoConta>> ObterPorContaEPeriodoAsync(
        Guid contaId, int ano, int? mes = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(s => s.ContaContabilId == contaId && s.Ano == ano && s.Ativo);
        if (mes.HasValue)
            query = query.Where(s => s.Mes == mes.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<SaldoConta?> ObterSaldoEspecificoAsync(
        Guid contaId, Guid empresaId, int ano, int mes, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            s => s.ContaContabilId == contaId && s.EmpresaId == empresaId
                && s.Ano == ano && s.Mes == mes && s.Ativo, ct);
    }

    public async Task AdicionarOuAtualizarAsync(SaldoConta saldo, CancellationToken ct = default)
    {
        var existente = await ObterSaldoEspecificoAsync(
            saldo.ContaContabilId, saldo.EmpresaId, saldo.Ano, saldo.Mes, ct);

        if (existente is not null)
        {
            existente.Valor = saldo.Valor;
            existente.SaldoDevedor = saldo.SaldoDevedor;
            existente.SaldoCredor = saldo.SaldoCredor;
            existente.Moeda = saldo.Moeda;
            existente.AlteradoEm = DateTime.UtcNow;
            _context.Entry(existente).State = EntityState.Modified;
        }
        else
        {
            await _dbSet.AddAsync(saldo, ct);
        }
    }
}
