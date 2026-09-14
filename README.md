# 🌿KaáDebug BFF API
### 📌 Visão geral
API RESTful desenvolvida em ASP.NET Core 9, atuando como BFF para um aplicativo mobile em um escopo de uma solução de monitoramento de inteligênte de plantas domésticas.

A KaáDebug API é um componente de um ecosistema desenvolvido em seis camadas:
- KaaDebug PlantCare API 
- KaaDebug Device Manager API
- KaaDebug Intelligence
- KaaDebug BFF API 
- Aplicativo Mobile
- Persistência de dados

Neste contexto, a KaáDebug BFF API, atua intermediando a relação do mobile e serviços do backend, gerenciado a segurança, o acesso aos dados e o processamento de operações.  

---
### 🏗️ Arquitetura e Padrões
Este projeto foi construído sob uma Arquitetura Monolítica em Camadas (Layered Architecture), com uma forte orientação ao padrão BFF (Backend for Frontend). Os contratos de saída (DTOs) foram desenhados sob medida para entregar payloads agregados e otimizados para as telas do aplicativo móvel, reduzindo o número de requisições e o processamento no client-side.

#### Padrões de Projeto (Design Patterns)
- Dependency Injection (DI): Desacoplamento de dependências via contêiner nativo do .NET (injeção via construtor com escopo por requisição - AddScoped).
- Repository Pattern: Repositórios específicos (rejeitando repositórios genéricos) para isolar o EF Core e permitir o uso intensivo de Eager Loading e filtros seguros via chave do usuário (prevenindo vulnerabilidades de IDOR).
- Result Pattern: Encapsulamento de retornos lógicos através da classe ServiceResult, evitando o overhead computacional do disparo de exceções (try/catch) para controle de fluxo e regras de negócio.
- DTO Pattern: Separação estrita de dados usando records imutáveis do C# para proteger o domínio e as chaves de banco de dados.

---
### 💻 Tecnologias Utilizadas
- Linguagem & Framework: C# / ASP.NET Core 9
- Banco de Dados: PostgreSQL
- ORM: Entity Framework Core
- Provider Db: Npgsql (com suporte nativo a snake_case e mapeamento direto de Enums)
- Segurança: Autenticação JWT (JSON Web Tokens) e Hashing de senhas com BCrypt.

----
### 📂 Estrutura do Projeto
A solução está logicamente dividida para garantir a separação de responsabilidades (SoC):
- Controllers/ (Apresentação): Roteamento HTTP, extração de claims JWT via ApiControllerBase e validação de payloads.
- Services/ (Business Logic): O coração do sistema. Orquestra regras complexas (ex: posse e vinculação de dispositivos), validações e integração com IA/Storage.
- Repositories/ (Acesso a Dados): Isolamento total de queries e mutações no banco de dados.
- Models/ (Domínio): Entidades mapeadas para o PostgreSQL, utilizando tipos avançados como JsonDocument para esquemas dinâmicos (JSONB).
- DTOs/ (Transferência): Contratos de comunicação de entrada e saída.

---
### 📡 Principais Endpoints
A API expõe rotas seguras (protegidas por token JWT) focadas na experiência do usuário móvel.
- POST /auth/login -  Autenticação de usuários, emissão de JWT e recuperação (OTP). 
- GET /dashboard -  Entrega um payload consolidado (plantas, alertas e perfil) para a tela inicial do App.
- GET, POST /plants - CRUD de plantas e orquestração de posse
- GET /vinculação com hardware ESP32.
- GET /plants/{id}/history - Histórico de telemetria processado (agrupamento e cálculo de médias) para aliviar o App.
- GET /devices/{code}/status - Checagem de disponibilidade de um dispositivo físico para associação.
- GET, PUTH /notificationsistórico de alertas gerados pelo sistema e marcação de leitura.

### ⚙️ Modelagem de Dados Estratégica
- Chaves Primárias (PKs): Utilização nativa de gen_random_uuid() para segurança na exposição de rotas, com exceção da tabela de séries temporais (sensor_readings), que utiliza um sequencial longo (BIGINT IDENTITY) focado em altíssima performance de inserção.
- Campos Dinâmicos (JSONB): Uso nativo do tipo JsonDocument para salvar dicas de cuidado (CareInfo) e retornos de IA (DiagnosisResult) no PostgreSQL, garantindo flexibilidade sem normalização excessiva.