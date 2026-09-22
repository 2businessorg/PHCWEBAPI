# Stocks API - Documentação

## Descrição Geral do Módulo
Endpoints para consulta, criação, atualização e remoção de stocks/artigos, incluindo lotes e visão por armazéns.

> **Base URL:** os exemplos abaixo usam paths relativos (`/stocks`, `/stocks/{reference}`, etc.).
> O prefixo `/api` faz parte da base URL da API e não é repetido nos exemplos.

> **Nota sobre endpoints em desenvolvimento:**
> Endpoints marcados com **Em Desenvolvimento** permanecem neste ficheiro para referência interna, mas não devem aparecer na documentação da página web.

---

## Estrutura do Módulo

O módulo Stocks está organizado no frontend em subpastas temáticas:

```
Stocks
├── Lotes
│   ├── Listar Lotes (GET /stocks/batches)
│   └── Stock por Lote e Armazém (GET /stocks/{reference}/batches)
├── Armazéns
│   ├── Listar Armazéns (GET /stocks/warehouses)
│   └── Stock por Armazém (GET /stocks/{reference}/warehouses)
├── Listar Stocks (GET /stocks)
├── Obter Stock por Referência (GET /stocks/{reference})
├── Criar Stock (POST /stocks)
└── Atualizar Stock (PATCH /stocks/{reference})
```

---

## Endpoint: Listar Stocks

### GET /stocks

**Descrição:**
Lista todos os stocks com paginação e filtros opcionais.

#### Parâmetros

##### Query Parameters

