using ContabilidadeAnalitica.Application.Common;
using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.CalculationEngine.Core;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Domain.Interfaces;

namespace ContabilidadeAnalitica.Application.UseCases.Demonstrativos;

public class DemonstrativoUseCases
{
    private readonly IUnitOfWork _uow;
    private readonly IMotorCalculoContabil _motorCalculo;

    public DemonstrativoUseCases(IUnitOfWork uow, IMotorCalculoContabil motorCalculo)
    {
        _uow = uow;
        _motorCalculo = motorCalculo;
    }

    public async Task<ResultadoOperacao<DemonstrativoResultadoDto>> GerarAsync(
        ConsultaDemonstrativoDto consulta, CancellationToken ct = default)
    {
        // Validações
        if (consulta.Anos == null || consulta.Anos.Count == 0)
            return new ValidacaoInvalida("Deve ser informado ao menos um ano para consulta.");

        var empresa = await _uow.Empresas.ObterPorIdAsync(consulta.EmpresaId, ct);
        if (empresa is null)
            return new NaoEncontrado($"Empresa '{consulta.EmpresaId}' não encontrada.");

        // Obter plano vigente
        var plano = await _uow.PlanosContas.ObterVigenteAsync(consulta.EmpresaId, null, ct);
        if (plano is null)
            return new NaoEncontrado($"Nenhum plano de contas vigente para a empresa '{consulta.EmpresaId}'.");

        // Obter hierarquia completa de contas
        var contas = await _uow.ContasContabeis.ObterHierarquiaCompletaAsync(plano.Id, ct);
        if (contas.Count == 0)
            return new NaoEncontrado("O plano de contas vigente não possui contas cadastradas.");

        // Obter saldos
        var saldos = await _uow.SaldosContas.ObterPorEmpresaEAnosAsync(consulta.EmpresaId, consulta.Anos, ct);

        // Obter template
        TemplateDemonstrativo? template = null;
        if (consulta.TemplateId.HasValue)
        {
            template = await _uow.TemplatesDemonstrativos.ObterComLinhasAsync(consulta.TemplateId.Value, ct);
            if (template is null)
                return new NaoEncontrado($"Template '{consulta.TemplateId}' não encontrado.");
        }
        else
        {
            template = await _uow.TemplatesDemonstrativos.ObterPadraoAsync(consulta.Tipo, consulta.EmpresaId, ct);
        }

        // Executar motor de cálculo
        var contexto = new ContextoCalculo
        {
            Contas = contas.ToList(),
            Saldos = saldos.ToList(),
            AnosConsultados = consulta.Anos,
            Tipo = consulta.Tipo,
            Template = template
        };

        var resultado = _motorCalculo.Calcular(contexto);

        if (resultado.TemErro)
            return new ConflitoDominio(resultado.MensagemErro!);

        var anosOrdenados = consulta.Anos.OrderBy(a => a).ToList();
        var contasDict = contas.ToDictionary(c => c.Codigo, c => c);

        // Calcular totais por natureza para AV
        var totaisPorNaturezaPorAno = CalcularTotaisPorNatureza(contas.ToList(), resultado.Linhas, anosOrdenados);

        // Converter LinhaCalculo → DemonstrativoLinhaDto (formato do frontend)
        var linhas = new List<DemonstrativoLinhaDto>();

        foreach (var l in resultado.Linhas)
        {
            // Ignorar separadores vazios
            if (l.TipoLinha == TipoLinha.Separador && string.IsNullOrWhiteSpace(l.Rotulo))
                continue;

            // Determinar a conta associada (se houver)
            ContaContabil? conta = null;
            if (!string.IsNullOrWhiteSpace(l.CodigoConta) && contasDict.TryGetValue(l.CodigoConta, out var c))
                conta = c;

            // Determinar Tipo e SubTipo
            string tipo = DeterminarTipo(l, conta);
            string subtipo = DeterminarSubTipo(l, conta);
            string nome = DeterminarNome(l, conta);
            string? codigo = l.CodigoConta;
            int nivel = conta?.Nivel ?? l.NivelIndentacao;
            bool sintetica = conta?.IsSintetica ?? (l.TipoLinha == TipoLinha.GrupoNatureza
                || l.TipoLinha == TipoLinha.GrupoSubtipo
                || l.TipoLinha == TipoLinha.Totalizador
                || l.TipoLinha == TipoLinha.Separador);

            // Determinar natureza para AV
            NaturezaConta? natureza = conta?.Natureza ?? InferirNatureza(l, tipo);

            // Construir lançamentos por ano com AV
            var lancamentos = new Dictionary<string, LancamentoAnoDto>();
            foreach (var ano in anosOrdenados)
            {
                var valor = l.ValoresPorAno.GetValueOrDefault(ano, 0m);
                decimal av = CalcularAV(valor, natureza, ano, totaisPorNaturezaPorAno);

                lancamentos[ano.ToString()] = new LancamentoAnoDto
                {
                    Valor = Math.Round(valor, 2),
                    AV = Math.Round(av, 2)
                };
            }

            // Calcular AH
            decimal ah = CalcularAH(l.ValoresPorAno, anosOrdenados);

            linhas.Add(new DemonstrativoLinhaDto
            {
                Nome = nome,
                Tipo = tipo,
                SubTipo = subtipo,
                Codigo = codigo,
                Nivel = nivel,
                Sintetica = sintetica,
                AH = Math.Round(ah, 2),
                Lancamentos = lancamentos
            });
        }

        var dto = new DemonstrativoResultadoDto(
            consulta.Tipo,
            template?.Nome ?? $"{consulta.Tipo} Padrão",
            consulta.EmpresaId,
            empresa.RazaoSocial,
            anosOrdenados,
            linhas,
            DateTime.UtcNow
        );

        return new Sucesso<DemonstrativoResultadoDto>(dto);
    }

