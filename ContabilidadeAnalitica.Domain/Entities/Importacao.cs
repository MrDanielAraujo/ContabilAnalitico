using ContabilidadeAnalitica.Domain.Common;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Representa uma importação de dados contábeis de sistema externo.
/// </summary>
public class Importacao : EntidadeBase, IAuditavel
{
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public string? Descricao { get; set; }
    public string? SistemaOrigem { get; set; }
    public StatusImportacao Status { get; set; } = StatusImportacao.Pendente;

    public int TotalItens { get; set; }
    public int ItensProcessados { get; set; }
    public int ItensComErro { get; set; }

    public DateTime? ProcessadoEm { get; set; }
    public string? MensagemErro { get; set; }

    public ICollection<ImportacaoItem> Itens { get; set; } = new List<ImportacaoItem>();
}
