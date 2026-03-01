using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Taxa de câmbio entre moedas para suporte multi-moeda futuro.
/// </summary>
public class TaxaCambio : EntidadeBase
{
    public string MoedaOrigem { get; set; } = string.Empty;
    public string MoedaDestino { get; set; } = string.Empty;
    public decimal Taxa { get; set; }
    public DateTime DataReferencia { get; set; }
    public string? Fonte { get; set; }
}
