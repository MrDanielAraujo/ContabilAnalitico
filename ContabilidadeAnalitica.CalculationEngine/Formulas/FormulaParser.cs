using System.Text.RegularExpressions;

namespace ContabilidadeAnalitica.CalculationEngine.Formulas;

/// <summary>
/// Parser de expressões contábeis.
/// Suporta referências a contas por código entre colchetes, operadores aritméticos e parênteses.
/// Exemplo: [1.1.01] + [1.1.02] - [2.1.01] * ([3.1] / [4.1])
/// </summary>
public static partial class FormulaParser
{
    /// <summary>
    /// Extrai todos os códigos de conta referenciados em uma expressão.
    /// </summary>
    public static List<string> ExtrairReferencias(string expressao)
    {
        var matches = ReferenciaRegex().Matches(expressao);
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    /// <summary>
    /// Avalia uma expressão substituindo referências por valores.
    /// </summary>
    public static decimal Avaliar(string expressao, Dictionary<string, decimal> valores)
    {
        var expressaoResolvida = ReferenciaRegex().Replace(expressao, match =>
        {
            var codigo = match.Groups[1].Value;
            return valores.TryGetValue(codigo, out var valor)
                ? valor.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "0";
        });

        return AvaliarExpressaoMatematica(expressaoResolvida);
    }

    /// <summary>
    /// Valida a sintaxe de uma expressão.
    /// </summary>
    public static (bool Valida, string? Erro) Validar(string expressao)
    {
        if (string.IsNullOrWhiteSpace(expressao))
            return (false, "Expressão vazia.");

        // Verificar caracteres válidos
        var semReferencias = ReferenciaRegex().Replace(expressao, "0");
        if (!ExpressaoValidaRegex().IsMatch(semReferencias.Replace(" ", "")))
            return (false, $"Expressão contém caracteres inválidos: '{expressao}'");

        // Verificar parênteses balanceados
        int nivel = 0;
        foreach (var c in semReferencias)
        {
            if (c == '(') nivel++;
            if (c == ')') nivel--;
            if (nivel < 0) return (false, "Parênteses desbalanceados.");
        }
        if (nivel != 0) return (false, "Parênteses desbalanceados.");

        return (true, null);
    }

    /// <summary>
    /// Avaliador de expressões matemáticas simples (recursive descent parser).
    /// Suporta +, -, *, /, parênteses e números decimais.
    /// </summary>
    private static decimal AvaliarExpressaoMatematica(string expressao)
    {
        var tokens = Tokenizar(expressao);
        var pos = 0;
        var resultado = ParseExpression(tokens, ref pos);
        return resultado;
    }

    private static List<string> Tokenizar(string expressao)
    {
        var tokens = new List<string>();
        var i = 0;
        var expr = expressao.Replace(" ", "");

        while (i < expr.Length)
        {
            if (char.IsDigit(expr[i]) || (expr[i] == '-' && (tokens.Count == 0 || tokens.Last() == "(" || IsOperator(tokens.Last()))))
            {
                var start = i;
                if (expr[i] == '-') i++;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                    i++;
                tokens.Add(expr[start..i]);
            }
            else if ("+-*/()".Contains(expr[i]))
            {
                tokens.Add(expr[i].ToString());
                i++;
            }
            else
            {
                i++;
            }
        }

        return tokens;
    }

    private static bool IsOperator(string token) => token is "+" or "-" or "*" or "/";

    private static decimal ParseExpression(List<string> tokens, ref int pos)
    {
        var left = ParseTerm(tokens, ref pos);

        while (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-"))
        {
            var op = tokens[pos++];
            var right = ParseTerm(tokens, ref pos);
            left = op == "+" ? left + right : left - right;
        }

        return left;
    }

    private static decimal ParseTerm(List<string> tokens, ref int pos)
    {
        var left = ParseFactor(tokens, ref pos);

        while (pos < tokens.Count && (tokens[pos] == "*" || tokens[pos] == "/"))
        {
            var op = tokens[pos++];
            var right = ParseFactor(tokens, ref pos);
            if (op == "/")
            {
                if (right == 0) return 0; // Divisão por zero retorna 0
                left /= right;
            }
            else
            {
                left *= right;
            }
        }

        return left;
    }

    private static decimal ParseFactor(List<string> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
            return 0;

        if (tokens[pos] == "(")
        {
            pos++; // skip '('
            var result = ParseExpression(tokens, ref pos);
            if (pos < tokens.Count && tokens[pos] == ")")
                pos++; // skip ')'
            return result;
        }

        if (decimal.TryParse(tokens[pos], System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            pos++;
            return value;
        }

        pos++;
        return 0;
    }

    [GeneratedRegex(@"\[([^\]]+)\]")]
    private static partial Regex ReferenciaRegex();

    [GeneratedRegex(@"^[\d\.\+\-\*\/\(\)\s]+$")]
    private static partial Regex ExpressaoValidaRegex();
}
