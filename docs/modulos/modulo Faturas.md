# Faturas API - Documentação

## Descrição Geral do Módulo
Endpoints para consulta e criação de faturas, incluindo tipos de documento, listagem paginada, detalhe por chave composta e criação com linhas.

> **Nota para leitura do Dicionário de Dados Interno e para a AI de frontend:**
> Nesta documentação, o `Dicionário de Dados Interno` deve destacar sobretudo os campos funcionais do recurso devolvido em `item`, `items` ou `data`.
> Estruturas genéricas e transversais, como `meta`, `links`, `code` e `message`, não devem competir com o dicionário do recurso principal quando não trazem detalhe funcional do domínio.
> Sempre que existir uma key do tipo `array` ou `object` no payload funcional, a documentação deve incluir também a secção que detalha exatamente o que existe dentro dessa estrutura, para que no frontend não se veja apenas a key sem o dicionário dos itens/subitens.
> A AI de frontend deve manter esse conteúdo dentro de secções colapsáveis: um colapsável para `Response` e, nos endpoints com body, outro colapsável para o payload de entrada.
> Dentro de cada colapsável deve ficar toda a informação estrutural correspondente, incluindo a tabela principal e o detalhe interno de arrays, objetos e subitens.

> **Base URL:** os exemplos abaixo usam paths relativos (`/invoices`, `/invoices/types`, etc.).
> O prefixo `/api` faz parte da base URL da API e não é repetido nos exemplos.

> **Nota sobre endpoints em desenvolvimento:**
> Endpoints ainda em desenvolvimento continuam presentes neste ficheiro `.md` para referência interna.
> Estes endpoints não devem aparecer na documentação da API na página web.

---

## Estrutura do Módulo

O módulo Faturas está organizado no frontend em subpastas temáticas:

```text
Faturas
├── Séries de Facturação
│   └── Listar Tipos (GET /invoices/types)
└── Faturas
  ├── Listar Faturas (GET /invoices)
  ├── Obter Fatura por Chave (GET /invoices/{ndoc}/{invoiceNumber}/{year})
  ├── Criar Fatura (POST /invoices)
  └── Eliminar Fatura (DELETE /invoices/{ndoc}/{invoiceNumber}/{year})
```

---

## Subpasta: Séries de Facturação

### Endpoint: Listar Tipos

#### GET /invoices/types

**Descrição:**
Lista os tipos de documento de faturação disponíveis no sistema.

#### Parâmetros

