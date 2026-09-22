namespace Shared.Kernel.UserFields;

/// <summary>
/// Represents the definition of a user-defined field for a specific module/table.
/// Read from u_addfields table in the primary (config) database.
/// </summary>
/// <param name="Alias">Friendly name exposed in the API response (e.g. "reference")</param>
/// <param name="ColumnName">Actual column name in the PHC database (e.g. "u_ref")</param>
/// <param name="FieldType">Data type hint: "string", "int", "decimal", "date", "boolean"</param>
/// <param name="IsReadOnly">If true, the field is returned but cannot be written via API</param>
public sealed record UserFieldDefinition(
    string Alias,
    string ColumnName,
    string FieldType,
    bool IsReadOnly);
