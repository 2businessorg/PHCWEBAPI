namespace Auth.Domain.Entities;

/// <summary>
/// Represents a user-defined field configuration for a specific tenant/module/table.
/// Maps to dbo.u_addfields in the primary (config) database.
/// </summary>
public class UserField
{
    /// <summary>
    /// Unique identifier (timestamp-based). Maps to u_addfieldsstamp.
    /// </summary>
    public string Stamp { get; set; } = string.Empty;

    /// <summary>
    /// FK to u_applicense - identifies which tenant this field belongs to.
    /// Maps to u_applicensestamp.
    /// </summary>
    public string AppLicenseStamp { get; set; } = string.Empty;

    /// <summary>
    /// Module name (e.g. "Clients", "Dossiers"). Maps to modulename.
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// PHC database table name (e.g. "cl", "bi"). Maps to tablename.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Friendly alias exposed in the API (e.g. "reference"). Maps to fieldalias.
    /// </summary>
    public string FieldAlias { get; set; } = string.Empty;

    /// <summary>
    /// Actual column name in the PHC database (e.g. "u_ref"). Maps to fieldcolumn.
    /// </summary>
    public string FieldColumn { get; set; } = string.Empty;

    /// <summary>
    /// Data type hint: "string", "int", "decimal", "date", "boolean". Maps to fieldtype.
    /// </summary>
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// If true, the field is returned but cannot be written via the API. Maps to readonly.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Soft-delete flag. Maps to inactive.
    /// </summary>
    public bool Inactive { get; set; }
}
