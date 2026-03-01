using ContabilidadeAnalitica.Application.Common;
using ContabilidadeAnalitica.Application.DTOs;
using ContabilidadeAnalitica.Domain.Entities;
using ContabilidadeAnalitica.Domain.Interfaces;

namespace ContabilidadeAnalitica.Application.UseCases.Empresas;

public class EmpresaUseCases
{
    private readonly IUnitOfWork _uow;

    public EmpresaUseCases(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ResultadoOperacao<EmpresaDto>> CriarAsync(CriarEmpresaDto dto, CancellationToken ct = default)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.RazaoSocial))
            erros.Add("Razão social é obrigatória.");

        if (!string.IsNullOrWhiteSpace(dto.CNPJ))
        {
            var existente = await _uow.Empresas.ObterPorCNPJAsync(dto.CNPJ, ct);
            if (existente is not null)
                return new ConflitoDominio($"Já existe uma empresa com o CNPJ '{dto.CNPJ}'.");
        }

        if (erros.Count > 0)
            return new ValidacaoInvalida(erros);

        if (dto.GrupoEmpresarialId.HasValue)
        {
            var grupo = await _uow.GruposEmpresariais.ObterPorIdAsync(dto.GrupoEmpresarialId.Value, ct);
            if (grupo is null)
                return new NaoEncontrado($"Grupo empresarial '{dto.GrupoEmpresarialId}' não encontrado.");
        }

        var empresa = new Empresa
        {
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            CNPJ = dto.CNPJ,
            CodigoExterno = dto.CodigoExterno,
            MoedaPadrao = dto.MoedaPadrao,
            GrupoEmpresarialId = dto.GrupoEmpresarialId
        };

        await _uow.Empresas.AdicionarAsync(empresa, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso<EmpresaDto>(MapToDto(empresa));
    }

    public async Task<ResultadoOperacao<EmpresaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var empresa = await _uow.Empresas.ObterPorIdAsync(id, ct);
        if (empresa is null)
            return new NaoEncontrado($"Empresa '{id}' não encontrada.");

        return new Sucesso<EmpresaDto>(MapToDto(empresa));
    }

    public async Task<ResultadoOperacao<List<EmpresaDto>>> ListarAsync(CancellationToken ct = default)
    {
        var empresas = await _uow.Empresas.ObterTodosAsync(ct);
        var dtos = empresas.Select(MapToDto).ToList();
        return new Sucesso<List<EmpresaDto>>(dtos);
    }

    public async Task<ResultadoComando> AtualizarAsync(Guid id, AtualizarEmpresaDto dto, CancellationToken ct = default)
    {
        var empresa = await _uow.Empresas.ObterPorIdAsync(id, ct);
        if (empresa is null)
            return new NaoEncontrado($"Empresa '{id}' não encontrada.");

        if (string.IsNullOrWhiteSpace(dto.RazaoSocial))
            return new ValidacaoInvalida("Razão social é obrigatória.");

        if (!string.IsNullOrWhiteSpace(dto.CNPJ) && dto.CNPJ != empresa.CNPJ)
        {
            var existente = await _uow.Empresas.ObterPorCNPJAsync(dto.CNPJ, ct);
            if (existente is not null)
                return new ConflitoDominio($"Já existe uma empresa com o CNPJ '{dto.CNPJ}'.");
        }

        empresa.RazaoSocial = dto.RazaoSocial;
        empresa.NomeFantasia = dto.NomeFantasia;
        empresa.CNPJ = dto.CNPJ;
        empresa.CodigoExterno = dto.CodigoExterno;
        empresa.MoedaPadrao = dto.MoedaPadrao;
        empresa.GrupoEmpresarialId = dto.GrupoEmpresarialId;
        empresa.AlteradoEm = DateTime.UtcNow;

        await _uow.Empresas.AtualizarAsync(empresa, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso();
    }

    public async Task<ResultadoComando> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var empresa = await _uow.Empresas.ObterPorIdAsync(id, ct);
        if (empresa is null)
            return new NaoEncontrado($"Empresa '{id}' não encontrada.");

        await _uow.Empresas.RemoverAsync(id, ct);
        await _uow.SalvarAsync(ct);

        return new Sucesso();
    }

    private static EmpresaDto MapToDto(Empresa e) => new(
        e.Id, e.RazaoSocial, e.NomeFantasia, e.CNPJ,
        e.CodigoExterno, e.MoedaPadrao, e.GrupoEmpresarialId, e.Ativo
    );
}
