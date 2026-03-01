using ContabilidadeAnalitica.Domain.Interfaces;
using ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;

namespace ContabilidadeAnalitica.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ContabilidadeDbContext _context;

    private IGrupoEmpresarialRepository? _gruposEmpresariais;
    private IEmpresaRepository? _empresas;
    private IPlanoContasRepository? _planosContas;
    private IContaContabilRepository? _contasContabeis;
    private ISaldoContaRepository? _saldosContas;
    private ITemplateDemonstrativoRepository? _templatesDemonstrativos;
    private IImportacaoRepository? _importacoes;

    public UnitOfWork(ContabilidadeDbContext context)
    {
        _context = context;
    }

    public IGrupoEmpresarialRepository GruposEmpresariais =>
        _gruposEmpresariais ??= new GrupoEmpresarialRepository(_context);

    public IEmpresaRepository Empresas =>
        _empresas ??= new EmpresaRepository(_context);

    public IPlanoContasRepository PlanosContas =>
        _planosContas ??= new PlanoContasRepository(_context);

    public IContaContabilRepository ContasContabeis =>
        _contasContabeis ??= new ContaContabilRepository(_context);

    public ISaldoContaRepository SaldosContas =>
        _saldosContas ??= new SaldoContaRepository(_context);

    public ITemplateDemonstrativoRepository TemplatesDemonstrativos =>
        _templatesDemonstrativos ??= new TemplateDemonstrativoRepository(_context);

    public IImportacaoRepository Importacoes =>
        _importacoes ??= new ImportacaoRepository(_context);

    public async Task<int> SalvarAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
