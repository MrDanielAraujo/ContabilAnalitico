# Sistema Contábil Analítico - Backend em C# / .NET 8

Este projeto é um backend corporativo em C# utilizando .NET 8 Web API para um sistema contábil analítico de nível institucional. Ele é projetado para o cálculo de demonstrativos financeiros (Balanço e Balancete) e segue princípios de engenharia de software para ambientes financeiros críticos.

## Arquitetura

O sistema implementa **Clean Architecture** combinada com **Domain-Driven Design (DDD)**, resultando na seguinte estrutura de projetos:

- **ContabilidadeAnalitica.Domain**: Contém as entidades, value objects, enums e regras de negócio centrais. É o coração do sistema e não depende de nenhuma outra camada.
- **ContabilidadeAnalitica.Application**: Orquestra os casos de uso (Use Cases), contém os DTOs (Data Transfer Objects) e a lógica de aplicação. Depende do Domain.
- **ContabilidadeAnalitica.CalculationEngine**: Um motor de cálculo contábil isolado, responsável pela agregação hierárquica e avaliação de fórmulas. Depende do Domain.
- **ContabilidadeAnalitica.Infrastructure**: Implementa a persistência de dados com Entity Framework Core, repositórios, e o padrão Unit of Work. Depende do Application e do Domain.
- **ContabilidadeAnalitica.API**: A camada de exposição via Web API (REST), contendo os Controllers. Depende do Application, Infrastructure e CalculationEngine.

## Principais Funcionalidades

- **Gerenciamento de Plano de Contas**: Suporte a múltiplos planos de contas por empresa, com versionamento temporal.
- **Hierarquia Contábil**: Contas sintéticas (agregadoras) e analíticas (lançamento) com níveis ilimitados.
- **Motor de Cálculo**: Agregação hierárquica automática (contas sintéticas somam as filhas) e avaliação de fórmulas entre contas (ex: `[1.1.01] + [1.1.02]`).
- **Demonstrativos Financeiros**: Geração de Balanço Patrimonial e Balancete de Verificação com base em anos dinâmicos.
- **Templates Configuráveis**: Permite a criação de templates para customizar a estrutura dos demonstrativos.
- **Importação de Dados**: Endpoint para importação de saldos contábeis via JSON, simulando integração com sistemas externos.
- **Seed de Dados**: O sistema é inicializado com um plano de contas completo e dados de teste para três anos (2024, 2025, 2026), permitindo o uso imediato da API.

## Como Executar o Projeto

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Passos para Execução

1. **Clone ou extraia o projeto** para um diretório local.

2. **Navegue até a pasta da API**:
   ```sh
   cd /caminho/para/o/projeto/ContabilidadeAnalitica/ContabilidadeAnalitica.API
   ```

3. **Execute o projeto**:
   ```sh
   dotnet run
   ```

4. **Acesse a API**:
   - A API estará disponível em `http://localhost:5000`.
   - A documentação interativa do Swagger UI estará disponível na raiz: `http://localhost:5000`.

O sistema utiliza um banco de dados **SQLite em memória** que é automaticamente criado e populado com dados de exemplo (seed) na inicialização. A conexão é mantida viva durante a execução da aplicação para que os dados persistam.

## Como Testar a API

A documentação do Swagger (`http://localhost:5000`) é a forma mais fácil de explorar e testar os endpoints.

Alternativamente, você pode usar `curl` ou outra ferramenta de sua preferência.

### Exemplo: Gerar Balanço Patrimonial

Este comando gera o Balanço Patrimonial para a empresa de exemplo, comparando os anos de 2024, 2025 e 2026.

```sh
curl -X POST "http://localhost:5000/api/demonstrativos" \
-H "Content-Type: application/json" \
-d 
{
  "empresaId": "b1000000-0000-0000-0000-000000000001",
  "tipo": "Balanco",
  "anos": [2024, 2025, 2026]
}
```

### Exemplo: Gerar Balancete de Verificação

```sh
curl -X GET "http://localhost:5000/api/demonstrativos/balancete/b1000000-0000-0000-0000-000000000001?anos=2024,2025,2026"
```

### Exemplo: Importar Saldos

Este comando simula a importação de saldos, incluindo um item com código de conta inválido para demonstrar o tratamento de erros.

```sh
curl -X POST "http://localhost:5000/api/importacoes" \
-H "Content-Type: application/json" \
-d 
{
  "empresaId": "b1000000-0000-0000-0000-000000000001",
  "descricao": "Importação de teste",
  "itens": [
    { "codigoConta": "1.1.01.001", "ano": 2026, "mes": 1, "valor": 5000 },
    { "codigoConta": "9.9.99", "ano": 2026, "mes": 1, "valor": 100 } 
  ]
}
```
