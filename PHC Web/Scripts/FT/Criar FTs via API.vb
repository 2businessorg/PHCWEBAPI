'script=insertFtAPI

Dim ExecuteQuery = Function(ByVal fQuery As String, ByVal fSqlParameters As List(Of System.Data.SqlClient.SqlParameter)) as DataTable
    Dim table As New DataTable
    Using connection as SqlClient.SqlConnection = SqlHelp.GetNewConnection()
		Using command As New SqlClient.SqlCommand(fQuery, connection)
			For Each sqlParameter As System.Data.SqlClient.SqlParameter in fSqlParameters
                command.Parameters.Add(sqlParameter)
            Next
            Using adapter As New SqlClient.SqlDataAdapter(command)
                adapter.Fill(table)
            End Using
		End Using
	End Using
    return table
End Function

Dim ApplyUserFieldsToRow = Sub(ByVal targetRow As DataRow, ByVal fieldsObj As Newtonsoft.Json.Linq.JObject, ByVal tableName As String)
    If targetRow Is Nothing OrElse fieldsObj Is Nothing Then
        Exit Sub
    End If

    For Each prop As Newtonsoft.Json.Linq.JProperty In fieldsObj.Properties()
        Dim columnName As String = prop.Name

        If String.IsNullOrWhiteSpace(columnName) Then
            Continue For
        End If

        If Not targetRow.Table.Columns.Contains(columnName) Then
            Continue For
        End If

        Dim token As Newtonsoft.Json.Linq.JToken = prop.Value

        If token Is Nothing OrElse token.Type = Newtonsoft.Json.Linq.JTokenType.Null Then
            targetRow(columnName) = DBNull.Value
            Continue For
        End If

        Dim targetType As Type = targetRow.Table.Columns(columnName).DataType
        Dim rawValue As Object = token.ToObject(Of Object)()

        If rawValue Is Nothing Then
            targetRow(columnName) = DBNull.Value
            Continue For
        End If

        Try
            If targetType Is GetType(String) Then
                targetRow(columnName) = token.ToString()
            ElseIf targetType Is GetType(DateTime) Then
                targetRow(columnName) = Convert.ToDateTime(rawValue)
            ElseIf targetType Is GetType(Boolean) Then
                targetRow(columnName) = Convert.ToBoolean(rawValue)
            Else
                targetRow(columnName) = Convert.ChangeType(rawValue, targetType, System.Globalization.CultureInfo.InvariantCulture)
            End If
        Catch convertEx As Exception
            Throw New Exception("Erro ao converter campo de utilizador '" & columnName & "' da tabela '" & tableName & "': " & convertEx.Message)
        End Try
    Next
End Sub

Dim DataRowToJsonObject = Function(ByVal sourceRow As DataRow) As Newtonsoft.Json.Linq.JObject
    Dim rowObj As New Newtonsoft.Json.Linq.JObject
    If sourceRow Is Nothing OrElse sourceRow.Table Is Nothing Then
        Return rowObj
    End If

    For Each col As DataColumn In sourceRow.Table.Columns
        Dim cellValue As Object = sourceRow(col)
        If cellValue Is DBNull.Value OrElse cellValue Is Nothing Then
            rowObj(col.ColumnName) = Newtonsoft.Json.Linq.JValue.CreateNull()
        Else
            rowObj(col.ColumnName) = Newtonsoft.Json.Linq.JToken.FromObject(cellValue)
        End If
    Next

    Return rowObj
End Function

mstamp = "<val>" & mstamp & "</val>"
Dim response as Object

