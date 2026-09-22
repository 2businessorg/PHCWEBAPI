namespace Admin.UI.Settings;

/// <summary>
/// Configurações de aparência do painel de administração
/// </summary>
public class AdminUISettings
{
    /// <summary>
    /// URL do logo da aplicação (pode ser URL externa ou caminho relativo)
    /// </summary>
    public string LogoUrl { get; set; } = "https://www.2business-si.com/wp-content/uploads/2024/12/2b_logo_ca_03.svg";

    /// <summary>
    /// Texto alternativo para o logo
    /// </summary>
    public string LogoAlt { get; set; } = "Logo";

    /// <summary>
    /// Largura do logo em pixels
    /// </summary>
    public int LogoWidth { get; set; } = 180;

    /// <summary>
    /// Altura do logo em pixels
    /// </summary>
    public int LogoHeight { get; set; } = 50;

    /// <summary>
    /// Configurações de cores do tema
    /// </summary>
    public ThemeColors Colors { get; set; } = new();
}

/// <summary>
/// Cores do tema da aplicação
/// </summary>
public class ThemeColors
{
    /// <summary>
    /// Cor primária (botões, links, highlights)
    /// Padrão: Google Blue #1a73e8
    /// </summary>
    public string Primary { get; set; } = "#1a73e8";

    /// <summary>
    /// Cor primária no hover
    /// </summary>
    public string PrimaryHover { get; set; } = "#1557b0";

    /// <summary>
    /// Cor primária clara (backgrounds)
    /// </summary>
    public string PrimaryLight { get; set; } = "#e8f0fe";

    /// <summary>
    /// Cor de sucesso (mensagens de confirmação)
    /// </summary>
    public string Success { get; set; } = "#1e8e3e";

    /// <summary>
    /// Cor de erro (mensagens de erro)
    /// </summary>
    public string Error { get; set; } = "#d93025";

    /// <summary>
    /// Cor de aviso (mensagens de atenção)
    /// </summary>
    public string Warning { get; set; } = "#f9ab00";

    /// <summary>
    /// Cor de informação (mensagens informativas)
    /// </summary>
    public string Info { get; set; } = "#1a73e8";

    /// <summary>
    /// Cor do texto principal
    /// </summary>
    public string TextPrimary { get; set; } = "#202124";

    /// <summary>
    /// Cor do texto secundário
    /// </summary>
    public string TextSecondary { get; set; } = "#5f6368";

    /// <summary>
    /// Cor de fundo da sidebar
    /// </summary>
    public string SidebarBackground { get; set; } = "#ffffff";

    /// <summary>
    /// Cor do texto da sidebar
    /// </summary>
    public string SidebarText { get; set; } = "#5f6368";

    /// <summary>
    /// Cor do item ativo na sidebar
    /// </summary>
    public string SidebarActive { get; set; } = "#e8f0fe";

    /// <summary>
    /// Cor do texto do item ativo na sidebar
    /// </summary>
    public string SidebarActiveText { get; set; } = "#1a73e8";
}
