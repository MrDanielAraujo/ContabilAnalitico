using ContabilidadeAnalitica.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadeAnalitica.API.Controllers;

/// <summary>
/// Extensões para converter resultados OneOf em respostas HTTP padronizadas.
/// </summary>
public static class ResultadoExtensions
{
    public static IActionResult ToActionResult<T>(this ResultadoOperacao<T> resultado)
    {
        return resultado.Match<IActionResult>(
            sucesso => new OkObjectResult(new ApiResponse<T>(sucesso.Dados)),
            validacao => new BadRequestObjectResult(new ApiErro("Validação", validacao.Erros)),
            naoEncontrado => new NotFoundObjectResult(new ApiErro("Não Encontrado", new List<string> { naoEncontrado.Mensagem })),
            conflito => new ConflictObjectResult(new ApiErro("Conflito", new List<string> { conflito.Mensagem }))
        );
    }

    public static IActionResult ToActionResult(this ResultadoComando resultado)
    {
        return resultado.Match<IActionResult>(
            sucesso => new NoContentResult(),
            validacao => new BadRequestObjectResult(new ApiErro("Validação", validacao.Erros)),
            naoEncontrado => new NotFoundObjectResult(new ApiErro("Não Encontrado", new List<string> { naoEncontrado.Mensagem })),
            conflito => new ConflictObjectResult(new ApiErro("Conflito", new List<string> { conflito.Mensagem }))
        );
    }

    public static IActionResult ToCreatedResult<T>(this ResultadoOperacao<T> resultado, string routeName, Func<T, object> routeValues)
    {
        return resultado.Match<IActionResult>(
            sucesso => new CreatedAtRouteResult(routeName, routeValues(sucesso.Dados), new ApiResponse<T>(sucesso.Dados)),
            validacao => new BadRequestObjectResult(new ApiErro("Validação", validacao.Erros)),
            naoEncontrado => new NotFoundObjectResult(new ApiErro("Não Encontrado", new List<string> { naoEncontrado.Mensagem })),
            conflito => new ConflictObjectResult(new ApiErro("Conflito", new List<string> { conflito.Mensagem }))
        );
    }
}

/// <summary>
/// Envelope de resposta padronizado para sucesso.
/// </summary>
public record ApiResponse<T>(T Dados, bool Sucesso = true);

/// <summary>
/// Envelope de resposta padronizado para erros.
/// </summary>
public record ApiErro(string Tipo, List<string> Mensagens, bool Sucesso = false);
