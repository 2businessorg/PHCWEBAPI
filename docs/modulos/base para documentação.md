# [Nome do Módulo] API - Documentação

## Descrição Geral do Módulo
[Uma breve descrição sobre o que este módulo gerencia (e.g., "Endpoints para criação, consulta, atualização e remoção de [entidade]. Suporta operações individuais e em lote.")]

> **Nota para organização do Dicionário de Dados Interno e da documentação de frontend:**
> O `Dicionário de Dados Interno` deve priorizar o payload funcional específico do endpoint, isto é, os campos de negócio devolvidos em `item`, `items`, `data` e respetivas estruturas internas.
> Estruturas genéricas e repetidas entre endpoints, como `meta`, `links`, `code` e `message`, não devem poluir o dicionário interno do recurso principal quando não acrescentam detalhe funcional do domínio.
> Sempre que existir um `array` ou `object` dentro de `item`, `items` ou `data`, deve existir também uma secção própria para detalhar exatamente os campos internos dessa estrutura, para que a equipa de frontend consiga ver não só a key principal mas também o dicionário de dados de cada item/subitem.
> Na documentação de frontend, tanto a secção de `Response` como a secção de payload de entrada devem manter blocos colapsáveis.
> Dentro desses colapsáveis deve ficar toda a informação estrutural: campos principais e também o detalhe interno de `array` e `object`, para que a AI de frontend apresente a key e o respetivo conteúdo interno no mesmo agrupamento visual.

---

## Estrutura do Módulo

O módulo [Nome] está organizado em subpastas temáticas (se aplicável) ou em operações principais:

**Padrão de organização no documento:**
- Use `## Subpasta: [Nome]` para cada grupo funcional apresentado na árvore
- Se existirem endpoints que ficam na raiz do módulo e não dentro de uma subpasta temática, use `## Pasta Principal: [Nome do Módulo]`
- Os endpoints devem ser documentados dentro da secção correspondente da subpasta ou da pasta principal, e não todos misturados numa única secção geral

**Com subpastas temáticas:**
```
[Nome do Módulo]
├── [Subpasta 1]
│   ├── Operação 1 (GET /endpoint1)
│   └── Operação 2 (GET /endpoint2/{id})
├── [Subpasta 2]
│   ├── Operação 3 (POST /endpoint3)
│   └── Operação 4 (PATCH /endpoint4/{id})
├── Operação 5 (GET /endpoint5)
└── Operação 6 (POST /endpoint6/bulk)
```

**Exemplo de secções no documento:**
```
## Subpasta: [Subpasta 1]
[Endpoints desta subpasta]

## Subpasta: [Subpasta 2]
[Endpoints desta subpasta]

## Pasta Principal: [Nome do Módulo]
[Endpoints que estão na raiz do módulo]
```

**Sem subpastas (operações simples):**
```
[Nome do Módulo]
├── Listar [Entidade] (GET /endpoint)
├── Obter [Entidade] (GET /endpoint/{id})
├── Criar [Entidade] (POST /endpoint)
├── Atualizar [Entidade] (PATCH /endpoint/{id})
├── Eliminar [Entidade] (DELETE /endpoint/{id})
└── Criar em Lote (POST /endpoint/bulk)
```

**Nota importante sobre endpoints em desenvolvimento:**
- Endpoints **Em Desenvolvimento** devem estar claramente marcados neste ficheiro com um aviso `Em Desenvolvimento`
- Estes endpoints devem continuar a aparecer neste ficheiro `.md` para referência interna e alinhamento da equipa
- Estes endpoints NÃO devem aparecer na documentação da API na página web

---

## Endpoint: [Nome da Operação]

### [Método HTTP] [Caminho do Endpoint]
(e.g., GET /products/{id})

**Descrição:**
[Uma descrição detalhada do que este endpoint faz. e.g., "Retorna uma lista paginada de todos os produtos."]

---

#### Parâmetros


##### Path Parameters (se aplicável)
[Para parâmetros que fazem parte do URL, como `{id}` ou `{branch?}`]

| Parâmetro | Tipo      | Obrigatório | Descrição                                  | Restrições/Padrão |
| :-------- | :-------- | :---------- | :----------------------------------------- | :---------------- |
| `id`      | `integer` | Sim         | Número identificador do [entidade].        |                   |
| `branch`  | `integer` | Não         | Estabelecimento do [entidade]. Default: `0`. |                   |
| `...`     | `...`     | `...`       | `...`                                      | `...`             |

##### Query Parameters (se aplicável)
[Para parâmetros na string de consulta, como `?page=1`]

| Parâmetro  | Tipo      | Obrigatório | Descrição                              | Restrições/Padrão              |
| :--------- | :-------- | :---------- | :------------------------------------- | :----------------------------- |
| `page`     | `integer` | Não         | Número da página. Default: `1`.        | Valor mínimo: `1`.             |
| `pageSize` | `integer` | Não         | Itens por página. Default: `20`.       | Valor mínimo: `1`, máximo: `100`. |
| `name`     | `string`  | Não         | Filtra por nome.                       |                                |
| `...`      | `...`     | `...`       | `...`                                  | `...`                          |

##### Body Parameters (se aplicável - para POST/PATCH/PUT)
[Para parâmetros enviados no corpo da requisição, geralmente JSON]

