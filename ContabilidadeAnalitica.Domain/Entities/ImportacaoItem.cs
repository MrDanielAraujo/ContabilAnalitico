using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Item individual de uma importação de dados contábeis.
/// </summary>
public class ImportacaoItem : EntidadeBase
{
    public Guid ImportacaoId { get; set; }
    public Importacao? Importacao { get; set; }

    /// <summary>
    /// Código da conta contábil no sistema de origem.
    /// </summary>
    public string CodigoConta { get; set; } = string.Empty;

    public int Ano { get; set; }
    public int Mes { get; set; }

    public decimal Valor { get; set; }
    public decimal SaldoDevedor { get; set; }
    public decimal SaldoCredor { get; set; }
    public string Moeda { get; set; } = "BRL";

    public bool Processado { get; set; }
    public bool Erro { get; set; }
    public string? MensagemErro { get; set; }
}
