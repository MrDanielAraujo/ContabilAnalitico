namespace ContabilidadeAnalitica.Domain.Common;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Inclui campos de auditoria e identidade.
/// </summary>
public abstract class EntidadeBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Campos de auditoria
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? CriadoPor { get; set; }
    public DateTime? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
    public bool Ativo { get; set; } = true;
}