| Parâmetro  | Tipo      | Obrigatório | Descrição                                  | Restrições/Padrão |
| :-------- | :-------- | :---------- | :----------------------------------------- | :---------------- |
| `name`    | `string`  | Sim         | Nome do [entidade].                        |                   |
| `nuit`    | `string`  | Sim         | NUIT do [entidade].                        | `9 caracteres`, deve ser único. |
| `address` | `string`  | Não         | Endereço / morada.                         |                   |
| `...`     | `...`     | `...`       | `...`                                      | `...`             |

##### Estrutura interna de `[array_field]` (se aplicável)
[Para Body Parameters que contêm arrays ou objetos aninhados, documente a estrutura interna]

| Campo  | Tipo      | Obrigatório | Descrição                |
| :----- | :-------- | :---------- | :----------------------- |
| `name` | `string`  | Sim         | Nome do item             |
| `...`  | `...`     | `...`       | `...`                    |

#### Exemplo cURL
[O comando cURL completo para esta operação. Se for um POST/PATCH, inclua o corpo JSON.]

```bash
curl https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api/[modulo]/[id]/[branch] \
  --request [METHOD] \
  --header 'Content-Type: application/json' \
  --header 'Authorization: Bearer YOUR_TOKEN' \
  --data '{
    "field": "value"
  }'
```

#### Respostas (Status Codes)

**Status Code: [Código HTTP] - [Breve descrição]**
(e.g., 200 - OK, 201 - Criado, 400 - Requisição Inválida, 403 - Sem Permissão, 404 - Não Encontrado)

Exemplo de Corpo da Resposta:

```json
{
  "code": "0000",
  "message": "[Mensagem de sucesso/erro]",
  "data": [
    {
      "id": 1,
      "name": "Exemplo de Cliente",
      "..." : "...",
      "addFields" : null
    }
  ],
  "meta": {
    "totalItems": 150,
    "pageSize": 20,
    "totalPages": 8,
    "currentPage": 1
  },
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/[modulo]?page=1" }
  ]
}
```

#### Dicionário de Dados da Resposta

Use esta secção para detalhar a estrutura do payload retornado. Quando aplicável, inclua logo abaixo um dicionário de dados interno no formato `Campo | Tipo | Descrição`.

**Regra de documentação para Respostas:**
- Cada resposta (GET, POST, PATCH) que retorna um objeto deve ter um dicionário de dados listando os campos funcionais do payload principal
- O foco do `Dicionário de Dados Interno` deve estar no conteúdo de `item`, `items` ou `data`, conforme a estrutura da resposta
- Se a resposta contiver um **array** dentro de `item`, `items` ou `data`, este é listado na tabela principal como tipo `array`, e cria uma **subtabela** "Dicionário interno de cada item em `[nome_do_array]`" logo abaixo
- Se a resposta contiver um **objeto aninhado** (não array) dentro de `item`, `items` ou `data`, crie uma subtabela "Dicionário interno de `[nome_do_objeto]`"
- Estruturas genéricas repetidas entre endpoints, como `meta`, `links`, `code` e `message`, só devem ser detalhadas quando houver necessidade real de clarificação; por defeito, não devem ocupar o dicionário interno do recurso principal
- **Não duplicar** campos já documentados - cada array ou subestrutura é documentado apenas uma vez
- Se o frontend mostrar apenas a key de um `array` ou `object`, a documentação deve complementar isso com a subtabela respetiva para expor o dicionário interno dos itens/subitens
- Na UI de frontend, toda a informação da resposta deve ficar dentro do mesmo bloco colapsável da resposta, incluindo o dicionário interno dos arrays/subitens

**Exemplo:**
```
#### Dicionário de Dados Interno
| Campo    | Tipo       | Descrição          |
| id       | integer    | Identificador      |
| prices   | array      | Lista de preços    |

**Dicionário interno de cada item em `prices`**
| Campo | Tipo | Descrição |
| value | decimal | Valor do preço |
```

---

**Regra de documentação para Pedidos (POST/PATCH) com arrays/objetos:**
- Para Body Parameters que contêm um **array** ou **objeto**, documente a sua estrutura interna com uma **subtabela** "Estrutura interna de `[nome_do_campo]`"
- Isso aparece **no mesmo nível hierárquico** que Body Parameters (com `#####`)
- Quando há múltiplos arrays/objetos, repita a tabela para cada um
- Na UI de frontend, o payload de entrada também deve ficar dentro de um bloco colapsável próprio, contendo tanto os `Body Parameters` como todas as secções de estrutura interna relacionadas

**Exemplo de POST com Estrutura Interna:**
```
##### Body Parameters
| Parâmetro | Tipo | Obrigatório | Descrição |
| items | array | Sim | Lista de itens a criar |
| prices | array | Não | Lista de preços |

##### Estrutura interna de `items`
| Campo | Tipo | Obrigatório | Descrição |
| name | string | Sim | Nome do item |

##### Estrutura interna de `prices`
| Campo | Tipo | Obrigatório | Descrição |
| table | integer | Sim | Número da tabela |
| value | decimal | Sim | Valor do preço |

#### Exemplo cURL
...
```

---

## Códigos de Erro Específicos do Módulo

[Tabela de códigos de erro específicos para este módulo, além dos erros gerais de autenticação/autorização.]

| Código   | Descrição                                          | Quando ocorre                                    |
| :------- | :------------------------------------------------- | :----------------------------------------------- |
| `0000`   | Operação concluída com sucesso                    | Criação, atualização ou remoção bem-sucedidas    |
| `CL001`  | Erro ao persistir [entidade] na base de dados     | Falha de persistência no SQL/EF                  |
| `CL002`  | Já existe um [entidade] com id X e branch Y       | Combinação id + branch já existente              |
| `...`    | `...`                                              | `...`                                            |

---
