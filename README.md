
# localizeBackendAPI

API REST para cadastro, consulta e gerenciamento de empresas e usuários, com autenticação JWT, integração com ReceitaWS e boas práticas de segurança.

---

## Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT (JSON Web Token) para autenticação

---

## Funcionalidades

- Cadastro, autenticação e gerenciamento de usuários
- Cadastro, consulta, edição e inativação de empresas
- Consulta de dados de empresas via ReceitaWS (CNPJ)
- Proteção dos endpoints via autenticação JWT
- Tratamento de erros padronizado
- CORS configurado para integração com frontend

---

## Estrutura dos Principais Endpoints

### Usuários

- `POST /api/Usuarios/register` — Cadastro de usuário (retorna JWT no corpo da resposta)
- `POST /api/Usuarios/login` — Autenticação e geração de JWT (retorna JWT no corpo da resposta)
- `GET /api/Usuarios/perfil` — Consulta de perfil do usuário autenticado
- `PUT /api/Usuarios/perfil` — Atualização de perfil e senha
- `GET /api/Usuarios` — Listagem de usuários (autenticado)
- `DELETE /api/Usuarios/{id}` — Exclusão de usuário

### Empresas

- `POST /api/Empresas` — Cadastro de empresa
- `GET /api/Empresas` — Listagem de empresas (autenticado)
- `GET /api/Empresas/{id}` — Consulta de empresa por ID
- `GET /api/Empresas/minhas` — Listagem de empresas do usuário autenticado
- `PUT /api/Empresas/{id}` — Atualização de empresa
- `PUT /api/Empresas/InactiveEmpresas?id={id}` — Inativação de empresa
- `DELETE /api/Empresas/{id}` — Exclusão de empresa
- `GET /api/Empresas/consultar-cnpj/{cnpj}` — Consulta de dados de empresa via ReceitaWS

---

## Segurança

- Autenticação JWT protegendo todos os endpoints sensíveis
- Senhas armazenadas como hash SHA256
- CORS restrito ao domínio do frontend (`http://localhost:3000`)
- Tratamento de erros centralizado e padronizado
---

## Como Executar

### 1. Pré-requisitos

- .NET 8 SDK
- SQL Server (local ou remoto)
- Visual Studio 2022

### 2. Configuração do Banco

- Configure a string de conexão em `appsettings.json` ou via variáveis de ambiente.
- O EF Core cria as tabelas automaticamente via migrations.

### 3. Configuração do JWT

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LocalizeBackend;Trusted_Connection=True;TrustServerCertificate=True"
},
"Jwt": {
  "Key": "sua-chave-secreta",
  "Issuer": "localizeBackendAPI",
  "Audience": "localizeFrontend"
}
```

### 4. Executando o Projeto

- Via terminal:
  ```bash
  dotnet run
  ```
- Ou via Visual Studio (F5)

---

## Integração com Frontend

- CORS está configurado para permitir integração com `http://localhost:3000`
- O frontend deve armazenar o JWT retornado nos endpoints de login/registro e enviá-lo no header:

---

## Observações Importantes

- **Nunca exponha sua chave JWT em repositórios públicos**
- **Sempre utilize HTTPS em produção**
- **Considere usar algoritmos de hash mais robustos para senhas (ex: bcrypt)**
- **Valide o formato do CNPJ antes de consultar a ReceitaWS**
- O backend não salva o JWT em cookies, apenas retorna no corpo da resposta. O armazenamento é responsabilidade do frontend.

---

## Desenvolvido por

João Victor dos Santos Costa

---

## DDL

<details>
<summary><strong>Usuários</strong></summary>

```sql
CREATE TABLE localizeBackend.dbo.Usuarios (
  Id uniqueidentifier DEFAULT newid() NOT NULL,
  Nome varchar(100) COLLATE Latin1_General_CI_AS NOT NULL,
  Email varchar(100) COLLATE Latin1_General_CI_AS NOT NULL,
  SenhaHash varchar(255) COLLATE Latin1_General_CI_AS NOT NULL,
  Ativo bit DEFAULT 1 NOT NULL,
  CONSTRAINT PK__Usuarios__3214EC07FC7A5FDB PRIMARY KEY (Id),
  CONSTRAINT UQ__Usuarios__A9D10534737EE24F UNIQUE (Email)
);
```
</details>

<details>
<summary><strong>Empresas</strong></summary>

```sql
CREATE TABLE localizeBackend.dbo.Empresas (
  Id uniqueidentifier DEFAULT newid() NOT NULL,
  NomeEmpresarial varchar(255) COLLATE Latin1_General_CI_AS NULL,
  NomeFantasia varchar(255) COLLATE Latin1_General_CI_AS NULL,
  CNPJ varchar(18) COLLATE Latin1_General_CI_AS NOT NULL,
  Situacao varchar(100) COLLATE Latin1_General_CI_AS NULL,
  Abertura varchar(20) COLLATE Latin1_General_CI_AS NULL,
  Tipo varchar(100) COLLATE Latin1_General_CI_AS NULL,
  NaturezaJuridica varchar(255) COLLATE Latin1_General_CI_AS NULL,
  AtividadePrincipal varchar(255) COLLATE Latin1_General_CI_AS NULL,
  Logradouro varchar(255) COLLATE Latin1_General_CI_AS NULL,
  Numero varchar(20) COLLATE Latin1_General_CI_AS NULL,
  Complemento varchar(255) COLLATE Latin1_General_CI_AS NULL,
  Bairro varchar(100) COLLATE Latin1_General_CI_AS NULL,
  Municipio varchar(100) COLLATE Latin1_General_CI_AS NULL,
  UF char(2) COLLATE Latin1_General_CI_AS NULL,
  CEP varchar(15) COLLATE Latin1_General_CI_AS NULL,
  UsuarioId uniqueidentifier NOT NULL,
  Ativo bit DEFAULT 1 NOT NULL,
  CONSTRAINT PK__Empresas__3214EC07900C4E86 PRIMARY KEY (Id),
  CONSTRAINT FK__Empresas__Usuari__3F466844 FOREIGN KEY (UsuarioId)
    REFERENCES localizeBackend.dbo.Usuarios(Id) ON DELETE CASCADE
);

CREATE UNIQUE NONCLUSTERED INDEX UQ_Empresas_Cnpj_UsuarioId
ON localizeBackend.dbo.Empresas (CNPJ ASC, UsuarioId ASC);
```
</details>
