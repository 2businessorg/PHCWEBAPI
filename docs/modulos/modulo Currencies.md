# Currencies API - Documentação

## Descrição Geral do Módulo
Endpoints para gestão de moedas e taxas de conversão. Permite listar, criar e atualizar moedas, bem como manter as taxas de câmbio de compra e venda para cada moeda.

> **Nota para leitura do Dicionário de Dados Interno e para a AI de frontend:**
> O `Dicionário de Dados Interno` destaca os campos funcionais do recurso devolvido em `item`, `items` ou `data`. Estruturas genéricas como `meta` e `links` não fazem parte do dicionário do recurso principal.
> Sempre que existir uma key do tipo `array` ou `object` no payload funcional, a documentação inclui a secção que detalha os campos internos dessa estrutura.
> A AI de frontend deve manter secções colapsáveis: um colapsável para `Response` e, nos endpoints com body, outro colapsável para o payload de entrada. Dentro de cada colapsável deve ficar toda a informação estrutural, incluindo tabela principal e dicionários internos de arrays e objetos.

> **Base URL:** os exemplos abaixo usam paths relativos (`/currencies`, `/currencies/{moeda}`, `/currencies/exchangeRates`, etc.).
> O prefixo `/api` faz parte da base URL da API e não é repetido nos exemplos.

> **Nota sobre endpoints em desenvolvimento:**
> Endpoints ainda em desenvolvimento continuam presentes neste ficheiro `.md` para referência interna.
> Estes endpoints não devem aparecer na documentação da API na página web.

---

## Estrutura do Módulo

O módulo Currencies está organizado em subpastas temáticas:

```text
Currencies
└── Taxas de Conversão
    ├── Listar Taxas (GET /currencies/exchangeRates)
    ├── Obter Taxas de Moeda (GET /currencies/exchangeRates/{moeda})
    └── Criar Taxa de Conversão (POST /currencies/exchangeRates)
├── Listar Moedas (GET /currencies)
├── Obter Moeda (GET /currencies/{moeda})
├── Criar Moeda (POST /currencies)
├── Atualizar Moeda (PATCH /currencies/{moeda})
└── Eliminar Moeda (DELETE /currencies/{moeda})

```

---

## Subpasta: Moedas

### Endpoint: Listar Moedas

#### GET /currencies

**Descrição:**
Lista todas as moedas disponíveis no sistema.

#### Parâmetros

