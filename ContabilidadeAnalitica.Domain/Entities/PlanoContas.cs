using ContabilidadeAnalitica.Domain.Common;

namespace ContabilidadeAnalitica.Domain.Entities;

/// <summary>
/// Representa uma versão do plano de contas de uma empresa.
/// Versionado por vigência temporal para preservar histórico.
/// </summary>
public class PlanoContas : EntidadeBase, IAuditavel
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Versao { get; set; } = 1;
    public DateTime VigenciaInicio { get; set; }
    public DateTime? VigenciaFim { get; set; }
    public bool Vigente { get; set; } = true;

    // Relacionamentos
    public Guid EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }

    public ICollection<ContaContabil> Contas { get; set; } = new List<ContaContabil>();

    /// <summary>
    /// Verifica se o plano está vigente em uma data específica.
    /// </summary>
    public bool EstaVigenteEm(DateTime data)
    {
        return data >= VigenciaInicio && (VigenciaFim == null || data <= VigenciaFim);
    }

    /// <summary>
    /// Encerra a vigência do plano de contas.
    /// </summary>
    public void EncerrarVigencia(DateTime dataFim)
    {
        VigenciaFim = dataFim;
        Vigente = false;
    }
}