Try
    Dim stream As New System.IO.MemoryStream(Encoding.UTF8.GetBytes(mstamp))
    Dim reader As System.Xml.XmlReader = New System.Xml.XmlTextReader(stream)

    Dim jsonVal As String = ""
    Dim column As String = ""
    
    Do While (reader.Read())
        Select Case reader.NodeType
            Case System.Xml.XmlNodeType.Element
                column = reader.Name.ToLower
            Case System.Xml.XmlNodeType.Text
                Dim value As String = reader.Value
                Select Case column
                    Case "val"
                        jsonVal = value
                End Select
        End Select
    Loop

    Dim jsonObj As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.Linq.JObject.Parse(jsonVal)
   
    ' ==========================================
    ' EXTRACTING REQUIRED AND OPTIONAL FIELDS
    ' ==========================================
    Dim docType   As Integer   = Convert.ToInt32(jsonObj("ndoc"))
    Dim clNo      As Integer   = Convert.ToInt32(jsonObj("no"))
    Dim ftano     As Integer   = If(jsonObj("ftano").Type <> Newtonsoft.Json.Linq.JTokenType.Null, Convert.ToInt32(jsonObj("ftano")), DateTime.Now.Year)
    Dim estab     As Integer   = If(jsonObj("estab").Type <> Newtonsoft.Json.Linq.JTokenType.Null, Convert.ToInt32(jsonObj("estab")), 0)
    Dim nome      As String    = If(jsonObj("nome").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("nome").ToString(), "")
    Dim fdata     As DateTime  = If(jsonObj("data").Type <> Newtonsoft.Json.Linq.JTokenType.Null, DateTime.Parse(jsonObj("data").ToString()), Date.Today)
    Dim moeda     As String    = If(jsonObj("moeda").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("moeda").ToString(), "")
    Dim utilizador As String    = If(jsonObj("createdBy") IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(jsonObj("createdBy").ToString()), jsonObj("createdBy").ToString().Trim(), "PHCAPI")
    If utilizador.Length > 30 Then utilizador = utilizador.Substring(0, 30)

    ' Criação do documento de fatura
    Dim CreateFtDocWs As bizlib.ftclass.CreateFTDoc
    CreateFtDocWs = New bizlib.ftclass.CreateFTDoc(docType, clNo)

    CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("ftano") = ftano
    CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("fdata") = fdata

    If estab > 0 Then
        CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("estab") = estab
    End If
    
    If Not String.IsNullOrEmpty(nome) Then
        CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("nome") = nome
    End If

    If Not String.IsNullOrEmpty(moeda) Then
        CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("moeda") = moeda
    End If

    ' Auditar utilizador que criou o documento (via API)
    CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("ousrinis") = utilizador
    CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("usrinis")  = utilizador

    ' Campos de utilizador dinâmicos do cabeçalho (ft/ft2/ft3)
    Dim addFieldsByTable As Newtonsoft.Json.Linq.JObject = TryCast(jsonObj("AddFieldsByTable"), Newtonsoft.Json.Linq.JObject)
    If addFieldsByTable IsNot Nothing Then
        Dim ftFields  As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable("ft"),  Newtonsoft.Json.Linq.JObject)
        Dim ft2Fields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable("ft2"), Newtonsoft.Json.Linq.JObject)
        Dim ft3Fields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable("ft3"), Newtonsoft.Json.Linq.JObject)
        
        ApplyUserFieldsToRow(CreateFtDocWs.MainformDataset.Tables(0).Rows(0), ftFields,  "ft")
        ApplyUserFieldsToRow(CreateFtDocWs.MainformDataset.Tables(1).Rows(0), ft2Fields, "ft2")
        ApplyUserFieldsToRow(CreateFtDocWs.MainformDataset.Tables(3).Rows(0), ft3Fields, "ft3")
    End If

    ' ==========================================
    ' ADICIONAR LINHAS DO DOCUMENTO
    ' ==========================================
    Dim lstFiJson As Newtonsoft.Json.Linq.JArray = TryCast(jsonObj("lstFi"), Newtonsoft.Json.Linq.JArray)
    If lstFiJson Is Nothing Then
        Throw New Exception("lstFi não encontrado ou inválido no payload.")
    End If

    Dim fi2Cursor As DataTable = CreateFtDocWs.MainFormDataSet.Tables(6)
    Dim lineIndex As Integer = 0
    Dim newrowWithRef As DataRow

    For Each lineToken As Newtonsoft.Json.Linq.JToken In lstFiJson
        Dim lineObj As Newtonsoft.Json.Linq.JObject = TryCast(lineToken, Newtonsoft.Json.Linq.JObject)
        If lineObj Is Nothing Then
            lineIndex += 1
            Continue For
        End If

        newrowWithRef = CreateFtDocWs.addLineref(lineObj("ref").ToString())
        newrowWithRef.Item("qtt") = Convert.ToDecimal(lineObj("qtt").ToString(), System.Globalization.CultureInfo.InvariantCulture)
        newrowWithRef.Item("armazem") = 1

        Dim ivaInclToken As Newtonsoft.Json.Linq.JToken = lineObj("ivaIncl")
        If ivaInclToken IsNot Nothing AndAlso ivaInclToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            newrowWithRef.Item("ivaincl") = Convert.ToBoolean(ivaInclToken.ToString())
        End If

        ' Campos opcionais da linha
        Dim designToken As Newtonsoft.Json.Linq.JToken = lineObj("design")
        If designToken IsNot Nothing AndAlso designToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            Dim designVal As String = designToken.ToString()
            If Not String.IsNullOrEmpty(designVal) Then
                newrowWithRef.Item("design") = designVal
            End If
        End If

        Dim pvToken As Newtonsoft.Json.Linq.JToken = lineObj("precoVenda")
        If pvToken IsNot Nothing AndAlso pvToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            newrowWithRef.Item("epv") = Convert.ToDecimal(pvToken.ToString(), System.Globalization.CultureInfo.InvariantCulture)
            newrowWithRef.Item("pv") = Convert.ToDecimal(pvToken.ToString(), System.Globalization.CultureInfo.InvariantCulture)
        End If

        Dim tabIvaToken As Newtonsoft.Json.Linq.JToken = lineObj("tabIva")
        If tabIvaToken IsNot Nothing AndAlso tabIvaToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            Dim tabIvaCode As Integer = Convert.ToInt32(tabIvaToken.ToString())
            newrowWithRef.Item("tabiva") = tabIvaCode

            Dim taxaParams As New List(Of System.Data.SqlClient.SqlParameter)
            taxaParams.Add(New System.Data.SqlClient.SqlParameter("@codigo", tabIvaCode))
            Dim taxaTable As DataTable = ExecuteQuery("SELECT taxa FROM taxasiva WHERE codigo = @codigo", taxaParams)
            If taxaTable.Rows.Count > 0 Then
                newrowWithRef.Item("iva") = Convert.ToDecimal(taxaTable.Rows(0)("taxa"))
            End If
        End If

        ' Campos de utilizador dinâmicos da linha (fi/fi2)
        Dim lineAddFieldsByTable As Newtonsoft.Json.Linq.JObject = TryCast(lineObj("AddFieldsByTable"), Newtonsoft.Json.Linq.JObject)
        If lineAddFieldsByTable IsNot Nothing Then
            Dim fiFields  As Newtonsoft.Json.Linq.JObject = TryCast(lineAddFieldsByTable("fi"),  Newtonsoft.Json.Linq.JObject)
            Dim fi2Fields As Newtonsoft.Json.Linq.JObject = TryCast(lineAddFieldsByTable("fi2"), Newtonsoft.Json.Linq.JObject)

            ApplyUserFieldsToRow(newrowWithRef, fiFields, "fi")

            If fi2Fields IsNot Nothing AndAlso fi2Cursor IsNot Nothing AndAlso lineIndex < fi2Cursor.Rows.Count Then
                ApplyUserFieldsToRow(fi2Cursor.Rows(lineIndex), fi2Fields, "fi2")
            End If
        End If

        lineIndex += 1
    Next

    ' GRAVAR DOCUMENTO
    CreateFtDocWs.Save()

    ' ==========================================
    ' CONSTRUÇÃO DA RESPOSTA JSON (SUCESSO)
    ' ==========================================
    Dim ftRow As DataRow = CreateFtDocWs.MainformDataset.Tables(0).Rows(0)
    Dim ft2Row As DataRow = CreateFtDocWs.MainformDataset.Tables(1).Rows(0)
    Dim ft3Row As DataRow = CreateFtDocWs.MainformDataset.Tables(3).Rows(0)
    Dim fiTable As DataTable = CreateFtDocWs.MainformDataset.Tables(2)
    
    Dim responseJson As New Newtonsoft.Json.Linq.JObject
    
    responseJson("nmdoc") = ftRow("nmdoc").ToString()
    responseJson("ndoc") = ftRow("ndoc").ToString()
    responseJson("fno") = ftRow("fno").ToString()
    responseJson("ftano") = ftRow("ftano").ToString()
    responseJson("no") = ftRow("no").ToString()
    responseJson("nome") = ftRow("Nome").ToString()
    responseJson("estab") = ftRow("Estab").ToString()
    responseJson("data") = Convert.ToDateTime(ftRow("fdata")).ToString("yyyy-MM-dd")
    responseJson("moeda") = ftRow("moeda").ToString()
    responseJson("total") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("total")))
    responseJson("totalMoeda") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("totalMoeda")))
    responseJson("ttiva") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("ttiva")))
    responseJson("tmIva") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("tmIva")))

    ' Campos de utilizador do cabeçalho lidos de volta após Save()
    If addFieldsByTable IsNot Nothing Then
        Dim headerUserFields As New Newtonsoft.Json.Linq.JObject
        Dim headerTableMap As New Dictionary(Of String, DataRow) From {
            {"ft",  ftRow},
            {"ft2", ft2Row},
            {"ft3", ft3Row}
        }
        For Each tablePair As KeyValuePair(Of String, DataRow) In headerTableMap
            Dim requestedFields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable(tablePair.Key), Newtonsoft.Json.Linq.JObject)
            If requestedFields IsNot Nothing Then
                Dim tableFieldsObj As New Newtonsoft.Json.Linq.JObject
                For Each prop As Newtonsoft.Json.Linq.JProperty In requestedFields.Properties()
                    If tablePair.Value.Table.Columns.Contains(prop.Name) Then
                        Dim cellValue As Object = tablePair.Value(prop.Name)
                        tableFieldsObj(prop.Name) = If(cellValue Is DBNull.Value OrElse cellValue Is Nothing,
                            Newtonsoft.Json.Linq.JValue.CreateNull(),
                            Newtonsoft.Json.Linq.JToken.FromObject(cellValue))
                    End If
                Next
                If tableFieldsObj.Count > 0 Then
                    headerUserFields(tablePair.Key) = tableFieldsObj
                End If
            End If
        Next
        If headerUserFields.Count > 0 Then
            responseJson("addFieldsByTable") = headerUserFields
        End If
    End If
    
    ' Array de linhas
    Dim linesArray As New Newtonsoft.Json.Linq.JArray
    Dim biRowIndex As Integer = 0
    
    For Each ftLineRow As DataRow In fiTable.Rows
        Dim lineObj As New Newtonsoft.Json.Linq.JObject
        
        lineObj("ref") = ftLineRow("ref").ToString().Trim()
        lineObj("design") = ftLineRow("design").ToString().Trim()
        lineObj("qtt") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftLineRow("qtt")))
        lineObj("tabiva") = ftLineRow("tabiva").ToString()
        lineObj("iva") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDouble(ftLineRow("iva")))
        lineObj("pv") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftLineRow("pv")))
        lineObj("pvmoeda") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftLineRow("pvmoeda")))
        lineObj("tiliquido") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftLineRow("tiliquido")))
        lineObj("tmoeda") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftLineRow("tmoeda")))

        ' Campos de utilizador da linha lidos de volta após Save()
        If lstFiJson IsNot Nothing AndAlso biRowIndex < lstFiJson.Count Then
            Dim srcLineObj As Newtonsoft.Json.Linq.JObject = TryCast(lstFiJson(biRowIndex), Newtonsoft.Json.Linq.JObject)
            Dim srcLineAddFields As Newtonsoft.Json.Linq.JObject = If(srcLineObj IsNot Nothing, TryCast(srcLineObj("AddFieldsByTable"), Newtonsoft.Json.Linq.JObject), Nothing)
            If srcLineAddFields IsNot Nothing Then
                Dim lineUserFields As New Newtonsoft.Json.Linq.JObject
                Dim lineTableMap As New Dictionary(Of String, DataRow)
                lineTableMap("fi") = ftLineRow
                If fi2Cursor IsNot Nothing AndAlso biRowIndex < fi2Cursor.Rows.Count Then
                    lineTableMap("fi2") = fi2Cursor.Rows(biRowIndex)
                End If
                For Each tablePair As KeyValuePair(Of String, DataRow) In lineTableMap
                    Dim requestedFields As Newtonsoft.Json.Linq.JObject = TryCast(srcLineAddFields(tablePair.Key), Newtonsoft.Json.Linq.JObject)
                    If requestedFields IsNot Nothing Then
                        Dim tableFieldsObj As New Newtonsoft.Json.Linq.JObject
                        For Each prop As Newtonsoft.Json.Linq.JProperty In requestedFields.Properties()
                            If tablePair.Value.Table.Columns.Contains(prop.Name) Then
                                Dim cellValue As Object = tablePair.Value(prop.Name)
                                tableFieldsObj(prop.Name) = If(cellValue Is DBNull.Value OrElse cellValue Is Nothing,
                                    Newtonsoft.Json.Linq.JValue.CreateNull(),
                                    Newtonsoft.Json.Linq.JToken.FromObject(cellValue))
                            End If
                        Next
                        If tableFieldsObj.Count > 0 Then
                            lineUserFields(tablePair.Key) = tableFieldsObj
                        End If
                    End If
                Next
                If lineUserFields.Count > 0 Then
                    lineObj("addFieldsByTable") = lineUserFields
                End If
            End If
        End If

        linesArray.Add(lineObj)
        biRowIndex += 1
    Next
    
    responseJson("linhas") = linesArray
    
    ' ==========================================
    ' RESPOSTA COM SUCESSO
    ' ==========================================
    Dim finalResponse As New Newtonsoft.Json.Linq.JObject
    finalResponse("success") = True
    finalResponse("data") = responseJson
    
    response = finalResponse

Catch e As Exception
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject
    errorResponse("success") = False
    errorResponse("code") = "FT001"
    errorResponse("message") = e.Message
    
    response = errorResponse
End Try

Return response


'MainFormDataSet.Tables:
' 0- FT
' 1- FT2
' 2- FI
' 3- FT3
' 4- FTRD
' 5- FTT
' 6- FI2
' 7- FTCC