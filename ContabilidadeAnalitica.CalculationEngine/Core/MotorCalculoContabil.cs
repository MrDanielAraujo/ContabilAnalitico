using ContabilidadeAnalitica.CalculationEngine.Aggregation;
using ContabilidadeAnalitica.CalculationEngine.Formulas;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;

namespace ContabilidadeAnalitica.CalculationEngine.Core;

/// <summary>
/// Motor de cálculo contábil determinístico.
/// Responsável por:
/// - Agregação hierárquica de contas
/// - Avaliação de fórmulas entre contas
/// - Cálculo de totais por subtipo e tipo
/// - Preparação dos dados para exibição tabular
/// - Detecção de dependências circulares
/// </summary>
public class MotorCalculoContabil : IMotorCalculoContabil
{
    public ResultadoCalculo Calcular(ContextoCalculo contexto)
    {
        var resultado = new ResultadoCalculo();

        try
        {
            // 1. Detectar dependências circulares
            var (temCiclo, ciclos) = DetectorDependenciaCircular.Detectar(contexto.Contas);
            if (temCiclo)
            {
                resultado.TemErro = true;
                resultado.MensagemErro = string.Join("; ", ciclos);
                return resultado;
            }

            // 2. Calcular saldos acumulados com agregação hierárquica
            var saldosCalculados = AgregadorHierarquico.CalcularSaldosAcumulados(
                contexto.Contas, contexto.Saldos, contexto.AnosConsultados);

            // 3. Avaliar fórmulas
            AvaliarFormulas(contexto.Contas, saldosCalculados, contexto.AnosConsultados);

            // 4. Gerar linhas do demonstrativo
            if (contexto.Template != null && contexto.Template.Linhas?.Count > 0)
            {
                resultado.Linhas = GerarLinhasComTemplate(
                    contexto.Template, contexto.Contas, saldosCalculados, contexto.AnosConsultados);
            }
            else
            {
                resultado.Linhas = GerarLinhasPadrao(
                    contexto.Contas, saldosCalculados, contexto.AnosConsultados, contexto.Tipo);
            }
        }
        catch (Exception ex)
        {
            resultado.TemErro = true;
            resultado.MensagemErro = $"Erro no motor de cálculo: {ex.Message}";
        }

        return resultado;
    }

    /// <summary>
    /// Avalia fórmulas de contas que possuem expressão de cálculo.
    /// </summary>
    private void AvaliarFormulas(
        List<ContaContabil> contas,
        Dictionary<string, Dictionary<int, decimal>> saldos,
        List<int> anos)
    {
        var contasComFormula = contas
            .Where(c => !string.IsNullOrWhiteSpace(c.ExpressaoCalculo))
            .ToList();

        // Ordenar por dependências (topological sort simplificado)
        var processadas = new HashSet<string>();
        var fila = new Queue<ContaContabil>(contasComFormula);
        int maxIteracoes = contasComFormula.Count * 2;
        int iteracao = 0;

        while (fila.Count > 0 && iteracao < maxIteracoes)
        {
            iteracao++;
            var conta = fila.Dequeue();
            var refs = FormulaParser.ExtrairReferencias(conta.ExpressaoCalculo!);

            // Verificar se todas as dependências que possuem fórmula já foram processadas
            var dependenciasFormula = refs
                .Where(r => contasComFormula.Any(c => c.Codigo == r))
                .ToList();

            if (dependenciasFormula.All(d => processadas.Contains(d)))
            {
                foreach (var ano in anos)
                {
                    var valoresRef = new Dictionary<string, decimal>();
                    foreach (var refCodigo in refs)
                    {
                        valoresRef[refCodigo] = saldos.GetValueOrDefault(refCodigo)?.GetValueOrDefault(ano) ?? 0;
                    }

                    var valorCalculado = FormulaParser.Avaliar(conta.ExpressaoCalculo!, valoresRef);

                    if (!saldos.ContainsKey(conta.Codigo))
                        saldos[conta.Codigo] = new Dictionary<int, decimal>();

                    saldos[conta.Codigo][ano] = valorCalculado;
                }

                processadas.Add(conta.Codigo);
            }
            else
            {
                fila.Enqueue(conta); // Reprocessar depois
            }
        }
    }

