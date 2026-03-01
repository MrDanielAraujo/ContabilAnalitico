using OneOf;

namespace ContabilidadeAnalitica.Application.Common;

/// <summary>
/// Tipos de resultado para Use Cases usando OneOf.
/// Evita exceções para controle de fluxo.
/// </summary>

// Resultado de sucesso genérico
public record Sucesso;
public record Sucesso<T>(T Dados);

// Resultado de validação inválida
public record ValidacaoInvalida(List<string> Erros)
{
    public ValidacaoInvalida(string erro) : this(new List<string> { erro }) { }
}

// Resultado de recurso não encontrado
public record NaoEncontrado(string Mensagem);

// Resultado de conflito de domínio
public record ConflitoDominio(string Mensagem);

/// <summary>
/// Resultado padrão para operações que retornam dados.
/// Sucesso<T> | ValidacaoInvalida | NaoEncontrado | ConflitoDominio
/// </summary>
[GenerateOneOf]
public partial class ResultadoOperacao<T> : OneOfBase<Sucesso<T>, ValidacaoInvalida, NaoEncontrado, ConflitoDominio>;

/// <summary>
/// Resultado padrão para operações sem retorno de dados.
/// Sucesso | ValidacaoInvalida | NaoEncontrado | ConflitoDominio
/// </summary>
[GenerateOneOf]
public partial class ResultadoComando : OneOfBase<Sucesso, ValidacaoInvalida, NaoEncontrado, ConflitoDominio>;
