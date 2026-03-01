namespace ContabilidadeAnalitica.Domain.ValueObjects;

/// <summary>
/// Value Object representando um valor monetário com moeda.
/// Preparado para suporte multi-moeda futuro.
/// </summary>
public sealed class Dinheiro : IEquatable<Dinheiro>
{
    public decimal Valor { get; }
    public string Moeda { get; }

    private Dinheiro() { Valor = 0; Moeda = "BRL"; }

    public Dinheiro(decimal valor, string moeda = "BRL")
    {
        Valor = valor;
        Moeda = moeda?.ToUpperInvariant() ?? "BRL";
    }

    public static Dinheiro Zero(string moeda = "BRL") => new(0, moeda);

    public Dinheiro Somar(Dinheiro outro)
    {
        if (Moeda != outro.Moeda)
            throw new InvalidOperationException($"Não é possível somar moedas diferentes: {Moeda} e {outro.Moeda}");
        return new Dinheiro(Valor + outro.Valor, Moeda);
    }

    public Dinheiro Subtrair(Dinheiro outro)
    {
        if (Moeda != outro.Moeda)
            throw new InvalidOperationException($"Não é possível subtrair moedas diferentes: {Moeda} e {outro.Moeda}");
        return new Dinheiro(Valor - outro.Valor, Moeda);
    }

    public Dinheiro Multiplicar(decimal fator) => new(Valor * fator, Moeda);

    public Dinheiro Dividir(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Divisão por zero não permitida.");
        return new Dinheiro(Valor / divisor, Moeda);
    }

    public Dinheiro Negar() => new(-Valor, Moeda);

    public bool Equals(Dinheiro? other) =>
        other is not null && Valor == other.Valor && Moeda == other.Moeda;

    public override bool Equals(object? obj) => Equals(obj as Dinheiro);
    public override int GetHashCode() => HashCode.Combine(Valor, Moeda);
    public override string ToString() => $"{Valor:N2} {Moeda}";

    public static Dinheiro operator +(Dinheiro a, Dinheiro b) => a.Somar(b);
    public static Dinheiro operator -(Dinheiro a, Dinheiro b) => a.Subtrair(b);
    public static Dinheiro operator *(Dinheiro a, decimal b) => a.Multiplicar(b);
    public static Dinheiro operator /(Dinheiro a, decimal b) => a.Dividir(b);
    public static Dinheiro operator -(Dinheiro a) => a.Negar();
}
