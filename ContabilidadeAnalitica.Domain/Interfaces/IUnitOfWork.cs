namespace ContabilidadeAnalitica.Domain.Interfaces;

/// <summary>
/// Unit of Work para garantir transações atômicas.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGrupoEmpresarialRepository GruposEmpresariais { get; }
    IEmpresaRepository Empresas { get; }
    IPlanoContasRepository PlanosContas { get; }
    IContaContabilRepository ContasContabeis { get; }
    ISaldoContaRepository SaldosContas { get; }
    ITemplateDemonstrativoRepository TemplatesDemonstrativos { get; }
    IImportacaoRepository Importacoes { get; }

    Task<int> SalvarAsync(CancellationToken ct = default);
}
