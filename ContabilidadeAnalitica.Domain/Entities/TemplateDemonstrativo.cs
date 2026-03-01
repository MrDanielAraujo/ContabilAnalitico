using ContabilidadeAnalitica.Domain.Common;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Template configurável para geração de demonstrativos financeiros.
/// Define a estrutura de exibição do Balanço, Balancete, DRE, etc.
/// </summary>
public class TemplateDemonstrativo : EntidadeBase, IAuditavel
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoDemonstrativo Tipo { get; set; }
    public bool Padrao { get; set; }

    /// <summary>
    /// Empresa associada (null = template global).
    /// </summary>
    public Guid? EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public ICollection<TemplateLinha> Linhas { get; set; } = new List<TemplateLinha>();
}
