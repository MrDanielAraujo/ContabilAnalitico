using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Data;

/// <summary>
/// Seed de dados para ambiente de desenvolvimento.
/// Inclui grupo empresarial, empresa, plano de contas completo e saldos de teste.
/// </summary>
public static class SeedData
{
    public static async Task InicializarAsync(ContabilidadeDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Empresas.AnyAsync())
            return; // Já possui dados

        // ── Grupo Empresarial ──
        var grupo = new GrupoEmpresarial
        {
            Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
            Nome = "Grupo Exemplo S.A.",
            Descricao = "Grupo empresarial de demonstração",
            CodigoExterno = "GRP001"
        };
        context.GruposEmpresariais.Add(grupo);

        // ── Empresa ──
        var empresa = new Empresa
        {
            Id = Guid.Parse("b1000000-0000-0000-0000-000000000001"),
            RazaoSocial = "Empresa Exemplo Ltda.",
            NomeFantasia = "Exemplo",
            CNPJ = "12.345.678/0001-90",
            CodigoExterno = "EMP001",
            MoedaPadrao = "BRL",
            GrupoEmpresarialId = grupo.Id
        };
        context.Empresas.Add(empresa);

        // ── Plano de Contas ──
        var plano = new PlanoContas
        {
            Id = Guid.Parse("c1000000-0000-0000-0000-000000000001"),
            Nome = "Plano de Contas Padrão",
            Descricao = "Plano de contas inicial para demonstração",
            Versao = 1,
            VigenciaInicio = new DateTime(2020, 1, 1),
            Vigente = true,
            EmpresaId = empresa.Id
        };
        context.PlanosContas.Add(plano);

        // ── Contas Contábeis ──
        var contas = CriarPlanoContasExemplo(plano.Id);
        context.ContasContabeis.AddRange(contas);

        // ── Saldos de Teste ──
        var saldos = CriarSaldosTeste(contas, empresa.Id);
        context.SaldosContas.AddRange(saldos);

        // ── Template Balanço Padrão ──
        var templateBalanco = CriarTemplateBalancoPadrao();
        context.TemplatesDemonstrativos.Add(templateBalanco);

        // ── Template Balancete Padrão ──
        var templateBalancete = CriarTemplateBalancetePadrao();
        context.TemplatesDemonstrativos.Add(templateBalancete);

