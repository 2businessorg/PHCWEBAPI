# Dossiers API - Documentação

## Descrição Geral do Módulo
Endpoints para criação, consulta e eliminação de dossiers. Um dossier representa um documento de negócio (ex: pré-fatura, encomenda, requisição interna, transferência de armazém), identificado por tipo de documento (`docTypeId`), número sequencial (`docNumber`) e ano (`year`). Suporta criação individual e em lote.

> **Nota para leitura do Dicionário de Dados Interno e para a AI de frontend:**
> O `Dicionário de Dados Interno` destaca os campos funcionais do recurso devolvido em `item`, `items` ou `data`. Estruturas genéricas como `meta`, `links`, `code` e `message` não fazem parte do dicionário do recurso principal.
> Sempre que existir uma key do tipo `array` ou `object` no payload funcional, a documentação inclui a secção que detalha os campos internos dessa estrutura, para que no frontend não se veja apenas a key sem o dicionário dos itens/subitens.
> A AI de frontend deve manter secções colapsáveis: um colapsável para `Response` e, nos endpoints com body, outro colapsável para o payload de entrada. Dentro de cada colapsável deve ficar toda a informação estrutural, incluindo tabela principal e dicionários internos de arrays e objetos.

> **Base URL:** os exemplos abaixo usam paths relativos (`/dossiers`, `/dossiers/types`, etc.).
> O prefixo `/api` faz parte da base URL da API e não é repetido nos exemplos.

> **Nota sobre endpoints em desenvolvimento:**
> Endpoints ainda em desenvolvimento continuam presentes neste ficheiro `.md` para referência interna.
> Estes endpoints não devem aparecer na documentação da API na página web.

---

## Estrutura do Módulo

O módulo Dossiers está organizado no frontend em subpastas temáticas:

```text
Dossiers
├── Tipos de Dossier
│   └── Listar Tipos (GET /dossiers/types)
├── Listar Dossiers (GET /dossiers)
├── Obter Dossier por Chave (GET /dossiers/{docTypeId}/{docNumber}/{year})
├── Criar Dossier (POST /dossiers)
├── Criar em Lote (POST /dossiers/bulk)
└── Eliminar Dossier (DELETE /dossiers/{docTypeId}/{docNumber}/{year})
```

---

## Subpasta: Tipos de Dossier

### Endpoint: Listar Tipos

#### GET /dossiers/types

**Descrição:**
Lista todos os tipos de dossier disponíveis no sistema. Cada tipo define o comportamento do dossier e a tabela associada (Cliente, Fornecedor, Entidade ou Contacto).

#### Parâmetros

