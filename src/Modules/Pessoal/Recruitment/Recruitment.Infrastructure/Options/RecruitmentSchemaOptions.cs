namespace Recruitment.Infrastructure.Options;

/// <summary>
/// Column / table mapping for PHC recruitment tables.
/// Names follow PHC U_* conventions; adjust per customer DB.
/// Documented in docs/modulos/Recrutamento-IA.md.
/// </summary>
public sealed class RecruitmentSchemaOptions
{
    public const string SectionName = "RecruitmentIa:Schema";

    public string ConnectionStringName { get; set; } = "DBconnect";

    public string CveTable { get; set; } = "cve";
    public string CveStampColumn { get; set; } = "cvestamp";
    public string CveNameColumn { get; set; } = "nome";
    /// <summary>Chosen: cve.u_estadoia (pendente|ok|erro).</summary>
    public string CveEstadoIaColumn { get; set; } = "u_estadoia";

    public string RctTable { get; set; } = "rct";
    public string RctStampColumn { get; set; } = "rctstamp";
    /// <summary>Public vacancy id (PHC rct.idrct). Route key; resolved to rctstamp.</summary>
    public string RctIdColumn { get; set; } = "idrct";

    /// <summary>
    /// Bridge table for RCT characteristics + weights.
    /// Lab seed OK; production must mirror RCT native weights (never hardcode forever).
    /// </summary>
    public string RctCriteriaTable { get; set; } = "u_rec_ia_crt";
    public string RctclbTable { get; set; } = "rctclb";
    public string RctclbUserColumn { get; set; } = "userno";
    public string RctclbNameColumn { get; set; } = "nome";

    public string SrtTable { get; set; } = "srt";
    public string SrtStampColumn { get; set; } = "srtstamp";
    public string SrtCveStampColumn { get; set; } = "cvestamp";
    public string SrtRctStampColumn { get; set; } = "rctstamp";
    public string SrtCondpColumn { get; set; } = "condp";
    /// <summary>Chosen: srt.u_scoreia to avoid native score collision (BR-02).</summary>
    public string SrtScoreIaColumn { get; set; } = "u_scoreia";
    public string SrtJustIaColumn { get; set; } = "u_justia";
    public string SrtModeloIaColumn { get; set; } = "u_modeloia";
    /// <summary>SCAMPOS: suffix after u_ is 8 chars max. u_prmveria (not u_promptveria).</summary>
    public string SrtPromptVerIaColumn { get; set; } = "u_prmveria";
    public string SrtStampIaColumn { get; set; } = "u_stampia";
    /// <summary>SCAMPOS: u_auditia (not u_auditoriaia).</summary>
    public string SrtAuditIaColumn { get; set; } = "u_auditia";
    public string SrtSelectionStateColumns { get; set; } = "estado,apurado,entrevista";

    public string AnexosTable { get; set; } = "anexos";
    public string AnexosStampColumn { get; set; } = "anexosstamp";
    public string AnexosOriTableColumn { get; set; } = "oritable";
    public string AnexosRecStampColumn { get; set; } = "recstamp";
    public string AnexosBytesColumn { get; set; } = "bdados";
    public string AnexosTipoColumn { get; set; } = "tipo";
    public string AnexosFileNameColumn { get; set; } = "nome";
    /// <summary>Chosen: anexos.u_texto for OCR plain text.</summary>
    public string AnexosTextoColumn { get; set; } = "u_texto";
    public string AnexosOriTableCveValue { get; set; } = "cve";
    public int AnexosCvTipoValue { get; set; } = 1;

    public string OutboxTable { get; set; } = "u_rec_ia_outbox";
}
