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
    Dim olCodigo As String = If(jsonObj("treasuryCode") IsNot Nothing, jsonObj("treasuryCode").ToString(), String.Empty)

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
    Dim rowWrapper As IRowWrapper = New WebControlLib.DataRowWrapper(tsreRow)
    Dim tsre As TsreRow = New TsreRow(rowWrapper)

    ' Pequeno mapeamento adicional para os campos usados pelo EmitirRecibo.
    tsre.Cmcc = rowWrapper("Cmcc")
    tsre.Oldoc = rowWrapper("Oldoc")
    tsre.Xddesc = rowWrapper("Xddesc")
    tsre.Xdstamp = rowWrapper("Xdstamp")
    tsre.Ndino = rowWrapper("Ndino")
    tsre.NdiDesc = rowWrapper("NdiDesc")
    tsre.Ndcno = rowWrapper("Ndcno")
    tsre.NdcDesc = rowWrapper("NdcDesc")
    tsre.AutoMl = rowWrapper("AutoMl")
    tsre.IntroRD = rowWrapper("IntroRD")
    tsre.SerieRD = rowWrapper("SerieRD")
    tsre.Nserierd = rowWrapper("Nserierd")
    tsre.Introfac = rowWrapper("Introfac")
    tsre.OUsrInis = rowWrapper("OUsrInis")
    tsre.OUsrData = rowWrapper("OUsrData")
    tsre.OUsrHora = rowWrapper("OUsrHora")
    tsre.UsrInis = rowWrapper("UsrInis")
    tsre.UsrData = rowWrapper("UsrData")
    tsre.UsrHora = rowWrapper("UsrHora")
    tsre.Intropag = rowWrapper("Intropag")
    tsre.MovInc = rowWrapper("MovInc")
    tsre.SerieOX = rowWrapper("SerieOX")
    tsre.IvaCaixa = rowWrapper("IvaCaixa")
    tsre.ManterNumero = rowWrapper("ManterNumero")
    tsre.ClIvaCaixa = rowWrapper("ClIvaCaixa")
    tsre.DispGestWeb = rowWrapper("DispGestWeb")
    tsre.Fechada = rowWrapper("Fechada")

    ' Resolver ftstamp (igual ao ccstamp) para cada factura indicada no JSON.
    Dim listaCC As New List(Of CcRow)()

    For Each line As Newtonsoft.Json.Linq.JToken In lines
        Dim invoiceNumber As Integer = Convert.ToInt32(line("invoiceNumber"))
        Dim invoiceTypeId As Integer = Convert.ToInt32(line("invoiceTypeId"))
        Dim invoiceYear As Integer = Convert.ToInt32(line("invoiceYear"))

        Dim ftTable As DataTable = ExecuteQuery(
            "SELECT TOP 1 ftstamp FROM ft WHERE ndoc = @invoiceTypeId AND fno = @invoiceNumber AND ftano = @invoiceYear and no = @clientId",
            New List(Of System.Data.SqlClient.SqlParameter) From {
                New System.Data.SqlClient.SqlParameter("@invoiceTypeId", invoiceTypeId),
                New System.Data.SqlClient.SqlParameter("@invoiceNumber", invoiceNumber),
                New System.Data.SqlClient.SqlParameter("@invoiceYear", invoiceYear),
                New System.Data.SqlClient.SqlParameter("@clientId", clientId)
            })

        If ftTable.Rows.Count = 0 Then
            Throw New Exception($"Não foi encontrado ftstamp para a factura {invoiceNumber}/{invoiceYear} (tipo {invoiceTypeId}).")
        End If

        Dim ftstamp As String = ftTable.Rows(0)("ftstamp").ToString()
        listaCC.Add(CcRow.GetRow(ftstamp))
    Next

    Dim dataProcessamento As DateTime = DateTime.Today
    Dim processa As Boolean = false

    ' Parâmetros do método EmitirRecibo:
    ' 1º tsre -> configuração da série do recibo
    ' 2º dataProcessamento -> data do lançamento
    ' 3º listaCC -> documentos/faturas a regularizar (ccstamp/ftstamp)
    ' 4º bankAccountId -> número interno da conta bancária
    ' 5º processa -> True para processar o recibo (False neste momento)
    ' 6º olCodigo -> código de tesouraria (opcional no JSON mínimo)
    Dim result As ReFormClass = ReFormClass.EmitirRecibo(tsre, dataProcessamento, listaCC, bankAccountId, processa, "")

    ' Atualizar rl.erec (valor recebido) com o amount indicado para cada factura
    Dim rlTable As DataTable = result.Rls
    Dim lineIndex As Integer = 0
    
    For Each line As Newtonsoft.Json.Linq.JToken In lines
        Dim invoiceNumber As Integer = Convert.ToInt32(line("invoiceNumber"))
        Dim invoiceTypeId As Integer = Convert.ToInt32(line("invoiceTypeId"))
        Dim amount As Decimal = Convert.ToDecimal(line("amount"))
        
        ' Procurar a linha RL que corresponde a esta factura (nrdoc/ndoc)
        Dim foundRow As DataRow = Nothing
        For Each rlRow As DataRow In rlTable.Rows
            If Convert.ToInt32(rlRow("ndoc")) = invoiceTypeId AndAlso Convert.ToInt32(rlRow("nrdoc")) = invoiceNumber Then
                foundRow = rlRow
                Exit For
            End If
        Next
        
        If foundRow IsNot Nothing Then
            ' Atualizar o valor recebido (erec) com o amount do JSON
            foundRow("erec") = amount
        End If
        
        lineIndex += 1
    Next
    
    ' Chamar Processar que retorna boolean e realiza o processamento final
    result.ReRlAct()
    result.REAct_Moeda()
    Dim processResult As Boolean = result.Processar(dataProcessamento)
    
    response = New Newtonsoft.Json.Linq.JObject
    response("success") = processResult
    response("reTotal") = result.Re.Rows(0)("total").ToString()
    response("processado") = processResult

Catch e As Exception
    Dim errorResponse As New Newtonsoft.Json.Linq.JObject
    errorResponse("success") = False
    errorResponse("code") = "RE001"
    errorResponse("message") = e.Message
    response = errorResponse
End Try

Return response