Não aplicável.

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies' \
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
      "currency": "EUR",
      "country": "Portugal"
    },
    {
      "currency": "USD",
      "country": "Estados Unidos"
    },
    {
      "currency": "MZN",
      "country": "Moçambique"
    }
  ],
  "meta": {
    "totalItems": 3,
    "itemCount": 3,
    "pageSize": 100,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/currencies" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de moedas |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda. Ex: `EUR`, `USD`, `MZN` |
| `country` | `string` | País ou região associado à moeda |

---

### Endpoint: Obter Moeda

#### GET /currencies/{moeda}

**Descrição:**
Obtém o detalhe de uma moeda específica pelo seu código.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `moeda` | `string` | Sim | Código da moeda | Ex: `EUR`, `USD` |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies/EUR' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "currency": "EUR",
    "country": "Portugal"
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/currencies/EUR" },
    { "rel": "list", "method": "GET", "href": "/api/currencies" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | `object` | Dados da moeda encontrada |

**Dicionário interno de `item`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda |
| `country` | `string` | País ou região associado à moeda |

**Status Code: 404 - Not Found**

Retornado quando a moeda especificada não existe.

---

### Endpoint: Criar Moeda

#### POST /currencies

**Descrição:**
Cria uma nova moeda no sistema.

#### Parâmetros

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `currency` | `string` | Sim | Código da moeda | Máximo: `3` caracteres |
| `country` | `string` | Sim | País ou região associado à moeda | |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "currency": "USD",
    "country": "Estados Unidos"
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Moeda criada com sucesso",
  "data": [
    {
      "currency": "USD",
      "country": "Estados Unidos"
    }
  ],
  "links": [
    { "rel": "self", "href": "/api/currencies/USD", "method": "GET" },
    { "rel": "list", "href": "/api/currencies", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `array` | Lista com a moeda criada |

**Dicionário interno de cada item em `data`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda |
| `country` | `string` | País ou região associado à moeda |

**Status Code: 400 - Requisição Inválida**

Retornado quando a moeda já existe ou os dados são inválidos.

---

### Endpoint: Atualizar Moeda
**Em Desenvolvimento**

#### PATCH /currencies/{moeda}

**Descrição:**
Atualiza o país associado a uma moeda existente.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `moeda` | `string` | Sim | Código da moeda | Ex: `EUR`, `USD` |

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `country` | `string` | Sim | Novo país ou região associado à moeda | |

#### Exemplo cURL

```bash
curl -X PATCH 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies/XPT' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "country": "Xptolandia"
  }'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Moeda atualizada com sucesso",
  "data": [
    {
      "currency": "XPT",
      "country": "Xptolandia"
    }
  ],
  "links": [
    { "rel": "self", "href": "/api/currencies/XPT", "method": "GET" },
    { "rel": "list", "href": "/api/currencies", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `array` | Lista com a moeda atualizada |

**Dicionário interno de cada item em `data`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda |
| `country` | `string` | País ou região associado à moeda atualizado |

**Status Code: 404 - Not Found**

Retornado quando a moeda não existe.

---

### Endpoint: Eliminar Moeda
**Em Desenvolvimento**

#### DELETE /currencies/{moeda}

**Descrição:**
Elimina permanentemente uma moeda do sistema.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `moeda` | `string` | Sim | Código da moeda | Ex: `EUR`, `USD` |

#### Exemplo cURL

```bash
curl -X DELETE 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies/XPT' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Moeda eliminada com sucesso",
  "data": null,
  "links": [
    { "rel": "list", "href": "/api/currencies", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `null` | Sem payload de dados no sucesso da eliminação |

**Status Code: 404 - Not Found**

Retornado quando a moeda não existe.

---

## Subpasta: Taxas de Conversão

### Endpoint: Listar Todas as Taxas de Conversão

#### GET /currencies/exchangeRates

**Descrição:**
Lista todas as taxas de câmbio de compra e venda disponíveis no sistema.

#### Parâmetros

Não aplicável.

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies/exchangeRates' \
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
      "currency": "USD",
      "country": "Estados Unidos",
      "buyRate": 64.520,
      "sellRate": 65.120,
      "date": "2026-04-23T00:00:00"
    },
    {
      "currency": "EUR",
      "country": "Portugal",
      "buyRate": 69.100,
      "sellRate": 69.900,
      "date": "2026-04-23T00:00:00"
    }
  ],
  "meta": {
    "totalItems": 2,
    "itemCount": 2,
    "pageSize": 100,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/currencies/exchangeRates" },
    { "rel": "currencies", "method": "GET", "href": "/api/currencies" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de taxas de conversão |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda |
| `country` | `string` | País ou região associado à moeda |
| `buyRate` | `decimal` | Taxa de compra da moeda |
| `sellRate` | `decimal` | Taxa de venda da moeda |
| `date` | `string` | Data da taxa de conversão no formato `YYYY-MM-DDTHH:mm:ss` |

---

### Endpoint: Obter Taxas de Conversão de uma Moeda

#### GET /currencies/exchangeRates/{moeda}

**Descrição:**
Obtém as taxas de câmbio de compra e venda para uma moeda específica.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `moeda` | `string` | Sim | Código da moeda | Ex: `EUR`, `USD` |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies/exchangeRates/USD' \
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
      "currency": "USD",
      "country": "Estados Unidos",
      "buyRate": 64.520,
      "sellRate": 65.120,
      "date": "2026-04-23T00:00:00"
    }
  ],
  "meta": {
    "totalItems": 1,
    "itemCount": 1,
    "pageSize": 1,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/currencies/exchangeRates/USD" },
    { "rel": "currency", "method": "GET", "href": "/api/currencies/USD" },
    { "rel": "list", "method": "GET", "href": "/api/currencies/exchangeRates" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de taxas de conversão para a moeda especificada |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda |
| `country` | `string` | País ou região associado à moeda |
| `buyRate` | `decimal` | Taxa de compra da moeda |
| `sellRate` | `decimal` | Taxa de venda da moeda |
| `date` | `string` | Data da taxa de conversão no formato `YYYY-MM-DDTHH:mm:ss` |

**Status Code: 404 - Not Found**

Retornado quando a moeda não existe.

---

### Endpoint: Criar Taxa de Conversão

#### POST /currencies/exchangeRates

**Descrição:**
Cria uma nova taxa de câmbio para uma moeda. A moeda deve ter sido criada previamente.

#### Parâmetros

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `currency` | `string` | Sim | Código da moeda | Deve existir em `/currencies` |
| `country` | `string` | Sim | País ou região associado à moeda | Deve corresponder à moeda |
| `buyRate` | `decimal` | Sim | Taxa de compra da moeda | Deve ser maior que `0` |
| `sellRate` | `decimal` | Sim | Taxa de venda da moeda | Deve ser maior que `0` |
| `date` | `string` | Não | Data da taxa de conversão | Formato: `YYYY-MM-DDTHH:mm:ss`. Padrão: data/hora actual |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/currencies/exchangeRates' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "currency": "USD",
    "country": "Estados Unidos",
    "buyRate": 64.0,
    "sellRate": 64.5,
    "date": "2026-04-23T00:00:00"
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Taxa de conversão criada com sucesso",
  "data": [
    {
      "currency": "USD",
      "country": "Estados Unidos",
      "buyRate": 64.0,
      "sellRate": 64.5,
      "date": "2026-04-23T00:00:00"
    }
  ],
  "links": [
    { "rel": "self", "href": "/api/currencies/exchangeRates/USD", "method": "GET" },
    { "rel": "rates", "href": "/api/currencies/exchangeRates", "method": "GET" },
    { "rel": "currency", "href": "/api/currencies/USD", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `array` | Lista com a taxa de conversão criada |

**Dicionário interno de cada item em `data`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `currency` | `string` | Código da moeda |
| `country` | `string` | País ou região associado à moeda |
| `buyRate` | `decimal` | Taxa de compra da moeda |
| `sellRate` | `decimal` | Taxa de venda da moeda |
| `date` | `string` | Data da taxa de conversão |

**Status Code: 400 - Requisição Inválida**

Retornado quando os dados são inválidos ou a moeda não existe.

---

## Códigos de Erro Específicos do Módulo

| Código | Descrição | Quando ocorre |
| :----- | :-------- | :------------ |
| `0000` | Operação concluída com sucesso | Operações concluídas com sucesso |
| `CUR001` | Erro ao persistir moeda na base de dados | Falha de persistência no SQL |
| `CUR002` | Moeda duplicada | Tentativa de criar moeda com código já existente |
| `CUR003` | Combinação inválida | Combinação moeda + país nunca foi criada anteriormente |
| `CUR004` | Moeda não encontrada | Moeda especificada não existe |
| `CUR005` | Erro de validação | Campo obrigatório em falta, formato inválido ou valor inválido |

---
alizar moeda
```bash
curl -X PATCH https://api.example.com/api/currencies/XPT \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "pais": "Xptolandia"
  }'
```

### Eliminar moeda
```bash
curl -X DELETE https://api.example.com/api/currencies/XPT \
  -H "Authorization: Bearer {token}"
```

### Obter moeda específica
```bash
curl -X GET https://api.example.com/api/currencies/USD \
  -H "Authorization: Bearer {token}"
```
