using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Representa uma empresa do sistema contábil.
/// Cada empresa possui seu próprio plano de contas.
/// </summary>
public class Empresa : EntidadeBase, IAuditavel
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? CNPJ { get; set; }
    public string? CodigoExterno { get; set; }
    public string MoedaPadrao { get; set; } = "BRL";

    // Relacionamentos
    public Guid? GrupoEmpresarialId { get; set; }
    public GrupoEmpresarial? GrupoEmpresarial { get; set; }

    public ICollection<PlanoContas> PlanosContas { get; set; } = new List<PlanoContas>();
    public ICollection<SaldoConta> Saldos { get; set; } = new List<SaldoConta>();
    public ICollection<Importacao> Importacoes { get; set; } = new List<Importacao>();
}
