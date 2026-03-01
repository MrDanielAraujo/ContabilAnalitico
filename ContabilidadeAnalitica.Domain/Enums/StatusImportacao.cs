namespace ContabilidadeAnalitica.Domain.Enums;

public enum StatusImportacao
{
    Pendente = 1,
    Processando = 2,
    Concluida = 3,
    ConcluidaComErros = 4,
    Falha = 5
}
