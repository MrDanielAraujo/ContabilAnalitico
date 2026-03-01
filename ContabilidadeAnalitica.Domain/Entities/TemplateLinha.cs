using ContabilidadeAnalitica.Domain.Common;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Linha de um template de demonstrativo.
/// Cada linha pode referenciar uma conta, grupo, fórmula ou ser um totalizador.
/// </summary>
public class TemplateLinha : EntidadeBase
{
    public Guid TemplateDemonstrativoId { get; set; }
    public TemplateDemonstrativo? TemplateDemonstrativo { get; set; }

    /// <summary>
    /// Rótulo de exibição da linha.
    /// </summary>
    public string Rotulo { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da linha (conta específica, grupo, totalizador, fórmula, separador).
    /// </summary>
    public TipoLinha TipoLinha { get; set; }

    /// <summary>
    /// Código da conta referenciada (quando TipoLinha = ContaEspecifica).
    /// </summary>
    public string? CodigoConta { get; set; }

    /// <summary>
    /// Natureza do grupo (quando TipoLinha = GrupoNatureza).
    /// </summary>
    public NaturezaConta? NaturezaFiltro { get; set; }

    /// <summary>
    /// Subtipo do grupo (quando TipoLinha = GrupoSubtipo).
    /// </summary>
    public SubtipoConta? SubtipoFiltro { get; set; }

    /// <summary>
    /// Expressão de cálculo (quando TipoLinha = Formula).
    /// Sintaxe: [1.1.01] + [1.1.02]
    /// </summary>
    public string? Expressao { get; set; }

    /// <summary>
    /// Linhas referenciadas para totalização (IDs separados por vírgula).
    /// </summary>
    public string? LinhasReferenciadas { get; set; }

    /// <summary>
    /// Ordem de exibição da linha no template.
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Nível de indentação para exibição.
    /// </summary>
    public int NivelIndentacao { get; set; }

    /// <summary>
    /// Indica se a linha deve ser exibida em negrito.
    /// </summary>
    public bool Negrito { get; set; }

    /// <summary>
    /// Indica se os valores devem ser invertidos (multiplicados por -1).
    /// Útil para passivos e receitas em certos demonstrativos.
    /// </summary>
    public bool InverterSinal { get; set; }
}