    /// <summary>
    /// Gera linhas do demonstrativo usando um template configurado.
    /// </summary>
    private List<LinhaCalculo> GerarLinhasComTemplate(
        TemplateDemonstrativo template,
        List<ContaContabil> contas,
        Dictionary<string, Dictionary<int, decimal>> saldos,
        List<int> anos)
    {
        var linhas = new List<LinhaCalculo>();
        var contasPorCodigo = contas.ToDictionary(c => c.Codigo, c => c);
        var linhasCalculadas = new Dictionary<Guid, Dictionary<int, decimal>>();

        foreach (var tl in template.Linhas!.OrderBy(l => l.Ordem))
        {
            var linha = new LinhaCalculo
            {
                Rotulo = tl.Rotulo,
                TipoLinha = tl.TipoLinha,
                NivelIndentacao = tl.NivelIndentacao,
                Negrito = tl.Negrito,
                ValoresPorAno = new Dictionary<int, decimal>()
            };

            switch (tl.TipoLinha)
            {
                case TipoLinha.ContaEspecifica:
                    linha.CodigoConta = tl.CodigoConta;
                    foreach (var ano in anos)
                    {
                        var valor = saldos.GetValueOrDefault(tl.CodigoConta ?? "")?.GetValueOrDefault(ano) ?? 0;
                        linha.ValoresPorAno[ano] = tl.InverterSinal ? -valor : valor;
                    }
                    break;

                case TipoLinha.GrupoNatureza:
                    var contasNatureza = contas.Where(c =>
                        c.Natureza == tl.NaturezaFiltro && c.ContaPaiId == null).ToList();
                    foreach (var ano in anos)
                    {
                        var soma = contasNatureza.Sum(c =>
                            saldos.GetValueOrDefault(c.Codigo)?.GetValueOrDefault(ano) ?? 0);
                        linha.ValoresPorAno[ano] = tl.InverterSinal ? -soma : soma;
                    }
                    break;

                case TipoLinha.GrupoSubtipo:
                    var contasSubtipo = contas.Where(c =>
                        c.Subtipo == tl.SubtipoFiltro && c.ContaPaiId == null).ToList();
                    foreach (var ano in anos)
                    {
                        var soma = contasSubtipo.Sum(c =>
                            saldos.GetValueOrDefault(c.Codigo)?.GetValueOrDefault(ano) ?? 0);
                        linha.ValoresPorAno[ano] = tl.InverterSinal ? -soma : soma;
                    }
                    break;

                case TipoLinha.Totalizador:
                    if (!string.IsNullOrWhiteSpace(tl.LinhasReferenciadas))
                    {
                        var idsRef = tl.LinhasReferenciadas.Split(',')
                            .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : Guid.Empty)
                            .Where(g => g != Guid.Empty)
                            .ToList();

                        foreach (var ano in anos)
                        {
                            var soma = idsRef.Sum(id =>
                                linhasCalculadas.GetValueOrDefault(id)?.GetValueOrDefault(ano) ?? 0);
                            linha.ValoresPorAno[ano] = tl.InverterSinal ? -soma : soma;
                        }
                    }
                    break;

                case TipoLinha.Formula:
                    if (!string.IsNullOrWhiteSpace(tl.Expressao))
                    {
                        var refs = FormulaParser.ExtrairReferencias(tl.Expressao);
                        foreach (var ano in anos)
                        {
                            var valoresRef = new Dictionary<string, decimal>();
                            foreach (var refCodigo in refs)
                            {
                                valoresRef[refCodigo] = saldos.GetValueOrDefault(refCodigo)?.GetValueOrDefault(ano) ?? 0;
                            }
                            var valor = FormulaParser.Avaliar(tl.Expressao, valoresRef);
                            linha.ValoresPorAno[ano] = tl.InverterSinal ? -valor : valor;
                        }
                    }
                    break;

                case TipoLinha.Separador:
                    foreach (var ano in anos)
                        linha.ValoresPorAno[ano] = 0;
                    break;
            }

            linhasCalculadas[tl.Id] = linha.ValoresPorAno;
            linhas.Add(linha);
        }

