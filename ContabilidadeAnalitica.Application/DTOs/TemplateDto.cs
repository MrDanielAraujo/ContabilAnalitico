using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.Application.DTOs;

public record TemplateDemonstrativoDto(
    Guid Id,
    string Nome,
    string? Descricao,
    TipoDemonstrativo Tipo,
    bool Padrao,
    Guid? EmpresaId,
    List<TemplateLinhaDto> Linhas
);

public record CriarTemplateDemonstrativoDto(
    string Nome,
    string? Descricao,
    TipoDemonstrativo Tipo,
    bool Padrao,
    Guid? EmpresaId,
    List<CriarTemplateLinhaDto> Linhas
);

public record TemplateLinhaDto(
    Guid Id,
    string Rotulo,
    TipoLinha TipoLinha,
    string? CodigoConta,
    NaturezaConta? NaturezaFiltro,
    SubtipoConta? SubtipoFiltro,
    string? Expressao,
    string? LinhasReferenciadas,
    int Ordem,
    int NivelIndentacao,
    bool Negrito,
    bool InverterSinal
);

public record CriarTemplateLinhaDto(
    string Rotulo,
    TipoLinha TipoLinha,
    string? CodigoConta,
    NaturezaConta? NaturezaFiltro,
    SubtipoConta? SubtipoFiltro,
    string? Expressao,
    string? LinhasReferenciadas,
    int Ordem,
    int NivelIndentacao = 0,
    bool Negrito = false,
    bool InverterSinal = false
);
