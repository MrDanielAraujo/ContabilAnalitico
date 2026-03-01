using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Representa o saldo de uma conta contábil em um período específico.
/// Armazena valores por conta, empresa e período.
/// </summary>
public class SaldoConta : EntidadeBase, IAuditavel
{
    public Guid ContaContabilId { get; set; }
    public ContaContabil? ContaContabil { get; set; }

    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    /// <summary>
    /// Ano do período contábil.
    /// </summary>
    public int Ano { get; set; }

    /// <summary>
    /// Mês do período contábil (1-12).
    /// </summary>
    public int Mes { get; set; }

    /// <summary>
    /// Valor do saldo no período.
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Moeda do saldo (preparação multi-moeda).
    /// </summary>
    public string Moeda { get; set; } = "BRL";

    /// <summary>
    /// Saldo devedor no período.
    /// </summary>
    public decimal SaldoDevedor { get; set; }

    /// <summary>
    /// Saldo credor no período.
    /// </summary>
    public decimal SaldoCredor { get; set; }

    /// <summary>
    /// Origem do saldo (manual, importação, cálculo).
    /// </summary>
    public string? Origem { get; set; }

    /// <summary>
    /// Referência à importação que gerou este saldo, se aplicável.
    /// </summary>
    public Guid? ImportacaoId { get; set; }
}