Não aplicável.

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/dossiers/types' \
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
      "type": 45,
      "docTypeName": "Encomenda de Cliente",
      "tableName": "Client",
      "table": "CL"
    },
    {
      "type": 2,
      "docTypeName": "Encomenda de Fornecedor",
      "tableName": "Supplier",
      "table": "FL"
    }
  ],
  "meta": {
    "totalItems": 2,
    "itemCount": 2,
    "pageSize": 2,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "href": "/api/dossiers/types", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de tipos de dossier |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `type` | `integer` | Identificador do tipo de dossier. Usado como `docTypeId` nas operações |
| `docTypeName` | `string` | Nome descritivo do tipo de dossier |
| `tableName` | `string` | Nome da tabela associada em inglês (`Client`, `Supplier`, `Entity`, `Contact`) |
| `table` | `string` | Código da tabela associada (`CL` = Cliente, `FL` = Fornecedor, `AG` = Entidade, `EM` = Contacto) |

---

## Pasta Principal: Dossiers

### Endpoint: Listar Dossiers

#### GET /dossiers

**Descrição:**
Lista dossiers com paginação e filtros opcionais. Quando `includeLines=true`, cada dossier inclui as suas linhas no payload.

#### Parâmetros

##### Query Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `docTypeId` | `integer` | Não | Filtra pelo tipo de dossier | |
| `docTypeName` | `string` | Não | Filtra pelo nome do tipo de dossier | |
| `docNumber` | `integer` | Não | Filtra pelo número do dossier | |
| `year` | `integer` | Não | Filtra pelo ano do dossier | Padrão: ano actual |
| `entityId` | `integer` | Não | Filtra pelo identificador da entidade | |
| `entityBranch` | `integer` | Não | Filtra pelo estabelecimento da entidade | |
| `entityName` | `string` | Não | Filtra pelo nome da entidade | |
| `page` | `integer` | Não | Número da página | Padrão: `1` |
| `pageSize` | `integer` | Não | Tamanho da página | Padrão: `20` |
| `includeLines` | `boolean` | Não | Indica se as linhas devem ser incluídas em cada dossier | Padrão: `false` |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/dossiers?year=2026&entityId=58&page=1&pageSize=10&includeLines=true' \
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
      "docTypeId": 45,
      "docTypeName": "Encomenda de Cliente",
      "docNumber": 123,
      "year": 2026,
      "entityId": 58,
      "entityBranch": 0,
      "entityName": "ABC Comércio Lda",
      "date": "2026-04-06",
      "currency": "MT",
      "total": 15000.00,
      "addFields": null,
      "lines": []
    }
  ],
  "meta": {
    "totalItems": 1,
    "itemCount": 1,
    "pageSize": 20,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "href": "/api/dossiers?page=1&pageSize=20", "method": "GET" },
    { "rel": "create", "href": "/api/dossiers", "method": "POST" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de dossiers encontrados |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de dossier |
| `docTypeName` | `string` | Nome descritivo do tipo de dossier |
| `docNumber` | `integer` | Número sequencial do dossier |
| `year` | `integer` | Ano do dossier |
| `entityId` | `integer` | Identificador da entidade (cliente, fornecedor, etc.) |
| `entityBranch` | `integer` | Estabelecimento da entidade |
| `entityName` | `string` | Nome da entidade |
| `date` | `string` | Data do dossier no formato `YYYY-MM-DD` |
| `currency` | `string` | Código da moeda do dossier |
| `total` | `decimal` | Total do dossier calculado automaticamente |
| `addFields` | `object` | Campos adicionais customizados |
| `lines` | `array` | Linhas do dossier (vazio se `includeLines=false`) |

**Dicionário interno de cada item em `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `productCode` | `string` | Referência do produto |
| `productName` | `string` | Nome ou descrição do produto |
| `quantity` | `decimal` | Quantidade da linha |
| `unitPrice` | `decimal` | Preço unitário |
| `vatCode` | `integer` | Código da tabela de IVA |
| `vatRate` | `decimal` | Percentagem de IVA aplicada |
| `vatIncluded` | `boolean` | Indica se o IVA está incluído no preço |
| `total` | `decimal` | Total da linha |

---

### Endpoint: Obter Dossier por Chave

#### GET /dossiers/{docTypeId}/{docNumber}/{year}

**Descrição:**
Obtém um dossier específico pela sua chave composta. A resposta inclui sempre as linhas do dossier.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `docTypeId` | `integer` | Sim | Identificador do tipo de dossier | |
| `docNumber` | `integer` | Sim | Número sequencial do dossier | |
| `year` | `integer` | Sim | Ano do dossier | |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/dossiers/45/123/2026' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "docTypeId": 45,
    "docTypeName": "Encomenda de Cliente",
    "docNumber": 123,
    "year": 2026,
    "entityId": 58,
    "entityBranch": 0,
    "entityName": "ABC Comércio Lda",
    "date": "2026-04-06",
    "currency": "MT",
    "total": 10600.00,
      "addFields": null,
    "lines": [
      {
        "productCode": "P1",
        "productName": "Produto 1",
        "quantity": 2,
        "unitPrice": 5000.00,
        "vatCode": 1,
        "vatRate": 16.0,
        "vatIncluded": false,
        "total": 10000.00
      }
    ]
  },
  "links": [
    { "rel": "self", "href": "/api/dossiers/45/123/2026", "method": "GET" },
    { "rel": "list", "href": "/api/dossiers?page=1&pageSize=20", "method": "GET" },
    { "rel": "delete", "href": "/api/dossiers/45/123/2026", "method": "DELETE" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | `object` | Dados do dossier encontrado |

**Dicionário interno de `item`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de dossier |
| `docTypeName` | `string` | Nome descritivo do tipo de dossier |
| `docNumber` | `integer` | Número sequencial do dossier |
| `year` | `integer` | Ano do dossier |
| `entityId` | `integer` | Identificador da entidade (cliente, fornecedor, etc.) |
| `entityBranch` | `integer` | Estabelecimento da entidade |
| `entityName` | `string` | Nome da entidade |
| `date` | `string` | Data do dossier no formato `YYYY-MM-DD` |
| `currency` | `string` | Código da moeda do dossier |
| `total` | `decimal` | Total do dossier calculado automaticamente |
| `addFields` | `object` | Campos adicionais customizados |
| `lines` | `array` | Linhas do dossier |

**Dicionário interno de cada item em `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `productCode` | `string` | Referência do produto |
| `productName` | `string` | Nome ou descrição do produto |
| `quantity` | `decimal` | Quantidade da linha |
| `unitPrice` | `decimal` | Preço unitário |
| `vatCode` | `integer` | Código da tabela de IVA |
| `vatRate` | `decimal` | Percentagem de IVA aplicada |
| `vatIncluded` | `boolean` | Indica se o IVA está incluído no preço |
| `total` | `decimal` | Total da linha |

**Status Code: 404 - Not Found**

Retornado quando não existe um dossier para a chave composta informada.

---

### Endpoint: Criar Dossier

#### POST /dossiers

**Descrição:**
Cria um novo dossier com cabeçalho e linhas. Campos não fornecidos são auto-resolvidos pela API a partir do catálogo de produtos e configurações do sistema.

#### Parâmetros

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `docTypeId` | `integer` | Sim | Identificador do tipo de dossier | Deve ser maior que `0` |
| `entityId` | `integer` | Sim | Identificador da entidade | Deve ser maior que `0` |
| `year` | `integer` | Não | Ano do dossier | Padrão: ano actual |
| `entityBranch` | `integer` | Não | Estabelecimento da entidade | Padrão: `0` |
| `entityName` | `string` | Não | Nome da entidade no dossier | Auto-resolvido se omitido |
| `date` | `string` | Não | Data do dossier | Formato: `YYYY-MM-DD`. Padrão: data actual |
| `currency` | `string` | Não | Código ISO da moeda | Auto-resolvido da configuração do sistema |
| `lines` | `array` | Sim | Linhas do dossier a criar | Mínimo: `1` item |

##### Estrutura interna de cada item em `lines`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `productCode` | `string` | Sim | Referência do produto |
| `productName` | `string` | Não | Nome ou descrição do produto. Auto-resolvido do catálogo se omitido |
| `quantity` | `decimal` | Sim | Quantidade da linha. Deve ser maior que `0` |
| `unitPrice` | `decimal` | Não | Preço unitário. Auto-resolvido da tabela de preços se omitido |
| `vatCode` | `integer` | Não | Código da tabela de IVA. Auto-resolvido do produto se omitido |
| `vatIncluded` | `boolean` | Não | Indica se o IVA está incluído no preço. Auto-resolvido do produto se omitido |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/dossiers' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "docTypeId": 45,
    "entityId": 58,
    "year": 2026,
    "entityBranch": 0,
    "entityName": "ABC Comércio Lda",
    "date": "2026-04-06",
    "currency": "MT",
    "lines": [
      {
        "productCode": "P1",
        "productName": "Produto 1",
        "quantity": 2.0,
        "unitPrice": 5000.00,
        "vatCode": 1,
        "vatIncluded": false
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
  "message": "Dossier criado com sucesso",
  "data": [
    {
      "docTypeId": 45,
      "docTypeName": "Encomenda de Cliente",
      "docNumber": 3,
      "year": 2026,
      "entityId": 58,
      "entityBranch": 0,
      "entityName": "ABC Comércio Lda",
      "date": "2026-04-06",
      "currency": "MT",
      "total": 10600.00,
      "addFields": null,
      "lines": [
        {
          "productCode": "P1",
          "productName": "Produto 1",
          "quantity": 2.00,
          "unitPrice": 5000.00,
          "vatCode": 1,
          "vatRate": 16.00,
          "vatIncluded": false,
          "total": 10000.00
        }
      ]
    }
  ],
  "links": [
    { "rel": "self", "href": "/api/dossiers/45/3/2026", "method": "GET" },
    { "rel": "list", "href": "/api/dossiers?page=1&pageSize=20", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `array` | Lista com o dossier criado |

**Dicionário interno de cada item em `data`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de dossier |
| `docTypeName` | `string` | Nome descritivo do tipo de dossier |
| `docNumber` | `integer` | Número sequencial do dossier gerado automaticamente |
| `year` | `integer` | Ano do dossier |
| `entityId` | `integer` | Identificador da entidade |
| `entityBranch` | `integer` | Estabelecimento da entidade |
| `entityName` | `string` | Nome da entidade |
| `date` | `string` | Data do dossier no formato `YYYY-MM-DD` |
| `currency` | `string` | Código da moeda do dossier |
| `total` | `decimal` | Total do dossier calculado automaticamente |
| `addFields` | `object` | Campos adicionais customizados |
| `lines` | `array` | Linhas do dossier criado |

**Dicionário interno de cada item em `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `productCode` | `string` | Referência do produto |
| `productName` | `string` | Nome ou descrição do produto |
| `quantity` | `decimal` | Quantidade da linha |
| `unitPrice` | `decimal` | Preço unitário |
| `vatCode` | `integer` | Código da tabela de IVA |
| `vatRate` | `decimal` | Percentagem de IVA aplicada |
| `vatIncluded` | `boolean` | Indica se o IVA está incluído no preço |
| `total` | `decimal` | Total da linha |

**Status Code: 400 - Requisição Inválida**

Retornado quando os dados de entrada falham validação.

**Status Code: 500 - Erro Interno do Servidor**

Retornado quando ocorre falha no processamento ou na integração com o PHC Web.

---

### Endpoint: Criar em Lote
**Em Desenvolvimento**

#### POST /dossiers/bulk

**Descrição:**
Cria múltiplos dossiers numa única requisição. Cada dossier aplica as mesmas regras de auto-resolução que o endpoint de criação individual. Falhas parciais não revertem os itens bem-sucedidos.

#### Parâmetros

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `items` | `array` | Sim | Lista de dossiers a criar | Máximo: `100` itens |

##### Estrutura interna de cada item em `items`

Segue a mesma estrutura que o `POST /dossiers`. Consulte `Body Parameters` e `Estrutura interna de cada item em lines` desse endpoint.

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/dossiers/bulk' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "items": [
      {
        "docTypeId": 45,
        "entityId": 58,
        "lines": [
          { "productCode": "P1", "quantity": 2 },
          { "productCode": "P2", "quantity": 5 }
        ]
      },
      {
        "docTypeId": 45,
        "year": 2026,
        "entityId": 60,
        "date": "2026-04-07",
        "lines": [
          { "productCode": "P1", "quantity": 5 }
        ]
      }
    ]
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado (sucesso completo ou parcial)**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "2 dossier(s) criado(s) com sucesso",
  "data": [
    {
      "index": 0,
      "success": true,
      "data": {
        "docTypeId": 45,
        "docTypeName": "Encomenda de Cliente",
        "docNumber": 3,
        "year": 2026,
        "entityId": 58,
        "entityBranch": 0,
        "entityName": "ABC Comércio Lda",
        "date": "2026-04-06",
        "currency": "MT",
        "total": 10600.00,
      "addFields": null,
        "lines": ["..."]
      },
      "error": null
    },
    {
      "index": 1,
      "success": false,
      "data": null,
      "error": {
        "code": "BO005",
        "message": "Cliente 999/0 não encontrado"
      }
    }
  ],
  "links": [
    { "rel": "list", "href": "/api/dossiers?page=1&pageSize=20", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `array` | Lista com o resultado de cada dossier processado no lote |

**Dicionário interno de cada item em `data`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `index` | `integer` | Índice do item no lote original (base `0`) |
| `success` | `boolean` | `true` se o dossier foi criado com sucesso, `false` se falhou |
| `data` | `object` | Dados do dossier criado (mesma estrutura do `POST /dossiers`). `null` em caso de erro |
| `error` | `object` | Detalhe do erro ocorrido. `null` em caso de sucesso |

**Dicionário interno de `error`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `code` | `string` | Código do erro específico |
| `message` | `string` | Mensagem descritiva do erro |

**Status Code: 400 - Requisição Inválida (falha total)**

Retornado quando todos os itens do lote falham ou o lote é inválido.

---

### Endpoint: Eliminar Dossier
**Em Desenvolvimento**

#### DELETE /dossiers/{docTypeId}/{docNumber}/{year}

**Descrição:**
Elimina permanentemente um dossier e todas as suas linhas associadas. Esta operação não pode ser desfeita.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `docTypeId` | `integer` | Sim | Identificador do tipo de dossier | |
| `docNumber` | `integer` | Sim | Número sequencial do dossier | |
| `year` | `integer` | Sim | Ano do dossier | |

#### Exemplo cURL

```bash
curl -X DELETE 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/dossiers/45/123/2026' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Dossier eliminado com sucesso",
  "data": null,
  "links": [
    { "rel": "list", "href": "/api/dossiers?page=1&pageSize=20", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `null` | Sem payload de dados no sucesso da eliminação |

**Status Code: 404 - Not Found**

Retornado quando não existe um dossier para a chave composta informada.

---

## Códigos de Erro Específicos do Módulo

| Código | Descrição | Quando ocorre |
| :----- | :-------- | :------------ |
| `0000` | Operação concluída com sucesso | Operações concluídas com sucesso |
| `BO001` | Erro ao persistir dossier na base de dados | Falha interna na persistência |
| `BO002` | Já existe dossier com chave tipo/número/ano | Dossier duplicado |
| `BO003` | Já existe dossier com NDos informado | Número de dossier duplicado |
| `BO004` | Dossier não encontrado | Não existe dossier para a chave composta informada |
| `BO005` | Entidade número/estabelecimento não encontrada | Entidade associada não existe (Cliente, Fornecedor, Entidade ou Contacto) |
| `BO006` | Número do Tipo de Dossier inválido | O tipo de dossier não existe |
| `BO007` | Referência de produto não existe | O produto informado não existe no catálogo |
| `BO008` | Quantidade deve ser maior que 0 | A quantidade da linha é menor ou igual a zero |
| `BO009` | Erro de validação | Campo obrigatório em falta ou formato inválido |
| `BO010` | Lote vazio ou inválido | Requisição em lote sem itens |
| `BO011` | Lote não pode conter mais de N itens | Requisição em lote excede o máximo de `100` itens |
| `BO012` | Moeda inválida | O código de moeda não é suportado |
| `BO013` | Código de IVA inválido | O código de IVA informado não existe |
| `BO014` | Erro na integração com PHC Web | Falha ao processar o dossier no PHC Web |
| `BO015` | Lote processado com sucessos e falhas | Sucesso parcial do lote |

---





