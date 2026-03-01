using ContabilidadeAnalitica.Application.Common;
using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Enums;
using ContabilidadeAnalitica.Domain.Interfaces;

namespace ContabilidadeAnalitica.Application.UseCases.Templates;

public class TemplateUseCases
{
    private readonly IUnitOfWork _uow;

    public TemplateUseCases(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ResultadoOperacao<TemplateDemonstrativoDto>> CriarAsync(CriarTemplateDemonstrativoDto dto, CancellationToken ct = default)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Nome))
            erros.Add("Nome do template é obrigatório.");
        if (dto.Linhas == null || dto.Linhas.Count == 0)
            erros.Add("O template deve possuir ao menos uma linha.");

        if (erros.Count > 0)
            return new ValidacaoInvalida(erros);

        if (dto.EmpresaId.HasValue)
        {
            var empresa = await _uow.Empresas.ObterPorIdAsync(dto.EmpresaId.Value, ct);
            if (empresa is null)
                return new NaoEncontrado($"Empresa '{dto.EmpresaId}' não encontrada.");
        }

        var template = new TemplateDemonstrativo
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Tipo = dto.Tipo,
            Padrao = dto.Padrao,
            EmpresaId = dto.EmpresaId,
            Linhas = dto.Linhas.Select(l => new TemplateLinha
            {
                Rotulo = l.Rotulo,
                TipoLinha = l.TipoLinha,
                CodigoConta = l.CodigoConta,
                NaturezaFiltro = l.NaturezaFiltro,
                SubtipoFiltro = l.SubtipoFiltro,
                Expressao = l.Expressao,
                LinhasReferenciadas = l.LinhasReferenciadas,
                Ordem = l.Ordem,
                NivelIndentacao = l.NivelIndentacao,
                Negrito = l.Negrito,
                InverterSinal = l.InverterSinal
            }).ToList()
        };

        await _uow.TemplatesDemonstrativos.AdicionarAsync(template, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso<TemplateDemonstrativoDto>(MapToDto(template));
    }

    public async Task<ResultadoOperacao<TemplateDemonstrativoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _uow.TemplatesDemonstrativos.ObterComLinhasAsync(id, ct);
        if (template is null)
            return new NaoEncontrado($"Template '{id}' não encontrado.");

        return new Sucesso<TemplateDemonstrativoDto>(MapToDto(template));
    }

    public async Task<ResultadoOperacao<List<TemplateDemonstrativoDto>>> ListarPorTipoAsync(TipoDemonstrativo tipo, CancellationToken ct = default)
    {
        var templates = await _uow.TemplatesDemonstrativos.ObterPorTipoAsync(tipo, ct);
        var dtos = templates.Select(MapToDto).ToList();
        return new Sucesso<List<TemplateDemonstrativoDto>>(dtos);
    }

    private static TemplateDemonstrativoDto MapToDto(TemplateDemonstrativo t) => new(
        t.Id, t.Nome, t.Descricao, t.Tipo, t.Padrao, t.EmpresaId,
        t.Linhas?.OrderBy(l => l.Ordem).Select(l => new TemplateLinhaDto(
            l.Id, l.Rotulo, l.TipoLinha, l.CodigoConta,
            l.NaturezaFiltro, l.SubtipoFiltro, l.Expressao,
            l.LinhasReferenciadas, l.Ordem, l.NivelIndentacao,
            l.Negrito, l.InverterSinal
        )).ToList() ?? new List<TemplateLinhaDto>()
    );
}
