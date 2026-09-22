'script=insertReAPI

' ExecuteQuery:
'   fQuery -> SQL a executar
'   fSqlParameters -> parâmetros da query
Dim ExecuteQuery = Function(ByVal fQuery As String, ByVal fSqlParameters As List(Of System.Data.SqlClient.SqlParameter)) As DataTable
    Dim table As New DataTable()
    Using connection As SqlClient.SqlConnection = SqlHelp.GetNewConnection()
        Using command As New SqlClient.SqlCommand(fQuery, connection)
            If fSqlParameters IsNot Nothing Then
                For Each sqlParameter As System.Data.SqlClient.SqlParameter In fSqlParameters
                    command.Parameters.Add(sqlParameter)
                Next
            End If

            Using adapter As New SqlClient.SqlDataAdapter(command)
                adapter.Fill(table)
            End Using
        End Using
    End Using

    Return table
End Function

' ExecuteNonQuery:
'   Executa comandos SQL de INSERT/UPDATE/DELETE
Dim ExecuteNonQuery = Function(ByVal fQuery As String, ByVal fSqlParameters As List(Of System.Data.SqlClient.SqlParameter)) As Integer
    Dim affected As Integer = 0
    Using connection As SqlClient.SqlConnection = SqlHelp.GetNewConnection()
        Using command As New SqlClient.SqlCommand(fQuery, connection)
            If fSqlParameters IsNot Nothing Then
                For Each sqlParameter As System.Data.SqlClient.SqlParameter In fSqlParameters
                    command.Parameters.Add(sqlParameter)
                Next
            End If

            connection.Open()
            affected = command.ExecuteNonQuery()
        End Using
    End Using

    Return affected
End Function

' ExecuteQueryTx:
'   fQuery          -> SQL a executar
'   fSqlParameters  -> parâmetros da query
'   fConnection     -> ligação já aberta
'   fTransaction    -> transação activa
Dim ExecuteQueryTx = Function(ByVal fQuery As String, ByVal fSqlParameters As List(Of System.Data.SqlClient.SqlParameter), ByVal fConnection As SqlClient.SqlConnection, ByVal fTransaction As SqlClient.SqlTransaction) As DataTable
    Dim table As New DataTable()
    Using command As New SqlClient.SqlCommand(fQuery, fConnection, fTransaction)
        If fSqlParameters IsNot Nothing Then
            For Each sqlParameter As System.Data.SqlClient.SqlParameter In fSqlParameters
                command.Parameters.Add(sqlParameter)
            Next
        End If
        Using adapter As New SqlClient.SqlDataAdapter(command)
            adapter.Fill(table)
        End Using
    End Using
    Return table
End Function

' ExecuteNonQueryTx:
'   fQuery          -> SQL de INSERT/UPDATE/DELETE
'   fSqlParameters  -> parâmetros da query
'   fConnection     -> ligação já aberta
'   fTransaction    -> transação activa
Dim ExecuteNonQueryTx = Function(ByVal fQuery As String, ByVal fSqlParameters As List(Of System.Data.SqlClient.SqlParameter), ByVal fConnection As SqlClient.SqlConnection, ByVal fTransaction As SqlClient.SqlTransaction) As Integer
    Using command As New SqlClient.SqlCommand(fQuery, fConnection, fTransaction)
        If fSqlParameters IsNot Nothing Then
            For Each sqlParameter As System.Data.SqlClient.SqlParameter In fSqlParameters
                command.Parameters.Add(sqlParameter)
            Next
        End If
        Return command.ExecuteNonQuery()
    End Using
End Function

' ApplyAddFieldsToInsert:
'   Preenche extraCols e extraParams com as colunas e valores de addFields
'   para serem incluídos directamente no INSERT (padrão análogo a ApplyUserFieldsToRow em FT/BO).
'   extraCols   -> lista de nomes de colunas a concatenar no INSERT
'   extraParams -> lista de SqlParameter a concatenar nos VALUES e na lista de parâmetros
'   fieldsObj   -> JObject com { "coluna": valor, ... }
Dim ApplyAddFieldsToInsert = Sub(ByVal extraCols As List(Of String), ByVal extraParams As List(Of System.Data.SqlClient.SqlParameter), ByVal fieldsObj As Newtonsoft.Json.Linq.JObject)
    If fieldsObj Is Nothing OrElse fieldsObj.Count = 0 Then
        Exit Sub
    End If

    Dim paramIndex As Integer = extraParams.Count

    For Each prop As Newtonsoft.Json.Linq.JProperty In fieldsObj.Properties()
        Dim columnName As String = prop.Name
        If String.IsNullOrWhiteSpace(columnName) Then
            Continue For
        End If
        ' Validar nome da coluna: apenas alfanuméricos e underscore (prevenir SQL injection)
        If Not System.Text.RegularExpressions.Regex.IsMatch(columnName, "^[A-Za-z0-9_]+$") Then
            Throw New Exception("Nome de coluna inválido em addFields: '" & columnName & "'")
        End If

        extraCols.Add(columnName)
        Dim token As Newtonsoft.Json.Linq.JToken = prop.Value
        If token Is Nothing OrElse token.Type = Newtonsoft.Json.Linq.JTokenType.Null Then
            extraParams.Add(New System.Data.SqlClient.SqlParameter("@af" & paramIndex, DBNull.Value))
        Else
            extraParams.Add(New System.Data.SqlClient.SqlParameter("@af" & paramIndex, token.ToObject(Of Object)()))
        End If
        paramIndex += 1
    Next
