namespace ContabilidadeAnalitica.Domain.ValueObjects;

/// <summary>
/// Value Object representando um período contábil (ano e mês).
/// </summary>
public sealed class Periodo : IEquatable<Periodo>, IComparable<Periodo>
{
    public int Ano { get; }
    public int Mes { get; }

    private Periodo() { Ano = DateTime.UtcNow.Year; Mes = 1; }

    public Periodo(int ano, int mes)
    {
        if (ano < 1900 || ano > 2100)
            throw new ArgumentOutOfRangeException(nameof(ano), "Ano deve estar entre 1900 e 2100.");
        if (mes < 1 || mes > 12)
            throw new ArgumentOutOfRangeException(nameof(mes), "Mês deve estar entre 1 e 12.");

        Ano = ano;
        Mes = mes;
    }

    /// <summary>
    /// Cria um período anual (mês 0 indica acumulado do ano).
    /// </summary>
    public static Periodo Anual(int ano) => new(ano, 12);

    public int ToInt() => Ano * 100 + Mes;

    public bool Equals(Periodo? other) =>
        other is not null && Ano == other.Ano && Mes == other.Mes;

    public override bool Equals(object? obj) => Equals(obj as Periodo);
    public override int GetHashCode() => HashCode.Combine(Ano, Mes);
    public override string ToString() => $"{Ano:D4}/{Mes:D2}";

    public int CompareTo(Periodo? other)
    {
        if (other is null) return 1;
        return ToInt().CompareTo(other.ToInt());
    }

    public static bool operator <(Periodo a, Periodo b) => a.CompareTo(b) < 0;
    public static bool operator >(Periodo a, Periodo b) => a.CompareTo(b) > 0;
    public static bool operator <=(Periodo a, Periodo b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Periodo a, Periodo b) => a.CompareTo(b) >= 0;
}
