namespace Shared.Kernel.UserFields;

/// <summary>
/// Utilitário para formatar valores de addFields antes de os devolver na resposta da API.
/// Garante que campos do tipo "date" são serializados como "yyyy-MM-dd" em vez de ISO 8601 completo.
/// </summary>
public static class UserFieldValueFormatter
{
    /// <summary>
    /// Formata <paramref name="value"/> de acordo com o <paramref name="fieldType"/> definido em u_addfields.
    /// Actualmente apenas o tipo "date" aplica formatação especial; todos os outros são devolvidos sem alteração.
    /// </summary>
    public static object? FormatForOutput(object? value, string fieldType)
    {
        if (value is null)
            return null;

        if (string.Equals(fieldType, "date", StringComparison.OrdinalIgnoreCase))
        {
            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd");

            if (value is DateOnly d)
                return d.ToString("yyyy-MM-dd");

            if (value is string s && DateTime.TryParse(s, out var parsed))
                return parsed.ToString("yyyy-MM-dd");
        }

        return value;
    }
}
