# API Fundo de Investimento

Uma API para gestão de aportes e resgates de um fundo de investimentos fictício, focada em alta performance e consistência transacional.

## Tecnologias Utilizadas

* **.NET 10 (C#):** Framework principal utilizando as últimas funcionalidades da linguagem.
* **PostgreSQL:** Banco de dados relacional para garantia de transações ACID.
* **Dapper:** Micro-ORM para acesso a dados de alta performance e controle total sobre as queries.
* **OpenAPI / Scalar:** Para documentação da API.

## Estrutura do Projeto

O projeto foi desenhado seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**:

* **FundoInvestimento.Api:** Camada de apresentação. Contém os endpoints REST, configurações de injeção de dependência e middlewares.
* **FundoInvestimento.Application:** Orquestração dos casos de uso. Contém os serviços de aplicação, DTOs e interfaces de abstração (ex: `IDbConnectionFactory`).
* **FundoInvestimento.Domain:** O coração do sistema. Contém as Entidades ricas, Enums e as regras de negócio fundamentais sem dependências externas.
* **FundoInvestimento.Infrastructure:** Implementação técnica. Contém os repositórios (Dapper), conexão com o banco e lógica de inicialização de dados.
* **FundoInvestimento.Libs:** Biblioteca de utilitários e pacotes compartilhados entre as camadas.
* **FundoInvestimento.Tests:** Garantia de qualidade com testes unitários focados nas regras de negócio e validação de cenários.

---

## Configuração e Segurança

### 1. Gestão de Segredos
Para garantir a segurança, a connection string do banco de dados não é armazenada no `appsettings.json`.

* **Desenvolvimento (Local):** Utilize o **.NET User Secrets**.
    ```bash
    dotnet user-secrets init
    dotnet user-secrets set "ConnectionStrings:Supabase" "Sua_Connection_String_Aqui"
    ```
* **Produção:** A aplicação lê a variável de ambiente `ConnectionStrings__Supabase`.

### 2. Inicialização do Banco de Dados
A aplicação possui um serviço de `DatabaseInitializer` que executa os scripts DDL automaticamente ao iniciar, porém, por segurança, esta funcionalidade está restrita ao ambiente de **Desenvolvimento (`Development`)**.

---

## Modelagem do Banco de Dados
		
### 1. Clientes (`cliente`)
| Campo | Descrição |
|---|---|
| `id` | Identificador único (UUIDv7) |
| `nome` | Nome completo do cliente |
| `cpf` | CPF único (sem formatação) |
| `saldo_disponivel` | Saldo líquido para novos investimentos |

### 2. Catálogo de Fundos (`fundo`)
| Campo | Descrição |
|---|---|
| `id` | Identificador único (UUIDv7) |
| `nome` | Nome do fundo de investimento |
| `horario_corte` | Horário limite (cut-off) para ordens no mesmo dia (D+0) |
| `valor_cota` | Valor unitário atual da cota (alta precisão) |
| `valor_minimo_aporte` | Valor mínimo para entrada no fundo |
| `valor_minimo_permanencia` | Valor residual mínimo exigido após resgates |
| `status_captacao` | Indica se o fundo está `ABERTO` ou `FECHADO` |

### 3. Posição do Cliente (`posicao_cliente`)
| Campo | Descrição |
|---|---|
| `id_cliente` | FK para tabela de clientes |
| `id_fundo` | FK para tabela de fundos |
| `quantidade_cotas` | Saldo atual de cotas do cliente no fundo específico |

### 4. Ordens e Agendamentos (`ordem`)
| Campo | Descrição |
|---|---|
| `id` | Identificador único (UUIDv7) |
| `id_cliente` | FK do solicitante |
| `id_fundo` | FK do fundo de destino |
| `tipo_operacao` | Tipo: `APORTE` ou `RESGATE` |
| `quantidade_cotas` | Volume da transação em cotas |
| `data_agendamento` | Data programada (NULL para ordens imediatas) |
| `status` | `PENDENTE`, `CONCLUIDO` ou `REJEITADO` |
| `criado_em` | Timestamp da criação da solicitação |


### Diagrama Entidade-Relacionamento (MER/DER)

```mermaid
erDiagram
    CLIENTE ||--o{ POSICAO_CLIENTE : "possui"
    CLIENTE ||--o{ ORDEM : "solicita"
    FUNDO ||--o{ POSICAO_CLIENTE : "compoe"
    FUNDO ||--o{ ORDEM : "recebe"

    CLIENTE {
        uuid id PK
        varchar nome
        varchar cpf UK
        numeric saldo_disponivel
    }
    
    FUNDO {
        uuid id PK
        varchar nome
        time horario_corte
        numeric valor_cota
        numeric valor_minimo_aporte
        numeric valor_minimo_permanencia
        varchar status_captacao
    }
    
    POSICAO_CLIENTE {
        uuid id_cliente PK, FK
        uuid id_fundo PK, FK
        integer quantidade_cotas
    }
    
    ORDEM {
        uuid id PK
        uuid id_cliente FK
        uuid id_fundo FK
        varchar tipo_operacao
        integer quantidade_cotas
        date data_agendamento
        varchar status
        timestamptz criado_em
    }
````