Não aplicável.

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/invoices/types' \
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
      "docTypeId": 1,
      "docTypeName": "Série A"
    },
    {
      "docTypeId": 21,
      "docTypeName": "Venda a Dinheiro"
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
    { "rel": "self", "href": "/api/invoices/types", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de tipos de documento |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de documento |
| `docTypeName` | `string` | Nome do tipo de documento |

---

## Pasta Principal: Faturas

### Endpoint: Listar Faturas

#### GET /invoices

**Descrição:**
Lista faturas com paginação e filtros opcionais. Quando `includeLines=true`, cada fatura inclui as suas linhas no payload.

#### Parâmetros

##### Query Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `ndoc` | `integer` | Não | Filtra pelo tipo de documento | |
| `invoiceNumber` | `integer` | Não | Filtra pelo número da fatura | |
| `year` | `integer` | Não | Filtra pelo ano da fatura | |
| `clientNumber` | `integer` | Não | Filtra pelo número do cliente | |
| `page` | `integer` | Não | Número da página | Padrão: `1` |
| `pageSize` | `integer` | Não | Tamanho da página | Padrão: `20` |
| `includeLines` | `boolean` | Não | Indica se as linhas devem ser incluídas em cada fatura | Padrão: `false` |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/invoices?year=2026&clientNumber=2&page=1&pageSize=10&includeLines=true' \
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
      "docTypeId": 21,
      "docTypeName": "Venda a Dinheiro",
      "invoiceNumber": 3,
      "year": 2026,
      "clientId": 2,
      "clientName": "Compra Tudo, Lda",
      "clientBranch": 0,
      "date": "2026-04-16",
      "currency": "MZN",
      "totalTax": 320.0,
      "totalTaxForeignCurrency": 0.0,
      "total": 2320.0,
      "totalForeignCurrency": 0.0,
      "notes": "",
      "addFields": null,
      "lines": [
        {
          "productCode": "S1",
          "productName": "Serviço",
          "quantity": 2.0,
          "unitPrice": 1000.0,
          "unitPriceForeignCurrency": 0.0,
          "vatCode": 2,
          "vatRate": 16.0,
          "vatIncluded": false,
          "total": 2000.0,
          "totalForeignCurrency": 0.0,
          "warehouse": 1,
          "batch": "",
          "addFields": null
        }
      ]
    }
  ],
  "meta": {
    "totalItems": 1,
    "itemCount": 1,
    "pageSize": 10,
    "totalPages": 1,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "href": "/api/invoices?page=1&pageSize=10", "method": "GET" },
    { "rel": "create", "href": "/api/invoices", "method": "POST" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | `array` | Lista de faturas encontradas |

**Dicionário interno de cada item em `items`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de documento |
| `docTypeName` | `string` | Nome do tipo de documento |
| `invoiceNumber` | `integer` | Número sequencial da fatura |
| `year` | `integer` | Ano da fatura |
| `clientId` | `integer` | Número do cliente |
| `clientName` | `string` | Nome do cliente |
| `clientBranch` | `integer` | Estabelecimento do cliente |
| `date` | `string` | Data da fatura no formato `YYYY-MM-DD` |
| `currency` | `string` | Código da moeda da fatura |
| `totalTax` | `decimal` | Total de IVA da fatura |
| `totalTaxForeignCurrency` | `decimal` | Total de IVA em moeda estrangeira |
| `total` | `decimal` | Total final da fatura |
| `totalForeignCurrency` | `decimal` | Total final em moeda estrangeira |
| `notes` | `string` | Observações da fatura |
| `addFields` | `object` | Campos adicionais de cabeçalho |
| `lines` | `array` | Linhas da fatura |

**Dicionário interno de `addFields` no cabeçalho da fatura**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Valor de um campo adicional configurado para o cabeçalho da fatura |

**Dicionário interno de cada item em `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `productCode` | `string` | Referência do produto |
| `productName` | `string` | Nome ou descrição do produto |
| `quantity` | `decimal` | Quantidade da linha |
| `unitPrice` | `decimal` | Preço unitário |
| `unitPriceForeignCurrency` | `decimal` | Preço unitário em moeda estrangeira |
| `vatCode` | `integer` | Código da tabela de IVA |
| `vatRate` | `decimal` | Percentagem de IVA aplicada |
| `vatIncluded` | `boolean` | Indica se o IVA está incluído no preço |
| `total` | `decimal` | Total da linha |
| `totalForeignCurrency` | `decimal` | Total da linha em moeda estrangeira |
| `warehouse` | `integer` | Número do armazém |
| `batch` | `string` | Número do lote |
| `addFields` | `object` | Campos adicionais configurados para a linha |

**Dicionário interno de `addFields` em cada item de `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Valor de um campo adicional configurado para a linha da fatura |

---

### Endpoint: Obter Fatura por Chave

#### GET /invoices/{ndoc}/{invoiceNumber}/{year}

**Descrição:**
Obtém uma fatura específica pela sua chave composta. A resposta inclui sempre as linhas da fatura.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `ndoc` | `integer` | Sim | Identificador do tipo de documento |
| `invoiceNumber` | `integer` | Sim | Número sequencial da fatura |
| `year` | `integer` | Sim | Ano da fatura |

#### Exemplo cURL

```bash
curl -X GET 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/invoices/21/3/2026' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "docTypeId": 21,
    "docTypeName": "Venda a Dinheiro",
    "invoiceNumber": 3,
    "year": 2026,
    "clientId": 2,
    "clientName": "Compra Tudo, Lda",
    "clientBranch": 0,
    "date": "2026-04-16",
    "currency": "MZN",
    "totalTax": 320.0,
    "totalTaxForeignCurrency": 0.0,
    "total": 2320.0,
    "totalForeignCurrency": 0.0,
    "notes": "",
    "addFields": null,
    "lines": [
      {
        "productCode": "S1",
        "productName": "Serviço",
        "quantity": 2.0,
        "unitPrice": 1000.0,
        "unitPriceForeignCurrency": 0.0,
        "vatCode": 2,
        "vatRate": 16.0,
        "vatIncluded": false,
        "total": 2000.0,
        "totalForeignCurrency": 0.0,
        "warehouse": 1,
        "batch": "",
        "addFields": null
      }
    ]
  },
  "links": [
    { "rel": "self", "href": "/api/invoices/21/3/2026", "method": "GET" },
    { "rel": "list", "href": "/api/invoices?page=1&pageSize=20", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | `object` | Dados da fatura encontrada |

**Dicionário interno de `item`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de documento |
| `docTypeName` | `string` | Nome do tipo de documento |
| `invoiceNumber` | `integer` | Número sequencial da fatura |
| `year` | `integer` | Ano da fatura |
| `clientId` | `integer` | Número do cliente |
| `clientName` | `string` | Nome do cliente |
| `clientBranch` | `integer` | Estabelecimento do cliente |
| `date` | `string` | Data da fatura no formato `YYYY-MM-DD` |
| `currency` | `string` | Código da moeda da fatura |
| `totalTax` | `decimal` | Total de IVA da fatura |
| `totalTaxForeignCurrency` | `decimal` | Total de IVA em moeda estrangeira |
| `total` | `decimal` | Total final da fatura |
| `totalForeignCurrency` | `decimal` | Total final em moeda estrangeira |
| `notes` | `string` | Observações da fatura |
| `addFields` | `object` | Campos adicionais de cabeçalho |
| `lines` | `array` | Linhas da fatura |

**Dicionário interno de `addFields` no cabeçalho da fatura**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Valor de um campo adicional configurado para o cabeçalho da fatura |

**Dicionário interno de cada item em `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `productCode` | `string` | Referência do produto |
| `productName` | `string` | Nome ou descrição do produto |
| `quantity` | `decimal` | Quantidade da linha |
| `unitPrice` | `decimal` | Preço unitário |
| `unitPriceForeignCurrency` | `decimal` | Preço unitário em moeda estrangeira |
| `vatCode` | `integer` | Código da tabela de IVA |
| `vatRate` | `decimal` | Percentagem de IVA aplicada |
| `vatIncluded` | `boolean` | Indica se o IVA está incluído no preço |
| `total` | `decimal` | Total da linha |
| `totalForeignCurrency` | `decimal` | Total da linha em moeda estrangeira |
| `warehouse` | `integer` | Número do armazém |
| `batch` | `string` | Número do lote |
| `addFields` | `object` | Campos adicionais configurados para a linha |

**Dicionário interno de `addFields` em cada item de `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Valor de um campo adicional configurado para a linha da fatura |

**Status Code: 404 - Not Found**

Retornado quando não existe uma fatura para a chave composta informada.

---

### Endpoint: Criar Fatura

#### POST /invoices

**Descrição:**
Cria uma nova fatura com cabeçalho e linhas. Campos não fornecidos podem ser auto-resolvidos pela API ou pelo PHC Web.

#### Parâmetros

##### Body Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `docTypeId` | `integer` | Sim | Identificador do tipo de documento | Deve ser maior que `0` |
| `clientId` | `integer` | Sim | Número do cliente | Deve ser maior que `0` |
| `year` | `integer` | Não | Ano da fatura | Padrão: ano actual |
| `clientBranch` | `integer` | Não | Estabelecimento do cliente | Padrão: `0` |
| `clientName` | `string` | Não | Nome do cliente na fatura | Máximo: `80` caracteres |
| `currency` | `string` | Não | Código ISO da moeda | Máximo: `3` caracteres |
| `date` | `string` | Não | Data da fatura | Formato: `YYYY-MM-DD` |
| `lines` | `array` | Sim | Linhas da fatura a criar | Mínimo: `1` item |
| `notes` | `string` | Não | Observações gerais | Máximo: `500` caracteres |
| `addFields` | `object` | Não | Campos adicionais de cabeçalho | |

##### Estrutura interna de cada item em `lines`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `productCode` | `string` | Sim | Referência do produto |
| `productName` | `string` | Não | Nome ou descrição do produto |
| `quantity` | `decimal` | Sim | Quantidade da linha |
| `warehouse` | `integer` | Não | Número do armazém |
| `unitPrice` | `decimal` | Não | Preço unitário |
| `vatCode` | `integer` | Não | Código da tabela de IVA |
| `vatIncluded` | `boolean` | Não | Indica se o IVA está incluído no preço |
| `addFields` | `object` | Não | Campos adicionais configurados para a linha |
| `batch` | `string` | Não | Número do lote |
| `serialNumber` | `string` | Não | Número de série |

##### Estrutura interna de `addFields`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Não | Valor de um campo adicional configurado para o cabeçalho da fatura |

##### Estrutura interna de `addFields` em cada item de `lines`

| Campo | Tipo | Obrigatório | Descrição |
| :---- | :--- | :---------- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Não | Valor de um campo adicional configurado para a linha da fatura |

#### Exemplo cURL

```bash
curl -X POST 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/invoices' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json' \
  --data '{
    "docTypeId": 21,
    "clientId": 2,
    "year": 2026,
    "clientBranch": 0,
    "clientName": "Compra Tudo, Lda",
    "currency": "MZN",
    "date": "2026-04-16",
    "lines": [
      {
        "productCode": "S1",
        "quantity": 2.0,
        "productName": "Serviço",
        "unitPrice": 1000.0,
        "vatCode": 2,
        "vatIncluded": false,
        "addFields": null,
        "warehouse": 1,
        "batch": "",
        "serialNumber": ""
      }
    ],
    "notes": "Observações gerais",
    "addFields": null
  }'