| Parâmetro   | Tipo      | Obrigatório | Descrição                            | Restrições/Padrão |
| :---------- | :-------- | :---------- | :----------------------------------- | :---------------- |
| `page`      | `integer` | Não         | Número da página                     | Padrão: `1`       |
| `pageSize`  | `integer` | Não         | Tamanho da página                    | Padrão: `50`      |
| `referencia`| `string`  | Não         | Filtra por referência do artigo      |                   |
| `descricao` | `string`  | Não         | Filtra por descrição                 |                   |
| `familia`   | `string`  | Não         | Filtra por família                   |                   |
| `inactivo`  | `boolean` | Não         | Filtra por estado ativo/inativo      |                   |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks?page=1&pageSize=50&referencia=ART-001' \
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
      "reference": "ART-001",
      "description": "Artigo de Teste",
      "isService": false,
      "prices": [
        { "table": 1, "value": 1250.0, "isTaxIncluded": true },
        { "table": 2, "value": 1200.0, "isTaxIncluded": true }
      ],
      "quantity": 500.5,
      "taxTableId": 1,
      "familyRef": "FAM001",
      "familyName": "Família Geral",
      "observations": "Artigo em stock",
      "isInactive": false,
      "useBatches": true,
      "addFields": null
    }
  ],
  "meta": {
    "totalItems": 1,
    "itemCount": 1,
    "pageSize": 50,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "href": "/api/stocks?page=1&pageSize=50&referencia=ART-001", "method": "GET" },
    { "rel": "last", "href": "/api/stocks?page=1&pageSize=50&referencia=ART-001", "method": "GET" },
    { "rel": "create", "href": "/api/stocks", "method": "POST" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo         | Tipo      | Descrição |
| :------------ | :-------- | :-------- |
| `reference`   | `string`  | Referência do artigo |
| `description` | `string`  | Descrição do artigo |
| `isService`   | `boolean` | Indica se é serviço |
| `prices`      | `array`   | Lista de preços por tabela |
| `quantity`    | `decimal` | Quantidade total em stock |
| `taxTableId`  | `integer` | ID da tabela de IVA |
| `familyRef`   | `string`  | Referência da família |
| `familyName`  | `string`  | Nome da família |
| `observations`| `string`  | Observações adicionais |
| `isInactive`  | `boolean` | Estado ativo/inativo |
| `useBatches`  | `boolean` | Indica se o artigo controla lotes |
| `addFields`   | `object`  | Campos adicionais dinâmicos|

**Dicionário interno de cada item em `prices`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `table` | `integer` | Número da tabela de preço |
| `value` | `decimal` | Valor do preço |
| `isTaxIncluded` | `boolean` | Indica se o IVA está incluído |

---

## Endpoint: Obter Stock por Referência

### GET /stocks/{reference}

**Descrição:**
Obtém os dados completos de um stock pela sua referência.

#### Parâmetros

##### Path Parameters

| Parâmetro   | Tipo     | Obrigatório | Descrição                | Restrições/Padrão |
| :---------- | :------- | :---------- | :----------------------- | :---------------- |
| `reference` | `string` | Sim         | Referência do artigo     |                   |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/ART-001' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "reference": "ART-001",
    "description": "Artigo de Teste",
    "isService": false,
    "prices": [
      { "table": 1, "value": 1250.0, "isTaxIncluded": true }
    ],
    "quantity": 500.5,
    "taxTableId": 1,
    "familyRef": "FAM001",
    "familyName": "Família Geral",
    "observations": "Artigo em stock",
    "isInactive": false,
    "useBatches": true,
    "addFields": null
  },
  "links": [
    { "rel": "self", "href": "/api/stocks/ART-001", "method": "GET" },
    { "rel": "list", "href": "/api/stocks", "method": "GET" },
    { "rel": "update", "href": "/api/stocks/ART-001", "method": "PATCH" },
    { "rel": "delete", "href": "/api/stocks/ART-001", "method": "DELETE" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo         | Tipo      | Descrição |
| :------------ | :-------- | :-------- |
| `reference`   | `string`  | Referência do artigo |
| `description` | `string`  | Descrição do artigo |
| `isService`   | `boolean` | Indica se é serviço |
| `prices`      | `array`   | Lista de preços por tabela |
| `quantity`    | `decimal` | Quantidade total em stock |
| `taxTableId`  | `integer` | ID da tabela de IVA |
| `familyRef`   | `string`  | Referência da família |
| `familyName`  | `string`  | Nome da família |
| `observations`| `string`  | Observações adicionais |
| `isInactive`  | `boolean` | Estado ativo/inativo |
| `useBatches`  | `boolean` | Indica se o artigo controla lotes |
| `addFields`   | `object`  | Campos adicionais dinâmicos|

**Dicionário interno de cada item em `prices`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `table` | `integer` | Número da tabela de preço |
| `value` | `decimal` | Valor do preço |
| `isTaxIncluded` | `boolean` | Indica se o IVA está incluído |

**Status Code: 404 - Not Found**

Retornado quando a referência informada não existe.

---

## Endpoint: Listar Lotes

### GET /stocks/batches

**Descrição:**
Lista lotes com paginação e filtros opcionais.

#### Parâmetros

##### Query Parameters

| Parâmetro   | Tipo      | Obrigatório | Descrição                        | Restrições/Padrão |
| :---------- | :-------- | :---------- | :------------------------------- | :---------------- |
| `page`      | `integer` | Não         | Número da página                 | Padrão: `1`       |
| `pageSize`  | `integer` | Não         | Tamanho da página                | Padrão: `50`      |
| `referencia`| `string`  | Não         | Filtra por referência do artigo  |                   |
| `lote`      | `string`  | Não         | Filtra por código do lote        |                   |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/batches?page=1&pageSize=50&referencia=ART-001' \
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
      "batch": "LOT-2024-001",
      "reference": "ART-001",
      "description": "Artigo de Teste",
      "supplierBatch": "SUPP-LOT-001",
      "stock": 250.5,
      "quantityOutYear": 150.0,
      "quantityInYear": 200.0,
      "lastEntry": "2026-04-10",
      "expiryDate": "2027-04-10"
    }
  ],
  "meta": {
    "totalItems": 1,
    "itemCount": 1,
    "pageSize": 50,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "href": "/api/stocks/batches?page=1&pageSize=50&referencia=ART-001", "method": "GET" },
    { "rel": "last", "href": "/api/stocks/batches?page=1&pageSize=50&referencia=ART-001", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de lotes |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `batch` | `string` | Identificador do lote |
| `reference` | `string` | Referência do artigo |
| `description` | `string` | Descrição do artigo |
| `supplierBatch` | `string` | Lote do fornecedor |
| `stock` | `decimal` | Quantidade atual do lote |
| `quantityOutYear` | `decimal` | Saídas no ano |
| `quantityInYear` | `decimal` | Entradas no ano |
| `lastEntry` | `date` | Data da última entrada |
| `expiryDate` | `date` | Data de validade |

---

## Endpoint: Stock por Lote e Armazém

### GET /stocks/{reference}/batches

**Descrição:**
Obtém os stocks por armazém para um artigo, com filtro opcional de lote.

#### Parâmetros

##### Path Parameters

| Parâmetro   | Tipo     | Obrigatório | Descrição             | Restrições/Padrão |
| :---------- | :------- | :---------- | :-------------------- | :---------------- |
| `reference` | `string` | Sim         | Referência do artigo  |                   |

##### Query Parameters

| Parâmetro | Tipo     | Obrigatório | Descrição                  | Restrições/Padrão |
| :-------- | :------- | :---------- | :------------------------- | :---------------- |
| `lote`    | `string` | Não         | Filtra por lote específico |                   |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/ART-001/batches?lote=LOT-2024-001' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

**Status Code: 404 - Not Found**

Retornado quando não existem dados para a referência/lote informados.

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de stocks por lote e armazém |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `batch` | `string` | Identificador do lote |
| `reference` | `string` | Referência do artigo |
| `warehouse` | `integer` | Número do armazém |
| `warehouseName` | `string` | Nome do armazém |
| `stock` | `decimal` | Quantidade em stock |
| `location` | `string` | Localização |

---

## Endpoint: Listar Armazéns

### GET /stocks/warehouses

**Descrição:**
Lista todos os armazéns disponíveis.

#### Parâmetros

Não aplicável.

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/warehouses' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de armazéns |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `number` | `integer` | Identificador do armazém |
| `name` | `string` | Nome do armazém |

---

## Endpoint: Stock por Armazém

### GET /stocks/{reference}/warehouses

**Descrição:**
Obtém o stock agregado por armazém de um artigo.

#### Parâmetros

##### Path Parameters

| Parâmetro   | Tipo     | Obrigatório | Descrição             | Restrições/Padrão |
| :---------- | :------- | :---------- | :-------------------- | :---------------- |
| `reference` | `string` | Sim         | Referência do artigo  |                   |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/ART-001/warehouses' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de stocks por armazém |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `warehouse` | `integer` | Número do armazém |
| `warehouseName` | `string` | Nome do armazém |
| `stock` | `decimal` | Quantidade total em armazém |
| `stockCost` | `decimal` | Preço de custo ponderado |
| `location` | `string` | Localização padrão |
| `orderedByClients` | `decimal` | Quantidade encomendada por clientes |
| `orderedFromSuppliers` | `decimal` | Quantidade encomendada a fornecedores |
| `minimumStock` | `decimal` | Stock mínimo definido |
| `quantityInReceipt` | `decimal` | Quantidade em receção |
| `quantityCaptive` | `decimal` | Quantidade cativada |

**Status Code: 404 - Not Found**

Retornado quando a referência não possui dados por armazém.

---

## Endpoint: Criar Stock

### POST /stocks

**Descrição:**
Cria um novo stock.

#### Parâmetros

##### Body Parameters

| Parâmetro       | Tipo      | Obrigatório | Descrição                    |
| :-------------- | :-------- | :---------- | :--------------------------- |
| `reference`     | `string`  | Sim         | Referência do artigo         |
| `description`   | `string`  | Sim         | Descrição do artigo          |
| `isService`     | `boolean` | Não         | Indica se é serviço          |
| `prices`        | `array`   | Não         | Lista de preços              |
| `taxTableId`    | `integer` | Não         | Tabela de IVA                |
| `familyRef`     | `string`  | Não         | Referência da família        |
| `observations`  | `string`  | Não         | Observações                  |
| `isInactive`    | `boolean` | Não         | Estado ativo/inativo         |
| `addFields`     | `object`  | Não         | Campos adicionais|

##### Estrutura interna de `prices`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `table` | `integer` | Sim | Número da tabela de preço |
| `value` | `decimal` | Sim | Valor do preço |
| `isTaxIncluded` | `boolean` | Não | Indica se o IVA está incluído |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "reference": "ART-002",
    "description": "Novo Artigo",
    "isService": false,
    "prices": [
      { "table": 1, "value": 1250.0, "isTaxIncluded": true }
    ],
    "taxTableId": 1,
    "familyRef": "FAM001",
    "observations": "Artigo novo",
    "isInactive": false,
    "addFields": null
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "reference": "ART-002",
    "description": "Novo Artigo",
    "isService": false,
    "prices": [
      { "table": 1, "value": 1250.0, "isTaxIncluded": true }
    ],
    "quantity": 0.0,
    "taxTableId": 1,
    "familyRef": "FAM001",
    "familyName": "Família Geral",
    "observations": "Artigo novo",
    "isInactive": false,
    "useBatches": false,
    "addFields": null
  },
  "links": [
    { "rel": "self", "href": "/api/stocks/ART-002", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo         | Tipo      | Descrição |
| :------------ | :-------- | :-------- |
| `reference`   | `string`  | Referência do artigo |
| `description` | `string`  | Descrição do artigo |
| `isService`   | `boolean` | Indica se é serviço |
| `prices`      | `array`   | Lista de preços por tabela |
| `quantity`    | `decimal` | Quantidade total em stock |
| `taxTableId`  | `integer` | ID da tabela de IVA |
| `familyRef`   | `string`  | Referência da família |
| `familyName`  | `string`  | Nome da família |
| `observations`| `string`  | Observações adicionais |
| `isInactive`  | `boolean` | Estado ativo/inativo |
| `useBatches`  | `boolean` | Indica se o artigo controla lotes |
| `addFields`   | `object`  | Campos adicionais dinâmicos|

**Dicionário interno de cada item em `prices`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `table` | `integer` | Número da tabela de preço |
| `value` | `decimal` | Valor do preço |
| `isTaxIncluded` | `boolean` | Indica se o IVA está incluído |

**Status Code: 400 - Requisição Inválida**

Retornado quando os dados obrigatórios estão faltando ou inválidos.

**Status Code: 409 - Conflito**

Retornado quando já existe um artigo com a mesma referência.

---

## Endpoint: Atualizar Stock

### PATCH /stocks/{reference}

**Descrição:**
Atualiza parcialmente um stock por referência.

#### Parâmetros

##### Path Parameters

| Parâmetro   | Tipo     | Obrigatório | Descrição             | Restrições/Padrão |
| :---------- | :------- | :---------- | :-------------------- | :---------------- |
| `reference` | `string` | Sim         | Referência do artigo  |                   |

##### Body Parameters

| Parâmetro       | Tipo      | Obrigatório | Descrição                    | Restrições/Padrão |
| :-------------- | :-------- | :---------- | :--------------------------- | :---------------- |
| `description`   | `string`  | Não         | Descrição do artigo          |                   |
| `isService`     | `boolean` | Não         | Indica se é serviço          |                   |
| `prices`        | `array`   | Não         | Lista de preços              |                   |
| `taxTableId`    | `integer` | Não         | Tabela de IVA                |                   |
| `familyRef`     | `string`  | Não         | Referência da família        |                   |
| `observations`  | `string`  | Não         | Observações                  |                   |
| `isInactive`    | `boolean` | Não         | Estado ativo/inativo         |                   |
| `addFields`     | `object`  | Não         | Campos adicionais|                   |

##### Estrutura interna de `prices`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `table` | `integer` | Sim | Número da tabela de preço |
| `value` | `decimal` | Sim | Valor do preço |
| `isTaxIncluded` | `boolean` | Não | Indica se o IVA está incluído |

#### Exemplo cURL

```bash
curl -X PATCH 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/ART-001' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "description": "Artigo Atualizado",
    "prices": [
      { "table": 1, "value": 1300.0, "isTaxIncluded": true }
    ],
    "addFields": {
      "color": "red"
    }
  }'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "reference": "ART-001",
    "description": "Artigo Atualizado",
    "isService": false,
    "prices": [
      { "table": 1, "value": 1300.0, "isTaxIncluded": true }
    ],
    "quantity": 500.5,
    "taxTableId": 1,
    "familyRef": "FAM001",
    "familyName": "Família Geral",
    "observations": "Atualizado",
    "isInactive": false,
    "useBatches": true,
    "addFields": {
      "color": "red"
    }
  },
  "links": [
    { "rel": "self", "href": "/api/stocks/ART-001", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo         | Tipo      | Descrição |
| :------------ | :-------- | :-------- |
| `reference`   | `string`  | Referência do artigo |
| `description` | `string`  | Descrição do artigo |
| `isService`   | `boolean` | Indica se é serviço |
| `prices`      | `array`   | Lista de preços por tabela |
| `quantity`    | `decimal` | Quantidade total em stock |
| `taxTableId`  | `integer` | ID da tabela de IVA |
| `familyRef`   | `string`  | Referência da família |
| `familyName`  | `string`  | Nome da família |
| `observations`| `string`  | Observações adicionais |
| `isInactive`  | `boolean` | Estado ativo/inativo |
| `useBatches`  | `boolean` | Indica se o artigo controla lotes |
| `addFields`   | `object`  | Campos adicionais dinâmicos|

**Dicionário interno de cada item em `prices`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `table` | `integer` | Número da tabela de preço |
| `value` | `decimal` | Valor do preço |
| `isTaxIncluded` | `boolean` | Indica se o IVA está incluído |

**Status Code: 404 - Not Found**

Retornado quando a referência informada não existe.

---

## Endpoint: Criar Stocks em Lote
**Em Desenvolvimento**

### POST /stocks/bulk

**Descrição:**
Cria múltiplos stocks em lote (máx. 100 itens), com suporte a sucesso parcial.

#### Parâmetros

##### Body Parameters

| Parâmetro | Tipo    | Obrigatório | Descrição                  |
| :-------- | :------ | :---------- | :------------------------- |
| `items`   | `array` | Sim         | Lista de stocks a criar    |

##### Estrutura interna de cada item em `items`

| Campo         | Tipo      | Obrigatório | Descrição |
| :------------ | :-------- | :---------- | :-------- |
| `reference`   | `string`  | Sim         | Referência do artigo |
| `description` | `string`  | Sim         | Descrição do artigo |
| `isService`   | `boolean` | Não         | Indica se é serviço |
| `prices`      | `array`   | Não         | Lista de preços por tabela |
| `taxTableId`  | `integer` | Não         | Tabela de IVA |
| `familyRef`   | `string`  | Não         | Referência da família |
| `observations`| `string`  | Não         | Observações |
| `isInactive`  | `boolean` | Não         | Estado ativo/inativo |
| `addFields`   | `object`  | Não         | Campos adicionais|

##### Estrutura interna de `prices`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `table` | `integer` | Sim | Número da tabela de preço |
| `value` | `decimal` | Sim | Valor do preço |
| `isTaxIncluded` | `boolean` | Não | Indica se o IVA está incluído |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/stocks/bulk' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "items": [
      {
        "reference": "ART-003",
        "description": "Artigo 3",
        "prices": [{ "table": 1, "value": 1200.0, "isTaxIncluded": true }]
      }
    ]
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta (sucesso parcial):

```json
{
  "items": [
    {
      "reference": "ART-003",
      "description": "Artigo 3",
      "isService": false,
      "prices": [
        { "table": 1, "value": 1200.0, "isTaxIncluded": true }
      ],
      "quantity": 0.0,
      "taxTableId": 0,
      "familyRef": null,
      "familyName": null,
      "observations": null,
      "isInactive": false,
      "useBatches": false,
      "addFields": null
    }
  ],
  "meta": {
    "totalProcessed": 1,
    "successCount": 1,
    "failureCount": 0
  },
  "links": [
    { "rel": "self", "href": "/api/stocks/bulk", "method": "POST" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de artigos criados |

**Dicionário interno de cada item em `items`**

| Campo         | Tipo      | Descrição |
| :------------ | :-------- | :-------- |
| `reference`   | `string`  | Referência do artigo |
| `description` | `string`  | Descrição do artigo |
| `isService`   | `boolean` | Indica se é serviço |
| `prices`      | `array`   | Lista de preços por tabela |
| `quantity`    | `decimal` | Quantidade total em stock |
| `taxTableId`  | `integer` | ID da tabela de IVA |
| `familyRef`   | `string`  | Referência da família |
| `familyName`  | `string`  | Nome da família |
| `observations`| `string`  | Observações adicionais |
| `isInactive`  | `boolean` | Estado ativo/inativo |
| `useBatches`  | `boolean` | Indica se o artigo controla lotes |
| `addFields`   | `object`  | Campos adicionais dinâmicos|

**Dicionário interno de cada item em `prices`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `table` | `integer` | Número da tabela de preço |
| `value` | `decimal` | Valor do preço |
| `isTaxIncluded` | `boolean` | Indica se o IVA está incluído |

**Status Code: 400 - Requisição Inválida**

Retornado quando o lote está vazio, ultrapassa o limite de 100 itens ou contém referências duplicadas.

**Status Code: 207 - Multi-Status**

Retornado quando há sucesso parcial (alguns itens criados, outros falharam).

---

## Endpoint: Eliminar Stock
**Em Desenvolvimento**

### DELETE /stocks/{reference}

**Descrição:**
Elimina um stock pela referência.

#### Parâmetros

##### Path Parameters

| Parâmetro   | Tipo     | Obrigatório | Descrição             | Restrições/Padrão |
| :---------- | :------- | :---------- | :-------------------- | :---------------- |
| `reference` | `string` | Sim         | Referência do artigo  |                   |

---

## Dicionário de Dados

Os detalhes de campo foram distribuídos nas secções de resposta correspondentes acima para facilitar o uso no frontend com IA e reduzir duplicação.

---

## Códigos de Erro Específicos do Módulo

| Código  | Descrição                                             | Quando ocorre |
| :------ | :---------------------------------------------------- | :------------ |
| `0000`  | Operação concluída com sucesso                        | Sucesso geral |
| `ST001` | Erro ao persistir stock na base de dados              | Falha de persistência |
| `ST002` | Já existe um artigo com referência {0}                | Duplicidade de referência |
| `ST003` | Referência inválida                                   | Validação de entrada |
| `ST004` | Stock não encontrado                                  | GET/PATCH/DELETE sem referência existente |
| `ST010` | Lote não pode conter mais de {0} itens               | Bulk acima do limite |
| `ST011` | Lote contém referência duplicada: {0}                 | Bulk com duplicados |
| `ST012` | Item no índice {0} falhou validação: {1}              | Erro de validação por item |
| `ST013` | Item no índice {0} falhou ao persistir: {1}           | Erro de persistência por item |
| `ST014` | Lote vazio ou inválido                                | Bulk inválido |
| `ST015` | Lote processado: {0} sucessos, {1} falhas             | Sucesso parcial |
| `ST016` | Erro de validação                                     | Validação geral |
| `ST017` | Tabela de preço inválida (deve estar entre 1 e 5)     | Validação de preços |

