'script=insertRdAPI

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
'   para serem incluídos directamente no INSERT.
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

    ' JSON mínimo para criação de RD
    ' {
    '   "docTypeId": 1,
    '   "clientId": 2,
    '   "bankAccountId": 1,
    '   "amount": 500.0,
    '   "date": null,
    '   "addFields": null
    ' }

    Dim docTypeId As Integer = Convert.ToInt32(jsonObj("docTypeId"))
    Dim clientId As Integer = Convert.ToInt32(jsonObj("clientId"))
    Dim bankAccountId As Integer = Convert.ToInt32(jsonObj("bankAccountId"))
    Dim amount As Decimal = Convert.ToDecimal(jsonObj("amount"))

    If amount <= 0D Then
        Throw New Exception("O valor do adiantamento deve ser maior que zero.")
    End If

    ' Data do documento: usa a data enviada ou hoje
    Dim dataProcessamento As DateTime
    If jsonObj("date") IsNot Nothing AndAlso jsonObj("date").Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
        dataProcessamento = Convert.ToDateTime(jsonObj("date").ToString())
    Else
        dataProcessamento = DateTime.Today
    End If
    Dim anoProcessamento As Integer = dataProcessamento.Year

    Dim utilizadorRaw As String = If(jsonObj("createdBy") IsNot Nothing, jsonObj("createdBy").ToString().Trim(), "PHCAPI")
    Dim utilizador As String = If(utilizadorRaw.Length > 30, utilizadorRaw.Substring(0, 30), utilizadorRaw)
    Dim hora As String = DateTime.Now.ToString("HH:mm:ss")
    Dim moeda As String = "MT"

    Dim descricao As String = If(jsonObj("descricao") IsNot Nothing AndAlso jsonObj("descricao").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("descricao").ToString().Trim(), "")

    ' Campos de utilizador dinâmicos do cabeçalho RD (opcional)
    Dim rdAddFields As Newtonsoft.Json.Linq.JObject = TryCast(jsonObj("addFields"), Newtonsoft.Json.Linq.JObject)

    ' 1. Configuração TSRD
    Dim tsrdTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 ndoc, nmdoc, cmcc, cmccn FROM tsrd WHERE ndoc = @ndoc ORDER BY tsrdstamp",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ndoc", docTypeId)
        })

    If tsrdTable.Rows.Count = 0 Then
        Throw New Exception($"Não foi encontrada configuração TSRD para o docTypeId {docTypeId}.")
    End If

    Dim tsrdRow As DataRow = tsrdTable.Rows(0)
    Dim nmdocRd As String = tsrdRow("nmdoc").ToString()

    ' 2. Cliente
    Dim clTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 no, nome, morada, local, codpost, ncont, zona, nib FROM cl WHERE no = @no",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@no", clientId)
        })

    If clTable.Rows.Count = 0 Then
        Throw New Exception($"Cliente {clientId} não encontrado.")
    End If

    Dim clRow As DataRow = clTable.Rows(0)
    Dim clientName As String = clRow("nome").ToString()
    Dim clientMorada As String = clRow("morada").ToString()
    Dim clientLocal As String = clRow("local").ToString()
    Dim clientCodpost As String = clRow("codpost").ToString()
    Dim clientNcont As String = clRow("ncont").ToString()
    Dim clientZona As String = clRow("zona").ToString()
    Dim clientNib As String = clRow("nib").ToString()

    ' 3. Conta bancária (ollocal)
    Dim olLocalTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 noconta, LEFT(LTRIM(RTRIM(bl.banco)) + '          ', 10) + ' ' + LTRIM(RTRIM(bl.conta)) AS ollocal FROM bl WHERE noconta = @bankAccountId",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@bankAccountId", bankAccountId)
        })

    If olLocalTable.Rows.Count = 0 Then
        Throw New Exception($"Conta bancária {bankAccountId} não encontrada.")
    End If

    Dim olLocal As String = olLocalTable.Rows(0)("ollocal").ToString()

    ' 4. Código contabilístico (olcodigo) via cm1 ligado ao cmcc da série TSRD
    Dim olCodigoTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 cm1.olcl FROM tsrd JOIN cm1 ON cm1.cm = tsrd.cmcc WHERE tsrd.ndoc = @ndoc",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ndoc", docTypeId)
        })
    Dim olCodigo As String = If(olCodigoTable.Rows.Count > 0, olCodigoTable.Rows(0)("olcl").ToString(), String.Empty)

    ' 5. Inserir RD em transação
    Dim rdStamp As String = ""
    Dim rdRno As Integer = 0

    Using txConnection As SqlClient.SqlConnection = SqlHelp.GetNewConnection()
        txConnection.Open()
        Using tx As SqlClient.SqlTransaction = txConnection.BeginTransaction()
            Try
                ' Gerar numeração com lock para evitar duplicados
                Dim rdKeyResult As DataTable = ExecuteQueryTx(
                    "SELECT ISNULL(MAX(rno), 0) + 1 AS nextrno, LEFT(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 25) AS rdstamp FROM rd WITH (UPDLOCK, HOLDLOCK) WHERE rdano = @ano",
                    New List(Of System.Data.SqlClient.SqlParameter) From {
                        New System.Data.SqlClient.SqlParameter("@ano", anoProcessamento)
                    }, txConnection, tx)

                If rdKeyResult.Rows.Count = 0 Then
                    Throw New Exception("Não foi possível gerar numeração para rd.")
                End If

                rdRno = Convert.ToInt32(rdKeyResult.Rows(0)("nextrno"))
                rdStamp = rdKeyResult.Rows(0)("rdstamp").ToString()

                ' Campos de utilizador dinâmicos de rd — incluídos directamente no INSERT
                Dim rdExtraCols As New List(Of String)()
                Dim rdExtraParams As New List(Of System.Data.SqlClient.SqlParameter)()
                ApplyAddFieldsToInsert(rdExtraCols, rdExtraParams, rdAddFields)

                Dim rdExtraColsSql As String = ""
                Dim rdExtraValsSql As String = ""
                For Each col As String In rdExtraCols
                    rdExtraColsSql &= ", " & col
                Next
                For Each p As System.Data.SqlClient.SqlParameter In rdExtraParams
                    rdExtraValsSql &= ", " & p.ParameterName
                Next

                Dim rdInsertParams As New List(Of System.Data.SqlClient.SqlParameter) From {
                    New System.Data.SqlClient.SqlParameter("@rdstamp", rdStamp),
                    New System.Data.SqlClient.SqlParameter("@nmdoc", nmdocRd),
                    New System.Data.SqlClient.SqlParameter("@rno", rdRno),
                    New System.Data.SqlClient.SqlParameter("@rdata", dataProcessamento),
                    New System.Data.SqlClient.SqlParameter("@nome", clientName),
                    New System.Data.SqlClient.SqlParameter("@morada", clientMorada),
                    New System.Data.SqlClient.SqlParameter("@local", clientLocal),
                    New System.Data.SqlClient.SqlParameter("@codpost", clientCodpost),
                    New System.Data.SqlClient.SqlParameter("@ncont", clientNcont),
                    New System.Data.SqlClient.SqlParameter("@zona", clientZona),
                    New System.Data.SqlClient.SqlParameter("@nib", clientNib),
                    New System.Data.SqlClient.SqlParameter("@total", amount),
                    New System.Data.SqlClient.SqlParameter("@etotal", amount),
                    New System.Data.SqlClient.SqlParameter("@base", amount),
                    New System.Data.SqlClient.SqlParameter("@ebase", amount),
                    New System.Data.SqlClient.SqlParameter("@ndoc", Convert.ToInt32(tsrdRow("ndoc"))),
                    New System.Data.SqlClient.SqlParameter("@no", clientId),
                    New System.Data.SqlClient.SqlParameter("@rdano", anoProcessamento),
                    New System.Data.SqlClient.SqlParameter("@olcodigo", olCodigo),
                    New System.Data.SqlClient.SqlParameter("@moeda", moeda),
                    New System.Data.SqlClient.SqlParameter("@contado", bankAccountId),
                    New System.Data.SqlClient.SqlParameter("@ollocal", olLocal),
                    New System.Data.SqlClient.SqlParameter("@descricao", descricao),
                    New System.Data.SqlClient.SqlParameter("@cm", tsrdRow("cmcc").ToString()),
                    New System.Data.SqlClient.SqlParameter("@cmdesc", tsrdRow("cmccn").ToString()),
                    New System.Data.SqlClient.SqlParameter("@ousrinis", utilizador),
                    New System.Data.SqlClient.SqlParameter("@ousrdata", Date.Today),
                    New System.Data.SqlClient.SqlParameter("@ousrhora", hora),
                    New System.Data.SqlClient.SqlParameter("@usrinis", utilizador),
                    New System.Data.SqlClient.SqlParameter("@usrdata", Date.Today),
                    New System.Data.SqlClient.SqlParameter("@usrhora", hora)
                }
                rdInsertParams.AddRange(rdExtraParams)

                ' ==========================================
                ' PONTO DE MANIPULAÇÃO (rd) — antes do INSERT
                ' Aqui pode ajustar qualquer variável do cabeçalho antes de ser gravado:
                '   rdStamp, rdRno, amount, clientName, olCodigo, olLocal, nmdocRd, ...
                '   rdInsertParams -> lista completa de SqlParameter (fixos + addFields)
                '   rdExtraCols    -> nomes das colunas adicionais
                '   rdExtraParams  -> parâmetros das colunas adicionais
                ' ==========================================
                '
                ' EXEMPLO: forçar o nome do cliente no cabeçalho
                '   Dim pNome As System.Data.SqlClient.SqlParameter = rdInsertParams.Find(Function(p) p.ParameterName = "@nome")
                '   If pNome IsNot Nothing Then pNome.Value = "Nome Manual Lda"
                '
                ' EXEMPLO: sobrepor o valor de um campo adicional (addFields) pelo índice do parâmetro
                '   Dim pAf As System.Data.SqlClient.SqlParameter = rdInsertParams.Find(Function(p) p.ParameterName = "@af0")
                '   If pAf IsNot Nothing Then pAf.Value = "valor-forçado"
                '
                ' EXEMPLO: adicionar uma coluna extra manualmente (sem vir do addFields do JSON)
                '   rdExtraCols.Add("u_referencia")
                '   rdExtraParams.Add(New System.Data.SqlClient.SqlParameter("@af_ref", "REF-001"))
                '   rdInsertParams.Add(New System.Data.SqlClient.SqlParameter("@af_ref", "REF-001"))
                '   rdExtraColsSql &= ", u_referencia"
                '   rdExtraValsSql &= ", @af_ref"
                '
                ExecuteNonQueryTx(
                    "INSERT INTO rd (rdstamp, nmdoc, rno, rdata, nome, morada, local, codpost, ncont, zona, nib, total, etotal, base, ebase, ndoc, no, rdano, olcodigo, moeda, contado, ollocal, descricao, cm, cmdesc, ousrinis, ousrdata, ousrhora, usrinis, usrdata, usrhora" & rdExtraColsSql & ") " & _
                    "VALUES (@rdstamp, @nmdoc, @rno, @rdata, @nome, @morada, @local, @codpost, @ncont, @zona, @nib, @total, @etotal, @base, @ebase, @ndoc, @no, @rdano, @olcodigo, @moeda, @contado, @ollocal, @descricao, @cm, @cmdesc, @ousrinis, @ousrdata, @ousrhora, @usrinis, @usrdata, @usrhora" & rdExtraValsSql & ")",
                    rdInsertParams, txConnection, tx)

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

    Dim okResponse As New Newtonsoft.Json.Linq.JObject
    okResponse("success") = True
    okResponse("clientId") = clientId
    okResponse("bankAccountId") = bankAccountId
    okResponse("ndoc") = docTypeId
    okResponse("stamp") = rdStamp
    okResponse("rno") = rdRno
    okResponse("rdano") = anoProcessamento
    okResponse("total") = Newtonsoft.Json.Linq.JToken.FromObject(amount)
    okResponse("addFields") = If(rdAddFields IsNot Nothing, CType(rdAddFields, Newtonsoft.Json.Linq.JToken), Newtonsoft.Json.Linq.JValue.CreateNull())
    response = okResponse

Catch e As Exception
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject
    errorResponse("success") = False
    errorResponse("code") = "RD001"
    errorResponse("message") = e.Message
    response = errorResponse
End Try

Return response
