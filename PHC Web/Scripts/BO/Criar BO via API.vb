'script=insertBoAPI

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



mstamp = "<val>" & mstamp & "</val>"
Dim response as Object
Dim ftstamp as String 

Try

    Dim stream As New System.IO.MemoryStream(Encoding.UTF8.GetBytes(mstamp))
    Dim reader As System.Xml.XmlReader = New System.Xml.XmlTextReader(stream)

    Dim jsonVal As String = ""
    Dim column As String = ""
    
    Do While (reader.Read())
        Select Case reader.NodeType
            Case System.Xml.XmlNodeType.Element
                ftstamp += reader.Name.ToLower
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
    ' Mapear os campos do JSON para variáveis VB
    Dim docType   As Integer   = Convert.ToInt32(jsonObj("Ndos"))
    Dim clNo      As Integer   = Convert.ToInt32(jsonObj("No"))
    
    ' Campos opcionais (verificar JTokenType.Null para valores null do JSON)
    Dim boano     As Integer   = If( jsonObj("Boano").Type <> Newtonsoft.Json.Linq.JTokenType.Null, Convert.ToInt32(jsonObj("Boano")), DateTime.Now.Year)
    Dim estab     As Integer   = If( jsonObj("Estab").Type <> Newtonsoft.Json.Linq.JTokenType.Null, Convert.ToInt32(jsonObj("Estab")), 0)
    Dim nome      As String    = If( jsonObj("Nome").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("Nome").ToString(), "")
    Dim dataObra  As DateTime  = If( jsonObj("Data").Type <> Newtonsoft.Json.Linq.JTokenType.Null, DateTime.Parse(jsonObj("Data").ToString()), Date.Today)
    Dim moeda     As String    = If( jsonObj("Moeda").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("Moeda").ToString(), "")
    Dim utilizador As String    = If(jsonObj("CreatedBy") IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(jsonObj("CreatedBy").ToString()), jsonObj("CreatedBy").ToString().Trim(), "PHCAPI")
    If utilizador.Length > 30 Then utilizador = utilizador.Substring(0, 30)
    
    Dim tdcursor As New DataTable
    tdcursor = PhcCommandBuilder.CreateSelect("*").From("ts").Where("ts.ndos = @ndos").AddSQLParameter("@ndos", docType).GetDataTable()
    
    ' Criação do dossier interno 
    Dim CreateBosDocWs As bizlib.boclass.CreateBODoc
    CreateBosDocWs = New bizlib.boclass.CreateBODoc(docType, clNo)

    ' Atribuir campos opcionais se fornecidos
    CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("Estab") = estab
    CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("Dataobra") = dataObra.Date
    
    ' Se nome foi fornecido, atribuir
    If Not String.IsNullOrEmpty(nome) Then
        CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("Nome") = nome
    End If
    
    ' Se moeda foi fornecida, atribuir
    If Not String.IsNullOrEmpty(moeda) Then
        CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("Moeda") = moeda
    End If
    
    ' Se boano foi fornecido diferente do current year, atribuir
    If boano <> DateTime.Now.Year Then
        CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("Boano") = boano
    End If

    ' Auditar utilizador que criou o documento (via API)
    CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("ousrinis") = utilizador
    CreateBosDocWs.MainformDataset.Tables(0).Rows(0).Item("usrinis")  = utilizador

    ' Campos de utilizador dinâmicos do cabeçalho (bo/bo2/bo3)
    Dim addFieldsByTable As Newtonsoft.Json.Linq.JObject = TryCast(jsonObj("AddFieldsByTable"), Newtonsoft.Json.Linq.JObject)
    If addFieldsByTable IsNot Nothing Then
        Dim boFields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable("bo"), Newtonsoft.Json.Linq.JObject)
        Dim bo2Fields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable("bo2"), Newtonsoft.Json.Linq.JObject)
        Dim bo3Fields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable("bo3"), Newtonsoft.Json.Linq.JObject)

        ApplyUserFieldsToRow(CreateBosDocWs.MainformDataset.Tables(0).Rows(0), boFields, "bo")
        ApplyUserFieldsToRow(CreateBosDocWs.MainformDataset.Tables(1).Rows(0), bo2Fields, "bo2")
        ApplyUserFieldsToRow(CreateBosDocWs.MainformDataset.Tables(2).Rows(0), bo3Fields, "bo3")
    End If


    ' Iterar por cada linha do LstBi via JArray (evita falha na deserialização de objectos aninhados como AddFieldsByTable)
    Dim lstBiJson As Newtonsoft.Json.Linq.JArray = TryCast(jsonObj("LstBi"), Newtonsoft.Json.Linq.JArray)
    If lstBiJson Is Nothing Then
        Throw New Exception("LstBi não encontrado ou inválido no payload.")
    End If

    Dim ebi2Cursor As DataTable = CreateBosDocWs.MainFormDataSet.Tables("ebi2")
    Dim lineIndex As Integer = 0
    Dim linha As DataRow

    For Each lineToken As Newtonsoft.Json.Linq.JToken In lstBiJson
        Dim lineObj As Newtonsoft.Json.Linq.JObject = TryCast(lineToken, Newtonsoft.Json.Linq.JObject)
        If lineObj Is Nothing Then
            lineIndex += 1
            Continue For
        End If

        linha = CreateBosDocWs.Addnewline()
        linha.Item("ref") = lineObj("Ref").ToString()
        linha.Item("qtt") = Convert.ToDecimal(lineObj("Qtt").ToString(), System.Globalization.CultureInfo.InvariantCulture)

        CreateBosDocWs.actRef(linha)
        CreateBosDocWs.actLinha(linha)

        ' Campos opcionais da linha
        Dim designToken As Newtonsoft.Json.Linq.JToken = lineObj("Design")
        If designToken IsNot Nothing AndAlso designToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            Dim designVal As String = designToken.ToString()
            If Not String.IsNullOrEmpty(designVal) Then
                linha.Item("design") = designVal
            End If
        End If

        Dim puToken As Newtonsoft.Json.Linq.JToken = lineObj("PrecoUnitario")
        If puToken IsNot Nothing AndAlso puToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            linha.Item("edebito") = Convert.ToDecimal(puToken.ToString(), System.Globalization.CultureInfo.InvariantCulture)
            linha.Item("debito") = Convert.ToDecimal(puToken.ToString(), System.Globalization.CultureInfo.InvariantCulture)
        End If

        Dim tabIvaToken As Newtonsoft.Json.Linq.JToken = lineObj("TabIva")
        If tabIvaToken IsNot Nothing AndAlso tabIvaToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            Dim tabIvaCode As Integer = Convert.ToInt32(tabIvaToken.ToString())
            linha.Item("tabiva") = tabIvaCode

            Dim taxaParams As New List(Of System.Data.SqlClient.SqlParameter)
            taxaParams.Add(New System.Data.SqlClient.SqlParameter("@codigo", tabIvaCode))
            Dim taxaTable As DataTable = ExecuteQuery("SELECT taxa FROM taxasiva WHERE codigo = @codigo", taxaParams)
            If taxaTable.Rows.Count > 0 Then
                linha.Item("iva") = Convert.ToDecimal(taxaTable.Rows(0)("taxa"))
            End If
        End If

        Dim ivaInclToken As Newtonsoft.Json.Linq.JToken = lineObj("IvaIncl")
        If ivaInclToken IsNot Nothing AndAlso ivaInclToken.Type <> Newtonsoft.Json.Linq.JTokenType.Null Then
            linha.Item("ivaincl") = Convert.ToBoolean(ivaInclToken.ToString())
        End If

        ' Campos de utilizador dinâmicos da linha (bi/bi2)
        Dim lineAddFieldsByTable As Newtonsoft.Json.Linq.JObject = TryCast(lineObj("AddFieldsByTable"), Newtonsoft.Json.Linq.JObject)
        If lineAddFieldsByTable IsNot Nothing Then
            Dim biFields As Newtonsoft.Json.Linq.JObject = TryCast(lineAddFieldsByTable("bi"), Newtonsoft.Json.Linq.JObject)
            Dim bi2Fields As Newtonsoft.Json.Linq.JObject = TryCast(lineAddFieldsByTable("bi2"), Newtonsoft.Json.Linq.JObject)

            ApplyUserFieldsToRow(linha, biFields, "bi")

            If bi2Fields IsNot Nothing AndAlso ebi2Cursor IsNot Nothing AndAlso lineIndex < ebi2Cursor.Rows.Count Then
                ApplyUserFieldsToRow(ebi2Cursor.Rows(lineIndex), bi2Fields, "bi2")
            End If
        End If

        lineIndex += 1
    Next


    ' ==========================================

    Dim bocursor As DataTable = CreateBosDocWs.MainFormDataSet.Tables("ebo")
    Dim bicursor As DataTable = CreateBosDocWs.MainFormDataSet.Tables("ebi")
    Dim bi2cursor As DataTable = CreateBosDocWs.MainFormDataSet.Tables("ebi2")

    Dim ndoctipo As Integer = 1
    bizlib.Utility.boutil.get_tsvalores(ndoctipo, tdcursor)

    For index As Integer = 0 To bicursor.Rows.Count - 1
        With New ActPromoBO(tdcursor.Rows(0), bocursor.Rows(0), bicursor, bicursor.Rows(index), bi2cursor)
            .CalculaPromos()
        End With

        bizlib.boclass.boaddreg.u_bottdeb(bocursor.Rows(0)("moeda"), tdcursor.Rows(0), bicursor.Rows(index), bocursor.Rows(0))
    Next

    bizlib.boclass.boaddreg.acttotais("", "", True, bicursor, bocursor.Rows(0), tdcursor.Rows(0))

    ' ==========================================
    ' CÁLCULO DE IVA E TOTAIS
    ' ==========================================
    Dim nBoTtdeb As Decimal = 0
    Dim nIVA As Decimal = 0
    Dim nTOTALcIVA As Decimal = 0
    
    ' Obter referências às tabelas
    Dim boRow As DataRow = CreateBosDocWs.MainformDataset.Tables(0).Rows(0)
    Dim bo2Row As DataRow = CreateBosDocWs.MainformDataset.Tables(1).Rows(0)
    Dim bo3Row As DataRow = CreateBosDocWs.MainformDataset.Tables(2).Rows(0)

    Dim biTable As DataTable = CreateBosDocWs.MainformDataset.Tables(3)
    Dim bi2Table As DataTable = CreateBosDocWs.MainformDataset.Tables(4)

    ' Recalcular BO - somar IVA de todas as tabelas
    ' IVA de BO (EBO12_IVA até EBO62_IVA - 6 tabelas de IVA)
    nIVA += boRow("EBO12_IVA") + boRow("EBO22_IVA")
    nIVA += boRow("EBO32_IVA") + boRow("EBO42_IVA")
    nIVA += boRow("EBO52_IVA") + boRow("EBO62_IVA")
    
    ' IVA de BO2 (EBO72_IVA até EBO92_IVA - 3 tabelas de IVA)
    nIVA += bo2Row("EBO72_IVA") + bo2Row("EBO82_IVA") + bo2Row("EBO92_IVA")
    
    ' Total débito (de BO)
    nBoTtdeb = boRow("ETOTALDEB")
    
    ' Total com IVA
    nTOTALcIVA = nBoTtdeb + nIVA
    
    ' Atualizar campos em BO2
    bo2Row("TOTIVA") = nIVA
    bo2Row("ETOTIVA") = nIVA
    
    ' Atualizar campos em BO
    boRow("U_IVA") = nIVA
    boRow("U_TOTIVA") = nTOTALcIVA
    boRow("U_BOIVA") = If(boRow("U_BOCAM") = 0, nIVA, nIVA / boRow("U_BOCAM"))
    boRow("U_BOTOT") = If(boRow("U_BOCAM") = 0, nTOTALcIVA, nTOTALcIVA / boRow("U_BOCAM"))

    CreateBosDocWs.Save()


    ' ==========================================
    ' CONSTRUÇÃO DA RESPOSTA JSON (SUCESSO)
    ' ==========================================
    Dim responseJson As New Newtonsoft.Json.Linq.JObject
    
    ' Dados principais do documento
    responseJson("nmdos") = boRow("nmdos").ToString()
    responseJson("ndos") = boRow("ndos").ToString()
    responseJson("obrano") = boRow("obrano").ToString()
    responseJson("boano") = boRow("boano").ToString()
    
    ' Dados do CL/EM/AG/FL
    responseJson("no") = boRow("no").ToString()
    responseJson("nome") = boRow("Nome").ToString()
    responseJson("estab") = boRow("Estab").ToString()
    responseJson("data") = Convert.ToDateTime(boRow("Dataobra")).ToString("yyyy-MM-dd")
    responseJson("moeda") = boRow("Moeda").ToString()
    responseJson("total") = Newtonsoft.Json.Linq.JToken.FromObject(nTOTALcIVA)




    ' Campos de utilizador do cabeçalho (bo/bo2/bo3) lidos de volta do DataRow após Save()
    If addFieldsByTable IsNot Nothing Then
        Dim headerUserFields As New Newtonsoft.Json.Linq.JObject
        Dim headerTableMap As New Dictionary(Of String, DataRow) From {
            {"bo", boRow},
            {"bo2", bo2Row},
            {"bo3", bo3Row}
        }
        For Each tablePair As KeyValuePair(Of String, DataRow) In headerTableMap
            Dim tableKey As String = tablePair.Key
            Dim tableRow As DataRow = tablePair.Value
            Dim requestedFields As Newtonsoft.Json.Linq.JObject = TryCast(addFieldsByTable(tableKey), Newtonsoft.Json.Linq.JObject)
            If requestedFields IsNot Nothing Then
                Dim tableFieldsObj As New Newtonsoft.Json.Linq.JObject
                For Each prop As Newtonsoft.Json.Linq.JProperty In requestedFields.Properties()
                    If tableRow.Table.Columns.Contains(prop.Name) Then
                        Dim cellValue As Object = tableRow(prop.Name)
                        If cellValue Is DBNull.Value OrElse cellValue Is Nothing Then
                            tableFieldsObj(prop.Name) = Newtonsoft.Json.Linq.JValue.CreateNull()
                        Else
                            tableFieldsObj(prop.Name) = Newtonsoft.Json.Linq.JToken.FromObject(cellValue)
                        End If
                    End If
                Next
                If tableFieldsObj.Count > 0 Then
                    headerUserFields(tableKey) = tableFieldsObj
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

    For Each biRow As DataRow In biTable.Rows
        Dim lineObj As New Newtonsoft.Json.Linq.JObject
        
        ' Obter dados do produto
        lineObj("ref") = biRow("Ref").ToString()
        lineObj("design") = biRow("Design").ToString()
        
        lineObj("qtt") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(biRow("Qtt")))
        lineObj("tabiva") = biRow("tabiva").ToString()
        lineObj("iva") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDouble(biRow("Iva")))
        lineObj("ivaincl") = Newtonsoft.Json.Linq.JToken.FromObject(biRow("Ivaincl"))
        lineObj("debito") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(biRow("Debito")))
        lineObj("ttdeb") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(biRow("Ttdeb")))

        ' Campos de utilizador da linha (bi/bi2) lidos de volta do DataRow após Save()
        If lstBiJson IsNot Nothing AndAlso biRowIndex < lstBiJson.Count Then
            Dim srcLineObj As Newtonsoft.Json.Linq.JObject = TryCast(lstBiJson(biRowIndex), Newtonsoft.Json.Linq.JObject)
            Dim srcLineAddFields As Newtonsoft.Json.Linq.JObject = If(srcLineObj IsNot Nothing, TryCast(srcLineObj("AddFieldsByTable"), Newtonsoft.Json.Linq.JObject), Nothing)
            If srcLineAddFields IsNot Nothing Then
                Dim lineUserFields As New Newtonsoft.Json.Linq.JObject
                Dim lineTableMap As New Dictionary(Of String, DataRow)
                lineTableMap("bi") = biRow
                If ebi2Cursor IsNot Nothing AndAlso biRowIndex < ebi2Cursor.Rows.Count Then
                    lineTableMap("bi2") = ebi2Cursor.Rows(biRowIndex)
                End If
                For Each tablePair As KeyValuePair(Of String, DataRow) In lineTableMap
                    Dim tableKey As String = tablePair.Key
                    Dim tableRow As DataRow = tablePair.Value
                    Dim requestedFields As Newtonsoft.Json.Linq.JObject = TryCast(srcLineAddFields(tableKey), Newtonsoft.Json.Linq.JObject)
                    If requestedFields IsNot Nothing Then
                        Dim tableFieldsObj As New Newtonsoft.Json.Linq.JObject
                        For Each prop As Newtonsoft.Json.Linq.JProperty In requestedFields.Properties()
                            If tableRow.Table.Columns.Contains(prop.Name) Then
                                Dim cellValue As Object = tableRow(prop.Name)
                                If cellValue Is DBNull.Value OrElse cellValue Is Nothing Then
                                    tableFieldsObj(prop.Name) = Newtonsoft.Json.Linq.JValue.CreateNull()
                                Else
                                    tableFieldsObj(prop.Name) = Newtonsoft.Json.Linq.JToken.FromObject(cellValue)
                                End If
                            End If
                        Next
                        If tableFieldsObj.Count > 0 Then
                            lineUserFields(tableKey) = tableFieldsObj
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
    ' ==========================================
    ' RESPOSTA COM ERRO
    ' ==========================================
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject
    errorResponse("success") = False
    errorResponse("code") = "BO014"
    errorResponse("message") = e.Message
    
    response = errorResponse
End Try

Return response