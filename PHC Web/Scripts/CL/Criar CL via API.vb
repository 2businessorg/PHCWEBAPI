'script=insertClAPI

' ─────────────────────────────────────────────────────────────────────────────
' Utilitários de base de dados
' ─────────────────────────────────────────────────────────────────────────────

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

' ─────────────────────────────────────────────────────────────────────────────
' ApplyAddFieldsToInsert
'   Preenche extraCols/extraParams com campos de utilizador para incluir no INSERT.
'   columnName deve seguir convenção PHC: us_<nome> (previne SQL injection).
' ─────────────────────────────────────────────────────────────────────────────
Dim ApplyAddFieldsToInsert = Sub(ByVal extraCols As List(Of String), ByVal extraParams As List(Of System.Data.SqlClient.SqlParameter), ByVal fieldsObj As Newtonsoft.Json.Linq.JObject)
    If fieldsObj Is Nothing OrElse fieldsObj.Count = 0 Then Exit Sub

    Dim paramIndex As Integer = extraParams.Count
    For Each prop As Newtonsoft.Json.Linq.JProperty In fieldsObj.Properties()
        Dim columnName As String = prop.Name
        If String.IsNullOrWhiteSpace(columnName) Then Continue For
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

' ─────────────────────────────────────────────────────────────────────────────
' GenerateStamp — 25 caracteres no formato PHC (yyyyMMddHHmmss + 11 aleatórios)
' ─────────────────────────────────────────────────────────────────────────────
Dim GenerateStamp = Function() As String
    Dim dt As DateTime = DateTime.Now
    Dim ts As String = dt.ToString("yyyyMMddHHmmss")
    Dim rnd As String = Guid.NewGuid().ToString("N").Substring(0, 11).ToUpper()
    Return (ts & rnd).Substring(0, 25)
End Function

' ─────────────────────────────────────────────────────────────────────────────
' NormalizeString — trim + trunca ao comprimento máximo
' ─────────────────────────────────────────────────────────────────────────────
Dim NormalizeString = Function(ByVal s As String, ByVal maxLen As Integer) As String
    If s Is Nothing Then Return String.Empty
    Dim t As String = s.Trim()
    Return If(t.Length > maxLen, t.Substring(0, maxLen), t)
End Function

' ─────────────────────────────────────────────────────────────────────────────
' BuildAddFieldsResponse — junta cl e cl2 addFields numa JObject plana para resposta
' ─────────────────────────────────────────────────────────────────────────────
Dim BuildAddFieldsResponse = Function(ByVal clFields As Newtonsoft.Json.Linq.JObject, ByVal cl2Fields As Newtonsoft.Json.Linq.JObject) As Newtonsoft.Json.Linq.JObject
    Dim result As New Newtonsoft.Json.Linq.JObject()
    If clFields IsNot Nothing Then
        For Each prop As Newtonsoft.Json.Linq.JProperty In clFields.Properties()
            result(prop.Name) = prop.Value
        Next
    End If
    If cl2Fields IsNot Nothing Then
        For Each prop As Newtonsoft.Json.Linq.JProperty In cl2Fields.Properties()
            result(prop.Name) = prop.Value
        Next
    End If
    Return result
End Function

Dim response As Object

