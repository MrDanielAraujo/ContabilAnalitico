using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Application.DTOs;

public record ImportacaoDto(
    Guid Id,
    Guid EmpresaId,
    string? Descricao,
    string? SistemaOrigem,
    StatusImportacao Status,
    int TotalItens,
    int ItensProcessados,
    int ItensComErro,
    DateTime CriadoEm,
    DateTime? ProcessadoEm,
    string? MensagemErro
);

public record CriarImportacaoDto(
    Guid EmpresaId,
    string? Descricao,
    string? SistemaOrigem,
    List<CriarImportacaoItemDto> Itens
);

public record CriarImportacaoItemDto(
    string CodigoConta,
    int Ano,
    int Mes,
    decimal Valor,
    decimal SaldoDevedor = 0,
    decimal SaldoCredor = 0,
    string Moeda = "BRL"
);

public record ImportacaoItemDto(
    Guid Id,
    string CodigoConta,
    int Ano,
    int Mes,
    decimal Valor,
    decimal SaldoDevedor,
    decimal SaldoCredor,
    string Moeda,
    bool Processado,
    bool Erro,
    string? MensagemErro
);
