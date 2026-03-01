using ContabilidadeAnalitica.Domain.Entities;

namespace ContabilidadeAnalitica.CalculationEngine.Formulas;

/// <summary>
/// Detecta dependências circulares entre fórmulas de contas contábeis.
/// Utiliza DFS (Depth-First Search) para encontrar ciclos no grafo de dependências.
/// </summary>
public static class DetectorDependenciaCircular
{
    /// <summary>
    /// Verifica se existem dependências circulares entre as fórmulas das contas.
    /// Retorna a lista de ciclos encontrados.
    /// </summary>
    public static (bool TemCiclo, List<string> Ciclos) Detectar(IEnumerable<ContaContabil> contas)
    {
        var contasComFormula = contas
            .Where(c => !string.IsNullOrWhiteSpace(c.ExpressaoCalculo))
            .ToList();

        if (contasComFormula.Count == 0)
            return (false, new List<string>());

        // Construir grafo de dependências
        var grafo = new Dictionary<string, List<string>>();
        foreach (var conta in contasComFormula)
        {
            var referencias = FormulaParser.ExtrairReferencias(conta.ExpressaoCalculo!);
            grafo[conta.Codigo] = referencias;
        }

        // DFS para detecção de ciclos
        var visitados = new HashSet<string>();
        var emPilha = new HashSet<string>();
        var ciclos = new List<string>();
        var caminho = new List<string>();

        foreach (var codigo in grafo.Keys)
        {
            if (!visitados.Contains(codigo))
            {
                DFS(codigo, grafo, visitados, emPilha, caminho, ciclos);
            }
        }

        return (ciclos.Count > 0, ciclos);
    }

    private static void DFS(
        string codigo,
        Dictionary<string, List<string>> grafo,
        HashSet<string> visitados,
        HashSet<string> emPilha,
        List<string> caminho,
        List<string> ciclos)
    {
        visitados.Add(codigo);
        emPilha.Add(codigo);
        caminho.Add(codigo);

        if (grafo.TryGetValue(codigo, out var dependencias))
        {
            foreach (var dep in dependencias)
            {
                if (!visitados.Contains(dep))
                {
                    if (grafo.ContainsKey(dep))
                        DFS(dep, grafo, visitados, emPilha, caminho, ciclos);
                }
                else if (emPilha.Contains(dep))
                {
                    // Ciclo encontrado
                    var inicio = caminho.IndexOf(dep);
                    var ciclo = string.Join(" -> ", caminho.Skip(inicio)) + " -> " + dep;
                    ciclos.Add($"Dependência circular detectada: {ciclo}");
                }
            }
        }

        caminho.RemoveAt(caminho.Count - 1);
        emPilha.Remove(codigo);
    }
}
