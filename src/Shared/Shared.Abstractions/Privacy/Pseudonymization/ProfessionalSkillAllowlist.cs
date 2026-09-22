namespace Shared.Abstractions.Privacy.Pseudonymization;

/// <summary>
/// Professional skills / technologies that MUST remain visible (do not tokenize).
/// Architecture section 5.3.
/// </summary>
public static class ProfessionalSkillAllowlist
{
    public static readonly IReadOnlySet<string> Default = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Java", ".NET", "C#", "CSharp", "React", "Angular", "Vue", "SAP", "SQL",
        "PostgreSQL", "Oracle", "Azure", "AWS", "GCP", "Kubernetes", "Docker",
        "Power BI", "PowerBI", "PHC", "TypeScript", "JavaScript", "Python",
        "Node.js", "NodeJS", "Spring", "Hibernate", "Entity Framework", "EF Core",
        "REST", "GraphQL", "Kafka", "RabbitMQ", "Redis", "MongoDB", "MySQL",
        "Linux", "Windows", "Git", "CI/CD", "Terraform", "Ansible", "Scrum", "Agile"
    };

    public static bool IsSkill(string text, IReadOnlySet<string>? extra = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmed = text.Trim();
        if (Default.Contains(trimmed))
            return true;

        return extra is not null && extra.Contains(trimmed);
    }
}
