# [Receipts] API - Documentação

## Descrição Geral do Módulo
O módulo **Receipts** gere a emissão e consulta de recibos de cliente no sistema PHC. Um recibo regulariza um ou mais documentos de conta corrente, normalmente facturas, e a criação é tratada pela lógica de negócio do PHC WEB.

> **Nota para organização do Dicionário de Dados Interno e da documentação de frontend:**
> O `Dicionário de Dados Interno` deve priorizar o payload funcional específico do endpoint, isto é, os campos de negócio devolvidos em `item`, `items`, `data` e respetivas estruturas internas.
> Estruturas genéricas e repetidas entre endpoints, como `meta`, `links`, `code` e `message`, não devem poluir o dicionário interno do recurso principal quando não acrescentam detalhe funcional do domínio.
> Sempre que existir um `array` ou `object` dentro de `item`, `items` ou `data`, deve existir também uma secção própria para detalhar exatamente os campos internos dessa estrutura, para que a equipa de frontend consiga ver não só a key principal mas também o dicionário de dados de cada item/subitem.
> Na documentação de frontend, tanto a secção de `Response` como a secção de payload de entrada devem manter blocos colapsáveis.
> Dentro desses colapsáveis deve ficar toda a informação estrutural: campos principais e também o detalhe interno de `array` e `object`, para que a AI de frontend apresente a key e o respetivo conteúdo interno no mesmo agrupamento visual.

---

## Estrutura do Módulo

O módulo Receipts está organizado em subpastas temáticas:

**Padrão de organização no documento:**
- Use `## Subpasta: [Nome]` para cada grupo funcional apresentado na árvore
- Se existirem endpoints que ficam na raiz do módulo e não dentro de uma subpasta temática, use `## Pasta Principal: [Nome do Módulo]`
- Os endpoints devem ser documentados dentro da secção correspondente da subpasta ou da pasta principal, e não todos misturados numa única secção geral

**Com subpastas temáticas:**
```
Receipts
└── Séries de Recibos
    └── GET /receipts/types
└── Pasta Principal: Receipts
    ├── GET /receipts/{ndoc}/{rno}/{reano}
    ├── GET /receipts
    └── POST /receipts
```

**Exemplo de secções no documento:**
```
## Subpasta: Séries de Recibos
[Endpoints desta subpasta]

## Pasta Principal: Receipts
[Endpoints que estão na raiz do módulo]
```

**Nota importante sobre endpoints em desenvolvimento:**
- Endpoints **Em Desenvolvimento** devem estar claramente marcados neste ficheiro com um aviso `Em Desenvolvimento`
- Estes endpoints devem continuar a aparecer neste ficheiro `.md` para referência interna e alinhamento da equipa
- Estes endpoints NÃO devem aparecer na documentação da API na página web

---

## Subpasta: Séries de Recibos

### Endpoint: Listar Séries de Recibos

#### GET /receipts/types
Lista as séries de recibo disponíveis.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
Nenhum.

##### Body Parameters
Nenhum.

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/receipts/types \
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
      "docTypeId": 1,
      "docTypeName": "Recibo"
    },
    {
      "docTypeId": 2,
      "docTypeName": "Recibo Externo"
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
    {
      "rel": "self",
      "href": "/api/receipts/types",
      "method": "GET"
    }
  ]
}
```

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | array | Lista das séries de recibo. |

**Dicionário interno de cada item em `items`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | integer | Identificador da série do recibo. |
| `docTypeName` | string | Nome descritivo da série. |

---

## Pasta Principal: Receipts

### Endpoint: Obter Recibo

#### GET /receipts/{ndoc}/{rno}/{reano}
Recupera um recibo completo pela sua chave composta. Sempre inclui as linhas.

#### Parâmetros

##### Path Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `ndoc` | integer | Sim | Número da série do recibo. |  |
| `rno` | integer | Sim | Número do recibo. |  |
| `reano` | integer | Sim | Ano do recibo. |  |

##### Query Parameters
Nenhum.

##### Body Parameters
Nenhum.

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/receipts/1/101/2026 \
  --request GET \
  --header 'Authorization: Bearer YOUR_TOKEN'
```

