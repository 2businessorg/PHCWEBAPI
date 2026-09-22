# [Clientes] API - Documentação

## Descrição Geral do Módulo
Endpoints para criação, consulta, atualização e remoção de clientes. Suporta operações individuais e em lote. Os clientes são identificados pela combinação de `id` e `branch`, permitindo gerenciar múltiplos estabelecimentos da mesma entidade.

> **Nota para organização do Dicionário de Dados Interno e da documentação de frontend:**
> O `Dicionário de Dados Interno` deve priorizar o payload funcional específico do endpoint, isto é, os campos de negócio devolvidos em `item`, `items`, `data` e respetivas estruturas internas.
> Estruturas genéricas e repetidas entre endpoints, como `meta`, `links`, `code` e `message`, não devem poluir o dicionário interno do recurso principal quando não acrescentam detalhe funcional do domínio.
> Sempre que existir um `array` ou `object` dentro de `item`, `items` ou `data`, deve existir também uma secção própria para detalhar exatamente os campos internos dessa estrutura, para que a equipa de frontend consiga ver não só a key principal mas também o dicionário de dados de cada item/subitem.
> Na documentação de frontend, tanto a secção de `Response` como a secção de payload de entrada devem manter blocos colapsáveis.
> Dentro desses colapsáveis deve ficar toda a informação estrutural: campos principais e também o detalhe interno de `array` e `object`, para que a AI de frontend apresente a key e o respetivo conteúdo interno no mesmo agrupamento visual.

---

## Estrutura do Módulo

O módulo Clientes está organizado em operações principais:

**Padrão de organização no documento:**
- Use `## Subpasta: [Nome]` para cada grupo funcional apresentado na árvore
- Se existirem endpoints que ficam na raiz do módulo e não dentro de uma subpasta temática, use `## Pasta Principal: [Nome do Módulo]`
- Os endpoints devem ser documentados dentro da secção correspondente da subpasta ou da pasta principal, e não todos misturados numa única secção geral

**Com operações simples (sem subpastas):**
```
Clientes
├── GET /clients
├── GET /clients/{id}/{branch}
├── POST /clients
├── PATCH /clients/{id}/{branch}
├── DELETE /clients/{id}/{branch} (Em Desenvolvimento)
└── POST /clients/bulk (Em Desenvolvimento)
```

**Nota importante sobre endpoints em desenvolvimento:**
- Endpoints **Em Desenvolvimento** devem estar claramente marcados neste ficheiro com um aviso `Em Desenvolvimento`
- Estes endpoints devem continuar a aparecer neste ficheiro `.md` para referência interna e alinhamento da equipa
- Estes endpoints NÃO devem aparecer na documentação da API na página web

---

## Pasta Principal: Clientes

### Endpoint: Listar Clientes

#### GET /clients
Retorna uma lista paginada de todos os clientes registados no sistema.

> **Nota:** Para obter um único cliente, use `GET /clients/{id}/{branch}` onde ambos os parâmetros são **obrigatórios**.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `page` | integer | Não | Número da página. | Default: `1`. |
| `pageSize` | integer | Não | Quantidade de itens por página. | Default: `20`. |
| `id` | integer | Não | Filtra clientes por ID (`no`). Devolve apenas os clientes com esse número de cliente. |  |
| `branch` | integer | Não | Filtra clientes por estabelecimento (`estab`). |  |
| `name` | string | Não | Filtra clientes por nome (pesquisa parcial). |  |
| `nuit` | string | Não | Filtra clientes por NUIT (correspondência exata). |  |
| `phone` | string | Não | Filtra clientes por telefone (pesquisa parcial). |  |
| `address` | string | Não | Filtra clientes por endereço (pesquisa parcial). |  |

##### Body Parameters
Nenhum.