        await context.SaveChangesAsync();
    }

    private static List<ContaContabil> CriarPlanoContasExemplo(Guid planoId)
    {
        var contas = new List<ContaContabil>();

        // ═══════════════════════════════════════
        // ATIVO
        // ═══════════════════════════════════════
        var ativo = Conta(planoId, "1", "ATIVO", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Sintetica, null, 1);
        contas.Add(ativo);

        // Ativo Circulante
        var ativoCirc = Conta(planoId, "1.1", "Ativo Circulante", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Sintetica, ativo.Id, 1);
        contas.Add(ativoCirc);

        var caixa = Conta(planoId, "1.1.01", "Caixa e Equivalentes de Caixa", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Sintetica, ativoCirc.Id, 1);
        contas.Add(caixa);
        contas.Add(Conta(planoId, "1.1.01.001", "Caixa Geral", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, caixa.Id, 1, true));
        contas.Add(Conta(planoId, "1.1.01.002", "Bancos Conta Movimento", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, caixa.Id, 2, true));
        contas.Add(Conta(planoId, "1.1.01.003", "Aplicações Financeiras de Liquidez Imediata", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, caixa.Id, 3, true));

        var receber = Conta(planoId, "1.1.02", "Contas a Receber", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Sintetica, ativoCirc.Id, 2);
        contas.Add(receber);
        contas.Add(Conta(planoId, "1.1.02.001", "Clientes", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, receber.Id, 1, true));
        contas.Add(Conta(planoId, "1.1.02.002", "(-) Provisão para Devedores Duvidosos", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, receber.Id, 2, true));

        var estoques = Conta(planoId, "1.1.03", "Estoques", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Sintetica, ativoCirc.Id, 3);
        contas.Add(estoques);
        contas.Add(Conta(planoId, "1.1.03.001", "Mercadorias para Revenda", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, estoques.Id, 1, true));
        contas.Add(Conta(planoId, "1.1.03.002", "Matérias-Primas", NaturezaConta.Ativo, SubtipoConta.Circulante, TipoConta.Analitica, estoques.Id, 2, true));

        // Ativo Não Circulante
        var ativoNaoCirc = Conta(planoId, "1.2", "Ativo Não Circulante", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Sintetica, ativo.Id, 2);
        contas.Add(ativoNaoCirc);

        var imobilizado = Conta(planoId, "1.2.01", "Imobilizado", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Sintetica, ativoNaoCirc.Id, 1);
        contas.Add(imobilizado);
        contas.Add(Conta(planoId, "1.2.01.001", "Terrenos", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Analitica, imobilizado.Id, 1, true));
        contas.Add(Conta(planoId, "1.2.01.002", "Edificações", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Analitica, imobilizado.Id, 2, true));
        contas.Add(Conta(planoId, "1.2.01.003", "Máquinas e Equipamentos", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Analitica, imobilizado.Id, 3, true));
        contas.Add(Conta(planoId, "1.2.01.004", "(-) Depreciação Acumulada", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Analitica, imobilizado.Id, 4, true));

        var intangivel = Conta(planoId, "1.2.02", "Intangível", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Sintetica, ativoNaoCirc.Id, 2);
        contas.Add(intangivel);
        contas.Add(Conta(planoId, "1.2.02.001", "Software", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Analitica, intangivel.Id, 1, true));
        contas.Add(Conta(planoId, "1.2.02.002", "(-) Amortização Acumulada", NaturezaConta.Ativo, SubtipoConta.NaoCirculante, TipoConta.Analitica, intangivel.Id, 2, true));

        // ═══════════════════════════════════════
        // PASSIVO
        // ═══════════════════════════════════════
        var passivo = Conta(planoId, "2", "PASSIVO", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Sintetica, null, 2);
        contas.Add(passivo);

        var passivoCirc = Conta(planoId, "2.1", "Passivo Circulante", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Sintetica, passivo.Id, 1);
        contas.Add(passivoCirc);

        var fornecedores = Conta(planoId, "2.1.01", "Fornecedores", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Sintetica, passivoCirc.Id, 1);
        contas.Add(fornecedores);
        contas.Add(Conta(planoId, "2.1.01.001", "Fornecedores Nacionais", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, fornecedores.Id, 1, true));
        contas.Add(Conta(planoId, "2.1.01.002", "Fornecedores Internacionais", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, fornecedores.Id, 2, true));

        var obrigTrib = Conta(planoId, "2.1.02", "Obrigações Tributárias", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Sintetica, passivoCirc.Id, 2);
        contas.Add(obrigTrib);
        contas.Add(Conta(planoId, "2.1.02.001", "ICMS a Recolher", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, obrigTrib.Id, 1, true));
        contas.Add(Conta(planoId, "2.1.02.002", "PIS/COFINS a Recolher", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, obrigTrib.Id, 2, true));
        contas.Add(Conta(planoId, "2.1.02.003", "IR/CS a Recolher", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, obrigTrib.Id, 3, true));

        var obrigTrab = Conta(planoId, "2.1.03", "Obrigações Trabalhistas", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Sintetica, passivoCirc.Id, 3);
        contas.Add(obrigTrab);
        contas.Add(Conta(planoId, "2.1.03.001", "Salários a Pagar", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, obrigTrab.Id, 1, true));
        contas.Add(Conta(planoId, "2.1.03.002", "FGTS a Recolher", NaturezaConta.Passivo, SubtipoConta.Circulante, TipoConta.Analitica, obrigTrab.Id, 2, true));

        // Passivo Não Circulante
        var passivoNaoCirc = Conta(planoId, "2.2", "Passivo Não Circulante", NaturezaConta.Passivo, SubtipoConta.NaoCirculante, TipoConta.Sintetica, passivo.Id, 2);
        contas.Add(passivoNaoCirc);
        contas.Add(Conta(planoId, "2.2.01", "Empréstimos e Financiamentos LP", NaturezaConta.Passivo, SubtipoConta.NaoCirculante, TipoConta.Analitica, passivoNaoCirc.Id, 1, true));

        // Patrimônio Líquido
        var pl = Conta(planoId, "2.3", "Patrimônio Líquido", NaturezaConta.Passivo, SubtipoConta.PatrimonioLiquido, TipoConta.Sintetica, passivo.Id, 3);
        contas.Add(pl);
        contas.Add(Conta(planoId, "2.3.01", "Capital Social", NaturezaConta.Passivo, SubtipoConta.PatrimonioLiquido, TipoConta.Analitica, pl.Id, 1, true));
        contas.Add(Conta(planoId, "2.3.02", "Reservas de Lucros", NaturezaConta.Passivo, SubtipoConta.PatrimonioLiquido, TipoConta.Analitica, pl.Id, 2, true));
        contas.Add(Conta(planoId, "2.3.03", "Lucros/Prejuízos Acumulados", NaturezaConta.Passivo, SubtipoConta.PatrimonioLiquido, TipoConta.Analitica, pl.Id, 3, true));

        // ═══════════════════════════════════════
        // RECEITAS
        // ═══════════════════════════════════════
        var receita = Conta(planoId, "3", "RECEITAS", NaturezaConta.Receita, SubtipoConta.ReceitaOperacional, TipoConta.Sintetica, null, 3);
        contas.Add(receita);

        var recOper = Conta(planoId, "3.1", "Receitas Operacionais", NaturezaConta.Receita, SubtipoConta.ReceitaOperacional, TipoConta.Sintetica, receita.Id, 1);
        contas.Add(recOper);
        contas.Add(Conta(planoId, "3.1.01", "Receita Bruta de Vendas", NaturezaConta.Receita, SubtipoConta.ReceitaOperacional, TipoConta.Analitica, recOper.Id, 1, true));
        contas.Add(Conta(planoId, "3.1.02", "Receita de Serviços", NaturezaConta.Receita, SubtipoConta.ReceitaOperacional, TipoConta.Analitica, recOper.Id, 2, true));
        contas.Add(Conta(planoId, "3.1.03", "(-) Deduções de Vendas", NaturezaConta.Receita, SubtipoConta.ReceitaOperacional, TipoConta.Analitica, recOper.Id, 3, true));

        var recNaoOper = Conta(planoId, "3.2", "Receitas Não Operacionais", NaturezaConta.Receita, SubtipoConta.ReceitaNaoOperacional, TipoConta.Sintetica, receita.Id, 2);
        contas.Add(recNaoOper);
        contas.Add(Conta(planoId, "3.2.01", "Receitas Financeiras", NaturezaConta.Receita, SubtipoConta.ReceitaNaoOperacional, TipoConta.Analitica, recNaoOper.Id, 1, true));

        // ═══════════════════════════════════════
        // DESPESAS
        // ═══════════════════════════════════════
        var despesa = Conta(planoId, "4", "DESPESAS", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Sintetica, null, 4);
        contas.Add(despesa);

        var despOper = Conta(planoId, "4.1", "Despesas Operacionais", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Sintetica, despesa.Id, 1);
        contas.Add(despOper);

        var cmv = Conta(planoId, "4.1.01", "Custo das Mercadorias Vendidas", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Sintetica, despOper.Id, 1);
        contas.Add(cmv);
        contas.Add(Conta(planoId, "4.1.01.001", "CMV - Mercadorias", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Analitica, cmv.Id, 1, true));

        var despAdm = Conta(planoId, "4.1.02", "Despesas Administrativas", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Sintetica, despOper.Id, 2);
        contas.Add(despAdm);
        contas.Add(Conta(planoId, "4.1.02.001", "Salários e Encargos", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Analitica, despAdm.Id, 1, true));
        contas.Add(Conta(planoId, "4.1.02.002", "Aluguéis", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Analitica, despAdm.Id, 2, true));
        contas.Add(Conta(planoId, "4.1.02.003", "Energia e Telecomunicações", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Analitica, despAdm.Id, 3, true));
        contas.Add(Conta(planoId, "4.1.02.004", "Depreciação e Amortização", NaturezaConta.Despesa, SubtipoConta.DespesaOperacional, TipoConta.Analitica, despAdm.Id, 4, true));

        var despNaoOper = Conta(planoId, "4.2", "Despesas Não Operacionais", NaturezaConta.Despesa, SubtipoConta.DespesaNaoOperacional, TipoConta.Sintetica, despesa.Id, 2);
        contas.Add(despNaoOper);
        contas.Add(Conta(planoId, "4.2.01", "Despesas Financeiras", NaturezaConta.Despesa, SubtipoConta.DespesaNaoOperacional, TipoConta.Analitica, despNaoOper.Id, 1, true));

        return contas;
    }

    private static List<SaldoConta> CriarSaldosTeste(List<ContaContabil> contas, Guid empresaId)
    {
        var saldos = new List<SaldoConta>();
        var contasPorCodigo = contas.ToDictionary(c => c.Codigo, c => c);

        // Dados para 3 anos: 2024, 2025, 2026
        var dadosTeste = new Dictionary<string, decimal[]>
        {
            // Ativo Circulante                    2024        2025        2026
            ["1.1.01.001"] = new[] { 150000.00m, 180000.00m, 210000.00m },
            ["1.1.01.002"] = new[] { 500000.00m, 620000.00m, 750000.00m },
            ["1.1.01.003"] = new[] { 300000.00m, 350000.00m, 400000.00m },
            ["1.1.02.001"] = new[] { 420000.00m, 480000.00m, 550000.00m },
            ["1.1.02.002"] = new[] { -21000.00m, -24000.00m, -27500.00m },
            ["1.1.03.001"] = new[] { 280000.00m, 310000.00m, 340000.00m },
            ["1.1.03.002"] = new[] { 120000.00m, 135000.00m, 150000.00m },

            // Ativo Não Circulante
            ["1.2.01.001"] = new[] { 800000.00m, 800000.00m, 800000.00m },
            ["1.2.01.002"] = new[] { 1200000.00m, 1200000.00m, 1500000.00m },
            ["1.2.01.003"] = new[] { 450000.00m, 500000.00m, 550000.00m },
            ["1.2.01.004"] = new[] { -350000.00m, -420000.00m, -490000.00m },
            ["1.2.02.001"] = new[] { 180000.00m, 220000.00m, 260000.00m },
            ["1.2.02.002"] = new[] { -60000.00m, -90000.00m, -120000.00m },

            // Passivo Circulante
            ["2.1.01.001"] = new[] { 320000.00m, 350000.00m, 380000.00m },
            ["2.1.01.002"] = new[] { 80000.00m, 95000.00m, 110000.00m },
            ["2.1.02.001"] = new[] { 45000.00m, 52000.00m, 58000.00m },
            ["2.1.02.002"] = new[] { 38000.00m, 42000.00m, 47000.00m },
            ["2.1.02.003"] = new[] { 65000.00m, 72000.00m, 80000.00m },
            ["2.1.03.001"] = new[] { 180000.00m, 195000.00m, 210000.00m },
            ["2.1.03.002"] = new[] { 25000.00m, 28000.00m, 30000.00m },

            // Passivo Não Circulante
            ["2.2.01"] = new[] { 600000.00m, 500000.00m, 400000.00m },

            // Patrimônio Líquido
            ["2.3.01"] = new[] { 1500000.00m, 1500000.00m, 1500000.00m },
            ["2.3.02"] = new[] { 350000.00m, 450000.00m, 580000.00m },
            ["2.3.03"] = new[] { 265000.00m, 477000.00m, 728500.00m },

            // Receitas
            ["3.1.01"] = new[] { 3200000.00m, 3800000.00m, 4500000.00m },
            ["3.1.02"] = new[] { 800000.00m, 950000.00m, 1100000.00m },
            ["3.1.03"] = new[] { -640000.00m, -760000.00m, -900000.00m },
            ["3.2.01"] = new[] { 45000.00m, 55000.00m, 65000.00m },

            // Despesas
            ["4.1.01.001"] = new[] { 1920000.00m, 2280000.00m, 2700000.00m },
            ["4.1.02.001"] = new[] { 480000.00m, 520000.00m, 560000.00m },
            ["4.1.02.002"] = new[] { 96000.00m, 100000.00m, 108000.00m },
            ["4.1.02.003"] = new[] { 36000.00m, 38000.00m, 40000.00m },
            ["4.1.02.004"] = new[] { 85000.00m, 95000.00m, 105000.00m },
            ["4.2.01"] = new[] { 72000.00m, 65000.00m, 58000.00m },
        };

        int[] anos = { 2024, 2025, 2026 };

        foreach (var (codigo, valores) in dadosTeste)
        {
            if (!contasPorCodigo.TryGetValue(codigo, out var conta))
                continue;

            for (int i = 0; i < anos.Length; i++)
            {
                // Distribuir valor anual em 12 meses (simplificado: valor/12 por mês)
                var valorMensal = valores[i] / 12;
                for (int mes = 1; mes <= 12; mes++)
                {
                    saldos.Add(new SaldoConta
                    {
                        ContaContabilId = conta.Id,
                        EmpresaId = empresaId,
                        Ano = anos[i],
                        Mes = mes,
                        Valor = Math.Round(valorMensal, 4),
                        SaldoDevedor = conta.Natureza == NaturezaConta.Ativo || conta.Natureza == NaturezaConta.Despesa
                            ? Math.Round(Math.Abs(valorMensal), 4) : 0,
                        SaldoCredor = conta.Natureza == NaturezaConta.Passivo || conta.Natureza == NaturezaConta.Receita
                            ? Math.Round(Math.Abs(valorMensal), 4) : 0,
                        Moeda = "BRL",
                        Origem = "Seed"
                    });
                }
            }
        }

        return saldos;
    }

    private static TemplateDemonstrativo CriarTemplateBalancoPadrao()
    {
        return new TemplateDemonstrativo
        {
            Id = Guid.Parse("d1000000-0000-0000-0000-000000000001"),
            Nome = "Balanço Patrimonial Padrão",
            Descricao = "Template padrão para Balanço Patrimonial",
            Tipo = TipoDemonstrativo.Balanco,
            Padrao = true,
            EmpresaId = null,
            Linhas = new List<TemplateLinha>
            {
                new() { Rotulo = "ATIVO", TipoLinha = TipoLinha.GrupoNatureza, NaturezaFiltro = NaturezaConta.Ativo, Ordem = 1, Negrito = true },
                new() { Rotulo = "  Ativo Circulante", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.1", Ordem = 2, NivelIndentacao = 1 },
                new() { Rotulo = "    Caixa e Equivalentes", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.1.01", Ordem = 3, NivelIndentacao = 2 },
                new() { Rotulo = "    Contas a Receber", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.1.02", Ordem = 4, NivelIndentacao = 2 },
                new() { Rotulo = "    Estoques", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.1.03", Ordem = 5, NivelIndentacao = 2 },
                new() { Rotulo = "  Ativo Não Circulante", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.2", Ordem = 6, NivelIndentacao = 1 },
                new() { Rotulo = "    Imobilizado", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.2.01", Ordem = 7, NivelIndentacao = 2 },
                new() { Rotulo = "    Intangível", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "1.2.02", Ordem = 8, NivelIndentacao = 2 },
                new() { Rotulo = "", TipoLinha = TipoLinha.Separador, Ordem = 9 },
                new() { Rotulo = "PASSIVO E PATRIMÔNIO LÍQUIDO", TipoLinha = TipoLinha.GrupoNatureza, NaturezaFiltro = NaturezaConta.Passivo, Ordem = 10, Negrito = true },
                new() { Rotulo = "  Passivo Circulante", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.1", Ordem = 11, NivelIndentacao = 1 },
                new() { Rotulo = "    Fornecedores", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.1.01", Ordem = 12, NivelIndentacao = 2 },
                new() { Rotulo = "    Obrigações Tributárias", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.1.02", Ordem = 13, NivelIndentacao = 2 },
                new() { Rotulo = "    Obrigações Trabalhistas", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.1.03", Ordem = 14, NivelIndentacao = 2 },
                new() { Rotulo = "  Passivo Não Circulante", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.2", Ordem = 15, NivelIndentacao = 1 },
                new() { Rotulo = "  Patrimônio Líquido", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.3", Ordem = 16, NivelIndentacao = 1 },
                new() { Rotulo = "    Capital Social", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.3.01", Ordem = 17, NivelIndentacao = 2 },
                new() { Rotulo = "    Reservas de Lucros", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.3.02", Ordem = 18, NivelIndentacao = 2 },
                new() { Rotulo = "    Lucros/Prejuízos Acumulados", TipoLinha = TipoLinha.ContaEspecifica, CodigoConta = "2.3.03", Ordem = 19, NivelIndentacao = 2 },
            }
        };
    }

    private static TemplateDemonstrativo CriarTemplateBalancetePadrao()
    {
        return new TemplateDemonstrativo
        {
            Id = Guid.Parse("d2000000-0000-0000-0000-000000000001"),
            Nome = "Balancete de Verificação Padrão",
            Descricao = "Template padrão para Balancete de Verificação",
            Tipo = TipoDemonstrativo.Balancete,
            Padrao = true,
            EmpresaId = null,
            Linhas = new List<TemplateLinha>() // Balancete usa geração automática (sem template)
        };
    }

    private static ContaContabil Conta(
        Guid planoId, string codigo, string nome,
        NaturezaConta natureza, SubtipoConta subtipo, TipoConta tipo,
        Guid? paiId, int ordem, bool aceitaLancamento = false, string? formula = null)
    {
        return new ContaContabil
        {
            Codigo = codigo,
            Nome = nome,
            Natureza = natureza,
            Subtipo = subtipo,
            Tipo = tipo,
            AceitaLancamento = aceitaLancamento,
            ExpressaoCalculo = formula,
            Nivel = codigo.Split('.').Length,
            Ordem = ordem,
            ContaPaiId = paiId,
            PlanoContasId = planoId
        };
    }
}
