using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.CalculationEngine.Core;

/// <summary>
/// Interface do motor de cálculo contábil determinístico.
/// Responsável por agregação hierárquica, avaliação de fórmulas e preparação de dados tabulares.
/// </summary>
public interface IMotorCalculoContabil
{
    ResultadoCalculo Calcular(ContextoCalculo contexto);
}

/// <summary>
/// Contexto de entrada para o motor de cálculo.
/// </summary>
public class ContextoCalculo
{
    public List<ContaContabil> Contas { get; set; } = new();
    public List<SaldoConta> Saldos { get; set; } = new();
    public List<int> AnosConsultados { get; set; } = new();
    public TipoDemonstrativo Tipo { get; set; }
    public TemplateDemonstrativo? Template { get; set; }
}

/// <summary>
/// Resultado do motor de cálculo.
/// </summary>
public class ResultadoCalculo
{
    public bool TemErro { get; set; }
    public string? MensagemErro { get; set; }
    public List<LinhaCalculo> Linhas { get; set; } = new();
}

/// <summary>
/// Linha calculada para exibição tabular.
/// </summary>
public class LinhaCalculo
{
    public string Rotulo { get; set; } = string.Empty;
    public string? CodigoConta { get; set; }
    public TipoLinha TipoLinha { get; set; }
    public int NivelIndentacao { get; set; }
    public bool Negrito { get; set; }
    public Dictionary<int, decimal> ValoresPorAno { get; set; } = new();
}