#### Respostas (Status Codes)

**Status Code: 200 - OK**

Exemplo de Corpo da Resposta:

```json
{
  "item": {
    "docTypeId": 1,
    "docTypeName": "Recibo",
    "receiptNumber": 101,
    "year": 2026,
    "date": "2026-05-09",
    "clientId": 100,
    "clientName": "Cliente Exemplo Lda.",
    "total": 1250.00,
    "totalForeignCurrency": 0.00,
    "currency": "EUR",
    "bankAccountId": 0,
    "bankAccountName": "Caixa Principal",
    "addFields": null,
    "lines": [
      {
        "invoiceNumber": 5432,
        "invoiceTypeId": "1",
        "docDescription": "Factura FT 1/5432",
        "amountSettled": 1250.00,
        "amountToBeSettled": 0.00,
        "documentDate": "2026-04-01",
        "addFields": null
      }
    ]
  },
  "links": [
    {
      "rel": "self",
      "href": "/api/receipts/1/101/2026",
      "method": "GET"
    },
    {
      "rel": "list",
      "href": "/api/receipts?page=1&pageSize=20",
      "method": "GET"
    }
  ]
}
```

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `item` | object | Dados do recibo. |

**Dicionário interno de `item`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | integer | Identificador da série do recibo. |
| `docTypeName` | string | Nome da série. |
| `receiptNumber` | integer | Número sequencial do recibo. |
| `year` | integer | Ano do recibo. |
| `date` | string | Data de processamento. |
| `clientId` | integer | Identificador do cliente. |
| `clientName` | string | Nome do cliente. |
| `total` | decimal | Total do recibo em moeda base. |
| `totalForeignCurrency` | decimal | Total em moeda estrangeira. |
| `currency` | string | Código ISO da moeda. |
| `bankAccountId` | integer | Identificador da conta bancária ou caixa. |
| `bankAccountName` | string | Nome da conta bancária/caixa. |
| `addFields` | object | Campos adicionais customizados do recibo. |
| `lines` | array | Linhas do recibo. |

**Dicionário interno de cada item em `lines`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `invoiceNumber` | integer | Número da factura regularizada. |
| `invoiceTypeId` | string | Série/tipo da factura regularizada. |
| `docDescription` | string | Descrição do documento regularizado. |
| `amountSettled` | decimal | Valor regularizado. |
| `amountToBeSettled` | decimal | Valor que permanece por regularizar no documento. |
| `documentDate` | string | Data do documento regularizado. |
| `addFields` | object | Campos adicionais customizados da linha. |

---

### Endpoint: Listar Recibos

#### GET /receipts
Lista paginada de recibos com filtros opcionais.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `ndoc` | integer | Não | Filtra pela série do recibo. |  |
| `rno` | integer | Não | Filtra pelo número do recibo. |  |
| `reano` | integer | Não | Filtra pelo ano do recibo. |  |
| `no` | integer | Não | Filtra pelo número do cliente. |  |
| `page` | integer | Não | Número da página. | Default: `1`. |
| `pageSize` | integer | Não | Itens por página. | Default: `20`. |
| `includeLines` | boolean | Não | Inclui as linhas na resposta. | Default: `false`. |

##### Body Parameters
Nenhum.

