using ContabilidadeAnalitica.Domain.Common;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Representa uma conta contábil hierárquica e versionada.
/// Suporta contas analíticas (lançamento) e sintéticas (agregação).
/// </summary>
public class ContaContabil : EntidadeBase, IAuditavel
{
    /// <summary>
    /// Código hierárquico da conta. Ex: "1.1.01.001"
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public NaturezaConta Natureza { get; set; }
    public SubtipoConta Subtipo { get; set; }
    public TipoConta Tipo { get; set; }

    /// <summary>
    /// Indica se a conta aceita lançamentos diretos.
    /// Apenas contas analíticas devem aceitar lançamentos.
    /// </summary>
    public bool AceitaLancamento { get; set; }

    /// <summary>
    /// Expressão de cálculo opcional usando referências a outras contas.
    /// Sintaxe: [1.1.01] + [1.1.02] - [2.1.01]
    /// </summary>
    public string? ExpressaoCalculo { get; set; }

    /// <summary>
    /// Nível hierárquico da conta (calculado a partir do código).
    /// </summary>
    public int Nivel { get; set; }

    /// <summary>
    /// Ordem de exibição dentro do mesmo nível.
    /// </summary>
    public int Ordem { get; set; }

    // Hierarquia
    public Guid? ContaPaiId { get; set; }
    public ContaContabil? ContaPai { get; set; }
    public ICollection<ContaContabil> ContasFilhas { get; set; } = new List<ContaContabil>();

    // Relacionamentos
    public Guid PlanoContasId { get; set; }
    public PlanoContas? PlanoContas { get; set; }

    public ICollection<SaldoConta> Saldos { get; set; } = new List<SaldoConta>();

    /// <summary>
    /// Verifica se a conta é sintética (agrega filhas).
    /// </summary>
    public bool IsSintetica => Tipo == TipoConta.Sintetica;

    /// <summary>
    /// Verifica se a conta possui fórmula de cálculo.
    /// </summary>
    public bool PossuiFormula => !string.IsNullOrWhiteSpace(ExpressaoCalculo);
}
