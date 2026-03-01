namespace ContabilidadeAnalitica.Application.DTOs;

public record EmpresaDto(
    Guid Id,
    string RazaoSocial,
    string? NomeFantasia,
    string? CNPJ,
    string? CodigoExterno,
    string MoedaPadrao,
    Guid? GrupoEmpresarialId,
    bool Ativo
);

public record CriarEmpresaDto(
    string RazaoSocial,
    string? NomeFantasia,
    string? CNPJ,
    string? CodigoExterno,
    string MoedaPadrao = "BRL",
    Guid? GrupoEmpresarialId = null
);

public record AtualizarEmpresaDto(
    string RazaoSocial,
    string? NomeFantasia,
    string? CNPJ,
    string? CodigoExterno,
    string MoedaPadrao = "BRL",
    Guid? GrupoEmpresarialId = null
);

public record GrupoEmpresarialDto(
    Guid Id,
    string Nome,
    string? Descricao,
    string? CodigoExterno,
    bool Ativo,
    List<EmpresaDto> Empresas
);

public record CriarGrupoEmpresarialDto(
    string Nome,
    string? Descricao,
    string? CodigoExterno
);
