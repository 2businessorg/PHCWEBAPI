'script=insertStAPI

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

' GenerateStamp: gera ststamp de 25 caracteres no formato PHC (yyyyMMddHHmmss + 11 chars aleatórios)
Dim GenerateStamp = Function() As String
    Dim dt As DateTime = DateTime.Now
    Dim ts As String = dt.ToString("yyyyMMddHHmmss")
    Dim rnd As String = Guid.NewGuid().ToString("N").Substring(0, 11).ToUpper()
    Return (ts & rnd).Substring(0, 25)
End Function

' NormalizeString: limpa e trunca string ao comprimento máximo
Dim NormalizeString = Function(ByVal s As String, ByVal maxLen As Integer) As String
    If s Is Nothing Then Return String.Empty
    Dim t As String = s.Trim()
    Return If(t.Length > maxLen, t.Substring(0, maxLen), t)
End Function

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

    ' JSON mínimo para criação de ST:
    ' {
    '   "reference": "ART-001",
    '   "description": "Artigo de Teste",
    '   "isService": false,
    '   "prices": [
    '     { "table": 1, "value": 1250.0, "isTaxIncluded": true }
    '   ],
    '   "taxTableId": 1,
    '   "familyRef": "FAM001",
    '   "observations": "...",
    '   "isInactive": false,
    '   "addFields": { "u_color": "blue", "u_netWeight": 1.25 }
    ' }

    ' Validar campos obrigatórios
    If jsonObj("reference") Is Nothing OrElse String.IsNullOrWhiteSpace(jsonObj("reference").ToString()) Then
        Throw New Exception("O campo 'reference' é obrigatório.")
    End If
    If jsonObj("description") Is Nothing OrElse String.IsNullOrWhiteSpace(jsonObj("description").ToString()) Then
        Throw New Exception("O campo 'description' é obrigatório.")
    End If

    ' Campos obrigatórios
    Dim reference As String = NormalizeString(jsonObj("reference").ToString(), 18)
    Dim description As String = NormalizeString(jsonObj("description").ToString(), 60)

    ' Campos opcionais
    Dim isService As Boolean = If(jsonObj("isService") IsNot Nothing, Convert.ToBoolean(jsonObj("isService")), False)
    Dim taxTableId As Integer = If(jsonObj("taxTableId") IsNot Nothing AndAlso Convert.ToInt32(jsonObj("taxTableId")) > 0, Convert.ToInt32(jsonObj("taxTableId")), 1)
    Dim familyRef As String = NormalizeString(If(jsonObj("familyRef") IsNot Nothing, jsonObj("familyRef").ToString(), ""), 18)
    Dim observations As String = NormalizeString(If(jsonObj("observations") IsNot Nothing, jsonObj("observations").ToString(), ""), 255)
    Dim isInactive As Boolean = If(jsonObj("isInactive") IsNot Nothing, Convert.ToBoolean(jsonObj("isInactive")), False)

    ' Campos de utilizador dinâmicos do artigo ST (opcional)
    Dim stAddFields As Newtonsoft.Json.Linq.JObject = TryCast(jsonObj("addFields"), Newtonsoft.Json.Linq.JObject)

    ' Preços por tabela (table 1..5 → pv1..pv5 / epv1..epv5 / iva1incl..iva5incl)
    Dim pv1 As Decimal = 0D, pv2 As Decimal = 0D, pv3 As Decimal = 0D, pv4 As Decimal = 0D, pv5 As Decimal = 0D
    Dim iva1incl As Boolean = False, iva2incl As Boolean = False, iva3incl As Boolean = False, iva4incl As Boolean = False, iva5incl As Boolean = False

    Dim pricesArray As Newtonsoft.Json.Linq.JArray = TryCast(jsonObj("prices"), Newtonsoft.Json.Linq.JArray)
    If pricesArray IsNot Nothing Then
        For Each priceToken As Newtonsoft.Json.Linq.JToken In pricesArray
            If priceToken("table") Is Nothing OrElse priceToken("value") Is Nothing Then Continue For
            Dim tbl As Integer = Convert.ToInt32(priceToken("table"))
            Dim val As Decimal = Convert.ToDecimal(priceToken("value"))
            Dim taxIncl As Boolean = If(priceToken("isTaxIncluded") IsNot Nothing, Convert.ToBoolean(priceToken("isTaxIncluded")), False)
            Select Case tbl
                Case 1 : pv1 = val : iva1incl = taxIncl
                Case 2 : pv2 = val : iva2incl = taxIncl
                Case 3 : pv3 = val : iva3incl = taxIncl
                Case 4 : pv4 = val : iva4incl = taxIncl
                Case 5 : pv5 = val : iva5incl = taxIncl
            End Select
        Next
    End If

    ' Verificar se referência já existe em st
    Dim existsTable As DataTable = ExecuteQuery(
        "SELECT TOP 1 ref FROM st WHERE LTRIM(RTRIM(LOWER(ref))) = LTRIM(RTRIM(LOWER(@ref)))",
        New List(Of System.Data.SqlClient.SqlParameter) From {
            New System.Data.SqlClient.SqlParameter("@ref", reference)
        })

    If existsTable.Rows.Count > 0 Then
        Throw New Exception($"Já existe um artigo com referência '{reference}'.")
    End If

    ' Obter nome da família (se fornecida)
    Dim familyName As String = String.Empty
    If Not String.IsNullOrWhiteSpace(familyRef) Then
        Dim famTable As DataTable = ExecuteQuery(
            "SELECT TOP 1 nome faminome FROM STFAMI WHERE ref = LTRIM(RTRIM(@familia))",
            New List(Of System.Data.SqlClient.SqlParameter) From {
                New System.Data.SqlClient.SqlParameter("@familia", familyRef)
            })
        If famTable.Rows.Count > 0 Then
            familyName = NormalizeString(famTable.Rows(0)("faminome").ToString(), 60)
        End If
    End If

    Dim now As DateTime = DateTime.Now
    Dim utilizador As String = NormalizeString(If(jsonObj("createdBy") IsNot Nothing, jsonObj("createdBy").ToString(), "PHCAPI"), 30)
    Dim ststamp As String = GenerateStamp()

    ' Colunas e parâmetros base do INSERT em st
    Dim baseCols As New List(Of String) From {
        "ststamp", "ref",      "design",   "stns",     "familia",  "faminome",
        "pv1",     "epv1",     "iva1incl",
        "pv2",     "epv2",     "iva2incl",
        "pv3",     "epv3",     "iva3incl",
        "pv4",     "epv4",     "iva4incl",
        "pv5",     "epv5",     "iva5incl",
        "tabiva",  "obs",      "inactivo",
        "ousrinis","ousrdata", "ousrhora",
        "usrinis", "usrdata",  "usrhora"
    }

    Dim baseParams As New List(Of System.Data.SqlClient.SqlParameter) From {
        New System.Data.SqlClient.SqlParameter("@ststamp",  ststamp),
        New System.Data.SqlClient.SqlParameter("@ref",      reference),
        New System.Data.SqlClient.SqlParameter("@design",   description),
        New System.Data.SqlClient.SqlParameter("@stns",     isService),
        New System.Data.SqlClient.SqlParameter("@familia",  familyRef),
        New System.Data.SqlClient.SqlParameter("@faminome", familyName),
        New System.Data.SqlClient.SqlParameter("@pv1",      pv1),
        New System.Data.SqlClient.SqlParameter("@epv1",     pv1),
        New System.Data.SqlClient.SqlParameter("@iva1incl", iva1incl),
        New System.Data.SqlClient.SqlParameter("@pv2",      pv2),
        New System.Data.SqlClient.SqlParameter("@epv2",     pv2),
        New System.Data.SqlClient.SqlParameter("@iva2incl", iva2incl),
        New System.Data.SqlClient.SqlParameter("@pv3",      pv3),
        New System.Data.SqlClient.SqlParameter("@epv3",     pv3),
        New System.Data.SqlClient.SqlParameter("@iva3incl", iva3incl),
        New System.Data.SqlClient.SqlParameter("@pv4",      pv4),
        New System.Data.SqlClient.SqlParameter("@epv4",     pv4),
        New System.Data.SqlClient.SqlParameter("@iva4incl", iva4incl),
        New System.Data.SqlClient.SqlParameter("@pv5",      pv5),
        New System.Data.SqlClient.SqlParameter("@epv5",     pv5),
        New System.Data.SqlClient.SqlParameter("@iva5incl", iva5incl),
        New System.Data.SqlClient.SqlParameter("@tabiva",   taxTableId),
        New System.Data.SqlClient.SqlParameter("@obs",      observations),
        New System.Data.SqlClient.SqlParameter("@inactivo", isInactive),
        New System.Data.SqlClient.SqlParameter("@ousrinis", utilizador),
        New System.Data.SqlClient.SqlParameter("@ousrdata", now.Date),
        New System.Data.SqlClient.SqlParameter("@ousrhora", now.ToString("HH:mm:ss")),
        New System.Data.SqlClient.SqlParameter("@usrinis",  utilizador),
        New System.Data.SqlClient.SqlParameter("@usrdata",  now.Date),
        New System.Data.SqlClient.SqlParameter("@usrhora",  now.ToString("HH:mm:ss"))
    }

    ' Colunas e parâmetros dos addFields (campos de utilizador)
    Dim extraCols As New List(Of String)()
    Dim extraParams As New List(Of System.Data.SqlClient.SqlParameter)()
    ApplyAddFieldsToInsert(extraCols, extraParams, stAddFields)

    ' Construir SQL de INSERT dinamicamente
    Dim colParts As New List(Of String)()
    For Each col As String In baseCols
        colParts.Add("[" & col & "]")
    Next
    For Each col As String In extraCols
        colParts.Add("[" & col & "]")
    Next

    Dim paramParts As New List(Of String)()
    For Each p As System.Data.SqlClient.SqlParameter In baseParams
        paramParts.Add(p.ParameterName)
    Next
    For Each p As System.Data.SqlClient.SqlParameter In extraParams
        paramParts.Add(p.ParameterName)
    Next

    Dim insertSql As String = "INSERT INTO [dbo].[st] (" & String.Join(", ", colParts) & ") VALUES (" & String.Join(", ", paramParts) & ")"

    ' Unir todos os parâmetros e executar INSERT
    Dim allParams As New List(Of System.Data.SqlClient.SqlParameter)(baseParams)
    allParams.AddRange(extraParams)

    Dim rowsAffected As Integer = ExecuteNonQuery(insertSql, allParams)
    If rowsAffected = 0 Then
        Throw New Exception("Nenhum registo foi inserido na tabela st.")
    End If

    ' Construir array de preços para a resposta (apenas tabelas com valor > 0)
    Dim pricesOut As New Newtonsoft.Json.Linq.JArray()
    If pv1 > 0D Then pricesOut.Add(New Newtonsoft.Json.Linq.JObject From {{"table", 1}, {"value", pv1}, {"isTaxIncluded", iva1incl}})
    If pv2 > 0D Then pricesOut.Add(New Newtonsoft.Json.Linq.JObject From {{"table", 2}, {"value", pv2}, {"isTaxIncluded", iva2incl}})
    If pv3 > 0D Then pricesOut.Add(New Newtonsoft.Json.Linq.JObject From {{"table", 3}, {"value", pv3}, {"isTaxIncluded", iva3incl}})
    If pv4 > 0D Then pricesOut.Add(New Newtonsoft.Json.Linq.JObject From {{"table", 4}, {"value", pv4}, {"isTaxIncluded", iva4incl}})
    If pv5 > 0D Then pricesOut.Add(New Newtonsoft.Json.Linq.JObject From {{"table", 5}, {"value", pv5}, {"isTaxIncluded", iva5incl}})

    Dim itemOut As New Newtonsoft.Json.Linq.JObject()
    itemOut("reference")   = reference
    itemOut("description") = description
    itemOut("isService")   = isService
    itemOut("prices")      = pricesOut
    itemOut("quantity")    = 0D
    itemOut("taxTableId")  = taxTableId
    itemOut("familyRef")   = familyRef
    itemOut("familyName")  = familyName
    itemOut("observations")= observations
    itemOut("isInactive")  = isInactive
    itemOut("useBatches")  = False
    ' Ecoar addFields no objecto de saída final
    If stAddFields IsNot Nothing AndAlso stAddFields.Count > 0 Then
        itemOut("addFields") = stAddFields
    End If

    Dim okResponse As New Newtonsoft.Json.Linq.JObject()
    okResponse("success") = True
    okResponse("item") = itemOut
    response = okResponse

Catch e As Exception
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject()
    errorResponse("success") = False
    errorResponse("code") = "ST001"
    errorResponse("message") = e.Message
    response = errorResponse
End Try

Return response
