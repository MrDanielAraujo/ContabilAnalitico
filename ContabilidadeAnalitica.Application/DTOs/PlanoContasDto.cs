using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Application.DTOs;

public record PlanoContasDto(
    Guid Id,
    string Nome,
    string? Descricao,
    int Versao,
    DateTime VigenciaInicio,
    DateTime? VigenciaFim,
    bool Vigente,
    Guid EmpresaId,
    List<ContaContabilDto> Contas
);

public record PlanoContasResumoDto(
    Guid Id,
    string Nome,
    int Versao,
    DateTime VigenciaInicio,
    DateTime? VigenciaFim,
    bool Vigente,
    Guid EmpresaId,
    int TotalContas
);

public record CriarPlanoContasDto(
    string Nome,
    string? Descricao,
    Guid EmpresaId,
    DateTime VigenciaInicio
);

public record ContaContabilDto(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    NaturezaConta Natureza,
    SubtipoConta Subtipo,
    TipoConta Tipo,
    bool AceitaLancamento,
    string? ExpressaoCalculo,
    int Nivel,
    int Ordem,
    Guid? ContaPaiId,
    Guid PlanoContasId,
    List<ContaContabilDto> ContasFilhas
);

public record CriarContaContabilDto(
    string Codigo,
    string Nome,
    string? Descricao,
    NaturezaConta Natureza,
    SubtipoConta Subtipo,
    TipoConta Tipo,
    bool AceitaLancamento,
    string? ExpressaoCalculo,
    int Ordem,
    Guid? ContaPaiId,
    Guid PlanoContasId
);

public record AtualizarContaContabilDto(
    string Nome,
    string? Descricao,
    NaturezaConta Natureza,
    SubtipoConta Subtipo,
    TipoConta Tipo,
    bool AceitaLancamento,
    string? ExpressaoCalculo,
    int Ordem
);