End Sub

Dim response As Object

Try
    mstamp = "<val>" & mstamp & "</val>"

    Dim stream As New System.IO.MemoryStream(Encoding.UTF8.GetBytes(mstamp))
    Dim reader As System.Xml.XmlReader = New System.Xml.XmlTextReader(stream)
    Dim jsonVal As String = ""
    Dim column As String = ""

    Do While reader.Read()
        Select Case reader.NodeType
            Case System.Xml.XmlNodeType.Element
                column = reader.Name.ToLower()
            Case System.Xml.XmlNodeType.Text
                If column = "val" Then
                    jsonVal = reader.Value
                End If
        End Select
    Loop

    Dim jsonObj As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.Linq.JObject.Parse(jsonVal)

    ' JSON mínimo para criação de RE
    ' {
    '   "docTypeId": 45,
    '   "clientId": 2,
    '   "bankAccountId": 1,
    '   "lines": [
    '     { "invoiceNumber": 5, "invoiceTypeId": 1, "invoiceYear": 2026, "amount": 200.5 }
    '   ]
    ' }
    
    Dim docTypeId As Integer = Convert.ToInt32(jsonObj("docTypeId"))
    Dim clientId As Integer = Convert.ToInt32(jsonObj("clientId"))
    Dim bankAccountId As Integer = Convert.ToInt32(jsonObj("bankAccountId"))
    Dim adiantamentoDocTypeId As Integer = If(jsonObj("adiantamentoDocTypeId") IsNot Nothing, Convert.ToInt32(jsonObj("adiantamentoDocTypeId")), 1)

    ' Campos de utilizador dinâmicos do cabeçalho RE (opcional)
    Dim reAddFields As Newtonsoft.Json.Linq.JObject = TryCast(jsonObj("addFields"), Newtonsoft.Json.Linq.JObject)

    Dim lines As Newtonsoft.Json.Linq.JArray = TryCast(jsonObj("lines"), Newtonsoft.Json.Linq.JArray)
    If lines Is Nothing OrElse lines.Count = 0 Then
        Throw New Exception("É necessário enviar pelo menos uma linha em 'lines'.")
    End If

    ' TSRE obtido dinamicamente pelo docTypeId.
    Dim tsreTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 * FROM tsre WHERE ndoc = @ndoc ORDER BY tsrestamp",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ndoc", docTypeId)
        })

    If tsreTable.Rows.Count = 0 Then
        Throw New Exception($"Não foi encontrada configuração TSRE para o docTypeId {docTypeId}.")
    End If

    Dim tsreRow As DataRow = tsreTable.Rows(0)

    ' Cliente
    Dim clTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 no, nome, morada, local, codpost, ncont, zona, nib FROM cl WHERE no = @no",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@no", clientId)
        })

    If clTable.Rows.Count = 0 Then
        Throw New Exception($"Cliente {clientId} não encontrado.")
    End If

    Dim clRow As DataRow = clTable.Rows(0)
    Dim clientName As String = clRow("nome")
    Dim clientMorada As String = clRow("morada")
    Dim clientLocal As String = clRow("local")
    Dim clientCodpost As String = clRow("codpost")
    Dim clientNcont As String = clRow("ncont")
    Dim clientZona As String = clRow("zona")
    Dim clientNib As String = clRow("nib")

    Dim dataProcessamento As DateTime = DateTime.Today
    Dim anoProcessamento As Integer = dataProcessamento.Year
    Dim utilizador As String = NormalizeString(If(jsonObj("createdBy") IsNot Nothing, jsonObj("createdBy").ToString(), "PHCAPI"), 30)
    Dim hora As String = DateTime.Now.ToString("HH:mm:ss")
    Dim moeda As String = "MT"

    Dim nmdocRe As String = tsreRow("nmdoc")

    ' Obter ollocal a partir de bl (banco/conta) ligado ao bankAccountId
    Dim olLocalTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 noconta, LEFT(LTRIM(RTRIM(bl.banco)) + '          ', 10) + ' ' + LTRIM(RTRIM(bl.conta)) AS ollocal FROM bl WHERE noconta = @bankAccountId",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@bankAccountId", bankAccountId)
        })
    Dim olLocal As String = If(olLocalTable.Rows.Count > 0, olLocalTable.Rows(0)("ollocal").ToString(), String.Empty)
    
    ' Obter configuração de adiantamento (tsrd)
    Dim tsrdTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 cmcc, cmccn, ndoc, nmdoc FROM tsrd WHERE ndoc = @ndoc",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ndoc", adiantamentoDocTypeId)
        })

    If tsrdTable.Rows.Count = 0 Then
        Throw New Exception($"Não foi encontrada configuração TSRD.")
    End If

    Dim tsrdRow As DataRow = tsrdTable.Rows(0)

    ' Obter olcodigo a partir de cm1 ligado ao cmcc da série de recibo (RE)
    Dim olCodigoTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 cm1.olcl FROM tsre JOIN cm1 ON cm1.cm = tsre.cmcc WHERE tsre.ndoc = @ndoc",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ndoc", docTypeId)
        })
    Dim olCodigo As String = If(olCodigoTable.Rows.Count > 0, olCodigoTable.Rows(0)("olcl").ToString(), String.Empty)

    ' Obter olcodigo a partir de cm1 ligado ao cmcc da série de adiantamento (RD)
    Dim olCodigoRdTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 cm1.olcl FROM tsrd JOIN cm1 ON cm1.cm = tsrd.cmcc WHERE tsrd.ndoc = @ndoc",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ndoc", adiantamentoDocTypeId)
        })
    Dim olCodigoRd As String = If(olCodigoRdTable.Rows.Count > 0, olCodigoRdTable.Rows(0)("olcl").ToString(), String.Empty)

    ' Estrutura para agrupar por factura
    ' Chave: invoiceTypeId|invoiceNumber|invoiceYear
    Dim gruposFactura As New Dictionary(Of String, List(Of Newtonsoft.Json.Linq.JToken))()

    For Each line As Newtonsoft.Json.Linq.JToken In lines
        Dim invoiceTypeId As Integer = Convert.ToInt32(line("invoiceTypeId"))
        Dim invoiceNumber As Integer = Convert.ToInt32(line("invoiceNumber"))
        Dim invoiceYear As Integer = Convert.ToInt32(line("invoiceYear"))
        Dim amount As Decimal = line("amount")

        If amount <= 0D Then
            Throw New Exception($"O valor da linha da factura {invoiceNumber}/{invoiceYear} deve ser maior que zero.")
        End If

        Dim chaveFactura As String = $"{invoiceTypeId}|{invoiceNumber}|{invoiceYear}"
        If Not gruposFactura.ContainsKey(chaveFactura) Then
            gruposFactura(chaveFactura) = New List(Of Newtonsoft.Json.Linq.JToken)()
        End If
        gruposFactura(chaveFactura).Add(line)
    Next

    ' Lista de resposta com REs criados
    Dim respostasRe As New List(Of Newtonsoft.Json.Linq.JObject)()

    ' Acumular tudo para criar um unico RE com varias linhas
    Dim linhasRecibo As New List(Of Dictionary(Of String, Object))()
    Dim adiantamentos As New List(Of Dictionary(Of String, Object))()
    Dim totalReciboGlobal As Decimal = 0D

    For Each grupoKvp As KeyValuePair(Of String, List(Of Newtonsoft.Json.Linq.JToken)) In gruposFactura
        Dim partes As String() = grupoKvp.Key.Split("|"c)
        Dim invoiceTypeId As Integer = Convert.ToInt32(partes(0))
        Dim invoiceNumber As Integer = Convert.ToInt32(partes(1))
        Dim invoiceYear As Integer = Convert.ToInt32(partes(2))

        ' 1. Obter ftstamp da factura
        Dim ftTable As DataTable = ExecuteQuery(
            "SELECT TOP 1 ftstamp, ndoc AS ftndoc, fno FROM ft " & _
            "WHERE ndoc = @invoiceTypeId AND fno = @invoiceNumber AND ftano = @invoiceYear AND no = @clientId",
            New List(Of System.Data.SqlClient.SqlParameter) From {
                New System.Data.SqlClient.SqlParameter("@invoiceTypeId", invoiceTypeId),
                New System.Data.SqlClient.SqlParameter("@invoiceNumber", invoiceNumber),
                New System.Data.SqlClient.SqlParameter("@invoiceYear", invoiceYear),
                New System.Data.SqlClient.SqlParameter("@clientId", clientId)
            })

        If ftTable.Rows.Count = 0 Then
            Throw New Exception($"Não foi encontrada a factura {invoiceNumber}/{invoiceYear} (tipo {invoiceTypeId}) para o cliente {clientId}.")
        End If

        Dim ftRow As DataRow = ftTable.Rows(0)
        Dim ftstamp As String = ftRow("ftstamp").ToString()
        Dim ftndoc As Integer = Convert.ToInt32(ftRow("ftndoc"))

        ' 2. Obter todos os registos CC para este ftstamp
        Dim ccTable As DataTable = ExecuteQuery(
            "SELECT cmdesc, ccstamp, nrdoc, datalc, dataven, edeb as deb, edebf as debf " & _
            "FROM cc WHERE ccstamp = @ftstamp ORDER BY datalc ASC",
            New List(Of System.Data.SqlClient.SqlParameter) From {
                New System.Data.SqlClient.SqlParameter("@ftstamp", ftstamp)
            })

        ' 3. Calcular saldo total de cc para esta factura
        Dim saldoTotal As Decimal = 0D
        For Each ccRow As DataRow In ccTable.Rows
            Dim rowSaldo As Decimal = Convert.ToDecimal(ccRow("deb")) - Convert.ToDecimal(ccRow("debf"))
            If rowSaldo > 0D Then saldoTotal += rowSaldo
        Next

        ' 4. Distribuir amount pelos registos CC
        Dim totalReciboFactura As Decimal = 0D
        Dim totalAdiantamentoFactura As Decimal = 0D
        Dim linhasFactura As New List(Of Dictionary(Of String, Object))()

        For Each lineItem As Newtonsoft.Json.Linq.JToken In grupoKvp.Value
            Dim amount As Decimal = lineItem("amount")
            Dim pagamento As Decimal = amount

            If saldoTotal > 0D Then
                For Each ccRow As DataRow In ccTable.Rows
                    If pagamento <= 0D Then Exit For

                    Dim rowSaldo As Decimal = Convert.ToDecimal(ccRow("deb")) - Convert.ToDecimal(ccRow("debf"))
                    If rowSaldo <= 0D Then Continue For

                    Dim vPagar As Decimal = Math.Min(pagamento, rowSaldo)

                    Dim info As New Dictionary(Of String, Object)()
                    info("ccstamp") = ccRow("ccstamp").ToString()
                    info("ndoc") = ftndoc
                    info("nrdoc") = Convert.ToInt32(ccRow("nrdoc"))
                    info("cdesc") = ccRow("cmdesc").ToString()
                    info("datalc") = Convert.ToDateTime(ccRow("datalc"))
                    info("dataven") = Convert.ToDateTime(ccRow("dataven"))
                    info("eval") = rowSaldo
                    info("erec") = vPagar
                    ' Campos de utilizador dinâmicos da linha RL (optional, vindos do lineItem)
                    info("addFields") = TryCast(lineItem("addFields"), Newtonsoft.Json.Linq.JObject)
                    linhasFactura.Add(info)
                    totalReciboFactura += vPagar
                    pagamento -= vPagar
                Next
            End If

            If pagamento > 0D Then
                totalAdiantamentoFactura += pagamento
            End If
        Next

        For Each info As Dictionary(Of String, Object) In linhasFactura
            info("invoiceYear") = invoiceYear
            linhasRecibo.Add(info)
        Next
        totalReciboGlobal += totalReciboFactura

        If totalAdiantamentoFactura > 0D Then
            Dim infoAdiantamento As New Dictionary(Of String, Object)()
            infoAdiantamento("invoiceNumber") = invoiceNumber
            infoAdiantamento("invoiceYear") = invoiceYear
            infoAdiantamento("total") = totalAdiantamentoFactura
            adiantamentos.Add(infoAdiantamento)
        End If
    Next

    ' Inserir tudo numa unica transação para garantir um unico RE por pedido
    Using txConnection As SqlClient.SqlConnection = SqlHelp.GetNewConnection()
        txConnection.Open()
        Using tx As SqlClient.SqlTransaction = txConnection.BeginTransaction()
            Try
                Dim reStamp As String = ""
                Dim reRno As Integer = 0

                If totalReciboGlobal > 0D Then
                    Dim reKeyResult As DataTable = ExecuteQueryTx(
                        "SELECT ISNULL(MAX(rno), 0) + 1 AS nextrno, LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 25) AS restamp FROM re WITH (UPDLOCK, HOLDLOCK) WHERE reano = @ano",
                        New List(Of System.Data.SqlClient.SqlParameter) From {
                            New System.Data.SqlClient.SqlParameter("@ano", anoProcessamento)
                        }, txConnection, tx)

                    If reKeyResult.Rows.Count = 0 Then
                        Throw New Exception("Não foi possível gerar numeração para re.")
                    End If
                    reRno = Convert.ToInt32(reKeyResult.Rows(0)("nextrno"))
                    reStamp = reKeyResult.Rows(0)("restamp").ToString()

                    ' Inserir RL (linhas) de todas as facturas no mesmo RE
                    For Each info As Dictionary(Of String, Object) In linhasRecibo
                        ' Campos de utilizador dinâmicos da linha (rl) — incluídos directamente no INSERT
                        Dim rlExtraCols As New List(Of String)()
                        Dim rlExtraParams As New List(Of System.Data.SqlClient.SqlParameter)()
                        ApplyAddFieldsToInsert(rlExtraCols, rlExtraParams, TryCast(info("addFields"), Newtonsoft.Json.Linq.JObject))

                        Dim rlExtraColsSql As String = ""
                        Dim rlExtraValsSql As String = ""
                        For Each col As String In rlExtraCols
                            rlExtraColsSql &= ", " & col
                        Next
                        For Each p As System.Data.SqlClient.SqlParameter In rlExtraParams
                            rlExtraValsSql &= ", " & p.ParameterName
                        Next

                        Dim rlParams As New List(Of System.Data.SqlClient.SqlParameter) From {
                            New System.Data.SqlClient.SqlParameter("@ndoc", Convert.ToInt32(info("ndoc"))),
                            New System.Data.SqlClient.SqlParameter("@rno", reRno),
                            New System.Data.SqlClient.SqlParameter("@cdesc", info("cdesc").ToString()),
                            New System.Data.SqlClient.SqlParameter("@nrdoc", Convert.ToInt32(info("nrdoc"))),
                            New System.Data.SqlClient.SqlParameter("@rec", Convert.ToDecimal(info("erec"))),
                            New System.Data.SqlClient.SqlParameter("@erec", Convert.ToDecimal(info("erec"))),
                            New System.Data.SqlClient.SqlParameter("@val", Convert.ToDecimal(info("eval"))),
                            New System.Data.SqlClient.SqlParameter("@eval", Convert.ToDecimal(info("eval"))),
                            New System.Data.SqlClient.SqlParameter("@datalc", Convert.ToDateTime(info("datalc"))),
                            New System.Data.SqlClient.SqlParameter("@dataven", Convert.ToDateTime(info("dataven"))),
                            New System.Data.SqlClient.SqlParameter("@restamp", reStamp),
                            New System.Data.SqlClient.SqlParameter("@ccstamp", info("ccstamp").ToString()),
                            New System.Data.SqlClient.SqlParameter("@process", True),
                            New System.Data.SqlClient.SqlParameter("@ousrinis", utilizador),
                            New System.Data.SqlClient.SqlParameter("@ousrdata", Date.Today),
                            New System.Data.SqlClient.SqlParameter("@ousrhora", hora),
                            New System.Data.SqlClient.SqlParameter("@usrinis", utilizador),
                            New System.Data.SqlClient.SqlParameter("@usrdata", Date.Today),
                            New System.Data.SqlClient.SqlParameter("@usrhora", hora)
                        }
                        rlParams.AddRange(rlExtraParams)

                        ' ==========================================
                        ' PONTO DE MANIPULAÇÃO (rl) — antes do INSERT
                        ' Aqui pode ajustar qualquer variável da linha antes de ser gravada:
                        '   info("cdesc"), info("erec"), info("eval"), info("addFields"), ...
                        '   rlParams       -> lista completa de SqlParameter (fixos + addFields)
                        '   rlExtraCols    -> nomes das colunas adicionais
                        '   rlExtraParams  -> parâmetros das colunas adicionais
                        ' ==========================================
                        '
                        ' EXEMPLO: forçar descrição ou sobrepor um campo fixo
                        '   Dim pCdesc As System.Data.SqlClient.SqlParameter = rlParams.Find(Function(p) p.ParameterName = "@cdesc")
                        '   If pCdesc IsNot Nothing Then pCdesc.Value = "Descrição manual"
                        '
                        ' EXEMPLO: sobrepor o valor de um campo adicional (addFields) pelo índice do parâmetro
                        '   Dim pAf As System.Data.SqlClient.SqlParameter = rlParams.Find(Function(p) p.ParameterName = "@af0")
                        '   If pAf IsNot Nothing Then pAf.Value = "valor-forçado"
                        '
                        ' EXEMPLO: adicionar uma coluna extra manualmente (sem vir do addFields do JSON)
                        '   rlExtraCols.Add("u_observacao")
                        '   rlExtraParams.Add(New System.Data.SqlClient.SqlParameter("@af_obs", "Observação manual"))
                        '   rlParams.Add(New System.Data.SqlClient.SqlParameter("@af_obs", "Observação manual"))
                        '   rlExtraColsSql &= ", u_observacao"
                        '   rlExtraValsSql &= ", @af_obs"
                        '
                        ExecuteNonQueryTx(
                            "INSERT INTO rl (rlstamp, ndoc, rno, cdesc, nrdoc, rec, erec, val, eval, datalc, dataven, restamp, ccstamp, process, ousrinis, ousrdata, ousrhora, usrinis, usrdata, usrhora" & rlExtraColsSql & ") " & _
                            "VALUES (LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 25), @ndoc, @rno, @cdesc, @nrdoc, @rec, @erec, @val, @eval, @datalc, @dataven, @restamp, @ccstamp, @process, @ousrinis, @ousrdata, @ousrhora, @usrinis, @usrdata, @usrhora" & rlExtraValsSql & ")",
                            rlParams, txConnection, tx)
                    Next

                    ' Inserir RE (cabeçalho)
                    ' Campos de utilizador dinâmicos do cabeçalho (re) — incluídos directamente no INSERT
                    Dim reExtraCols As New List(Of String)()
                    Dim reExtraParams As New List(Of System.Data.SqlClient.SqlParameter)()
                    ApplyAddFieldsToInsert(reExtraCols, reExtraParams, reAddFields)

                    Dim reExtraColsSql As String = ""
                    Dim reExtraValsSql As String = ""
                    For Each col As String In reExtraCols
                        reExtraColsSql &= ", " & col
                    Next
                    For Each p As System.Data.SqlClient.SqlParameter In reExtraParams
                        reExtraValsSql &= ", " & p.ParameterName
                    Next

                    Dim reInsertParams As New List(Of System.Data.SqlClient.SqlParameter) From {
                        New System.Data.SqlClient.SqlParameter("@restamp", reStamp),
                        New System.Data.SqlClient.SqlParameter("@nmdoc", nmdocRe),
                        New System.Data.SqlClient.SqlParameter("@rno", reRno),
                        New System.Data.SqlClient.SqlParameter("@rdata", dataProcessamento),
                        New System.Data.SqlClient.SqlParameter("@nome", clientName),
                        New System.Data.SqlClient.SqlParameter("@morada", clientMorada),
                        New System.Data.SqlClient.SqlParameter("@local", clientLocal),
                        New System.Data.SqlClient.SqlParameter("@codpost", clientCodpost),
                        New System.Data.SqlClient.SqlParameter("@ncont", clientNcont),
                        New System.Data.SqlClient.SqlParameter("@zona", clientZona),
                        New System.Data.SqlClient.SqlParameter("@nib", clientNib),
                        New System.Data.SqlClient.SqlParameter("@total", totalReciboGlobal),
                        New System.Data.SqlClient.SqlParameter("@etotal", totalReciboGlobal),
                        New System.Data.SqlClient.SqlParameter("@ndoc", docTypeId),
                        New System.Data.SqlClient.SqlParameter("@no", clientId),
                        New System.Data.SqlClient.SqlParameter("@reano", anoProcessamento),
                        New System.Data.SqlClient.SqlParameter("@olcodigo", olCodigo),
                        New System.Data.SqlClient.SqlParameter("@totalmoeda", totalReciboGlobal),
                        New System.Data.SqlClient.SqlParameter("@moeda", moeda),
                        New System.Data.SqlClient.SqlParameter("@contado", bankAccountId),
                        New System.Data.SqlClient.SqlParameter("@ollocal", olLocal),
                        New System.Data.SqlClient.SqlParameter("@process", True),
                        New System.Data.SqlClient.SqlParameter("@procdata", dataProcessamento),
                        New System.Data.SqlClient.SqlParameter("@ousrinis", utilizador),
                        New System.Data.SqlClient.SqlParameter("@ousrdata", Date.Today),
                        New System.Data.SqlClient.SqlParameter("@ousrhora", hora),
                        New System.Data.SqlClient.SqlParameter("@usrinis", utilizador),
                        New System.Data.SqlClient.SqlParameter("@usrdata", Date.Today),
                        New System.Data.SqlClient.SqlParameter("@usrhora", hora)
                    }
                    reInsertParams.AddRange(reExtraParams)

                    ' ========================================j==
                    ' PONTO DE MANIPULAÇÃO (re) — antes do INSERT
                    ' Aqui pode ajustar qualquer variável do cabeçalho antes de ser gravado:
                    '   reStamp, reRno, totalReciboGlobal, clientName, olCodigo, olLocal, ...
                    '   reInsertParams -> lista completa de SqlParameter (fixos + addFields)
                    '   reExtraCols    -> nomes das colunas adicionais
                    '   reExtraParams  -> parâmetros das colunas adicionais
                    ' ==========================================
                    '
                    ' EXEMPLO: forçar o nome do cliente no cabeçalho
                    '   Dim pNome As System.Data.SqlClient.SqlParameter = reInsertParams.Find(Function(p) p.ParameterName = "@nome")
                    '   If pNome IsNot Nothing Then pNome.Value = "Nome Manual Lda"
                    '
                    ' EXEMPLO: sobrepor o valor de um campo adicional (addFields) pelo índice do parâmetro
                    '   Dim pAf As System.Data.SqlClient.SqlParameter = reInsertParams.Find(Function(p) p.ParameterName = "@af0")
                    '   If pAf IsNot Nothing Then pAf.Value = "valor-forçado"
                    '
                    ' EXEMPLO: adicionar uma coluna extra manualmente (sem vir do addFields do JSON)
                    '   reExtraCols.Add("u_referencia")
                    '   reExtraParams.Add(New System.Data.SqlClient.SqlParameter("@af_ref", "REF-001"))
                    '   reInsertParams.Add(New System.Data.SqlClient.SqlParameter("@af_ref", "REF-001"))
                    '   reExtraColsSql &= ", u_referencia"
                    '   reExtraValsSql &= ", @af_ref"
                    '
                    ExecuteNonQueryTx(
                        "INSERT INTO re (restamp, nmdoc, rno, rdata, nome, morada, local, codpost, ncont, zona, nib, total, etotal, ndoc, no, reano, olcodigo, totalmoeda, moeda, contado, ollocal, process, procdata, ousrinis, ousrdata, ousrhora, usrinis, usrdata, usrhora" & reExtraColsSql & ") " & _
                        "VALUES (@restamp, @nmdoc, @rno, @rdata, @nome, @morada, @local, @codpost, @ncont, @zona, @nib, @total, @etotal, @ndoc, @no, @reano, @olcodigo, @totalmoeda, @moeda, @contado, @ollocal, @process, @procdata, @ousrinis, @ousrdata, @ousrhora, @usrinis, @usrdata, @usrhora" & reExtraValsSql & ")",
                        reInsertParams, txConnection, tx)

                    ' Adicionar RE à lista de resposta
                    Dim linhasResposta As New Newtonsoft.Json.Linq.JArray()
                    For Each linhaInfo As Dictionary(Of String, Object) In linhasRecibo
                        Dim linhaObj As New Newtonsoft.Json.Linq.JObject
                        linhaObj("nrdoc") = Convert.ToInt32(linhaInfo("nrdoc"))
                        linhaObj("ftano") = Convert.ToInt32(linhaInfo("invoiceYear"))
                        linhaObj("cdesc") = linhaInfo("cdesc").ToString()
                        linhaObj("erec") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(linhaInfo("erec")))
                        linhaObj("eval") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(linhaInfo("eval")))
                        ' Ecoar addFields da linha (rl) na resposta
                        Dim linhaAddFields As Newtonsoft.Json.Linq.JObject = TryCast(linhaInfo("addFields"), Newtonsoft.Json.Linq.JObject)
                        linhaObj("addFields") = If(linhaAddFields IsNot Nothing, CType(linhaAddFields, Newtonsoft.Json.Linq.JToken), Newtonsoft.Json.Linq.JValue.CreateNull())
                        linhasResposta.Add(linhaObj)
                    Next

                    Dim reResponse As New Newtonsoft.Json.Linq.JObject
                    reResponse("type") = "RE"
                    reResponse("ndoc") = docTypeId
                    reResponse("stamp") = reStamp
                    reResponse("rno") = reRno
                    reResponse("bankAccountId") = bankAccountId
                    reResponse("total") = Newtonsoft.Json.Linq.JToken.FromObject(totalReciboGlobal)
                    reResponse("toRegularize") = Newtonsoft.Json.Linq.JToken.FromObject(totalReciboGlobal)
                    reResponse("linhas") = linhasResposta
                    ' Ecoar addFields do cabeçalho RE na resposta
                    reResponse("addFields") = If(reAddFields IsNot Nothing, CType(reAddFields, Newtonsoft.Json.Linq.JToken), Newtonsoft.Json.Linq.JValue.CreateNull())
                    respostasRe.Add(reResponse)
                End If

                For Each infoAdiantamento As Dictionary(Of String, Object) In adiantamentos
                    Dim rdKeyResult As DataTable = ExecuteQueryTx(
                        "SELECT ISNULL(MAX(rno), 0) + 1 AS nextrno, LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 25) AS rdstamp FROM rd WITH (UPDLOCK, HOLDLOCK) WHERE rdano = @ano",
                        New List(Of System.Data.SqlClient.SqlParameter) From {
                            New System.Data.SqlClient.SqlParameter("@ano", anoProcessamento)
                        }, txConnection, tx)

                    If rdKeyResult.Rows.Count = 0 Then
                        Throw New Exception("Não foi possível gerar numeração para rd.")
                    End If

                    Dim rdRno As Integer = Convert.ToInt32(rdKeyResult.Rows(0)("nextrno"))
                    Dim rdStamp As String = rdKeyResult.Rows(0)("rdstamp").ToString()
                    Dim totalAdiantamentoFactura As Decimal = Convert.ToDecimal(infoAdiantamento("total"))

                    ExecuteNonQueryTx(
                        "INSERT INTO rd (rdstamp, nmdoc, rno, rdata, nome, morada, local, codpost, ncont, zona, nib, total, etotal, base, ebase, ndoc, no, rdano, olcodigo, moeda, contado, ollocal, cm, cmdesc, ousrinis, ousrdata, ousrhora, usrinis, usrdata, usrhora) " & _
                        "VALUES (@rdstamp, @nmdoc, @rno, @rdata, @nome, @morada, @local, @codpost, @ncont, @zona, @nib, @total, @etotal, @base, @ebase, @ndoc, @no, @rdano, @olcodigo, @moeda, @contado, @ollocal, @cm, @cmdesc, @ousrinis, @ousrdata, @ousrhora, @usrinis, @usrdata, @usrhora)",
                        New List(Of System.Data.SqlClient.SqlParameter) From {
                            New System.Data.SqlClient.SqlParameter("@rdstamp", rdStamp),
                            New System.Data.SqlClient.SqlParameter("@nmdoc", tsrdRow("nmdoc")),
                            New System.Data.SqlClient.SqlParameter("@rno", rdRno),
                            New System.Data.SqlClient.SqlParameter("@rdata", dataProcessamento),
                            New System.Data.SqlClient.SqlParameter("@nome", clientName),
                            New System.Data.SqlClient.SqlParameter("@morada", clientMorada),
                            New System.Data.SqlClient.SqlParameter("@local", clientLocal),
                            New System.Data.SqlClient.SqlParameter("@codpost", clientCodpost),
                            New System.Data.SqlClient.SqlParameter("@ncont", clientNcont),
                            New System.Data.SqlClient.SqlParameter("@zona", clientZona),
                            New System.Data.SqlClient.SqlParameter("@nib", clientNib),
                            New System.Data.SqlClient.SqlParameter("@total", totalAdiantamentoFactura),
                            New System.Data.SqlClient.SqlParameter("@etotal", totalAdiantamentoFactura),
                            New System.Data.SqlClient.SqlParameter("@base", totalAdiantamentoFactura),
                            New System.Data.SqlClient.SqlParameter("@ebase", totalAdiantamentoFactura),
                            New System.Data.SqlClient.SqlParameter("@ndoc", tsrdRow("ndoc")),
                            New System.Data.SqlClient.SqlParameter("@no", clientId),
                            New System.Data.SqlClient.SqlParameter("@rdano", anoProcessamento),
                            New System.Data.SqlClient.SqlParameter("@olcodigo", olCodigoRd),
                            New System.Data.SqlClient.SqlParameter("@moeda", moeda),
                            New System.Data.SqlClient.SqlParameter("@contado", bankAccountId),
                            New System.Data.SqlClient.SqlParameter("@ollocal", olLocal),
                            New System.Data.SqlClient.SqlParameter("@cm", tsrdRow("cmcc")),
                            New System.Data.SqlClient.SqlParameter("@cmdesc", tsrdRow("cmccn")),
                            New System.Data.SqlClient.SqlParameter("@ousrinis", utilizador),
                            New System.Data.SqlClient.SqlParameter("@ousrdata", Date.Today),
                            New System.Data.SqlClient.SqlParameter("@ousrhora", hora),
                            New System.Data.SqlClient.SqlParameter("@usrinis", utilizador),
                            New System.Data.SqlClient.SqlParameter("@usrdata", Date.Today),
                            New System.Data.SqlClient.SqlParameter("@usrhora", hora)
                        }, txConnection, tx)

                    ' Adicionar RD à lista de resposta
                    Dim rdResponse As New Newtonsoft.Json.Linq.JObject
                    rdResponse("type") = "RD"
                    rdResponse("ndoc") = Convert.ToInt32(tsrdRow("ndoc"))
                    rdResponse("stamp") = rdStamp
                    rdResponse("rno") = rdRno
                    rdResponse("total") = Newtonsoft.Json.Linq.JToken.FromObject(totalAdiantamentoFactura)
                    rdResponse("toRegularize") = Newtonsoft.Json.Linq.JToken.FromObject(0D)
                    rdResponse("nrdoc") = Convert.ToInt32(infoAdiantamento("invoiceNumber"))
                    rdResponse("ftano") = Convert.ToInt32(infoAdiantamento("invoiceYear"))
                    respostasRe.Add(rdResponse)
                Next

                tx.Commit()
            Catch txEx As Exception
                Try
                    tx.Rollback()
                Catch
                End Try
                Throw New Exception("Transação anulada. Nenhum registo foi inserido. " & txEx.Message)
            End Try
        End Using
    End Using

    Dim reDocs As New List(Of Newtonsoft.Json.Linq.JObject)()
    Dim rdDocs As New List(Of Newtonsoft.Json.Linq.JObject)()

    For Each doc As Newtonsoft.Json.Linq.JObject In respostasRe
        Dim tipo As String = If(doc("type") IsNot Nothing, doc("type").ToString(), "")
        If tipo = "RE" Then
            reDocs.Add(doc)
        ElseIf tipo = "RD" Then
            rdDocs.Add(doc)
        End If
    Next

    ' RE consolidado (um objeto) com todas as linhas regularizadas
    Dim reOut As Newtonsoft.Json.Linq.JToken = Nothing
    If reDocs.Count > 0 Then
        Dim reLinhas As New Newtonsoft.Json.Linq.JArray()
        Dim reTotal As Decimal = 0D

        For Each reDoc As Newtonsoft.Json.Linq.JObject In reDocs
            reTotal += If(reDoc("total") IsNot Nothing, Convert.ToDecimal(reDoc("total")), 0D)
            Dim linhasToken As Newtonsoft.Json.Linq.JToken = reDoc("linhas")
            If linhasToken IsNot Nothing AndAlso linhasToken.Type = Newtonsoft.Json.Linq.JTokenType.Array Then
                For Each linha As Newtonsoft.Json.Linq.JToken In CType(linhasToken, Newtonsoft.Json.Linq.JArray)
                    reLinhas.Add(linha)
                Next
            End If
        Next

        Dim firstRe As Newtonsoft.Json.Linq.JObject = reDocs(0)
        Dim reObj As New Newtonsoft.Json.Linq.JObject
        reObj("ndoc") = If(firstRe("ndoc"), docTypeId)
        reObj("rno") = If(firstRe("rno"), 0)
        reObj("total") = Newtonsoft.Json.Linq.JToken.FromObject(reTotal)
        reObj("linhas") = reLinhas
        ' Ecoar addFields do cabeçalho RE no objecto de saída final
        If firstRe("addFields") IsNot Nothing Then
            reObj("addFields") = firstRe("addFields")
        End If
        reOut = reObj
    End If

    ' RD em lista (podem existir vários adiantamentos)
    Dim rdOut As Newtonsoft.Json.Linq.JToken = Nothing
    If rdDocs.Count > 0 Then
        Dim rdArray As New Newtonsoft.Json.Linq.JArray()
        For Each rdDoc As Newtonsoft.Json.Linq.JObject In rdDocs
            Dim rdObj As New Newtonsoft.Json.Linq.JObject
            rdObj("ndoc") = If(rdDoc("ndoc"), 0)
            rdObj("rno") = If(rdDoc("rno"), 0)
            rdObj("total") = If(rdDoc("total") IsNot Nothing, rdDoc("total"), Newtonsoft.Json.Linq.JToken.FromObject(0D))
            rdObj("nrdoc") = If(rdDoc("nrdoc"), 0)
            rdObj("ftano") = If(rdDoc("ftano"), 0)
            rdArray.Add(rdObj)
        Next
        rdOut = rdArray
    End If

    Dim documentsObj As New Newtonsoft.Json.Linq.JObject
    documentsObj("Re") = If(reOut, Newtonsoft.Json.Linq.JValue.CreateNull())
    documentsObj("Rd") = If(rdOut, Newtonsoft.Json.Linq.JValue.CreateNull())

    Dim totalCountResponse As Integer = 0
    If reOut IsNot Nothing Then totalCountResponse += 1
    If rdOut IsNot Nothing Then totalCountResponse += CType(rdOut, Newtonsoft.Json.Linq.JArray).Count

    Dim okResponse As New Newtonsoft.Json.Linq.JObject
    okResponse("success") = True
    okResponse("clientId") = clientId
    okResponse("bankAccountId") = bankAccountId
    okResponse("totalCount") = totalCountResponse
    okResponse("documents") = documentsObj
    response = okResponse

Catch e As Exception
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject
    errorResponse("success") = False
    errorResponse("code") = "RE001"
    errorResponse("message") = e.Message
    response = errorResponse
End Try

Return response