```

#### Respostas (Status Codes)

**Status Code: 201 - Criado**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Fatura criada com sucesso",
  "data": [
    {
      "docTypeId": 21,
      "docTypeName": "Venda a Dinheiro",
      "invoiceNumber": 22,
      "year": 2026,
      "clientId": 2,
      "clientName": "Compra Tudo, Lda",
      "clientBranch": 0,
      "date": "2026-04-16",
      "currency": "MZN",
      "totalTax": 320.0,
      "totalTaxForeignCurrency": 0.0,
      "total": 2320.0,
      "totalForeignCurrency": 0.0,
      "notes": "Observações gerais",
      "addFields": null,
      "lines": [
        {
          "productCode": "S1",
          "productName": "Serviço",
          "quantity": 2.0,
          "unitPrice": 1000.0,
          "unitPriceForeignCurrency": 0.0,
          "vatCode": 2,
          "vatRate": 16.0,
          "vatIncluded": false,
          "total": 2320.0,
          "totalForeignCurrency": 0.0,
          "warehouse": 1,
          "batch": "",
          "addFields": null
        }
      ]
    }
  ],
  "links": [
    { "rel": "self", "href": "/api/invoices/21/22/2026", "method": "GET" },
    { "rel": "list", "href": "/api/invoices?page=1&pageSize=20", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `array` | Lista com a fatura criada |

**Dicionário interno de cada item em `data`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | `integer` | Identificador do tipo de documento |
| `docTypeName` | `string` | Nome do tipo de documento |
| `invoiceNumber` | `integer` | Número sequencial da fatura criada |
| `year` | `integer` | Ano da fatura |
| `clientId` | `integer` | Número do cliente |
| `clientName` | `string` | Nome do cliente |
| `clientBranch` | `integer` | Estabelecimento do cliente |
| `date` | `string` | Data da fatura no formato `YYYY-MM-DD` |
| `currency` | `string` | Código da moeda da fatura |
| `totalTax` | `decimal` | Total de IVA da fatura |
| `totalTaxForeignCurrency` | `decimal` | Total de IVA em moeda estrangeira |
| `total` | `decimal` | Total final da fatura |
| `totalForeignCurrency` | `decimal` | Total final em moeda estrangeira |
| `notes` | `string` | Observações da fatura |
| `addFields` | `object` | Campos adicionais de cabeçalho |
| `lines` | `array` | Linhas da fatura |

**Dicionário interno de `addFields` no cabeçalho da fatura**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Valor de um campo adicional configurado para o cabeçalho da fatura |

**Dicionário interno de cada item em `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `productCode` | `string` | Referência do produto |
| `productName` | `string` | Nome ou descrição do produto |
| `quantity` | `decimal` | Quantidade da linha |
| `unitPrice` | `decimal` | Preço unitário |
| `unitPriceForeignCurrency` | `decimal` | Preço unitário em moeda estrangeira |
| `vatCode` | `integer` | Código da tabela de IVA |
| `vatRate` | `decimal` | Percentagem de IVA aplicada |
| `vatIncluded` | `boolean` | Indica se o IVA está incluído no preço |
| `total` | `decimal` | Total da linha |
| `totalForeignCurrency` | `decimal` | Total da linha em moeda estrangeira |
| `warehouse` | `integer` | Número do armazém |
| `batch` | `string` | Número do lote |
| `addFields` | `object` | Campos adicionais configurados para a linha |

