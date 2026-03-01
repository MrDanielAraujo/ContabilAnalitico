using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.CalculationEngine.Aggregation;

/// <summary>
/// Responsável pela agregação hierárquica de contas contábeis.
/// Contas sintéticas somam automaticamente os valores das filhas.
/// </summary>
public static class AgregadorHierarquico
{
    /// <summary>
    /// Calcula os saldos acumulados anuais para todas as contas, incluindo agregação hierárquica.
    /// </summary>
    /// <returns>Dicionário [CodigoConta][Ano] = ValorAcumulado</returns>
    public static Dictionary<string, Dictionary<int, decimal>> CalcularSaldosAcumulados(
        List<ContaContabil> contas,
        List<SaldoConta> saldos,
        List<int> anos)
    {
        var resultado = new Dictionary<string, Dictionary<int, decimal>>();

        // Indexar saldos por conta e ano (acumulado anual = soma dos meses)
        var saldosPorContaAno = saldos
            .GroupBy(s => new { s.ContaContabilId, s.Ano })
            .ToDictionary(
                g => g.Key,
                g => g.Sum(s => s.Valor)
            );

        var contasPorId = contas.ToDictionary(c => c.Id, c => c);
        var contasPorCodigo = contas.ToDictionary(c => c.Codigo, c => c);

        // Inicializar resultado para todas as contas e anos
        foreach (var conta in contas)
        {
            resultado[conta.Codigo] = new Dictionary<int, decimal>();
            foreach (var ano in anos)
            {
                resultado[conta.Codigo][ano] = 0;
            }
        }

        // Preencher saldos das contas analíticas (folhas)
        foreach (var conta in contas.Where(c => c.Tipo == TipoConta.Analitica))
        {
            foreach (var ano in anos)
            {
                var key = new { ContaContabilId = conta.Id, Ano = ano };
                if (saldosPorContaAno.TryGetValue(key, out var valor))
                {
                    resultado[conta.Codigo][ano] = valor;
                }
            }
        }

        // Agregar hierarquicamente (bottom-up): contas sintéticas = soma das filhas diretas
        // Ordenar por nível decrescente para processar folhas primeiro
        var contasOrdenadas = contas
            .Where(c => c.Tipo == TipoConta.Sintetica)
            .OrderByDescending(c => c.Nivel)
            .ToList();

        // Múltiplas passadas para garantir propagação completa
        for (int passada = 0; passada < 10; passada++)
        {
            bool mudou = false;
            foreach (var contaSintetica in contasOrdenadas)
            {
                var filhas = contas.Where(c => c.ContaPaiId == contaSintetica.Id).ToList();
                if (filhas.Count == 0) continue;

                foreach (var ano in anos)
                {
                    var somaFilhas = filhas.Sum(f => resultado.GetValueOrDefault(f.Codigo)?.GetValueOrDefault(ano) ?? 0);
                    var valorAtual = resultado[contaSintetica.Codigo][ano];
                    if (somaFilhas != valorAtual)
                    {
                        resultado[contaSintetica.Codigo][ano] = somaFilhas;
                        mudou = true;
                    }
                }
            }
            if (!mudou) break;
        }

        return resultado;
    }
}