Try
    ' ─────────────────────────────────────────────────────────────────────────
    ' Leitura do parâmetro XML/JSON
    ' ─────────────────────────────────────────────────────────────────────────
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
                If column = "val" Then jsonVal = reader.Value
        End Select
    Loop

    ' ─────────────────────────────────────────────────────────────────────────
    ' Parse JSON de entrada
    ' Formato esperado (ver valDeEntrada.json):
    ' {
    '   "id":       0,               ← 0 = auto-increment
    '   "branch":   0,
    '   "name":     "Cliente Lda.",
    '   "nuit":     "400123456",
    '   "phone":    "841234567",
    '   "address":  "Rua ...",
    '   "email":    "a@b.com",
    '   "inactive": false,
    '   "createdBy":"PHCAPI",
    '   "addFieldsByTable": {
    '     "cl":  { "us_segmento": "PME" },
    '     "cl2": { "us_limcredito": 50000 }
    '   }
    ' }
    ' ─────────────────────────────────────────────────────────────────────────
    Dim jsonObj As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.Linq.JObject.Parse(jsonVal)

    ' Validações obrigatórias
    If jsonObj("name") Is Nothing OrElse String.IsNullOrWhiteSpace(jsonObj("name").ToString()) Then
        Throw New Exception("O campo 'name' é obrigatório.")
    End If
    If jsonObj("nuit") Is Nothing OrElse String.IsNullOrWhiteSpace(jsonObj("nuit").ToString()) Then
        Throw New Exception("O campo 'nuit' é obrigatório.")
    End If

    ' Campos obrigatórios
    Dim nome      As String  = NormalizeString(jsonObj("name").ToString(), 55)
    Dim ncont     As String  = NormalizeString(jsonObj("nuit").ToString(), 20)

    ' Campos opcionais
    Dim telefone  As String  = NormalizeString(If(jsonObj("phone")   IsNot Nothing, jsonObj("phone").ToString(),   ""), 60)
    Dim morada    As String  = NormalizeString(If(jsonObj("address") IsNot Nothing, jsonObj("address").ToString(), ""), 55)
    Dim email     As String  = NormalizeString(If(jsonObj("email")   IsNot Nothing, jsonObj("email").ToString(),   ""), 100)
    Dim isInactive As Boolean = If(jsonObj("inactive") IsNot Nothing, Convert.ToBoolean(jsonObj("inactive")), False)
    Dim utilizador As String  = NormalizeString(If(jsonObj("createdBy") IsNot Nothing, jsonObj("createdBy").ToString(), "PHCAPI"), 30)

    Dim requestedNo    As Decimal = If(jsonObj("id")     IsNot Nothing, Convert.ToDecimal(jsonObj("id")),     0D)
    Dim requestedEstab As Decimal = If(jsonObj("branch") IsNot Nothing, Convert.ToDecimal(jsonObj("branch")), 0D)

    ' addFieldsByTable → cl e cl2 separados
    Dim addFieldsByTable As Newtonsoft.Json.Linq.JObject = TryCast(jsonObj("addFieldsByTable"), Newtonsoft.Json.Linq.JObject)
    Dim clAddFields  As Newtonsoft.Json.Linq.JObject = Nothing
    Dim cl2AddFields As Newtonsoft.Json.Linq.JObject = Nothing
    If addFieldsByTable IsNot Nothing Then
        clAddFields  = TryCast(addFieldsByTable("cl"),  Newtonsoft.Json.Linq.JObject)
        cl2AddFields = TryCast(addFieldsByTable("cl2"), Newtonsoft.Json.Linq.JObject)
    End If

    ' ─────────────────────────────────────────────────────────────────────────
    ' Determinar No do cliente
    ' ─────────────────────────────────────────────────────────────────────────
    Dim clNo    As Decimal
    Dim clEstab As Decimal = requestedEstab

    If requestedNo = 0 Then
        ' Auto-increment: MAX(no) + 1
        Dim maxNoTable As DataTable = ExecuteQuery(
            "SELECT ISNULL(MAX(no), 0) AS maxno FROM cl",
            Nothing)
        clNo = Convert.ToDecimal(maxNoTable.Rows(0)("maxno")) + 1D
    Else
        clNo = requestedNo
    End If

    Dim stamp As String = GenerateStamp()
    Dim now   As DateTime = DateTime.Now

    ' ─────────────────────────────────────────────────────────────────────────
    ' AJUSTE DE VARIÁVEIS (personalize aqui antes do INSERT)
    ' ─────────────────────────────────────────────────────────────────────────
    ' Este bloco é executado ANTES de qualquer INSERT.
    ' Modifique as variáveis acima conforme necessário para transformar os dados
    ' antes de os persistir.
    '
    ' Exemplos comuns:
    '   nome     = StrConv(nome, vbProperCase)        ' Capitalizar nome
    '   ncont    = ncont.PadLeft(9, "0"c)             ' Nuit com 9 dígitos
    '   morada   = morada.ToUpper()                    ' Morada em maiúsculas
    '   email    = email.ToLower()                     ' Email em minúsculas
    '   telefone = telefone.Replace(" ", "")           ' Remover espaços do telefone
    '   utilizador = "PHCAPI"                          ' Forçar utilizador de auditoria
    '
    ' [Adicione os seus ajustes aqui]
    ' ─────────────────────────────────────────────────────────────────────────

    ' ─────────────────────────────────────────────────────────────────────────
    ' INSERT em cl2 (primeiro — partilha o mesmo clstamp)
    ' ─────────────────────────────────────────────────────────────────────────
    Dim cl2BaseCols As New List(Of String) From {
        "cl2stamp",
        "ousrinis", "ousrdata", "ousrhora",
        "usrinis",  "usrdata",  "usrhora"
    }

    Dim cl2BaseParams As New List(Of System.Data.SqlClient.SqlParameter) From {
        New System.Data.SqlClient.SqlParameter("@cl2stamp",    stamp),
        New System.Data.SqlClient.SqlParameter("@cl2ousrinis", utilizador),
        New System.Data.SqlClient.SqlParameter("@cl2ousrdata", now.Date),
        New System.Data.SqlClient.SqlParameter("@cl2ousrhora", now.ToString("HH:mm:ss")),
        New System.Data.SqlClient.SqlParameter("@cl2usrinis",  utilizador),
        New System.Data.SqlClient.SqlParameter("@cl2usrdata",  now.Date),
        New System.Data.SqlClient.SqlParameter("@cl2usrhora",  now.ToString("HH:mm:ss"))
    }

    Dim cl2ExtraCols   As New List(Of String)()
    Dim cl2ExtraParams As New List(Of System.Data.SqlClient.SqlParameter)()
    ApplyAddFieldsToInsert(cl2ExtraCols, cl2ExtraParams, cl2AddFields)

    Dim cl2ColParts   As New List(Of String)()
    Dim cl2ParamParts As New List(Of String)()
    For Each col As String In cl2BaseCols   : cl2ColParts.Add("[" & col & "]")               : Next
    For Each col As String In cl2ExtraCols  : cl2ColParts.Add("[" & col & "]")               : Next
    For Each p   As System.Data.SqlClient.SqlParameter In cl2BaseParams  : cl2ParamParts.Add(p.ParameterName) : Next
    For Each p   As System.Data.SqlClient.SqlParameter In cl2ExtraParams : cl2ParamParts.Add(p.ParameterName) : Next

    Dim cl2InsertSql As String = "INSERT INTO [dbo].[cl2] (" & String.Join(", ", cl2ColParts) & ") VALUES (" & String.Join(", ", cl2ParamParts) & ")"

    Dim cl2AllParams As New List(Of System.Data.SqlClient.SqlParameter)(cl2BaseParams)
    cl2AllParams.AddRange(cl2ExtraParams)

    Dim cl2Rows As Integer = ExecuteNonQuery(cl2InsertSql, cl2AllParams)
    If cl2Rows = 0 Then Throw New Exception("Nenhum registo foi inserido na tabela cl2.")

    ' ─────────────────────────────────────────────────────────────────────────
    ' INSERT em cl (depois de cl2)
    ' ─────────────────────────────────────────────────────────────────────────
    Dim clBaseCols As New List(Of String) From {
        "clstamp", "no",       "estab",    "nome",     "ncont",
        "telefone","morada",   "email",    "inactivo",
        "ousrinis","ousrdata", "ousrhora",
        "usrinis", "usrdata",  "usrhora"
    }

    Dim clBaseParams As New List(Of System.Data.SqlClient.SqlParameter) From {
        New System.Data.SqlClient.SqlParameter("@clstamp",  stamp),
        New System.Data.SqlClient.SqlParameter("@no",       clNo),
        New System.Data.SqlClient.SqlParameter("@estab",    clEstab),
        New System.Data.SqlClient.SqlParameter("@nome",     nome),
        New System.Data.SqlClient.SqlParameter("@ncont",    ncont),
        New System.Data.SqlClient.SqlParameter("@telefone", telefone),
        New System.Data.SqlClient.SqlParameter("@morada",   morada),
        New System.Data.SqlClient.SqlParameter("@email",    email),
        New System.Data.SqlClient.SqlParameter("@inactivo", isInactive),
        New System.Data.SqlClient.SqlParameter("@ousrinis", utilizador),
        New System.Data.SqlClient.SqlParameter("@ousrdata", now.Date),
        New System.Data.SqlClient.SqlParameter("@ousrhora", now.ToString("HH:mm:ss")),
        New System.Data.SqlClient.SqlParameter("@usrinis",  utilizador),
        New System.Data.SqlClient.SqlParameter("@usrdata",  now.Date),
        New System.Data.SqlClient.SqlParameter("@usrhora",  now.ToString("HH:mm:ss"))
    }

    Dim clExtraCols   As New List(Of String)()
    Dim clExtraParams As New List(Of System.Data.SqlClient.SqlParameter)()
    ApplyAddFieldsToInsert(clExtraCols, clExtraParams, clAddFields)

    Dim clColParts   As New List(Of String)()
    Dim clParamParts As New List(Of String)()
    For Each col As String In clBaseCols   : clColParts.Add("[" & col & "]")               : Next
    For Each col As String In clExtraCols  : clColParts.Add("[" & col & "]")               : Next
    For Each p   As System.Data.SqlClient.SqlParameter In clBaseParams  : clParamParts.Add(p.ParameterName) : Next
    For Each p   As System.Data.SqlClient.SqlParameter In clExtraParams : clParamParts.Add(p.ParameterName) : Next

    Dim clInsertSql As String = "INSERT INTO [dbo].[cl] (" & String.Join(", ", clColParts) & ") VALUES (" & String.Join(", ", clParamParts) & ")"

    Dim clAllParams As New List(Of System.Data.SqlClient.SqlParameter)(clBaseParams)
    clAllParams.AddRange(clExtraParams)

    Dim clRows As Integer = ExecuteNonQuery(clInsertSql, clAllParams)
    If clRows = 0 Then Throw New Exception("Nenhum registo foi inserido na tabela cl.")

    ' ─────────────────────────────────────────────────────────────────────────
    ' Construir resposta
    ' ─────────────────────────────────────────────────────────────────────────
    Dim allAddFields As Newtonsoft.Json.Linq.JObject = BuildAddFieldsResponse(clAddFields, cl2AddFields)

    Dim itemOut As New Newtonsoft.Json.Linq.JObject()
    itemOut("id")       = clNo
    itemOut("branch")   = clEstab
    itemOut("name")     = nome
    itemOut("nuit")     = ncont
    itemOut("phone")    = telefone
    itemOut("address")  = morada
    itemOut("email")    = email
    itemOut("inactive") = isInactive
    If allAddFields.Count > 0 Then
        itemOut("addFields") = allAddFields
    Else
        itemOut("addFields") = Nothing
    End If

    Dim okResponse As New Newtonsoft.Json.Linq.JObject()
    okResponse("success") = True
    okResponse("item")    = itemOut
    response = okResponse

Catch e As Exception
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject()
    errorResponse("success") = False
    errorResponse("code")    = "CL001"
    errorResponse("message") = e.Message
    response = errorResponse
End Try

Return response.ToString()