**Dicionário interno de `addFields` em cada item de `lines`**

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `[aliasDoCampo]` | `string \| number \| boolean \| null` | Valor de um campo adicional configurado para a linha da fatura |

**Status Code: 400 - Requisição Inválida**

Retornado quando os dados de entrada falham validação.

**Status Code: 500 - Erro Interno do Servidor**

Retornado quando ocorre falha no processamento da fatura ou na integração com o PHC Web.

---

### Endpoint: Eliminar Fatura
**Em Desenvolvimento**

#### DELETE /invoices/{ndoc}/{invoiceNumber}/{year}

**Descrição:**
Elimina uma fatura específica pela sua chave composta.

#### Parâmetros

##### Path Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `ndoc` | `integer` | Sim | Identificador do tipo de documento |
| `invoiceNumber` | `integer` | Sim | Número sequencial da fatura |
| `year` | `integer` | Sim | Ano da fatura |

#### Exemplo cURL

```bash
curl -X DELETE 'https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/invoices/21/22/2026' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --header 'Content-Type: application/json'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "Fatura eliminada com sucesso",
  "data": null,
  "links": [
    { "rel": "list", "href": "/api/invoices?page=1&pageSize=20", "method": "GET" }
  ]
}
```

#### Dicionário de Dados Interno

| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | `null` | Sem payload de dados no sucesso da eliminação |