#### Exemplo cURL
```bash
curl 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/clients?page=1&pageSize=20' \
  --request GET \
  --header 'Authorization: Bearer YOUR_TOKEN'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "items": [
    {
      "id": 1,
      "branch": 0,
      "name": "Cliente Genérico",
      "nuit": "---------------",
      "phone": "",
      "address": "__",
      "email": "",
      "inactive": false,
      "addFields": null
    },
    {
      "id": 4,
      "branch": 0,
      "name": "Teste123",
      "nuit": "123456789",
      "phone": "",
      "address": "Teste",
      "email": "",
      "inactive": false,
      "addFields": null
    }
  ],
  "meta": {
    "totalItems": 150,
    "itemCount": 2,
    "pageSize": 20,
    "totalPages": 8,
    "currentPage": 1
  },
  "links": [
    {
      "rel": "self",
      "method": "GET",
      "href": "/api/clients?page=1&pageSize=20"
    },
    {
      "rel": "next",
      "method": "GET",
      "href": "/api/clients?page=2&pageSize=20"
    }
  ]
}
```

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | array | Lista paginada de clientes. |

**Dicionário interno de cada item em `items`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `id` | integer | Identificador único do cliente. |
| `branch` | integer | Estabelecimento do cliente. |
| `name` | string | Nome do cliente. |
| `nuit` | string | NUIT do cliente. |
| `phone` | string | Número de telefone. |
| `address` | string | Endereço/morada. |
| `email` | string | Correio electrónico. |
| `inactive` | boolean | Status ativo/inativo. |
| `addFields` | object | Campos adicionais customizados. |

---

### Endpoint: Obter Cliente

#### GET /clients/{id}/{branch}
Retorna um único cliente identificado pela combinação **obrigatória** de `id` + `branch`.

#### Parâmetros

##### Path Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `id` | integer | Sim | Número do cliente (`no`). | Deve existir no sistema. |
| `branch` | integer | Sim | Estabelecimento do cliente (`estab`). | Usar `0` para o estabelecimento principal. |

##### Query Parameters
Nenhum.

##### Body Parameters
Nenhum.

#### Exemplo cURL
```bash
curl 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/clients/4/0' \
  --request GET \
  --header 'Authorization: Bearer YOUR_TOKEN'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "id": 4,
    "branch": 0,
    "name": "Teste123",
    "nuit": "123456789",
    "phone": "",
    "address": "Teste",
    "email": "",
    "inactive": false,
    "addFields": null
  },
  "links": [
    {
      "rel": "self",
      "method": "GET",
      "href": "/api/clients/4/0"
    },
    {
      "rel": "list",
      "method": "GET",
      "href": "/api/clients"
    }
  ]
}
```

**Status Code: 404 - Not Found**

Retornado quando a combinação `id` + `branch` não existe.

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | object | Dados do cliente. |

**Dicionário interno de `item`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `id` | integer | Identificador único do cliente. |
| `branch` | integer | Estabelecimento do cliente. |
| `name` | string | Nome do cliente. |
| `nuit` | string | NUIT do cliente. |
| `phone` | string | Número de telefone. |
| `address` | string | Endereço/morada. |
| `email` | string | Correio electrónico. |
| `inactive` | boolean | Status ativo/inativo. |
| `addFields` | object | Campos adicionais customizados. |

---

### Endpoint: Criar Cliente

#### POST /clients
Cria um novo cliente no sistema. Se `id` e `branch` forem omitidos, o sistema gera automaticamente um novo `id` com `branch = 0`.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
Nenhum.

