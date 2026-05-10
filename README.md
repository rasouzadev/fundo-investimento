# API Fundo de Investimento

Uma API para gestão de aportes e resgates de um fundo de investimentos fictício.

##  Tecnologias Utilizadas

* **.NET 10 (C#):** Framework principal da aplicação.
* **OpenAPI / Scalar:** Para documentação da API.

## Estrutura do Projeto

O projeto foi desenhado seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, dividindo as responsabilidades nos seguintes módulos:

* **FundoInvestimento.Api:** Camada de apresentação. Contém os endpoints REST (Controllers ou Minimal APIs), configurações de injeção de dependência (`Program.cs`, extensões) e middlewares.
* **FundoInvestimento.Application:** Orquestração dos casos de uso. Contém os serviços de aplicação, DTOs (Data Transfer Objects) para entrada/saída e interfaces que definem contratos (ex: interfaces de repositório).
* **FundoInvestimento.Domain:** O coração do sistema. Contém as Entidades, Enums e as regras de negócio puras.
* **FundoInvestimento.Infrastructure:** Implementação técnica. Contém os repositórios reais, conexão com o banco e scripts de criação.
* **FundoInvestimento.Libs:** Biblioteca de utilitários/pacotes NuGet compartilhados.
* **FundoInvestimento.Tests:** Projeto dedicado à garantia de qualidade, contendo os testes unitários das regras de negócio e validação dos casos de uso.