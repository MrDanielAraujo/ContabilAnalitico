using System.Text.RegularExpressions;

namespace ContabilidadeAnalitica.Domain.ValueObjects;

/// <summary>
/// Value Object representando o código hierárquico de uma conta contábil.
/// Exemplos: "1", "1.1", "1.1.01", "1.1.01.001"
/// </summary>
public sealed partial class CodigoConta : IEquatable<CodigoConta>, IComparable<CodigoConta>
{
    public string Valor { get; }

    private CodigoConta() { Valor = string.Empty; }

    public CodigoConta(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Código da conta não pode ser vazio.", nameof(valor));

        var normalizado = valor.Trim();
        if (!CodigoContaRegex().IsMatch(normalizado))
            throw new ArgumentException($"Código de conta inválido: '{normalizado}'. Use formato como '1.1.01'.", nameof(valor));

        Valor = normalizado;
    }

    /// <summary>
    /// Retorna o código da conta pai. Ex: "1.1.01" -> "1.1"
    /// Retorna null se for conta raiz.
    /// </summary>
    public string? CodigoPai()
    {
        var ultimoPonto = Valor.LastIndexOf('.');
        return ultimoPonto > 0 ? Valor[..ultimoPonto] : null;
    }

    /// <summary>
    /// Retorna o nível hierárquico da conta. Ex: "1" = 1, "1.1" = 2, "1.1.01" = 3
    /// </summary>
    public int Nivel() => Valor.Split('.').Length;

    public bool Equals(CodigoConta? other) =>
        other is not null && Valor == other.Valor;

    public override bool Equals(object? obj) => Equals(obj as CodigoConta);
    public override int GetHashCode() => Valor.GetHashCode();
    public override string ToString() => Valor;

    public int CompareTo(CodigoConta? other)
    {
        if (other is null) return 1;
        return string.Compare(Valor, other.Valor, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^[0-9]+(\.[0-9]+)*$")]
    private static partial Regex CodigoContaRegex();
}
