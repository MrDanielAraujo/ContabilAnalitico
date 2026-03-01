using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Domain.Interfaces;

public interface ITemplateDemonstrativoRepository : IRepository<TemplateDemonstrativo>
{
    Task<TemplateDemonstrativo?> ObterComLinhasAsync(Guid templateId, CancellationToken ct = default);
    Task<TemplateDemonstrativo?> ObterPadraoAsync(TipoDemonstrativo tipo, Guid? empresaId = null, CancellationToken ct = default);
    Task<IReadOnlyList<TemplateDemonstrativo>> ObterPorTipoAsync(TipoDemonstrativo tipo, CancellationToken ct = default);
}
