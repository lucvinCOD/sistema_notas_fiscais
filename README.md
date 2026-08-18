# Sistema de Notas Fiscais

Sistema desenvolvido como projeto prático utilizando Angular no frontend e ASP.NET Core com C# no backend.

A aplicação foi estruturada com dois microsserviços independentes:

- Estoque.Api
- Faturamento.Api

O objetivo é permitir o cadastro e gerenciamento de produtos, emissão de notas fiscais, validação de estoque e baixa automática das quantidades vendidas.

---

## Arquitetura

A aplicação está dividida em três partes principais:

```text
Angular
  |
  | HTTP
  v
Faturamento.Api
  |
  | HTTP
  v
Estoque.Api

Cada microsserviço possui responsabilidade própria e banco de dados independente.

sistema_notas_fiscais/
|
|-- services/
|   |
|   |-- Estoque.Api/
|   |   |-- Data/
|   |   |-- Migrations/
|   |   |-- Models/
|   |   |-- Program.cs
|   |   `-- estoque.db
|   |
|   `-- Faturamento.Api/
|       |-- Data/
|       |-- Migrations/
|       |-- Models/
|       |-- Program.cs
|       `-- faturamento.db
|
|-- frontend/
|   |
|   `-- src/
|       `-- app/
|           |-- models/
|           |-- pages/
|           |-- services/
|           |-- app.config.ts
|           |-- app.routes.ts
|           `-- app.ts
|
|-- .gitignore
|-- LICENSE
`-- README.md

Os arquivos .db são bancos SQLite locais e não são versionados no GitHub. A estrutura dos bancos pode ser recriada através das migrations do Entity Framework Core.

## Tecnologias
### Backend
C#
.NET 10
ASP.NET Core
Entity Framework Core
SQLite
REST API
HttpClient

### Frontend
Angular
TypeScript
HTML
CSS
Angular Router
Angular HttpClient
RxJS

### Ferramentas
Git
GitHub
Visual Studio Code
Node.js
npm
Angular CLI
.NET CLI

## Microsserviço de Estoque
O Estoque.Api é responsável pelo gerenciamento dos produtos disponíveis no sistema.
Cada produto possui:

- ID
- Nome
- Quantidade
- Preço

### Endpoints
#### Listar produtos:
GET /produtos

#### Buscar produto por ID:
GET /produtos/{id}

#### Criar produto:
POST /produtos
Exemplo:
{
  "nome": "Notebook",
  "quantidade": 10,
  "preco": 3500
}

#### Atualizar produto:
PUT /produtos/{id}

#### Excluir produto:
DELETE /produtos{id}

#### Baixar estoque:
POST /produtos/{id}/baixar?quantidade=1
Esse endpoint é utilizado pelo microsserviço de Faturamento após a validação da disponibilidade do produto.

## Microsserviço de Faturamento
O Faturamento.Api é responsável pela emissão e consulta das notas fiscais.
Uma nota fiscal possui:

- ID
- Data de emissão
- Status
- Valor total
- Lista de itens

Cada item possui:

-Produto
-Quantidade
-Preço unitário
-Subtotal

### Endpoints
#### Emitir nota:
POST /notas
{
  "itens": [
    {
      "produtoId": 1,
      "quantidade": 2
    }
  ]
}
O cliente não precisa informar o nome nem o preço do produto.
O Faturamento.Api consulta o Estoque.Api e utiliza os dados oficiais do produto

#### Listas notas
GET /notas

#### Buscar nota por ID
GET /notas/{id}

### Fluxo de emissão de nota
Usuário
  |
  v
Angular
  |
  | POST /notas
  v
Faturamento.Api
  |
  | GET /produtos/{id}
  v
Estoque.Api
  |
  v
Validação de estoque
  |
  | estoque disponível
  v
Cálculo dos subtotais
  |
  v
Cálculo do valor total
  |
  | POST /produtos/{id}/baixar
  v
Baixa do estoque
  |
  v
Nota salva no faturamento.db

### Tratamento de falhas

O sistema possui algumas validações para evitar inconsistências durante a emissão das notas.
Antes de alterar o estoque, todos os itens são validados, isso evita um cenário como:

Produto A válido
Produto A baixado

Produto B inválido
Operação interrompida

A aplicação primeiro valida todos os produtos e somente depois inicia as baixas.

Também são tratados alguns cenários HTTP.

### Estoque insuficiente
A emissão é recusada.
400 Bad Request

### Produto inexistente
A requisição é interrompida.
404 Not Found

### Serviço de Estoque indisponível
Caso o Estoque.Api esteja fora do ar:
503 Service Unavailable

Dessa forma, o Faturamento consegue identificar quando uma dependência externa está indisponível.

## Persistência
Cada microsserviço possui seu próprio banco SQLite.

Estoque.Api
    |
    `-- estoque.db


Faturamento.Api
    |
    `-- faturamento.db

O Entity Framework Core é utilizado como ORM para mapear as classes C# para estruturas relacionais no banco de dados.
As alterações estruturais dos bancos são controladas através de migrations.

## Como executar o projeto
Pré-requisitos
É necessário possuir:

- .NET 10 SDK
- Node.js
- npm
- Angular CLI

1. Executar Estoque.Api
Abra um terminal:
    cd services/Estoque.Api
Caso o banco ainda não exista:
    dotnet ef database update
Depois:
    dotnet run
A API será executada em:
    http://localhost:5266

2. Executar Faturamento.Api
Abra outro terminal:
    cd services/Faturamento.Api
Caso o banco ainda não exista:
    dotnet ef database update
Depois:
    dotnet run

A API será executada em:
    http://localhost:5111

3. Executar o frontend Angular
Abra outro terminal:
    cd frontend
Instale as dependências:
    npm install
No Windows PowerShell, caso exista restrição de execução de scripts:
    npm.cmd install
Depois execute:
    ng serve
ou:
    ng.cmd serve
A aplicação estará disponível em:
    http://localhost:4200

## CORS
As APIs permitem requisições originadas do frontend Angular:
    http://localhost:4200
Isso permite a comunicação entre o navegador e os dois microsserviços durante o ambiente de desenvolvimento.

## Funcionalidades
Atualmente o sistema permite:
- cadastrar produtos;
- listar produtos;
- editar produtos;
- excluir produtos;
- controlar quantidades em estoque;
- criar notas fiscais;
- adicionar múltiplos itens a uma nota;
- consultar dados oficiais dos produtos no Estoque.Api;
- validar disponibilidade de estoque;
- calcular subtotal dos itens;
- calcular valor total da nota;
- baixar estoque automaticamente após emissão;
- listar notas emitidas;
- consultar itens das notas;
- persistir dados em SQLite;
- tratar indisponibilidade entre microsserviços.

## Decisões técnicas

### Microsserviços
Estoque e Faturamento foram separados para manter responsabilidades independentes.
O Faturamento.Api não acessa diretamente o banco do Estoque.
A comunicação acontece através da API HTTP do Estoque.Api.

### SQLite
SQLite foi escolhido por ser um banco relacional leve e adequado ao escopo do projeto, permitindo persistência real sem a necessidade de configurar um servidor de banco separado.

### Entity Framework Core
Foi utilizado para realizar o mapeamento objeto-relacional entre as classes C# e o SQLite, além do gerenciamento da estrutura do banco através de migrations.

### Angular
O Angular foi utilizado como frontend para fornecer uma interface única de gerenciamento do Estoque e emissão de notas fiscais.

Autor
Desenvolvido por Lucas Ângelo Rodrigues Viana.