#### Exemplo cURL
```bash
curl "https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/receipts?no=247&reano=2026&includeLines=true&page=1&pageSize=10" \
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
      "docTypeId": 1,
      "docTypeName": "Recibo",
      "receiptNumber": 101,
      "year": 2026,
      "date": "2026-05-09",
      "clientId": 247,
      "clientName": "Cliente Exemplo Lda.",
      "total": 1250.00,
      "totalForeignCurrency": 0.00,
      "currency": "EUR",
      "bankAccountId": 0,
      "bankAccountName": "Caixa Principal",
      "addFields": null,
      "lines": [
        {
          "invoiceNumber": 5432,
          "invoiceTypeId": "1",
          "docDescription": "Factura FT 1/5432",
          "amountSettled": 1250.00,
          "amountToBeSettled": 0.00,
          "documentDate": "2026-04-01",
          "addFields": null
        }
      ]
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
    {
      "rel": "self",
      "href": "/api/receipts?page=1&pageSize=20",
      "method": "GET"
    },
    {
      "rel": "create",
      "href": "/api/receipts",
      "method": "POST"
    }
  ]
}
```

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `items` | array | Lista paginada de recibos. |

**Dicionário interno de cada item em `items`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | integer | Identificador da série do recibo. |
| `docTypeName` | string | Nome descritivo da série. |
| `receiptNumber` | integer | Número do recibo. |
| `year` | integer | Ano do recibo. |
| `date` | string | Data de processamento. |
| `clientId` | integer | Identificador do cliente. |
| `clientName` | string | Nome do cliente. |
| `total` | decimal | Total do recibo em moeda base. |
| `totalForeignCurrency` | decimal | Total em moeda estrangeira. |
| `currency` | string | Código ISO da moeda. |
| `bankAccountId` | integer | Identificador da conta bancária ou caixa. |
| `bankAccountName` | string | Nome da conta bancária/caixa. |
| `addFields` | object | Campos adicionais customizados do recibo. |
| `lines` | array | Linhas do recibo. |

**Dicionário interno de cada item em `lines`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `invoiceNumber` | integer | Número da factura regularizada. |
| `invoiceTypeId` | string | Série/tipo da factura regularizada. |
| `docDescription` | string | Descrição do documento regularizado. |
| `amountSettled` | decimal | Valor regularizado. |
| `amountToBeSettled` | decimal | Valor que permanece por regularizar no documento. |
| `documentDate` | string | Data do documento regularizado. |
| `addFields` | object | Campos adicionais customizados da linha. |

---

### Endpoint: Criar Recibo

#### POST /receipts
Cria um novo recibo, regularizando uma ou mais facturas de um cliente. O código de tesouraria, local e data de processamento são resolvidos internamente pelo PHC WEB.

#### Parâmetros

##### Path Parameters
Nenhum.

##### Query Parameters
Nenhum.

##### Body Parameters
| Parâmetro | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :-------- | :--- | :---------- | :-------- | :---------------- |
| `docTypeId` | integer | Sim | Série do recibo. | Deve existir em `tsre`. |
| `clientId` | integer | Sim | Número do cliente. | Deve existir em `cl`. |
| `bankAccountId` | integer | Sim | Conta bancária ou caixa. | `0` para caixa principal. |
| `addFields` | object | Não | Campos adicionais customizados do recibo. | Chaves dinâmicas. |
| `lines` | array | Sim | Linhas de regularização. | Pelo menos um item. |

##### Estrutura interna de `lines`
| Campo | Tipo | Obrigatório | Descrição | Restrições/Padrão |
| :---- | :--- | :---------- | :-------- | :---------------- |
| `invoiceNumber` | integer | Sim | Número da factura a regularizar. |  |
| `invoiceTypeId` | integer | Sim | Série da factura. |  |
| `invoiceYear` | integer | Sim | Ano da factura. |  |
| `amount` | decimal | Sim | Valor a regularizar. | Maior que zero. |
| `addFields` | object | Não | Campos adicionais customizados da linha. | Chaves dinâmicas. |