##### Body Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `id` | integer | Não | Identificador do cliente. | Se omitido, auto-incrementa. |
| `branch` | integer | Não | Estabelecimento do cliente. | Default: `0`. |
| `name` | string | Sim | Nome do cliente. |  |
| `nuit` | string | Sim | NUIT do cliente (9 caracteres). | Deve ser único. |
| `phone` | string | Não | Número de telefone. |  |
| `address` | string | Não | Endereço/morada. |  |
| `email` | string | Não | Correio electrónico. |  |
| `addFields` | object | Não | Campos adicionais customizados. |  |

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/clients \
  --request POST \
  --header 'Content-Type: application/json' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --data '{
    "name": "Novo Cliente",
    "nuit": "999888777",
    "phone": "891234567",
    "address": "Av. XPTO, Maputo",
    "email": "novo@cliente.com",
    "addFields": null
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Cliente criado com sucesso",
  "data": [
    {
      "id": 8,
      "branch": 0,
      "name": "Novo Cliente",
      "nuit": "999888777",
      "phone": "891234567",
      "address": "Av. XPTO, Maputo",
      "email": "novo@cliente.com",
      "inactive": false,
      "addFields": null
    }
  ],
  "links": [
    {
      "rel": "self",
      "href": "/api/clients/8/0",
      "method": "GET"
    },
    {
      "rel": "list",
      "href": "/api/clients",
      "method": "GET"
    }
  ]
}
```

**Status Code: 400 - Bad Request**

Retornado quando faltam campos obrigatórios.

**Status Code: 409 - Conflict**

Retornado quando já existe um cliente com o mesmo NUIT ou combinação `id + branch`.

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | array | Array contendo o cliente criado. |

**Dicionário interno de cada item em `data`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `id` | integer | Identificador único do cliente. |
| `branch` | integer | Estabelecimento do cliente. |
| `name` | string | Nome do cliente. |
| `nuit` | string | NUIT do cliente. |
| `phone` | string | Número de telefone. |
| `address` | string | Endereço/morada. |
| `email` | string | Correio electrónico. |
| `inactive` | boolean | Status ativo/inativo. |
| `addFields` | object | Campos adicionais customizados. |

---

### Endpoint: Atualizar Cliente

#### PATCH /clients/{id}/{branch}
Atualiza informações de um cliente existente. Permite modificar campos como nome, telefone, endereço, email e status inativo.

#### Parâmetros

##### Path Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `id` | integer | Sim | Identificador único do cliente. |  |
| `branch` | integer | Sim | Estabelecimento do cliente. |  |

##### Query Parameters
Nenhum.

##### Body Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `name` | string | Não | Nome do cliente. |  |
| `phone` | string | Não | Número de telefone. |  |
| `address` | string | Não | Endereço/morada. |  |
| `email` | string | Não | Correio electrónico. |  |
| `inactive` | boolean | Não | Status ativo/inativo. |  |

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/clients/4/0 \
  --request PATCH \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "name": "Teste123 Atualizado",
    "phone": "258 84 999 9999",
    "address": "Teste - Rua Nova",
    "email": "novo@email.com",
    "inactive": false
  }'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Cliente atualizado com sucesso",
  "data": [
    {
      "id": 4,
      "branch": 0,
      "name": "Teste123 Atualizado",
      "nuit": "123456789",
      "phone": "258 84 999 9999",
      "address": "Teste - Rua Nova",
      "email": "novo@email.com",
      "inactive": false,
      "addFields": null
    }
  ],
  "links": [
    {
      "rel": "self",
      "href": "/api/clients/4/0",
      "method": "GET"
    },
    {
      "rel": "list",
      "href": "/api/clients",
      "method": "GET"
    }
  ]
}
```

**Status Code: 400 - Bad Request**

**Status Code: 409 - Conflict**

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | array | Array contendo o cliente atualizado. |

**Dicionário interno de cada item em `data`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `id` | integer | Identificador único do cliente. |
| `branch` | integer | Estabelecimento do cliente. |
| `name` | string | Nome do cliente. |
| `nuit` | string | NUIT do cliente. |
| `phone` | string | Número de telefone. |
| `address` | string | Endereço/morada. |
| `email` | string | Correio electrónico. |
| `inactive` | boolean | Status ativo/inativo. |
| `addFields` | object | Campos adicionais customizados. |

---

### Endpoint: Eliminar Cliente
**Em Desenvolvimento**

#### DELETE /clients/{id}/{branch}
Elimina um cliente específico identificado pela combinação de `id` e `branch`.

#### Parâmetros

##### Path Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `id` | integer | Sim | Identificador único do cliente. |  |
| `branch` | integer | Sim | Estabelecimento do cliente. |  |

##### Query Parameters
Nenhum.