**Status Code: 404 - Not Found**

Retornado quando não existe uma fatura para a chave composta informada.

---


## Códigos de Erro Específicos do Módulo

| Código | Descrição | Quando ocorre |
| :----- | :-------- | :------------ |
| `0000` | Operação concluída com sucesso | Operações concluídas com sucesso |
| `FT001` | Validação falhou | Campo obrigatório em falta, formato inválido ou regra de negócio violada |
| `FT002` | Fatura não encontrada | Não existe fatura para a chave composta informada |
| `FT003` | Não existe uma série de facturação com ID informado | O tipo de documento não existe |
| `FT004` | Cliente não encontrado | O cliente informado não existe no PHC |
| `FT005` | Referência de produto inválida | O produto informado não existe no catálogo |
| `FT006` | Quantidade deve ser maior que zero | A quantidade da linha é menor ou igual a zero |
| `FT007` | Código de IVA inválido | O código de IVA informado não existe |
| `FT008` | Moeda inválida | O código de moeda não é suportado |
| `FT009` | Formato de data inválido | A data não está no formato esperado |
| `FT010` | Formato de hora inválido | A hora não está no formato esperado |
| `FT011` | Já existe fatura com a chave informada | Foi detectada tentativa de criar uma fatura duplicada |
| `FT012` | Erro ao processar no PHC Web | Ocorreu falha na integração ou processamento interno |
| `FT013` | Armazém inválido | O armazém informado não existe |
| `FT014` | Condição de pagamento inválida | A condição de pagamento não existe |
| `FT015` | Nenhuma linha fornecida | O array `lines` foi enviado vazio |
| `FT999` | Erro interno do servidor | Ocorreu um erro não identificado |