        return linhas;
    }

    /// <summary>
    /// Gera linhas padrão do demonstrativo quando não há template configurado.
    /// Organiza por natureza e hierarquia.
    /// </summary>
    private List<LinhaCalculo> GerarLinhasPadrao(
        List<ContaContabil> contas,
        Dictionary<string, Dictionary<int, decimal>> saldos,
        List<int> anos,
        TipoDemonstrativo tipo)
    {
        var linhas = new List<LinhaCalculo>();

        NaturezaConta[] naturezas = tipo switch
        {
            TipoDemonstrativo.Balanco => new[] { NaturezaConta.Ativo, NaturezaConta.Passivo },
            TipoDemonstrativo.Balancete => new[] { NaturezaConta.Ativo, NaturezaConta.Passivo, NaturezaConta.Receita, NaturezaConta.Despesa },
            TipoDemonstrativo.DRE => new[] { NaturezaConta.Receita, NaturezaConta.Despesa },
            _ => new[] { NaturezaConta.Ativo, NaturezaConta.Passivo, NaturezaConta.Receita, NaturezaConta.Despesa }
        };

        foreach (var natureza in naturezas)
        {
            // Cabeçalho da natureza
            linhas.Add(new LinhaCalculo
            {
                Rotulo = natureza.ToString().ToUpperInvariant(),
                TipoLinha = TipoLinha.Separador,
                NivelIndentacao = 0,
                Negrito = true,
                ValoresPorAno = anos.ToDictionary(a => a, _ => 0m)
            });

            // Contas raiz desta natureza
            var contasRaiz = contas
                .Where(c => c.Natureza == natureza && c.ContaPaiId == null)
                .OrderBy(c => c.Ordem)
                .ThenBy(c => c.Codigo)
                .ToList();

            foreach (var conta in contasRaiz)
            {
                AdicionarContaEFilhas(conta, contas, saldos, anos, linhas, 0);
            }

            // Total da natureza
            var totalNatureza = new LinhaCalculo
            {
                Rotulo = $"TOTAL {natureza.ToString().ToUpperInvariant()}",
                TipoLinha = TipoLinha.Totalizador,
                NivelIndentacao = 0,
                Negrito = true,
                ValoresPorAno = new Dictionary<int, decimal>()
            };

            foreach (var ano in anos)
            {
                totalNatureza.ValoresPorAno[ano] = contasRaiz.Sum(c =>
                    saldos.GetValueOrDefault(c.Codigo)?.GetValueOrDefault(ano) ?? 0);
            }

            linhas.Add(totalNatureza);

            // Linha em branco
            linhas.Add(new LinhaCalculo
            {
                Rotulo = "",
                TipoLinha = TipoLinha.Separador,
                ValoresPorAno = anos.ToDictionary(a => a, _ => 0m)
            });
        }

        return linhas;
    }

    private void AdicionarContaEFilhas(
        ContaContabil conta,
        List<ContaContabil> todasContas,
        Dictionary<string, Dictionary<int, decimal>> saldos,
        List<int> anos,
        List<LinhaCalculo> linhas,
        int nivel)
    {
        var linha = new LinhaCalculo
        {
            Rotulo = $"{conta.Codigo} - {conta.Nome}",
            CodigoConta = conta.Codigo,
            TipoLinha = conta.IsSintetica ? TipoLinha.GrupoNatureza : TipoLinha.ContaEspecifica,
            NivelIndentacao = nivel,
            Negrito = conta.IsSintetica,
            ValoresPorAno = new Dictionary<int, decimal>()
        };

        foreach (var ano in anos)
        {
            linha.ValoresPorAno[ano] = saldos.GetValueOrDefault(conta.Codigo)?.GetValueOrDefault(ano) ?? 0;
        }

        linhas.Add(linha);

        // Filhas
        var filhas = todasContas
            .Where(c => c.ContaPaiId == conta.Id)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Codigo)
            .ToList();

        foreach (var filha in filhas)
        {
            AdicionarContaEFilhas(filha, todasContas, saldos, anos, linhas, nivel + 1);
        }
    }
}