    // ─────────────────────────────────────────────
    // Helpers para AV, AH e mapeamento de campos
    // ─────────────────────────────────────────────

    /// <summary>
    /// Calcula os totais por natureza (Ativo, Passivo, Receita, Despesa) para cada ano.
    /// Usado como denominador na Análise Vertical.
    /// Busca os totais diretamente nos saldos calculados pelo motor, não nas linhas do template.
    /// </summary>
    private Dictionary<NaturezaConta, Dictionary<int, decimal>> CalcularTotaisPorNatureza(
        List<ContaContabil> contas,
        List<LinhaCalculo> linhasCalculadas,
        List<int> anos)
    {
        var totais = new Dictionary<NaturezaConta, Dictionary<int, decimal>>();

        foreach (NaturezaConta nat in Enum.GetValues<NaturezaConta>())
        {
            totais[nat] = new Dictionary<int, decimal>();
            foreach (var ano in anos)
                totais[nat][ano] = 0m;
        }

        // Buscar as contas raiz de cada natureza (nível 1, sem pai)
        var contasRaiz = contas.Where(c => c.ContaPaiId == null).ToList();
        foreach (var contaRaiz in contasRaiz)
        {
            // Buscar o valor nas linhas calculadas pelo motor (por código)
            var linhaCorrespondente = linhasCalculadas
                .FirstOrDefault(l => l.CodigoConta == contaRaiz.Codigo);

            if (linhaCorrespondente != null)
            {
                foreach (var ano in anos)
                {
                    totais[contaRaiz.Natureza][ano] += Math.Abs(
                        linhaCorrespondente.ValoresPorAno.GetValueOrDefault(ano, 0m));
                }
            }
            else
            {
                // Se não encontrou nas linhas (ex: template com GrupoNatureza),
                // buscar em qualquer linha que contenha o código da conta raiz
                // ou calcular a partir das linhas filhas
                var linhasFilhas = linhasCalculadas
                    .Where(l => !string.IsNullOrWhiteSpace(l.CodigoConta)
                        && l.CodigoConta.StartsWith(contaRaiz.Codigo + ".")
                        && contas.Any(c => c.Codigo == l.CodigoConta && c.ContaPaiId == contaRaiz.Id))
                    .ToList();

                if (linhasFilhas.Count > 0)
                {
                    foreach (var ano in anos)
                    {
                        totais[contaRaiz.Natureza][ano] += Math.Abs(
                            linhasFilhas.Sum(lf => lf.ValoresPorAno.GetValueOrDefault(ano, 0m)));
                    }
                }
                else
                {
                    // Fallback: buscar qualquer linha com GrupoNatureza correspondente
                    var linhasNatureza = linhasCalculadas
                        .Where(l => l.TipoLinha == TipoLinha.GrupoNatureza
                            && l.Rotulo.Contains(contaRaiz.Nome, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (linhasNatureza.Count > 0)
                    {
                        foreach (var ano in anos)
                        {
                            totais[contaRaiz.Natureza][ano] += Math.Abs(
                                linhasNatureza.First().ValoresPorAno.GetValueOrDefault(ano, 0m));
                        }
                    }
                }
            }
        }

        return totais;
    }

    /// <summary>
    /// Análise Vertical: percentual da conta em relação ao total da natureza no mesmo ano.
    /// </summary>
    private decimal CalcularAV(
        decimal valor,
        NaturezaConta? natureza,
        int ano,
        Dictionary<NaturezaConta, Dictionary<int, decimal>> totais)
    {
        if (!natureza.HasValue) return 0m;

        var totalNatureza = totais.GetValueOrDefault(natureza.Value)?.GetValueOrDefault(ano, 0m) ?? 0m;
        if (totalNatureza == 0m) return 0m;

        return (valor / totalNatureza) * 100m;
    }

    /// <summary>
    /// Análise Horizontal: variação percentual entre o último e o primeiro ano.
    /// Fórmula: ((ValorÚltimoAno - ValorPrimeiroAno) / |ValorPrimeiroAno|) * 100
    /// </summary>
    private decimal CalcularAH(Dictionary<int, decimal> valoresPorAno, List<int> anosOrdenados)
    {
        if (anosOrdenados.Count < 2) return 0m;

        var primeiroAno = anosOrdenados.First();
        var ultimoAno = anosOrdenados.Last();

        var valorPrimeiro = valoresPorAno.GetValueOrDefault(primeiroAno, 0m);
        var valorUltimo = valoresPorAno.GetValueOrDefault(ultimoAno, 0m);

        if (valorPrimeiro == 0m) return 0m;

        return ((valorUltimo - valorPrimeiro) / Math.Abs(valorPrimeiro)) * 100m;
    }

    /// <summary>
    /// Determina o nome limpo da conta (sem prefixo de código quando já vem do motor).
    /// </summary>
    private string DeterminarNome(LinhaCalculo linha, ContaContabil? conta)
    {
        if (conta != null)
            return conta.Nome;

        // Limpar rótulos que contêm "código - nome"
        var rotulo = linha.Rotulo.Trim();
        if (!string.IsNullOrWhiteSpace(linha.CodigoConta) && rotulo.StartsWith(linha.CodigoConta))
        {
            var idx = rotulo.IndexOf(" - ");
            if (idx > 0)
                return rotulo[(idx + 3)..].Trim();
        }

        return rotulo;
    }

    /// <summary>
    /// Mapeia a natureza da conta para o nome esperado pelo frontend.
    /// </summary>
    private string DeterminarTipo(LinhaCalculo linha, ContaContabil? conta)
    {
        if (conta != null)
            return MapearNatureza(conta.Natureza);

        // Inferir do rótulo para linhas sem conta
        var rotulo = linha.Rotulo.Trim().ToUpperInvariant();
        if (rotulo.Contains("ATIVO")) return "Ativo";
        if (rotulo.Contains("PASSIVO")) return "Passivo";
        if (rotulo.Contains("RECEITA")) return "Receita";
        if (rotulo.Contains("DESPESA")) return "Despesa";

        return "";
    }

    /// <summary>
    /// Mapeia o subtipo da conta para o nome esperado pelo frontend.
    /// </summary>
    private string DeterminarSubTipo(LinhaCalculo linha, ContaContabil? conta)
    {
        if (conta != null)
            return MapearSubtipo(conta.Subtipo);

        return "";
    }

    private NaturezaConta? InferirNatureza(LinhaCalculo linha, string tipo)
    {
        return tipo switch
        {
            "Ativo" => NaturezaConta.Ativo,
            "Passivo" => NaturezaConta.Passivo,
            "Receita" => NaturezaConta.Receita,
            "Despesa" => NaturezaConta.Despesa,
            _ => null
        };
    }

    private static string MapearNatureza(NaturezaConta natureza) => natureza switch
    {
        NaturezaConta.Ativo => "Ativo",
        NaturezaConta.Passivo => "Passivo",
        NaturezaConta.Receita => "Receita",
        NaturezaConta.Despesa => "Despesa",
        _ => ""
    };

    private static string MapearSubtipo(SubtipoConta subtipo) => subtipo switch
    {
        SubtipoConta.Circulante => "Circulante",
        SubtipoConta.NaoCirculante => "Não Circulante",
        SubtipoConta.PatrimonioLiquido => "Patrimônio Líquido",
        SubtipoConta.ReceitaOperacional => "Receita Operacional",
        SubtipoConta.ReceitaNaoOperacional => "Receita Não Operacional",
        SubtipoConta.DespesaOperacional => "Despesa Operacional",
        SubtipoConta.DespesaNaoOperacional => "Despesa Não Operacional",
        SubtipoConta.Compensacao => "Compensação",
        _ => ""
    };
}
