namespace Dossiers.Application.Mappings;

/// <summary>
/// Mapeador de tipos de entidade para Inglês
/// </summary>
public static class DossierTypeNameMapper
{
    private static readonly Dictionary<string, string> EntityTypeTranslations = new()
    {
        { "CL", "Client" },
        { "FL", "Supplier" },
        { "AG", "Entity" },
        { "EM", "Contact" }
    };

    /// <summary>
    /// Obtém o nome em Inglês para um tipo de entidade
    /// </summary>
    public static string GetEnglishEntityTypeName(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return string.Empty;

        var trimmedType = entityType.Trim();
        
        return EntityTypeTranslations.TryGetValue(trimmedType, out var englishName) 
            ? englishName 
            : trimmedType; // Retorna o tipo original se não houver tradução
    }
}
