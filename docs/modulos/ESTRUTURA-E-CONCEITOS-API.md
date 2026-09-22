# Estrutura e Conceitos da API PHCAPI

## Objetivo
Este documento centraliza as regras globais da API PHCAPI: formatos de resposta, paginação, filtros, links HATEOAS, operações em lote, convenções de dados e práticas para frontend.

As regras aqui descritas aplicam-se a todos os módulos. Quando existir divergência pontual num módulo, o documento do módulo prevalece para esse caso específico.

---

## Estrutura de Resposta
A PHCAPI usa padrões de resposta consistentes por tipo de operação.

### Leituras com coleção (GET de listagem)
Formato padrão com items, meta e links.

```json
{
  "items": [
    {
      "id": 1,
      "name": "Exemplo"
    }
  ],
  "meta": {
    "totalItems": 150,
    "itemCount": 20,
    "pageSize": 20,
    "totalPages": 8,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/resource?page=1&pageSize=20" },
    { "rel": "next", "method": "GET", "href": "/api/resource?page=2&pageSize=20" },
    { "rel": "create", "method": "POST", "href": "/api/resource" }
  ]
}
```

### Leituras de item único (GET por chave)
Formato padrão com item e links.

```json
{
  "item": {
    "id": 1,
    "name": "Exemplo"
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/resource/1" },
    { "rel": "list", "method": "GET", "href": "/api/resource?page=1&pageSize=20" }
  ]
}
```

### Mutações (POST, PUT, PATCH, DELETE)
Formato padrão com code, message, data e links.

```json
{
  "code": "0000",
  "message": "Operacao concluida com sucesso",
  "data": {
    "id": 1,
    "name": "Exemplo"
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/resource/1" },
    { "rel": "list", "method": "GET", "href": "/api/resource" }
  ]
}
```

## Operações em Lote (Bulk)
Para endpoints do tipo POST /{resource}/bulk:

### Request
- Corpo com items (array)
- Máximo recomendado: 100 itens por requisição
- Cada item segue as mesmas regras do POST simples

```json
{
  "items": [
    { "name": "Item 1" },
    { "name": "Item 2" }
  ]
}
```

### Response
Formato de resumo da operação com detalhe por item processado.

```json
{
  "code": "0000",
  "message": "Lote processado: X sucessos, Y falhas",
  "data": [
    {
      "index": 0,
      "success": true,
      "data": { "id": 10, "name": "Item 1" },
      "error": null
    },
    {
      "index": 1,
      "success": false,
      "data": null,
      "error": { "code": "ERR001", "message": "Descricao" }
    }
  ]
}
```

### Campos por item
- index: posição original no array de entrada
- success: resultado do item
- data: objeto criado/atualizado quando success=true
- error: objeto de erro quando success=false

### Comportamento esperado
- Validação item a item
- Falhas parciais não devem impedir avaliação dos restantes itens
- Regras de transação podem variar por módulo (consultar documentação específica)

---

## Campos Dinâmicos (addFields)
O campo addFields permite incluir campos personalizados num recurso, quando o endpoint disponibiliza essa opção.

### Como usar
- O campo é opcional
- Deve ser enviado como objeto chave/valor
- Quando não existirem campos personalizados, o valor deve ser null

### Exemplo
```json
{
  "addFields": {
    "origin": "campanha_abril",
    "channel": "web"
  }
}
```

---

## Links HATEOAS
Cada endpoint pode devolver links de navegação e ações.

### rel mais comuns
- self: recurso atual
- list: listagem do recurso
- next: próxima página
- prev: página anterior
- last: última página
- create: criação
- update: atualização
- delete: remoção

### method
- Indica o verbo HTTP da operação associada ao link

---