#### Exemplo cURL
```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/receipts \
  --request POST \
  --header 'Content-Type: application/json' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --data '{
    "docTypeId": 1,
    "clientId": 2,
    "bankAccountId": 1,
    "addFields": null
    "lines": [
      {
        "invoiceNumber": 5,
        "invoiceTypeId": 1,
        "invoiceYear": 2026,
        "amount": 2500.00,
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
  "message": "Recibo criado com sucesso",
  "data": {
    "clientId": 2,
    "bankAccountId": 1,
    "totalCount": 1,
    "documents": {
      "receipt": {
        "docTypeId": 1,
        "receiptNumber": 101,
        "year": 2026,
        "total": 2500.00,
        "addFields": null,
        "lines": [
          {
            "invoiceNumber": 5,
            "invoiceYear": 2026,
            "docDescription": "Factura FT 1/5",
            "amountSettled": 2500.00,
            "amountTotal": 2500.00,
            "addFields": null
          }
        ]
      },
      "advances": [
        {
          "docTypeId": 3,
          "receiptNumber": 15,
          "total": 100.00,
          "invoiceNumber": 5,
          "invoiceYear": 2026
        }
      ]
    }
  },
  "links": [
    {
      "rel": "self",
      "href": "/api/receipts/1/101/2026",
      "method": "GET"
    },
    {
      "rel": "list",
      "href": "/api/receipts?page=1&pageSize=20",
      "method": "GET"
    }
  ]
}
```

#### Dicionário de Dados da Resposta

**Dicionário de Dados Interno**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `data` | object | Resultado da criação do recibo. |

**Dicionário interno de `data`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `clientId` | integer | Identificador do cliente. |
| `bankAccountId` | integer | Conta bancária/caixa usada na criação. |
| `totalCount` | integer | Total de documentos gerados. |
| `documents` | object | Documentos gerados pelo processo. |

**Dicionário interno de `documents`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `receipt` | object | Documento de recibo principal. |
| `advances` | array | Adiantamentos gerados. |

**Dicionário interno de `receipt`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | integer | Série do recibo. |
| `receiptNumber` | integer | Número do recibo. |
| `year` | integer | Ano do recibo. |
| `total` | decimal | Total regularizado. |
| `addFields` | object | Campos adicionais customizados do recibo. |
| `lines` | array | Linhas do recibo gerado. |

**Dicionário interno de cada item em `lines`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `invoiceNumber` | integer | Número da factura. |
| `invoiceYear` | integer | Ano da factura. |
| `docDescription` | string | Descrição da factura. |
| `amountSettled` | decimal | Valor efectivamente regularizado. |
| `amountTotal` | decimal | Valor total do documento. |
| `addFields` | object | Campos adicionais customizados da linha. |

**Dicionário interno de cada item em `advances`**
| Campo | Tipo | Descrição |
| :---- | :--- | :-------- |
| `docTypeId` | integer | Série do adiantamento. |
| `receiptNumber` | integer | Número do adiantamento. |
| `total` | decimal | Total do adiantamento. |
| `invoiceNumber` | integer | Factura de origem. |
| `invoiceYear` | integer | Ano da factura de origem. |

---

## Códigos de Erro Específicos do Módulo

| Código | Descrição | Quando ocorre |
| :---- | :-------- | :------------ |
| `0000` | Operação concluída com sucesso | Criação ou consulta bem-sucedidas. |
| `RE001` | Recibo não encontrado | O recibo solicitado não existe. |
| `RE002` | Série de recibo inválida: {ndoc} | A série indicada não existe em `tsre`. |
| `RE003` | Cliente {no} não encontrado | O cliente indicado não existe em `cl`. |
| `RE006` | O valor a regularizar deve ser maior que zero | O valor enviado em `amount` é inválido. |
| `RE007` | O recibo deve ter pelo menos uma linha de regularização | O array `lines` veio vazio. |
| `RE008` | Erro de validação | Falhou uma validação de campos. |
| `RE009` | Erro na integração com PHC WEB: {message} | Falha ao processar o recibo no PHC WEB. |
