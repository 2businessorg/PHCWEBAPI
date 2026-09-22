# VatTaxes API - Documentação

## Descrição Geral do Módulo
Endpoints para consulta e atualização de taxas de IVA. Permite listar todas as taxas disponíveis, obter o detalhe de uma taxa pelo seu código e atualizar os seus campos.

> **Nota para leitura do Dicionário de Dados Interno e para a AI de frontend:**
> O `Dicionário de Dados Interno` destaca os campos funcionais do recurso devolvido em `item` ou `items`. Estruturas genéricas como `meta` e `links` não fazem parte do dicionário do recurso principal.
> A AI de frontend deve manter secções colapsáveis: um colapsável para `Response` e, nos endpoints com body, outro colapsável para o payload de entrada. Dentro de cada colapsável deve ficar toda a informação estrutural, incluindo tabela principal e dicionários internos de arrays e objetos.

> **Base URL:** os exemplos abaixo usam paths relativos (`/vatTaxes`, `/vatTaxes/{code}`, etc.).
> O prefixo `/api` faz parte da base URL da API e não é repetido nos exemplos.

---

## Estrutura do Módulo

O módulo VatTaxes não possui subpastas temáticas:

```text
VatTaxes
├── Listar Taxas de IVA (GET /vatTaxes)
├── Obter Taxa de IVA (GET /vatTaxes/{code})
└── Atualizar Taxa de IVA (PATCH /vatTaxes/{code})
```

---

### Endpoint: Listar Taxas de IVA

#### GET /vatTaxes

**Descrição:**
Lista todas as taxas de IVA disponíveis com paginação e filtros opcionais.

#### Parâmetros

##### Query Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `page` | `integer` | Não | Número da página | Padrão: `1` |
| `pageSize` | `integer` | Não | Tamanho da página | Padrão: `20` |
| `code` | `integer` | Não | Filtra pelo código da taxa | |
| `rate` | `decimal` | Não | Filtra pela percentagem de IVA | |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/vatTaxes?page=1&pageSize=50' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "items": [
    {
      "code": 1,
      "rate": 16.00,
      "reference": "IVA_NORMAL",
      "description": "IVA Normal"
    },
    {
      "code": 2,
      "rate": 6.00,
      "reference": "IVA_REDUZIDO",
      "description": "IVA Reduzido"
    }
  ],
  "meta": {
    "totalItems": 5,
    "itemCount": 2,
    "pageSize": 50,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/vatTaxes?page=1&pageSize=50" },
    { "rel": "create", "method": "POST", "href": "/api/vatTaxes" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de taxas de IVA |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `code` | `integer` | Código identificador da taxa. Chave primária |
| `rate` | `decimal` | Percentagem de IVA. Ex: `16.00` para 16% |
| `reference` | `string` | Referência/código textual da taxa. Ex: `IVA_NORMAL` |
| `description` | `string` | Descrição legível da taxa |

---

### Endpoint: Obter Taxa de IVA

#### GET /vatTaxes/{code}

**Descrição:**
Obtém o detalhe de uma taxa de IVA pelo seu código identificador.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `code` | `integer` | Sim | Código identificador da taxa de IVA | Deve ser maior que `0` |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/vatTaxes/1' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "code": 1,
    "rate": 16.00,
    "reference": "IVA_NORMAL",
    "description": "IVA Normal"
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/vatTaxes/1" },
    { "rel": "list", "method": "GET", "href": "/api/vatTaxes" },
    { "rel": "update", "method": "PATCH", "href": "/api/vatTaxes/1" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | `object` | Dados da taxa de IVA encontrada |

**Dicionário interno de `item`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `code` | `integer` | Código identificador da taxa. Chave primária |
| `rate` | `decimal` | Percentagem de IVA. Ex: `16.00` para 16% |
| `reference` | `string` | Referência/código textual da taxa. Ex: `IVA_NORMAL` |
| `description` | `string` | Descrição legível da taxa |

**Status Code: 404 - Not Found**

Retornado quando não existe uma taxa de IVA com o código informado.

---

### Endpoint: Atualizar Taxa de IVA

#### PATCH /vatTaxes/{code}

**Descrição:**
Atualiza os campos de uma taxa de IVA existente. Apenas os campos enviados no body são alterados.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `code` | `integer` | Sim | Código identificador da taxa de IVA | Deve ser maior que `0` |

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `rate` | `decimal` | Não | Nova percentagem de IVA | Deve ser maior que `0` |
| `reference` | `string` | Não | Nova referência/código textual da taxa | |
| `description` | `string` | Não | Nova descrição da taxa | |

#### Exemplo cURL

```bash
curl -X PATCH 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/vatTaxes/1' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "rate": 18.00,
    "reference": "IVA_NORMAL_UPDATED",
    "description": "IVA Normal Atualizado"
  }'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "code": 1,
    "rate": 18.00,
    "reference": "IVA_NORMAL_UPDATED",
    "description": "IVA Normal Atualizado"
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/vatTaxes/1" },
    { "rel": "list", "method": "GET", "href": "/api/vatTaxes" },
    { "rel": "update", "method": "PATCH", "href": "/api/vatTaxes/1" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | `object` | Dados da taxa de IVA após a atualização |

**Dicionário interno de `item`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `code` | `integer` | Código identificador da taxa. Chave primária |
| `rate` | `decimal` | Percentagem de IVA atualizada |
| `reference` | `string` | Referência/código textual atualizado |
| `description` | `string` | Descrição atualizada da taxa |

**Status Code: 400 - Requisição Inválida**

Retornado quando os dados de entrada falham validação.

**Status Code: 404 - Not Found**

Retornado quando não existe uma taxa de IVA com o código informado.

---

## Códigos de Erro Específicos do Módulo

| Código | Descrição | Quando ocorre |
| :----- | :-------- | :------------ |
| `0000` | Operação concluída com sucesso | Operações concluídas com sucesso |
| `VT001` | Taxa de IVA não encontrada | Não existe taxa com o código informado |
| `VT002` | Combinação de código e taxa já existe | Tentativa de criar com dados duplicados |
| `VT003` | Código de taxa inválido | O campo `code` deve ser maior que `0` |
| `VT004` | Valor de taxa de IVA inválido | O campo `rate` deve ser maior que `0` |
| `VT005` | Taxa atualizada com sucesso | PATCH concluído com sucesso |
| `VT006` | Erro ao persistir taxa na base de dados | Falha de persistência no SQL |

---
