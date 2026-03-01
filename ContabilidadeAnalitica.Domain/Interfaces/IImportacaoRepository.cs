using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface IImportacaoRepository : IRepository<Importacao>
{
    Task<Importacao?> ObterComItensAsync(Guid importacaoId, CancellationToken ct = default);
    Task<IReadOnlyList<Importacao>> ObterPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}