##### Body Parameters
Nenhum.

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/clients/4/0 \
  --request DELETE \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Cliente eliminado com sucesso",
  "data": null,
  "links": [
    {
      "rel": "list",
      "href": "/api/clients",
      "method": "GET"
    }
  ]
}
```

**Status Code: 404 - Not Found**

---

### Endpoint: Criar Múltiplos Clientes
**Em Desenvolvimento**

#### POST /clients/bulk
Cria múltiplos clientes em uma única requisição. Cada item do array segue a mesma estrutura que o endpoint `POST /clients`.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
Nenhum.

##### Body Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `items` | array | Sim | Array de clientes a criar. | Máximo 100 itens. |

##### Estrutura interna de `items`
| Campo | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :---- | :--- | :---------- | :-------- | :---------------- |
| `id` | integer | Não | Identificador do cliente. | Se omitido, auto-incrementa. |
| `branch` | integer | Não | Estabelecimento do cliente. | Default: `0`. |
| `name` | string | Sim | Nome do cliente. |  |
| `nuit` | string | Sim | NUIT do cliente. | Deve ser único. |
| `phone` | string | Não | Número de telefone. |  |
| `address` | string | Não | Endereço/morada. |  |
| `email` | string | Não | Correio electrónico. |  |
| `addFields` | object | Não | Campos adicionais customizados. |  |

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/clients/bulk \
  --request POST \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "items": [
      {
        "name": "Cliente 1",
        "nuit": "123456789",
        "phone": "258 84 111 1111",
        "address": "Endereço 1",
        "email": "cliente1@test.com",
        "addFields": null
      },
      {
        "name": "Cliente 2",
        "nuit": "987654321",
        "phone": "258 84 222 2222",
        "address": "Endereço 2",
        "email": "cliente2@test.com",
        "addFields": null
      }
    ]
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Lote de clientes criado com sucesso",
  "data": [
    {
      "id": 9,
      "branch": 0,
      "name": "Cliente 1",
      "nuit": "123456789",
      "phone": "258 84 111 1111",
      "address": "Endereço 1",
      "email": "cliente1@test.com",
      "inactive": false,
      "addFields": null
    },
    {
      "id": 10,
      "branch": 0,
      "name": "Cliente 2",
      "nuit": "987654321",
      "phone": "258 84 222 2222",
      "address": "Endereço 2",
      "email": "cliente2@test.com",
      "inactive": false,
      "addFields": null
    }
  ],
  "links": [
    {
      "rel": "list",
      "href": "/api/clients",
      "method": "GET"
    }
  ]
}
```

**Status Code: 207 - Multi-Status**

Retornado quando alguns itens do lote foram criados com sucesso e outros falharam.

**Status Code: 400 - Bad Request**

**Status Code: 413 - Payload Too Large**

Retornado quando o lote contém mais de 100 itens.

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | array | Array contendo os clientes criados. |

**Dicionário interno de cada item em `data`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `id` | integer | Identificador único do cliente. |
| `branch` | integer | Estabelecimento do cliente. |
| `name` | string | Nome do cliente. |
| `nuit` | string | NUIT do cliente. |
| `phone` | string | Número de telefone. |
| `address` | string | Endereço/morada. |
| `email` | string | Correio electrónico. |
| `inactive` | boolean | Status ativo/inativo. |
| `addFields` | object | Campos adicionais customizados. |

---

## Códigos de Erro Específicos do Módulo

| Código | Descrição | Quando ocorre |
| :---- | :-------- | :------------ |
| `0000` | Operação concluída com sucesso | Criação, atualização ou remoção bem-sucedidas |
| `CL001` | Erro ao persistir cliente na base de dados | Falha de persistência no SQL/EF |
| `CL002` | Já existe um cliente com id X e branch Y | Combinação `id + branch` duplicada |
| `CL004` | Cliente não encontrado | GET/PATCH/DELETE — cliente não existe |
| `CL005` | Cliente com NUIT X já existe | Conflito de NUIT em criação automática |
| `CL006` | Cliente com NUIT X já existe associado ao id Y | Conflito de NUIT associado a outro `id` |
| `CL007` | Combinação inválida entre id e branch | Regras inválidas de composição entre `id` e `branch` |
| `CL010` | Lote não pode conter mais de X itens | POST `/clients/bulk` com mais de 100 itens |
| `CL011` | Lote contém NUIT duplicado: X | POST `/clients/bulk` com NUIT repetido no mesmo lote |
| `CL015` | Operação parcialmente concluída | Bulk com alguns itens validados e outros falhados |

---

