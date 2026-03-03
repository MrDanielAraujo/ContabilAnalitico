using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Application.DTOs;

/// <summary>
/// Resultado completo de um demonstrativo financeiro.
/// Formato alinhado com o contrato esperado pelo frontend.
/// </summary>
public record DemonstrativoResultadoDto(
    TipoDemonstrativo Tipo,
    string NomeTemplate,
    Guid EmpresaId,
    string EmpresaNome,
    List<int> AnosConsultados,
    List<DemonstrativoLinhaDto> Linhas,
    DateTime GeradoEm
);

/// <summary>
/// Uma linha do demonstrativo no formato esperado pelo frontend.
/// Cada linha possui Nome, Tipo, SubTipo, AH e um dicionário de lançamentos por ano.
/// </summary>
public class DemonstrativoLinhaDto
{
    /// <summary>Nome da conta contábil.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Tipo/natureza da conta (ex: "Ativo", "Passivo", "Receita", "Despesa").</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Subtipo da conta (ex: "Circulante", "Não Circulante", "Patrimônio Líquido").</summary>
    public string SubTipo { get; set; } = string.Empty;

    /// <summary>Código da conta contábil.</summary>
    public string? Codigo { get; set; }

    /// <summary>Nível hierárquico da conta (para indentação no frontend).</summary>
    public int Nivel { get; set; }

    /// <summary>Indica se a conta é sintética (agrupadora).</summary>
    public bool Sintetica { get; set; }

    /// <summary>
    /// Análise Horizontal (AH): variação percentual entre o último e o primeiro ano consultado.
    /// Fórmula: ((ValorÚltimoAno - ValorPrimeiroAno) / |ValorPrimeiroAno|) * 100
    /// </summary>
    public decimal AH { get; set; }

    /// <summary>
    /// Lançamentos por ano. Chave = ano (string), Valor = objeto com Valor e AV.
    /// </summary>
    public Dictionary<string, LancamentoAnoDto> Lancamentos { get; set; } = new();
}

/// <summary>
/// Valor de uma conta em um ano específico, com Análise Vertical (AV).
/// </summary>
public class LancamentoAnoDto
{
    /// <summary>Valor (saldo) da conta no ano.</summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Análise Vertical (AV): percentual da conta em relação ao total do grupo.
    /// Para contas de Ativo: (Valor / Total Ativo) * 100
    /// Para contas de Passivo/PL: (Valor / Total Passivo+PL) * 100
    /// Para contas de Receita/Despesa: (Valor / Receita Líquida) * 100
    /// </summary>
    public decimal AV { get; set; }
}

/// <summary>
/// Requisição para geração de demonstrativo.
/// </summary>
public record ConsultaDemonstrativoDto(
    Guid EmpresaId,
    TipoDemonstrativo Tipo,
    List<int> Anos,
    Guid? TemplateId = null
);
