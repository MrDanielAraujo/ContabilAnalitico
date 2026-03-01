using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Application.DTOs;

/// <summary>
/// Resultado completo de um demonstrativo financeiro.
/// Estruturado para exibição tabular: Conta | Ano1 | Ano2 | ... | AnoAtual
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
/// Uma linha do demonstrativo com valores por ano.
/// </summary>
public record DemonstrativoLinhaDto(
    string Rotulo,
    string? CodigoConta,
    TipoLinha TipoLinha,
    int NivelIndentacao,
    bool Negrito,
    Dictionary<int, decimal> ValoresPorAno
);

/// <summary>
/// Requisição para geração de demonstrativo.
/// </summary>
public record ConsultaDemonstrativoDto(
    Guid EmpresaId,
    TipoDemonstrativo Tipo,
    List<int> Anos,
    Guid? TemplateId = null
);
