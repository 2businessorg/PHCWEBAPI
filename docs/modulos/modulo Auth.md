# Auth API - Documentação

## Descrição Geral do Módulo
O módulo Auth gere autenticação de utilizadores da API. Nesta fase, o foco da documentação é apenas o endpoint de login para obtenção de token.

> **Nota para organização do Dicionário de Dados Interno e da documentação de frontend:**
> O `Dicionário de Dados Interno` deve priorizar o payload funcional específico do endpoint, isto é, os campos de negócio devolvidos em `item`, `items`, `data` e respetivas estruturas internas.
> Estruturas genéricas e repetidas entre endpoints, como `meta`, `links`, `code` e `message`, não devem poluir o dicionário interno do recurso principal quando não acrescentam detalhe funcional do domínio.
> Sempre que existir um `array` ou `object` dentro de `item`, `items` ou `data`, deve existir também uma secção própria para detalhar exatamente os campos internos dessa estrutura.

---

## Estrutura do Módulo

```text
Auth
└── Pasta Principal: Auth
    └── POST /auth/login
```

---

## Pasta Principal: Auth

### Endpoint: Login

#### POST /auth/login

**Descrição:**
Autentica um utilizador com `username` e `password`. Em caso de sucesso, devolve um token JWT e metadados de autenticação.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
Nenhum.

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `username` | `string` | Sim | Nome do utilizador | Não pode ser vazio |
| `password` | `string` | Sim | Palavra-passe do utilizador | Não pode ser vazio |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/auth/login' \
  --header 'Content-Type: application/json' \
  --data '{
    "username": "",
    "password": ""
  }'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK (login processado)**

Exemplo de Corpo da Resposta (credenciais válidas):

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2026-05-19T13:00:00Z",
  "allowed": true,
  "outputResponse": "AUTHENTICATED"
}
```

Exemplo de Corpo da Resposta (credenciais inválidas):

```json
{
  "token": "",
  "expiration": null,
  "allowed": false,
  "outputResponse": "BAD_CREDENTIALS"
}
```

**Status Code: 400 - Requisição Inválida**

Retornado quando o body é inválido (por exemplo, `username` ou `password` ausentes).

Exemplo de Corpo da Resposta:

```json
{
  "errors": [
    "User Name is required",
    "Password is required"
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `token` | `string` | JWT para autenticação nas chamadas seguintes. Vazio quando login falha |
| `expiration` | `string \| null` | Data/hora de expiração do token. `null` quando login falha |
| `allowed` | `boolean` | Indica se a autenticação foi aceite |
| `outputResponse` | `string` | Resultado textual da autenticação (`AUTHENTICATED`, `BAD_CREDENTIALS`, `INTERNAL_ERROR`) |

---

## Códigos de Resultado do Login

| Valor | Descrição | Quando ocorre |
| :---- | :-------- | :------------ |
| `AUTHENTICATED` | Utilizador autenticado | Credenciais válidas |
| `BAD_CREDENTIALS` | Credenciais inválidas | Username/password inválidos |
| `INTERNAL_ERROR` | Erro interno | Exceção inesperada no processo de login |
