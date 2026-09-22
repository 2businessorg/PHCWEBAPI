namespace Shared.Kernel.Utilities;

/// <summary>
/// Fornece valores padrão para campos de auditoria nas entidades
/// </summary>
public static class AuditFieldProvider
{
    /// <summary>
    /// Usuário padrão para inserções na API
    /// </summary>
    public const string DefaultUser = "PHCAPI";

    /// <summary>
    /// Obter a hora atual formatada como HH:mm:ss
    /// </summary>
    /// <returns>Hora no formato HH:mm:ss</returns>
    public static string GetCurrentTime()
    {
        return DateTime.UtcNow.ToString("HH:mm:ss");
    }

    /// <summary>
    /// Obter a data de hoje às 00:00:00
    /// </summary>
    /// <returns>Data de hoje com hora zerada</returns>
    public static DateTime GetTodayAtMidnight()
    {
        return DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Preencher os campos de auditoria de uma entidade
    /// </summary>
    /// <typeparam name="T">Tipo da entidade que possui campos de auditoria</typeparam>
    /// <param name="entity">Entidade a preencher</param>
    /// <param name="user">Usuário (padrão: DefaultUser)</param>
    public static void SetAuditFields<T>(T entity, string? user = null) where T : class
    {
        user ??= DefaultUser;
        var currentTime = GetCurrentTime();
        var todayAtMidnight = GetTodayAtMidnight();

        var type = entity.GetType();

        // Preencher campos de criação original (O)
        var ousrinisProp = type.GetProperty("Ousrinis");
        var ousrdataProp = type.GetProperty("Ousrdata");
        var ousrhoraProp = type.GetProperty("Ousrhora");

        // Preencher campos de última atualização
        var usrinisProp = type.GetProperty("Usrinis");
        var usrdataProp = type.GetProperty("Usrdata");
        var usrhoraProp = type.GetProperty("Usrhora");

        // Campos de criação original
        ousrinisProp?.SetValue(entity, user);
        ousrdataProp?.SetValue(entity, todayAtMidnight);
        ousrhoraProp?.SetValue(entity, currentTime);

        // Campos de última atualização
        usrinisProp?.SetValue(entity, user);
        usrdataProp?.SetValue(entity, todayAtMidnight);
        usrhoraProp?.SetValue(entity, currentTime);
    }

    /// <summary>
    /// Preencher apenas os campos de auditoria de atualização
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    /// <param name="entity">Entidade a preencher</param>
    /// <param name="user">Usuário (padrão: DefaultUser)</param>
    public static void SetUpdateAuditFields<T>(T entity, string? user = null) where T : class
    {
        user ??= DefaultUser;
        var currentTime = GetCurrentTime();
        var todayAtMidnight = GetTodayAtMidnight();

        var type = entity.GetType();

        // Preencher apenas campos de última atualização
        var usrinisProp = type.GetProperty("Usrinis");
        var usrdataProp = type.GetProperty("Usrdata");
        var usrhoraProp = type.GetProperty("Usrhora");

        usrinisProp?.SetValue(entity, user);
        usrdataProp?.SetValue(entity, todayAtMidnight);
        usrhoraProp?.SetValue(entity, currentTime);
    }
}
