namespace ContabilidadeAnalitica.Domain.Common;

/// <summary>
/// Interface marcadora para entidades que devem participar do audit trail completo.
/// Preparação para implementação futura de log de alterações em tabela separada.
/// </summary>
public interface IAuditavel
{
    Guid Id { get; }
    DateTime CriadoEm { get; set; }
    string? CriadoPor { get; set; }
    DateTime? AlteradoEm { get; set; }
    string? AlteradoPor { get; set; }
}
