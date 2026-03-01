using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Interfaces;

/// <summary>
/// Interface genérica de repositório para operações CRUD básicas.
/// </summary>
public interface IRepository<T> where T : EntidadeBase
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ObterTodosAsync(CancellationToken ct = default);
    Task<T> AdicionarAsync(T entidade, CancellationToken ct = default);
    Task AtualizarAsync(T entidade, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
}
