using Treasury.Application.DTOs;
using Treasury.Application.Mappings;
using Treasury.Application.Matching;

namespace Agent.Presentation.MCP;

/// <summary>
/// Compact MCP payloads so small models receive totals instead of hundreds of rows.
/// </summary>
public static class ReconciliationMcpCompact
{
    public const int SampleSize = 5;

    public const string Instruction =
        "Usa os totais e contagens deste JSON. Nao somes linhas. Nao inventes valores. " +
        "Nao trates prefixos de stamp (AON, ANM, SDO, KFU) como nomes de empresas. " +
        "Usa o campo account.name exacto como accountName nas outras tools.";

    public static object FromMovements(ReconciliationMovementsOutputDTO result, int sampleSize = SampleSize)
    {
        var summary = ReconciliationSummaryMapper.From(result);
        return new
        {
            instruction = Instruction,
            account = CompactAccount(result.Account),
            period = result.Period,
            summary = CompactSummary(summary),
            sampleBankMovements = result.BankMovements
                .Take(sampleSize)
                .Select(m => new
                {
                    date = m.Date,
                    description = m.Description,
                    amount = m.Amount
                }),
            sampleTreasuryMovements = result.TreasuryMovements
                .Take(sampleSize)
                .Select(m => new
                {
                    date = m.Date,
                    description = m.Description,
                    inflow = m.Inflow,
                    outflow = m.Outflow
                }),
            truncated = !summary.TotalsAreComplete
                || result.BankMovementCount > sampleSize
                || result.TreasuryMovementCount > sampleSize
        };
    }

    public static object FromSummary(ReconciliationSummaryOutputDTO result)
        => new
        {
            instruction = result.Instruction,
            account = CompactAccount(result.Account),
            period = result.Period,
            summary = CompactSummary(result)
        };

    public static object FromAccounts(TreasuryAccountListOutputDTO result)
        => new
        {
            instruction =
                "Usa o campo name exacto como accountName nas outras tools. " +
                "Nao inventes contas. Se o utilizador pedir uma conta que nao esta na lista, diz que nao existe.",
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            accounts = result.Items.Select(CompactAccount)
        };

    public static object FromAccount(TreasuryAccountOutputDTO account)
        => new
        {
            instruction = "Usa o campo name exacto como accountName nas outras tools.",
            account = CompactAccount(account)
        };

    public const string MatchInstruction =
        "TU fazes o match. Usa party, cheque, document, invoice, date e valueDate. Nao pares so por amount. " +
        "Nunca pares +X com -X. Nao inventes reforco de banco. " +
        "MATCHED: mesmo sinal + (party a sobrepor OU cheque OU invoice/document) + min(|date|,|valueDate|) <= 30 dias. " +
        "PARTIAL MATCHED: mesmo sinal + party/cheque/invoice, mas datas > 30 dias. Nunca MATCHED nesse caso. " +
        "UNMATCHED: so amount, nomes diferentes, ou sinais opostos. " +
        "Nao deixes nos leftovers nenhum par 1:1 com amount+party e datas <=30d. " +
        "1:N/N:1 so se a soma dos amount bater E party/datas apoiarem. Cada ref numa so combination. " +
        "Depois: 1 UNMATCHED com bankRefs que sobram e 1 UNMATCHED com treasuryRefs que sobram. " +
        "Resposta = SO JSON {\"combinations\":[{\"status\",\"reason\",\"bankRefs\",\"treasuryRefs\"}]}.";

    public static object FromMatchCandidates(ReconciliationMovementsOutputDTO result)
        => new
        {
            instruction = MatchInstruction,
            account = CompactAccount(result.Account),
            period = result.Period,
            bankMovementCount = result.BankMovementTotalCount,
            treasuryMovementCount = result.TreasuryMovementTotalCount,
            bankMovements = result.BankMovements.Select((m, i) => CompactMatchLine(
                $"B{i + 1}",
                m.Date,
                m.ValueDate,
                m.Amount,
                m.Description,
                m.Document,
                m.Cheque)),
            treasuryMovements = result.TreasuryMovements.Select((m, i) => CompactMatchLine(
                $"T{i + 1}",
                m.Date,
                m.ValueDate,
                m.Inflow - m.Outflow,
                m.Description,
                m.Document,
                m.Cheque))
        };

    private static object CompactMatchLine(
        string refId,
        DateOnly date,
        DateOnly valueDate,
        decimal amount,
        string description,
        string document,
        string cheque)
        => new
        {
            refId,
            date,
            valueDate,
            amount,
            party = ReconciliationTextHints.Party(description, document, cheque),
            invoice = ReconciliationTextHints.Invoice(description, document),
            document = string.IsNullOrWhiteSpace(document) ? null : document.Trim(),
            cheque = string.IsNullOrWhiteSpace(cheque) ? null : cheque.Trim(),
            description
        };

    private static object CompactAccount(TreasuryAccountOutputDTO account)
        => new
        {
            name = account.Name,
            code = account.Code,
            accountNumber = account.AccountNumber,
            inactive = account.Inactive,
            currency = account.Currency,
            balance = account.Balance
        };

    private static object CompactSummary(ReconciliationSummaryOutputDTO summary)
        => new
        {
            bankMovementCount = summary.BankMovementCount,
            treasuryMovementCount = summary.TreasuryMovementCount,
            bankAmountTotal = summary.BankAmountTotal,
            bankCreditTotal = summary.BankCreditTotal,
            bankDebitTotal = summary.BankDebitTotal,
            treasuryInflowTotal = summary.TreasuryInflowTotal,
            treasuryOutflowTotal = summary.TreasuryOutflowTotal,
            treasuryNetTotal = summary.TreasuryNetTotal,
            totalsAreComplete = summary.TotalsAreComplete
        };
}
