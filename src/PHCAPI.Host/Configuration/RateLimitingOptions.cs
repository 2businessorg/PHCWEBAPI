namespace PHCAPI.Host.Configuration;

/// <summary>
/// Rate Limiting configuration options
/// 🛡️ SECURITY: Configurable limits per endpoint to prevent brute force, DoS, and API abuse
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Login endpoint rate limiting configuration
    /// 🔴 CRITICAL: Anti-brute force protection
    /// </summary>
    public EndpointLimitOptions LoginEndpoint { get; set; } = new();

    /// <summary>
    /// Parameter creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection
    /// </summary>
    public EndpointLimitOptions ParametersCreate { get; set; } = new();

    /// <summary>
    /// Parameter deletion endpoint rate limiting
    /// 🔴 CRITICAL: Prevent accidental/malicious mass deletion
    /// </summary>
    public EndpointLimitOptions ParametersDelete { get; set; } = new();

    /// <summary>
    /// Parameter update endpoint rate limiting
    /// 🟡 MEDIUM: Control frequent modifications
    /// </summary>
    public EndpointLimitOptions ParametersUpdate { get; set; } = new();

    /// <summary>
    /// Parameter query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions ParametersQuery { get; set; } = new();

    /// <summary>
    /// Client creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection
    /// </summary>
    public EndpointLimitOptions ClientsCreate { get; set; } = new();

    /// <summary>
    /// Client deletion endpoint rate limiting
    /// 🔴 CRITICAL: Prevent accidental/malicious mass deletion
    /// </summary>
    public EndpointLimitOptions ClientsDelete { get; set; } = new();

    /// <summary>
    /// Client update endpoint rate limiting
    /// 🟡 MEDIUM: Control frequent modifications
    /// </summary>
    public EndpointLimitOptions ClientsUpdate { get; set; } = new();

    /// <summary>
    /// Client bulk creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection for bulk create
    /// </summary>
    public EndpointLimitOptions ClientsBulkCreate { get; set; } = new();

    /// <summary>
    /// Client query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions ClientsQuery { get; set; } = new();

    /// <summary>
    /// Dossier creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection
    /// </summary>
    public EndpointLimitOptions DossiersCreate { get; set; } = new();

    /// <summary>
    /// Dossier deletion endpoint rate limiting
    /// 🔴 CRITICAL: Prevent accidental/malicious mass deletion
    /// </summary>
    public EndpointLimitOptions DossiersDelete { get; set; } = new();

    /// <summary>
    /// Dossier bulk creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection for bulk create
    /// </summary>
    public EndpointLimitOptions DossiersBulkCreate { get; set; } = new();

    /// <summary>
    /// Dossier query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions DossiersQuery { get; set; } = new();

    /// <summary>
    /// Receipts creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection
    /// </summary>
    public EndpointLimitOptions ReceiptsCreate { get; set; } = new();

    /// <summary>
    /// Receipts query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions ReceiptsQuery { get; set; } = new();

    /// <summary>
    /// Stock creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection
    /// </summary>
    public EndpointLimitOptions StocksCreate { get; set; } = new();

    /// <summary>
    /// Stock update endpoint rate limiting
    /// 🟡 MEDIUM: Control frequent modifications
    /// </summary>
    public EndpointLimitOptions StocksUpdate { get; set; } = new();

    /// <summary>
    /// Stock deletion endpoint rate limiting
    /// 🔴 CRITICAL: Prevent accidental/malicious mass deletion
    /// </summary>
    public EndpointLimitOptions StocksDelete { get; set; } = new();

    /// <summary>
    /// Stock query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions StocksQuery { get; set; } = new();

    /// <summary>
    /// VAT Taxes query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions VatTaxesQuery { get; set; } = new();

    /// <summary>
    /// VAT Taxes update endpoint rate limiting
    /// 🟡 MEDIUM: Control frequent modifications
    /// </summary>
    public EndpointLimitOptions VatTaxesUpdate { get; set; } = new();

    /// <summary>
    /// Advances creation endpoint rate limiting
    /// 🟠 HIGH: Anti-spam protection
    /// </summary>
    public EndpointLimitOptions AdvancesCreate { get; set; } = new();

    /// <summary>
    /// Advances query endpoint rate limiting
    /// 🟢 LOW: Prevent scraping and excessive queries
    /// </summary>
    public EndpointLimitOptions AdvancesQuery { get; set; } = new();

    /// <summary>
    /// Treasury / bank reconciliation query endpoint rate limiting
    /// </summary>
    public EndpointLimitOptions TreasuryQuery { get; set; } = new();

    /// <summary>
    /// Local AI chat endpoint rate limiting
    /// </summary>
    public EndpointLimitOptions AiChat { get; set; } = new();

    /// <summary>
    /// Global fallback rate limiter
    /// 🌍 Applies to endpoints without specific policy
    /// </summary>
    public EndpointLimitOptions GlobalLimit { get; set; } = new();
}

/// <summary>
/// Configuration for individual endpoint rate limits
/// </summary>
public sealed class EndpointLimitOptions
{
    /// <summary>
    /// Maximum number of requests allowed in the time window
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowInSeconds { get; set; } = 60;

    /// <summary>
    /// Algorithm type: FixedWindow or SlidingWindow
    /// - FixedWindow: Hard reset at window boundary (better for critical operations)
    /// - SlidingWindow: Gradual reset (better for user experience)
    /// </summary>
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;

    /// <summary>
    /// Number of segments for Sliding Window algorithm
    /// Higher = smoother reset, but more memory
    /// Ignored for FixedWindow
    /// </summary>
    public int SegmentsPerWindow { get; set; } = 6;

    /// <summary>
    /// Whether to enable this rate limit policy
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Rate limiting algorithm types
/// </summary>
public enum RateLimitAlgorithm
{
    /// <summary>
    /// Fixed window: All permits reset at the same time
    /// Example: 00:00-00:59 = 5 requests, 01:00-01:59 = reset
    /// </summary>
    FixedWindow,

    /// <summary>
    /// Sliding window: Gradual reset over time segments
    /// Example: Window slides every 10 seconds (6 segments of 10s each)
    /// </summary>
    SlidingWindow
}
