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
    ' Campos obrigatórios
    Dim docType   As Integer   = Convert.ToInt32(jsonObj("ndoc"))
    Dim clNo      As Integer   = Convert.ToInt32(jsonObj("no"))
    
    ' Campos opcionais (verificar JTokenType.Null para valores null do JSON)
    Dim ftano     As Integer   = If(jsonObj("ftano").Type <> Newtonsoft.Json.Linq.JTokenType.Null, Convert.ToInt32(jsonObj("ftano")), DateTime.Now.Year)
    Dim estab     As Integer   = If(jsonObj("estab").Type <> Newtonsoft.Json.Linq.JTokenType.Null, Convert.ToInt32(jsonObj("estab")), 0)
    Dim nome      As String    = If(jsonObj("nome").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("nome").ToString(), "")
    Dim fdata     As DateTime  = If(jsonObj("data").Type <> Newtonsoft.Json.Linq.JTokenType.Null, DateTime.Parse(jsonObj("data").ToString()), Date.Today)
    Dim moeda     As String    = If(jsonObj("moeda").Type <> Newtonsoft.Json.Linq.JTokenType.Null, jsonObj("moeda").ToString(), "")
    
    ' Criação do documento de fatura
    Dim CreateFtDocWs As bizlib.ftclass.CreateFTDoc
    CreateFtDocWs = New bizlib.ftclass.CreateFTDoc(docType, clNo)

    CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("ftano") = ftano
    CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("fdata") = fdata

    ' Atribuir campos opcionais se fornecidos
    If estab > 0 Then
        CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("estab") = estab
    End If
    
    If Not String.IsNullOrEmpty(nome) Then
        CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("nome") = nome
    End If

    If Not String.IsNullOrEmpty(moeda) Then
        CreateFtDocWs.MainformDataset.Tables(0).Rows(0).Item("moeda") = moeda
    End If

    ' ==========================================
    ' ADICIONAR LINHAS DO DOCUMENTO
    ' ==========================================
    Dim dtLstRefs As DataTable = Newtonsoft.Json.JsonConvert.DeserializeObject(Of DataTable)(jsonObj("lstFi").ToString())
    Dim newrowWithRef As DataRow
    
    For Each dr As DataRow In dtLstRefs.Rows
        Dim ref As String = dr("ref").ToString()
        
        ' Adicionar nova linha pela referência
        'newrowWithRef = CreateFtDocWs.addLineref(ref)
        newrowWithRef = CreateFtDocWs.addNewLine()

        ' Atribuir quantidade
        newrowWithRef.Item("ref") = ref
        newrowWithRef.Item("qtt") = Convert.ToDecimal(dr("qtt"))
        newrowWithRef.Item("armazem") = 1
        
        CreateFtDocWs.actLinha(newrowWithRef)

        ' Campos opcionais da linha
        If Not IsDBNull(dr("design")) AndAlso Not String.IsNullOrEmpty(dr("design").ToString()) Then
            newrowWithRef.Item("design") = dr("design").ToString()
        End If
        
        If Not IsDBNull(dr("precoVenda")) Then
            newrowWithRef.Item("epv") = Convert.ToDecimal(dr("precoVenda"))
        End If
        
        If Not IsDBNull(dr("tabIva")) Then
            newrowWithRef.Item("tabiva") = Convert.ToInt32(dr("tabIva"))
        End If
        
        If Not IsDBNull(dr("ivaIncl")) Then
            newrowWithRef.Item("ivaincl") = Convert.ToBoolean(dr("ivaIncl"))
        End If

    Next

    ' GRAVAR DOCUMENTO
    CreateFtDocWs.Save()

    ' ==========================================
    ' CONSTRUÇÃO DA RESPOSTA JSON (SUCESSO)
    ' ==========================================
    Dim ftRow As DataRow = CreateFtDocWs.MainformDataset.Tables(0).Rows(0)
    Dim fiTable As DataTable = CreateFtDocWs.MainformDataset.Tables(2)
    
    Dim responseJson As New Newtonsoft.Json.Linq.JObject
    
    ' Dados principais do documento
    responseJson("nmdoc") = ftRow("nmdoc").ToString()
    responseJson("ndoc") = ftRow("ndoc").ToString()
    responseJson("fno") = ftRow("fno").ToString()
    responseJson("ftano") = ftRow("ftano").ToString()
    
    ' Dados do cliente
    responseJson("no") = ftRow("no").ToString()
    responseJson("nome") = ftRow("Nome").ToString()
    responseJson("estab") = ftRow("Estab").ToString()


    responseJson("data") = Convert.ToDateTime(ftRow("fdata")).ToString("yyyy-MM-dd")
    responseJson("total") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("total")))
    responseJson("totalMoeda") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("totalMoeda")))
    responseJson("ttiva") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("ttiva")))
    responseJson("tmIva") = Newtonsoft.Json.Linq.JToken.FromObject(Convert.ToDecimal(ftRow("tmIva")))
    
    ' Array de linhas
    Dim linesArray As New Newtonsoft.Json.Linq.JArray
    
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
        
        linesArray.Add(lineObj)
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
    errorResponse("code") = "FT001"
    errorResponse("message") = e.Message
    
    response = errorResponse
End Try

Return response