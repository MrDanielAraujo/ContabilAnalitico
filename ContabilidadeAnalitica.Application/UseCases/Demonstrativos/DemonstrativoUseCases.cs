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

        var linhas = resultado.Linhas.Select(l => new DemonstrativoLinhaDto(
            l.Rotulo,
            l.CodigoConta,
            l.TipoLinha,
            l.NivelIndentacao,
            l.Negrito,
            l.ValoresPorAno
        )).ToList();

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
}
