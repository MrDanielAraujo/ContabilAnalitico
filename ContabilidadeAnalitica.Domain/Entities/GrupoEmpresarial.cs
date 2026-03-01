using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Representa um grupo empresarial para consolidação futura.
/// </summary>
public class GrupoEmpresarial : EntidadeBase, IAuditavel
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? CodigoExterno { get; set; }

    // Navegação
    public ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
}
