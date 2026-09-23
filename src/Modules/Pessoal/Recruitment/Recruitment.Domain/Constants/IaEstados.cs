namespace Recruitment.Domain.Constants;

/// <summary>PHC rct.idrct is a string id.</summary>
public static class RctIds
{
    public const int MaxLength = 50;
}

/// <summary>
/// Values for CVE.u_estadoia (BR-12 / swimlane Analisar).
/// </summary>
public static class IaEstados
{
    public const string Pendente = "pendente";
    public const string Ok = "ok";
    public const string Erro = "erro";
}

/// <summary>
/// Outbox row lifecycle (BR-01 / BR-12).
/// </summary>
public static class OutboxEstados
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Done = "done";
    public const string Error = "error";
}

/// <summary>
/// User-facing HITL copy (AH-04 / AH-06). Never attribute selection decisions to IA.
/// </summary>
public static class HitlCopy
{
    public const string RankingTitle =
        "Ordenacao por score IA (input ao RH -- nao e decisao)";

    public const string Footer =
        "Score IA = input. Decisao de avancar/rejeitar e so humana no PHC.";

    public const string SemEvidencia = "sem evidencia no CV";

    /// <summary>
    /// HITL ranking is the human pre-selection list. IA does not decide.
    /// </summary>
    public const string PreSelectionNote =
        "Lista ordenada para pre-seleccao humana. Nao e decisao automatica da IA.";

    /// <summary>Forbidden substrings (case/accent-insensitive; pela IA | by AI) — AH-04 kill case.</summary>
    public static readonly string[] ForbiddenPhrases =
    [
        "seleccionado pela ia",
        "selecionado pela ia",
        "rejeitado pela ia",
        "avancado pela ia",
        "avancada pela ia",
        "seleccionado by ai",
        "selecionado by ai",
        "rejeitado by ai",
        "avancado by ai",
        "avancada by ai"
    ];
}
